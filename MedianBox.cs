using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg;

[Obsolete("Uso lento di LINQ",true)]
public class MedianBox {
    private Dictionary<Point3D, int> Punti = new();
    public Int16 MinX { get; private set; } = Int16.MaxValue;
    public Int16 MinY { get; private set; } = Int16.MaxValue;
    public Int16 MinZ { get; private set; } = Int16.MaxValue;
    public Int16 MaxX { get; private set; } = Int16.MinValue;
    public Int16 MaxY { get; private set; } = Int16.MinValue;
    public Int16 MaxZ { get; private set; } = Int16.MinValue;

    public int RangeX { get => Punti.Count > 0 ? MaxX - MinX : 0; }
    public int RangeY { get => Punti.Count > 0 ? MaxY - MinY : 0; }
    public int RangeZ { get => Punti.Count > 0 ? MaxZ - MinZ : 0; }

    public int RangeMax { get => Math.Max(Math.Max(RangeX, RangeY), RangeZ); }

    public int TotFrequenze { get; private set; }

    public List<Point3D> Sort() {
        List<Point3D> ris = Punti.Keys.ToList();
        if (RangeMax == RangeX)
            ris.Sort(new Point3DComparerX());            
        if (RangeMax == RangeY)
            ris.Sort(new Point3DComparerY());
        if (RangeMax == RangeZ)
            ris.Sort(new Point3DComparerZ());
        return ris;
    }

    public void Add(Point3D punto, int frequenza = 1) {
        TotFrequenze += frequenza;
        if (Punti.ContainsKey(punto)) {
            Punti[punto] += frequenza;
        }
        else {
            Punti.Add(punto, frequenza);
            if (MinX > punto.X)
                MinX = punto.X;
            if (MinY > punto.Y)
                MinY = punto.Y;
            if (MinZ > punto.Z)
                MinZ = punto.Z;
            if (MaxX < punto.X)
                MaxX = punto.X;
            if (MaxY < punto.Y)
                MaxY = punto.Y;
            if (MaxZ < punto.Z)
                MaxZ = punto.Z;
        }
    }
    public int Get(Point3D punto) {
        if (Punti.ContainsKey(punto)) {             
            return Punti[punto];
        }
        else {
            return 0;
        }
    }
    public int Remove(Point3D punto) {
        if (Punti.ContainsKey(punto)) {
            int frequenza = Punti[punto];
            TotFrequenze -= frequenza;
            Punti.Remove(punto);
            // deve ricalcolare i Min/Max
            var Keys = Punti.Keys.ToList();
            MinX = Keys.Select(k => k.X).Min();
            MaxX = Keys.Select(k => k.X).Max();
            MinY = Keys.Select(k => k.Y).Min();
            MaxY = Keys.Select(k => k.Y).Max();
            MinZ = Keys.Select(k => k.Z).Min();
            MaxZ = Keys.Select(k => k.Z).Max();

            return frequenza;
        }
        else {
            return 0;
        }
    }
}
