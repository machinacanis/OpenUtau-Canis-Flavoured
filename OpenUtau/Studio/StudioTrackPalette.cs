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
        // Legacy defaults live in StudioTrackColorParams.cs; every tunable
        // constant used below comes from the passed StudioTrackColorConfig,
        // which falls back to those defaults when a preset omits a field.

        static readonly Color OnWhite = Color.FromRgb(0xF5, 0xF5, 0xF5);
        static readonly Color OnBlack = Color.FromRgb(0x1A, 0x1A, 0x1A);
        static readonly Color LutBlue = Color.FromRgb(0x4E, 0xA6, 0xEA);
        static readonly Color White = Avalonia.Media.Colors.White;

        /// <summary>
        /// Pure. tracks[i] is display index i == TrackNo after UpdateTrackNo.
        /// <paramref name="config"/> (preset-owned tunables) defaults to legacy
        /// constants; null behaves exactly like the pre-config algorithms.
        /// </summary>
        public static StudioTrackSwatch[] ResolveAll(
            IReadOnlyList<UTrack> tracks, StudioTrackPaletteContext ctx,
            StudioTrackColorConfig? config = null) {
            if (tracks == null || tracks.Count == 0) {
                return Array.Empty<StudioTrackSwatch>();
            }
            config ??= StudioTrackColorConfig.Default();
            try {
                return ctx.Mode switch {
                    StudioTrackColorMode.Rainbow => ResolveIndexed(tracks, ctx, config, rainbow: true),
                    StudioTrackColorMode.ThemeGradient => ResolveIndexed(tracks, ctx, config, rainbow: false),
                    _ => ResolveFixed(tracks, ctx, config),
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
            double lWave = WaveformL(l);
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
            IReadOnlyList<UTrack> tracks, StudioTrackPaletteContext ctx,
            StudioTrackColorConfig config) {
            var swatches = new StudioTrackSwatch[tracks.Count];
            for (int i = 0; i < tracks.Count; i++) {
                swatches[i] = FixedFill(tracks[i], ctx, config);
            }
            return swatches;
        }

        static StudioTrackSwatch[] ResolveIndexed(
            IReadOnlyList<UTrack> tracks, StudioTrackPaletteContext ctx,
            StudioTrackColorConfig config, bool rainbow) {
            int n = tracks.Count;
            var fills = new Color[n];
            for (int i = 0; i < n; i++) {
                fills[i] = rainbow
                    ? RainbowFill(i, n, ctx, config)
                    : ThemeFill(i, n, ctx, config);
            }
            if (!rainbow) {
                EnforceNeighborHueDistinctness(fills, ctx.IsDark, config);
            }
            var swatches = new StudioTrackSwatch[n];
            for (int i = 0; i < n; i++) {
                swatches[i] = DeriveSurfaces(fills[i], ctx, headerAccent: fills[i]);
            }
            return swatches;
        }

        static StudioTrackSwatch FixedFill(
            UTrack track, StudioTrackPaletteContext ctx, StudioTrackColorConfig config) {
            string name = track.TrackColor ?? "Blue";
            if (IsDefaultBlue(name)) {
                return IdentityStudioBlue(ctx);
            }
            Color lut = ThemeManager.GetTrackColor(name).AccentColor.Color;
            var (h, s, l) = StudioColorMath.RgbToHsl(lut);
            TargetSL(ctx, out double sFill, out double lFill);
            s = Lerp(s, sFill, 0.30);
            l = Lerp(l, lFill, 0.30);
            Color fill = FinishFill(h, s, l, ctx.IsDark);
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

        static Color RainbowFill(
            int i, int n, StudioTrackPaletteContext ctx, StudioTrackColorConfig config) {
            double h = HueAt(i, n, config);
            TargetSL(ctx, out double s, out double l);
            double lWave = RainbowLuma(i, ctx.IsDark, l, config);
            return FinishFill(h, s, lWave, ctx.IsDark);
        }

        static Color ThemeFill(
            int i, int n, StudioTrackPaletteContext ctx, StudioTrackColorConfig config) {
            if (n <= 1) {
                return ctx.Accent1;
            }
            var (hs, ss, _) = StudioColorMath.RgbToHsl(ctx.Accent1);
            var (_, s2, _) = StudioColorMath.RgbToHsl(ctx.Accent2);
            TargetSL(ctx, out double sFill, out double lFill);
            double lowChromaSeed = config.GradientLowChromaSeedOrDefault;
            double h;
            double s;
            if (ss < lowChromaSeed && s2 < lowChromaSeed) {
                h = HueAt(i, n, config);
                s = Math.Max(sFill * config.GradientLowChromaSatScaleOrDefault,
                    config.GradientLowChromaFallbackSOrDefault);
            } else {
                h = ThemeHue(i, n, hs, config);
                s = sFill;
            }
            double l = LumaForThemeTrack(i, ctx.IsDark, lFill, config);
            ApplyHueCompensation(h, ref s, ref l, ctx.IsDark);
            FitToTargetY(h, s, ref l, ctx.IsDark);
            return StudioColorMath.HslToRgb(h, s, l);
        }

        static Color FinishFill(double h, double s, double l, bool isDark) {
            ApplyHueCompensation(h, ref s, ref l, isDark);
            FitRelativeLuminance(h, s, ref l, isDark);
            return StudioColorMath.HslToRgb(h, s, l);
        }

        static double HueAt(int i, int n, StudioTrackColorConfig config) {
            int cycle = Math.Max(1, config.RainbowCycleTracksOrDefault);
            double h0 = config.RainbowStartHueOrDefault;
            double span = Math.Abs(config.RainbowHueSpanOrDefault);
            if (n <= 1) {
                return HueMod(h0);
            }
            double t = n <= cycle ? i / (double)(n - 1) : (i % cycle) / (double)(cycle - 1);
            double direction = config.RainbowReverseOrDefault ? 1.0 : -1.0;
            return HueMod(h0 + direction * t * span);
        }

        static double ThemeHue(int i, int n, double hs, StudioTrackColorConfig config) {
            double span = Math.Min(config.GradientMaxHueSpanOrDefault,
                config.GradientBaseHueSpanOrDefault
                + config.GradientHueSpanPerTrackOrDefault * n);
            if (n <= 1) {
                return hs;
            }
            double t = i / (double)(n - 1);
            return HueMod(hs - span + 2 * span * t);
        }

        /// <summary>
        /// Theme fill lightness. By default every Theme track is fitted to the
        /// same target relative luminance (a smooth family). The luma knobs add
        /// an optional per-track sine wave around that target; amplitude 0 (the
        /// default) keeps the flat equal-luminance fan.
        /// </summary>
        static double LumaForThemeTrack(
            int i, bool isDark, double lFill, StudioTrackColorConfig config) {
            double amp = isDark
                ? config.GradientDarkLumaAmpOrDefault
                : config.GradientLightLumaAmpOrDefault;
            return LumaWave(i, lFill, amp, config.GradientLumaWaveDivisorOrDefault);
        }

        /// <summary>
        /// Rainbow lightness, mirrors the Theme luma knobs so a rainbow can
        /// breathe per track instead of sitting on one flat lightness.
        /// </summary>
        static double RainbowLuma(
            int i, bool isDark, double lFill, StudioTrackColorConfig config) {
            double amp = isDark
                ? config.RainbowLumaAmpDarkOrDefault
                : config.RainbowLumaAmpLightOrDefault;
            return LumaWave(i, lFill, amp, config.RainbowLumaWaveDivisorOrDefault);
        }

        static double LumaWave(int i, double lFill, double amp, double divisor) {
            if (Math.Abs(amp) < 1e-9) {
                return lFill;
            }
            double d = Math.Abs(divisor) < 1e-9
                ? StudioTrackColorParams.GradientLumaWaveDivisor
                : divisor;
            return StudioColorMath.Clamp(
                lFill + amp * Math.Sin(i * Math.PI / d), 0.34, 0.70);
        }

        /// <summary>
        /// Dark fills are neon on black (mid L, high S). Light fills are colored
        /// paper on a pale ground (high L, low S) — not darkened jewel tones.
        /// </summary>
        static void TargetSL(StudioTrackPaletteContext ctx, out double sFill, out double lFill) {
            var (_, sb, lb) = StudioColorMath.RgbToHsl(ctx.Background);
            if (ctx.IsDark) {
                sFill = StudioColorMath.Clamp(0.68 + 0.08 * (0.35 - sb), 0.62, 0.75);
                lFill = StudioColorMath.Clamp(0.56 - 0.08 * (lb - 0.08), 0.50, 0.62);
            } else {
                sFill = StudioColorMath.Clamp(0.38 - 0.10 * sb, 0.28, 0.48);
                lFill = StudioColorMath.Clamp(0.72 + 0.08 * (lb - 0.80), 0.66, 0.80);
            }
        }

        static void ApplyHueCompensation(double h, ref double s, ref double l, bool isDark) {
            h = HueMod(h);
            if (isDark) {
                if (h >= 45 && h <= 75) {
                    l = Math.Min(l, 0.52);
                    s = Math.Min(s, 0.72);
                }
                if (h >= 90 && h <= 140) {
                    s = Math.Min(s, 0.58);
                }
                if (h >= 170 && h <= 200) {
                    l = Math.Min(l, 0.55);
                }
                if (h >= 280 && h <= 320) {
                    s = Math.Min(s, 0.62);
                }
            } else {
                if (h <= 30 || h >= 330) {
                    s = Math.Min(s, 0.42);
                }
                if (h >= 45 && h <= 140) {
                    s = Math.Min(s, 0.40);
                }
            }
        }

        /// <summary>
        /// HSL L is not perceptual. Lift navy/brick on light paper; lift navy
        /// on dark. Lime/cyan glare is handled by saturation cuts, not by
        /// pulling L into mud.
        /// </summary>
        static void FitRelativeLuminance(double h, double s, ref double l, bool isDark) {
            double yMin = isDark ? 0.14 : 0.46;
            double yMax = isDark ? 1.0 : 0.68;
            double lo = isDark ? 0.48 : 0.66;
            double hi = isDark ? 0.64 : 0.84;
            double y = StudioColorMath.RelativeLuminance(StudioColorMath.HslToRgb(h, s, l));
            if (y >= yMin && y <= yMax) {
                return;
            }
            for (int i = 0; i < 10; i++) {
                double mid = (lo + hi) * 0.5;
                y = StudioColorMath.RelativeLuminance(StudioColorMath.HslToRgb(h, s, mid));
                if (y < yMin) {
                    lo = mid;
                } else if (y > yMax) {
                    hi = mid;
                } else {
                    l = mid;
                    return;
                }
            }
            l = (lo + hi) * 0.5;
        }

        /// <summary>
        /// Theme fan: every Track sits on the same relative luminance so the
        /// hue sweep reads as a smooth family, not a lightness sawtooth.
        /// </summary>
        static void FitToTargetY(double h, double s, ref double l, bool isDark) {
            double yTarget = isDark ? 0.32 : 0.52;
            double lo = isDark ? 0.36 : 0.60;
            double hi = isDark ? 0.80 : 0.88;
            for (int i = 0; i < 12; i++) {
                double mid = (lo + hi) * 0.5;
                double y = StudioColorMath.RelativeLuminance(StudioColorMath.HslToRgb(h, s, mid));
                if (y < yTarget) {
                    lo = mid;
                } else {
                    hi = mid;
                }
            }
            l = (lo + hi) * 0.5;
        }

        /// <summary>
        /// Adjacent theme tracks keep at least <see cref="StudioTrackColorConfig.GradientNeighborHueMinOrDefault"/>
        /// degrees of hue separation; when two are too close the later one is
        /// pushed away by <see cref="StudioTrackColorConfig.GradientNeighborHuePushOrDefault"/>.
        /// </summary>
        static void EnforceNeighborHueDistinctness(
            Color[] fills, bool isDark, StudioTrackColorConfig config) {
            double minDeg = config.GradientNeighborHueMinOrDefault;
            double pushDeg = config.GradientNeighborHuePushOrDefault;
            if (minDeg <= 0) {
                return;
            }
            for (int i = 0; i < fills.Length - 1; i++) {
                var (h1, _, _) = StudioColorMath.RgbToHsl(fills[i]);
                var (h2, s2, l2) = StudioColorMath.RgbToHsl(fills[i + 1]);
                double dh = Math.Min(Math.Abs(h1 - h2), 360 - Math.Abs(h1 - h2));
                if (dh >= minDeg) {
                    continue;
                }
                h2 = HueMod(h2 + pushDeg);
                FitToTargetY(h2, s2, ref l2, isDark);
                fills[i + 1] = StudioColorMath.HslToRgb(h2, s2, l2);
            }
        }

        static Color PickOnFill(Color fill) {
            double cW = StudioColorMath.ContrastRatio(OnWhite, fill);
            double cB = StudioColorMath.ContrastRatio(OnBlack, fill);
            return cW >= cB ? OnWhite : OnBlack;
        }

        static double WaveformL(double l) {
            double sign = l >= 0.5 ? -1 : 1;
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
