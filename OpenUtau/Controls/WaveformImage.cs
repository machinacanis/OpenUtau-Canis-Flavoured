using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using OpenUtau.App.Studio;
using OpenUtau.App.ViewModels;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;
using OpenUtau.Core.Util;
using ReactiveUI;
using ReactiveUI.Primitives;
using Serilog;

namespace OpenUtau.App.Controls {
    class WaveformImage : Control {
        public static readonly DirectProperty<WaveformImage, double> TickWidthProperty =
            AvaloniaProperty.RegisterDirect<WaveformImage, double>(
                nameof(TickWidth),
                o => o.TickWidth,
                (o, v) => o.TickWidth = v);
        public static readonly DirectProperty<WaveformImage, double> TickOffsetProperty =
            AvaloniaProperty.RegisterDirect<WaveformImage, double>(
                nameof(TickOffset),
                o => o.TickOffset,
                (o, v) => o.TickOffset = v);
        public static readonly DirectProperty<WaveformImage, double> TrackHeightProperty =
            AvaloniaProperty.RegisterDirect<WaveformImage, double>(
                nameof(TrackHeight),
                o => o.TrackHeight,
                (o, v) => o.TrackHeight = v);
        public static readonly DirectProperty<WaveformImage, double> TrackOffsetProperty =
            AvaloniaProperty.RegisterDirect<WaveformImage, double>(
                nameof(TrackOffset),
                o => o.TrackOffset,
                (o, v) => o.TrackOffset = v);
        public static readonly DirectProperty<WaveformImage, bool> ShowWaveformProperty =
            AvaloniaProperty.RegisterDirect<WaveformImage, bool>(
                nameof(ShowWaveform),
                o => o.ShowWaveform,
                (o, v) => o.ShowWaveform = v);

        public double TickWidth {
            get => tickWidth;
            set => SetAndRaise(TickWidthProperty, ref tickWidth, value);
        }
        public double TickOffset {
            get => tickOffset;
            set => SetAndRaise(TickOffsetProperty, ref tickOffset, value);
        }
        public double TrackHeight {
            get => trackHeight;
            set => SetAndRaise(TrackHeightProperty, ref trackHeight, value);
        }
        public double TrackOffset {
            get => trackOffset;
            set => SetAndRaise(TrackOffsetProperty, ref trackOffset, value);
        }
        public bool ShowWaveform {
            get => showWaveform;
            set => SetAndRaise(ShowWaveformProperty, ref showWaveform, value);
        }

        private double tickWidth;
        private double tickOffset;
        private double trackHeight;
        private double trackOffset;
        private bool showWaveform;

        private WriteableBitmap? bitmap;
        private float[] sampleData = Array.Empty<float>();
        private float[] colMin = Array.Empty<float>();
        private float[] colMax = Array.Empty<float>();
        private int sampleCount;
        private int[] bitmapData = Array.Empty<int>();
        private readonly DispatcherTimer refreshTimer;
        private DateTime mixUnlockTime = DateTime.MinValue;
        private bool wasRendering = false;

        // Legacy tuning constants now live on preferences
        // (WaveformAmpScaleRows / WaveformFollowOffsetRows /
        // WaveformSmartSplit* / WaveformFallbackLeadMs / WaveformAlphaPercent)
        // so Studio UI users can tune the layouts without rebuilding.
        int waveformRgb = PackRgb(ThemeManager.WaveformColor);

        public WaveformImage() {
            // The projection payload is not read here; deliveries are repaint signals.
            OpenUtau.Core.Render.RenderView.Inst.Observe(_ => InvalidateVisual());

            // 渲染过程中每个片段完成都会触发一次刷新，合并为 50ms 一次，
            // 避免播放/预渲染时反复全量重画波形。
            refreshTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(50),
                DispatcherPriority.Background,
                RefreshTimer_Tick);
            MessageBus.Current.Listen<WaveformRefreshEvent>()
                .Subscribe(e => {
                    refreshTimer.Stop();
                    refreshTimer.Start();
                });
            MessageBus.Current.Listen<ThemeChangedEvent>()
                .Subscribe(_ => {
                    waveformRgb = PackRgb(ThemeManager.WaveformColor);
                    refreshTimer.Stop();
                    refreshTimer.Start();
                });
            MessageBus.Current.Listen<StudioUIChangedEvent>()
                .Subscribe(_ => {
                    refreshTimer.Stop();
                    refreshTimer.Start();
                });
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e) {
            refreshTimer.Stop();
            InvalidateVisual();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (change.Property == DataContextProperty ||
                change.Property == TickWidthProperty ||
                change.Property == TickOffsetProperty ||
                change.Property == TrackHeightProperty ||
                change.Property == TrackOffsetProperty ||
                change.Property == ShowWaveformProperty) {
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext context) {
            if (DataContext == null || double.IsNaN(((NotesViewModel)DataContext).TickOffset)) {
                return;
            }
            if (!ShowWaveform) {
                base.Render(context);
                return;
            }
            var bitmap = GetBitmap();
            if (bitmap != null) {
                Array.Clear(bitmapData, 0, bitmapData.Length);
                var viewModel = (NotesViewModel?)DataContext;
                if (viewModel != null &&
                    viewModel.TickWidth > ViewConstants.PianoRollTickWidthShowDetails) {
                    var project = viewModel.Project;
                    var part = viewModel.Part;
                    if (project != null && part != null) {
                        double leftMs = project.timeAxis.TickPosToMsPos(viewModel.TickOrigin + viewModel.TickOffset);
                        double rightMs = project.timeAxis.TickPosToMsPos(viewModel.TickOrigin + viewModel.TickOffset + viewModel.ViewportTicks);
                        int samplePos = (int)(leftMs * 44100 / 1000) * 2;
                        sampleCount = (int)((rightMs - leftMs) * 44100 / 1000) * 2;

                        if (sampleData.Length < sampleCount) {
                            Array.Resize(ref sampleData, sampleCount);
                        }
                        bool needsAnotherFrame = false;
                        Array.Clear(sampleData, 0, sampleData.Length);

                        // The part's current projection: phrase hashes and
                        // precomputed layouts, shared by the placement list below
                        // and the draw coverage further down.
                        var projection = OpenUtau.Core.Render.RenderView.Inst.Current(part);
                        var phraseView = new (ulong hash, double startMs, double endMs)[projection.Phrases.Count];
                        for (int p = 0; p < projection.Phrases.Count; ++p) {
                            var view = projection.Phrases[p];
                            phraseView[p] = (view.Hash, view.Layout.StartMs, view.Layout.EndMs);
                        }

                        // Only phrases whose pcm has rendered appear, so a part still
                        // rendering draws only what has finished.
                        var planner = OpenUtau.Core.PlaybackManager.Inst.MixPlanner;
                        if (MixPlanner.TryGetPartPlacements(planner, part, phraseView, out var pcmList)) {
                            var slots = new OpenUtau.Core.SignalChain.SampleSlot[pcmList.Count];
                            for (int i = 0; i < pcmList.Count; ++i) {
                                var p = pcmList[i];
                                slots[i] = new OpenUtau.Core.SignalChain.SampleSlot(
                                    p.posMs, p.durMs, 0, p.channels, p.pcm,
                                    OpenUtau.Core.SignalChain.SlotState.Ready);
                            }
                            var source = new OpenUtau.Core.SignalChain.SlotMixSource();
                            source.SetSlots(slots);
                            source.Mix(samplePos, sampleData, 0, sampleCount);
                        }

                        bool isRendering = PlaybackManager.Inst.StartingToPlay;
                        if (wasRendering && !isRendering) {
                            mixUnlockTime = DateTime.Now;
                        }
                        wasRendering = isRendering;

                        double snapAgeMs = (DateTime.Now - mixUnlockTime).TotalMilliseconds;
                        double snapProgress = Math.Clamp(snapAgeMs / 300.0, 0.0, 1.0);
                        float snapEase = 1.0f - (float)Math.Pow(1.0 - snapProgress, 3);

                        if (snapProgress < 1.0) needsAnotherFrame = true;

int drawWidth = Math.Min((int)Bounds.Width, bitmap.PixelSize.Width);
                        int drawHeight = Math.Min((int)Bounds.Height, bitmap.PixelSize.Height);
                        if (colMin.Length < drawWidth) {
                            Array.Resize(ref colMin, drawWidth);
                            Array.Resize(ref colMax, drawWidth);
                        }
                        Array.Clear(colMin, 0, drawWidth);
                        Array.Clear(colMax, 0, drawWidth);

                        // Phrase audio ranges as [startMs, endMs] pairs, matching
                        // the slot layout of the mix, so that time ranges
                        // without any phrase are left blank instead of drawing a
                        // zero-volume line. Silence inside a phrase still draws.
                        double[]? phraseRanges = null;
                        if (phraseView.Length > 0) {
                            phraseRanges = new double[phraseView.Length * 2];
                            for (int p = 0; p < phraseView.Length; ++p) {
                                phraseRanges[p * 2] = phraseView[p].startMs;
                                phraseRanges[p * 2 + 1] = phraseView[p].endMs;
                            }
                        }

                        int startSample = 0;
                        double columnStartMs = leftMs;
                        for (int i = 0; i < drawWidth; ++i) {
                            double endTick = viewModel.TickOrigin + viewModel.TickOffset + (i + 1.0) / viewModel.TickWidth;
                            double endMs = project.timeAxis.TickPosToMsPos(endTick);
                            int endSample = Math.Clamp((int)((endMs - leftMs) * 44100 / 1000) * 2, 0, sampleCount);

// Skip drawing where no phrase has audio.
                            bool covered = false;
                            if (phraseRanges != null) {
                                for (int p = 0; p < phraseRanges.Length; p += 2) {
                                    if (phraseRanges[p + 1] > columnStartMs && phraseRanges[p] < endMs) {
                                        covered = true;
                                        break;
                                    }
                                }
                            }
                            if (!covered) {
                                startSample = endSample;
                                columnStartMs = endMs;
                                continue;
                            }
                            if (endSample > startSample) {
                                float rawMin = float.MaxValue;
                                float rawMax = float.MinValue;
                                for (int s = startSample; s < endSample; s++) {
                                    float val = sampleData[s];
                                    if (val < rawMin) rawMin = val;
                                    if (val > rawMax) rawMax = val;
                                }
                                if (rawMin == float.MaxValue) rawMin = 0;
                                if (rawMax == float.MinValue) rawMax = 0;
                                colMin[i] = rawMin * snapEase;
                                colMax[i] = rawMax * snapEase;
                            }
                            startSample = endSample;
                            columnStartMs = endMs;
                        }

                        int layout = Preferences.Default.WaveformLayout;
                        double scale = viewModel.TrackHeight * Preferences.Default.WaveformAmpScaleRows
                            * (Math.Max(1, Preferences.Default.WaveformScalePercent) / 100.0);
                        double leftTick = viewModel.TickOffset;
                        double rightTick = viewModel.TickOffset + viewModel.ViewportTicks;
                        if (!StudioUI.IsEnabled) {
                            DrawClassicStrip(bitmapData, bitmap.PixelSize.Width, drawWidth, drawHeight);
                        } else if (layout == 2) {
                            DrawFixed(bitmapData, bitmap.PixelSize.Width, drawWidth, drawHeight, scale);
                        } else if (layout == 1 && Preferences.Default.WaveformFollowMode == 1) {
                            DrawFollowSmart(viewModel, project, part, bitmapData, bitmap.PixelSize.Width,
                                drawWidth, drawHeight, scale, leftTick, rightTick);
                        } else {
                            bool followAbsolute = layout == 1;
                            foreach (var note in part.notes) {
                                NoteWaveRange(project, part, note, out int waveStart, out int waveEnd,
                                    out bool fadeIn, out int fadeOutTicks);
                                if (waveEnd < leftTick || waveStart > rightTick) {
                                    continue;
                                }
                                int x0 = (int)Math.Floor((waveStart - viewModel.TickOffset) * viewModel.TickWidth);
                                int x1 = (int)Math.Ceiling((waveEnd - viewModel.TickOffset) * viewModel.TickWidth);
                                x0 = Math.Clamp(x0, 0, drawWidth);
                                x1 = Math.Clamp(x1, 0, drawWidth);
                                if (x1 <= x0) {
                                    continue;
                                }
                                float tone = note.AdjustedTone - 0.5f;
                                if (followAbsolute) {
                                    tone -= (float)Preferences.Default.WaveformFollowOffsetRows;
                                }
                                double yCenter = viewModel.TickToneToPoint(note.position, tone).Y;
                                double fadeInSpan = note.position - waveStart;
                                for (int i = x0; i < x1; i++) {
                                    if (colMin[i] == 0 && colMax[i] == 0) {
                                        continue;
                                    }
                                    double tick = viewModel.TickOffset + i / viewModel.TickWidth;
                                    float alpha = 1f;
                                    if (fadeIn && fadeInSpan > 0 && tick < note.position) {
                                        alpha = (float)Math.Clamp((tick - waveStart) / fadeInSpan, 0, 1);
                                    }
                                    if (fadeOutTicks > 0 && tick > note.End - fadeOutTicks) {
                                        alpha *= (float)Math.Clamp((note.End - tick) / fadeOutTicks, 0, 1);
                                    }
                                    if (alpha <= 0) {
                                        continue;
                                    }
                                    int y1 = (int)Math.Round(yCenter - colMax[i] * scale);
                                    int y2 = (int)Math.Round(yCenter - colMin[i] * scale);
                                    DrawPeak(bitmapData, bitmap.PixelSize.Width, drawHeight, i, y1, y2, alpha);
                                }
                            }
                        }

                        if (needsAnotherFrame) {
                            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
                        }
                    }
                }
                using (var frameBuffer = bitmap.Lock()) {
                    Marshal.Copy(bitmapData, 0, frameBuffer.Address, bitmapData.Length);
                }
            }
            base.Render(context);
            if (bitmap != null) {
                var rect = Bounds.WithX(0).WithY(0);
                context.DrawImage(bitmap, rect, rect);
            }
        }

        private WriteableBitmap? GetBitmap() {
            int desiredWidth = (int)Bounds.Width;
            int desiredHeight = (int)Bounds.Height;
            if (desiredWidth == 0 || desiredHeight == 0) {
                return null;
            }
            if (bitmap == null ||
                bitmap.PixelSize.Width < desiredWidth ||
                bitmap.PixelSize.Height < desiredHeight) {
                bitmap?.Dispose();
                var size = new PixelSize(desiredWidth, desiredHeight);
                bitmap = new WriteableBitmap(
                    size, new Vector(96, 96),
                    Avalonia.Platform.PixelFormat.Rgba8888,
                    Avalonia.Platform.AlphaFormat.Unpremul);
                Log.Information($"Created bitmap {size}");
                bitmapData = new int[size.Width * size.Height];
            }
            return bitmap;
        }

        static UPhoneme? FirstPhoneme(UVoicePart part, UNote note) {
            foreach (var ph in part.phonemes) {
                if (ph.Parent == note) {
                    return ph;
                }
            }
            return null;
        }

        static UPhoneme? LastPhoneme(UVoicePart part, UNote note) {
            UPhoneme? last = null;
            foreach (var ph in part.phonemes) {
                if (ph.Parent == note) {
                    last = ph;
                }
            }
            return last;
        }


        // Classic editor: 60px strip, 60px above the notes-canvas bottom.
        void DrawClassicStrip(int[] data, int stride, int drawWidth, int drawHeight) {
            const int stripHeight = 60;
            const int marginBottom = 60;
            const int color = 0x7F7F7F7F;
            int stripTop = Math.Max(0, drawHeight - marginBottom - stripHeight);
            for (int i = 0; i < drawWidth; i++) {
                if (colMin[i] == 0 && colMax[i] == 0) {
                    continue;
                }
                float min = 0.5f + colMin[i] * 0.5f;
                float max = 0.5f + colMax[i] * 0.5f;
                int yMin = stripTop + (int)Math.Round(Math.Clamp(min * stripHeight, 0, stripHeight - 1));
                int yMax = stripTop + (int)Math.Round(Math.Clamp(max * stripHeight, 0, stripHeight - 1));
                if (yMin > yMax) {
                    int temp = yMax;
                    yMax = yMin;
                    yMin = temp;
                }
                yMin = Math.Clamp(yMin, 0, drawHeight - 1);
                yMax = Math.Clamp(yMax, 0, drawHeight - 1);
                for (int y = yMin; y <= yMax; y++) {
                    data[i + stride * y] = color;
                }
            }
        }

        // Studio fixed layout: one horizontal peak strip. Amplitude uses
        // the same pixel scale as embed/follow; only the center line moves.
        void DrawFixed(int[] data, int stride, int drawWidth, int drawHeight, double scale) {
            int bottomPx = Math.Max(0, Preferences.Default.WaveformFixedBottomPx);
            double yCenter = drawHeight - bottomPx;
            for (int i = 0; i < drawWidth; i++) {
                if (colMin[i] == 0 && colMax[i] == 0) {
                    continue;
                }
                int y1 = (int)Math.Round(yCenter - colMax[i] * scale);
                int y2 = (int)Math.Round(yCenter - colMin[i] * scale);
                DrawPeak(data, stride, drawHeight, i, y1, y2, 1f);
            }
        }

        void NoteWaveRange(UProject project, UVoicePart part, UNote note,
            out int waveStart, out int waveEnd, out bool fadeIn, out int fadeOutTicks) {
            bool adjPrev = note.Prev != null && note.Prev.End >= note.position;
            bool adjNext = note.Next != null && note.End >= note.Next.position;
            waveStart = note.position;
            waveEnd = note.End;
            fadeIn = false;
            fadeOutTicks = 0;
            if (!adjPrev) {
                int lead = LeadTicks(project, part, note);
                if (note.Prev != null) {
                    lead = Math.Min(lead, Math.Max(0, note.position - note.Prev.End));
                }
                waveStart = note.position - lead;
            }
            if (!adjNext) {
                int tail = TailTicks(project, part, note);
                if (note.Next != null) {
                    tail = Math.Min(tail, Math.Max(0, note.Next.position - note.End));
                }
                waveEnd = note.End + tail;
            }
            if (Preferences.Default.WaveformStyle == 0) {
                if (adjPrev && note.Prev!.AdjustedTone != note.AdjustedTone) {
                    int fadeInTicks = MsDeltaTicks(project, note.PositionMs, Math.Max(0, Preferences.Default.WaveformFadeInMs));
                    waveStart = note.position - fadeInTicks;
                    fadeIn = true;
                }
                if (adjNext && note.Next!.AdjustedTone != note.AdjustedTone) {
                    fadeOutTicks = Math.Min(
                        MsDeltaTicks(project, note.EndMs, Math.Max(0, Preferences.Default.WaveformFadeOutMs)),
                        Math.Max(1, note.duration / 2));
                }
            }
        }

        // Connected notes form a phrase. Smart follow keeps a horizontal band
        // under the group's lowest note. A 2-semitone drop or leap starts a
        // new shelf unless a later connected note returns to the old floor.
        void DrawFollowSmart(NotesViewModel viewModel, UProject project, UVoicePart part,
            int[] data, int stride, int drawWidth, int drawHeight,
            double scale, double leftTick, double rightTick) {
            var notes = new List<UNote>(part.notes);
            int i = 0;
            while (i < notes.Count) {
                int start = i;
                float groupMin = notes[i].AdjustedTone;
                i++;
                while (i < notes.Count && notes[i - 1].End >= notes[i].position) {
                    if (ShouldSplitSmart(notes, i, groupMin)) {
                        break;
                    }
                    float tone = notes[i].AdjustedTone;
                    if (tone < groupMin) {
                        groupMin = tone;
                    }
                    i++;
                }
                DrawFollowGroup(viewModel, project, part, notes, start, i, groupMin,
                    data, stride, drawWidth, drawHeight, scale, leftTick, rightTick);
            }
        }

        static bool ReturnsToFloor(List<UNote> notes, int i, float groupMin) {
            int seen = 0;
            int lookAhead = Math.Max(0, Preferences.Default.WaveformSmartLookAhead);
            float returnSlop = (float)Preferences.Default.WaveformSmartReturnSlop;
            for (int j = i + 1;
                j < notes.Count && notes[j - 1].End >= notes[j].position && seen < lookAhead;
                j++, seen++) {
                if (notes[j].AdjustedTone <= groupMin + returnSlop) {
                    return true;
                }
            }
            return false;
        }

        static bool ShouldSplitSmart(List<UNote> notes, int i, float groupMin) {
            float tone = notes[i].AdjustedTone;
            float splitDown = (float)Preferences.Default.WaveformSmartSplitDown;
            float splitUp = (float)Preferences.Default.WaveformSmartSplitUp;
            if (tone <= groupMin - splitDown) {
                return !ReturnsToFloor(notes, i, groupMin);
            }
            if (tone >= groupMin + splitUp) {
                return !ReturnsToFloor(notes, i, groupMin);
            }
            return false;
        }

        void DrawFollowGroup(NotesViewModel viewModel, UProject project, UVoicePart part,
            List<UNote> notes, int start, int end, float minTone,
            int[] data, int stride, int drawWidth, int drawHeight,
            double scale, double leftTick, double rightTick) {
            float bandTone = minTone - 0.5f - (float)Preferences.Default.WaveformFollowOffsetRows;
            bool gradient = Preferences.Default.WaveformStyle == 0;
            bool splitPrev = start > 0 && notes[start - 1].End >= notes[start].position;
            bool splitNext = end < notes.Count && notes[end - 1].End >= notes[end].position;
            for (int n = start; n < end; n++) {
                var note = notes[n];
                NoteWaveRange(project, part, note, out int waveStart, out int waveEnd,
                    out bool fadeIn, out int fadeOutTicks);
                if (n > start) {
                    waveStart = Math.Max(waveStart, note.position);
                    fadeIn = false;
                } else if (gradient && splitPrev) {
                    int fadeInTicks = MsDeltaTicks(project, note.PositionMs,
                        Math.Max(0, Preferences.Default.WaveformFadeInMs));
                    waveStart = note.position - fadeInTicks;
                    fadeIn = true;
                }
                if (n < end - 1) {
                    waveEnd = Math.Min(waveEnd, note.End);
                    fadeOutTicks = 0;
                } else if (gradient && splitNext) {
                    fadeOutTicks = Math.Min(
                        MsDeltaTicks(project, note.EndMs, Math.Max(0, Preferences.Default.WaveformFadeOutMs)),
                        Math.Max(1, note.duration / 2));
                    waveEnd = Math.Min(waveEnd, note.End);
                }
                if (waveEnd < leftTick || waveStart > rightTick) {
                    continue;
                }
                int x0 = (int)Math.Floor((waveStart - viewModel.TickOffset) * viewModel.TickWidth);
                int x1 = (int)Math.Ceiling((waveEnd - viewModel.TickOffset) * viewModel.TickWidth);
                x0 = Math.Clamp(x0, 0, drawWidth);
                x1 = Math.Clamp(x1, 0, drawWidth);
                if (x1 <= x0) {
                    continue;
                }
                double yCenter = viewModel.TickToneToPoint(note.position, bandTone).Y;
                double fadeInSpan = note.position - waveStart;
                for (int x = x0; x < x1; x++) {
                    if (colMin[x] == 0 && colMax[x] == 0) {
                        continue;
                    }
                    double tick = viewModel.TickOffset + x / viewModel.TickWidth;
                    float alpha = 1f;
                    if (fadeIn && fadeInSpan > 0 && tick < note.position) {
                        alpha = (float)Math.Clamp((tick - waveStart) / fadeInSpan, 0, 1);
                    }
                    if (fadeOutTicks > 0 && tick > note.End - fadeOutTicks) {
                        alpha *= (float)Math.Clamp((note.End - tick) / fadeOutTicks, 0, 1);
                    }
                    if (alpha <= 0) {
                        continue;
                    }
                    int y1 = (int)Math.Round(yCenter - colMax[x] * scale);
                    int y2 = (int)Math.Round(yCenter - colMin[x] * scale);
                    DrawPeak(data, stride, drawHeight, x, y1, y2, alpha);
                }
            }
        }

        static int LeadTicks(UProject project, UVoicePart part, UNote note) {
            var first = FirstPhoneme(part, note);
            if (first != null && first.preutter > 0) {
                return MsDeltaTicks(project, first.PositionMs, first.preutter);
            }
            return MsDeltaTicks(project, note.PositionMs, Preferences.Default.WaveformFallbackLeadMs);
        }

        static int TailTicks(UProject project, UVoicePart part, UNote note) {
            var last = LastPhoneme(part, note);
            if (last != null && last.envelope.data.Count >= 5) {
                int p4 = project.timeAxis.MsPosToTickPos(last.PositionMs + last.envelope.data[4].X) - part.position;
                if (p4 > note.End) {
                    return p4 - note.End;
                }
            }
            return MsDeltaTicks(project, note.EndMs, Preferences.Default.WaveformFallbackLeadMs);
        }


        static int MsDeltaTicks(UProject project, double atMs, double deltaMs) {
            return Math.Max(0, project.timeAxis.MsPosToTickPos(atMs) - project.timeAxis.MsPosToTickPos(atMs - deltaMs));
        }

        void DrawPeak(int[] data, int width, int height, int x, int y1, int y2, float alpha) {
            if (x < 0 || x >= width || alpha <= 0) {
                return;
            }
            if (y1 > y2) {
                int temp = y2;
                y2 = y1;
                y1 = temp;
            }
            if (y2 < 0 || y1 >= height) {
                return;
            }
            y1 = Math.Max(0, y1);
            y2 = Math.Min(height - 1, y2);
            int maxAlpha = Math.Clamp(
                (int)Math.Round(Math.Clamp(Preferences.Default.WaveformAlphaPercent, 0, 100) * 2.55), 0, 255);
            int a = Math.Clamp((int)(maxAlpha * alpha), 0, 255);
            int color = waveformRgb | (a << 24);
            for (var y = y1; y <= y2; ++y) {
                data[x + width * y] = color;
            }
        }

        static int PackRgb(Color color) {
            // Rgba8888 little-endian: R, G, B, A
            return color.R | (color.G << 8) | (color.B << 16);
        }
    }

}
