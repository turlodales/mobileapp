using System;
using Toggl.Shared.Extensions;
using static System.Math;

namespace Toggl.Shared
{
    public struct Color : IEquatable<Color>
    {
        public byte Alpha { get; }
        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }

        public Color(byte red, byte green, byte blue, byte alpha = 255)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public Color(uint argb)
        {
            Alpha = (byte)((argb >> 24) & 255);
            Red = (byte)((argb >> 16) & 255);
            Green = (byte)((argb >> 8) & 255);
            Blue = (byte)(argb & 255);
        }

        /// <summary>
        /// Creates a Color from a hexadecimal string. Valid formats: aarrggbb, #aarrggbb, rrggbb, #rrggbb
        /// </summary>
        public Color(string hex) : this(hexStringToInt(hex))
        {
        }

        private static uint hexStringToInt(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return 0;

            hex = hex.TrimStart('#');

            var hexLength = hex.Length;

            if (hexLength == 6)
                return 0xFF000000 + Convert.ToUInt32(hex, 16);

            if (hexLength == 8)
                return Convert.ToUInt32(hex, 16);

            throw new ArgumentException("Invalid hex string was provided. Valid formats: aarrggbb, #aarrggbb, rrggbb, #rrggbb");
        }

        public override string ToString()
        {
            return $"{{a={Alpha}, r={Red}, g={Green}, b={Blue}}}";
        }

        public override int GetHashCode()
            => HashCode.Combine(Alpha, Red, Green, Blue);

        public static bool operator ==(Color color, Color otherColor)
            => color.Red == otherColor.Red
            && color.Green == otherColor.Green
            && color.Blue == otherColor.Blue
            && color.Alpha == otherColor.Alpha;

        public static bool operator !=(Color color, Color otherColor)
            => !(color == otherColor);

        public override bool Equals(object? obj)
        {
            if (obj is Color color)
                return this == color;

            return false;
        }

        public bool Equals(Color other)
            => this == other;

        public static Color ParseAndAdjustToUserTheme(string hex, bool isInDarkMode)
            => new Color(hex).AdjustForUserTheme(isInDarkMode);

        public static Color ParseAndAdjustToLabel(string hex, bool isInDarkMode)
            => new Color(hex).ToLabelColor(isInDarkMode);

        public static Color FromHSB(float hue, float saturation, float brightness)
        {
            if (!hue.IsInRange(0F, 360F))
                throw new ArgumentOutOfRangeException(nameof(hue));

            if (!saturation.IsInRange(0F, 1F))
                throw new ArgumentOutOfRangeException(nameof(saturation));

            if (!brightness.IsInRange(0F, 1F))
                throw new ArgumentOutOfRangeException(nameof(brightness));

            hue /= 360.0f;
            byte r = 0, g = 0, b = 0;

            if (saturation == 0)
            {
                r = g = b = (byte)(brightness * 255.0f + 0.5f);
            }
            else
            {
                float h = (hue - (byte)Floor(hue)) * 6.0f;
                float f = h - (byte)Floor(h);
                float p = brightness * (1.0f - saturation);
                float q = brightness * (1.0f - saturation * f);
                float t = brightness * (1.0f - (saturation * (1.0f - f)));
                switch ((int)h)
                {
                    case 0:
                        r = (byte)(brightness * 255.0f + 0.5f);
                        g = (byte)(t * 255.0f + 0.5f);
                        b = (byte)(p * 255.0f + 0.5f);
                        break;
                    case 1:
                        r = (byte)(q * 255.0f + 0.5f);
                        g = (byte)(brightness * 255.0f + 0.5f);
                        b = (byte)(p * 255.0f + 0.5f);
                        break;
                    case 2:
                        r = (byte)(p * 255.0f + 0.5f);
                        g = (byte)(brightness * 255.0f + 0.5f);
                        b = (byte)(t * 255.0f + 0.5f);
                        break;
                    case 3:
                        r = (byte)(p * 255.0f + 0.5f);
                        g = (byte)(q * 255.0f + 0.5f);
                        b = (byte)(brightness * 255.0f + 0.5f);
                        break;
                    case 4:
                        r = (byte)(t * 255.0f + 0.5f);
                        g = (byte)(p * 255.0f + 0.5f);
                        b = (byte)(brightness * 255.0f + 0.5f);
                        break;
                    case 5:
                        r = (byte)(brightness * 255.0f + 0.5f);
                        g = (byte)(p * 255.0f + 0.5f);
                        b = (byte)(q * 255.0f + 0.5f);
                        break;
                }
            }

            return new Color(r, g, b);
        }
    }

    public static class ColorExtensions
    {
        public static Color AdjustForUserTheme(this Color color, bool isUsingDarkMode)
        {
            if (!isUsingDarkMode)
                return color;

            var hue = color.GetHue();
            var saturation = color.GetSaturation();
            var brightness = color.GetBrightness();

            saturation = AdjustSaturationToDarkMode(saturation, brightness);
            brightness = AdjustBrightnessToDarkMode(brightness);

            return Color.FromHSB(hue, saturation, brightness);
        }

        public static Color ToLabelColor(this Color color, bool isUsingDarkMode)
        {
            var hue = color.GetHue();
            var saturation = color.GetSaturation();
            var value = color.GetBrightness();

            if (isUsingDarkMode)
            {
                saturation = AdjustSaturationToDarkMode(saturation, value);
                value = Min(AdjustBrightnessToDarkMode(value) + .05F, 1.0F);
            }
            else
            {
                value = Max(value - .15F, 0F);
            }

            return Color.FromHSB(hue, saturation, value);
        }

        private static float AdjustBrightnessToDarkMode(float brightness)
            => (2F + brightness) / 3F;

        private static float AdjustSaturationToDarkMode(float saturation, float brightness)
            => (saturation * brightness) / 1F;

        public static float GetHue(this Color color)
        {
            if (color.Red == color.Green && color.Green == color.Blue)
                return 0; // 0 makes as good an UNDEFINED value as any

            var r = color.Red / 255.0f;
            var g = color.Green / 255.0f;
            var b = color.Blue / 255.0f;

            var min = Min(r, Min(g, b));
            var max = Max(r, Max(g, b));
            var delta = max - min;

            float hue = 0.0f;
            if (r == max)
            {
                hue = (g - b) / delta;
            }
            else if (g == max)
            {
                hue = 2 + (b - r) / delta;
            }
            else if (b == max)
            {
                hue = 4 + (r - g) / delta;
            }

            hue *= 60;

            if (hue < 0.0f)
            {
                hue += 360.0f;
            }

            return hue;
        }

        public static float GetSaturation(this Color color)
        {
            var min = Min(color.Red, Min(color.Green, color.Blue));
            var max = Max(color.Red, Max(color.Green, color.Blue));
            var delta = max - min;

            if (delta == 0)
                return 0;

            return delta / (float)max;
        }

        public static float GetBrightness(this Color color)
        {
            var max = Max(color.Red, Max(color.Green, color.Blue));

            return (float)(max / 255d);
        }

        public static string ToHexString(this Color color)
            => $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";

        public static Color WithAlpha(this Color color, byte alpha)
            => new Color(color.Red, color.Green, color.Blue, alpha);

        public static double CalculateLuminance(this Color color)
        {
            // Adjusted relative luminance
            // math based on https://www.w3.org/WAI/GL/wiki/Relative_luminance

            var rsrgb = color.Red / 255.0;
            var gsrgb = color.Green / 255.0;
            var bsrgb = color.Blue / 255.0;

            var lowGammaCoeficient = 1 / 12.92;

            var r = rsrgb <= 0.03928 ? rsrgb * lowGammaCoeficient : adjustGamma(rsrgb);
            var g = gsrgb <= 0.03928 ? gsrgb * lowGammaCoeficient : adjustGamma(gsrgb);
            var b = bsrgb <= 0.03928 ? bsrgb * lowGammaCoeficient : adjustGamma(bsrgb);

            var luma = r * 0.2126 + g * 0.7152 + b * 0.0722;
            return luma;

            double adjustGamma(double channel)
                => Pow((channel + 0.055) / 1.055, 2.4);
        }
    }
}
