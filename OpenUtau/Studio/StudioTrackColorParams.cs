namespace OpenUtau.App.Studio {
    using System;

    /// <summary>
    /// Default constants of the Studio track palette algorithms. Keeping the
    /// numbers next to the algorithms makes presets override them with the
    /// exact legacy defaults.
    /// </summary>
    public static class StudioTrackColorParams {
        // Rainbow
        /// <summary>Start hue of the rainbow strip (red end, degrees).</summary>
        public const double RainbowStartHue = 8.0;
        /// <summary>Hue span from first to last rainbow track (degrees).</summary>
        public const double RainbowHueSpan = 250.0;
        /// <summary>Tracks after which the rainbow strip cycles.</summary>
        public const int RainbowCycleTracks = 12;

        // Theme gradient
        /// <summary>Base hue span around the theme accent (degrees).</summary>
        public const double GradientBaseHueSpan = 18.0;
        /// <summary>Extra hue span per additional gradient track (degrees).</summary>
        public const double GradientHueSpanPerTrack = 2.5;
        /// <summary>Maximum gradient hue span (degrees).</summary>
        public const double GradientMaxHueSpan = 48.0;
        /// <summary>Theme accent saturation below which a low-chroma ramp is used.</summary>
        public const double GradientLowChromaSeed = 0.12;
        /// <summary>Minimum hue difference between adjacent theme-gradient tracks.</summary>
        public const double GradientNeighborDistinctMin = 0.07;
        /// <summary>Lightness push applied when adjacent gradients are too close.</summary>
        public const double GradientNeighborLightnessPush = 0.08;
        /// <summary>Saturation of the low-chroma fallback ramp.</summary>
        public const double GradientLowChromaFallbackS = 0.28;
        /// <summary>Minimum distinctness required before low-chroma hue fallback.</summary>
        public const double GradientLowChromaDistinctMin = 0.05;
        /// <summary>Dark-theme lightness amplitude along the gradient.</summary>
        public const double GradientDarkLumaAmp = 0.07;
        /// <summary>Light-theme lightness amplitude along the gradient.</summary>
        public const double GradientLightLumaAmp = 0.06;
        /// <summary>Luma-wave phase divisor (2.5 ≈ 5/2 rad per track step).</summary>
        public const double GradientLumaWaveDivisor = 2.5;
    }

    /// <summary>
    /// User-tunable track palette parameters. When a preset omits a field the
    /// default (legacy algorithm) value is used. Non-finite and out-of-range
    /// values from hand-written presets are sanitized by the OrDefault getters
    /// before they reach the palette algorithms.
    /// </summary>
    public sealed class StudioTrackColorConfig {
        public double? RainbowStartHue { get; set; }
        public double? RainbowHueSpan { get; set; }
        public int? RainbowCycleTracks { get; set; }
        public double? GradientBaseHueSpan { get; set; }
        public double? GradientHueSpanPerTrack { get; set; }
        public double? GradientMaxHueSpan { get; set; }
        public double? GradientLowChromaSeed { get; set; }
        public double? GradientNeighborDistinctMin { get; set; }
        public double? GradientNeighborLightnessPush { get; set; }
        public double? GradientLowChromaFallbackS { get; set; }
        public double? GradientLowChromaDistinctMin { get; set; }
        public double? GradientDarkLumaAmp { get; set; }
        public double? GradientLightLumaAmp { get; set; }
        public double? GradientLumaWaveDivisor { get; set; }

        public double RainbowStartHueOrDefault =>
            Sanitize(RainbowStartHue, StudioTrackColorParams.RainbowStartHue, 0, 360);
        public double RainbowHueSpanOrDefault =>
            Sanitize(RainbowHueSpan, StudioTrackColorParams.RainbowHueSpan, 0, 360);
        public int RainbowCycleTracksOrDefault =>
            (int)Math.Round(Sanitize(RainbowCycleTracks, StudioTrackColorParams.RainbowCycleTracks, 1, 24));
        public double GradientBaseHueSpanOrDefault =>
            Sanitize(GradientBaseHueSpan, StudioTrackColorParams.GradientBaseHueSpan, 0, 180);
        public double GradientHueSpanPerTrackOrDefault =>
            Sanitize(GradientHueSpanPerTrack, StudioTrackColorParams.GradientHueSpanPerTrack, 0, 20);
        public double GradientMaxHueSpanOrDefault =>
            Sanitize(GradientMaxHueSpan, StudioTrackColorParams.GradientMaxHueSpan, 1, 180);
        public double GradientLowChromaSeedOrDefault =>
            Sanitize(GradientLowChromaSeed, StudioTrackColorParams.GradientLowChromaSeed, 0, 0.5);
        public double GradientNeighborDistinctMinOrDefault =>
            Sanitize(GradientNeighborDistinctMin, StudioTrackColorParams.GradientNeighborDistinctMin, 0, 1);
        public double GradientNeighborLightnessPushOrDefault =>
            Sanitize(GradientNeighborLightnessPush, StudioTrackColorParams.GradientNeighborLightnessPush, 0, 0.5);
        public double GradientLowChromaFallbackSOrDefault =>
            Sanitize(GradientLowChromaFallbackS, StudioTrackColorParams.GradientLowChromaFallbackS, 0, 1);
        public double GradientLowChromaDistinctMinOrDefault =>
            Sanitize(GradientLowChromaDistinctMin, StudioTrackColorParams.GradientLowChromaDistinctMin, 0, 1);
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
        /// Full explicit set at the legacy default values. Presets saved through
        /// the UI persist this complete set so files round-trip; the algorithm
        /// falls back per-field via the OrDefault getters.
        /// </summary>
        public static StudioTrackColorConfig FromCurrent() => new() {
            RainbowStartHue = StudioTrackColorParams.RainbowStartHue,
            RainbowHueSpan = StudioTrackColorParams.RainbowHueSpan,
            RainbowCycleTracks = StudioTrackColorParams.RainbowCycleTracks,
            GradientBaseHueSpan = StudioTrackColorParams.GradientBaseHueSpan,
            GradientHueSpanPerTrack = StudioTrackColorParams.GradientHueSpanPerTrack,
            GradientMaxHueSpan = StudioTrackColorParams.GradientMaxHueSpan,
            GradientLowChromaSeed = StudioTrackColorParams.GradientLowChromaSeed,
            GradientNeighborDistinctMin = StudioTrackColorParams.GradientNeighborDistinctMin,
            GradientNeighborLightnessPush = StudioTrackColorParams.GradientNeighborLightnessPush,
            GradientLowChromaFallbackS = StudioTrackColorParams.GradientLowChromaFallbackS,
            GradientLowChromaDistinctMin = StudioTrackColorParams.GradientLowChromaDistinctMin,
            GradientDarkLumaAmp = StudioTrackColorParams.GradientDarkLumaAmp,
            GradientLightLumaAmp = StudioTrackColorParams.GradientLightLumaAmp,
            GradientLumaWaveDivisor = StudioTrackColorParams.GradientLumaWaveDivisor,
        };
    }
}
