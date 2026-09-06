using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace OpenUtau.App.Studio {
    /// <summary>
    /// Built-in Studio UI palette factories. Presets are managed as YAML files
    /// in <c>DataPath/Presets</c>; these factories produce the palettes used to
    /// seed the built-in preset files and as a fallback for missing entries.
    /// </summary>
    public static class StudioThemeGenerator {
        public const string StudioDark = "Studio Dark";
        public const string TokyoNight = "Tokyo Night";
        public const string TokyoDay = "Tokyo Day";

        // Electric cyan: the signature accent of the Studio Dark preset.
        static readonly Color DarkSeed = Color.FromRgb(0x00, 0xB8, 0xFF);

        public static Dictionary<string, Color> BuildPalette(string presetName) =>
            presetName switch {
                TokyoNight => BuildTokyoNight(),
                TokyoDay => BuildTokyoDay(),
                _ => BuildStudioDark(),
            };

        /// <summary>
        /// Studio Dark: the original electric-cyan dark Studio palette.
        /// </summary>
        public static Dictionary<string, Color> BuildStudioDark() {
            return new Dictionary<string, Color> {
                ["BackgroundColor"] = Color.FromRgb(0x14, 0x16, 0x19),
                ["BackgroundColorPointerOver"] = Color.FromRgb(0x1F, 0x23, 0x29),
                ["BackgroundColorPressed"] = Color.FromRgb(0x29, 0x2E, 0x36),
                ["BackgroundColorDisabled"] = Color.FromRgb(0x10, 0x12, 0x14),
                ["ForegroundColor"] = Color.FromRgb(0xE2, 0xE6, 0xEC),
                ["ForegroundColorPointerOver"] = Color.FromRgb(0xFF, 0xFF, 0xFF),
                ["ForegroundColorPressed"] = Color.FromRgb(0xF0, 0xF3, 0xF8),
                ["ForegroundColorDisabled"] = Color.FromRgb(0x55, 0x5D, 0x6C),
                ["BorderColor"] = Color.FromRgb(0x2B, 0x30, 0x3A),
                ["BorderColorPointerOver"] = Color.FromRgb(0x3E, 0x46, 0x54),
                ["SystemAccentColor"] = Darken(DarkSeed, 0.13),
                ["SystemAccentColorLight1"] = Lighten(DarkSeed, 0.26),
                ["SystemAccentColorDark1"] = Darken(DarkSeed, 0.20),
                ["NeutralAccentColor"] = Color.FromRgb(0x32, 0x37, 0x42),
                ["NeutralAccentColorPointerOver"] = Color.FromRgb(0x42, 0x49, 0x57),
                ["AccentColor1"] = Darken(DarkSeed, 0.28),
                ["AccentColor2"] = DarkSeed,
                ["AccentColor3"] = Color.FromRgb(0x00, 0xE5, 0xFF),
                ["TickLineColor"] = Color.FromRgb(0x23, 0x27, 0x30),
                ["BarNumberColor"] = Color.FromRgb(0x6E, 0x77, 0x88),
                ["FinalPitchColor"] = Lighten(DarkSeed, 0.22),
                ["TrackBackgroundAltColor"] = Color.FromRgb(0x18, 0x1B, 0x20),
                ["WarningColor"] = Color.FromRgb(0x3D, 0x27, 0x00),
                ["WhiteKeyColorLeft"] = Color.FromRgb(0x23, 0x27, 0x2F),
                ["WhiteKeyColorRight"] = Color.FromRgb(0x2B, 0x30, 0x3A),
                ["WhiteKeyNameColor"] = Color.FromRgb(0xCC, 0xD3, 0xDE),
                ["CenterKeyColorLeft"] = Color.FromRgb(0x15, 0x24, 0x34),
                ["CenterKeyColorRight"] = Color.FromRgb(0x1A, 0x2F, 0x45),
                ["CenterKeyNameColor"] = Color.FromRgb(0x00, 0xA3, 0xFF),
                ["BlackKeyColorLeft"] = Color.FromRgb(0x10, 0x12, 0x15),
                ["BlackKeyColorRight"] = Color.FromRgb(0x15, 0x17, 0x1B),
                ["BlackKeyNameColor"] = Color.FromRgb(0x61, 0x6B, 0x7B),
            };
        }

        /// <summary>
        /// Tokyo Night: dark palette from folke/tokyonight.nvim.
        /// </summary>
        public static Dictionary<string, Color> BuildTokyoNight() {
            return new Dictionary<string, Color> {
                ["BackgroundColor"] = Color.FromRgb(0x1A, 0x1B, 0x26),
                ["BackgroundColorPointerOver"] = Color.FromRgb(0x29, 0x2E, 0x42),
                ["BackgroundColorPressed"] = Color.FromRgb(0x2F, 0x35, 0x49),
                ["BackgroundColorDisabled"] = Color.FromRgb(0x16, 0x16, 0x1E),
                ["ForegroundColor"] = Color.FromRgb(0xC0, 0xCA, 0xF5),
                ["ForegroundColorPointerOver"] = Color.FromRgb(0xD3, 0xDC, 0xF8),
                ["ForegroundColorPressed"] = Color.FromRgb(0xA9, 0xB1, 0xD6),
                ["ForegroundColorDisabled"] = Color.FromRgb(0x56, 0x5F, 0x89),
                ["BorderColor"] = Color.FromRgb(0x41, 0x48, 0x68),
                ["BorderColorPointerOver"] = Color.FromRgb(0x73, 0x7A, 0xA2),
                ["SystemAccentColor"] = Color.FromRgb(0x7A, 0xA2, 0xF7),
                ["SystemAccentColorLight1"] = Color.FromRgb(0x89, 0xDD, 0xFF),
                ["SystemAccentColorDark1"] = Color.FromRgb(0x3D, 0x59, 0xA1),
                ["NeutralAccentColor"] = Color.FromRgb(0x41, 0x48, 0x68),
                ["NeutralAccentColorPointerOver"] = Color.FromRgb(0x54, 0x5C, 0x7E),
                ["AccentColor1"] = Color.FromRgb(0x7A, 0xA2, 0xF7),
                ["AccentColor2"] = Color.FromRgb(0x7D, 0xCF, 0xFF),
                ["AccentColor3"] = Color.FromRgb(0x89, 0xDD, 0xFF),
                ["TickLineColor"] = Color.FromRgb(0x3B, 0x42, 0x61),
                ["BarNumberColor"] = Color.FromRgb(0x56, 0x5F, 0x89),
                ["FinalPitchColor"] = Color.FromRgb(0x7D, 0xCF, 0xFF),
                ["TrackBackgroundAltColor"] = Color.FromRgb(0x16, 0x16, 0x1E),
                ["WarningColor"] = Color.FromRgb(0x4A, 0x42, 0x26),
                ["WhiteKeyColorLeft"] = Color.FromRgb(0x29, 0x2E, 0x42),
                ["WhiteKeyColorRight"] = Color.FromRgb(0x24, 0x28, 0x3B),
                ["WhiteKeyNameColor"] = Color.FromRgb(0xA9, 0xB1, 0xD6),
                ["CenterKeyColorLeft"] = Color.FromRgb(0x2C, 0x3A, 0x63),
                ["CenterKeyColorRight"] = Color.FromRgb(0x33, 0x46, 0x7C),
                ["CenterKeyNameColor"] = Color.FromRgb(0x7A, 0xA2, 0xF7),
                ["BlackKeyColorLeft"] = Color.FromRgb(0x15, 0x16, 0x1E),
                ["BlackKeyColorRight"] = Color.FromRgb(0x1A, 0x1B, 0x26),
                ["BlackKeyNameColor"] = Color.FromRgb(0x56, 0x5F, 0x89),
            };
        }

        /// <summary>
        /// Tokyo Day: light palette from folke/tokyonight.nvim.
        /// </summary>
        public static Dictionary<string, Color> BuildTokyoDay() {
            return new Dictionary<string, Color> {
                ["BackgroundColor"] = Color.FromRgb(0xE1, 0xE2, 0xE7),
                ["BackgroundColorPointerOver"] = Color.FromRgb(0xEC, 0xEC, 0xF0),
                ["BackgroundColorPressed"] = Color.FromRgb(0xC4, 0xC8, 0xDA),
                ["BackgroundColorDisabled"] = Color.FromRgb(0xE8, 0xE9, 0xEE),
                ["ForegroundColor"] = Color.FromRgb(0x37, 0x60, 0xBF),
                ["ForegroundColorPointerOver"] = Color.FromRgb(0xFF, 0xFF, 0xFF),
                ["ForegroundColorPressed"] = Color.FromRgb(0x1D, 0x1F, 0x2A),
                ["ForegroundColorDisabled"] = Color.FromRgb(0x84, 0x8C, 0xB5),
                ["BorderColor"] = Color.FromRgb(0xA1, 0xA6, 0xC5),
                ["BorderColorPointerOver"] = Color.FromRgb(0x84, 0x8C, 0xB5),
                ["SystemAccentColor"] = Color.FromRgb(0x2E, 0x7D, 0xE9),
                ["SystemAccentColorLight1"] = Color.FromRgb(0x35, 0x8A, 0xFF),
                ["SystemAccentColorDark1"] = Color.FromRgb(0x16, 0x64, 0xC0),
                ["NeutralAccentColor"] = Color.FromRgb(0xC4, 0xC8, 0xDA),
                ["NeutralAccentColorPointerOver"] = Color.FromRgb(0xB4, 0xB5, 0xB9),
                ["AccentColor1"] = Color.FromRgb(0x2E, 0x7D, 0xE9),
                ["AccentColor2"] = Color.FromRgb(0x00, 0x71, 0x97),
                ["AccentColor3"] = Color.FromRgb(0x00, 0x7E, 0xA8),
                ["TickLineColor"] = Color.FromRgb(0xC4, 0xC8, 0xDA),
                ["BarNumberColor"] = Color.FromRgb(0x84, 0x8C, 0xB5),
                ["FinalPitchColor"] = Color.FromRgb(0x00, 0x71, 0x97),
                ["TrackBackgroundAltColor"] = Color.FromRgb(0xE9, 0xE9, 0xEC),
                ["WarningColor"] = Color.FromRgb(0xF2, 0xD9, 0xA0),
                ["WhiteKeyColorLeft"] = Color.FromRgb(0xFF, 0xFF, 0xFF),
                ["WhiteKeyColorRight"] = Color.FromRgb(0xE9, 0xE9, 0xEC),
                ["WhiteKeyNameColor"] = Color.FromRgb(0x37, 0x60, 0xBF),
                ["CenterKeyColorLeft"] = Color.FromRgb(0xD4, 0xDC, 0xF2),
                ["CenterKeyColorRight"] = Color.FromRgb(0xC4, 0xC8, 0xDA),
                ["CenterKeyNameColor"] = Color.FromRgb(0x16, 0x64, 0xC0),
                ["BlackKeyColorLeft"] = Color.FromRgb(0xB4, 0xB5, 0xB9),
                ["BlackKeyColorRight"] = Color.FromRgb(0xA1, 0xA6, 0xC5),
                ["BlackKeyNameColor"] = Color.FromRgb(0x1D, 0x1F, 0x2A),
            };
        }

        /// <summary>
        /// Computes a bright, prominent color for the pitch prediction line:
        /// the complementary hue of the given accent color (hue + 180°) with
        /// high saturation and a lightness tuned to the palette. This keeps
        /// the line vivid (e.g. orange against blue notes/waveform) in every
        /// preset while still fitting the overall palette logic.
        /// </summary>
        public static Color ProminentComplementary(Color accent, bool darkMode) {
            var (h, s, _) = StudioColorMath.RgbToHsl(accent);
            double hue = (h + 180.0) % 360.0;
            double saturation = Math.Max(s, 0.9);
            double lightness = darkMode ? 0.65 : 0.45;
            return StudioColorMath.HslToRgb(hue, saturation, lightness);
        }

        static Color Lighten(Color c, double amount) => StudioColorMath.Lighten(c, amount);

        static Color Darken(Color c, double amount) => StudioColorMath.Darken(c, amount);
    }
}
