using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using OpenUtau.App.Studio;
using OpenUtau.App.ViewModels;
using OpenUtau.Core.Ustx;

namespace OpenUtau.App.Controls {
    public partial class ExpSelector : UserControl {
        public static readonly DirectProperty<ExpSelector, int> IndexProperty =
            AvaloniaProperty.RegisterDirect<ExpSelector, int>(
                nameof(Index),
                o => o.Index,
                (o, v) => o.Index = v);

        public int Index {
            get => index;
            set => SetAndRaise(IndexProperty, ref index, value);
        }

        private int index;


        public ExpSelector() {
            InitializeComponent();
            DataContext = new ExpSelectorViewModel();
            ((ExpSelectorViewModel)DataContext!).Index = Index;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            if (change.Property == IndexProperty) {
                ((ExpSelectorViewModel)DataContext!).Index = Index;
            }
        }

        private void TextBlockPointerPressed(object sender, PointerPressedEventArgs e) {
            var vm = (ExpSelectorViewModel)DataContext!;
            bool wasVisible = vm.DisplayMode == ExpDisMode.Visible;
            vm.OnSelected(true);
            bool usesNotesOverlay = vm.Descriptor?.type != UExpressionType.Options;
            StudioExpLayout.HandleSelectorClick(wasVisible, usesNotesOverlay);
        }

        public void SelectExp() {
            ((ExpSelectorViewModel)DataContext!).OnSelected(false);
        }
    }
}
