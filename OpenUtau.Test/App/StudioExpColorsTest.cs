using System;
using Avalonia.Media;
using OpenUtau.App.Studio;
using Xunit;

namespace OpenUtau.App {
    public class StudioExpColorsTest {
        static StudioTrackPaletteContext Dark(StudioTrackColorMode mode) => new(
            mode,
            Color.FromRgb(0x30, 0x30, 0x30),
            Color.FromRgb(0x4E, 0xA6, 0xEA),
            Color.FromRgb(0xFF, 0x67, 0x9D),
            IsDark: true);

        [Fact]
        public void ResolveSwatches_Rainbow_AssignsDistinctOrderedHues() {
            var swatches = StudioExpColors.ResolveSwatches(4, Dark(StudioTrackColorMode.Rainbow));
            Assert.Equal(4, swatches.Length);
            var again = StudioExpColors.ResolveSwatches(4, Dark(StudioTrackColorMode.Rainbow));
            Assert.Equal(swatches[0].HeaderAccent, again[0].HeaderAccent);
            for (int i = 0; i < swatches.Length; i++) {
                for (int j = i + 1; j < swatches.Length; j++) {
                    var (hi, _, _) = StudioColorMath.RgbToHsl(swatches[i].HeaderAccent);
                    var (hj, _, _) = StudioColorMath.RgbToHsl(swatches[j].HeaderAccent);
                    double delta = Math.Abs(hi - hj);
                    delta = Math.Min(delta, 360 - delta);
                    Assert.True(delta > 8, $"index {i} and {j} hues too close ({hi:0.0}, {hj:0.0})");
                }
            }
        }

        [Fact]
        public void ResolveSwatches_Fixed_AssignsDistinctColorsInOrder() {
            var swatches = StudioExpColors.ResolveSwatches(3, Dark(StudioTrackColorMode.Fixed));
            var again = StudioExpColors.ResolveSwatches(3, Dark(StudioTrackColorMode.Fixed));
            Assert.Equal(swatches[0].HeaderAccent, again[0].HeaderAccent);
            Assert.Equal(swatches[1].HeaderAccent, again[1].HeaderAccent);
            Assert.NotEqual(swatches[0].HeaderAccent, swatches[1].HeaderAccent);
            Assert.NotEqual(swatches[1].HeaderAccent, swatches[2].HeaderAccent);
        }
    }
}
