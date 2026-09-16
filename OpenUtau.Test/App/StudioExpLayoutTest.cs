using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using OpenUtau.App.Studio;
using Xunit;

namespace OpenUtau.App {
    [Collection(StudioUiSerialCollection.Name)]
    public class StudioExpLayoutTest {
        static List<Control> Slots(int count = 10) =>
            Enumerable.Range(0, count).Select(_ => (Control)new Button { IsVisible = true }).ToList();

        [AvaloniaFact]
        public void Overflow_HidesSelectorsThatDoNotFit() {
            using var scope = new StudioUiScope(true);
            var selectors = Slots();
            StudioExpLayout.ApplyOverflow(180, selectors);
            Assert.True(selectors[0].IsVisible);
            Assert.True(selectors[1].IsVisible);
            Assert.True(selectors[2].IsVisible);
            Assert.False(selectors[3].IsVisible);
            Assert.False(selectors[9].IsVisible);
        }

        [AvaloniaFact]
        public void Overflow_KeepsPrimaryAndSecondaryVisible() {
            using var scope = new StudioUiScope(true);
            var selectors = Slots();
            StudioExpLayout.ApplyOverflow(180, selectors, primary: 9, secondary: 8);
            Assert.True(selectors[0].IsVisible);
            Assert.True(selectors[8].IsVisible);
            Assert.True(selectors[9].IsVisible);
            Assert.False(selectors[1].IsVisible);
            Assert.False(selectors[7].IsVisible);
        }

        [AvaloniaFact]
        public void Overflow_KeepsAtLeastPrimary() {
            using var scope = new StudioUiScope(true);
            var selectors = Slots();
            StudioExpLayout.ApplyOverflow(10, selectors, primary: 7, secondary: 2);
            Assert.True(selectors[7].IsVisible);
            Assert.False(selectors[0].IsVisible);
            Assert.False(selectors[2].IsVisible);
        }

        [AvaloniaFact]
        public void Overflow_ShowsAllWhenWidthAllows() {
            using var scope = new StudioUiScope(true);
            var selectors = Slots();
            StudioExpLayout.ApplyOverflow(600, selectors, primary: 9, secondary: 8);
            Assert.All(selectors, selector => Assert.True(selector.IsVisible));
        }

        [AvaloniaFact]
        public void Overflow_StudioOff_DoesNotHide() {
            using var scope = new StudioUiScope(false);
            var selectors = Slots();
            StudioExpLayout.ApplyOverflow(60, selectors);
            Assert.All(selectors, selector => Assert.True(selector.IsVisible));
        }

        [AvaloniaFact]
        public void Apply_Studio_MovesToBottomRowHorizontal() {
            var panel = new Border();
            var stack = new StackPanel { Orientation = Orientation.Vertical };
            var gear = new Button();
            var status = new TextBlock { IsVisible = false };
            var selectors = Slots();
            selectors[4].IsVisible = false;

            StudioExpLayout.Apply(panel, stack, gear, status, selectors, studio: true);

            Assert.Equal(StudioExpLayout.StudioRow, Grid.GetRow(panel));
            Assert.Equal(StudioExpLayout.StudioColumnSpan, Grid.GetColumnSpan(panel));
            Assert.Equal(StudioExpLayout.BarHeight, panel.Height);
            Assert.Equal(Dock.Right, DockPanel.GetDock(gear));
            Assert.Equal(Dock.Right, DockPanel.GetDock(status));
            Assert.True(status.IsVisible);
            Assert.Equal(Orientation.Horizontal, stack.Orientation);
            Assert.False(selectors[4].IsVisible);
        }

        [AvaloniaFact]
        public void Apply_Classic_RestoresLeftRailAndShowsAll() {
            var panel = new Border { Height = StudioExpLayout.BarHeight };
            var stack = new StackPanel { Orientation = Orientation.Horizontal };
            var gear = new Button();
            var status = new TextBlock { IsVisible = true };
            DockPanel.SetDock(gear, Dock.Right);
            Grid.SetRow(panel, StudioExpLayout.StudioRow);
            Grid.SetColumnSpan(panel, StudioExpLayout.StudioColumnSpan);
            var selectors = Slots();
            selectors[7].IsVisible = false;

            StudioExpLayout.Apply(panel, stack, gear, status, selectors, studio: false);

            Assert.Equal(StudioExpLayout.ClassicRow, Grid.GetRow(panel));
            Assert.Equal(StudioExpLayout.ClassicColumnSpan, Grid.GetColumnSpan(panel));
            Assert.True(double.IsNaN(panel.Height));
            Assert.Equal(Dock.Bottom, DockPanel.GetDock(gear));
            Assert.False(status.IsVisible);
            Assert.Equal(Orientation.Vertical, stack.Orientation);
            Assert.All(selectors, selector => Assert.True(selector.IsVisible));
        }

        [AvaloniaFact]
        public void ApplyCurveToolbar_Studio_PlacesTwoColumnGridInGutter() {
            var toolbar = new ListBox();
            Grid.SetColumn(toolbar, StudioExpLayout.ClassicToolbarColumn);
            toolbar.HorizontalAlignment = HorizontalAlignment.Left;
            toolbar.VerticalAlignment = VerticalAlignment.Top;

            StudioExpLayout.ApplyCurveToolbar(toolbar, studio: true);

            Assert.Equal(StudioExpLayout.StudioToolbarColumn, Grid.GetColumn(toolbar));
            Assert.Equal(HorizontalAlignment.Stretch, toolbar.HorizontalAlignment);
            Assert.Equal(VerticalAlignment.Top, toolbar.VerticalAlignment);
            var grid = Assert.IsType<UniformGrid>(toolbar.ItemsPanel.Build());
            Assert.Equal(StudioExpLayout.ToolbarColumns, grid.Columns);
        }

        [AvaloniaFact]
        public void ApplyCurveToolbar_Classic_RestoresHorizontalOverlay() {
            var toolbar = new ListBox();
            StudioExpLayout.ApplyCurveToolbar(toolbar, studio: true);
            StudioExpLayout.ApplyCurveToolbar(toolbar, studio: false);

            Assert.Equal(StudioExpLayout.ClassicToolbarColumn, Grid.GetColumn(toolbar));
            Assert.Equal(HorizontalAlignment.Left, toolbar.HorizontalAlignment);
            Assert.Equal(VerticalAlignment.Top, toolbar.VerticalAlignment);
            var stack = Assert.IsType<StackPanel>(toolbar.ItemsPanel.Build());
            Assert.Equal(Orientation.Horizontal, stack.Orientation);
        }
    }
}
