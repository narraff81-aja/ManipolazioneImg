using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg.Colors;
/// <summary>
/// Provides color conversion functionality.
/// </summary>
/// <remarks>
/// http://en.wikipedia.org/wiki/HSV_color_space
/// http://www.easyrgb.com/math.php?MATH=M19#text19
/// </remarks>
public class ColorConverter {
    public static ColorRGB ColorToRgb(Color color) {
        return new(color.R, color.G, color.B);//, color.A
    }
    public static Color RgbToColor(ColorRGB rgb) {
        return Color.FromArgb(rgb.Red, rgb.Green, rgb.Blue);//rgb.Alpha,
    }
    public static ColorHSB RgbToHsb(ColorRGB rgb) {
        // _NOTE #1: Even though we're dealing with a very small range of
        // numbers, the accuracy of all calculations is fairly important.
        // For this reason, I've opted to use double data types instead
        // of float, which gives us a little bit extra precision (recall
        // that precision is the number of significant digits with which
        // the result is expressed).
        var r = rgb.Red / 255d;
        var g = rgb.Green / 255d;
        var b = rgb.Blue / 255d;
        var minValue = Math.Min(r, Math.Min(g, b));
        var maxValue = Math.Max(r, Math.Max(g, b));
        var delta = maxValue - minValue;
        double hue = 0;
        double saturation;
        var brightness = maxValue * 100;
        if (Math.Abs(maxValue - 0) < 0.00001 || Math.Abs(delta - 0) < 0.00001) {
            hue = 0;
            saturation = 0;
        }
        else {
            // _NOTE #2: FXCop insists that we avoid testing for floating
            // point equality (CA1902). Instead, we'll perform a series of
            // tests with the help of 0.00001 that will provide
            // a more accurate equality evaluation.
            if (Math.Abs(minValue - 0) < 0.00001) {
                saturation = 100;
            }
            else {
                saturation = delta / maxValue * 100;
            }
            if (Math.Abs(r - maxValue) < 0.00001) {
                hue = (g - b) / delta;
            }
            else if (Math.Abs(g - maxValue) < 0.00001) {
                hue = 2 + (b - r) / delta;
            }
            else if (Math.Abs(b - maxValue) < 0.00001) {
                hue = 4 + (r - g) / delta;
            }
        }
        hue *= 60;
        if (hue < 0) {
            hue += 360;
        }
        return new(hue, saturation, brightness);//, rgb.Alpha
    }
    public static ColorHSL RgbToHsl(ColorRGB rgb) {
        var varR = rgb.Red / 255.0; //Where RGB values = 0 ÷ 255
        var varG = rgb.Green / 255.0;
        var varB = rgb.Blue / 255.0;
        var varMin = Math.Min(varR, Math.Min(varG, varB)); //Min. value of RGB
        var varMax = Math.Max(varR, Math.Max(varG, varB)); //Max. value of RGB
        var delMax = varMax - varMin; //Delta RGB value
        double h;
        double s;
        var l = (varMax + varMin) / 2;
        if (Math.Abs(delMax - 0) < 0.00001) //This is a gray, no chroma...
        {
            h = 0; //HSL results = 0 ÷ 1
            s = 0;
            // UK:
            // s = 1.0;
        }
        else //Chromatic data...
        {
            if (l < 0.5) {
                s = delMax / (varMax + varMin);
            }
            else {
                s = delMax / (2.0 - varMax - varMin);
            }
            var delR = ((varMax - varR) / 6.0 + delMax / 2.0) / delMax;
            var delG = ((varMax - varG) / 6.0 + delMax / 2.0) / delMax;
            var delB = ((varMax - varB) / 6.0 + delMax / 2.0) / delMax;
            if (Math.Abs(varR - varMax) < 0.00001) {
                h = delB - delG;
            }
            else if (Math.Abs(varG - varMax) < 0.00001) {
                h = 1.0 / 3.0 + delR - delB;
            }
            else if (Math.Abs(varB - varMax) < 0.00001) {
                h = 2.0 / 3.0 + delG - delR;
            }
            else {
                // Uwe Keim.
                h = 0.0;
            }
            if (h < 0.0) {
                h += 1.0;
            }
            if (h > 1.0) {
                h -= 1.0;
            }
        }
        // --
        return new(h * 360.0, s * 100.0, l * 100.0);
    }
    public static ColorRGB HsbToRgb(ColorHSB hsb) {
        double red = 0, green = 0, blue = 0;
        double h = hsb.Hue;
        var s = (double)hsb.Saturation / 100;
        var b = (double)hsb.Brightness / 100;
        if (Math.Abs(s - 0) < 0.00001) {
            red = b;
            green = b;
            blue = b;
        }
        else {
            // the color wheel has six sectors.
            var sectorPosition = h / 60;
            var sectorNumber = (int)Math.Floor(sectorPosition);
            var fractionalSector = sectorPosition - sectorNumber;
            var p = b * (1 - s);
            var q = b * (1 - s * fractionalSector);
            var t = b * (1 - s * (1 - fractionalSector));
            // Assign the fractional colors to r, g, and b
            // based on the sector the angle is in.
            switch (sectorNumber) {
                case 0:
                    red = b;
                    green = t;
                    blue = p;
                    break;
                case 1:
                    red = q;
                    green = b;
                    blue = p;
                    break;
                case 2:
                    red = p;
                    green = b;
                    blue = t;
                    break;
                case 3:
                    red = p;
                    green = q;
                    blue = b;
                    break;
                case 4:
                    red = t;
                    green = p;
                    blue = b;
                    break;
                case 5:
                    red = b;
                    green = p;
                    blue = q;
                    break;
            }
        }
        var nRed = Convert.ToByte(red * 255);
        var nGreen = Convert.ToByte(green * 255);
        var nBlue = Convert.ToByte(blue * 255);
        return new(nRed, nGreen, nBlue);//, hsb.Alpha
    }
    public static ColorRGB HslToRgb(ColorHSL hsl) {
        double red, green, blue;
        var h = hsl.Hue / 360.0;
        var s = hsl.Saturation / 100.0;
        var l = hsl.Lightness / 100.0;
        if (Math.Abs(s - 0.0) < 0.00001) {
            red = l;
            green = l;
            blue = l;
        }
        else {
            double var2;
            if (l < 0.5) {
                var2 = l * (1.0 + s);
            }
            else {
                var2 = l + s - s * l;
            }
            var var1 = 2.0 * l - var2;
            red = hue2Rgb(var1, var2, h + 1.0 / 3.0);
            green = hue2Rgb(var1, var2, h);
            blue = hue2Rgb(var1, var2, h - 1.0 / 3.0);
        }
        // --
        var nRed = Convert.ToByte(red * 255.0);
        var nGreen = Convert.ToByte(green * 255.0);
        var nBlue = Convert.ToByte(blue * 255.0);
        return new(nRed, nGreen, nBlue);//, hsl.Alpha
    }
    private static double hue2Rgb(double v1, double v2, double vH) {
        if (vH < 0.0) {
            vH += 1.0;
        }
        if (vH > 1.0) {
            vH -= 1.0;
        }
        if (6.0 * vH < 1.0) {
            return v1 + (v2 - v1) * 6.0 * vH;
        }
        if (2.0 * vH < 1.0) {
            return v2;
        }
        if (3.0 * vH < 2.0) {
            return v1 + (v2 - v1) * (2.0 / 3.0 - vH) * 6.0;
        }
        return v1;
    }

    public static ColorXYZ RgbToXyz(ColorRGB rgb) {
        // per CIELAB (D65)
        //Where RGB values = 0 ÷ 255

        // STEP 1: Linearizzazione RGB (Sgamma-tizzazione) e normalizzazione a [0..1]
        List<double> varRGB = new() { rgb.Red, rgb.Green, rgb.Blue };
        for (int i = 0; i < varRGB.Count; i++) {
            double value = varRGB[i] / 255.0;
            if (value > 0.04045)
                value = Math.Pow(((value + 0.055) / 1.055), 2.4);//2.4 
            else
                value /= 12.92;
            varRGB[i] = value;
        }
        // STEP 2: Conversione a XYZ (Matrice di trasformazione standard sRGB D65)
        double x = varRGB[0] * 0.4124564 + varRGB[1] * 0.3575761 + varRGB[2] * 0.1804375;
        double y = varRGB[0] * 0.2126729 + varRGB[1] * 0.7151522 + varRGB[2] * 0.0721750;
        double z = varRGB[0] * 0.0193339 + varRGB[1] * 0.1191920 + varRGB[2] * 0.9503041;
        /*double X = varRGB[0] * 0.4124 + varRGB[1] * 0.3576 + varRGB[2] * 0.1805;
        double Y = varRGB[0] * 0.2126 + varRGB[1] * 0.7152 + varRGB[2] * 0.0722;
        double Z = varRGB[0] * 0.0193 + varRGB[1] * 0.1192 + varRGB[2] * 0.9505;//0.9504*/

        return new ColorXYZ(Math.Round(100 * x, 4), Math.Round(100 * y, 4), Math.Round(100 * z, 4));
    }
    public static ColorLAB XyzToLab(ColorXYZ XYZ) {
        // per CIELAB (D65)
        //Where XYZ values = 100 *
        double[] varXYZ = { XYZ.X, XYZ.Y, XYZ.Z };

        // Observer= 2°, Illuminant= D65
        // STEP 1: Conversione XYZ -> LAB usando l'illuminante di riferimento D65 
        // (X=95.047, Y=100.00, Z=108.883 normalizzati a scala 0..1)
        varXYZ[0] /= 95.047; // 0.95047;
        varXYZ[1] /= 100.00; // 1.00000;
        varXYZ[2] /= 108.883; // 1.08883;
        /* Illuminant= D50
        varXYZ[0] /= 96.42; // 0.9642;
        varXYZ[1] /= 100.00; // 1.00000;
        varXYZ[2] /= 82.51; // 0.8251;
        */

        double delta = 6.0 / 29.0;
        for (int i = 0; i < varXYZ.Length; i++) {
            if (varXYZ[i] > delta * delta * delta) {
                varXYZ[i] = Math.Pow(varXYZ[i], 1.0 / 3.0);
            }
            else {
                //varXYZ[i] = (7.787 * varXYZ[i]) + (16.0 / 116.0);//ok
                varXYZ[i] = (varXYZ[i] / (3.0 * delta * delta)) + (4.0 / 29.0);
            }
        }
        double L = (116.0 * varXYZ[1]) - 16.0;
        double a = 500.0 * (varXYZ[0] - varXYZ[1]);
        double b = 200.0 * (varXYZ[1] - varXYZ[2]);
        return new ColorLAB(Math.Round(L, 4), Math.Round(a, 4), Math.Round(b, 4));
    }

    public static ColorLAB RgbToLab(ColorRGB rgb) {
        // per CIELAB (D65)
        return XyzToLab(RgbToXyz(rgb));
    }

    public static ColorXYZ RgbToXyzD50(ColorRGB rgb) {
        // per CIELAB (D50)
        //Where RGB values = 0 ÷ 255

        // STEP 1: Linearizzazione RGB (Sgamma-tizzazione) e normalizzazione a [0..1]
        List<double> varRGB = new() { rgb.Red, rgb.Green, rgb.Blue };
        for (int i = 0; i < varRGB.Count; i++) {
            double value = varRGB[i] / 255.0;
            if (value > 0.04045)
                value = Math.Pow(((value + 0.055) / 1.055), 2.4);//2.4 
            else
                value /= 12.92;
            varRGB[i] = value;
        }
        // STEP 2: Conversione a XYZ (Matrice di trasformazione standard sRGB D50)
        // Valori per illuminante D50 (sRGB)
        double x = varRGB[0] * 0.4360747 + varRGB[1] * 0.3850649 + varRGB[2] * 0.1430804;
        double y = varRGB[0] * 0.2225045 + varRGB[1] * 0.7168786 + varRGB[2] * 0.0606169;
        double z = varRGB[0] * 0.0139322 + varRGB[1] * 0.0971045 + varRGB[2] * 0.7141733;

        return new ColorXYZ(Math.Round(100 * x, 4), Math.Round(100 * y, 4), Math.Round(100 * z, 4));
    }
    public static ColorLAB XyzD50ToLabD50(ColorXYZ XYZ) {
        // per CIELAB (D50)
        //Where XYZ values = 100 *
        double[] varXYZ = { XYZ.X, XYZ.Y, XYZ.Z };


        //Observer= 2°,  Illuminant= D50
        // STEP 1: Conversione XYZ -> LAB usando l'illuminante di riferimento D65         
        varXYZ[0] /= 96.4212; // 0.9642;
        varXYZ[1] /= 100.00; // 1.00000;
        varXYZ[2] /= 82.5188; // 0.8251;        

        double delta = 6.0 / 29.0;
        for (int i = 0; i < varXYZ.Length; i++) {
            if (varXYZ[i] > delta * delta * delta) {
                varXYZ[i] = Math.Pow(varXYZ[i], 1.0 / 3.0);
            }
            else {
                //varXYZ[i] = (7.787 * varXYZ[i]) + (16.0 / 116.0);//ok
                varXYZ[i] = (varXYZ[i] / (3.0 * delta * delta)) + (4.0 / 29.0);
            }
        }
        double L = (116.0 * varXYZ[1]) - 16.0;
        double a = 500.0 * (varXYZ[0] - varXYZ[1]);
        double b = 200.0 * (varXYZ[1] - varXYZ[2]);
        return new ColorLAB(Math.Round(L, 4), Math.Round(a, 4), Math.Round(b, 4));
    }

    public static ColorLAB RgbToLabD50(ColorRGB rgb) {
        // per CIELAB (D50)
        return XyzD50ToLabD50(RgbToXyzD50(rgb));
    }
    public static ColorXYZ LabToXyz(ColorLAB lab) {
        // per CIELAB (D65)
        // Riferimento bianco D65
        double refX = 95.047;
        double refY = 100.0;
        double refZ = 108.883;

        double fy = (lab.L + 16.0) / 116.0;
        double fx = (lab.a / 500.0) + fy;
        double fz = fy - (lab.b / 200.0);

        double fy3 = fy * fy * fy;
        double fx3 = fx * fx * fx;
        double fz3 = fz * fz * fz;

        double delta = 6.0 / 29.0;
        double delta3 = delta * delta * delta;//0.008856
        double pendenza = 29.0 / 3.0;
        double pendenza3 = pendenza * pendenza * pendenza;//903.3
        // 7.787 = 903.3 / 116 = 903.3 / (29 * 4)

        double xr = fx3 > delta3 ? fx3 : (116.0 * fx - 16.0) / 7.787;
        //double yr = lab.L > (delta3 * pendenza3) ? fy3 : lab.L / pendenza3;
        double yr = lab.L / pendenza3 > delta3 ? fy3 : lab.L / pendenza3;
        double zr = fz3 > delta3 ? fz3 : (116.0 * fz - 16.0) / 7.787;

        double x = xr * refX;
        double y = yr * refY;
        double z = zr * refZ;

        return new(x, y, z);
        /*
         Intervalli di input (\(L^{*}a^{*}b^{*}\)):
        \(L^{*}\) va da 0 a 100.
        \(a^{*}\) e \(b^{*}\) solitamente vanno da -128 a 128 (ma possono variare in base al gamut).
        
        Soglia di transizione:
        Si usa il valore pivot standard 0.008856 (6/29 al cubo) per determinare se 
        applicare la formula cubica o quella lineare.
        La costante 903.3 (24389/27=29/3 al cubo) rappresenta la pendenza lineare per l'asse Y (Luminanza).
         */
    }
    public static ColorRGB XyzToRgb(ColorXYZ XYZ) {
        // per CIELAB (D65)
        // 1. Conversione da XYZ a RGB lineare (matrice sRGB / D65)
        double rLinear = 3.2406 * XYZ.X - 1.5372 * XYZ.Y - 0.4986 * XYZ.Z;
        double gLinear = -0.9689 * XYZ.X + 1.8758 * XYZ.Y + 0.0415 * XYZ.Z;
        double bLinear = 0.0557 * XYZ.X - 0.2040 * XYZ.Y + 1.0570 * XYZ.Z;

        // 2. Applicazione della correzione di gamma (sRGB companding)
        double GammaCorrect(double c) {
            if (c <= 0.0031308)
                return 12.92 * c;
            else
                return 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;
        }

        double r = GammaCorrect(rLinear);
        double g = GammaCorrect(gLinear);
        double b = GammaCorrect(bLinear);

        // 3. Clamping ed estensione nell'intervallo 0-255
        byte Clamp(double value) => (byte)(Math.Clamp(value, 0.0, 1.0) * 255);

        return new(Clamp(r), Clamp(g), Clamp(b));
    }

    public static ColorRGB LabToRgb(ColorLAB lab) {
        // per CIELAB (D65)
        return XyzToRgb(LabToXyz(lab));
    }
}
