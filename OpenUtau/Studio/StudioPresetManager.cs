using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Media;
using OpenUtau.Core;
using OpenUtau.Core.Util;
using Serilog;

namespace OpenUtau.App.Studio {
    /// <summary>
    /// Loads and saves Studio UI presets as YAML files in
    /// <c>DataPath/Presets</c>. The built-in presets (Studio Dark, Tokyo Night,
    /// Tokyo Day) are seeded as real YAML files on first use, so every preset —
    /// including the defaults — is a plain file that can be copied, edited, or
    /// deleted, and dropping a new YAML in the folder adds a new preset.
    /// </summary>
    public static class StudioPresetManager {
        public static string PresetsPath => PathManager.Inst.PresetsPath;

        /// <summary>
        /// Current built-in preset format version. Bump this when built-in
        /// preset contents change so existing seeded files are regenerated.
        /// </summary>
        public const int BuiltInVersion = 2;

        static readonly string[] BuiltInNames = [
            StudioThemeGenerator.StudioDark,
            StudioThemeGenerator.TokyoNight,
            StudioThemeGenerator.TokyoDay,
        ];

        public static List<StudioPreset> LoadAll() {
            EnsureDefaults();
            var presets = new List<StudioPreset>();
            foreach (var file in Directory.EnumerateFiles(PresetsPath, "*.yaml")) {
                try {
                    var preset = Yaml.DefaultDeserializer.Deserialize<StudioPreset>(
                        File.ReadAllText(file, Encoding.UTF8));
                    if (preset != null && !string.IsNullOrWhiteSpace(preset.Name)) {
                        presets.Add(preset);
                    }
                } catch (Exception e) {
                    Log.Error(e, "Failed to parse Studio UI preset {File}", file);
                }
            }
            return presets
                .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static StudioPreset? Load(string name) =>
            LoadAll().FirstOrDefault(p => p.Name == name);

        public static StudioPreset Save(StudioPreset preset) {
            EnsureDefaults();
            preset.Version = BuiltInVersion;
            File.WriteAllText(GetPath(preset.Name),
                Yaml.DefaultSerializer.Serialize(preset), Encoding.UTF8);
            return preset;
        }

        public static StudioPreset GetBuiltIn(string name) => new() {
            Version = BuiltInVersion,
            Name = name,
            IsDark = name != StudioThemeGenerator.TokyoDay,
            Palette = ToHex(StudioThemeGenerator.BuildPalette(name)),
            Ui = DefaultUi(),
        };

        public static void EnsureDefaults() {
            try {
                Directory.CreateDirectory(PresetsPath);
            } catch (Exception e) {
                Log.Error(e, "Failed to create Studio UI presets directory {Path}", PresetsPath);
                return;
            }
            foreach (var name in BuiltInNames) {
                var path = GetPath(name);
                int existingVersion = -1;
                if (File.Exists(path)) {
                    try {
                        existingVersion = Yaml.DefaultDeserializer
                            .Deserialize<StudioPreset>(File.ReadAllText(path, Encoding.UTF8))
                            .Version;
                    } catch (Exception e) {
                        Log.Error(e, "Failed to parse Studio UI preset {File}", path);
                    }
                }
                if (existingVersion >= BuiltInVersion) {
                    continue;
                }
                try {
                    File.WriteAllText(path,
                        Yaml.DefaultSerializer.Serialize(GetBuiltIn(name)), Encoding.UTF8);
                } catch (Exception e) {
                    Log.Error(e, "Failed to seed Studio UI preset {Name}", name);
                }
            }
        }

        /// <summary>
        /// Builds the resource dictionary used by the app for a preset. Missing
        /// palette entries fall back to the matching built-in palette.
        /// </summary>
        public static ResourceDictionary BuildResourceDictionary(StudioPreset preset) {
            var fallback = preset.IsDark
                ? StudioThemeGenerator.BuildStudioDark()
                : StudioThemeGenerator.BuildTokyoDay();
            var dict = new ResourceDictionary {
                ["IsDarkMode"] = preset.IsDark,
            };
            foreach (var kv in fallback) {
                dict[kv.Key] = kv.Value;
            }
            foreach (var kv in preset.Palette) {
                if (Color.TryParse(kv.Value, out var c)) {
                    dict[kv.Key] = c;
                }
            }
            return dict;
        }

        public static StudioPresetUi DefaultUi() => new() {
            TrackColorMode = 0,
            TrackColorConfig = StudioTrackColorConfig.FromCurrent(),
            WaveformStyle = 1,
            WaveformLayout = 0,
            WaveformFollowMode = 1,
            WaveformFadeInMs = 80,
            WaveformFadeOutMs = 80,
            WaveformColorMode = 0,
            WaveformColorHex = "#4EA6EAFF",
            WaveformColorInvert = false,
            WaveformScalePercent = 150,
            WaveformFixedBottomPx = 120,
            NoteStrokeColorMode = 1,
            NoteStrokeColorHex = "#FFFFFFFF",
            NoteStrokeColorInvert = false,
            NoteStrokeThickness = 1,
            PitchPredictionColorMode = 0,
            PitchPredictionColorHex = "#FFFFFFFF",
            PitchPredictionColorInvert = true,
            PitchPredictionThickness = 2,
            NoteRoundedCorners = true,
            NoteCornerRadiusPx = 5,
            NoteSolidFill = false,
            NoteLyricVAlign = 2,
            NoteLyricHAlign = 0,
            NoteLyricFontFamily = "Noto Sans, Segoe UI",
            NoteLyricScalePercent = 130,
            NoteLyricWeight = 600,
            NoteLyricItalic = false,
            NoteLyricColorMode = 3,
            NoteLyricColorHex = "#FFFFFFFF",
            NoteLyricColorInvert = false,
        };

        static string GetPath(string name) {
            var sanitized = new string(name
                .Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '-' : ch)
                .ToArray());
            return Path.Combine(PresetsPath, sanitized + ".yaml");
        }

        static Dictionary<string, string> ToHex(Dictionary<string, Color> colors) =>
            colors.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
    }
}