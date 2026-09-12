using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace OpenUtau.App {
    /// <summary>
    /// Forces the native title bar to follow the dark theme on Windows.
    /// Avalonia only applies DWMWA_USE_IMMERSIVE_DARK_MODE on Windows 11 (build 22000+),
    /// so Windows 10 is left with a light title bar without this. Windows 11 additionally
    /// gets a pure black caption to match the request.
    /// </summary>
    static class Win32TitleBar {
        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwaUseImmersiveDarkModePre20H1 = 19;
        private const int DwmwaBorderColor = 34;
        private const int DwmwaCaptionColor = 35;
        private const int DwmwaTextColor = 36;
        private const int DwmwaColorDefault = unchecked((int)0xFFFFFFFF);
        private const int Black = 0x00000000;
        private const int White = unchecked((int)0x00FFFFFF);

        public static readonly AttachedProperty<bool> EnabledProperty =
            AvaloniaProperty.RegisterAttached<Window, bool>("Enabled", typeof(Win32TitleBar));

        private static readonly ConditionalWeakTable<Window, object> tracked = new();

        static Win32TitleBar() {
            EnabledProperty.Changed.AddClassHandler<Window>((window, _) => Track(window));
        }

        public static bool GetEnabled(Window window) => window.GetValue(EnabledProperty);

        public static void SetEnabled(Window window, bool value) => window.SetValue(EnabledProperty, value);

        private static void Track(Window window) {
            if (!tracked.TryGetValue(window, out _)) {
                tracked.Add(window, new object());
                window.Opened += (_, _) => Apply(window);
                window.ActualThemeVariantChanged += (_, _) => Apply(window);
            }
            Apply(window);
        }

        private static void Apply(Window window) {
            if (!OperatingSystem.IsWindows()) {
                return;
            }
            var handle = window.TryGetPlatformHandle();
            if (handle == null || handle.Handle == IntPtr.Zero) {
                return;
            }
            bool dark = window.ActualThemeVariant == ThemeVariant.Dark;
            int useDark = dark ? 1 : 0;
            if (DwmSetWindowAttribute(handle.Handle, DwmwaUseImmersiveDarkMode, ref useDark, sizeof(int)) != 0) {
                DwmSetWindowAttribute(handle.Handle, DwmwaUseImmersiveDarkModePre20H1, ref useDark, sizeof(int));
            }
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000)) {
                int caption = dark ? Black : DwmwaColorDefault;
                int border = dark ? Black : DwmwaColorDefault;
                int text = dark ? White : DwmwaColorDefault;
                DwmSetWindowAttribute(handle.Handle, DwmwaCaptionColor, ref caption, sizeof(int));
                DwmSetWindowAttribute(handle.Handle, DwmwaBorderColor, ref border, sizeof(int));
                DwmSetWindowAttribute(handle.Handle, DwmwaTextColor, ref text, sizeof(int));
            }
        }

#pragma warning disable SYSLIB1054
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
#pragma warning restore SYSLIB1054
    }
}
