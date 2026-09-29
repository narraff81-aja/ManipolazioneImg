using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg.Colors;

public readonly struct ColorLAB: IEquatable<ColorLAB> {//, IComparable<ColorLAB>
    /// <summary>
    /// Luminosità (0 a 100) 
    /// </summary>
    public readonly double L;
    /// <summary>
    /// Asse Verde-Rosso (da -128 a 128 circa)
    /// </summary>
    public readonly double a;
    /// <summary>
    /// Asse Blu-Giallo (da -128 a 128 circa)
    /// </summary>
    public readonly double b;
    public ColorLAB(double l, double a, double b) {
        this.L = l;
        this.a = a;
        this.b = b;
    }

    public static Point3D ToPoint3D(ColorLAB color) {
        return new Point3D((Int16)Math.Round(100 * color.L), (Int16)Math.Round(100 * color.a), (Int16)Math.Round(100 * color.b));
    }
    public Point3D ToPoint3D() {
        return ToPoint3D(this);
    }

    public static ColorLAB FromPoint3D(Point3D p3d) {
        return new ColorLAB(p3d.X/100.0,p3d.Y / 100.0,p3d.Z / 100.0);
    }

    public bool Equals(ColorLAB other) {
        return this.L == other.L && this.a == other.a && this.b == other.b;
    }
    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj == null || obj is not ColorLAB)
            return false;
        return this.Equals(obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(L, a, b);
    }
    public override string ToString() {
        return $"lab({L:F2}%, {a:F2}%, {b:F2}%)";
    }
}
