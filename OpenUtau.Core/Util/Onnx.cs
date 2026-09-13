using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using OpenUtau.Core.Util;
using Serilog;

namespace OpenUtau.Core {
    public class GpuInfo {
        public int deviceId;
        public string description = "";

        override public string ToString() {
            return $"[{deviceId}] {description}";
        }
    }

    public enum OnnxRunnerChoice {
        Default,
        CPU,
        CPUForCoreML,
    }

    public class Onnx {

        private static bool cudaAvailable = OS.IsLinux() && CudaGpuDetector.IsCudaAvailable() && CudaGpuDetector.IsCuDnnAvailable();

        private static readonly Dictionary<int, OrtEpDevice> devices = initializeDevices();

        /// <summary>
        /// ORT devices, empty when the native runtime could not be loaded.
        /// </summary>
        /// <remarks>
        /// The probe in OnnxNativeAvailability runs before the first native call on purpose. When the
        /// app-local native onnxruntime cannot be loaded, the OS loader silently binds an older copy from
        /// System32 instead, and that version mismatch kills the process with an access violation which no
        /// managed handler can catch. Degrading to "no devices" keeps the app usable; ONNX-dependent
        /// features can report OnnxNativeAvailability.UnavailableReason.
        /// </remarks>
        private static Dictionary<int, OrtEpDevice> initializeDevices() {
            if (!OnnxNativeAvailability.IsAvailable) {
                return new Dictionary<int, OrtEpDevice>();
            }
            var env = OrtEnv.Instance();
            var ortDevices = env.GetEpDevices();

            return ortDevices
                .Where(device => device.EpName.ToLower().Contains("dml"))
                .Select((device, index) => new { index, device })
                .ToDictionary(x => x.index, x => x.device);
        }

        public static List<string> getRunnerOptions() {
            if (OS.IsWindows()) {
                return new List<string> {
                "CPU",
                "DirectML"
                };
            } else if (OS.IsMacOS()) {
                return new List<string> {
                "CPU",
                "CoreML"
                };
            } else if (cudaAvailable) {
                return new List<string> {
                "CPU",
                "CUDA"
                };
            } else if (OS.IsAndroid()) {
                return new List<string> {
                "CPU",
                "NNAPI"
                };
            }
            return new List<string> {
                "CPU"        
            };
        }

        public static List<GpuInfo> getGpuInfo() {
            if (cudaAvailable) {
                return CudaGpuDetector.GetCudaDevices();
            }         
     
            if (OS.IsAndroid()) {
                return new List<GpuInfo>{new GpuInfo {
                    deviceId = 0, // eliminate exception of taking OnnxGpuOptions[0]
                }};
            }

            if (!OnnxNativeAvailability.IsAvailable) {
                return new List<GpuInfo>();
            }

            List<GpuInfo> gpuList = new List<GpuInfo>();
            var env = OrtEnv.Instance();
            var ortDevices = env.GetEpDevices();

            var i = 0;
            foreach (var device in ortDevices.Where(device => device.EpName.ToLower().Contains("dml"))) {
                var description = "";
                foreach (var item in device.HardwareDevice.Metadata.Entries) {
                    if (item.Key.ToLower() == "description") {
                        description = $"{item.Value} ({device.HardwareDevice.Type})";
                        break;
                    }
                }
                if (string.IsNullOrEmpty(description)) { // fallback
                    description = $"{device.EpName} {device.HardwareDevice.Vendor} ({device.HardwareDevice.Type})";
                }
                devices[i] = device;
                gpuList.Add(new GpuInfo {
                    deviceId = i++,
                    description = description
                });
            }
            return gpuList;
        }

        private static SessionOptions getOnnxSessionOptions(bool coremlEnableOnSubgraphs = false) {
            SessionOptions options = new SessionOptions();
            List<string> runnerOptions = getRunnerOptions();
            string runner = Preferences.Default.OnnxRunner;
            if (String.IsNullOrEmpty(runner)) {
                runner = runnerOptions[0];
            }
            if (!runnerOptions.Contains(runner)) {
                runner = "CPU";
            }
            switch (runner) {
                case "DirectML":
                    var d = devices[Preferences.Default.OnnxGpu];
                    options.AppendExecutionProvider(
                        OrtEnv.Instance(),
                        new List<OrtEpDevice> { d },
                        new Dictionary<string, string> { }
                     );
                    break;
                case "CoreML":
                    // Note: MLProgram format has stricter validation and may fail with complex DiffSinger models
                    // that have topological sorting issues (e.g., variance_predictor with diffusion embeddings)
                    // so we always use NeuralNetwork format (default) as MLProgram fails with complex models.
                    options.AppendExecutionProvider("CoreML", new Dictionary<string, string> {
                        { "MLComputeUnits", "ALL" },
                        { "RequireStaticInputShapes", "1"},
                        { "ModelFormat", "NeuralNetwork"},
                        { "EnableOnSubgraphs", coremlEnableOnSubgraphs ? "1" : "0" }  // Disable subgraph processing to avoid complex control flow issues
                    });
                    break;
                case "CUDA":
                    options.AppendExecutionProvider_CUDA(Preferences.Default.OnnxGpu);
                    break;
                case "NNAPI":
                    options.AppendExecutionProvider_Nnapi();
                    break;
            }
            return options;
        }

        public static InferenceSession getInferenceSession(byte[] model, OnnxRunnerChoice runnerChoice = OnnxRunnerChoice.Default, Action<SessionOptions>? configure = null) {
            if (runnerChoice == OnnxRunnerChoice.CPU ||
                (runnerChoice == OnnxRunnerChoice.CPUForCoreML && Preferences.Default.OnnxRunner == "CoreML")) {
                if (configure == null) {
                    return new InferenceSession(model);
                }
                var cpuOptions = new SessionOptions();
                configure(cpuOptions);
                return new InferenceSession(model, cpuOptions);
            } else {
                // Try with CoreML subgraphs enabled first, fallback to default if it fails
                if (OS.IsMacOS() && Preferences.Default.OnnxRunner == "CoreML") {
                    try {
                        var coremlOptions = getOnnxSessionOptions(coremlEnableOnSubgraphs: true);
                        configure?.Invoke(coremlOptions);
                        return new InferenceSession(model, coremlOptions);
                    } catch (Exception e) {
                        Log.Warning(e, "Failed to create session with CoreML subgraphs enabled, falling back to default settings");
                    }
                }
                var options = getOnnxSessionOptions();
                configure?.Invoke(options);
                return new InferenceSession(model, options);
            }
        }

        public static InferenceSession getInferenceSession(string modelPath, OnnxRunnerChoice runnerChoice = OnnxRunnerChoice.Default, Action<SessionOptions>? configure = null) {
            if (runnerChoice == OnnxRunnerChoice.CPU ||
                (runnerChoice == OnnxRunnerChoice.CPUForCoreML && Preferences.Default.OnnxRunner == "CoreML")) {
                if (configure == null) {
                    return new InferenceSession(modelPath);
                }
                var cpuOptions = new SessionOptions();
                configure(cpuOptions);
                return new InferenceSession(modelPath, cpuOptions);
            } else {
                // Try with CoreML subgraphs enabled first, fallback to default if it fails
                if (OS.IsMacOS() && Preferences.Default.OnnxRunner == "CoreML") {
                    try {
                        var coremlOptions = getOnnxSessionOptions(coremlEnableOnSubgraphs: true);
                        configure?.Invoke(coremlOptions);
                        return new InferenceSession(modelPath, coremlOptions);
                    } catch (Exception e) {
                        Log.Warning(e, "Failed to create session with CoreML subgraphs enabled, falling back to default settings");
                    }
                }
                var options = getOnnxSessionOptions();
                configure?.Invoke(options);
                return new InferenceSession(modelPath, options);
            }
        }

        public static void VerifyInputNames(InferenceSession session, IEnumerable<NamedOnnxValue> inputs) {
            var sessionInputNames = session.InputNames.ToHashSet();
            var givenInputNames = inputs.Select(v => v.Name).ToHashSet();
            var missing = sessionInputNames
                .Except(givenInputNames)
                .OrderBy(s => s, StringComparer.InvariantCulture)
                .ToArray();
            if (missing.Length > 0) {
                throw new ArgumentException("Missing input(s) for the inference session: " + string.Join(", ", missing));
            }
            var unexpected = givenInputNames
                .Except(sessionInputNames)
                .OrderBy(s => s, StringComparer.InvariantCulture)
                .ToArray();
            if (unexpected.Length > 0) {
                throw new ArgumentException("Unexpected input(s) for the inference session: " + string.Join(", ", unexpected));
            }
        }
    }
}
