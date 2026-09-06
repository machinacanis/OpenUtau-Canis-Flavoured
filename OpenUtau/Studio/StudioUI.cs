using System.Linq;
using OpenUtau.Core.Util;
using ReactiveUI;

namespace OpenUtau.App.Studio {
    /// <summary>
    /// Studio UI is an opt-in editor chrome, appearance, and (later) layout/tools
    /// layer. Classic OpenUtau editing (NotesViewModel, Commands, hit testing)
    /// stays shared. Studio-only behavior belongs in this namespace — do not
    /// scatter <c>if (UseStudioUI)</c> through ViewModels.
    /// </summary>
    public class StudioUIChangedEvent { }

    public static class StudioUI {
        public const string StudioTheme = "Studio";
        // Legacy: WarmSage was a Studio-only palette before palettes became
        // auto-generated presets inside the single Studio theme.
        public const string WarmSageTheme = "WarmSage";

        public static readonly string[] ClassicThemes = ["Light", "Dark"];

        public static bool IsEnabled => Preferences.Default.UseStudioUI;

        public static bool IsStudioTheme(string? themeName) =>
            themeName == StudioTheme || themeName == WarmSageTheme;

        /// <summary>
        /// Studio palettes are configured inside the Studio UI page, so they
        /// never appear in the general Appearance theme list.
        /// </summary>
        public static string[] BuiltInThemes => ClassicThemes;

        public static bool IsBuiltIn(string themeName) =>
            ClassicThemes.Contains(themeName) || IsStudioTheme(themeName);

        /// <summary>
        /// When the user turns Studio UI on, the whole app is covered by the
        /// auto-generated Studio palette regardless of the classic theme name.
        /// </summary>
        public static bool EnsureStudioTheme() {
            if (!IsEnabled || Preferences.Default.ThemeName == StudioTheme) {
                return false;
            }
            Preferences.Default.ThemeName = StudioTheme;
            return true;
        }

        /// <summary>
        /// Drop Studio-only palettes when the user turns Studio UI off.
        /// </summary>
        public static bool EnsureClassicTheme() {
            if (IsEnabled || !IsStudioTheme(Preferences.Default.ThemeName)) {
                return false;
            }
            Preferences.Default.ThemeName = "Dark";
            return true;
        }

        public static void NotifyChanged() {
            StudioTrackPaintCache.Invalidate(StudioTrackPaletteChangeReason.StudioUIToggled);
            MessageBus.Current.SendMessage(new StudioUIChangedEvent());
        }
    }
}
