using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace OpenUtau.App {
    /// <summary>
    /// The Studio One button skin must be reusable: it is driven only by the
    /// <c>s1</c> class and works on any Button / ToggleButton, not just the
    /// track header's M / S / Fx.
    /// </summary>
    [Collection(StudioUiSerialCollection.Name)]
    public class StudioOneControlsTest {
        // Keeps the hosting windows alive; the control is only reachable through them.
        static readonly List<Window> hosts = new();

        static Window Host(Control content) {
            var window = new Window {
                Width = 200,
                Height = 100,
                Content = content,
            };
            hosts.Add(window);
            // Measure/arrange instead of Show(): a render pass would attach
            // process-wide static brushes to this test's compositor, and those
            // belong to another app instance.
            window.Measure(new Size(200, 100));
            window.Arrange(new Rect(0, 0, 200, 100));
            Dispatcher.UIThread.RunJobs();
            return window;
        }

        static ToggleButton RoleToggle(string role, out Path glyph) {
            glyph = new Path { Data = Geometry.Parse("M0,0 L10,0 L10,10 Z") };
            glyph.Classes.Add("filled");
            var button = new ToggleButton { Content = glyph, IsChecked = true };
            button.Classes.Add("s1");
            button.Classes.Add(role);
            return button;
        }

        [AvaloniaFact]
        public void SoloRole_Checked_UsesAmberChrome() {
            using var skin = new StudioSkinScope();
            var button = RoleToggle("solo", out var glyph);
            Host(button);
            var chrome = StudioSkinTestHelpers.Chrome(button);
            Assert.NotNull(chrome);
            StudioSkinTestHelpers.AssertResourceColor("TrackSoloBrush", chrome!.Background);
            StudioSkinTestHelpers.AssertResourceColor("TrackSoloBorderBrush", chrome.BorderBrush);
            StudioSkinTestHelpers.AssertResourceColor("OnTrackSoloBrush", glyph.Fill);
        }

        [AvaloniaFact]
        public void MuteRole_Checked_UsesRedChrome() {
            using var skin = new StudioSkinScope();
            var button = RoleToggle("mute", out var glyph);
            Host(button);
            var chrome = StudioSkinTestHelpers.Chrome(button);
            Assert.NotNull(chrome);
            StudioSkinTestHelpers.AssertResourceColor("TrackMuteBrush", chrome!.Background);
            StudioSkinTestHelpers.AssertResourceColor("TrackMuteBorderBrush", chrome.BorderBrush);
            StudioSkinTestHelpers.AssertResourceColor("OnTrackMuteBrush", glyph.Fill);
        }

        [AvaloniaFact]
        public void NoRole_Checked_UsesThemeAccent() {
            using var skin = new StudioSkinScope();
            var button = new ToggleButton { Content = "x", IsChecked = true };
            button.Classes.Add("s1");
            Host(button);
            var chrome = StudioSkinTestHelpers.Chrome(button);
            Assert.NotNull(chrome);
            StudioSkinTestHelpers.AssertResourceColor("AccentBrush2", chrome!.Background);
        }

        [AvaloniaFact]
        public void FxButton_On_UsesThemeAccent() {
            using var skin = new StudioSkinScope();
            var button = new Button { Content = "fx" };
            button.Classes.Add("s1");
            button.Classes.Add("fxOn");
            Host(button);
            var chrome = StudioSkinTestHelpers.Chrome(button);
            Assert.NotNull(chrome);
            StudioSkinTestHelpers.AssertResourceColor("AccentBrush2", chrome!.Background);
        }

        [AvaloniaFact]
        public void WithoutS1Class_KeepsDefaultChrome() {
            using var skin = new StudioSkinScope();
            var button = new ToggleButton { Content = "M", IsChecked = true };
            Host(button);
            Assert.Null(StudioSkinTestHelpers.Chrome(button));
        }

        [AvaloniaFact]
        public void Unchecked_KeepsNeutralChrome() {
            using var skin = new StudioSkinScope();
            var button = new ToggleButton { Content = "M" };
            button.Classes.Add("s1");
            button.Classes.Add("mute");
            Host(button);
            var chrome = StudioSkinTestHelpers.Chrome(button);
            Assert.NotNull(chrome);
            StudioSkinTestHelpers.AssertResourceColor("NeutralAccentBrush", chrome!.Background);
            Assert.Equal(1, chrome.BorderThickness.Left);
            Assert.Equal(3, chrome.CornerRadius.TopLeft);
        }

        /// <summary>
        /// The skin must be reachable through the Studio chrome include chain
        /// (StudioStyles.axaml -> StudioOneControls.axaml), which is what gates
        /// it to Studio UI mode.
        /// </summary>
        [AvaloniaFact]
        public void StudioStyles_IncludesTheSkin() {
            var include = new StyleInclude(new Uri("avares://OpenUtau/App.axaml")) {
                Source = new Uri("avares://OpenUtau/Styles/StudioStyles.axaml"),
            };
            Application.Current!.Styles.Add(include);
            try {
                var button = new ToggleButton { Content = "M" };
                button.Classes.Add("s1");
                Host(button);
                Assert.NotNull(StudioSkinTestHelpers.Chrome(button));
            } finally {
                Application.Current.Styles.Remove(include);
            }
        }
    }
}
