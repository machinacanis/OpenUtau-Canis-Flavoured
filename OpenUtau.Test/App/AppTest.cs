using Xunit;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using OpenUtau.App;
using ReactiveUI.Avalonia;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

public class TestAppBuilder {
    // Mirrors Program.BuildAvaloniaApp: ViewModels use ReactiveUI's WhenAnyValue,
    // so the headless test app must initialize ReactiveUI too.
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseReactiveUI(_ => { })
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

namespace OpenUtau.App {
    public class AppTest {
        [Fact]
        public void BuildTest() {
            Assert.False(typeof(App).IsAbstract);
            Assert.False(typeof(Program).IsAbstract);
        }

        [AvaloniaFact]
        public void StringsTest() {
            // The headless test session already built the app on its dispatcher
            // thread; building a second one from the test thread breaks it.
            var app = Application.Current as App;
            Assert.NotNull(app);

            var languages = App.GetLanguages();
            Assert.True(languages.Count > 1);
            Assert.Contains("en-US", languages.Keys);
            Assert.Contains("zh-CN", languages.Keys);
            Assert.Contains("ja-JP", languages.Keys);
            foreach (var pair in languages) {
                Assert.NotNull(pair.Value);
            }
        }
    }
}
