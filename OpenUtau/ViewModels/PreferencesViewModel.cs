using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using OpenUtau.Audio;
using OpenUtau.Classic;
using OpenUtau.Core;
using OpenUtau.Core.Util;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Primitives;
using ReactiveUI.Avalonia;
using OpenUtau.App.Studio;
using OpenUtau.Core.HiFiUtau;
using OpenUtau.Core.Render;
using Serilog;

namespace OpenUtau.App.ViewModels {
    public class LyricsHelperOption {
        public readonly Type klass;
        public LyricsHelperOption(Type klass) {
            this.klass = klass;
        }
        public override string ToString() {
            return klass.Name;
        }
    }

    public partial class PreferencesViewModel : ViewModelBase {
        // General
        private CultureInfo? language;
        private CultureInfo? sortingOrder;

        public List<CultureInfo>? Languages { get; }
        public CultureInfo? Language {
            get => language;
            set => this.RaiseAndSetIfChanged(ref language, value);
        }
        public List<CultureInfo>? SortingOrders { get; }
        public CultureInfo? SortingOrder {
            get => sortingOrder;
            set => this.RaiseAndSetIfChanged(ref sortingOrder, value);
        }
        // Release channel index: 0 = stable, 1 = beta, 2 = alpha.
        [Reactive] public partial int Channel { get; set; }

        // Playback
        private List<AudioOutputDevice>? audioOutputDevices;
        private AudioOutputDevice? audioOutputDevice;

        public List<AudioOutputDevice>? AudioOutputDevices {
            get => audioOutputDevices;
            set => this.RaiseAndSetIfChanged(ref audioOutputDevices, value);
        }
        public AudioOutputDevice? AudioOutputDevice {
            get => audioOutputDevice;
            set => this.RaiseAndSetIfChanged(ref audioOutputDevice, value);
        }
        [Reactive] public partial bool UseSystemDefaultDevice { get; set; }
        [Reactive] public partial int PreferPortAudio { get; set; }
        [Reactive] public partial uint AudioBackEnd { get; set; }
        [Reactive] public partial int LockStartTime { get; set; }
        [Reactive] public partial int PlaybackAutoScroll { get; set; }
        [Reactive] public partial double PlayPosMarkerMargin { get; set; }
        [Reactive] public partial int MetronomeVolume { get; set; }
        [Reactive] public partial int MetronomeHighFrequency { get; set; }
        [Reactive] public partial int MetronomeLowFrequency { get; set; }

        // Paths
        public string SingerPath => PathManager.Inst.SingersPath;
        public string AdditionalSingersPath => !string.IsNullOrWhiteSpace(PathManager.Inst.AdditionalSingersPath) ? PathManager.Inst.AdditionalSingersPath : "(None)";
        [Reactive] public partial bool InstallToAdditionalSingersPath { get; set; }
        [Reactive] public partial bool LoadDeepFolders { get; set; }

        // Editing
        public List<LyricsHelperOption> LyricsHelpers { get; } =
            ActiveLyricsHelper.Inst.Available
                .Select(klass => new LyricsHelperOption(klass))
                .ToList();
        [Reactive] public partial LyricsHelperOption? LyricsHelper { get; set; }
        [Reactive] public partial bool LyricsHelperBrackets { get; set; }
        [Reactive] public partial bool PenPlusDefault { get; set; }

        // Render
        [Reactive] public partial bool PreRender { get; set; }
        [Reactive] public partial int NumRenderThreads { get; set; }
        public int LogicalCoreCount {
            get => Environment.ProcessorCount;
        }
        [Reactive] public partial bool HighThreads { get; set; }
        public int SafeMaxThreadCount {
            get => Math.Min(8, LogicalCoreCount / 2);
        }
        [Reactive] public partial bool SkipRenderingMutedTracks { get; set; }
        [Reactive] public partial bool ClearCacheOnQuit { get; set; }
        public List<string> OnnxRunnerOptions { get; set; }
        [Reactive] public partial string OnnxRunner { get; set; }
        public List<GpuInfo> OnnxGpuOptions { get; set; }
        [Reactive] public partial GpuInfo OnnxGpu { get; set; }
        [Reactive] public partial bool ShowOnnxGpu { get; set; }

        // GAME backend (onnx / ggml)
        public List<string> GameBackendOptions { get; } = new() { "ONNX", "GGML" };
        [Reactive] public partial string GameBackend { get; set; }

        // Appearance
        [Reactive] public partial string ThemeName { get; set; }
        [Reactive] public partial int DegreeStyle { get; set; }
        [Reactive] public partial bool UseTrackColor { get; set; }
        [Reactive] public partial bool ShowPortrait { get; set; }
        [Reactive] public partial bool ShowIcon { get; set; }
        [Reactive] public partial bool ShowGhostNotes { get; set; }
        [Reactive] public partial bool NoteHoverGlow { get; set; }
        [Reactive] public partial bool ShowPlaybackNoteHighlight { get; set; }
        [Reactive] public partial bool ShowPlaybackNoteBounce { get; set; }
        [Reactive] public partial bool DetachPianoRoll { get; set; }
        [Reactive] public partial bool ThemeEditable { get; set; }
        public List<string> ThemeItems => ThemeManager.GetAvailableThemes();
        public bool IsThemeEditorOpen => Views.ThemeEditorWindow.IsOpen;
        public bool ThemeSelectionEnabled => !IsThemeEditorOpen && !UseStudioUI;

        // Studio UI
        [Reactive] public partial bool UseStudioUI { get; set; }
        [Reactive] public partial int StudioTrackColorMode { get; set; }
        [Reactive] public partial double RainbowStartHue { get; set; }
        [Reactive] public partial double RainbowHueSpan { get; set; }
        [Reactive] public partial int RainbowCycleTracks { get; set; }
        [Reactive] public partial bool RainbowReverse { get; set; }
        [Reactive] public partial double RainbowLumaAmpDark { get; set; }
        [Reactive] public partial double RainbowLumaAmpLight { get; set; }
        [Reactive] public partial double RainbowLumaWaveDivisor { get; set; }
        [Reactive] public partial double GradientBaseHueSpan { get; set; }
        [Reactive] public partial double GradientHueSpanPerTrack { get; set; }
        [Reactive] public partial double GradientMaxHueSpan { get; set; }
        [Reactive] public partial double GradientLowChromaSeed { get; set; }
        [Reactive] public partial double GradientNeighborHueMin { get; set; }
        [Reactive] public partial double GradientNeighborHuePush { get; set; }
        [Reactive] public partial double GradientLowChromaFallbackS { get; set; }
        [Reactive] public partial double GradientLowChromaSatScale { get; set; }
        [Reactive] public partial double GradientDarkLumaAmp { get; set; }
        [Reactive] public partial double GradientLightLumaAmp { get; set; }
        [Reactive] public partial double GradientLumaWaveDivisor { get; set; }
        [Reactive] public partial string CurrentPreset { get; set; } = string.Empty;
        [Reactive] public partial string SelectedPresetItem { get; set; } = string.Empty;
        public List<string> PresetItems { get; private set; } = new();
        public bool PresetDirty { get; private set; }
        bool _applyingPreset;
        static readonly HashSet<string> PresetUiPropertyNames = new(StringComparer.Ordinal) {
            nameof(StudioTrackColorMode),
            nameof(RainbowStartHue),
            nameof(RainbowHueSpan),
            nameof(RainbowCycleTracks),
            nameof(RainbowReverse),
            nameof(RainbowLumaAmpDark),
            nameof(RainbowLumaAmpLight),
            nameof(RainbowLumaWaveDivisor),
            nameof(GradientBaseHueSpan),
            nameof(GradientHueSpanPerTrack),
            nameof(GradientMaxHueSpan),
            nameof(GradientLowChromaSeed),
            nameof(GradientNeighborHueMin),
            nameof(GradientNeighborHuePush),
            nameof(GradientLowChromaFallbackS),
            nameof(GradientLowChromaSatScale),
            nameof(GradientDarkLumaAmp),
            nameof(GradientLightLumaAmp),
            nameof(GradientLumaWaveDivisor),
            nameof(WaveformStyle),
            nameof(WaveformLayout),
            nameof(WaveformFollowMode),
            nameof(WaveformFadeInMs),
            nameof(WaveformFadeOutMs),
            nameof(WaveformColorMode),
            nameof(WaveformColorHex),
            nameof(WaveformColorInvert),
            nameof(WaveformScalePercent),
            nameof(WaveformFixedBottomPx),
            nameof(WaveformAmpScaleRows),
            nameof(WaveformFollowOffsetRows),
            nameof(WaveformSmartSplitDown),
            nameof(WaveformSmartSplitUp),
            nameof(WaveformSmartReturnSlop),
            nameof(WaveformSmartLookAhead),
            nameof(WaveformFallbackLeadMs),
            nameof(WaveformAlphaPercent),
            nameof(NoteStrokeColorMode),
            nameof(NoteStrokeColorHex),
            nameof(NoteStrokeColorInvert),
            nameof(NoteStrokeThickness),
            nameof(PitchPredictionColorMode),
            nameof(PitchPredictionColorHex),
            nameof(PitchPredictionColorInvert),
            nameof(PitchPredictionThickness),
            nameof(NoteRoundedCorners),
            nameof(NoteCornerRadiusPx),
            nameof(NoteSolidFill),
            nameof(NoteLyricVAlign),
            nameof(NoteLyricHAlign),
            nameof(NoteLyricFontFamily),
            nameof(NoteLyricScalePercent),
            nameof(NoteLyricWeight),
            nameof(NoteLyricItalic),
            nameof(NoteLyricColorMode),
            nameof(NoteLyricColorHex),
            nameof(NoteLyricColorInvert),
            nameof(NoteLyricPaddingPx),
            nameof(NoteLyricShrinkToFit),
        };
        [Reactive] public partial int WaveformStyle { get; set; }
        [Reactive] public partial int WaveformLayout { get; set; }
        [Reactive] public partial int WaveformFollowMode { get; set; }
        [Reactive] public partial int WaveformFadeInMs { get; set; }
        [Reactive] public partial int WaveformFadeOutMs { get; set; }
        [Reactive] public partial int WaveformColorMode { get; set; }
        [Reactive] public partial string WaveformColorHex { get; set; }
        [Reactive] public partial bool WaveformColorInvert { get; set; }
        [Reactive] public partial int WaveformScalePercent { get; set; }
        [Reactive] public partial int WaveformFixedBottomPx { get; set; }
        [Reactive] public partial double WaveformAmpScaleRows { get; set; }
        [Reactive] public partial double WaveformFollowOffsetRows { get; set; }
        [Reactive] public partial double WaveformSmartSplitDown { get; set; }
        [Reactive] public partial double WaveformSmartSplitUp { get; set; }
        [Reactive] public partial double WaveformSmartReturnSlop { get; set; }
        [Reactive] public partial int WaveformSmartLookAhead { get; set; }
        [Reactive] public partial double WaveformFallbackLeadMs { get; set; }
        [Reactive] public partial int WaveformAlphaPercent { get; set; }
        [Reactive] public partial int NoteStrokeColorMode { get; set; }
        [Reactive] public partial string NoteStrokeColorHex { get; set; }
        [Reactive] public partial bool NoteStrokeColorInvert { get; set; }
        [Reactive] public partial int NoteStrokeThickness { get; set; }
        [Reactive] public partial int PitchPredictionColorMode { get; set; }
        [Reactive] public partial string PitchPredictionColorHex { get; set; }
        [Reactive] public partial bool PitchPredictionColorInvert { get; set; }
        [Reactive] public partial int PitchPredictionThickness { get; set; }
        [Reactive] public partial bool NoteRoundedCorners { get; set; }
        [Reactive] public partial int NoteCornerRadiusPx { get; set; }
        [Reactive] public partial bool NoteSolidFill { get; set; }
        [Reactive] public partial int NoteLyricVAlign { get; set; }
        [Reactive] public partial int NoteLyricHAlign { get; set; }
        [Reactive] public partial string NoteLyricFontFamily { get; set; }
        [Reactive] public partial int NoteLyricScalePercent { get; set; }
        [Reactive] public partial int NoteLyricWeight { get; set; }
        [Reactive] public partial bool NoteLyricItalic { get; set; }
        [Reactive] public partial int NoteLyricColorMode { get; set; }
        [Reactive] public partial string NoteLyricColorHex { get; set; }
        [Reactive] public partial bool NoteLyricColorInvert { get; set; }
        [Reactive] public partial int NoteLyricPaddingPx { get; set; }
        [Reactive] public partial bool NoteLyricShrinkToFit { get; set; }
        [Reactive] public partial double NoteHoverGlowDurationSec { get; set; }
        [Reactive] public partial double PlaybackHighlightFadeInPerSec { get; set; }
        [Reactive] public partial double PlaybackHighlightFadeOutPerSec { get; set; }
        [Reactive] public partial double PlaybackNoteBounceDurationSec { get; set; }
        [Reactive] public partial double PlaybackNoteBounceHeightPx { get; set; }
        public bool WaveformGradientOptionsVisible => WaveformStyle == 0 && WaveformLayout != 2;
        public bool WaveformFollowOptionsVisible => WaveformLayout == 1;
        public bool WaveformFixedOptionsVisible => WaveformLayout == 2;
        public bool WaveformFillOptionsVisible => WaveformLayout != 2;
        public bool NoteCornerRadiusVisible => NoteRoundedCorners;
        public bool RainbowTrackOptionsVisible => StudioTrackColorMode == 1;
        public bool GradientTrackOptionsVisible => StudioTrackColorMode == 2;

        // UTAU
        public List<string> DefaultRendererOptions { get; set; }
        [Reactive] public partial string DefaultRenderer { get; set; }
        [Reactive] public partial int OtoEditor { get; set; }
        public string VLabelerPath => Preferences.Default.VLabelerPath;
        public string SetParamPath => Preferences.Default.SetParamPath;

        // Diffsinger
        public List<int> DiffSingerStepsOptions { get; } = new List<int> { 2, 5, 10, 20, 50, 100, 200, 500, 1000 };
        public List<int> DiffSingerStepsVarianceOptions { get; } = new List<int> { 2, 5, 10, 20, 50, 100, 200, 500, 1000 };
        public List<int> DiffSingerStepsPitchOptions { get; } = new List<int> { 2, 5, 10, 20, 50, 100, 200, 500, 1000 };
        [Reactive] public partial int DiffSingerSteps { get; set; }
        [Reactive] public partial int DiffSingerStepsVariance { get; set; }
        [Reactive] public partial int DiffSingerStepsPitch { get; set; }
        [Reactive] public partial double DiffSingerDepth { get; set; }
        [Reactive] public partial bool DiffSingerTensorCache { get; set; }
        [Reactive] public partial bool DiffSingerVarianceLocalPitchPatch { get; set; }
        [Reactive] public partial bool DiffSingerLangCodeHide { get; set; }
        [Reactive] public partial bool DiffSingerLocalRetaking { get; set; }

        // HiFiUTAU / Custom Server
        [Reactive] public partial bool HifiUtauEmbedded { get; set; }
        [Reactive] public partial string HifiUtauSplicerPath { get; set; } = string.Empty;
        [Reactive] public partial string HifiUtauHnsepPath { get; set; } = string.Empty;
        [Reactive] public partial bool HifiUtauPreload { get; set; }
        public List<int> HifiUtauThreadsOptions { get; } = new List<int> { 0, 1, 2, 4, 8 };
        [Reactive] public partial int HifiUtauIntraOpThreads { get; set; }
        [Reactive] public partial string CustomServerUrl { get; set; } = string.Empty;
        [Reactive] public partial string HifiUtauStatus { get; set; } = string.Empty;

        // Advanced
        [Reactive] public partial bool RememberMid { get; set; }
        [Reactive] public partial bool RememberUst { get; set; }
        [Reactive] public partial bool RememberVsqx { get; set; }
        [Reactive] public partial bool Wayland { get; set; }
        public string WinePath => Preferences.Default.WinePath;

        public PreferencesViewModel() {
            var audioOutput = PlaybackManager.Inst.AudioOutput;
            if (audioOutput != null) {
                AudioOutputDevices = audioOutput.GetOutputDevices();
                int deviceNumber = audioOutput.DeviceNumber;
                var device = AudioOutputDevices.FirstOrDefault(d => d.deviceNumber == deviceNumber);
                if (device != null) {
                    AudioOutputDevice = device;
                }
            }
            UseSystemDefaultDevice = Preferences.Default.UseSystemDefaultAudioDevice;
            AudioBackEnd = Preferences.Default.AudioBackEnd;
            PlaybackAutoScroll = Preferences.Default.PlaybackAutoScroll;
            PlayPosMarkerMargin = Preferences.Default.PlayPosMarkerMargin;
            MetronomeVolume = Preferences.Default.MetronomeVolume;
            MetronomeHighFrequency = Preferences.Default.MetronomeHighFrequency;
            MetronomeLowFrequency = Preferences.Default.MetronomeLowFrequency;
            LockStartTime = Preferences.Default.LockStartTime;
            InstallToAdditionalSingersPath = Preferences.Default.InstallToAdditionalSingersPath;
            LoadDeepFolders = Preferences.Default.LoadDeepFolderSinger;
            ToolsManager.Inst.Initialize();
            var pattern = new Regex(@"Strings\.([\w-]+)\.axaml");
            Languages = App.GetLanguages().Keys
                .Select(lang => CultureInfo.GetCultureInfo(lang))
                .ToList();
            Language = string.IsNullOrEmpty(Preferences.Default.Language)
                ? null
                : CultureInfo.GetCultureInfo(Preferences.Default.Language);
            SortingOrders = Languages.ToList();
            SortingOrders.Insert(0, CultureInfo.InvariantCulture);
            SortingOrder = Preferences.Default.SortingOrder == null ? Language
                : string.IsNullOrEmpty(Preferences.Default.SortingOrder) ? CultureInfo.InvariantCulture
                : CultureInfo.GetCultureInfo(Preferences.Default.SortingOrder);
            PreRender = Preferences.Default.PreRender;
            DefaultRendererOptions = Renderers.getRendererOptions();
            DefaultRenderer = String.IsNullOrEmpty(Preferences.Default.DefaultRenderer) ?
               DefaultRendererOptions[0] : Preferences.Default.DefaultRenderer;
            NumRenderThreads = Preferences.Default.NumRenderThreads;
            OnnxRunnerOptions = Onnx.getRunnerOptions();
            OnnxRunner = String.IsNullOrEmpty(Preferences.Default.OnnxRunner) ?
               OnnxRunnerOptions[0] : Preferences.Default.OnnxRunner;
            OnnxGpuOptions = Onnx.getGpuInfo();
OnnxGpu = OnnxGpuOptions.Count > 0
                ? OnnxGpuOptions.FirstOrDefault(x => x.deviceId == Preferences.Default.OnnxGpu, OnnxGpuOptions[0])
                : new GpuInfo();
            ShowOnnxGpu = (OnnxRunner == "DirectML" || OnnxRunner == "CUDA");
            HifiUtauEmbedded = Preferences.Default.HifiUtauEmbedded;
            HifiUtauSplicerPath = Preferences.Default.HifiUtauSplicerPath;
            HifiUtauHnsepPath = Preferences.Default.HifiUtauHnsepPath;
            HifiUtauPreload = Preferences.Default.HifiUtauPreload;
            HifiUtauIntraOpThreads = Preferences.Default.HifiUtauIntraOpThreads;
            CustomServerUrl = Preferences.Default.DefaultServerUrl;
            RefreshHifiUtauStatus();
            // GAME backend: ONNX is the default, GGML is available when installed.
            // The options list always includes both so the ComboBox UX is stable.
            GameBackend = Preferences.Default.GameBackend switch {
                "ggml" => "GGML",
                _ => "ONNX",  // default / empty / unrecognized all map to ONNX
            };
            DiffSingerDepth = Preferences.Default.DiffSingerDepth * 100;
            DiffSingerSteps = Preferences.Default.DiffSingerSteps;
            DiffSingerStepsVariance = Preferences.Default.DiffSingerStepsVariance;
            DiffSingerStepsPitch = Preferences.Default.DiffSingerStepsPitch;
            DiffSingerTensorCache = Preferences.Default.DiffSingerTensorCache;
            DiffSingerVarianceLocalPitchPatch = Preferences.Default.DiffSingerVarianceLocalPitchPatch;
            DiffSingerLangCodeHide = Preferences.Default.DiffSingerLangCodeHide;
            DiffSingerLocalRetaking = Preferences.Default.DiffSingerLocalRetaking;
            SkipRenderingMutedTracks = Preferences.Default.SkipRenderingMutedTracks;
            ThemeName = Preferences.Default.ThemeName;
            DegreeStyle = Preferences.Default.DegreeStyle;
            UseTrackColor = Preferences.Default.UseTrackColor;
            ShowPortrait = Preferences.Default.ShowPortrait;
            ShowIcon = Preferences.Default.ShowIcon;
            ShowGhostNotes = Preferences.Default.ShowGhostNotes;
            NoteHoverGlow = Preferences.Default.NoteHoverGlow;
            ShowPlaybackNoteHighlight = Preferences.Default.ShowPlaybackNoteHighlight;
            ShowPlaybackNoteBounce = Preferences.Default.ShowPlaybackNoteBounce;
            NoteHoverGlowDurationSec = Preferences.Default.NoteHoverGlowDurationSec;
            PlaybackHighlightFadeInPerSec = Preferences.Default.PlaybackHighlightFadeInPerSec;
            PlaybackHighlightFadeOutPerSec = Preferences.Default.PlaybackHighlightFadeOutPerSec;
            PlaybackNoteBounceDurationSec = Preferences.Default.PlaybackNoteBounceDurationSec;
            PlaybackNoteBounceHeightPx = Preferences.Default.PlaybackNoteBounceHeightPx;
            DetachPianoRoll = Preferences.Default.DetachPianoRoll;
            UseStudioUI = Preferences.Default.UseStudioUI;
            StudioTrackColorMode = Preferences.Default.StudioTrackColorMode;
            RainbowStartHue = Preferences.Default.RainbowStartHue;
            RainbowHueSpan = Preferences.Default.RainbowHueSpan;
            RainbowCycleTracks = Preferences.Default.RainbowCycleTracks;
            RainbowReverse = Preferences.Default.RainbowReverse;
            RainbowLumaAmpDark = Preferences.Default.RainbowLumaAmpDark;
            RainbowLumaAmpLight = Preferences.Default.RainbowLumaAmpLight;
            RainbowLumaWaveDivisor = Preferences.Default.RainbowLumaWaveDivisor;
            GradientBaseHueSpan = Preferences.Default.GradientBaseHueSpan;
            GradientHueSpanPerTrack = Preferences.Default.GradientHueSpanPerTrack;
            GradientMaxHueSpan = Preferences.Default.GradientMaxHueSpan;
            GradientLowChromaSeed = Preferences.Default.GradientLowChromaSeed;
            GradientNeighborHueMin = Preferences.Default.GradientNeighborHueMin;
            GradientNeighborHuePush = Preferences.Default.GradientNeighborHuePush;
            GradientLowChromaFallbackS = Preferences.Default.GradientLowChromaFallbackS;
            GradientLowChromaSatScale = Preferences.Default.GradientLowChromaSatScale;
            GradientDarkLumaAmp = Preferences.Default.GradientDarkLumaAmp;
            GradientLightLumaAmp = Preferences.Default.GradientLightLumaAmp;
            GradientLumaWaveDivisor = Preferences.Default.GradientLumaWaveDivisor;
            InitPresets();
            WaveformStyle = Preferences.Default.WaveformStyle;
            WaveformLayout = Preferences.Default.WaveformLayout;
            WaveformFollowMode = Preferences.Default.WaveformFollowMode;
            WaveformFadeInMs = Preferences.Default.WaveformFadeInMs;
            WaveformFadeOutMs = Preferences.Default.WaveformFadeOutMs;
            WaveformColorMode = Preferences.Default.WaveformColorMode;
            WaveformColorHex = Preferences.Default.WaveformColorHex;
            WaveformColorInvert = Preferences.Default.WaveformColorInvert;
            WaveformScalePercent = Preferences.Default.WaveformScalePercent;
            WaveformFixedBottomPx = Preferences.Default.WaveformFixedBottomPx;
            WaveformAmpScaleRows = Preferences.Default.WaveformAmpScaleRows;
            WaveformFollowOffsetRows = Preferences.Default.WaveformFollowOffsetRows;
            WaveformSmartSplitDown = Preferences.Default.WaveformSmartSplitDown;
            WaveformSmartSplitUp = Preferences.Default.WaveformSmartSplitUp;
            WaveformSmartReturnSlop = Preferences.Default.WaveformSmartReturnSlop;
            WaveformSmartLookAhead = Preferences.Default.WaveformSmartLookAhead;
            WaveformFallbackLeadMs = Preferences.Default.WaveformFallbackLeadMs;
            WaveformAlphaPercent = Preferences.Default.WaveformAlphaPercent;
            NoteStrokeColorMode = Preferences.Default.NoteStrokeColorMode;
            NoteStrokeColorHex = Preferences.Default.NoteStrokeColorHex;
            NoteStrokeColorInvert = Preferences.Default.NoteStrokeColorInvert;
            NoteStrokeThickness = Preferences.Default.NoteStrokeThickness;
            PitchPredictionColorMode = Preferences.Default.PitchPredictionColorMode;
            PitchPredictionColorHex = Preferences.Default.PitchPredictionColorHex;
            PitchPredictionColorInvert = Preferences.Default.PitchPredictionColorInvert;
            PitchPredictionThickness = Preferences.Default.PitchPredictionThickness;
            NoteRoundedCorners = Preferences.Default.NoteRoundedCorners;
            NoteCornerRadiusPx = Preferences.Default.NoteCornerRadiusPx;
            NoteSolidFill = Preferences.Default.NoteSolidFill;
            NoteLyricVAlign = Preferences.Default.NoteLyricVAlign;
            NoteLyricHAlign = Preferences.Default.NoteLyricHAlign;
            NoteLyricFontFamily = Preferences.Default.NoteLyricFontFamily;
            NoteLyricScalePercent = Preferences.Default.NoteLyricScalePercent;
            NoteLyricWeight = Preferences.Default.NoteLyricWeight;
            NoteLyricItalic = Preferences.Default.NoteLyricItalic;
            NoteLyricColorMode = Preferences.Default.NoteLyricColorMode;
            NoteLyricColorHex = Preferences.Default.NoteLyricColorHex;
            NoteLyricColorInvert = Preferences.Default.NoteLyricColorInvert;
            NoteLyricPaddingPx = Preferences.Default.NoteLyricPaddingPx;
            NoteLyricShrinkToFit = Preferences.Default.NoteLyricShrinkToFit;
            Channel = Preferences.Default.Channel switch {
                "beta" => 1,
                "alpha" => 2,
                _ => 0
            };
            LyricsHelper = LyricsHelpers.FirstOrDefault(option => option.klass.Equals(ActiveLyricsHelper.Inst.GetPreferred()));
            LyricsHelperBrackets = Preferences.Default.LyricsHelperBrackets;
            OtoEditor = Preferences.Default.OtoEditor;
            RememberMid = Preferences.Default.RememberMid;
            RememberUst = Preferences.Default.RememberUst;
            RememberVsqx = Preferences.Default.RememberVsqx;
            ClearCacheOnQuit = Preferences.Default.ClearCacheOnQuit;
            Wayland = Preferences.Default.UseWayland;

            MessageBus.Current.Listen<ThemeEditorStateChangedEvent>()
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsThemeEditorOpen)));
            
            this.WhenAnyValue(vm => vm.UseSystemDefaultDevice)
                .Subscribe(useDefault => {
                    Preferences.Default.UseSystemDefaultAudioDevice = useDefault;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.AudioOutputDevice)
                .OfType<AudioOutputDevice>()
                .SubscribeOn(AvaloniaScheduler.Instance)
                .Subscribe(device => {
                    if (UseSystemDefaultDevice) {
                        return;
                    }
                    if (PlaybackManager.Inst.AudioOutput != null) {
                        try {
                            PlaybackManager.Inst.AudioOutput.SelectDevice(device.guid, device.deviceNumber);
                        } catch (Exception e) {
                            DocManager.Inst.ExecuteCmd(new ErrorMessageNotification($"Failed to select device {device.name}", e));
                        }
                    }
                });
            this.WhenAnyValue(vm => vm.AudioBackEnd)
                .Subscribe(index => {
                    Preferences.Default.AudioBackEnd = index;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.PlaybackAutoScroll)
                .Subscribe(autoScroll => {
                    Preferences.Default.PlaybackAutoScroll = autoScroll;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.PlayPosMarkerMargin)
                .Subscribe(playPosMarkerMargin => {
                    Preferences.Default.PlayPosMarkerMargin = playPosMarkerMargin;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.MetronomeVolume)
                .Subscribe(metronomeVolume => {
                    Preferences.Default.MetronomeVolume = metronomeVolume;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.MetronomeHighFrequency)
                .Subscribe(metronomeHighFrequency => {
                    Preferences.Default.MetronomeHighFrequency = metronomeHighFrequency;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.MetronomeLowFrequency)
                .Subscribe(metronomeLowFrequency => {
                    Preferences.Default.MetronomeLowFrequency = metronomeLowFrequency;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.LockStartTime)
                .Subscribe(lockStartTime => {
                    Preferences.Default.LockStartTime = lockStartTime;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.InstallToAdditionalSingersPath)
                .Subscribe(additionalSingersPath => {
                    Preferences.Default.InstallToAdditionalSingersPath = additionalSingersPath;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.Wayland)
                .Subscribe(additionalSingersPath => {
                    Preferences.Default.UseWayland = Wayland;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.LoadDeepFolders)
                .Subscribe(loadDeepFolders => {
                    Preferences.Default.LoadDeepFolderSinger = loadDeepFolders;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.PreRender)
                .Subscribe(preRender => {
                    Preferences.Default.PreRender = preRender;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.Language)
                .OfType<CultureInfo>()
                .Subscribe(lang => {
                    Preferences.Default.Language = lang?.Name ?? string.Empty;
                    Preferences.Save();
                    App.SetLanguage(Preferences.Default.Language);
                });
            this.WhenAnyValue(vm => vm.SortingOrder)
                .OfType<CultureInfo>()
                .Subscribe(so => {
                    Preferences.Default.SortingOrder = so?.Name ?? null;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.ThemeName)
                .Subscribe(themeName => {
                    ThemeEditable = !ThemeManager.IsBuiltIn(themeName) && !Colors.CustomTheme.IsPackageTheme(themeName);
                    if (!IsThemeEditorOpen) {
                        Preferences.Default.ThemeName = themeName;
                        Preferences.Save();
                        App.SetTheme();
                    }
                });
            this.WhenAnyValue(vm => vm.UseStudioUI)
                .Subscribe(enabled => {
                    Preferences.Default.UseStudioUI = enabled;
                    bool themeChanged = enabled
                        ? StudioUI.EnsureStudioTheme()
                        : StudioUI.EnsureClassicTheme();
                    if (themeChanged && ThemeName != Preferences.Default.ThemeName) {
                        ThemeName = Preferences.Default.ThemeName;
                    }
                    Preferences.Save();
                    this.RaisePropertyChanged(nameof(ThemeItems));
                    this.RaisePropertyChanged(nameof(ThemeSelectionEnabled));
                    App.SetTheme();
                    StudioUI.NotifyChanged();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                    MessageBus.Current.SendMessage(new NotesRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.StudioTrackColorMode)
                .Subscribe(mode => {
                    Preferences.Default.StudioTrackColorMode = mode;
                    Preferences.Save();
                    this.RaisePropertyChanged(nameof(RainbowTrackOptionsVisible));
                    this.RaisePropertyChanged(nameof(GradientTrackOptionsVisible));
                    StudioTrackPaintCache.Invalidate(StudioTrackPaletteChangeReason.Mode);
                });
            this.WhenAnyValue(vm => vm.RainbowStartHue)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.RainbowStartHue = v));
            this.WhenAnyValue(vm => vm.RainbowHueSpan)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.RainbowHueSpan = v));
            this.WhenAnyValue(vm => vm.RainbowCycleTracks)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.RainbowCycleTracks = v));
            this.WhenAnyValue(vm => vm.RainbowReverse)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.RainbowReverse = v));
            this.WhenAnyValue(vm => vm.RainbowLumaAmpDark)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.RainbowLumaAmpDark = v));
            this.WhenAnyValue(vm => vm.RainbowLumaAmpLight)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.RainbowLumaAmpLight = v));
            this.WhenAnyValue(vm => vm.RainbowLumaWaveDivisor)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.RainbowLumaWaveDivisor = v));
            this.WhenAnyValue(vm => vm.GradientBaseHueSpan)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.GradientBaseHueSpan = v));
            this.WhenAnyValue(vm => vm.GradientHueSpanPerTrack)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.GradientHueSpanPerTrack = v));
            this.WhenAnyValue(vm => vm.GradientMaxHueSpan)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.GradientMaxHueSpan = v));
            this.WhenAnyValue(vm => vm.GradientLowChromaSeed)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.GradientLowChromaSeed = v));
            this.WhenAnyValue(vm => vm.GradientNeighborHueMin)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.GradientNeighborHueMin = v));
            this.WhenAnyValue(vm => vm.GradientNeighborHuePush)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.GradientNeighborHuePush = v));
            this.WhenAnyValue(vm => vm.GradientLowChromaFallbackS)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.GradientLowChromaFallbackS = v));
            this.WhenAnyValue(vm => vm.GradientLowChromaSatScale)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.GradientLowChromaSatScale = v));
            this.WhenAnyValue(vm => vm.GradientDarkLumaAmp)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.GradientDarkLumaAmp = v));
            this.WhenAnyValue(vm => vm.GradientLightLumaAmp)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.GradientLightLumaAmp = v));
            this.WhenAnyValue(vm => vm.GradientLumaWaveDivisor)
                .Subscribe(v => OnTrackColorTuned(() => Preferences.Default.GradientLumaWaveDivisor = v));
            this.WhenAnyValue(vm => vm.SelectedPresetItem)
                .Subscribe(display => {
                    if (string.IsNullOrEmpty(display)) {
                        return;
                    }
                    string name = display.EndsWith(" *", StringComparison.Ordinal)
                        ? display[..^2]
                        : display;
                    // Never re-apply the currently selected preset: the dirty
                    // marker ("*") also flows through this property and would
                    // otherwise reset the very change the user just made.
                    if (name == CurrentPreset) {
                        return;
                    }
                    ApplyPreset(name);
                });
            // Any change to a preset-owned UI value marks the current preset as
            // modified (shown as a trailing "*" in the preset list).
            PropertyChanged += OnPresetPropertyChanged;
            this.WhenAnyValue(vm => vm.WaveformStyle)
                .Subscribe(style => {
                    Preferences.Default.WaveformStyle = style;
                    Preferences.Save();
                    this.RaisePropertyChanged(nameof(WaveformGradientOptionsVisible));
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformLayout)
                .Subscribe(layout => {
                    Preferences.Default.WaveformLayout = layout;
                    Preferences.Save();
                    this.RaisePropertyChanged(nameof(WaveformFollowOptionsVisible));
                    this.RaisePropertyChanged(nameof(WaveformFixedOptionsVisible));
                    this.RaisePropertyChanged(nameof(WaveformFillOptionsVisible));
                    this.RaisePropertyChanged(nameof(WaveformGradientOptionsVisible));
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformFollowMode)
                .Subscribe(mode => {
                    Preferences.Default.WaveformFollowMode = mode;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformFadeInMs)
                .Subscribe(ms => {
                    Preferences.Default.WaveformFadeInMs = ms;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformFadeOutMs)
                .Subscribe(ms => {
                    Preferences.Default.WaveformFadeOutMs = ms;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformColorMode)
                .Subscribe(_ => {
                    Preferences.Default.WaveformColorMode = WaveformColorMode;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.WaveformColorHex)
                .Subscribe(hex => {
                    Preferences.Default.WaveformColorHex = hex ?? string.Empty;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.WaveformColorInvert)
                .Subscribe(invert => {
                    Preferences.Default.WaveformColorInvert = invert;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.WaveformScalePercent)
                .Subscribe(percent => {
                    Preferences.Default.WaveformScalePercent = percent;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformFixedBottomPx)
                .Subscribe(px => {
                    Preferences.Default.WaveformFixedBottomPx = px;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformAmpScaleRows)
                .Subscribe(v => {
                    Preferences.Default.WaveformAmpScaleRows = v;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformFollowOffsetRows)
                .Subscribe(v => {
                    Preferences.Default.WaveformFollowOffsetRows = v;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformSmartSplitDown)
                .Subscribe(v => {
                    Preferences.Default.WaveformSmartSplitDown = v;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformSmartSplitUp)
                .Subscribe(v => {
                    Preferences.Default.WaveformSmartSplitUp = v;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformSmartReturnSlop)
                .Subscribe(v => {
                    Preferences.Default.WaveformSmartReturnSlop = v;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformSmartLookAhead)
                .Subscribe(v => {
                    Preferences.Default.WaveformSmartLookAhead = v;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformFallbackLeadMs)
                .Subscribe(v => {
                    Preferences.Default.WaveformFallbackLeadMs = v;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.WaveformAlphaPercent)
                .Subscribe(v => {
                    Preferences.Default.WaveformAlphaPercent = v;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new WaveformRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.NoteStrokeColorMode)
                .Subscribe(_ => {
                    Preferences.Default.NoteStrokeColorMode = NoteStrokeColorMode;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteStrokeColorHex)
                .Subscribe(hex => {
                    Preferences.Default.NoteStrokeColorHex = hex ?? string.Empty;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteStrokeColorInvert)
                .Subscribe(invert => {
                    Preferences.Default.NoteStrokeColorInvert = invert;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteStrokeThickness)
                .Subscribe(thickness => {
                    Preferences.Default.NoteStrokeThickness = thickness;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.PitchPredictionColorMode)
                .Subscribe(_ => {
                    Preferences.Default.PitchPredictionColorMode = PitchPredictionColorMode;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.PitchPredictionColorHex)
                .Subscribe(hex => {
                    Preferences.Default.PitchPredictionColorHex = hex ?? string.Empty;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.PitchPredictionColorInvert)
                .Subscribe(invert => {
                    Preferences.Default.PitchPredictionColorInvert = invert;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.PitchPredictionThickness)
                .Subscribe(thickness => {
                    Preferences.Default.PitchPredictionThickness = thickness;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteRoundedCorners)
                .Subscribe(rounded => {
                    Preferences.Default.NoteRoundedCorners = rounded;
                    Preferences.Save();
                    this.RaisePropertyChanged(nameof(NoteCornerRadiusVisible));
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteCornerRadiusPx)
                .Subscribe(px => {
                    Preferences.Default.NoteCornerRadiusPx = px;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteSolidFill)
                .Subscribe(solid => {
                    Preferences.Default.NoteSolidFill = solid;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteLyricVAlign)
                .Subscribe(align => {
                    Preferences.Default.NoteLyricVAlign = align;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteLyricHAlign)
                .Subscribe(align => {
                    Preferences.Default.NoteLyricHAlign = align;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteLyricFontFamily)
                .Subscribe(family => {
                    Preferences.Default.NoteLyricFontFamily = family ?? string.Empty;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteLyricScalePercent)
                .Subscribe(percent => {
                    Preferences.Default.NoteLyricScalePercent = percent;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteLyricWeight)
                .Subscribe(weight => {
                    Preferences.Default.NoteLyricWeight = weight;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteLyricItalic)
                .Subscribe(italic => {
                    Preferences.Default.NoteLyricItalic = italic;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteLyricColorMode)
                .Subscribe(_ => {
                    Preferences.Default.NoteLyricColorMode = NoteLyricColorMode;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteLyricColorHex)
                .Subscribe(hex => {
                    Preferences.Default.NoteLyricColorHex = hex ?? string.Empty;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteLyricColorInvert)
                .Subscribe(invert => {
                    Preferences.Default.NoteLyricColorInvert = invert;
                    Preferences.Save();
                    ThemeManager.ApplyPianoRollStyle();
                });
            this.WhenAnyValue(vm => vm.NoteLyricPaddingPx)
                .Subscribe(px => {
                    Preferences.Default.NoteLyricPaddingPx = px;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new NotesRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.NoteLyricShrinkToFit)
                .Subscribe(shrink => {
                    Preferences.Default.NoteLyricShrinkToFit = shrink;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new NotesRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.DegreeStyle)
                .Subscribe(degreeStyle => {
                    Preferences.Default.DegreeStyle = degreeStyle;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new PianorollRefreshEvent("Part"));
                });
            this.WhenAnyValue(vm => vm.UseTrackColor)
                .Subscribe(trackColor => {
                    Preferences.Default.UseTrackColor = trackColor;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new PianorollRefreshEvent("TrackColor"));
                });
            this.WhenAnyValue(vm => vm.ShowPortrait)
                .Subscribe(showPortrait => {
                    Preferences.Default.ShowPortrait = showPortrait;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new PianorollRefreshEvent("Portrait"));
                });
            this.WhenAnyValue(vm => vm.ShowIcon)
                .Subscribe(showIcon => {
                    Preferences.Default.ShowIcon = showIcon;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new PianorollRefreshEvent("Portrait"));
                });
            this.WhenAnyValue(vm => vm.ShowGhostNotes)
                .Subscribe(showGhostNotes => {
                    Preferences.Default.ShowGhostNotes = showGhostNotes;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new PianorollRefreshEvent("Part"));
                });
            this.WhenAnyValue(vm => vm.NoteHoverGlow)
                .Subscribe(noteHoverGlow => {
                    Preferences.Default.NoteHoverGlow = noteHoverGlow;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new NotesRefreshEvent());
                });
            this.WhenAnyValue(vm => vm.ShowPlaybackNoteHighlight)
                .Subscribe(showPlaybackNoteHighlight => {
                    Preferences.Default.ShowPlaybackNoteHighlight = showPlaybackNoteHighlight;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new PianorollRefreshEvent("PlaybackNoteHighlight"));
                });
            this.WhenAnyValue(vm => vm.ShowPlaybackNoteBounce)
                .Subscribe(showPlaybackNoteBounce => {
                    Preferences.Default.ShowPlaybackNoteBounce = showPlaybackNoteBounce;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new PianorollRefreshEvent("PlaybackNoteBounce"));
                });
            this.WhenAnyValue(vm => vm.NoteHoverGlowDurationSec)
                .Subscribe(v => {
                    Preferences.Default.NoteHoverGlowDurationSec = v;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.PlaybackHighlightFadeInPerSec)
                .Subscribe(v => {
                    Preferences.Default.PlaybackHighlightFadeInPerSec = v;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.PlaybackHighlightFadeOutPerSec)
                .Subscribe(v => {
                    Preferences.Default.PlaybackHighlightFadeOutPerSec = v;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.PlaybackNoteBounceDurationSec)
                .Subscribe(v => {
                    Preferences.Default.PlaybackNoteBounceDurationSec = v;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.PlaybackNoteBounceHeightPx)
                .Subscribe(v => {
                    Preferences.Default.PlaybackNoteBounceHeightPx = v;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.DetachPianoRoll)
                .Subscribe(detachPianoRoll => {
                    Preferences.Default.DetachPianoRoll = detachPianoRoll;
                    Preferences.Save();
                    MessageBus.Current.SendMessage(new PianorollRefreshEvent("Attachment"));
                });
            this.WhenAnyValue(vm => vm.Channel)
                .Subscribe(channel => {
                    Preferences.Default.Channel = channel switch {
                        1 => "beta",
                        2 => "alpha",
                        _ => "stable"
                    };
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.LyricsHelper)
                .OfType<LyricsHelperOption>()
                .Subscribe(option => {
                    ActiveLyricsHelper.Inst.Set(option?.klass);
                    Preferences.Default.LyricHelper = option?.klass?.Name ?? string.Empty;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.LyricsHelperBrackets)
                .Subscribe(brackets => {
                    Preferences.Default.LyricsHelperBrackets = brackets;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.OtoEditor)
                .Subscribe(index => {
                    Preferences.Default.OtoEditor = index;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.NumRenderThreads)
                .Subscribe(index => {
                    Preferences.Default.NumRenderThreads = index;
                    HighThreads = index > SafeMaxThreadCount ? true : false;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.DefaultRenderer)
                .Subscribe(index => {
                    Preferences.Default.DefaultRenderer = index;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.OnnxRunner)
                .Subscribe(index => {
                    Preferences.Default.OnnxRunner = index;
                    Preferences.Save();
                    ToggleOnnxGpuDisplay(index == "DirectML" || index == "CUDA");
                });
            this.WhenAnyValue(vm => vm.OnnxGpu)
                .Subscribe(index => {
                    Preferences.Default.OnnxGpu = index.deviceId;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.HifiUtauEmbedded)
                .Subscribe(value => {
                    Preferences.Default.HifiUtauEmbedded = value;
                    Preferences.Save();
                    RefreshHifiUtauStatus();
                });
            this.WhenAnyValue(vm => vm.HifiUtauPreload)
                .Subscribe(value => {
                    Preferences.Default.HifiUtauPreload = value;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.HifiUtauSplicerPath)
                .Subscribe(path => {
                    Preferences.Default.HifiUtauSplicerPath = path ?? string.Empty;
                    Preferences.Save();
                    RefreshHifiUtauStatus();
                });
            this.WhenAnyValue(vm => vm.HifiUtauHnsepPath)
                .Subscribe(path => {
                    Preferences.Default.HifiUtauHnsepPath = path ?? string.Empty;
                    Preferences.Save();
                    RefreshHifiUtauStatus();
                });
            this.WhenAnyValue(vm => vm.HifiUtauIntraOpThreads)
                .Subscribe(value => {
                    Preferences.Default.HifiUtauIntraOpThreads = value;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.CustomServerUrl)
                .Subscribe(url => {
                    if (!string.IsNullOrWhiteSpace(url)) {
                        Preferences.Default.DefaultServerUrl = url;
                        Preferences.Save();
                    }
                });
            this.WhenAnyValue(vm => vm.GameBackend)
                .Subscribe(index => {
                    Preferences.Default.GameBackend = index == "GGML" ? "ggml" : "onnx";
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.RememberMid)
                .Subscribe(index => {
                    Preferences.Default.RememberMid = index;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.RememberUst)
                .Subscribe(index => {
                    Preferences.Default.RememberUst = index;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.RememberVsqx)
                .Subscribe(index => {
                    Preferences.Default.RememberVsqx = index;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.ClearCacheOnQuit)
                .Subscribe(index => {
                    Preferences.Default.ClearCacheOnQuit = index;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.DiffSingerSteps)
                .Subscribe(index => {
                    Preferences.Default.DiffSingerSteps = index;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.DiffSingerStepsVariance)
                 .Subscribe(index => {
                     Preferences.Default.DiffSingerStepsVariance = index;
                     Preferences.Save();
                 });
            this.WhenAnyValue(vm => vm.DiffSingerStepsPitch)
                .Subscribe(index => {
                    Preferences.Default.DiffSingerStepsPitch = index;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.DiffSingerDepth)
                .Subscribe(index => {
                    Preferences.Default.DiffSingerDepth = index / 100;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.DiffSingerTensorCache)
                .Subscribe(useCache => {
                    Preferences.Default.DiffSingerTensorCache = useCache;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.DiffSingerVarianceLocalPitchPatch)
                .Subscribe(useLocalPatch => {
                    Preferences.Default.DiffSingerVarianceLocalPitchPatch = useLocalPatch;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.DiffSingerLangCodeHide)
                .Subscribe(useCache => {
                    Preferences.Default.DiffSingerLangCodeHide = useCache;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.DiffSingerLocalRetaking)
                .Subscribe(value => {
                    Preferences.Default.DiffSingerLocalRetaking = value;
                    Preferences.Save();
                });
            this.WhenAnyValue(vm => vm.SkipRenderingMutedTracks)
                .Subscribe(skipRenderingMutedTracks => {
                    Preferences.Default.SkipRenderingMutedTracks = skipRenderingMutedTracks;
                    Preferences.Save();
                });
        }

        public void TestAudioOutputDevice() {
            try {
                PlaybackManager.Inst.PlayTestSound();
            } catch (Exception e) {
                Log.Error(e, "Failed to play test sound.");
                DocManager.Inst.ExecuteCmd(new ErrorMessageNotification("Failed to play test sound.", e));
            }
        }

        /// <summary>
        /// One track-palette slider change was already applied to the reactive
        /// property; persist and re-render the track palette cache.
        /// </summary>
        void OnTrackColorTuned(Action storeToPreferences) {
            storeToPreferences();
            Preferences.Save();
            StudioTrackPaintCache.Invalidate(StudioTrackPaletteChangeReason.Mode);
        }

        public void TestMetronome() {
            try {
                PlaybackManager.Inst.PlayMetronomeClick();
            } catch (Exception e) {
                Log.Error(e, "Failed to play metronome preview.");
                DocManager.Inst.ExecuteCmd(new ErrorMessageNotification("Failed to play metronome preview.", e));
            }
        }

        public void ResetMetronomeVolume() {
            MetronomeVolume = new Preferences.SerializablePreferences().MetronomeVolume;
        }

        public void ResetMetronomeHighFrequency() {
            MetronomeHighFrequency = new Preferences.SerializablePreferences().MetronomeHighFrequency;
        }

        public void ResetMetronomeLowFrequency() {
            MetronomeLowFrequency = new Preferences.SerializablePreferences().MetronomeLowFrequency;
        }

        void InitPresets() {
            StudioPresetManager.EnsureDefaults();
            CurrentPreset = string.IsNullOrEmpty(Preferences.Default.StudioPreset)
                ? StudioThemeGenerator.StudioDark
                : Preferences.Default.StudioPreset;
            var preset = StudioPresetManager.Load(CurrentPreset);
            if (preset == null) {
                CurrentPreset = StudioThemeGenerator.StudioDark;
                Preferences.Default.StudioPreset = CurrentPreset;
                preset = StudioPresetManager.GetBuiltIn(CurrentPreset);
            }
            PresetDirty = !PresetMatches(preset);
            RefreshPresetItems();
        }

        void RefreshPresetItems() {
            var names = StudioPresetManager.LoadAll().Select(p => p.Name).ToList();
            var items = names
                .Select(name => name == CurrentPreset && PresetDirty ? name + " *" : name)
                .ToList();
            // Only replace the list when its content actually changed — replacing
            // the ItemsSource instance on every refresh drops the ComboBox
            // selection text even though the selection itself is unchanged.
            if (!PresetItems.SequenceEqual(items)) {
                PresetItems = items;
                this.RaisePropertyChanged(nameof(PresetItems));
            }
            string display = CurrentPreset + (PresetDirty ? " *" : "");
            if (SelectedPresetItem != display) {
                SelectedPresetItem = display;
            }
        }

        void MarkPresetDirty() {
            if (_applyingPreset || PresetDirty) {
                return;
            }
            PresetDirty = true;
            RefreshPresetItems();
        }

        void OnPresetPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName != null && PresetUiPropertyNames.Contains(e.PropertyName)) {
                MarkPresetDirty();
            }
        }

        void ApplyPreset(string name) {
            var preset = StudioPresetManager.Load(name)
                         ?? StudioPresetManager.GetBuiltIn(name);
            _applyingPreset = true;
            try {
                ApplyPresetUi(preset.Ui);
                Preferences.Default.StudioPreset = name;
                CurrentPreset = name;
                Preferences.Save();
                App.SetTheme();
                StudioUI.NotifyChanged();
            } finally {
                _applyingPreset = false;
            }
            PresetDirty = false;
            RefreshPresetItems();
        }

        void ApplyPresetUi(StudioPresetUi ui) {
            static void ApplyInt(int? value, Action<int> set) {
                if (value.HasValue) {
                    set(value.Value);
                }
            }
            static void ApplyDouble(double? value, Action<double> set) {
                if (value.HasValue) {
                    set(value.Value);
                }
            }
            static void ApplyBool(bool? value, Action<bool> set) {
                if (value.HasValue) {
                    set(value.Value);
                }
            }
            static void ApplyString(string? value, Action<string> set) {
                if (!string.IsNullOrEmpty(value)) {
                    set(value);
                }
            }
            ApplyInt(ui.TrackColorMode, v => StudioTrackColorMode = v);
            if (ui.TrackColorConfig != null) {
                var c = ui.TrackColorConfig;
                if (c.RainbowStartHue.HasValue) Preferences.Default.RainbowStartHue = c.RainbowStartHue.Value;
                if (c.RainbowHueSpan.HasValue) Preferences.Default.RainbowHueSpan = c.RainbowHueSpan.Value;
                if (c.RainbowCycleTracks.HasValue) Preferences.Default.RainbowCycleTracks = c.RainbowCycleTracks.Value;
                if (c.RainbowReverse.HasValue) Preferences.Default.RainbowReverse = c.RainbowReverse.Value;
                if (c.RainbowLumaAmpDark.HasValue) Preferences.Default.RainbowLumaAmpDark = c.RainbowLumaAmpDark.Value;
                if (c.RainbowLumaAmpLight.HasValue) Preferences.Default.RainbowLumaAmpLight = c.RainbowLumaAmpLight.Value;
                if (c.RainbowLumaWaveDivisor.HasValue) Preferences.Default.RainbowLumaWaveDivisor = c.RainbowLumaWaveDivisor.Value;
                if (c.GradientBaseHueSpan.HasValue) Preferences.Default.GradientBaseHueSpan = c.GradientBaseHueSpan.Value;
                if (c.GradientHueSpanPerTrack.HasValue) Preferences.Default.GradientHueSpanPerTrack = c.GradientHueSpanPerTrack.Value;
                if (c.GradientMaxHueSpan.HasValue) Preferences.Default.GradientMaxHueSpan = c.GradientMaxHueSpan.Value;
                if (c.GradientLowChromaSeed.HasValue) Preferences.Default.GradientLowChromaSeed = c.GradientLowChromaSeed.Value;
                if (c.GradientNeighborHueMin.HasValue) Preferences.Default.GradientNeighborHueMin = c.GradientNeighborHueMin.Value;
                if (c.GradientNeighborHuePush.HasValue) Preferences.Default.GradientNeighborHuePush = c.GradientNeighborHuePush.Value;
                if (c.GradientLowChromaFallbackS.HasValue) Preferences.Default.GradientLowChromaFallbackS = c.GradientLowChromaFallbackS.Value;
                if (c.GradientLowChromaSatScale.HasValue) Preferences.Default.GradientLowChromaSatScale = c.GradientLowChromaSatScale.Value;
                if (c.GradientDarkLumaAmp.HasValue) Preferences.Default.GradientDarkLumaAmp = c.GradientDarkLumaAmp.Value;
                if (c.GradientLightLumaAmp.HasValue) Preferences.Default.GradientLightLumaAmp = c.GradientLightLumaAmp.Value;
                if (c.GradientLumaWaveDivisor.HasValue) Preferences.Default.GradientLumaWaveDivisor = c.GradientLumaWaveDivisor.Value;
                SyncTrackColorToView();
            }
            ApplyInt(ui.WaveformStyle, v => WaveformStyle = v);
            ApplyInt(ui.WaveformLayout, v => WaveformLayout = v);
            ApplyInt(ui.WaveformFollowMode, v => WaveformFollowMode = v);
            ApplyInt(ui.WaveformFadeInMs, v => WaveformFadeInMs = v);
            ApplyInt(ui.WaveformFadeOutMs, v => WaveformFadeOutMs = v);
            ApplyInt(ui.WaveformColorMode, v => WaveformColorMode = v);
            ApplyString(ui.WaveformColorHex, v => WaveformColorHex = v);
            ApplyBool(ui.WaveformColorInvert, v => WaveformColorInvert = v);
            ApplyInt(ui.WaveformScalePercent, v => WaveformScalePercent = v);
            ApplyInt(ui.WaveformFixedBottomPx, v => WaveformFixedBottomPx = v);
            ApplyDouble(ui.WaveformAmpScaleRows, v => WaveformAmpScaleRows = v);
            ApplyDouble(ui.WaveformFollowOffsetRows, v => WaveformFollowOffsetRows = v);
            ApplyDouble(ui.WaveformSmartSplitDown, v => WaveformSmartSplitDown = v);
            ApplyDouble(ui.WaveformSmartSplitUp, v => WaveformSmartSplitUp = v);
            ApplyDouble(ui.WaveformSmartReturnSlop, v => WaveformSmartReturnSlop = v);
            ApplyInt(ui.WaveformSmartLookAhead, v => WaveformSmartLookAhead = v);
            ApplyDouble(ui.WaveformFallbackLeadMs, v => WaveformFallbackLeadMs = v);
            ApplyInt(ui.WaveformAlphaPercent, v => WaveformAlphaPercent = v);
            ApplyInt(ui.NoteStrokeColorMode, v => NoteStrokeColorMode = v);
            ApplyString(ui.NoteStrokeColorHex, v => NoteStrokeColorHex = v);
            ApplyBool(ui.NoteStrokeColorInvert, v => NoteStrokeColorInvert = v);
            ApplyInt(ui.NoteStrokeThickness, v => NoteStrokeThickness = v);
            ApplyInt(ui.PitchPredictionColorMode, v => PitchPredictionColorMode = v);
            ApplyString(ui.PitchPredictionColorHex, v => PitchPredictionColorHex = v);
            ApplyBool(ui.PitchPredictionColorInvert, v => PitchPredictionColorInvert = v);
            ApplyInt(ui.PitchPredictionThickness, v => PitchPredictionThickness = v);
            ApplyBool(ui.NoteRoundedCorners, v => NoteRoundedCorners = v);
            ApplyInt(ui.NoteCornerRadiusPx, v => NoteCornerRadiusPx = v);
            ApplyBool(ui.NoteSolidFill, v => NoteSolidFill = v);
            ApplyInt(ui.NoteLyricVAlign, v => NoteLyricVAlign = v);
            ApplyInt(ui.NoteLyricHAlign, v => NoteLyricHAlign = v);
            ApplyString(ui.NoteLyricFontFamily, v => NoteLyricFontFamily = v);
            ApplyInt(ui.NoteLyricScalePercent, v => NoteLyricScalePercent = v);
            ApplyInt(ui.NoteLyricWeight, v => NoteLyricWeight = v);
            ApplyBool(ui.NoteLyricItalic, v => NoteLyricItalic = v);
            ApplyInt(ui.NoteLyricColorMode, v => NoteLyricColorMode = v);
            ApplyString(ui.NoteLyricColorHex, v => NoteLyricColorHex = v);
            ApplyBool(ui.NoteLyricColorInvert, v => NoteLyricColorInvert = v);
            ApplyInt(ui.NoteLyricPaddingPx, v => NoteLyricPaddingPx = v);
            ApplyBool(ui.NoteLyricShrinkToFit, v => NoteLyricShrinkToFit = v);
        }

        bool PresetMatches(StudioPreset preset) {
            var ui = preset.Ui;
            return Match(ui.TrackColorMode, Preferences.Default.StudioTrackColorMode)
                && ConfigMatches(ui.TrackColorConfig)
                && Match(ui.WaveformStyle, Preferences.Default.WaveformStyle)
                && Match(ui.WaveformLayout, Preferences.Default.WaveformLayout)
                && Match(ui.WaveformFollowMode, Preferences.Default.WaveformFollowMode)
                && Match(ui.WaveformFadeInMs, Preferences.Default.WaveformFadeInMs)
                && Match(ui.WaveformFadeOutMs, Preferences.Default.WaveformFadeOutMs)
                && Match(ui.WaveformColorMode, Preferences.Default.WaveformColorMode)
                && Match(ui.WaveformColorHex, Preferences.Default.WaveformColorHex)
                && Match(ui.WaveformColorInvert, Preferences.Default.WaveformColorInvert)
                && Match(ui.WaveformScalePercent, Preferences.Default.WaveformScalePercent)
                && Match(ui.WaveformFixedBottomPx, Preferences.Default.WaveformFixedBottomPx)
                && Match(ui.WaveformAmpScaleRows, Preferences.Default.WaveformAmpScaleRows)
                && Match(ui.WaveformFollowOffsetRows, Preferences.Default.WaveformFollowOffsetRows)
                && Match(ui.WaveformSmartSplitDown, Preferences.Default.WaveformSmartSplitDown)
                && Match(ui.WaveformSmartSplitUp, Preferences.Default.WaveformSmartSplitUp)
                && Match(ui.WaveformSmartReturnSlop, Preferences.Default.WaveformSmartReturnSlop)
                && Match(ui.WaveformSmartLookAhead, Preferences.Default.WaveformSmartLookAhead)
                && Match(ui.WaveformFallbackLeadMs, Preferences.Default.WaveformFallbackLeadMs)
                && Match(ui.WaveformAlphaPercent, Preferences.Default.WaveformAlphaPercent)
                && Match(ui.NoteStrokeColorMode, Preferences.Default.NoteStrokeColorMode)
                && Match(ui.NoteStrokeColorHex, Preferences.Default.NoteStrokeColorHex)
                && Match(ui.NoteStrokeColorInvert, Preferences.Default.NoteStrokeColorInvert)
                && Match(ui.NoteStrokeThickness, Preferences.Default.NoteStrokeThickness)
                && Match(ui.PitchPredictionColorMode, Preferences.Default.PitchPredictionColorMode)
                && Match(ui.PitchPredictionColorHex, Preferences.Default.PitchPredictionColorHex)
                && Match(ui.PitchPredictionColorInvert, Preferences.Default.PitchPredictionColorInvert)
                && Match(ui.PitchPredictionThickness, Preferences.Default.PitchPredictionThickness)
                && Match(ui.NoteRoundedCorners, Preferences.Default.NoteRoundedCorners)
                && Match(ui.NoteCornerRadiusPx, Preferences.Default.NoteCornerRadiusPx)
                && Match(ui.NoteSolidFill, Preferences.Default.NoteSolidFill)
                && Match(ui.NoteLyricVAlign, Preferences.Default.NoteLyricVAlign)
                && Match(ui.NoteLyricHAlign, Preferences.Default.NoteLyricHAlign)
                && Match(ui.NoteLyricFontFamily, Preferences.Default.NoteLyricFontFamily)
                && Match(ui.NoteLyricScalePercent, Preferences.Default.NoteLyricScalePercent)
                && Match(ui.NoteLyricWeight, Preferences.Default.NoteLyricWeight)
                && Match(ui.NoteLyricItalic, Preferences.Default.NoteLyricItalic)
                && Match(ui.NoteLyricColorMode, Preferences.Default.NoteLyricColorMode)
                && Match(ui.NoteLyricColorHex, Preferences.Default.NoteLyricColorHex)
                && Match(ui.NoteLyricColorInvert, Preferences.Default.NoteLyricColorInvert)
                && Match(ui.NoteLyricPaddingPx, Preferences.Default.NoteLyricPaddingPx)
                && Match(ui.NoteLyricShrinkToFit, Preferences.Default.NoteLyricShrinkToFit);
        }

        void SyncTrackColorToView() {
            RainbowStartHue = Preferences.Default.RainbowStartHue;
            RainbowHueSpan = Preferences.Default.RainbowHueSpan;
            RainbowCycleTracks = Preferences.Default.RainbowCycleTracks;
            RainbowReverse = Preferences.Default.RainbowReverse;
            RainbowLumaAmpDark = Preferences.Default.RainbowLumaAmpDark;
            RainbowLumaAmpLight = Preferences.Default.RainbowLumaAmpLight;
            RainbowLumaWaveDivisor = Preferences.Default.RainbowLumaWaveDivisor;
            GradientBaseHueSpan = Preferences.Default.GradientBaseHueSpan;
            GradientHueSpanPerTrack = Preferences.Default.GradientHueSpanPerTrack;
            GradientMaxHueSpan = Preferences.Default.GradientMaxHueSpan;
            GradientLowChromaSeed = Preferences.Default.GradientLowChromaSeed;
            GradientNeighborHueMin = Preferences.Default.GradientNeighborHueMin;
            GradientNeighborHuePush = Preferences.Default.GradientNeighborHuePush;
            GradientLowChromaFallbackS = Preferences.Default.GradientLowChromaFallbackS;
            GradientLowChromaSatScale = Preferences.Default.GradientLowChromaSatScale;
            GradientDarkLumaAmp = Preferences.Default.GradientDarkLumaAmp;
            GradientLightLumaAmp = Preferences.Default.GradientLightLumaAmp;
            GradientLumaWaveDivisor = Preferences.Default.GradientLumaWaveDivisor;
        }

        bool ConfigMatches(StudioTrackColorConfig? config) {
            if (config == null) {
                return true;
            }
            return Match(config.RainbowStartHue, Preferences.Default.RainbowStartHue)
                && Match(config.RainbowHueSpan, Preferences.Default.RainbowHueSpan)
                && Match(config.RainbowCycleTracks, Preferences.Default.RainbowCycleTracks)
                && Match(config.RainbowReverse, Preferences.Default.RainbowReverse)
                && Match(config.RainbowLumaAmpDark, Preferences.Default.RainbowLumaAmpDark)
                && Match(config.RainbowLumaAmpLight, Preferences.Default.RainbowLumaAmpLight)
                && Match(config.RainbowLumaWaveDivisor, Preferences.Default.RainbowLumaWaveDivisor)
                && Match(config.GradientBaseHueSpan, Preferences.Default.GradientBaseHueSpan)
                && Match(config.GradientHueSpanPerTrack, Preferences.Default.GradientHueSpanPerTrack)
                && Match(config.GradientMaxHueSpan, Preferences.Default.GradientMaxHueSpan)
                && Match(config.GradientLowChromaSeed, Preferences.Default.GradientLowChromaSeed)
                && Match(config.GradientNeighborHueMin, Preferences.Default.GradientNeighborHueMin)
                && Match(config.GradientNeighborHuePush, Preferences.Default.GradientNeighborHuePush)
                && Match(config.GradientLowChromaFallbackS, Preferences.Default.GradientLowChromaFallbackS)
                && Match(config.GradientLowChromaSatScale, Preferences.Default.GradientLowChromaSatScale)
                && Match(config.GradientDarkLumaAmp, Preferences.Default.GradientDarkLumaAmp)
                && Match(config.GradientLightLumaAmp, Preferences.Default.GradientLightLumaAmp)
                && Match(config.GradientLumaWaveDivisor, Preferences.Default.GradientLumaWaveDivisor);
        }

        /// <summary>
        /// Saves the current Studio UI appearance values under <paramref name="name"/>
        /// into <c>DataPath/Presets</c> as a YAML preset.
        /// </summary>
        public void SavePreset(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                return;
            }
            var current = StudioPresetManager.Load(CurrentPreset)
                          ?? StudioPresetManager.GetBuiltIn(CurrentPreset);
            var preset = new StudioPreset {
                Name = name,
                IsDark = current.IsDark,
                Palette = new Dictionary<string, string>(current.Palette),
                Ui = CaptureUi(),
            };
            StudioPresetManager.Save(preset);
            Preferences.Default.StudioPreset = name;
            CurrentPreset = name;
            PresetDirty = false;
            Preferences.Save();
            RefreshPresetItems();
        }

        StudioPresetUi CaptureUi() => new() {
            TrackColorMode = Preferences.Default.StudioTrackColorMode,
            TrackColorConfig = new StudioTrackColorConfig {
                RainbowStartHue = Preferences.Default.RainbowStartHue,
                RainbowHueSpan = Preferences.Default.RainbowHueSpan,
                RainbowCycleTracks = Preferences.Default.RainbowCycleTracks,
                RainbowReverse = Preferences.Default.RainbowReverse,
                RainbowLumaAmpDark = Preferences.Default.RainbowLumaAmpDark,
                RainbowLumaAmpLight = Preferences.Default.RainbowLumaAmpLight,
                RainbowLumaWaveDivisor = Preferences.Default.RainbowLumaWaveDivisor,
                GradientBaseHueSpan = Preferences.Default.GradientBaseHueSpan,
                GradientHueSpanPerTrack = Preferences.Default.GradientHueSpanPerTrack,
                GradientMaxHueSpan = Preferences.Default.GradientMaxHueSpan,
                GradientLowChromaSeed = Preferences.Default.GradientLowChromaSeed,
                GradientNeighborHueMin = Preferences.Default.GradientNeighborHueMin,
                GradientNeighborHuePush = Preferences.Default.GradientNeighborHuePush,
                GradientLowChromaFallbackS = Preferences.Default.GradientLowChromaFallbackS,
                GradientLowChromaSatScale = Preferences.Default.GradientLowChromaSatScale,
                GradientDarkLumaAmp = Preferences.Default.GradientDarkLumaAmp,
                GradientLightLumaAmp = Preferences.Default.GradientLightLumaAmp,
                GradientLumaWaveDivisor = Preferences.Default.GradientLumaWaveDivisor,
            },
            WaveformStyle = Preferences.Default.WaveformStyle,
            WaveformLayout = Preferences.Default.WaveformLayout,
            WaveformFollowMode = Preferences.Default.WaveformFollowMode,
            WaveformFadeInMs = Preferences.Default.WaveformFadeInMs,
            WaveformFadeOutMs = Preferences.Default.WaveformFadeOutMs,
            WaveformColorMode = Preferences.Default.WaveformColorMode,
            WaveformColorHex = Preferences.Default.WaveformColorHex,
            WaveformColorInvert = Preferences.Default.WaveformColorInvert,
            WaveformScalePercent = Preferences.Default.WaveformScalePercent,
            WaveformFixedBottomPx = Preferences.Default.WaveformFixedBottomPx,
            WaveformAmpScaleRows = Preferences.Default.WaveformAmpScaleRows,
            WaveformFollowOffsetRows = Preferences.Default.WaveformFollowOffsetRows,
            WaveformSmartSplitDown = Preferences.Default.WaveformSmartSplitDown,
            WaveformSmartSplitUp = Preferences.Default.WaveformSmartSplitUp,
            WaveformSmartReturnSlop = Preferences.Default.WaveformSmartReturnSlop,
            WaveformSmartLookAhead = Preferences.Default.WaveformSmartLookAhead,
            WaveformFallbackLeadMs = Preferences.Default.WaveformFallbackLeadMs,
            WaveformAlphaPercent = Preferences.Default.WaveformAlphaPercent,
            NoteStrokeColorMode = Preferences.Default.NoteStrokeColorMode,
            NoteStrokeColorHex = Preferences.Default.NoteStrokeColorHex,
            NoteStrokeColorInvert = Preferences.Default.NoteStrokeColorInvert,
            NoteStrokeThickness = Preferences.Default.NoteStrokeThickness,
            PitchPredictionColorMode = Preferences.Default.PitchPredictionColorMode,
            PitchPredictionColorHex = Preferences.Default.PitchPredictionColorHex,
            PitchPredictionColorInvert = Preferences.Default.PitchPredictionColorInvert,
            PitchPredictionThickness = Preferences.Default.PitchPredictionThickness,
            NoteRoundedCorners = Preferences.Default.NoteRoundedCorners,
            NoteCornerRadiusPx = Preferences.Default.NoteCornerRadiusPx,
            NoteSolidFill = Preferences.Default.NoteSolidFill,
            NoteLyricVAlign = Preferences.Default.NoteLyricVAlign,
            NoteLyricHAlign = Preferences.Default.NoteLyricHAlign,
            NoteLyricFontFamily = Preferences.Default.NoteLyricFontFamily,
            NoteLyricScalePercent = Preferences.Default.NoteLyricScalePercent,
            NoteLyricWeight = Preferences.Default.NoteLyricWeight,
            NoteLyricItalic = Preferences.Default.NoteLyricItalic,
            NoteLyricColorMode = Preferences.Default.NoteLyricColorMode,
            NoteLyricColorHex = Preferences.Default.NoteLyricColorHex,
            NoteLyricColorInvert = Preferences.Default.NoteLyricColorInvert,
            NoteLyricPaddingPx = Preferences.Default.NoteLyricPaddingPx,
            NoteLyricShrinkToFit = Preferences.Default.NoteLyricShrinkToFit,
        };

        static bool Match(int? a, int b) => !a.HasValue || a.Value == b;
        static bool Match(bool? a, bool b) => !a.HasValue || a.Value == b;
        static bool Match(string? a, string b) => string.IsNullOrEmpty(a) || a == b;
        static bool Match(double? a, double b) => !a.HasValue || Math.Abs(a.Value - b) < 1e-9;

        public void OpenResamplerLocation() {
            try {
                string path = PathManager.Inst.ResamplersPath;
                Directory.CreateDirectory(path);
                OS.OpenFolder(path);
            } catch (Exception e) {
                DocManager.Inst.ExecuteCmd(new ErrorMessageNotification(e));
            }
        }

        public void SetAddlSingersPath(string path) {
            Preferences.Default.AdditionalSingerPath = path;
            Preferences.Save();
            this.RaisePropertyChanged(nameof(AdditionalSingersPath));
        }

        public void SetVLabelerPath(string path) {
            Preferences.Default.VLabelerPath = path;
            Preferences.Save();
            this.RaisePropertyChanged(nameof(VLabelerPath));
        }

        public void SetSetParamPath(string path) {
            Preferences.Default.SetParamPath = path;
            Preferences.Save();
            this.RaisePropertyChanged(nameof(SetParamPath));
        }

        public void SetWinePath(string path) {
            Preferences.Default.WinePath = path;
            Preferences.Save();
            ToolsManager.Inst.Initialize();
            this.RaisePropertyChanged(nameof(WinePath));
        }

        public void RefreshThemes() {
            Colors.CustomTheme.ListThemes();
            _ = OudepLoaderRegistry.LoadAllAsync();
            this.RaisePropertyChanged(nameof(ThemeItems));
        }

        public void ToggleOnnxGpuDisplay(bool show) {
            ShowOnnxGpu = show;
        }

        public void SetHifiUtauSplicerPath(string path) {
            Preferences.Default.HifiUtauSplicerPath = path ?? string.Empty;
            Preferences.Save();
            HifiUtauSplicerPath = Preferences.Default.HifiUtauSplicerPath;
            RefreshHifiUtauStatus();
        }

        public void SetHifiUtauHnsepPath(string path) {
            Preferences.Default.HifiUtauHnsepPath = path ?? string.Empty;
            Preferences.Save();
            HifiUtauHnsepPath = Preferences.Default.HifiUtauHnsepPath;
            RefreshHifiUtauStatus();
        }

        public void ResetHifiUtauPaths() {
            Preferences.Default.HifiUtauSplicerPath = string.Empty;
            Preferences.Default.HifiUtauHnsepPath = string.Empty;
            Preferences.Save();
            HifiUtauSplicerPath = string.Empty;
            HifiUtauHnsepPath = string.Empty;
            RefreshHifiUtauStatus();
        }

        public void RefreshHifiUtauStatus() {
            var store = HiFiUtauModelStore.Inst;
            string mode = Preferences.Default.HifiUtauEmbedded
                ? ThemeManager.GetString("prefs.hifiutau.status.mode.embedded")
                : ThemeManager.GetString("prefs.hifiutau.status.mode.disabled");
            string splicer = store.SplicerReady
                ? ThemeManager.GetString("prefs.hifiutau.status.ready")
                : (store.SplicerError ?? ThemeManager.GetString("prefs.hifiutau.status.notloaded"));
            string hnsep = store.HnsepReady
                ? ThemeManager.GetString("prefs.hifiutau.status.ready")
                : (store.HnsepError ?? ThemeManager.GetString("prefs.hifiutau.status.notloaded"));
            HifiUtauStatus = string.Format(ThemeManager.GetString("prefs.hifiutau.status.format"),
                mode,
                store.SplicerDir ?? ThemeManager.GetString("prefs.hifiutau.status.notfound"),
                splicer,
                store.HnsepDir ?? ThemeManager.GetString("prefs.hifiutau.status.notfound"),
                hnsep);
        }
    }
}
