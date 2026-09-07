using System.Collections.Generic;

namespace OpenUtau.App.Studio {
    /// <summary>
    /// A Studio UI preset: a named bundle of a color palette plus the Studio UI
    /// appearance values. Presets are plain YAML files under
    /// <c>DataPath/Presets</c>, so dropping a file in that folder adds a preset.
    /// </summary>
    public class StudioPreset {
        /// <summary>
        /// Preset file format version. Built-in presets are regenerated when
        /// their file version is older than
        /// <see cref="StudioPresetManager.BuiltInVersion"/>.
        /// </summary>
        public int Version { get; set; } = 0;
        public string Name { get; set; } = "";
        public bool IsDark { get; set; } = true;
        public Dictionary<string, string> Palette { get; set; } = new();
        public StudioPresetUi Ui { get; set; } = new();
    }

    /// <summary>
    /// Studio UI appearance values carried by a preset. Nullable members mean
    /// "not specified": a hand-written preset may omit any of them and only the
    /// listed values are applied when the preset is selected.
    /// </summary>
    public class StudioPresetUi {
        /// <summary>
        /// Track color mode stored with the preset: 0 = Fixed, 1 = Rainbow,
        /// 2 = Theme gradient. Null keeps the user's current global mode.
        /// </summary>
        public int? TrackColorMode { get; set; }
        /// <summary>Rainbow / theme-gradient tunables (preset-owned).</summary>
        public StudioTrackColorConfig? TrackColorConfig { get; set; }
        public int? WaveformStyle { get; set; }
        public int? WaveformLayout { get; set; }
        public int? WaveformFollowMode { get; set; }
        public int? WaveformFadeInMs { get; set; }
        public int? WaveformFadeOutMs { get; set; }
        public int? WaveformColorMode { get; set; }
        public string? WaveformColorHex { get; set; }
        public bool? WaveformColorInvert { get; set; }
        public int? WaveformScalePercent { get; set; }
        public int? WaveformFixedBottomPx { get; set; }
        public int? NoteStrokeColorMode { get; set; }
        public string? NoteStrokeColorHex { get; set; }
        public bool? NoteStrokeColorInvert { get; set; }
        public int? NoteStrokeThickness { get; set; }
        public int? PitchPredictionColorMode { get; set; }
        public string? PitchPredictionColorHex { get; set; }
        public bool? PitchPredictionColorInvert { get; set; }
        public int? PitchPredictionThickness { get; set; }
        public bool? NoteRoundedCorners { get; set; }
        public int? NoteCornerRadiusPx { get; set; }
        public bool? NoteSolidFill { get; set; }
        public int? NoteLyricVAlign { get; set; }
        public int? NoteLyricHAlign { get; set; }
        public string? NoteLyricFontFamily { get; set; }
        public int? NoteLyricScalePercent { get; set; }
        public int? NoteLyricWeight { get; set; }
        public bool? NoteLyricItalic { get; set; }
        public int? NoteLyricColorMode { get; set; }
        public string? NoteLyricColorHex { get; set; }
        public bool? NoteLyricColorInvert { get; set; }
    }
}