using Avalonia.Controls;
using OpenUtau.Core.Ustx;

namespace OpenUtau.App.Views {
    /// <summary>
    /// Owns the single, modeless Track Polish window. Showing it non-modally
    /// (instead of <c>ShowDialog</c>) keeps the main window interactive while
    /// the FX window is open. The window is closed when its track is removed,
    /// the project is replaced, or the main window closes.
    /// </summary>
    static class MixFxWindowManager {
        private static MixFxDialog? window;
        private static UTrack? track;

        public static void Open(Window owner, UTrack target) {
            if (window != null) {
                if (ReferenceEquals(track, target) && window.IsVisible) {
                    window.Activate();
                    return;
                }
                CloseAll();
            }
            var dialog = new MixFxDialog(target);
            window = dialog;
            track = target;
            dialog.Closed += (_, _) => {
                if (ReferenceEquals(window, dialog)) {
                    window = null;
                    track = null;
                }
            };
            dialog.Show(owner);
        }

        public static void CloseFor(UTrack target) {
            if (ReferenceEquals(track, target)) {
                CloseAll();
            }
        }

        public static void CloseAll() {
            var dialog = window;
            window = null;
            track = null;
            dialog?.Close();
        }
    }
}
