using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using OpenUtau.Core.Ustx;
using Serilog;

namespace OpenUtau.App.Studio {
    /// <summary>
    /// Pure Studio Track color resolution. No static cache, no Application.Current,
    /// no MessageBus. Unit tests call these methods with a constructed context.
    /// </summary>
    public static class StudioTrackPalette {
        const double H0 = 8.0;
        const double Hspan = 250.0;
        const int Cycle = 12;
        const double NeighborDistinctMin = 0.07;
        const double NeighborLightnessPush = 0.08;
        const double LowChromaDistinctMin = 0.05;
        const double LowChromaSeed = 0.12;
        const double LowChromaFallbackS = 0.28;

        static readonly Color OnWhite = Color.FromRgb(0xF5, 0xF5, 0xF5);
        static readonly Color OnBlack = Color.FromRgb(0x1A, 0x1A, 0x1A);
        static readonly Color LutBlue = Color.FromRgb(0x4E, 0xA6, 0xEA);
        static readonly Color White = Avalonia.Media.Colors.White;

        /// <summary>
        /// Pure. tracks[i] is display index i == TrackNo after UpdateTrackNo.
        /// </summary>
        public static StudioTrackSwatch[] ResolveAll(
            IReadOnlyList<UTrack> tracks, StudioTrackPaletteContext ctx) {
            if (tracks == null || tracks.Count == 0) {
                return Array.Empty<StudioTrackSwatch>();
            }
            try {
                return ctx.Mode switch {
                    StudioTrackColorMode.Rainbow => ResolveIndexed(tracks, ctx, rainbow: true),
                    StudioTrackColorMode.ThemeGradient => ResolveIndexed(tracks, ctx, rainbow: false),
                    _ => ResolveFixed(tracks, ctx),
                };
            } catch (Exception e) {
                Log.Error(e, "StudioTrackPalette.ResolveAll failed");
                var fallback = new StudioTrackSwatch[tracks.Count];
                for (int i = 0; i < fallback.Length; i++) {
                    fallback[i] = IdentityStudioBlue(ctx);
                }
                return fallback;
            }
        }

        /// <summary>
        /// Pure Classic field set. Ignores ctx.Mode.
        /// </summary>
        public static StudioTrackSwatch ResolveClassicFallback(
            UTrack track, StudioTrackPaletteContext ctx) {
            var lut = ThemeManager.GetTrackColor(track.TrackColor ?? "Blue");
            return new StudioTrackSwatch(
                Fill: ctx.Accent1,
                FillSelected: ctx.Accent2,
                Waveform: White,
                NoteThumbnail: White,
                HeaderAccent: lut.AccentColor.Color,
                OnFill: White,
                OnHeaderAccent: White,
                CenterKey: lut.AccentColorCenterKey.Color,
                FillMuted: ctx.Accent1,
                DrawSelectedStroke: false);
        }

        /// <summary>
        /// Derived surfaces for a non-identity fill. Rainbow, Theme, and Fixed
        /// non-Blue call this; Studio Fixed Blue and Classic do not.
        /// </summary>
        public static StudioTrackSwatch DeriveSurfaces(
            Color fill, StudioTrackPaletteContext ctx, Color headerAccent) {
            var (h, s, l) = StudioColorMath.RgbToHsl(fill);
            Color fillSelected = ctx.IsDark
                ? StudioColorMath.HslToRgb(h, Math.Min(s + 0.05, 1), StudioColorMath.Clamp(l + 0.08, 0, 0.78))
                : StudioColorMath.HslToRgb(h, Math.Min(s + 0.08, 1), StudioColorMath.Clamp(l - 0.06, 0.28, 1));
            double lWave = WaveformL(l, ctx.IsDark);
            Color waveform = StudioColorMath.HslToRgb(h, Math.Min(s + 0.05, 1), lWave);
            Color centerKey = StudioColorMath.HslToRgb(
                h, s * 0.45, ctx.IsDark ? Math.Min(l + 0.25, 0.85) : Math.Max(l - 0.15, 0.75));
            return new StudioTrackSwatch(
                Fill: fill,
                FillSelected: fillSelected,
                Waveform: waveform,
                NoteThumbnail: waveform,
                HeaderAccent: headerAccent,
                OnFill: PickOnFill(fill),
                OnHeaderAccent: PickOnFill(headerAccent),
                CenterKey: centerKey,
                FillMuted: MixMuted(fill, ctx),
                DrawSelectedStroke: true);
        }

        static StudioTrackSwatch[] ResolveFixed(
            IReadOnlyList<UTrack> tracks, StudioTrackPaletteContext ctx) {
            var swatches = new StudioTrackSwatch[tracks.Count];
            for (int i = 0; i < tracks.Count; i++) {
                swatches[i] = FixedFill(tracks[i], ctx);
            }
            return swatches;
        }

        static StudioTrackSwatch[] ResolveIndexed(
            IReadOnlyList<UTrack> tracks, StudioTrackPaletteContext ctx, bool rainbow) {
            int n = tracks.Count;
            var fills = new Color[n];
            for (int i = 0; i < n; i++) {
                fills[i] = rainbow ? RainbowFill(i, n, ctx) : ThemeFill(i, n, ctx);
            }
            if (!rainbow) {
                EnforceNeighborDistinctness(fills);
                ApplyLowChromaHueFallback(fills, n);
            }
            var swatches = new StudioTrackSwatch[n];
            for (int i = 0; i < n; i++) {
                swatches[i] = DeriveSurfaces(fills[i], ctx, headerAccent: fills[i]);
            }
            return swatches;
        }

        static StudioTrackSwatch FixedFill(UTrack track, StudioTrackPaletteContext ctx) {
            string name = track.TrackColor ?? "Blue";
            if (IsDefaultBlue(name)) {
                return IdentityStudioBlue(ctx);
            }
            Color lut = ThemeManager.GetTrackColor(name).AccentColor.Color;
            var (h, s, l) = StudioColorMath.RgbToHsl(lut);
            TargetSL(ctx, out double sFill, out double lFill);
            s = Lerp(s, sFill, 0.30);
            l = Lerp(l, lFill, 0.30);
            ApplyHueCompensation(h, ref s, ref l, ctx.IsDark);
            Color fill = StudioColorMath.HslToRgb(h, s, l);
            return DeriveSurfaces(fill, ctx, headerAccent: lut);
        }

        static StudioTrackSwatch IdentityStudioBlue(StudioTrackPaletteContext ctx) {
            var blue = ThemeManager.GetTrackColor("Blue");
            return new StudioTrackSwatch(
                Fill: ctx.Accent1,
                FillSelected: ctx.Accent2,
                Waveform: White,
                NoteThumbnail: White,
                HeaderAccent: LutBlue,
                OnFill: White,
                OnHeaderAccent: White,
                CenterKey: blue.AccentColorCenterKey.Color,
                FillMuted: MixMuted(ctx.Accent1, ctx),
                DrawSelectedStroke: false);
        }

        static bool IsDefaultBlue(string name) =>
            string.IsNullOrEmpty(name)
            || name == "Blue"
            || !ThemeManager.TrackColors.Any(c => c.Name == name);

        static Color RainbowFill(int i, int n, StudioTrackPaletteContext ctx) {
            double h = HueAt(i, n);
            TargetSL(ctx, out double s, out double l);
            ApplyHueCompensation(h, ref s, ref l, ctx.IsDark);
            return StudioColorMath.HslToRgb(h, s, l);
        }

        static Color ThemeFill(int i, int n, StudioTrackPaletteContext ctx) {
            if (n <= 1) {
                return ctx.Accent1;
            }
            var (hs, ss, _) = StudioColorMath.RgbToHsl(ctx.Accent1);
            var (_, s2, _) = StudioColorMath.RgbToHsl(ctx.Accent2);
            TargetSL(ctx, out double sFill, out double lFill);
            double h = ThemeHue(i, n, hs, ss, s2);
            ThemeSL(i, ctx.IsDark, ss, sFill, lFill, out double s, out double l);
            ApplyHueCompensation(h, ref s, ref l, ctx.IsDark);
            return StudioColorMath.HslToRgb(h, s, l);
        }

        static double HueAt(int i, int n) {
            if (n <= 1) {
                return H0;
            }
            double t = n <= Cycle ? i / (double)(n - 1) : (i % Cycle) / (double)(Cycle - 1);
            return HueMod(H0 - t * Hspan);
        }

        static double ThemeHue(int i, int n, double hs, double ss, double s2) {
            if (ss < LowChromaSeed && s2 < LowChromaSeed) {
                return hs;
            }
            double span = Math.Min(48.0, 18.0 + 2.5 * n);
            if (n <= 1) {
                return hs;
            }
            double t = i / (double)(n - 1);
            return HueMod(hs - span + 2 * span * t);
        }

        static void ThemeSL(
            int i, bool isDark, double ss, double sFill, double lFill,
            out double s, out double l) {
            double amp = isDark ? 0.07 : 0.06;
            l = StudioColorMath.Clamp(lFill + amp * Math.Sin(i * Math.PI / 2.5), 0.34, 0.70);
            s = ss < LowChromaSeed ? Math.Max(sFill * 0.45, 0.22) : Lerp(ss, sFill, 0.55);
        }

        static void TargetSL(StudioTrackPaletteContext ctx, out double sFill, out double lFill) {
            var (_, sb, lb) = StudioColorMath.RgbToHsl(ctx.Background);
            if (ctx.IsDark) {
                sFill = StudioColorMath.Clamp(0.72 + 0.15 * (0.35 - sb), 0.62, 0.90);
                lFill = StudioColorMath.Clamp(0.58 - 0.10 * (lb - 0.08), 0.50, 0.64);
            } else {
                sFill = StudioColorMath.Clamp(0.55 - 0.20 * sb, 0.40, 0.65);
                lFill = StudioColorMath.Clamp(0.42 + 0.08 * (lb - 0.80), 0.38, 0.50);
            }
        }

        static void ApplyHueCompensation(double h, ref double s, ref double l, bool isDark) {
            h = HueMod(h);
            if (h >= 45 && h <= 75) {
                l = Math.Min(l, isDark ? 0.52 : 0.40);
                s = Math.Min(s, 0.78);
            }
            if (h >= 170 && h <= 200) {
                l = Math.Min(l, isDark ? 0.55 : 0.42);
            }
        }

        static void EnforceNeighborDistinctness(Color[] fills) {
            for (int i = 0; i < fills.Length - 1; i++) {
                if (Distinctness(fills[i], fills[i + 1]) >= NeighborDistinctMin) {
                    continue;
                }
                var (_, _, l1) = StudioColorMath.RgbToHsl(fills[i]);
                var (h2, s2, l2) = StudioColorMath.RgbToHsl(fills[i + 1]);
                l2 = l2 >= l1 ? l2 + NeighborLightnessPush : l2 - NeighborLightnessPush;
                l2 = StudioColorMath.Clamp(l2, 0, 1);
                fills[i + 1] = StudioColorMath.HslToRgb(h2, s2, l2);
            }
        }

        static void ApplyLowChromaHueFallback(Color[] fills, int n) {
            for (int i = 0; i < fills.Length - 1; i++) {
                if (Distinctness(fills[i], fills[i + 1]) >= LowChromaDistinctMin) {
                    continue;
                }
                var (_, _, l) = StudioColorMath.RgbToHsl(fills[i + 1]);
                fills[i + 1] = StudioColorMath.HslToRgb(HueAt(i + 1, n), LowChromaFallbackS, l);
            }
        }

        static double Distinctness(Color a, Color b) {
            var (h1, _, l1) = StudioColorMath.RgbToHsl(a);
            var (h2, _, l2) = StudioColorMath.RgbToHsl(b);
            double dh = Math.Min(Math.Abs(h1 - h2), 360 - Math.Abs(h1 - h2)) / 360.0;
            return Math.Max(dh, Math.Abs(l1 - l2));
        }

        static Color PickOnFill(Color fill) {
            double cW = StudioColorMath.ContrastRatio(OnWhite, fill);
            double cB = StudioColorMath.ContrastRatio(OnBlack, fill);
            return cW >= cB ? OnWhite : OnBlack;
        }

        static double WaveformL(double l, bool dark) {
            double sign = dark ? -1 : 1;
            double result = StudioColorMath.Clamp(l + sign * 0.22, 0.18, 0.85);
            if (Math.Abs(result - l) < 0.12) {
                result = StudioColorMath.Clamp(l + sign * 0.12, 0, 1);
            }
            return result;
        }

        static Color MixMuted(Color fill, StudioTrackPaletteContext ctx) {
            Color mixed = StudioColorMath.Mix(fill, ctx.Background, 0.55);
            var (h, s, l) = StudioColorMath.RgbToHsl(mixed);
            return StudioColorMath.HslToRgb(h, s * 0.6, l);
        }

        static double HueMod(double h) => ((h % 360) + 360) % 360;

        static double Lerp(double a, double b, double t) => a * (1 - t) + b * t;
    }
}
