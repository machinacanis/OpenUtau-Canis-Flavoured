using System.Linq;
using OpenUtau.App.Studio;
using Xunit;

namespace OpenUtau.App {
    public class StudioPresetConfigTest {
        [Fact]
        public void DefaultConfig_RoundTripsThroughYaml() {
            var preset = StudioPresetManager.GetBuiltIn(StudioThemeGenerator.StudioDark);
            Assert.Equal(0, preset.Ui.TrackColorMode);
            Assert.NotNull(preset.Ui.TrackColorConfig);
            Assert.Equal(StudioTrackColorParams.RainbowStartHue,
                preset.Ui.TrackColorConfig!.RainbowStartHueOrDefault);
            Assert.False(preset.Ui.TrackColorConfig.RainbowReverseOrDefault);
            Assert.Equal(StudioTrackColorParams.RainbowLumaAmpDark,
                preset.Ui.TrackColorConfig.RainbowLumaAmpDarkOrDefault);
            Assert.Equal(1.5, preset.Ui.WaveformAmpScaleRows);
            Assert.Equal(69, preset.Ui.WaveformAlphaPercent);
            Assert.Equal(5, preset.Ui.NoteLyricPaddingPx);
            Assert.False(preset.Ui.NoteLyricShrinkToFit);

            var yaml = Core.Yaml.DefaultSerializer.Serialize(preset);
            var back = Core.Yaml.DefaultDeserializer.Deserialize<StudioPreset>(yaml);
            Assert.NotNull(back);
            Assert.Equal(StudioTrackColorParams.RainbowStartHue,
                back!.Ui.TrackColorConfig!.RainbowStartHueOrDefault);
            Assert.False(back.Ui.TrackColorConfig.RainbowReverseOrDefault);
            Assert.Equal(StudioTrackColorParams.RainbowLumaAmpLight,
                back.Ui.TrackColorConfig.RainbowLumaAmpLightOrDefault);
            Assert.Equal(1.5, back.Ui.WaveformAmpScaleRows);
            Assert.Equal(69, back.Ui.WaveformAlphaPercent);
            Assert.Equal(5, back.Ui.NoteLyricPaddingPx);
            Assert.False(back.Ui.NoteLyricShrinkToFit);
            Assert.Equal(StudioTrackColorParams.GradientNeighborHueMin,
                back.Ui.TrackColorConfig.GradientNeighborHueMinOrDefault);
            Assert.Equal(StudioTrackColorParams.GradientLightLumaAmp,
                back.Ui.TrackColorConfig.GradientLightLumaAmpOrDefault);
        }

        [Fact]
        public void CustomConfig_RoundTripsThroughYaml() {
            var cfg = StudioTrackColorConfig.FromCurrent();
            cfg.RainbowStartHue = 210;
            cfg.RainbowHueSpan = 90;
            cfg.RainbowReverse = true;
            cfg.RainbowLumaAmpDark = 0.12;
            cfg.RainbowLumaWaveDivisor = 3.2;
            cfg.GradientBaseHueSpan = 30;
            cfg.GradientLumaWaveDivisor = 3;
            var preset = StudioPresetManager.GetBuiltIn(StudioThemeGenerator.TokyoDay);
            preset.Ui.TrackColorMode = 2;
            preset.Ui.TrackColorConfig = cfg;

            var yaml = Core.Yaml.DefaultSerializer.Serialize(preset);
            var back = Core.Yaml.DefaultDeserializer.Deserialize<StudioPreset>(yaml);
            Assert.Equal(2, back!.Ui.TrackColorMode);
            Assert.Equal(210, back.Ui.TrackColorConfig!.RainbowStartHueOrDefault);
            Assert.Equal(90, back.Ui.TrackColorConfig.RainbowHueSpanOrDefault);
            Assert.True(back.Ui.TrackColorConfig.RainbowReverseOrDefault);
            Assert.Equal(0.12, back.Ui.TrackColorConfig.RainbowLumaAmpDarkOrDefault);
            Assert.Equal(3.2, back.Ui.TrackColorConfig.RainbowLumaWaveDivisorOrDefault);
            Assert.Equal(30, back.Ui.TrackColorConfig.GradientBaseHueSpanOrDefault);
            Assert.Equal(3, back.Ui.TrackColorConfig.GradientLumaWaveDivisorOrDefault);
        }

        [Fact]
        public void MinimalPreset_OmittedConfig_FallsBackToDefaults() {
            // Preset YAML uses snake_case (OpenUtau Yaml helper); the mode is
            // present, config is omitted -> algorithm falls back to defaults.
            const string yaml = """
                name: Minimal
                version: 1
                is_dark: true
                palette: {}
                ui:
                  track_color_mode: 1
                """;
            var preset = Core.Yaml.DefaultDeserializer.Deserialize<StudioPreset>(yaml);
            Assert.NotNull(preset);
            Assert.Equal(1, preset!.Ui.TrackColorMode);
            Assert.Null(preset.Ui.TrackColorConfig);
            // Algorithm-side fallback still yields the legacy rainbow numbers.
            var cfg = preset.Ui.TrackColorConfig ?? StudioTrackColorConfig.Default();
            Assert.Equal(StudioTrackColorParams.RainbowStartHue, cfg.RainbowStartHueOrDefault);
            Assert.Equal(StudioTrackColorParams.RainbowHueSpan, cfg.RainbowHueSpanOrDefault);
        }
    }
}
