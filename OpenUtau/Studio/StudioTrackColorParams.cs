namespace OpenUtau.App.Studio {
    using System;

    /// <summary>
    /// Default constants of the Studio track palette algorithms. Keeping the
    /// numbers next to the algorithms makes presets override them with the
    /// exact defaults.
    /// </summary>
    public static class StudioTrackColorParams {
        // Rainbow
        /// <summary>Start hue of the rainbow strip (red end, degrees).</summary>
        public const double RainbowStartHue = 8.0;
        /// <summary>Hue span from first to last rainbow track (degrees).</summary>
        public const double RainbowHueSpan = 250.0;
        /// <summary>Tracks after which the rainbow strip cycles.</summary>
        public const int RainbowCycleTracks = 12;
        /// <summary>Walk the hue strip upward (increasing hue) instead of downward.</summary>
        public const bool RainbowReverse = false;
        /// <summary>Dark-theme amplitude of the optional per-track rainbow luma wave (0 = off).</summary>
        public const double RainbowLumaAmpDark = 0.0;
        /// <summary>Light-theme amplitude of the optional per-track rainbow luma wave (0 = off).</summary>
        public const double RainbowLumaAmpLight = 0.0;
        /// <summary>Rainbow luma-wave phase divisor (2.5 ≈ 5/2 rad per track step).</summary>
        public const double RainbowLumaWaveDivisor = 2.5;

        // Theme gradient
        /// <summary>Base hue span around the theme accent (degrees).</summary>
        public const double GradientBaseHueSpan = 14.0;
        /// <summary>Extra hue span per additional gradient track (degrees).</summary>
        public const double GradientHueSpanPerTrack = 4.0;
        /// <summary>Maximum gradient hue span (degrees).</summary>
        public const double GradientMaxHueSpan = 52.0;
        /// <summary>Theme accent saturation below which a low-chroma ramp is used.</summary>
        public const double GradientLowChromaSeed = 0.12;
        /// <summary>Minimum hue separation between adjacent theme-gradient tracks (degrees).</summary>
        public const double GradientNeighborHueMin = 6.0;
        /// <summary>Hue offset applied when adjacent gradients are too close (degrees).</summary>
        public const double GradientNeighborHuePush = 8.0;
        /// <summary>Saturation scale (multiplier) of the low-chroma ramp fills.</summary>
        public const double GradientLowChromaSatScale = 0.45;
        /// <summary>Saturation floor of the low-chroma ramp fills.</summary>
        public const double GradientLowChromaFallbackS = 0.22;
        /// <summary>Dark-theme amplitude of the optional per-track luma wave (0 = off).</summary>
        public const double GradientDarkLumaAmp = 0.0;
        /// <summary>Light-theme amplitude of the optional per-track luma wave (0 = off).</summary>
        public const double GradientLightLumaAmp = 0.0;
        /// <summary>Luma-wave phase divisor (2.5 ≈ 5/2 rad per track step).</summary>
        public const double GradientLumaWaveDivisor = 2.5;
    }

    /// <summary>
    /// User-tunable track palette parameters. When a preset omits a field the
    /// default value is used. Non-finite and out-of-range values from
    /// hand-written presets are sanitized by the OrDefault getters before they
    /// reach the palette algorithms.
    /// </summary>
    public sealed class StudioTrackColorConfig {
        public double? RainbowStartHue { get; set; }
        public double? RainbowHueSpan { get; set; }
        public int? RainbowCycleTracks { get; set; }
        public bool? RainbowReverse { get; set; }
        public double? RainbowLumaAmpDark { get; set; }
        public double? RainbowLumaAmpLight { get; set; }
        public double? RainbowLumaWaveDivisor { get; set; }
        public double? GradientBaseHueSpan { get; set; }
        public double? GradientHueSpanPerTrack { get; set; }
        public double? GradientMaxHueSpan { get; set; }
        public double? GradientLowChromaSeed { get; set; }
        public double? GradientNeighborHueMin { get; set; }
        public double? GradientNeighborHuePush { get; set; }
        public double? GradientLowChromaSatScale { get; set; }
        public double? GradientLowChromaFallbackS { get; set; }
        public double? GradientDarkLumaAmp { get; set; }
        public double? GradientLightLumaAmp { get; set; }
        public double? GradientLumaWaveDivisor { get; set; }

        public double RainbowStartHueOrDefault =>
            Sanitize(RainbowStartHue, StudioTrackColorParams.RainbowStartHue, 0, 360);
        public double RainbowHueSpanOrDefault =>
            Sanitize(RainbowHueSpan, StudioTrackColorParams.RainbowHueSpan, 0, 360);
        public int RainbowCycleTracksOrDefault =>
            (int)Math.Round(Sanitize(RainbowCycleTracks, StudioTrackColorParams.RainbowCycleTracks, 1, 24));
        public bool RainbowReverseOrDefault => RainbowReverse ?? StudioTrackColorParams.RainbowReverse;
        public double RainbowLumaAmpDarkOrDefault =>
            Sanitize(RainbowLumaAmpDark, StudioTrackColorParams.RainbowLumaAmpDark, 0, 0.3);
        public double RainbowLumaAmpLightOrDefault =>
            Sanitize(RainbowLumaAmpLight, StudioTrackColorParams.RainbowLumaAmpLight, 0, 0.3);
        public double RainbowLumaWaveDivisorOrDefault =>
            Sanitize(RainbowLumaWaveDivisor, StudioTrackColorParams.RainbowLumaWaveDivisor, 0.1, 10);
        public double GradientBaseHueSpanOrDefault =>
            Sanitize(GradientBaseHueSpan, StudioTrackColorParams.GradientBaseHueSpan, 0, 180);
        public double GradientHueSpanPerTrackOrDefault =>
            Sanitize(GradientHueSpanPerTrack, StudioTrackColorParams.GradientHueSpanPerTrack, 0, 20);
        public double GradientMaxHueSpanOrDefault =>
            Sanitize(GradientMaxHueSpan, StudioTrackColorParams.GradientMaxHueSpan, 1, 180);
        public double GradientLowChromaSeedOrDefault =>
            Sanitize(GradientLowChromaSeed, StudioTrackColorParams.GradientLowChromaSeed, 0, 0.5);
        public double GradientNeighborHueMinOrDefault =>
            Sanitize(GradientNeighborHueMin, StudioTrackColorParams.GradientNeighborHueMin, 0, 360);
        public double GradientNeighborHuePushOrDefault =>
            Sanitize(GradientNeighborHuePush, StudioTrackColorParams.GradientNeighborHuePush, 0, 360);
        public double GradientLowChromaSatScaleOrDefault =>
            Sanitize(GradientLowChromaSatScale, StudioTrackColorParams.GradientLowChromaSatScale, 0, 1);
        public double GradientLowChromaFallbackSOrDefault =>
            Sanitize(GradientLowChromaFallbackS, StudioTrackColorParams.GradientLowChromaFallbackS, 0, 1);
        public double GradientDarkLumaAmpOrDefault =>
            Sanitize(GradientDarkLumaAmp, StudioTrackColorParams.GradientDarkLumaAmp, 0, 0.3);
        public double GradientLightLumaAmpOrDefault =>
            Sanitize(GradientLightLumaAmp, StudioTrackColorParams.GradientLightLumaAmp, 0, 0.3);
        public double GradientLumaWaveDivisorOrDefault =>
            Sanitize(GradientLumaWaveDivisor, StudioTrackColorParams.GradientLumaWaveDivisor, 0.1, 10);

        static double Sanitize(double? value, double fallback, double min, double max) {
            if (!value.HasValue || double.IsNaN(value.Value) || double.IsInfinity(value.Value)) {
                return fallback;
            }
            return Math.Clamp(value.Value, min, max);
        }

        public static StudioTrackColorConfig Default() => new();

        /// <summary>
        /// Full explicit set at the default values. Presets saved through the UI
        /// persist this complete set so files round-trip; the algorithm falls
        /// back per-field via the OrDefault getters.
        /// </summary>
        public static StudioTrackColorConfig FromCurrent() => new() {
            RainbowStartHue = StudioTrackColorParams.RainbowStartHue,
            RainbowHueSpan = StudioTrackColorParams.RainbowHueSpan,
            RainbowCycleTracks = StudioTrackColorParams.RainbowCycleTracks,
            RainbowReverse = StudioTrackColorParams.RainbowReverse,
            RainbowLumaAmpDark = StudioTrackColorParams.RainbowLumaAmpDark,
            RainbowLumaAmpLight = StudioTrackColorParams.RainbowLumaAmpLight,
            RainbowLumaWaveDivisor = StudioTrackColorParams.RainbowLumaWaveDivisor,
            GradientBaseHueSpan = StudioTrackColorParams.GradientBaseHueSpan,
            GradientHueSpanPerTrack = StudioTrackColorParams.GradientHueSpanPerTrack,
            GradientMaxHueSpan = StudioTrackColorParams.GradientMaxHueSpan,
            GradientLowChromaSeed = StudioTrackColorParams.GradientLowChromaSeed,
            GradientNeighborHueMin = StudioTrackColorParams.GradientNeighborHueMin,
            GradientNeighborHuePush = StudioTrackColorParams.GradientNeighborHuePush,
            GradientLowChromaSatScale = StudioTrackColorParams.GradientLowChromaSatScale,
            GradientLowChromaFallbackS = StudioTrackColorParams.GradientLowChromaFallbackS,
            GradientDarkLumaAmp = StudioTrackColorParams.GradientDarkLumaAmp,
            GradientLightLumaAmp = StudioTrackColorParams.GradientLightLumaAmp,
            GradientLumaWaveDivisor = StudioTrackColorParams.GradientLumaWaveDivisor,
        };
    }
}
