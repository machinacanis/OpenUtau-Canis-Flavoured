using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OpenUtau.App.ViewModels;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;
using ReactiveUI;

namespace OpenUtau.App.Views {
    public partial class MixFxDialog : Window {
        readonly MixFxViewModel viewModel;
        readonly UTrack? track;

        public MixFxDialog() : this(null) { }

        public MixFxDialog(UTrack? track) {
            InitializeComponent();
            this.track = track;
            DataContext = viewModel = new MixFxViewModel(track);
            viewModel.AskForName = PromptForNameAsync;
            AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
            KeyDown += OnForwardKeyDown;
        }

        // Space always drives the transport here, even when a preset ComboBox
        // has focus. Intercepting on the tunnel route stops the ComboBox (or a
        // checkbox/button) from consuming Space and reopening its dropdown.
        void OnPreviewKeyDown(object? sender, KeyEventArgs args) {
            if (args.Key == Key.Space && args.KeyModifiers == KeyModifiers.None) {
                if (Owner is MainWindow mainWindow) {
                    mainWindow.HandleGlobalShortcut(args);
                }
            }
        }

        // The window is modeless, so keep the main window's global shortcuts
        // (Ctrl+Z/S, ...) working while this window has focus. KeyDown only
        // receives keys no focused control consumed.
        void OnForwardKeyDown(object? sender, KeyEventArgs args) {
            if (args.Key == Key.F4 && args.KeyModifiers == KeyModifiers.Alt) {
                // Let the OS close this window instead of shutting down the app.
                return;
            }
            if (Owner is MainWindow mainWindow) {
                mainWindow.HandleGlobalShortcut(args);
            }
        }

        Task<string?> PromptForNameAsync() {
            var tcs = new TaskCompletionSource<string?>();
            var dialog = new TypeInDialog();
            dialog.Title = ThemeManager.GetString("mixfx.library.save");
            dialog.SetText(string.Empty);
            string? captured = null;
            dialog.onFinish = name => {
                if (!string.IsNullOrWhiteSpace(name)) captured = name;
            };
            dialog.Closed += (_, __) => tcs.TrySetResult(captured);
            dialog.ShowDialog(this);
            return tcs.Task;
        }

        void Apply() {
            viewModel.Apply();
            if (track != null) {
                MessageBus.Current.SendMessage(new MixFxChangedNotification(track.TrackNo));
            }
            // Re-render from the current position so the change is audible
            // immediately while the window stays open for further tweaking.
            if (PlaybackManager.Inst.PlayingMaster) {
                PlaybackManager.Inst.Play(DocManager.Inst.Project, DocManager.Inst.playPosTick);
            }
        }

        void OnApplyClicked(object sender, RoutedEventArgs e) {
            Apply();
        }

        void OnOkClicked(object sender, RoutedEventArgs e) {
            Apply();
            Close();
        }

        void OnCancelClicked(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
