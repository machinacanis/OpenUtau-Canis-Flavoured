using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OpenUtau.App.Controls;
using OpenUtau.App.ViewModels;
using Xunit;

namespace OpenUtau.App {
    [Collection(StudioUiSerialCollection.Name)]
    public class TrackHeaderChromeTest {
        // Keeps the hosting windows alive; the header is only reachable through them.
        static readonly List<Window> hosts = new();

        static TrackHeader ShowHeader(double trackHeight) {
            var header = new TrackHeader {
                DataContext = new TrackHeaderViewModel(),
                TrackHeight = trackHeight,
                Height = trackHeight,
            };
            var window = new Window {
                Width = 300,
                Height = (int)trackHeight,
                Content = header,
            };
            hosts.Add(window);
            // Measure/arrange instead of Show(): a render pass would attach the
            // shared static brushes (ThemeManager, StudioTrackPaintCache) to this
            // test's compositor, and those were created by another app instance.
            window.Measure(new Size(300, trackHeight));
            window.Arrange(new Rect(0, 0, 300, trackHeight));
            Dispatcher.UIThread.RunJobs();
            return header;
        }

        static Border ColorBar(TrackHeader header) =>
            header.GetVisualDescendants().OfType<Border>().First(border => border.Name == "TrackColorBar");

        static T Find<T>(TrackHeader header, string name) where T : Control =>
            header.GetVisualDescendants().OfType<T>().First(control => control.Name == name);

        [AvaloniaFact]
        public void ColorBar_WidthFollowsStudioUI() {
            using (new StudioUiScope(true)) {
                Assert.Equal(6, ColorBar(ShowHeader(105)).Width);
            }
            using (new StudioUiScope(false)) {
                Assert.Equal(0, ColorBar(ShowHeader(105)).Width);
            }
        }

        [AvaloniaFact]
        public void MuteSolo_OptIntoTheStudioSkin_FxDoesNot() {
            using var skin = new StudioSkinScope();
            var header = ShowHeader(63);

            var mute = Find<ToggleButton>(header, "MuteButton");
            var solo = Find<ToggleButton>(header, "SoloButton");
            var fx = Find<Button>(header, "FxButton");
            Assert.Contains("s1", mute.Classes);
            Assert.Contains("mute", mute.Classes);
            Assert.Contains("s1", solo.Classes);
            Assert.Contains("solo", solo.Classes);

            Assert.NotNull(StudioSkinTestHelpers.Chrome(mute));
            Assert.NotNull(StudioSkinTestHelpers.Chrome(solo));

            // Fx keeps the classic transparent look on purpose.
            Assert.DoesNotContain("s1", fx.Classes);
            Assert.Null(StudioSkinTestHelpers.Chrome(fx));
        }

        [AvaloniaFact]
        public void TrackNoBadge_SitsBesideTheName_NotOverTheAvatar() {
            var header = ShowHeader(105);
            var badge = header.GetVisualDescendants().OfType<Border>()
                .First(border => border.Name == "TrackNoBadge");
            var avatarPanel = header.GetVisualDescendants().OfType<Panel>()
                .First(panel => panel.Name == "AvatarPanel");
            var name = Find<Button>(header, "TrackNameButton");

            Assert.DoesNotContain(badge, avatarPanel.GetVisualDescendants());
            Assert.Same(badge.Parent, name.Parent);
        }

        [AvaloniaFact]
        public void HeaderButtons_FitCompactTrackHeight() {
            var header = ShowHeader(63);
            var buttons = header.GetVisualDescendants().OfType<StackPanel>()
                .First(panel => panel.Name == "HeaderButtons");
            // 63 - 2 (Border margin) - 2 (BorderThickness) = 59px of content.
            Assert.True(buttons.DesiredSize.Height <= 59,
                $"header buttons need {buttons.DesiredSize.Height}px, only 59px available");
        }
    }
}
