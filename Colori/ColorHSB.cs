using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg.Colors;

public readonly struct ColorHSB : IEquatable<ColorHSB> {//, IComparable<ColorHSB>
    /// <summary>
    /// Hue 0.0°-360.0°
    /// </summary>
    public readonly double Hue;
    /// <summary>
    /// Saturation 0.0%-100.0%
    /// </summary>
    public readonly double Saturation;
    /// <summary>
    /// Brightness 0.0%-100.0%
    /// </summary>
    public readonly double Brightness;   

    public ColorHSB(double hue, double saturation, double brightness) {
        this.Hue = Math.Round(Math.Clamp(hue, 0.0, 360.0), 3);
        this.Saturation = Math.Round(Math.Clamp(saturation, 0.0, 100.0), 2);
        this.Brightness = Math.Round(Math.Clamp(brightness, 0.0, 100.0), 2);
    }

    public bool Equals(ColorHSB other) {
        return this.Hue == other.Hue && this.Saturation == other.Saturation && this.Brightness == other.Brightness;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj == null || obj is not ColorHSB)
            return false;
        return this.Equals(obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Hue, Saturation, Brightness);
    }

    public override string ToString() {//"f3""d""d"
        return $"hsb({this.Hue:F2}, {this.Saturation:F2}%, {this.Brightness:F2}%)";//, {this.Alpha}
    }

    /*public int CompareTo(ColorHSB? other) {
        if (other == null) return 1;
        if (this.Hue != other.Hue) return Math.Sign(this.Hue.CompareTo(other.Hue));
        if (this.Saturation != other.Saturation) return Math.Sign(this.Saturation.CompareTo(other.Saturation));
        return Math.Sign(this.Brightness.CompareTo(other.Brightness));
    }*/
}
