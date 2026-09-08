using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using OpenUtau.App.Studio;
using OpenUtau.App.Views;
using OpenUtau.Colors;
using Serilog;

namespace OpenUtau.App {
    public class App : Application {
#if DEBUG
        static bool developerToolsAttached;
#endif

        public override void Initialize() {
            Log.Information("Initializing application.");
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            // Headless test sessions build the app more than once per process,
            // and developer tools can only be attached to one app instance.
            if (!developerToolsAttached) {
                developerToolsAttached = true;
                this.AttachDeveloperTools();
            }
#endif
            InitializeCulture();
            InitializeTheme();
            Log.Information("Initialized application.");
        }

        public override void OnFrameworkInitializationCompleted() {
            Log.Information("Framework initialization completed.");
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new SplashWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void InitializeCulture() {
            Log.Information("Initializing culture.");
            string sysLang = CultureInfo.InstalledUICulture.Name;
            string prefLang = Core.Util.Preferences.Default.Language;
            var languages = GetLanguages();
            if (languages.ContainsKey(prefLang)) {
                SetLanguage(prefLang);
            } else if (languages.ContainsKey(sysLang)) {
                SetLanguage(sysLang);
                Core.Util.Preferences.Default.Language = sysLang;
                Core.Util.Preferences.Save();
            } else {
                SetLanguage("en-US");
            }

            // Force using InvariantCulture to prevent issues caused by culture dependent string conversion, especially for floating point numbers.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Log.Information("Initialized culture.");
        }

        public static Dictionary<string, IResourceProvider> GetLanguages() {
            if (Current == null) {
                return new();
            }
            var result = new Dictionary<string, IResourceProvider>();
            foreach (string key in Current.Resources.Keys.OfType<string>()) {
                if (key.StartsWith("strings-") &&
                    Current.Resources.TryGetResource(key, ThemeVariant.Default, out var res) &&
                    res is IResourceProvider rp) {
                    result.Add(key.Replace("strings-", ""), rp);
                }
            }
            return result;
        }

        public static void SetLanguage(string language) {
            if (Current == null) {
                return;
            }
            var languages = GetLanguages();
            foreach (var res in languages.Values) {
                Current.Resources.MergedDictionaries.Remove(res);
            }
            if (language != "en-US") {
                Current.Resources.MergedDictionaries.Add(languages["en-US"]);
            }
            if (languages.TryGetValue(language, out var res1)) {
                Current.Resources.MergedDictionaries.Add(res1);
            }
        }

        static async void InitializeTheme() {
            Log.Information("Initializing theme.");
            try {
                CustomTheme.ListThemes();
                await OudepLoaderRegistry.LoadAllAsync();
            } catch (Exception e) {
                Log.Error(e, "Failed to load themes from packages.");
            }
            SetTheme();
            Log.Information("Initialized theme.");
        }

        static IStyle? studioStyles;

        public static void SetTheme() {
            if (Current == null) {
                return;
            }
            StudioUI.EnsureClassicTheme();
            if (StudioUI.IsEnabled) {
                StudioUI.EnsureStudioTheme();
            }
            var light = (IResourceDictionary) Current.Resources["themes-light"]!;
            var dark = (IResourceDictionary) Current.Resources["themes-dark"]!;
            var custom = (IResourceDictionary) Current.Resources["themes-custom"]!;
            var warmSage = (IResourceDictionary) Current.Resources["themes-warmsage"]!;
            switch (Core.Util.Preferences.Default.ThemeName) {
                case "Light":
                    ApplyTheme(light);
                    Current.RequestedThemeVariant = ThemeVariant.Light;
                    break;
                case "Dark":
                    ApplyTheme(dark);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case "WarmSage":
                    ApplyTheme(warmSage);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case "Studio": {
                    var preset = StudioPresetManager.Load(Core.Util.Preferences.Default.StudioPreset)
                                 ?? StudioPresetManager.GetBuiltIn(StudioThemeGenerator.StudioDark);
                    ApplyTheme(StudioPresetManager.BuildResourceDictionary(preset));
                    Current.RequestedThemeVariant = preset.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;
                    break;
                }
                default:
                    ApplyTheme(custom);
                    CustomTheme.ApplyTheme(Core.Util.Preferences.Default.ThemeName);
                    if (CustomTheme.Default.IsDarkMode == true) {
                        Current.RequestedThemeVariant = ThemeVariant.Dark;
                    } else {
                        Current.RequestedThemeVariant = ThemeVariant.Light;
                    }
                    break;
            }
            ApplyStudioStyles(StudioUI.IsEnabled);
            ThemeManager.LoadTheme();
            StudioTrackPaintCache.Invalidate(StudioTrackPaletteChangeReason.ThemeResources);
        }

        static void ApplyStudioStyles(bool enable) {
            // An IStyle instance can only belong to one Styles collection, and
            // headless test sessions build the app more than once per process.
            var app = Current;
            if (app == null) {
                return;
            }
            bool present = studioStyles != null && app.Styles.Contains(studioStyles);
            if (enable && !present) {
                studioStyles = new StyleInclude(new Uri("avares://OpenUtau/App.axaml")) {
                    Source = new Uri("avares://OpenUtau/Styles/StudioStyles.axaml")
                };
                app.Styles.Add(studioStyles);
            } else if (!enable && present) {
                app.Styles.Remove(studioStyles!);
                studioStyles = null;
            }
        }

        private static void ApplyTheme(IResourceDictionary resDict) { 
            var res = Current?.Resources;
            foreach (var item in resDict) {
                res![item.Key] = item.Value;
            }
        }
    }
}
