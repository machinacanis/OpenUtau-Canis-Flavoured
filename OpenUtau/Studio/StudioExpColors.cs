using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace OpenUtau.App.Studio {
    /// <summary>
    /// One color per expression, in document order, using the same palette
    /// the track headers use. Fixed mode cycles the named track colors;
    /// Rainbow and ThemeGradient ignore the name and key off index.
    /// </summary>
    public static class StudioExpColors {
        public sealed class Paint {
            public required IBrush Square { get; init; }
            public required IBrush Fill { get; init; }
            public required IBrush SelectedFill { get; init; }
            public required IPen DefaultPen { get; init; }
            public required IPen EditedPen { get; init; }
            public required IPen SelectedDefaultPen { get; init; }
            public required IPen SelectedEditedPen { get; init; }
        }

        static readonly Color Fallback = Color.FromRgb(0x4E, 0xA6, 0xEA);
        static Paint? fallbackPaint;
        static Dictionary<string, Paint>? paints;
        static string[]? keys;
        static bool subscribed;

        public static StudioTrackSwatch[] ResolveSwatches(
            int count, StudioTrackPaletteContext ctx, StudioTrackColorConfig? config = null) {
            if (count <= 0) {
                return Array.Empty<StudioTrackSwatch>();
            }
            int paletteCount = Math.Max(1, ThemeManager.TrackColors.Count);
            var tracks = new UTrack[count];
            for (int i = 0; i < count; i++) {
                tracks[i] = new UTrack {
                    TrackNo = i,
                    TrackColor = ThemeManager.TrackColors[i % paletteCount].Name,
                };
            }
            return StudioTrackPalette.ResolveAll(tracks, ctx, config);
        }

        public static IBrush Square(string? abbr) => For(abbr).Square;

        public static Paint For(string? abbr) {
            EnsureSubscribed();
            Ensure();
            if (!string.IsNullOrEmpty(abbr) && paints != null && paints.TryGetValue(abbr, out var paint)) {
                return paint;
            }
            return fallbackPaint ??= FromColor(Fallback);
        }

        public static void Invalidate() {
            paints = null;
            keys = null;
        }

        static void EnsureSubscribed() {
            if (subscribed) {
                return;
            }
            subscribed = true;
            MessageBus.Current.Listen<StudioTrackPaletteChangedEvent>()
                .Subscribe(_ => Invalidate());
        }

        static void Ensure() {
            var project = DocManager.Inst.Project;
            var current = project?.expressions.Values.Select(d => d.abbr).ToArray() ?? Array.Empty<string>();
            if (paints != null && keys != null && keys.SequenceEqual(current)) {
                return;
            }
            keys = current;
            paints = new Dictionary<string, Paint>(current.Length);
            if (current.Length == 0) {
                return;
            }
            var swatches = StudioTrackPaintCache.ResolveSequence(
                current.Select((abbr, i) => new UTrack {
                    TrackNo = i,
                    TrackColor = ThemeManager.TrackColors[i % Math.Max(1, ThemeManager.TrackColors.Count)].Name,
                }).ToArray());
            for (int i = 0; i < current.Length && i < swatches.Length; i++) {
                paints[current[i]] = FromColor(swatches[i].HeaderAccent);
            }
        }

        static Paint FromColor(Color color) {
            var selected = StudioColorMath.Lighten(color, 0.35);
            var fill = new SolidColorBrush(color);
            var selectedFill = new SolidColorBrush(selected);
            var dim = Color.FromArgb(0x80, color.R, color.G, color.B);
            var selectedDim = Color.FromArgb(0x80, selected.R, selected.G, selected.B);
            return new Paint {
                Square = fill,
                Fill = fill,
                SelectedFill = selectedFill,
                DefaultPen = new Pen(new SolidColorBrush(dim), 3),
                EditedPen = new Pen(fill, 3),
                SelectedDefaultPen = new Pen(new SolidColorBrush(selectedDim), 3),
                SelectedEditedPen = new Pen(selectedFill, 3),
            };
        }
    }
}
