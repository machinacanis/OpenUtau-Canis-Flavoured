using System;
using Avalonia.Media;

namespace OpenUtau.App.Studio {
    /// <summary>
    /// Shared sRGB / HSL / WCAG helpers for Studio UI. HSL conversion matches
    /// the original <see cref="StudioThemeGenerator"/> implementation.
    /// </summary>
    public static class StudioColorMath {
        public static (double H, double S, double L) RgbToHsl(Color c) {
            double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double l = (max + min) / 2;
            if (Math.Abs(max - min) < 1e-9) {
                return (0, 0, l);
            }
            double d = max - min;
            double s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            double h;
            if (max == r) {
                h = (g - b) / d + (g < b ? 6 : 0);
            } else if (max == g) {
                h = (b - r) / d + 2;
            } else {
                h = (r - g) / d + 4;
            }
            h *= 60;
            return (h, s, l);
        }

        public static Color HslToRgb(double h, double s, double l) {
            h = ((h % 360) + 360) % 360;
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs(h / 60 % 2 - 1));
            double m = l - c / 2;
            double r = 0, g = 0, b = 0;
            if (h < 60) {
                (r, g, b) = (c, x, 0);
            } else if (h < 120) {
                (r, g, b) = (x, c, 0);
            } else if (h < 180) {
                (r, g, b) = (0, c, x);
            } else if (h < 240) {
                (r, g, b) = (0, x, c);
            } else if (h < 300) {
                (r, g, b) = (x, 0, c);
            } else {
                (r, g, b) = (c, 0, x);
            }
            byte To255(double v) => (byte)Math.Round(Math.Clamp((v + m) * 255, 0, 255));
            return Color.FromRgb(To255(r), To255(g), To255(b));
        }

        public static Color Mix(Color a, Color b, double t) {
            t = Math.Clamp(t, 0, 1);
            byte Channel(byte x, byte y) => (byte)Math.Round(x * (1 - t) + y * t);
            return Color.FromRgb(Channel(a.R, b.R), Channel(a.G, b.G), Channel(a.B, b.B));
        }

        public static Color Lighten(Color c, double amount) => Mix(c, Avalonia.Media.Colors.White, amount);

        public static Color Darken(Color c, double amount) => Mix(c, Avalonia.Media.Colors.Black, amount);

        /// <summary>
        /// Clamp that never throws when the bounds are reversed.
        /// </summary>
        public static double Clamp(double x, double a, double b) {
            double lo = Math.Min(a, b);
            double hi = Math.Max(a, b);
            return Math.Clamp(x, lo, hi);
        }

        /// <summary>WCAG 2 relative luminance (sRGB).</summary>
        public static double RelativeLuminance(Color c) {
            static double Linear(byte channel) {
                double x = channel / 255.0;
                return x <= 0.04045 ? x / 12.92 : Math.Pow((x + 0.055) / 1.055, 2.4);
            }
            return 0.2126 * Linear(c.R) + 0.7152 * Linear(c.G) + 0.0722 * Linear(c.B);
        }

        /// <summary>WCAG 2 contrast ratio of two sRGB colors.</summary>
        public static double ContrastRatio(Color a, Color b) {
            double ya = RelativeLuminance(a);
            double yb = RelativeLuminance(b);
            double max = Math.Max(ya, yb);
            double min = Math.Min(ya, yb);
            return (max + 0.05) / (min + 0.05);
        }

        /// <summary>
        /// Rgba8888 packed pixel: R | G&lt;&lt;8 | B&lt;&lt;16 | A&lt;&lt;24.
        /// Not <c>Color.ToUInt32()</c> (that is ARGB).
        /// </summary>
        public static uint PackRgba8888(Color c) =>
            (uint)c.R | ((uint)c.G << 8) | ((uint)c.B << 16) | ((uint)c.A << 24);
    }
}
