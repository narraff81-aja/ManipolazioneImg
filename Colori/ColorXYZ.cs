using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg.Colors;

public readonly struct ColorXYZ: IEquatable<ColorXYZ> {
    /// <summary>
    /// 0.0%-100.0%
    /// </summary>
    public readonly double X;
    /// <summary>
    /// 0.0%-100.0%
    /// </summary>
    public readonly double Y;
    /// <summary>
    /// 0.0%-100.0%
    /// </summary>
    public readonly double Z;
    public ColorXYZ(double x, double y, double z) {
        this.X = x;// Math.Round(Math.Max(Math.Min(x, 100.0), 0.0), 2) ;
        this.Y = y;// Math.Round(Math.Max(Math.Min(y, 100.0), 0.0), 2);
        this.Z = z;// Math.Round(Math.Max(Math.Min(z, 100.0), 0.0), 2);
    }

    public bool Equals(ColorXYZ other) {
        return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
    }
    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj == null || obj is not ColorXYZ)
            return false;
        return this.Equals(obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(X, Y, Z);
    }
    public override string ToString() {
        return $"xyz({X:F2}%, {Y:F2}%, {Z:F2}%)";
    }
}
