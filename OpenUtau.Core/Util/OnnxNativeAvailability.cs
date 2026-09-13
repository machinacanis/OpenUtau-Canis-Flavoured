// Copyright 2026 StAkira
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Serilog;

namespace OpenUtau.Core.Util {
    /// <summary>
    /// Reports whether the native ONNX Runtime shipped next to the app can be loaded, and, critically,
    /// loads it by absolute path so the OS loader cannot silently bind a different copy.
    ///
    /// Why this exists. OpenUtau ships its own onnxruntime native library under
    /// runtimes/&lt;rid&gt;/native/ (normal builds) or the application root (published builds). When that
    /// library cannot be loaded - the usual cause being a missing Microsoft Visual C++ 2015-2022
    /// Redistributable - the Windows loader does not report an error to the app: it keeps searching and
    /// binds an older onnxruntime.dll from System32 (the Windows inbox AI component). Managed ORT then
    /// calls an entry point that this older native library does not export and the process dies with an
    /// access violation (0xC0000005), which no managed try/catch, AppDomain handler, or log sink can
    /// observe.
    ///
    /// This type removes the silent fallback: it asks for one specific file by absolute path, so the
    /// answer is either "loaded exactly this" or "could not load", never "loaded something else".
    /// Callers use <see cref="IsAvailable"/> to degrade gracefully instead of faulting.
    ///
    /// Note that a DllImportResolver cannot solve this: managed ORT loads its native library itself and
    /// never goes through CLR P/Invoke resolution, so a resolver is not invoked at all (measured: zero
    /// calls).
    /// </summary>
    public static class OnnxNativeAvailability {
        /// <summary>True when the app-local native ONNX Runtime was loaded and its version is usable.</summary>
        public static bool IsAvailable {
            get {
                EnsureProbed();
                return probeResult == ProbeResult.Available;
            }
        }

        /// <summary>Absolute path of the native library that was loaded, or null when unavailable.</summary>
        public static string? LoadedPath => loadedPath;

        /// <summary>Reason why ONNX is unavailable, or null when it is available.</summary>
        public static string? UnavailableReason => unavailableReason;

        /// <summary>Short hint for a user, or null when no action would help.</summary>
        public static string? UnavailableHint => unavailableHint;

        /// <summary>
        /// Absolute paths that are probed, in order. The application root holds the native library in
        /// published builds; runtimes/&lt;rid&gt;/native/ holds it in normal builds.
        /// </summary>
        public static IReadOnlyList<string> CandidatePaths {
            get {
                string? fileName = NativeFileName();
                if (fileName == null) {
                    return Array.Empty<string>();
                }
                var candidates = new List<string> {
                    Path.Combine(AppContext.BaseDirectory, fileName),
                };
                string? rid = NativeRidFolder();
                if (rid != null) {
                    candidates.Add(Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", fileName));
                }
                return candidates;
            }
        }

        enum ProbeResult {
            Unknown,
            Available,
            Unavailable,
        }

        static readonly object probeLock = new object();
        // Held for the lifetime of the process so the loaded module is never unloaded.
        static IntPtr nativeHandle = IntPtr.Zero;
        static bool probed = false;
        static ProbeResult probeResult = ProbeResult.Unknown;
        static string? loadedPath = null;
        static string? unavailableReason = null;
        static string? unavailableHint = null;

        static void EnsureProbed() {
            if (probed) {
                return;
            }
            lock (probeLock) {
                if (probed) {
                    return;
                }
                Probe();
                probed = true;
            }
        }

        static void Probe() {
            try {
                ProbeCore();
            } catch (Exception e) {
                probeResult = ProbeResult.Unavailable;
                unavailableReason = $"Unexpected error while probing the native ONNX Runtime: {e.Message}";
                Log.Warning(e, "ONNX native probe failed with an unexpected error.");
            }
        }

        static void ProbeCore() {
            string? fileName = NativeFileName();
            if (fileName == null) {
                SetUnavailable(
                    $"No native ONNX Runtime is shipped for architecture " +
                    $"{RuntimeInformation.ProcessArchitecture} on this OS.",
                    hint: null);
                return;
            }

            var failures = new List<string>();
            foreach (var candidate in CandidatePaths) {
                if (!File.Exists(candidate)) {
                    failures.Add($"{candidate}: not found");
                    continue;
                }
                if (!NativeLibrary.TryLoad(candidate, out var handle)) {
                    failures.Add($"{candidate}: load failed{FormatError(Marshal.GetLastWin32Error())}");
                    continue;
                }

                string? versionProblem = DescribeVersionProblem(candidate);
                if (versionProblem != null) {
                    // Refuse a native library older than the managed wrapper: that combination is exactly
                    // what faults with 0xC0000005 once ORT calls an entry point the native side lacks.
                    NativeLibrary.Free(handle);
                    SetUnavailable(versionProblem, VersionHint());
                    return;
                }

                nativeHandle = handle;
                loadedPath = candidate;
                probeResult = ProbeResult.Available;
                Log.Information($"ONNX native runtime loaded from {candidate}.");
                return;
            }

            SetUnavailable(
                "Could not load the native ONNX Runtime that ships with OpenUtau. Tried: " +
                string.Join("; ", failures),
                failures.Exists(f => f.Contains("load failed")) ? MissingRuntimeHint() : null);
        }

        static void SetUnavailable(string reason, string? hint) {
            probeResult = ProbeResult.Unavailable;
            unavailableReason = reason;
            unavailableHint = hint;
            Log.Warning("ONNX is unavailable. {Reason}", reason);
            if (hint != null) {
                Log.Warning("ONNX unavailable hint: {Hint}", hint);
            }
        }

        static string FormatError(int error) {
            if (error == 0) {
                return string.Empty;
            }
            string name = error switch {
                126 => "ERROR_MOD_NOT_FOUND",
                193 => "ERROR_BAD_EXE_FORMAT",
                _ => "win32 error",
            };
            return $" ({name} {error})";
        }

        /// <summary>
        /// Returns a description of the version problem when the native library is older than the
        /// managed ORT wrapper, or null when the pair looks compatible (or versions are unavailable).
        /// </summary>
        static string? DescribeVersionProblem(string nativePath) {
            var native = ParseVersion(FileVersionInfo.GetVersionInfo(nativePath).FileVersion);
            var managed = typeof(Microsoft.ML.OnnxRuntime.OrtEnv).Assembly.GetName().Version;
            if (native == null || managed == null) {
                return null;
            }
            bool older = native.Major > 0 && (native.Major < managed.Major ||
                (native.Major == managed.Major && native.Minor < managed.Minor));
            if (!older) {
                return null;
            }
            return $"The native ONNX Runtime at {nativePath} is version {native}, older than the " +
                $"managed wrapper {managed}. An older native library does not export every entry point " +
                $"this build calls.";
        }

        static Version? ParseVersion(string? text) {
            if (string.IsNullOrEmpty(text)) {
                return null;
            }
            // File versions are dotted numbers, sometimes with extra components or a suffix.
            var separators = new[] { '.', '-', ' ' };
            var numbers = new List<int>();
            foreach (var part in text.Split(separators, StringSplitOptions.RemoveEmptyEntries)) {
                if (!int.TryParse(part, out int value) || value < 0) {
                    break;
                }
                numbers.Add(value);
                if (numbers.Count == 4) {
                    break;
                }
            }
            if (numbers.Count < 2) {
                return null;
            }
            while (numbers.Count < 4) {
                numbers.Add(0);
            }
            return new Version(numbers[0], numbers[1], numbers[2], numbers[3]);
        }

        static string MissingRuntimeHint() =>
            "Install the Microsoft Visual C++ 2015-2022 Redistributable (x64) and restart OpenUtau. " +
            "Its absence is the usual reason the bundled ONNX Runtime cannot be loaded.";

        static string VersionHint() =>
            "The application directory contains an ONNX Runtime older than this build expects. " +
            "Reinstall or update OpenUtau so the bundled native library matches.";

        /// <summary>File name of the native ONNX Runtime for the current OS, or null if unsupported.</summary>
        static string? NativeFileName() {
            if (OS.IsWindows()) {
                return "onnxruntime.dll";
            }
            if (OS.IsMacOS()) {
                return "libonnxruntime.dylib";
            }
            if (OS.IsLinux()) {
                return "libonnxruntime.so";
            }
            return null;
        }

        /// <summary>
        /// Folder under runtimes/ that holds the native library for this process, following the layout
        /// this repository publishes (runtimes/osx/native, not runtimes/osx-x64/native).
        /// </summary>
        static string? NativeRidFolder() {
            var arch = RuntimeInformation.ProcessArchitecture;
            if (OS.IsWindows()) {
                return arch switch {
                    Architecture.X86 => "win-x86",
                    Architecture.Arm64 => "win-arm64",
                    Architecture.X64 => "win-x64",
                    _ => null,
                };
            }
            if (OS.IsMacOS()) {
                return arch switch {
                    Architecture.Arm64 => "osx-arm64",
                    Architecture.X64 => "osx-x64",
                    _ => null,
                };
            }
            if (OS.IsLinux()) {
                return arch switch {
                    Architecture.Arm64 => "linux-arm64",
                    Architecture.X64 => "linux-x64",
                    _ => null,
                };
            }
            return null;
        }
    }
}