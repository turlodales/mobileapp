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

        public static Color FromHSB(float hue, float saturation, float brightness)
        {
            if (!hue.IsInRange(0F, 360F))
                throw new ArgumentOutOfRangeException(nameof(hue));

            if (!saturation.IsInRange(0F, 1F))
                throw new ArgumentOutOfRangeException(nameof(saturation));

            if (!brightness.IsInRange(0F, 1F))
                throw new ArgumentOutOfRangeException(nameof(brightness));

            if (0 == saturation)
            {
                var normalBrightness = Convert.ToByte(brightness * 255);
                return new Color(normalBrightness, normalBrightness, normalBrightness);
            }

            float fMax, fMid, fMin;
            int iSextant;
            byte iMax, iMid, iMin;

            if (0.5 < brightness)
            {
                fMax = brightness - (brightness * saturation) + saturation;
                fMin = brightness + (brightness * saturation) - saturation;
            }
            else
            {
                fMax = brightness + (brightness * saturation);
                fMin = brightness - (brightness * saturation);
            }

            iSextant = (int)Floor(hue / 60f);
            if (300f <= hue)
            {
                hue -= 360f;
            }

            hue /= 60f;
            hue -= 2f * (float)Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2)
            {
                fMid = (hue * (fMax - fMin)) + fMin;
            }
            else
            {
                fMid = fMin - (hue * (fMax - fMin));
            }

            iMax = Convert.ToByte(fMax * 255);
            iMid = Convert.ToByte(fMid * 255);
            iMin = Convert.ToByte(fMin * 255);

            return iSextant switch
            {
                1 => new Color(iMid, iMax, iMin),
                2 => new Color(iMin, iMax, iMid),
                3 => new Color(iMin, iMid, iMax),
                4 => new Color(iMid, iMin, iMax),
                5 => new Color(iMax, iMin, iMid),
                _ => new Color(iMax, iMid, iMin)
            };
        }
    }

    public static class ColorExtensions
    {
        public static Color AdjustForUserTheme(this Color color, bool isUsingDarkMode)
        {
            var hue = color.GetHue();
            var saturation = color.GetSaturation();
            var brightness = color.GetBrightness();

            if (isUsingDarkMode)
            {
                saturation = AdjustSaturationToDarkMode(saturation, brightness);
                brightness = AdjustBrightnessToDarkMode(brightness);
            }

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
            float r = (float)color.Red / 255.0f;
            float g = (float)color.Green / 255.0f;
            float b = (float)color.Blue / 255.0f;

            float max, min;
            float l, s = 0;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            // if max == min, then there is no color and
            // the saturation is zero.
            if (max != min)
            {
                l = (max + min) / 2;

                if (l <= .5)
                {
                    s = (max - min) / (max + min);
                }
                else
                {
                    s = (max - min) / (2 - max - min);
                }
            }
            return s;
        }

        public static float GetBrightness(this Color color)
        {
            float r = (float)color.Red / 255.0f;
            float g = (float)color.Green / 255.0f;
            float b = (float)color.Blue / 255.0f;

            float max, min;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            return (max + min) / 2;
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
