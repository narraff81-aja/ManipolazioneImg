using ManipolazioneImg.Colors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg;

public readonly struct Point3D : IEquatable<Point3D>{
    public readonly Int16 X, Y, Z;
    public Point3D(Int16 x, Int16 y, Int16 z) {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public bool Equals(Point3D other) {
        return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
    }

    public override string ToString() {
        return $"P 3D({X}, {Y}, {Z})";
    }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (obj == null || obj is not Point3D)
            return false;
        return this.Equals(obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(X,Y,Z);
    }
    public static double Distance(Point3D p1, Point3D p2) {
        return Math.Sqrt(Math.Pow((p1.X - p2.X), 2) +
            Math.Pow((p1.Y - p2.Y), 2) + Math.Pow((p1.Z - p2.Z), 2));
    }

    public double Distance(Point3D other) {
        return Point3D.Distance(this, other);
    }
    /// <summary>
    /// distanza al quadrato
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static double DistanceSqr(Point3D p1, Point3D p2) {
        return (p1.X - p2.X)*(p1.X - p2.X) +
                (p1.Y - p2.Y)*(p1.Y - p2.Y)+
                (p1.Z - p2.Z)*(p1.Z - p2.Z);
    }
    /// <summary>
    /// distanza al quadrato
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public double DistanceSqr(Point3D other) {
        return Point3D.DistanceSqr(this, other);
    }
}
public class Point3DComparerX : IComparer<Point3D> {
    public int Compare(Point3D a, Point3D b) {
        int cmp = a.X.CompareTo(b.X);
        if (cmp != 0) return cmp;
        cmp = a.Y.CompareTo(b.Y);
        if (cmp != 0) return cmp;
        return a.Z.CompareTo(b.Z);
    }
}

public class Point3DComparerY : IComparer<Point3D> {
    public int Compare(Point3D a, Point3D b) {
        int cmp = a.Y.CompareTo(b.Y);
        if (cmp != 0) return cmp;
        cmp = a.Z.CompareTo(b.Z);
        if (cmp != 0) return cmp;
        return a.X.CompareTo(b.X);
    }
}

public class Point3DComparerZ : IComparer<Point3D> {
    public int Compare(Point3D a, Point3D b) {
        int cmp = a.Z.CompareTo(b.Z);
        if (cmp != 0) return cmp;
        cmp = a.X.CompareTo(b.X);
        if (cmp != 0) return cmp;
        return a.Y.CompareTo(b.Y);
    }
}