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
        public void Overflow_KeepsPrimaryVisible() {
            using var scope = new StudioUiScope(true);
            var selectors = Slots();
            StudioExpLayout.ApplyOverflow(180, selectors, primary: 9);
            Assert.True(selectors[0].IsVisible);
            Assert.True(selectors[1].IsVisible);
            Assert.True(selectors[9].IsVisible);
            Assert.False(selectors[8].IsVisible);
            Assert.False(selectors[2].IsVisible);
        }

        [AvaloniaFact]
        public void Overflow_KeepsAtLeastPrimary() {
            using var scope = new StudioUiScope(true);
            var selectors = Slots();
            StudioExpLayout.ApplyOverflow(10, selectors, primary: 7);
            Assert.True(selectors[7].IsVisible);
            Assert.False(selectors[0].IsVisible);
            Assert.False(selectors[2].IsVisible);
        }

        [AvaloniaFact]
        public void Overflow_ShowsAllWhenWidthAllows() {
            using var scope = new StudioUiScope(true);
            var selectors = Slots();
            StudioExpLayout.ApplyOverflow(600, selectors, primary: 9);
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

        [AvaloniaFact]
        public void ApplyOverlay_StudioClosed_CollapsesExpRowAndHidesCanvases() {
            var parts = OverlayParts();
            ApplyOverlay(parts, studio: true, open: false);

            Assert.Equal(0, parts.Row.Height.Value);
            Assert.Equal(0, parts.Row.MinHeight);
            Assert.Equal(0, parts.Row.MaxHeight);
            Assert.Equal(0, parts.SplitterRow.Height.Value);
            Assert.Equal(0, parts.Splitter.Height);
            Assert.False(parts.Splitter.IsVisible);
            Assert.False(parts.Dim.IsVisible);
            Assert.False(parts.Primary.IsVisible);
            Assert.False(parts.Secondary.IsVisible);
            Assert.False(parts.Toolbar.IsVisible);
        }

        [AvaloniaFact]
        public void ApplyOverlay_StudioOpen_PlacesPrimaryOnNotesRowHidesSecondary() {
            var parts = OverlayParts();
            ApplyOverlay(parts, studio: true, open: true);

            Assert.Equal(0, parts.Row.Height.Value);
            Assert.Equal(0, parts.SplitterRow.Height.Value);
            Assert.Equal(StudioExpLayout.NotesRow, Grid.GetRow(parts.Primary));
            Assert.Equal(StudioExpLayout.NotesRow, Grid.GetRow(parts.Toolbar));
            Assert.Equal(StudioExpLayout.StudioToolbarColumn, Grid.GetColumn(parts.Toolbar));
            Assert.True(parts.Dim.IsVisible);
            Assert.True(parts.Primary.IsVisible);
            Assert.False(parts.Secondary.IsVisible);
            Assert.Equal(StudioExpLayout.OverlayZIndex + 1, parts.Primary.ZIndex);
        }

        [AvaloniaFact]
        public void ApplyOverlay_Classic_RestoresExpRow() {
            var parts = OverlayParts();
            ApplyOverlay(parts, studio: true, open: true);
            ApplyOverlay(parts, studio: false, open: false);

            Assert.Equal(StudioExpLayout.ClassicExpRowHeight, parts.Row.Height.Value);
            Assert.Equal(StudioExpLayout.ClassicExpRowMinHeight, parts.Row.MinHeight);
            Assert.True(parts.SplitterRow.Height.IsAuto);
            Assert.Equal(StudioExpLayout.ClassicRow, Grid.GetRow(parts.Primary));
            Assert.Equal(StudioExpLayout.ClassicRow, Grid.GetRow(parts.Secondary));
            Assert.Equal(StudioExpLayout.ClassicRow, Grid.GetRow(parts.Toolbar));
            Assert.False(parts.Dim.IsVisible);
        }

        [AvaloniaFact]
        public void ApplyOverlay_StudioOptions_RestoresBottomStrip() {
            var parts = OverlayParts();
            ApplyOverlay(parts, studio: true, open: true, usesNotesOverlay: false);

            Assert.Equal(StudioExpLayout.ClassicExpRowHeight, parts.Row.Height.Value);
            Assert.Equal(StudioExpLayout.ClassicExpRowMinHeight, parts.Row.MinHeight);
            Assert.True(parts.SplitterRow.Height.IsAuto);
            Assert.Equal(10, parts.Splitter.Height);
            Assert.Equal(StudioExpLayout.ClassicRow, Grid.GetRow(parts.Primary));
            Assert.Equal(StudioExpLayout.ClassicRow, Grid.GetRow(parts.Secondary));
            Assert.False(parts.Secondary.IsVisible);
            Assert.False(parts.Dim.IsVisible);
        }

        [AvaloniaFact]
        public void HandleSelectorClick_Options_DoesNotOpenOverlay() {
            using var scope = new StudioUiScope(true);
            StudioExpLayout.SetOverlayOpen(true);
            StudioExpLayout.HandleSelectorClick(wasPrimaryVisible: false, usesNotesOverlay: false);
            Assert.False(StudioExpLayout.OverlayOpen);
        }

        [AvaloniaFact]
        public void HandleSelectorClick_TogglesWhenAlreadyVisible() {
            using var scope = new StudioUiScope(true);
            StudioExpLayout.SetOverlayOpen(false);
            StudioExpLayout.HandleSelectorClick(wasPrimaryVisible: true);
            Assert.True(StudioExpLayout.OverlayOpen);
            StudioExpLayout.HandleSelectorClick(wasPrimaryVisible: true);
            Assert.False(StudioExpLayout.OverlayOpen);
        }

        [AvaloniaFact]
        public void HandleSelectorClick_OpensWhenSelectingOther() {
            using var scope = new StudioUiScope(true);
            StudioExpLayout.SetOverlayOpen(false);
            StudioExpLayout.HandleSelectorClick(wasPrimaryVisible: false);
            Assert.True(StudioExpLayout.OverlayOpen);
            StudioExpLayout.SetOverlayOpen(false);
        }

        [AvaloniaFact]
        public void HandleSelectorClick_Classic_DoesNotOpen() {
            using var scope = new StudioUiScope(false);
            StudioExpLayout.SetOverlayOpen(false);
            StudioExpLayout.HandleSelectorClick(wasPrimaryVisible: false);
            Assert.False(StudioExpLayout.OverlayOpen);
        }

        static OverlayFixture OverlayParts() {
            var row = new RowDefinition {
                Height = new GridLength(StudioExpLayout.ClassicExpRowHeight),
                MinHeight = StudioExpLayout.ClassicExpRowMinHeight,
                MaxHeight = StudioExpLayout.ClassicExpRowMaxHeight,
            };
            var splitterRow = new RowDefinition { Height = GridLength.Auto };
            var primary = new Border { IsVisible = true };
            var secondary = new Border { IsVisible = true };
            Grid.SetRow(primary, StudioExpLayout.ClassicRow);
            Grid.SetRow(secondary, StudioExpLayout.ClassicRow);
            return new OverlayFixture(row, splitterRow, new GridSplitter { IsVisible = true, Height = 10 }, new Border(), primary, secondary, new ListBox { IsVisible = true });
        }

        static void ApplyOverlay(OverlayFixture parts, bool studio, bool open, bool usesNotesOverlay = true) {
            StudioExpLayout.ApplyOverlay(
                studio, open, showExpressions: true,
                parts.Row, parts.SplitterRow, parts.Splitter, parts.Dim, parts.Primary, parts.Secondary, parts.Toolbar,
                usesNotesOverlay);
        }

        readonly record struct OverlayFixture(
            RowDefinition Row,
            RowDefinition SplitterRow,
            GridSplitter Splitter,
            Border Dim,
            Border Primary,
            Border Secondary,
            ListBox Toolbar);
    }
}
