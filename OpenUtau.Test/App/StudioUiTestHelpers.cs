using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.VisualTree;
using OpenUtau.Core.Util;
using Xunit;

namespace OpenUtau.App {
    /// <summary>
    /// Tests that flip the global Studio UI preference or add the Studio button
    /// skin to the application styles must not overlap each other.
    /// </summary>
    [CollectionDefinition(StudioUiSerialCollection.Name, DisableParallelization = true)]
    public class StudioUiSerialCollection {
        public const string Name = "StudioUiSerial";
    }

    /// <summary>Sets the UseStudioUI preference and restores it afterwards.</summary>
    public sealed class StudioUiScope : IDisposable {
        readonly bool previous;

        public StudioUiScope(bool enabled) {
            previous = Preferences.Default.UseStudioUI;
            Preferences.Default.UseStudioUI = enabled;
        }

        public void Dispose() => Preferences.Default.UseStudioUI = previous;
    }

    /// <summary>
    /// Loads the Studio button skin exactly the way StudioStyles.axaml does, so a
    /// test can exercise it without depending on the preference at app startup.
    /// </summary>
    public sealed class StudioSkinScope : IDisposable {
        readonly StyleInclude include;

        public StudioSkinScope() {
            include = new StyleInclude(new Uri("avares://OpenUtau/App.axaml")) {
                Source = new Uri("avares://OpenUtau/Styles/StudioOneControls.axaml"),
            };
            Application.Current!.Styles.Add(include);
        }

        public void Dispose() => Application.Current!.Styles.Remove(include);
    }

    public static class StudioSkinTestHelpers {
        /// <summary>The chrome Border owned by the Studio button skin template.</summary>
        public static Border Chrome(Control control) =>
            control.GetVisualDescendants().OfType<Border>()
                .FirstOrDefault(border => border.Name == "PART_Chrome");

        public static void AssertResourceColor(string resourceKey, IBrush actual) {
            Assert.True(Application.Current!.TryFindResource(resourceKey, out var expected),
                $"resource {resourceKey} not found");
            Assert.Equal(((ISolidColorBrush)expected!).Color, ((ISolidColorBrush)actual!).Color);
        }

        /// <summary>For brushes that are not solid, e.g. the chrome gradient.</summary>
        public static void AssertResourceBrush(string resourceKey, IBrush actual) {
            Assert.True(Application.Current!.TryFindResource(resourceKey, out var expected),
                $"resource {resourceKey} not found");
            Assert.Same(expected, actual);
        }
    }
}
