using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg.Colors;

public readonly struct ColorRGB: IEquatable<ColorRGB> {//, IComparable<ColorRGB>
    /// <summary>
    /// Red 0-255
    /// </summary>
    public readonly byte Red;
    /// <summary>
    /// Green 0-255
    /// </summary>
    public readonly byte Green;
    /// <summary>
    /// Blue 0-255
    /// </summary>
    public readonly byte Blue;
    
    public ColorRGB(byte red, byte green, byte blue) {        
        this.Red = red;
        this.Green = green;
        this.Blue = blue;
    }
    public ColorRGB() :this(0,0,0){
    }

    public string ToHexString() {
       return $"#{ Convert.ToHexString(new[] { this.Red, this.Green, this.Blue })}";
    }
    public static ColorRGB FromHexString(string hexColor) {
        if( hexColor[0]=='#')
            hexColor= hexColor.Substring(1);

        var ris= Convert.FromHexString(hexColor);
        return new ColorRGB(ris[0], ris[1], ris[2]);
    }
    public static Point3D ToPoint3D(ColorRGB color) {
        return new Point3D(color.Red, color.Green, color.Blue);
    }
    public Point3D ToPoint3D() {
        return ToPoint3D(this);
    }

    public static ColorRGB FromPoint3D(Point3D p3d) {
        return new ColorRGB((byte)Math.Clamp(p3d.X,byte.MinValue,byte.MaxValue),
            (byte)Math.Clamp(p3d.Y, byte.MinValue, byte.MaxValue),
            (byte)Math.Clamp(p3d.Z, byte.MinValue, byte.MaxValue));
    }

    public bool Equals(ColorRGB other) {
        return this.Red == other.Red && this.Green == other.Green && this.Blue == other.Blue;
    }


    public override string ToString() {
        return $"rgb({this.Red}, {this.Green}, {this.Blue})";//, {this.Alpha}
    }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj == null || obj is not ColorRGB)
            return false;
        return this.Equals(obj);
    }

    public override int GetHashCode() { 
        return HashCode.Combine(Red,Green,Blue); 
    }

/*
    public int CompareTo(ColorRGB? other) { 
        if (other==null) return 1;
        if (this.Red != other.Red) return Math.Sign( this.Red.CompareTo(other.Red));
        if (this.Green != other.Green) return Math.Sign(this.Green.CompareTo(other.Green));
        return Math.Sign(this.Blue.CompareTo(other.Blue));
    }*/
}
