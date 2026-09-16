using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace OpenUtau.App.Studio {
    /// <summary>
    /// Studio-only expression chrome: selector strip under the curve, and a
    /// two-column curve-tool palette in the vacated left gutter. Classic
    /// layout stays in <c>PianoRoll.axaml</c>.
    /// </summary>
    public static class StudioExpLayout {
        public const double SelectorWidth = 60;
        public const double BarHeight = 22;
        public const int ClassicRow = 5;
        public const int ClassicColumnSpan = 1;
        public const int StudioRow = 6;
        public const int StudioColumnSpan = 3;
        public const int ClassicToolbarColumn = 1;
        public const int StudioToolbarColumn = 0;
        public const int ToolbarColumns = 2;
        public const int ToolbarMinRows = 4;

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

        public static void ApplyOverflow(
            double availableWidth,
            IReadOnlyList<Control> selectors,
            int primary = 0,
            int secondary = 1) {
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
                secondary = Math.Clamp(secondary, 0, n - 1);
                visible[primary] = true;
                int used = 1;
                if (used < fit && secondary != primary) {
                    visible[secondary] = true;
                    used++;
                }
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
