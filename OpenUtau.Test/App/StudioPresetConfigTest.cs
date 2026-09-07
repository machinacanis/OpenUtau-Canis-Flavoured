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

            var yaml = Core.Yaml.DefaultSerializer.Serialize(preset);
            var back = Core.Yaml.DefaultDeserializer.Deserialize<StudioPreset>(yaml);
            Assert.NotNull(back);
            Assert.Equal(StudioTrackColorParams.RainbowStartHue,
                back!.Ui.TrackColorConfig!.RainbowStartHueOrDefault);
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
