using ManipolazioneImg.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg;

public class ColorItem {
    public Point3D Punto { get; }
    public int Frequenza { get; } 
    public ColorRGB ColoreOriginale { get; }


    public ColorItem(Point3D point, int count, ColorRGB originalColor) {
        Punto = point;
        Frequenza = count;
        ColoreOriginale = originalColor;
        
    }
}
public class ColorItemComparerX : IComparer<ColorItem> {
    public int Compare(ColorItem? a, ColorItem? b) {
        //MAI! if(a.Point==null || b.Point ==null)
        if(a == null && b == null) return 0;
        if (a == null) return -1;
        if (b == null) return 1;
        int cmp = a.Punto.X.CompareTo(b.Punto.X);
        if (cmp != 0) return cmp;
        cmp = a.Punto.Y.CompareTo(b.Punto.Y);
        if (cmp != 0) return cmp;
        return a.Punto.Z.CompareTo(b.Punto.Z);
    }
    
}

public class ColorItemComparerY : IComparer<ColorItem> {
    public int Compare(ColorItem? a, ColorItem? b) {
        //MAI! if(a.Point==null || b.Point ==null)
        if (a == null && b == null) return 0;
        if (a == null) return -1;
        if (b == null) return 1;
        int cmp = a.Punto.Y.CompareTo(b.Punto.Y);
        if (cmp != 0) return cmp;
        cmp = a.Punto.Z.CompareTo(b.Punto.Z);
        if (cmp != 0) return cmp;
        return a.Punto.X.CompareTo(b.Punto.X);
    }
}

public class ColorItemComparerZ : IComparer<ColorItem> {
    public int Compare(ColorItem? a, ColorItem? b) {
        //MAI! if(a.Point==null || b.Point ==null)
        if (a == null && b == null) return 0;
        if (a == null) return -1;
        if (b == null) return 1;
        int cmp = a.Punto.Z.CompareTo(b.Punto.Z);
        if (cmp != 0) return cmp;
        cmp = a.Punto.X.CompareTo(b.Punto.X);
        if (cmp != 0) return cmp;
        return a.Punto.Y.CompareTo(b.Punto.Y);
    }
}