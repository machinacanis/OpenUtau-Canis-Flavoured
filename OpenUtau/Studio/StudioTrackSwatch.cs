using Avalonia.Media;

namespace OpenUtau.App.Studio {
    public enum StudioTrackColorMode {
        Fixed = 0,
        Rainbow = 1,
        ThemeGradient = 2,
    }

    public enum StudioTrackPaletteChangeReason {
        TracksMutated,
        TrackColor,
        Mode,
        ThemeResources,
        StudioUIToggled,
    }

    /// <summary>One Track's resolved sRGB. No brushes. Pure data for ResolveAll.</summary>
    public readonly record struct StudioTrackSwatch(
        Color Fill,
        Color FillSelected,
        Color Waveform,
        Color NoteThumbnail,
        Color HeaderAccent,
        Color OnFill,
        Color OnHeaderAccent,
        Color CenterKey,
        Color FillMuted,
        bool DrawSelectedStroke);

    /// <summary>
    /// Algorithm input. No UseTrackColor — that is a UI-mapping concern,
    /// not a fill-generation input.
    /// </summary>
    public readonly record struct StudioTrackPaletteContext(
        StudioTrackColorMode Mode,
        Color Background,
        Color Accent1,
        Color Accent2,
        bool IsDark);

    /// <summary>
    /// Frozen paints for one Track. Built later by StudioTrackPaintCache from a
    /// swatch. Defined here so later PRs can wire controls without moving types.
    /// </summary>
    public sealed class StudioTrackPaint {
        public required StudioTrackSwatch Colors { get; init; }
        public required SolidColorBrush FillBrush { get; init; }
        public required SolidColorBrush FillSelectedBrush { get; init; }
        public required SolidColorBrush FillMutedBrush { get; init; }
        public required SolidColorBrush HeaderAccentBrush { get; init; }
        public required SolidColorBrush OnFillBrush { get; init; }
        public required SolidColorBrush OnHeaderAccentBrush { get; init; }
        /// <summary>Same thickness as today: <c>new Pen(NoteThumbnailBrush, 3)</c>.</summary>
        public required IPen NoteThumbnailPen { get; init; }
        public required IPen? SelectedStrokePen { get; init; }
        /// <summary>
        /// Rgba8888 packed pixel: R | G&lt;&lt;8 | B&lt;&lt;16 | A&lt;&lt;24.
        /// Not <c>Color.ToUInt32()</c>.
        /// </summary>
        public required uint WaveformPackedRgba { get; init; }
        public required TrackColor LegacyTrackColor { get; init; }
        public required SolidColorBrush GhostBrush { get; init; }
    }
}
