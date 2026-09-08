using Avalonia.Headless.XUnit;
using OpenUtau.App.Studio;
using OpenUtau.App.ViewModels;
using ReactiveUI;
using Xunit;

namespace OpenUtau.App {
    [Collection(StudioUiSerialCollection.Name)]
    public class StudioTrackLayoutTest {
        [AvaloniaFact]
        public void StudioOn_UsesCompactHeightAndColorBar() {
            using var scope = new StudioUiScope(true);
            Assert.Equal(63, StudioTrackLayout.DefaultTrackHeight);
            Assert.Equal(63, StudioTrackLayout.CompactTrackHeight);
            Assert.Equal(6, StudioTrackLayout.ColorBarWidth);
        }

        [AvaloniaFact]
        public void StudioOff_UsesClassicHeightAndNoColorBar() {
            using var scope = new StudioUiScope(false);
            Assert.Equal(105, StudioTrackLayout.DefaultTrackHeight);
            Assert.Equal(0, StudioTrackLayout.ColorBarWidth);
        }

        [AvaloniaFact]
        public void CompactHeight_IsTwoWholeZoomStepsBelowClassic() {
            // 105 -> 84 -> 63: a single step still shows the phonemizer row.
            Assert.Equal(0, (105 - StudioTrackLayout.CompactTrackHeight) % 21);
            Assert.Equal(2, (105 - StudioTrackLayout.CompactTrackHeight) / 21);
        }

        [AvaloniaFact]
        public void TracksViewModel_StartsAtModeDefault() {
            using (new StudioUiScope(true)) {
                Assert.Equal(63, new TracksViewModel().TrackHeight);
            }
            using (new StudioUiScope(false)) {
                Assert.Equal(105, new TracksViewModel().TrackHeight);
            }
        }

        [AvaloniaFact]
        public void TracksViewModel_FollowsDefaultWhenUserHasNotZoomed() {
            using var scope = new StudioUiScope(false);
            var vm = new TracksViewModel();
            Assert.Equal(105, vm.TrackHeight);

            using (new StudioUiScope(true)) {
                MessageBus.Current.SendMessage(new StudioUIChangedEvent());
            }
            Assert.Equal(63, vm.TrackHeight);
        }

        [AvaloniaFact]
        public void TracksViewModel_KeepsUserZoomOnStudioToggle() {
            using var scope = new StudioUiScope(false);
            var vm = new TracksViewModel();
            vm.TrackHeight = 84;

            using (new StudioUiScope(true)) {
                MessageBus.Current.SendMessage(new StudioUIChangedEvent());
            }
            Assert.Equal(84, vm.TrackHeight);
        }
    }
}
