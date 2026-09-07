using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using OpenUtau.App.Controls;
using OpenUtau.App.Studio;
using OpenUtau.Core.Util;
using ReactiveUI;

namespace OpenUtau.App {
    class ThemeChangedEvent { }

    class ThemeManager {
        public static bool IsDarkMode = false;
        public static IBrush ForegroundBrush = Brushes.Black;
        public static IBrush BackgroundBrush = Brushes.White;
        public static IBrush NeutralAccentBrush = Brushes.Gray;
        public static IBrush NeutralAccentBrushSemi = Brushes.Gray;
        public static IPen NeutralAccentPen = new Pen(Brushes.Black);
        public static IPen NeutralAccentPenSemi = new Pen(Brushes.Black);
        public static IBrush AccentBrush1 = Brushes.White;
        public static IPen AccentPen1 = new Pen(Brushes.White);
        public static IPen AccentPen1Thickness2 = new Pen(Brushes.White);
        public static IPen AccentPen1Thickness3 = new Pen(Brushes.White);
        public static IPen AccentPen1Thickness4 = new Pen(Brushes.White);
        public static IBrush AccentBrush1Semi = Brushes.Gray;
        public static IBrush AccentBrush2 = Brushes.Gray;
        public static IPen AccentPen2 = new Pen(Brushes.White);
        public static IPen AccentPen2Thickness2 = new Pen(Brushes.White);
        public static IPen AccentPen2Thickness3 = new Pen(Brushes.White);
        public static IPen AccentPen2Thickness4 = new Pen(Brushes.White);
        public static IBrush AccentBrush2Semi = Brushes.Gray;
        public static IBrush AccentBrush3 = Brushes.Gray;
        public static IPen AccentPen3 = new Pen(Brushes.White);
        public static IPen AccentPen3Thick = new Pen(Brushes.White);
        public static IBrush AccentBrush3Semi = Brushes.Gray;
        public static IBrush TickLineBrushLow = Brushes.Black;
        public static IBrush BarNumberBrush = Brushes.Black;
        public static IPen BarNumberPen = new Pen(Brushes.White);
        public static IBrush FinalPitchBrush = Brushes.Gray;
        public static IPen FinalPitchPen = new Pen(Brushes.Gray);
        public static IBrush RealCurveFillBrush = Brushes.Gray;
        public static IBrush RealCurveStrokeBrush = Brushes.Gray;
        public static IPen RealCurvePen = new Pen(Brushes.Gray, 1D, DashStyle.Dash);
        public static IBrush WhiteKeyBrush = Brushes.White;
        public static IBrush WhiteKeyNameBrush = Brushes.Black;
        public static IBrush CenterKeyBrush = Brushes.White;
        public static IBrush CenterKeyNameBrush = Brushes.Black;
        public static IBrush BlackKeyBrush = Brushes.Black;
        public static IBrush BlackKeyNameBrush = Brushes.White;
        public static IBrush ExpBrush = Brushes.White;
        public static IBrush ExpNameBrush = Brushes.Black;
        public static IBrush ExpShadowBrush = Brushes.Gray;
        public static IBrush ExpShadowNameBrush = Brushes.White;
        public static IBrush ExpActiveBrush = Brushes.Black;
        public static IBrush ExpActiveNameBrush = Brushes.White;
        public static IBrush NoteFillBrush = Brushes.Transparent;
        public static IBrush NoteFillSelectedBrush = Brushes.Transparent;
        public static IPen? NoteStrokePen;
        public static IPen? NoteStrokeSelectedPen;
        public static double NoteCornerRadius = 2;
        public static IBrush OnAccentBrush = Brushes.White;
        public static IBrush NoteLyricBrush = Brushes.White;
        public static Typeface NoteLyricTypeface = Typeface.Default;
        public static double NoteLyricFontSize = 12;
        public static Color WaveformColor = Color.FromRgb(0x4E, 0xA6, 0xEA);

        public static List<TrackColor> TrackColors = new List<TrackColor>(){
                new TrackColor("Pink", "#F06292", "#EC407A", "#F48FB1", "#FAC7D8"),
                new TrackColor("Red", "#EF5350", "#E53935", "#E57373", "#F2B9B9"),
                new TrackColor("Orange", "#FF8A65", "#FF7043", "#FFAB91", "#FFD5C8"),
                new TrackColor("Yellow", "#FBC02D", "#F9A825", "#FDD835", "#FEF1B6"),
                new TrackColor("Light Green", "#CDDC39", "#C0CA33", "#DCE775", "#F2F7CE"),
                new TrackColor("Green", "#66BB6A", "#43A047", "#A5D6A7", "#D2EBD3"),
                new TrackColor("Light Blue", "#4FC3F7", "#29B6F6", "#81D4FA", "#C0EAFD"),
                new TrackColor("Blue", "#4EA6EA", "#1E88E5", "#90CAF9", "#C8E5FC"),
                new TrackColor("Purple", "#BA68C8", "#AB47BC", "#CE93D8", "#E7C9EC"),
                new TrackColor("Pink2", "#E91E63", "#C2185B", "#F06292", "#F8B1C9"),
                new TrackColor("Red2", "#D32F2F", "#B71C1C", "#EF5350", "#F7A9A8"),
                new TrackColor("Orange2", "#FF5722", "#E64A19", "#FF7043", "#FFB8A1"),
                new TrackColor("Yellow2", "#FF8F00", "#FF7F00", "#FFB300", "#FFE097"),
                new TrackColor("Light Green2", "#AFB42B", "#9E9D24", "#CDDC39", "#E6EE9C"),
                new TrackColor("Green2", "#2E7D32", "#1B5E20", "#43A047", "#A1D0A3"),
                new TrackColor("Light Blue2", "#1976D2", "#0D47A1", "#2196F3", "#90CBF9"),
                new TrackColor("Blue2", "#3949AB", "#283593", "#5C6BC0", "#AEB5E0"),
                new TrackColor("Purple2", "#7B1FA2", "#4A148C", "#AB47BC", "#D5A3DE"),
            };

        public static List<string> GetAvailableThemes() {
            Colors.CustomTheme.ListThemes();
            return [..StudioUI.BuiltInThemes, ..Colors.CustomTheme.Themes.Select(v => v.Key)];
        }

        public static bool IsBuiltIn(string themeName) => StudioUI.IsBuiltIn(themeName);

        public static void LoadTheme() {
            if (Application.Current == null) {
                return;
            }
            IResourceDictionary resDict = Application.Current.Resources;
            object? outVar;
            IsDarkMode = false;
            var themeVariant = ThemeVariant.Default;
            if (resDict.TryGetResource("IsDarkMode", themeVariant, out outVar)) {
                if (outVar is bool b) {
                    IsDarkMode = b;
                }
            }
            if (resDict.TryGetResource("SystemControlForegroundBaseHighBrush", themeVariant, out outVar)) {
                ForegroundBrush = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("SystemControlBackgroundAltHighBrush", themeVariant, out outVar)) {
                BackgroundBrush = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("NeutralAccentBrush", themeVariant, out outVar)) {
                NeutralAccentBrush = (IBrush)outVar!;
                NeutralAccentPen = new Pen(NeutralAccentBrush, 1);
            }
            if (resDict.TryGetResource("NeutralAccentBrushSemi", themeVariant, out outVar)) {
                NeutralAccentBrushSemi = (IBrush)outVar!;
                NeutralAccentPenSemi = new Pen(NeutralAccentBrushSemi, 1);
            }
            if (resDict.TryGetResource("AccentBrush1", themeVariant, out outVar)) {
                AccentBrush1 = (IBrush)outVar!;
                AccentPen1 = new Pen(AccentBrush1);
                AccentPen1Thickness2 = new Pen(AccentBrush1, 2);
                AccentPen1Thickness3 = new Pen(AccentBrush1, 3);
                AccentPen1Thickness4 = new Pen(AccentBrush1, 4);
            }
            if (resDict.TryGetResource("AccentBrush1Semi", themeVariant, out outVar)) {
                AccentBrush1Semi = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("AccentBrush2", themeVariant, out outVar)) {
                AccentBrush2 = (IBrush)outVar!;
                AccentPen2 = new Pen(AccentBrush2, 1);
                AccentPen2Thickness2 = new Pen(AccentBrush2, 2);
                AccentPen2Thickness3 = new Pen(AccentBrush2, 3);
                AccentPen2Thickness4 = new Pen(AccentBrush2, 4);
            }
            if (resDict.TryGetResource("AccentBrush2Semi", themeVariant, out outVar)) {
                AccentBrush2Semi = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("AccentBrush3", themeVariant, out outVar)) {
                AccentBrush3 = (IBrush)outVar!;
                AccentPen3 = new Pen(AccentBrush3, 1);
                AccentPen3Thick = new Pen(AccentBrush3, 3);
            }
            if (resDict.TryGetResource("AccentBrush3Semi", themeVariant, out outVar)) {
                AccentBrush3Semi = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("TickLineBrushLow", themeVariant, out outVar)) {
                TickLineBrushLow = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("BarNumberBrush", themeVariant, out outVar)) {
                BarNumberBrush = (IBrush)outVar!;
                BarNumberPen = new Pen(BarNumberBrush, 1);
            }
            if (resDict.TryGetResource("FinalPitchBrush", themeVariant, out outVar)) {
                FinalPitchBrush = (IBrush)outVar!;
                FinalPitchPen = new Pen(FinalPitchBrush, 1);
            }
            if (resDict.TryGetResource("RealCurveFillBrush", themeVariant, out outVar)) {
                RealCurveFillBrush = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("RealCurveStrokeBrush", themeVariant, out outVar)) {
                RealCurveStrokeBrush = (IBrush)outVar!;
                RealCurvePen = new Pen(RealCurveStrokeBrush, 2, DashStyle.Dash);
            }
            ApplyPianoRollStyle(false);
            SetKeyboardBrush();
            TextLayoutCache.Clear();
            MessageBus.Current.SendMessage(new ThemeChangedEvent());
        }

        public static void ApplyPianoRollStyle(bool notify = true) {
            var noteColor = SolidColor(AccentBrush1, Color.FromRgb(0x4E, 0xA6, 0xEA));
            var noteSelectedColor = SolidColor(AccentBrush2, noteColor);
            OnAccentBrush = IsDarkMode ? ForegroundBrush : BackgroundBrush;

            if (!StudioUI.IsEnabled) {
                NoteFillBrush = AccentBrush1;
                NoteFillSelectedBrush = AccentBrush2;
                NoteStrokePen = null;
                NoteStrokeSelectedPen = null;
                NoteCornerRadius = 2;
                NoteLyricBrush = Brushes.White;
                NoteLyricTypeface = Typeface.Default;
                NoteLyricFontSize = 12;
                WaveformColor = noteColor;
                if (notify) {
                    TextLayoutCache.Clear();
                    MessageBus.Current.SendMessage(new ThemeChangedEvent());
                }
                return;
            }

            bool solid = Preferences.Default.NoteSolidFill;
            if (solid) {
                NoteFillBrush = AccentBrush1;
                NoteFillSelectedBrush = AccentBrush2;
            } else {
                NoteFillBrush = new SolidColorBrush(noteColor, 0.22);
                NoteFillSelectedBrush = new SolidColorBrush(noteSelectedColor, 0.28);
            }

            ResolvePalette(
                Preferences.Default.NoteStrokeColorMode,
                Preferences.Default.NoteStrokeColorHex,
                Preferences.Default.NoteStrokeColorInvert,
                out var stroke, out var strokeSelected);
            double thickness = Math.Max(0, Preferences.Default.NoteStrokeThickness);
            NoteStrokePen = new Pen(stroke, thickness);
            NoteStrokeSelectedPen = new Pen(strokeSelected, thickness);
            NoteCornerRadius = Preferences.Default.NoteRoundedCorners
                ? Math.Max(0, Preferences.Default.NoteCornerRadiusPx)
                : 0;

            if (Preferences.Default.PitchPredictionColorMode == 0) {
                // Theme mode: always emit a bright, prominent pitch line. The
                // complementary hue of the accent (not a bitwise invert) keeps
                // it vivid — e.g. orange against blue notes/waveform — in every
                // palette. The legacy invert flag is ignored here so the default
                // stays conspicuous; it still applies to the explicit modes.
                FinalPitchBrush = new SolidColorBrush(
                    StudioThemeGenerator.ProminentComplementary(
                        SolidColor(AccentBrush1, Color.FromRgb(0x00, 0xB8, 0xFF)),
                        IsDarkMode));
            } else {
                ResolvePalette(
                    Preferences.Default.PitchPredictionColorMode,
                    Preferences.Default.PitchPredictionColorHex,
                    Preferences.Default.PitchPredictionColorInvert,
                    out var pitch, out _);
                FinalPitchBrush = pitch;
            }
            FinalPitchPen = new Pen(FinalPitchBrush, Math.Max(0, Preferences.Default.PitchPredictionThickness));

            ResolvePalette(
                Preferences.Default.WaveformColorMode,
                Preferences.Default.WaveformColorHex,
                Preferences.Default.WaveformColorInvert,
                out var waveform, out _);
            WaveformColor = SolidColor(waveform, noteColor);

            ResolvePalette(
                Preferences.Default.NoteLyricColorMode,
                Preferences.Default.NoteLyricColorHex,
                Preferences.Default.NoteLyricColorInvert,
                out var lyric, out _);
            NoteLyricBrush = lyric;
            NoteLyricTypeface = BuildLyricTypeface();
            NoteLyricFontSize = 12.0 * (Math.Max(1, Preferences.Default.NoteLyricScalePercent) / 100.0);
            TextLayoutCache.Clear();

            if (notify) {
                MessageBus.Current.SendMessage(new ThemeChangedEvent());
            }
        }

        static Typeface BuildLyricTypeface() {
            FontFamily family = FontFamily.Default;
            string raw = Preferences.Default.NoteLyricFontFamily?.Trim() ?? string.Empty;
            if (raw.Length > 0) {
                try {
                    family = FontFamily.Parse(raw);
                } catch {
                    family = FontFamily.Default;
                }
            }
            var weight = (FontWeight)Math.Clamp(Preferences.Default.NoteLyricWeight, 1, 999);
            var style = Preferences.Default.NoteLyricItalic ? FontStyle.Italic : FontStyle.Normal;
            return new Typeface(family, style, weight);
        }

        static void ResolvePalette(int mode, string? hex, bool invert, out IBrush normal, out IBrush selected) {
            switch (mode) {
                case 1: // accent
                    normal = AccentBrush3;
                    selected = AccentBrush2;
                    break;
                case 2: // gray
                    normal = NeutralAccentBrush;
                    selected = ForegroundBrush;
                    break;
                case 3: // text
                    normal = selected = TextBrush;
                    break;
                case 4: // emphasis text
                    normal = selected = EmphasisTextBrush;
                    break;
                case 5: // rgb hex
                    var rgb = new SolidColorBrush(ParseHexColor(hex, Color.FromRgb(0x4E, 0xA6, 0xEA)));
                    normal = selected = rgb;
                    break;
                default: // theme
                    normal = AccentBrush1;
                    selected = AccentBrush2;
                    break;
            }
            if (invert) {
                normal = InvertBrush(normal);
                selected = InvertBrush(selected);
            }
        }

        /// <summary>
        /// Text color: the theme foreground. Automatically light in dark
        /// palettes and dark in light palettes (the palette decides, inversion
        /// is only needed when the user manually toggles Invert).
        /// </summary>
        static IBrush TextBrush => ForegroundBrush;
        /// <summary>
        /// Emphasis text color: a web-style highlight color (bright blue by
        /// default). The dark palette uses the bright variant, the light
        /// palette automatically switches to the darker blue so emphasized
        /// text stays readable — light/dark is decided by inversion of the
        /// palette, like link/highlight colors on the web.
        /// </summary>
        static readonly IBrush EmphasisDarkBrush = new SolidColorBrush(Color.FromRgb(0x4F, 0xC3, 0xF7));
        static readonly IBrush EmphasisLightBrush = new SolidColorBrush(Color.FromRgb(0x1E, 0x88, 0xE5));
        static IBrush EmphasisTextBrush => IsDarkMode ? EmphasisDarkBrush : EmphasisLightBrush;

        static IBrush InvertBrush(IBrush brush) {
            var c = SolidColor(brush, Color.FromRgb(255, 255, 255));
            return new SolidColorBrush(Color.FromArgb(c.A, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B)));
        }

        public static Color ParseHexColor(string? hex, Color fallback) {
            if (string.IsNullOrWhiteSpace(hex)) {
                return fallback;
            }
            string s = hex.Trim();
            if (s.StartsWith('#')) {
                s = s.Substring(1);
            } else if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) {
                s = s.Substring(2);
            }
            if (s.Length == 3 || s.Length == 4) {
                var expanded = new StringBuilder(s.Length * 2);
                foreach (char c in s) {
                    expanded.Append(c);
                    expanded.Append(c);
                }
                s = expanded.ToString();
            }
            if (s.Length == 6) {
                s += "FF";
            }
            if (s.Length != 8 ||
                !uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value)) {
                return fallback;
            }
            byte r = (byte)((value >> 24) & 0xFF);
            byte g = (byte)((value >> 16) & 0xFF);
            byte b = (byte)((value >> 8) & 0xFF);
            byte a = (byte)(value & 0xFF);
            return Color.FromArgb(a, r, g, b);
        }

        public static Color SolidColor(IBrush brush, Color fallback) {
            return brush is ISolidColorBrush solid ? solid.Color : fallback;
        }

        public static void ChangePianorollColor(string color) {
            ApplyPianorollTrackColor(GetTrackColor(color));
        }

        public static void ChangePianorollColor(StudioTrackPaint paint) {
            ApplyPianorollTrackColor(paint.LegacyTrackColor);
        }

        private static void ApplyPianorollTrackColor(TrackColor tcolor) {
            if (Application.Current == null) {
                return;
            }
            try {
                IResourceDictionary resDict = Application.Current.Resources;

                resDict["SelectedTrackAccentBrush"] = tcolor.AccentColor;
                resDict["SelectedTrackAccentLightBrush"] = tcolor.AccentColorLight;
                resDict["SelectedTrackAccentLightBrushSemi"] = tcolor.AccentColorLightSemi;
                resDict["SelectedTrackAccentDarkBrush"] = tcolor.AccentColorDark;
                resDict["SelectedTrackCenterKeyBrush"] = tcolor.AccentColorCenterKey;

                SetKeyboardBrush();
            } catch { }
            MessageBus.Current.SendMessage(new ThemeChangedEvent());
        }
        private static void SetKeyboardBrush() {
            if (Application.Current == null) {
                return;
            }
            IResourceDictionary resDict = Application.Current.Resources;
            object? outVar;
            var themeVariant = ThemeVariant.Default;

            if (Preferences.Default.UseTrackColor) {
                if (IsDarkMode) {
                    if (resDict.TryGetResource("SelectedTrackAccentBrush", themeVariant, out outVar)) {
                        CenterKeyNameBrush = (IBrush)outVar!;
                        WhiteKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("SelectedTrackCenterKeyBrush", themeVariant, out outVar)) {
                        CenterKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("WhiteKeyNameBrush", themeVariant, out outVar)) {
                        WhiteKeyNameBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("BlackKeyBrush", themeVariant, out outVar)) {
                        BlackKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("BlackKeyNameBrush", themeVariant, out outVar)) {
                        BlackKeyNameBrush = (IBrush)outVar!;
                    }
                    ExpBrush = BlackKeyBrush;
                    ExpNameBrush = BlackKeyNameBrush;
                    ExpActiveBrush = WhiteKeyBrush;
                    ExpActiveNameBrush = WhiteKeyNameBrush;
                    ExpShadowBrush = CenterKeyBrush;
                    ExpShadowNameBrush = CenterKeyNameBrush;
                } else { // LightMode
                    if (resDict.TryGetResource("SelectedTrackAccentBrush", themeVariant, out outVar)) {
                        CenterKeyNameBrush = (IBrush)outVar!;
                        WhiteKeyNameBrush = (IBrush)outVar!;
                        BlackKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("SelectedTrackCenterKeyBrush", themeVariant, out outVar)) {
                        CenterKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("WhiteKeyBrush", themeVariant, out outVar)) {
                        WhiteKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("BlackKeyNameBrush", themeVariant, out outVar)) {
                        BlackKeyNameBrush = (IBrush)outVar!;
                    }
                    ExpBrush = WhiteKeyBrush;
                    ExpNameBrush = WhiteKeyNameBrush;
                    ExpActiveBrush = BlackKeyBrush;
                    ExpActiveNameBrush = BlackKeyNameBrush;
                    ExpShadowBrush = CenterKeyBrush;
                    ExpShadowNameBrush = CenterKeyNameBrush;
                }
            } else { // DefColor
                if (resDict.TryGetResource("WhiteKeyBrush", themeVariant, out outVar)) {
                    WhiteKeyBrush = (IBrush)outVar!;
                }
                if (resDict.TryGetResource("WhiteKeyNameBrush", themeVariant, out outVar)) {
                    WhiteKeyNameBrush = (IBrush)outVar!;
                }
                if (resDict.TryGetResource("CenterKeyBrush", themeVariant, out outVar)) {
                    CenterKeyBrush = (IBrush)outVar!;
                }
                if (resDict.TryGetResource("CenterKeyNameBrush", themeVariant, out outVar)) {
                    CenterKeyNameBrush = (IBrush)outVar!;
                }
                if (resDict.TryGetResource("BlackKeyBrush", themeVariant, out outVar)) {
                    BlackKeyBrush = (IBrush)outVar!;
                }
                if (resDict.TryGetResource("BlackKeyNameBrush", themeVariant, out outVar)) {
                    BlackKeyNameBrush = (IBrush)outVar!;
                }
                if (!IsDarkMode) {
                    ExpBrush = WhiteKeyBrush;
                    ExpNameBrush = WhiteKeyNameBrush;
                    ExpActiveBrush = BlackKeyBrush;
                    ExpActiveNameBrush = BlackKeyNameBrush;
                    ExpShadowBrush = CenterKeyBrush;
                    ExpShadowNameBrush = CenterKeyNameBrush;
                } else {
                    ExpBrush = BlackKeyBrush;
                    ExpNameBrush = BlackKeyNameBrush;
                    ExpActiveBrush = WhiteKeyBrush;
                    ExpActiveNameBrush = WhiteKeyNameBrush;
                    ExpShadowBrush = CenterKeyBrush;
                    ExpShadowNameBrush = CenterKeyNameBrush;
                }
            }
        }

        public static string GetString(string key) {
            TryGetString(key, out string value);
            return value;
        }

        public static bool TryGetString(string key, out string value) {
            if (Application.Current == null) {
                value = key;
                return false;
            }
            IResourceDictionary resDict = Application.Current.Resources;
            if (resDict.TryGetResource(key, ThemeVariant.Default, out var outVar) && outVar is string s) {
                value = s;
                return true;
            }
            value = key;
            return false;
        }

        public static TrackColor GetTrackColor(string name) {
            if (TrackColors.Any(c => c.Name == name)) {
                return TrackColors.First(c => c.Name == name);
            }
            return TrackColors.First(c => c.Name == "Blue");
        }
    }

    public class TrackColor {
        public string Name { get; set; } = "";
        public SolidColorBrush AccentColor { get; set; }
        public SolidColorBrush AccentColorDark { get; set; } // Pressed
        public SolidColorBrush AccentColorLight { get; set; } // PointerOver
        public SolidColorBrush AccentColorLightSemi { get; set; } // BackGround
        public SolidColorBrush AccentColorCenterKey { get; set; } // Keyboard

        public TrackColor(string name, string accentColor, string darkColor, string lightColor, string centerKey) {
            Name = name;
            AccentColor = SolidColorBrush.Parse(accentColor);
            AccentColorDark = SolidColorBrush.Parse(darkColor);
            AccentColorLight = SolidColorBrush.Parse(lightColor);
            AccentColorLightSemi = SolidColorBrush.Parse(lightColor);
            AccentColorLightSemi.Opacity = 0.5;
            AccentColorCenterKey = SolidColorBrush.Parse(centerKey);
        }
    }
}
