// Regression cover for the ONNX native availability probe.
//
// Context: when the app-local native onnxruntime cannot be loaded (typically a missing
// Microsoft Visual C++ 2015-2022 Redistributable), the Windows loader silently binds an older
// onnxruntime.dll from System32. Managed ORT then calls an entry point that older native library
// does not export and the process dies with an access violation, which no managed handler can
// catch. The probe in OnnxNativeAvailability asks for one exact file by absolute path so that
// failure becomes a reportable false instead of a silent fallback.
//
// The probe caches its result once per process, so these tests exercise the deterministic parts:
// the candidate path set, and the decision the loader would make for a given candidate.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenUtau.Core;
using OpenUtau.Core.Util;
using Xunit;

namespace OpenUtau.Test.Core.Util {
    public class OnnxNativeAvailabilityTest {
        [Fact]
        public void CandidatePathsAreAbsoluteAndInsideAppBaseDirectory() {
            var candidates = OnnxNativeAvailability.CandidatePaths;
            Assert.NotEmpty(candidates);
            foreach (var candidate in candidates) {
                Assert.True(Path.IsPathRooted(candidate), $"not absolute: {candidate}");
                Assert.StartsWith(AppContext.BaseDirectory, candidate);
            }
        }

        [Fact]
        public void CandidatePathsIncludeTheRuntimesLayout() {
            // Native assets live under runtimes/<rid>/native in normal builds and in the application
            // root in published builds, so both have to be probed.
            var candidates = OnnxNativeAvailability.CandidatePaths;
            // The runtimes candidate must be under runtimes/<rid>/native, whatever the rid is.
            Assert.Contains(candidates, c => c.Contains($"{Path.DirectorySeparatorChar}runtimes{Path.DirectorySeparatorChar}")
                && c.Contains($"{Path.DirectorySeparatorChar}native{Path.DirectorySeparatorChar}"));
            string appRoot = Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);
            Assert.Contains(candidates, c => Path.TrimEndingDirectorySeparator(Path.GetDirectoryName(c)!) == appRoot);
        }

        [Fact]
        public void MissingCandidateCannotBeLoadedAndDoesNotFallBack() {
            // A path that does not exist must fail. The point is that the failure is reported rather
            // than turning into a binding of some other copy found on the search path.
            string missing = Path.Combine(AppContext.BaseDirectory, "runtimes", "definitely-absent-rid", "native", "onnxruntime-absent.dll");
            Assert.False(File.Exists(missing));
            Assert.False(NativeLibrary.TryLoad(missing, out _));
        }

        [Fact]
        public void UnloadableCandidateIsRejectedWithoutBlockingTheFile() {
            // Mirrors the reported crash: the file exists but cannot be loaded. The probe must report
            // false, and it must not leave a lock behind on a file it refused.
            string dir = Path.Combine(AppContext.BaseDirectory, "onnx-probe-test");
            Directory.CreateDirectory(dir);
            string garbage = Path.Combine(dir, "unloadable-native.bin");
            try {
                File.WriteAllBytes(garbage, new byte[4096]);
                Assert.True(File.Exists(garbage));
                Assert.False(NativeLibrary.TryLoad(garbage, out _));
                File.Delete(garbage);
            } finally {
                if (Directory.Exists(dir)) {
                    Directory.Delete(dir, true);
                }
            }
        }

        [Fact]
        public void LoadableCandidateIsAccepted() {
            // Control case: when the file really is loadable, TryLoad accepts it by absolute path.
            // Use the shipped native ONNX Runtime; a managed assembly is not a valid stand-in
            // because dlopen on Linux/macOS rejects a PE managed image while LoadLibrary on
            // Windows loads it, making the control case platform-dependent.
            var candidate = OnnxNativeAvailability.CandidatePaths.FirstOrDefault(File.Exists);
            if (candidate == null) {
                Assert.Skip("No native ONNX Runtime is present in this test host layout.");
            }
            Assert.True(NativeLibrary.TryLoad(candidate, out var handle));
            NativeLibrary.Free(handle);
        }

        [Fact]
        public void DiagnosticIdentityIsCoherent() {
            // On platforms without a shipped native runtime the probe reports unsupported; everywhere
            // else it either loaded a path or explains why it did not.
            if (OnnxNativeAvailability.IsAvailable) {
                Assert.NotNull(OnnxNativeAvailability.LoadedPath);
                Assert.True(File.Exists(OnnxNativeAvailability.LoadedPath));
                Assert.Null(OnnxNativeAvailability.UnavailableReason);
                Assert.Equal(Onnx.getRunnerOptions(), Onnx.getRunnerOptions());
            } else {
                Assert.False(string.IsNullOrWhiteSpace(OnnxNativeAvailability.UnavailableReason));
            }
        }
    }
}