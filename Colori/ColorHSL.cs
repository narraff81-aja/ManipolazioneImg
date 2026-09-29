using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg.Colors;

public readonly struct ColorHSL : IEquatable<ColorHSL> {//, IComparable<ColorHSL>
    /// <summary>
    /// Hue 0.0°-360.0°
    /// </summary>
    public readonly double Hue;
    /// <summary>
    /// Saturation 0.0%-100.0%
    /// </summary>
    public readonly double Saturation;
    /// <summary>
    /// Lightness 0.0%-100.0%
    /// </summary>
    public readonly double Lightness;

    public ColorHSL(double hue, double saturation, double lightness) {
        this.Hue = Math.Round(Math.Clamp(hue, 0.0, 360.0), 3);
        this.Saturation = Math.Round(Math.Clamp(saturation, 0.0, 100.0), 2);
        this.Lightness = Math.Round(Math.Clamp(lightness, 0.0, 100.0), 2);
    }

    public static Point3D ToConicPoint3D(ColorHSL color) {
        double z = color.Lightness / 100;
        double fattore = 1.0 - Math.Abs(2 * z - 1);
        double x = color.Saturation / 100 * fattore * Math.Cos(color.Hue * Math.PI / 180);
        double y = color.Saturation / 100 * fattore * Math.Sin(color.Hue * Math.PI / 180);
        // per Point3D vanno moltiplicati x 1000
        return new Point3D((Int16)Math.Round(1000 * x), (Int16)Math.Round(1000 * y),(Int16) Math.Round(1000* z));
    }
    public Point3D ToConicPoint3D() {
        return ToConicPoint3D(this);
    }

    public static Point3D ToPoint3D(ColorHSL color) {
        //Profilo Parabolico
        double z = color.Lightness / 100;
        double fattore = 1.0 - Math.Pow(2 * z - 1,2);
        double x = color.Saturation / 100 * fattore * Math.Cos(color.Hue * Math.PI / 180);
        double y = color.Saturation / 100 * fattore * Math.Sin(color.Hue * Math.PI / 180);
        // per Point3D vanno moltiplicati x 1000 
        return new Point3D((Int16)Math.Round(1000 * x), (Int16)Math.Round(1000 * y), (Int16)Math.Round(1000 * z));
    }
    public Point3D ToPoint3D() {
        //Profilo Parabolico
        return ToPoint3D(this);
    }

    public static ColorHSL FromPoint3D(Point3D p3d, bool isConic=false) {
        double x = p3d.X * 100.0 / 1000;
        double y = p3d.Y * 100.0 / 1000;
        double z = p3d.Z * 100.0 / 1000; // = Lightness
        double raggio = Math.Sqrt(x * x + y * y);// = color.Saturation * fattore
        double angolo = Math.Atan2(y, x);// = -pi a +pi
        double hue = (angolo < 0 ? 2 * Math.PI + angolo : angolo)*180.0/ Math.PI;// 0° - 360°
        double fattore= isConic? 1.0 - Math.Abs(2 * z - 1) : 1.0 - Math.Pow(2 * z - 1, 2);
        double saturation = raggio / fattore;
        return new ColorHSL(hue,saturation,z);
    }

    public bool Equals(ColorHSL other) {
        return this.Hue == other.Hue && this.Saturation == other.Saturation && this.Lightness == other.Lightness;
    }
    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj == null || obj is not ColorHSL)
            return false;
        return this.Equals(obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Hue, Saturation, Lightness);
    }

    public override string ToString() {//"f3""d""d"
        return $"hsl({this.Hue:F2}, {this.Saturation:F2}%, {this.Lightness:F2}%)";//, {this.Alpha}
    }

    /* public int CompareTo(ColorHSL? other) {
         if (other == null) return 1;
         if (this.Hue != other.Hue) return Math.Sign(this.Hue.CompareTo(other.Hue));
         if (this.Saturation != other.Saturation) return Math.Sign(this.Saturation.CompareTo(other.Saturation));
         return Math.Sign(this.Lightness.CompareTo(other.Lightness));
     }*/

}
