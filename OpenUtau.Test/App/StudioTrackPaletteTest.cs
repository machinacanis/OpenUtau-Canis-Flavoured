using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using OpenUtau.App.Studio;
using OpenUtau.Core.Ustx;
using Xunit;

namespace OpenUtau.App {
    public class StudioTrackPaletteTest {
        static readonly Color OnWhite = Color.FromRgb(0xF5, 0xF5, 0xF5);
        static readonly Color OnBlack = Color.FromRgb(0x1A, 0x1A, 0x1A);

        [Fact]
        public void Rainbow_FourTracks_StudioDark_HuesDecreaseThroughMagenta() {
            var ctx = StudioDark(StudioTrackColorMode.Rainbow);
            var swatches = StudioTrackPalette.ResolveAll(Tracks(4), ctx);
            Assert.Equal(4, swatches.Length);
            double[] expected = [8, 285, 202, 118];
            for (int i = 0; i < 4; i++) {
                var (h, s, l) = StudioColorMath.RgbToHsl(swatches[i].Fill);
                Assert.True(HueDelta(h, expected[i]) <= 8,
                    $"track {i}: hue {h:0.0} expected ~{expected[i]}");
                Assert.InRange(s, 0.55, 0.78);
                Assert.InRange(l, 0.45, 0.70);
            }
        }

        [Fact]
        public void Rainbow_StretchesWhenNLeq12_WrapsWhenNGt12() {
            var ctx = StudioDark(StudioTrackColorMode.Rainbow);
            var n12 = StudioTrackPalette.ResolveAll(Tracks(12), ctx);
            var (hFirst, _, _) = StudioColorMath.RgbToHsl(n12[0].Fill);
            var (hLast, _, _) = StudioColorMath.RgbToHsl(n12[11].Fill);
            Assert.True(HueDelta(hFirst, hLast) >= 90,
                $"N=12 first-last ΔH={HueDelta(hFirst, hLast):0.0} expected large");

            var n13 = StudioTrackPalette.ResolveAll(Tracks(13), ctx);
            var (h0, _, _) = StudioColorMath.RgbToHsl(n13[0].Fill);
            var (h12, _, _) = StudioColorMath.RgbToHsl(n13[12].Fill);
            Assert.True(HueDelta(h0, h12) <= 8,
                $"N=13 index 0 hue {h0:0.0} vs 12 {h12:0.0} expected wrap");
        }

        [Fact]
        public void Rainbow_SkipsMuddyYellow() {
            var ctx = StudioDark(StudioTrackColorMode.Rainbow);
            foreach (int n in new[] { 1, 4, 12, 24 }) {
                var swatches = StudioTrackPalette.ResolveAll(Tracks(n), ctx);
                for (int i = 0; i < n; i++) {
                    var (h, _, _) = StudioColorMath.RgbToHsl(swatches[i].Fill);
                    Assert.False(h >= 40 && h <= 90,
                        $"N={n} i={i} hue {h:0.0} entered muddy yellow [40,90]");
                    if (n == 1) {
                        Assert.True(HueDelta(h, 8) <= 8,
                            $"N=1 rainbow hue {h:0.0} expected ~8, not theme accent");
                        Assert.NotEqual(ctx.Accent1, swatches[i].Fill);
                    }
                }
            }
        }

        [Fact]
        public void Rainbow_TokyoDay_IsPastelNotMuddy() {
            var ctx = TokyoDay(StudioTrackColorMode.Rainbow);
            var swatches = StudioTrackPalette.ResolveAll(Tracks(5), ctx);
            Assert.Equal(5, swatches.Length);
            for (int i = 0; i < swatches.Length; i++) {
                var (h, s, l) = StudioColorMath.RgbToHsl(swatches[i].Fill);
                double y = StudioColorMath.RelativeLuminance(swatches[i].Fill);
                Assert.InRange(s, 0.24, 0.50);
                Assert.InRange(l, 0.62, 0.86);
                Assert.True(y >= 0.44,
                    $"track {i} hue {h:0.0}: Y={y:0.00} expected pastel, not brick/navy");
                Assert.Equal(OnBlack, swatches[i].OnFill);
            }
        }

        [Fact]
        public void ThemeGradient_TokyoDay_StaysLight() {
            var ctx = TokyoDay(StudioTrackColorMode.ThemeGradient);
            var swatches = StudioTrackPalette.ResolveAll(Tracks(5), ctx);
            var (hSeed, _, _) = StudioColorMath.RgbToHsl(ctx.Accent1);
            double[] ys = new double[swatches.Length];
            for (int i = 0; i < swatches.Length; i++) {
                var (h, s, l) = StudioColorMath.RgbToHsl(swatches[i].Fill);
                ys[i] = StudioColorMath.RelativeLuminance(swatches[i].Fill);
                Assert.True(HueDelta(h, hSeed) <= 52,
                    $"track {i}: |H-Hseed|={HueDelta(h, hSeed):0.0} > 52");
                Assert.InRange(s, 0.22, 0.55);
                Assert.InRange(l, 0.58, 0.90);
                Assert.True(ys[i] >= 0.40,
                    $"track {i} hue {h:0.0}: Y={ys[i]:0.00} expected light paper, not ink");
            }
            Assert.True(ys.Max() - ys.Min() <= 0.08,
                $"Tokyo Day theme ΔY={ys.Max() - ys.Min():0.00} expected even paper");
        }

        [Fact]
        public void ThemeGradient_StudioDark_FamilyAroundCyan() {
            var ctx = StudioDark(StudioTrackColorMode.ThemeGradient);
            var swatches = StudioTrackPalette.ResolveAll(Tracks(4), ctx);
            var (hSeed, _, _) = StudioColorMath.RgbToHsl(ctx.Accent1);
            double[] ys = new double[swatches.Length];
            double[] hs = new double[swatches.Length];
            for (int i = 0; i < swatches.Length; i++) {
                var (h, _, _) = StudioColorMath.RgbToHsl(swatches[i].Fill);
                hs[i] = h;
                ys[i] = StudioColorMath.RelativeLuminance(swatches[i].Fill);
                Assert.True(HueDelta(h, hSeed) <= 52,
                    $"track {i}: |H-Hseed|={HueDelta(h, hSeed):0.0} > 52 (complementary fan?)");
                var expected = StudioTrackPalette.DeriveSurfaces(
                    swatches[i].Fill, ctx, swatches[i].Fill);
                Assert.Equal(expected.Waveform, swatches[i].Waveform);
                Assert.Equal(expected.FillSelected, swatches[i].FillSelected);
            }
            Assert.True(ys.Max() - ys.Min() <= 0.08,
                $"Studio Dark theme ΔY={ys.Max() - ys.Min():0.00} expected even neon");
            for (int i = 0; i < swatches.Length - 1; i++) {
                Assert.True(HueDelta(hs[i], hs[i + 1]) >= 6,
                    $"adjacent {i}/{i + 1} ΔH={HueDelta(hs[i], hs[i + 1]):0.0}");
            }
        }

        [Fact]
        public void ThemeGradient_SixTracks_EvenY() {
            foreach (var ctx in new[] {
                StudioDark(StudioTrackColorMode.ThemeGradient),
                TokyoDay(StudioTrackColorMode.ThemeGradient),
            }) {
                var swatches = StudioTrackPalette.ResolveAll(Tracks(6), ctx);
                double[] ys = new double[6];
                for (int i = 0; i < 6; i++) {
                    ys[i] = StudioColorMath.RelativeLuminance(swatches[i].Fill);
                }
                Assert.True(ys.Max() - ys.Min() <= 0.08,
                    $"{(ctx.IsDark ? "dark" : "light")} n=6 ΔY={ys.Max() - ys.Min():0.00}");
            }
        }

        [Fact]
        public void ThemeGradient_LowChroma_FallsBackToLightnessRamp() {
            var gray = Color.FromRgb(0x88, 0x88, 0x88);
            var ctx = new StudioTrackPaletteContext(
                StudioTrackColorMode.ThemeGradient,
                Color.FromRgb(0x14, 0x16, 0x19),
                gray,
                gray,
                IsDark: true);
            var swatches = StudioTrackPalette.ResolveAll(Tracks(4), ctx);
            bool ok = false;
            for (int i = 0; i < swatches.Length; i++) {
                var (_, s, l) = StudioColorMath.RgbToHsl(swatches[i].Fill);
                if (s >= 0.20) {
                    ok = true;
                }
                if (i < swatches.Length - 1) {
                    var (_, _, l2) = StudioColorMath.RgbToHsl(swatches[i + 1].Fill);
                    if (Math.Abs(l - l2) >= 0.08) {
                        ok = true;
                    }
                }
            }
            Assert.True(ok, "low-chroma theme gradient should boost S or adjacent ΔL");
        }

        [Fact]
        public void DeriveSurfaces_OnFill_PicksBetterOfWhiteBlack() {
            var ctx = StudioDark(StudioTrackColorMode.Fixed);
            Color[] samples = [
                Color.FromRgb(0xFB, 0xC0, 0x2D), // yellow
                Color.FromRgb(0x00, 0xB8, 0xFF), // cyan
                Color.FromRgb(0xEF, 0x53, 0x50), // red
                Color.FromRgb(0x4A, 0x14, 0x8C), // dark purple
            ];
            foreach (var fill in samples) {
                var swatch = StudioTrackPalette.DeriveSurfaces(fill, ctx, fill);
                double cW = StudioColorMath.ContrastRatio(OnWhite, fill);
                double cB = StudioColorMath.ContrastRatio(OnBlack, fill);
                Color winner = cW >= cB ? OnWhite : OnBlack;
                Assert.Equal(winner, swatch.OnFill);
                if (Math.Max(cW, cB) >= 4.5) {
                    Assert.True(StudioColorMath.ContrastRatio(swatch.OnFill, fill) >= 4.5);
                }
            }

            Color mid = FindMidLYellowGreen();
            double midW = StudioColorMath.ContrastRatio(OnWhite, mid);
            double midB = StudioColorMath.ContrastRatio(OnBlack, mid);
            Assert.True(Math.Max(midW, midB) < 4.5,
                $"mid-L yellow-green {mid} should have both contrasts < 4.5 (w={midW:0.2} b={midB:0.2})");
            var midSwatch = StudioTrackPalette.DeriveSurfaces(mid, ctx, mid);
            Color midWinner = midW >= midB ? OnWhite : OnBlack;
            Assert.Equal(midWinner, midSwatch.OnFill);
        }

        [Fact]
        public void DeriveSurfaces_WaveformDeltaL() {
            Assert.Equal(0.5, StudioColorMath.Clamp(0.5, 0.8, 0.2));

            var darkCtx = StudioDark(StudioTrackColorMode.Rainbow);
            var darkFill = Color.FromRgb(0x00, 0xB8, 0xFF);
            var dark = StudioTrackPalette.DeriveSurfaces(darkFill, darkCtx, darkFill);
            var (_, _, lFillDark) = StudioColorMath.RgbToHsl(dark.Fill);
            var (_, _, lWaveDark) = StudioColorMath.RgbToHsl(dark.Waveform);
            Assert.True(Math.Abs(lWaveDark - lFillDark) >= 0.12,
                $"dark ΔL={Math.Abs(lWaveDark - lFillDark):0.00}");

            var lightCtx = new StudioTrackPaletteContext(
                StudioTrackColorMode.Rainbow,
                Color.FromRgb(0xE1, 0xE2, 0xE7),
                Color.FromRgb(0x2E, 0x7D, 0xE9),
                Color.FromRgb(0x00, 0x71, 0x97),
                IsDark: false);
            var lightFill = Color.FromRgb(0x2E, 0x7D, 0xE9);
            var light = StudioTrackPalette.DeriveSurfaces(lightFill, lightCtx, lightFill);
            var (_, _, lFillLight) = StudioColorMath.RgbToHsl(light.Fill);
            var (_, _, lWaveLight) = StudioColorMath.RgbToHsl(light.Waveform);
            Assert.True(Math.Abs(lWaveLight - lFillLight) >= 0.12,
                $"light ΔL={Math.Abs(lWaveLight - lFillLight):0.00}");

            var pastel = StudioColorMath.HslToRgb(8, 0.36, 0.76);
            var pastelSwatch = StudioTrackPalette.DeriveSurfaces(pastel, lightCtx, pastel);
            var (_, _, lFillPastel) = StudioColorMath.RgbToHsl(pastelSwatch.Fill);
            var (_, _, lWavePastel) = StudioColorMath.RgbToHsl(pastelSwatch.Waveform);
            Assert.True(lWavePastel < lFillPastel,
                $"pastel waveform L={lWavePastel:0.00} should be darker than fill L={lFillPastel:0.00}");
            Assert.True(lFillPastel - lWavePastel >= 0.12,
                $"pastel ΔL={lFillPastel - lWavePastel:0.00}");
        }

        [Fact]
        public void Fixed_Blue_IsExactAccent1AndAccent2() {
            var ctx = StudioDark(StudioTrackColorMode.Fixed);
            var swatch = StudioTrackPalette.ResolveAll(Tracks(1, "Blue"), ctx)[0];
            Assert.Equal(ctx.Accent1, swatch.Fill);
            Assert.Equal(ctx.Accent2, swatch.FillSelected);
            Assert.False(swatch.DrawSelectedStroke);
            Assert.Equal(Avalonia.Media.Colors.White, swatch.Waveform);
            Assert.Equal(Color.FromRgb(0x4E, 0xA6, 0xEA), swatch.HeaderAccent);
            Assert.NotEqual(swatch.Fill, swatch.FillMuted);

            var unknown = StudioTrackPalette.ResolveAll(Tracks(1, "NotAColor"), ctx)[0];
            Assert.Equal(ctx.Accent1, unknown.Fill);
            Assert.Equal(ctx.Accent2, unknown.FillSelected);
        }

        [Fact]
        public void Fixed_Pink_UsesNamedLut() {
            var ctx = StudioDark(StudioTrackColorMode.Fixed);
            var swatch = StudioTrackPalette.ResolveAll(Tracks(1, "Pink"), ctx)[0];
            var pink = Color.Parse("#F06292");
            var (hLut, _, _) = StudioColorMath.RgbToHsl(pink);
            var (hFill, _, _) = StudioColorMath.RgbToHsl(swatch.Fill);
            Assert.True(HueDelta(hFill, hLut) <= 5,
                $"Pink fill hue {hFill:0.0} vs LUT {hLut:0.0}");
            Assert.True(swatch.DrawSelectedStroke);
            Assert.Equal(pink, swatch.HeaderAccent);
        }

        [Fact]
        public void ClassicFallback_IgnoresMode_AndKeepsAccent2Selected() {
            var ctx = StudioDark(StudioTrackColorMode.Rainbow);
            var track = new UTrack("Track1") { TrackNo = 0, TrackColor = "Pink" };
            var swatch = StudioTrackPalette.ResolveClassicFallback(track, ctx);
            Assert.Equal(ctx.Accent1, swatch.Fill);
            Assert.Equal(ctx.Accent2, swatch.FillSelected);
            Assert.Equal(Color.Parse("#F06292"), swatch.HeaderAccent);
            Assert.Equal(swatch.Fill, swatch.FillMuted);
            Assert.False(swatch.DrawSelectedStroke);
            Assert.Equal(Avalonia.Media.Colors.White, swatch.Waveform);
        }

        static StudioTrackPaletteContext StudioDark(StudioTrackColorMode mode) {
            var p = StudioThemeGenerator.BuildStudioDark();
            return new StudioTrackPaletteContext(
                mode,
                p["BackgroundColor"],
                p["AccentColor1"],
                p["AccentColor2"],
                IsDark: true);
        }

        static StudioTrackPaletteContext TokyoDay(StudioTrackColorMode mode) {
            var p = StudioThemeGenerator.BuildTokyoDay();
            return new StudioTrackPaletteContext(
                mode,
                p["BackgroundColor"],
                p["AccentColor1"],
                p["AccentColor2"],
                IsDark: false);
        }

        static List<UTrack> Tracks(int n, string color = "Blue") {
            var list = new List<UTrack>(n);
            for (int i = 0; i < n; i++) {
                list.Add(new UTrack($"Track{i + 1}") { TrackNo = i, TrackColor = color });
            }
            return list;
        }

        static double HueDelta(double a, double b) {
            double d = Math.Abs(a - b) % 360;
            return Math.Min(d, 360 - d);
        }

        static Color FindMidLYellowGreen() {
            for (double l = 0.42; l <= 0.58; l += 0.01) {
                for (double s = 0.10; s <= 0.45; s += 0.05) {
                    var c = StudioColorMath.HslToRgb(90, s, l);
                    double cW = StudioColorMath.ContrastRatio(OnWhite, c);
                    double cB = StudioColorMath.ContrastRatio(OnBlack, c);
                    if (Math.Max(cW, cB) < 4.5) {
                        return c;
                    }
                }
            }
            return StudioColorMath.HslToRgb(90, 0.20, 0.48);
        }
    }
}
