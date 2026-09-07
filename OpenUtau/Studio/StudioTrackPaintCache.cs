using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;
using OpenUtau.Core.Util;
using ReactiveUI;
using Serilog;

namespace OpenUtau.App.Studio {
    public class StudioTrackPaletteChangedEvent { }

    /// <summary>
    /// UI brush cache for <see cref="StudioTrackPalette"/> swatches.
    /// Controls call <see cref="ForTrack"/> / <see cref="ForPart"/>; Classic vs Studio
    /// field mapping stays here so ViewModels do not branch on mode.
    /// </summary>
    public static class StudioTrackPaintCache {
        static readonly Color FallbackAccent1 = Color.FromRgb(0x4E, 0xA6, 0xEA);
        static readonly Color FallbackAccent2 = Color.FromRgb(0xFF, 0x67, 0x9D);
        static readonly Color FallbackBackground = Color.FromRgb(0x30, 0x30, 0x30);
        static readonly SolidColorBrush WhiteTextBrush = new(Avalonia.Media.Colors.White);
        static readonly Dictionary<uint, SolidColorBrush> textBrushes = new();

        static StudioTrackPaint[]? paints;

        public static void Invalidate(StudioTrackPaletteChangeReason reason) {
            // Always notify: reorder can keep N/mode/accents unchanged.
            var ctx = Rebuild();
            int n = paints?.Length ?? 0;
            Log.Debug(
                "StudioTrackPaintCache.Invalidate reason={Reason} mode={Mode} n={N} bg={Background} dark={IsDark}",
                reason, ctx.Mode, n, ctx.Background, ctx.IsDark);
            MessageBus.Current.SendMessage(new StudioTrackPaletteChangedEvent());
        }

        public static StudioTrackPaint ForTrack(UTrack track) {
            Ensure();
            int i = track.TrackNo;
            if (paints == null || i < 0 || i >= paints.Length) {
                return IdentityFallback();
            }
            return paints[i];
        }

        public static StudioTrackPaint ForPart(UPart part) {
            Ensure();
            int i = part.trackNo;
            if (paints == null || i < 0 || i >= paints.Length) {
                return IdentityFallback();
            }
            return paints[i];
        }

        static void Ensure() {
            var tracks = DocManager.Inst.Project.tracks;
            if (paints == null || paints.Length != tracks.Count) {
                Rebuild();
            }
        }

        static StudioTrackPaletteContext Rebuild() {
            var ctx = ContextFromTheme();
            try {
                var tracks = DocManager.Inst.Project.tracks;
                if (tracks == null || tracks.Count == 0) {
                    paints = Array.Empty<StudioTrackPaint>();
                    return ctx;
                }
                StudioTrackSwatch[] swatches;
                if (StudioUI.IsEnabled) {
                    swatches = StudioTrackPalette.ResolveAll(tracks, ctx);
                } else {
                    swatches = new StudioTrackSwatch[tracks.Count];
                    for (int i = 0; i < tracks.Count; i++) {
                        swatches[i] = StudioTrackPalette.ResolveClassicFallback(tracks[i], ctx);
                    }
                }
                var built = new StudioTrackPaint[swatches.Length];
                for (int i = 0; i < swatches.Length; i++) {
                    built[i] = BuildPaint(tracks[i], swatches[i], ctx);
                }
                paints = built;
            } catch (Exception e) {
                Log.Error(e, "StudioTrackPaintCache.Rebuild failed");
                paints = Array.Empty<StudioTrackPaint>();
            }
            return ctx;
        }

        static StudioTrackPaint IdentityFallback() {
            var ctx = ContextFromTheme();
            var dummy = new UTrack { TrackColor = "Blue" };
            if (StudioUI.IsEnabled) {
                var identityCtx = ctx with { Mode = StudioTrackColorMode.Fixed };
                var swatch = StudioTrackPalette.ResolveAll(new[] { dummy }, identityCtx)[0];
                return BuildPaint(dummy, swatch, identityCtx);
            }
            return BuildPaint(dummy, StudioTrackPalette.ResolveClassicFallback(dummy, ctx), ctx);
        }

        static StudioTrackPaletteContext ContextFromTheme() {
            var mode = (StudioTrackColorMode)Preferences.Default.StudioTrackColorMode;
            if (Application.Current == null) {
                return new StudioTrackPaletteContext(
                    mode, FallbackBackground, FallbackAccent1, FallbackAccent2, IsDark: true);
            }
            IResourceDictionary resDict = Application.Current.Resources;
            Color background = TryReadColor(resDict, "BackgroundColor", FallbackBackground);
            Color accent1 = TryReadColor(resDict, "AccentColor1", FallbackAccent1);
            Color accent2 = TryReadColor(resDict, "AccentColor2", FallbackAccent2);
            bool isDark;
            if (resDict.TryGetResource("IsDarkMode", ThemeVariant.Default, out var darkVar)
                && darkVar is bool darkFlag) {
                isDark = darkFlag;
            } else {
                isDark = StudioColorMath.RelativeLuminance(background) < 0.45;
            }
            return new StudioTrackPaletteContext(mode, background, accent1, accent2, isDark);
        }

        static Color TryReadColor(IResourceDictionary res, string key, Color fallback) {
            if (!res.TryGetResource(key, ThemeVariant.Default, out var value) || value == null) {
                return fallback;
            }
            return value switch {
                Color c => c,
                ISolidColorBrush b => b.Color,
                _ => fallback,
            };
        }

        static StudioTrackPaint BuildPaint(
            UTrack track, StudioTrackSwatch swatch, StudioTrackPaletteContext ctx) {
            var fillBrush = new SolidColorBrush(swatch.Fill);
            var fillSelectedBrush = new SolidColorBrush(swatch.FillSelected);
            var fillMutedBrush = new SolidColorBrush(swatch.FillMuted);
            var onFillBrush = TextBrush(swatch.OnFill);
            var onHeaderAccentBrush = TextBrush(swatch.OnHeaderAccent);
            var noteThumbBrush = new SolidColorBrush(swatch.NoteThumbnail);
            IPen notePen = new Pen(noteThumbBrush, 3);
            IPen? selectedStroke = swatch.DrawSelectedStroke ? new Pen(onFillBrush, 1) : null;
            IPen? fadeHandle = swatch.DrawSelectedStroke ? new Pen(fillSelectedBrush, 1) : null;
            IPen? fadeLine = swatch.DrawSelectedStroke ? new Pen(onFillBrush) : null;
            uint packed = StudioColorMath.PackRgba8888(swatch.Waveform);

            SolidColorBrush headerAccentBrush;
            TrackColor legacy;
            SolidColorBrush ghost;
            if (!StudioUI.IsEnabled) {
                var named = ThemeManager.GetTrackColor(track.TrackColor ?? "Blue");
                var blue = ThemeManager.GetTrackColor("Blue");
                headerAccentBrush = named.AccentColor;
                legacy = Preferences.Default.UseTrackColor ? named : blue;
                ghost = named.AccentColorLightSemi;
            } else if (!swatch.DrawSelectedStroke) {
                var blue = ThemeManager.GetTrackColor("Blue");
                headerAccentBrush = blue.AccentColor;
                legacy = blue;
                ghost = blue.AccentColorLightSemi;
            } else {
                string name = ctx.Mode == StudioTrackColorMode.Fixed
                    ? (track.TrackColor ?? "Blue")
                    : "(studio)";
                legacy = AsLegacyTrackColor(name, swatch);
                headerAccentBrush = new SolidColorBrush(swatch.HeaderAccent);
                ghost = legacy.AccentColorLightSemi;
            }

            return new StudioTrackPaint {
                Colors = swatch,
                FillBrush = fillBrush,
                FillSelectedBrush = fillSelectedBrush,
                FillMutedBrush = fillMutedBrush,
                HeaderAccentBrush = headerAccentBrush,
                OnFillBrush = onFillBrush,
                OnHeaderAccentBrush = onHeaderAccentBrush,
                NoteThumbnailPen = notePen,
                SelectedStrokePen = selectedStroke,
                FadeHandlePen = fadeHandle,
                FadeLinePen = fadeLine,
                WaveformPackedRgba = packed,
                LegacyTrackColor = legacy,
                GhostBrush = ghost,
            };
        }

        static TrackColor AsLegacyTrackColor(string name, StudioTrackSwatch swatch) {
            Color light = StudioColorMath.Lighten(swatch.HeaderAccent, 0.20);
            return new TrackColor(
                name,
                Hex(swatch.HeaderAccent),
                Hex(swatch.FillSelected),
                Hex(light),
                Hex(swatch.CenterKey));
        }

        static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        // TextLayoutCache keys by IBrush reference; intern so Rebuild does not leak layouts.
        static SolidColorBrush TextBrush(Color c) {
            if (c == Avalonia.Media.Colors.White) {
                return WhiteTextBrush;
            }
            uint key = StudioColorMath.PackRgba8888(c);
            if (!textBrushes.TryGetValue(key, out var brush)) {
                brush = new SolidColorBrush(c);
                textBrushes[key] = brush;
            }
            return brush;
        }
    }
}
