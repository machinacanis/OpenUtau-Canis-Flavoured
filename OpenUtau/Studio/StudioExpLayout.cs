using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using ReactiveUI;

namespace OpenUtau.App.Studio {
    /// <summary>
    /// Studio-only expression chrome: selector strip under the curve, a
    /// two-column curve-tool palette in the vacated left gutter, and a
    /// notes-area overlay for editing. Classic layout stays in
    /// <c>PianoRoll.axaml</c>.
    /// </summary>
    public class StudioExpOverlayChangedEvent { }

    public static class StudioExpLayout {
        public const double SelectorWidth = 60;
        public const double BarHeight = 22;
        public const int ClassicRow = 5;
        public const int SplitterRow = 4;
        public const int ClassicColumnSpan = 1;
        public const int StudioRow = 6;
        public const int StudioColumnSpan = 3;
        public const int NotesRow = 3;
        public const int OverlayZIndex = 40;
        public const double ClassicExpRowHeight = 150;
        public const double ClassicExpRowMinHeight = 132;
        public const double ClassicExpRowMaxHeight = 600;
        public const int ClassicToolbarColumn = 1;
        public const int StudioToolbarColumn = 0;
        public const int ToolbarColumns = 2;
        public const int ToolbarMinRows = 4;

        public static bool OverlayOpen { get; private set; }

        static readonly ITemplate<Panel?> ClassicToolbarPanel =
            new FuncTemplate<Panel?>(() => new StackPanel { Orientation = Orientation.Horizontal });
        static readonly ITemplate<Panel?> StudioToolbarPanel =
            new FuncTemplate<Panel?>(() => new UniformGrid { Columns = ToolbarColumns });

        public static void Apply(
            Border panel,
            StackPanel stack,
            Control gear,
            Control? status,
            IReadOnlyList<Control> selectors,
            bool studio) {
            if (studio) {
                Grid.SetRow(panel, StudioRow);
                Grid.SetColumnSpan(panel, StudioColumnSpan);
                panel.Height = BarHeight;
                DockPanel.SetDock(gear, Dock.Right);
                stack.Orientation = Orientation.Horizontal;
                if (status != null) {
                    DockPanel.SetDock(status, Dock.Right);
                    status.IsVisible = true;
                }
            } else {
                Grid.SetRow(panel, ClassicRow);
                Grid.SetColumnSpan(panel, ClassicColumnSpan);
                panel.ClearValue(Layoutable.HeightProperty);
                DockPanel.SetDock(gear, Dock.Bottom);
                stack.Orientation = Orientation.Vertical;
                if (status != null) {
                    status.IsVisible = false;
                }
                foreach (var selector in selectors) {
                    selector.IsVisible = true;
                }
            }
        }

        public static void ApplyCurveToolbar(ListBox toolbar, bool studio) {
            if (studio) {
                Grid.SetColumn(toolbar, StudioToolbarColumn);
                toolbar.HorizontalAlignment = HorizontalAlignment.Stretch;
                toolbar.VerticalAlignment = VerticalAlignment.Top;
                toolbar.ItemsPanel = StudioToolbarPanel;
            } else {
                Grid.SetColumn(toolbar, ClassicToolbarColumn);
                toolbar.HorizontalAlignment = HorizontalAlignment.Left;
                toolbar.VerticalAlignment = VerticalAlignment.Top;
                toolbar.ItemsPanel = ClassicToolbarPanel;
            }
        }

        public static void SetOverlayOpen(bool open) {
            if (!StudioUI.IsEnabled) {
                open = false;
            }
            if (OverlayOpen == open) {
                return;
            }
            OverlayOpen = open;
            MessageBus.Current.SendMessage(new StudioExpOverlayChangedEvent());
        }

        public static void HandleSelectorClick(bool wasPrimaryVisible, bool usesNotesOverlay = true) {
            if (!StudioUI.IsEnabled) {
                return;
            }
            if (!usesNotesOverlay) {
                SetOverlayOpen(false);
                return;
            }
            SetOverlayOpen(wasPrimaryVisible ? !OverlayOpen : true);
        }

        public static void ApplyOverlay(
            bool studio,
            bool open,
            bool showExpressions,
            RowDefinition expRow,
            RowDefinition splitterRow,
            Control splitter,
            Control dim,
            Control primary,
            Control secondary,
            Control toolbar,
            bool usesNotesOverlay = true) {
            bool overlay = studio && open && showExpressions && usesNotesOverlay;
            if (overlay) {
                CollapseRow(expRow);
                CollapseRow(splitterRow);
                splitter.IsVisible = false;
                splitter.Height = 0;
                Grid.SetRow(primary, NotesRow);
                Grid.SetRow(secondary, NotesRow);
                Grid.SetRow(toolbar, NotesRow);
                Grid.SetColumn(toolbar, StudioToolbarColumn);
                primary.ZIndex = OverlayZIndex + 1;
                secondary.ZIndex = OverlayZIndex;
                toolbar.ZIndex = OverlayZIndex + 2;
                dim.ZIndex = OverlayZIndex;
                dim.IsVisible = true;
                primary.IsVisible = true;
                secondary.IsVisible = false;
                toolbar.ClearValue(Visual.IsVisibleProperty);
                return;
            }
            if (studio && usesNotesOverlay) {
                CollapseRow(expRow);
                CollapseRow(splitterRow);
                splitter.IsVisible = false;
                splitter.Height = 0;
                Grid.SetRow(primary, NotesRow);
                Grid.SetRow(secondary, NotesRow);
                Grid.SetRow(toolbar, ClassicRow);
                Grid.SetColumn(toolbar, StudioToolbarColumn);
                dim.IsVisible = false;
                primary.IsVisible = false;
                secondary.IsVisible = false;
                toolbar.IsVisible = false;
                return;
            }
            RestoreStrip(
                studio, showExpressions, expRow, splitterRow, splitter,
                dim, primary, secondary, toolbar);
        }

        static void RestoreStrip(
            bool studio,
            bool showExpressions,
            RowDefinition expRow,
            RowDefinition splitterRow,
            Control splitter,
            Control dim,
            Control primary,
            Control secondary,
            Control toolbar) {
            if (showExpressions) {
                expRow.Height = new GridLength(ClassicExpRowHeight);
                expRow.MinHeight = ClassicExpRowMinHeight;
                expRow.MaxHeight = ClassicExpRowMaxHeight;
            } else {
                CollapseRow(expRow);
            }
            splitterRow.Height = GridLength.Auto;
            splitterRow.MinHeight = 0;
            splitterRow.MaxHeight = double.PositiveInfinity;
            splitter.ClearValue(Visual.IsVisibleProperty);
            splitter.Height = 10;
            Grid.SetRow(primary, ClassicRow);
            Grid.SetRow(secondary, ClassicRow);
            Grid.SetRow(toolbar, ClassicRow);
            Grid.SetColumn(toolbar, studio ? StudioToolbarColumn : ClassicToolbarColumn);
            primary.ClearValue(Visual.ZIndexProperty);
            secondary.ClearValue(Visual.ZIndexProperty);
            toolbar.ClearValue(Visual.ZIndexProperty);
            dim.ClearValue(Visual.ZIndexProperty);
            dim.IsVisible = false;
            primary.ClearValue(Visual.IsVisibleProperty);
            if (studio) {
                secondary.IsVisible = false;
            } else {
                secondary.ClearValue(Visual.IsVisibleProperty);
            }
            toolbar.ClearValue(Visual.IsVisibleProperty);
        }

        static void CollapseRow(RowDefinition row) {
            row.Height = new GridLength(0);
            row.MinHeight = 0;
            row.MaxHeight = 0;
        }

        public static void ApplyOverflow(
            double availableWidth,
            IReadOnlyList<Control> selectors,
            int primary = 0) {
            if (!StudioUI.IsEnabled || availableWidth <= 0 || selectors.Count == 0) {
                return;
            }
            int n = selectors.Count;
            int fit = Math.Clamp((int)(availableWidth / SelectorWidth), 1, n);
            var visible = new bool[n];
            if (fit >= n) {
                Array.Fill(visible, true);
            } else {
                primary = Math.Clamp(primary, 0, n - 1);
                visible[primary] = true;
                int used = 1;
                for (int i = 0; i < n && used < fit; i++) {
                    if (!visible[i]) {
                        visible[i] = true;
                        used++;
                    }
                }
            }
            for (int i = 0; i < n; i++) {
                selectors[i].IsVisible = visible[i];
            }
        }
    }
}
