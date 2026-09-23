using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Media;
using OpenUtau.App.Studio;
using OpenUtau.App.Controls;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;
using ReactiveUI;
using ReactiveUI.Primitives;
using ReactiveUI.SourceGenerators;

namespace OpenUtau.App.ViewModels {
    public partial class ExpSelectorViewModel : ViewModelBase, ICmdSubscriber {
        [Reactive] public partial int Index { get; set; }
        [Reactive] public partial int SelectedIndex { get; set; }
        [Reactive] public partial ExpDisMode DisplayMode { get; set; }
        [Reactive] public partial UExpressionDescriptor? Descriptor { get; set; }
        public string Abbr {
            get{
                if (Descriptor == null) {
                    return "";
                }
                return Descriptor.abbr;
            }
        }
        public ObservableCollection<UExpressionDescriptor> Descriptors => descriptors;
        public string Header => header.Value;
        [Reactive] public partial IBrush TagBrush { get; set; }
        [Reactive] public partial IBrush Background { get; set; }
        [Reactive] public partial IBrush ColorBrush { get; set; }

        ObservableCollection<UExpressionDescriptor> descriptors = new ObservableCollection<UExpressionDescriptor>();
        ObservableAsPropertyHelper<string> header;

        public ExpSelectorViewModel() {
            DocManager.Inst.AddSubscriber(this);
            this.WhenAnyValue(x => x.DisplayMode)
                .Subscribe(_ => RefreshBrushes());
            this.WhenAnyValue(x => x.Descriptor)
                .Select(descriptor => descriptor == null ? string.Empty : descriptor.abbr.ToUpperInvariant())
                .ToProperty(this, x => x.Header, out header);
            this.WhenAnyValue(x => x.Descriptor)
                .OfType<UExpressionDescriptor>()
                .Subscribe(SelectionChanged);
            this.WhenAnyValue(x => x.Index, x => x.Descriptors)
                .Subscribe(tuple => {
                    SetExp(DocManager.Inst.Project.expSelectors[tuple.Item1]);
                });
            MessageBus.Current.Listen<ThemeChangedEvent>()
                .Subscribe(_ => {
                    RefreshBrushes();
                    RefreshColor();
                });
            MessageBus.Current.Listen<StudioTrackPaletteChangedEvent>()
                .Subscribe(_ => RefreshColor());
            TagBrush = ThemeManager.ExpNameBrush;
            Background = ThemeManager.ExpBrush;
            ColorBrush = StudioExpColors.Square(null);
            OnListChange();
        }

        public bool SetExp(string abbr) {
            if(Descriptors.Any(d => d.abbr == abbr)) {
                Descriptor = Descriptors.First(d => d.abbr == abbr);
                return true;
            } else {
                if (Descriptors != null && Descriptors.Count > Index) {
                    Descriptor = Descriptors[Index];
                }
                return false;
            }
        }

        public void OnSelected(bool store, bool updateShadow = true) {
            if (DisplayMode != ExpDisMode.Visible && Descriptor != null) {
                DocManager.Inst.ExecuteCmd(new SelectExpressionNotification(Descriptor.abbr, Index, updateShadow));
            }
            if (store) {
                var project = DocManager.Inst.Project;
                if (updateShadow) {
                    project.expSecondary = project.expPrimary;
                }
                project.expPrimary = Index;
            }
        }

        void SelectionChanged(UExpressionDescriptor? descriptor) {
            RefreshColor();
            if (descriptor != null) {
                bool updateShadow = DisplayMode != ExpDisMode.Visible && !StudioUI.IsEnabled;
                DocManager.Inst.ExecuteCmd(new SelectExpressionNotification(descriptor.abbr, Index, updateShadow));
            }
            if (!string.IsNullOrEmpty(Abbr)) {
                DocManager.Inst.Project.expSelectors[Index] = Abbr;
            }
        }

        public void OnNext(UCommand cmd, bool isUndo) {
            if (cmd is LoadProjectNotification ||
                cmd is LoadPartNotification ||
                cmd is ConfigureExpressionsCommand) {
                OnListChange();
            } else if (cmd is SelectExpressionNotification) {
                OnSelectExp((SelectExpressionNotification)cmd);
            }
        }

        private void OnListChange() {
            var selectedIndex = SelectedIndex;
            Descriptors.Clear();
            DocManager.Inst.Project.expressions.Values.ToList().ForEach(Descriptors.Add);
            if (selectedIndex >= descriptors.Count) {
                selectedIndex = Index;
            }
            SelectedIndex = selectedIndex;
            RefreshColor();
        }

        private void OnSelectExp(SelectExpressionNotification cmd) {
            if (Descriptors.Count == 0) {
                return;
            }
            if (cmd.SelectorIndex == Index) {
                if (Descriptors[SelectedIndex].abbr != cmd.ExpKey) {
                    SelectedIndex = Descriptors.IndexOf(Descriptors.First(d => d.abbr == cmd.ExpKey));
                }
                DisplayMode = ExpDisMode.Visible;
            } else if (StudioUI.IsEnabled) {
                if (DisplayMode != ExpDisMode.Hidden) {
                    DisplayMode = ExpDisMode.Hidden;
                }
            } else if (cmd.UpdateShadow) {
                DisplayMode = DisplayMode == ExpDisMode.Visible ? ExpDisMode.Shadow : ExpDisMode.Hidden;
            }
        }

        private void RefreshBrushes() {
            TagBrush = DisplayMode == ExpDisMode.Visible
                    ? ThemeManager.ExpActiveNameBrush
                    : DisplayMode == ExpDisMode.Shadow
                    ? ThemeManager.ExpShadowNameBrush
                    : ThemeManager.ExpNameBrush;
            Background = DisplayMode == ExpDisMode.Visible
                    ? ThemeManager.ExpActiveBrush
                    : DisplayMode == ExpDisMode.Shadow
                    ? ThemeManager.ExpShadowBrush
                    : ThemeManager.ExpBrush;
        }

        void RefreshColor() {
            ColorBrush = StudioExpColors.Square(Descriptor?.abbr);
        }
    }
}
