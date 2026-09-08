using Avalonia.Media;
using OpenUtau.App.Studio;
using OpenUtau.Core;
using OpenUtau.Core.Util;
using Xunit;

namespace OpenUtau.App {
    [Collection(StudioUiSerialCollection.Name)]
    public class StudioUITest {
        [Fact]
        public void Default_IsClassicEditor() {
            bool previous = Preferences.Default.UseStudioUI;
            string previousTheme = Preferences.Default.ThemeName;
            try {
                Preferences.Default.UseStudioUI = false;
                Assert.False(StudioUI.IsEnabled);
                Assert.Equal(["Light", "Dark"], StudioUI.BuiltInThemes);
                Assert.True(StudioUI.IsBuiltIn("Light"));
                Assert.True(StudioUI.IsBuiltIn("Studio"));
                Assert.DoesNotContain("Studio", StudioUI.BuiltInThemes);
            } finally {
                Preferences.Default.UseStudioUI = previous;
                Preferences.Default.ThemeName = previousTheme;
            }
        }

        [Fact]
        public void Enabled_KeepsStudioPaletteOutOfAppearanceList() {
            bool previous = Preferences.Default.UseStudioUI;
            string previousTheme = Preferences.Default.ThemeName;
            try {
                Preferences.Default.UseStudioUI = true;
                Assert.True(StudioUI.IsEnabled);
                Assert.DoesNotContain("Studio", StudioUI.BuiltInThemes);
                Assert.DoesNotContain("WarmSage", StudioUI.BuiltInThemes);
                Assert.True(StudioUI.IsStudioTheme("Studio"));
            } finally {
                Preferences.Default.UseStudioUI = previous;
                Preferences.Default.ThemeName = previousTheme;
            }
        }

        [Fact]
        public void Enable_ForcesStudioTheme() {
            bool previous = Preferences.Default.UseStudioUI;
            string previousTheme = Preferences.Default.ThemeName;
            try {
                Preferences.Default.UseStudioUI = true;
                Preferences.Default.ThemeName = "Dark";
                Assert.True(StudioUI.EnsureStudioTheme());
                Assert.Equal("Studio", Preferences.Default.ThemeName);
                Assert.False(StudioUI.EnsureStudioTheme());
            } finally {
                Preferences.Default.UseStudioUI = previous;
                Preferences.Default.ThemeName = previousTheme;
            }
        }

        [Fact]
        public void Disable_DropsStudioPaletteBackToDark() {
            bool previous = Preferences.Default.UseStudioUI;
            string previousTheme = Preferences.Default.ThemeName;
            try {
                Preferences.Default.UseStudioUI = false;
                Preferences.Default.ThemeName = StudioUI.StudioTheme;
                Assert.True(StudioUI.EnsureClassicTheme());
                Assert.Equal("Dark", Preferences.Default.ThemeName);
                Assert.False(StudioUI.EnsureClassicTheme());
            } finally {
                Preferences.Default.UseStudioUI = previous;
                Preferences.Default.ThemeName = previousTheme;
            }
        }

        [Fact]
        public void Generator_StudioDark_BuildsElectricCyanPalette() {
            var palette = StudioThemeGenerator.BuildStudioDark();
            Assert.Equal(Color.FromRgb(0x00, 0xB8, 0xFF), palette["AccentColor2"]);
            Assert.NotEqual(palette["BackgroundColor"], palette["ForegroundColor"]);
        }

        [Fact]
        public void Generator_TokyoNight_BuildsNightPalette() {
            var palette = StudioThemeGenerator.BuildTokyoNight();
            Assert.Equal(Color.FromRgb(0x1A, 0x1B, 0x26), palette["BackgroundColor"]);
            Assert.Equal(Color.FromRgb(0x7D, 0xCF, 0xFF), palette["AccentColor2"]);
        }

        [Fact]
        public void Generator_TokyoDay_BuildsDayPalette() {
            var palette = StudioThemeGenerator.BuildTokyoDay();
            Assert.Equal(Color.FromRgb(0xE1, 0xE2, 0xE7), palette["BackgroundColor"]);
            Assert.Equal(Color.FromRgb(0x37, 0x60, 0xBF), palette["ForegroundColor"]);
        }

        [Fact]
        public void Preset_YamlRoundTrip() {
            var preset = new StudioPreset {
                Version = 1,
                Name = "Test Preset",
                IsDark = true,
                Palette = new() { ["BackgroundColor"] = "#1A1B26" },
                Ui = new StudioPresetUi {
                    WaveformStyle = 1,
                    NoteLyricColorMode = 3,
                },
            };
            var yaml = Yaml.DefaultSerializer.Serialize(preset);
            var loaded = Yaml.DefaultDeserializer.Deserialize<StudioPreset>(yaml);
            Assert.Equal("Test Preset", loaded.Name);
            Assert.True(loaded.IsDark);
            Assert.Equal(1, loaded.Ui.WaveformStyle);
            Assert.Equal(3, loaded.Ui.NoteLyricColorMode);
            Assert.Equal("#1A1B26", loaded.Palette["BackgroundColor"]);
        }

        [Fact]
        public void BuiltInUiDefaults_EnableNoteRoundedCornersAt5px() {
            var ui = StudioPresetManager.DefaultUi();
            Assert.True(ui.NoteRoundedCorners);
            Assert.Equal(5, ui.NoteCornerRadiusPx);
        }

        [Fact]
        public void Preset_BuildResourceDictionaryFallsBackToBuiltIn() {
            var preset = new StudioPreset {
                Name = "Custom",
                IsDark = true,
                Palette = new() { ["BackgroundColor"] = "#1A1B26" },
            };
            var dict = StudioPresetManager.BuildResourceDictionary(preset);
            Assert.Equal(Color.FromRgb(0x1A, 0x1B, 0x26), (Color)dict["BackgroundColor"]!);
            Assert.Equal(Color.FromRgb(0x2B, 0x30, 0x3A), (Color)dict["BorderColor"]!);
        }

        [Fact]
        public void ProminentComplementary_StudioDark_IsVividOrange() {
            var palette = StudioThemeGenerator.BuildStudioDark();
            var pitch = StudioThemeGenerator.ProminentComplementary(palette["AccentColor1"], darkMode: true);
            Assert.True(pitch.R > 200, $"expected bright red channel, got {pitch}");
            Assert.True(pitch.G < 180, $"expected muted green channel, got {pitch}");
            Assert.True(pitch.B < 120, $"expected low blue channel, got {pitch}");
        }

        [Fact]
        public void ProminentComplementary_TokyoNight_IsVividAmber() {
            var palette = StudioThemeGenerator.BuildTokyoNight();
            var pitch = StudioThemeGenerator.ProminentComplementary(palette["AccentColor1"], darkMode: true);
            Assert.True(pitch.R > 220, $"expected bright red channel, got {pitch}");
            Assert.True(pitch.G > 150, $"expected warm green channel, got {pitch}");
            Assert.True(pitch.B < 120, $"expected low blue channel, got {pitch}");
        }

        [Fact]
        public void ProminentComplementary_TokyoDay_IsProminent() {
            var palette = StudioThemeGenerator.BuildTokyoDay();
            var pitch = StudioThemeGenerator.ProminentComplementary(palette["AccentColor1"], darkMode: false);
            Assert.True(pitch.R > 150, $"expected bright red channel, got {pitch}");
            Assert.True(pitch.G < 140, $"expected muted green channel, got {pitch}");
            Assert.True(pitch.B < 80, $"expected low blue channel, got {pitch}");
        }
    }
}