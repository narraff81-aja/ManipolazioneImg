using ManipolazioneImg.Colors;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg;

public class MedianCutBox {
    private List<ColorItem> _Items;

    // Limiti del Bounding Box nello spazio 3D
    public int MinX { get; private set; }
    public int MaxX { get; private set; }
    public int MinY { get; private set; }
    public int MaxY { get; private set; }
    public int MinZ { get; private set; }
    public int MaxZ { get; private set; }

    // Numero totale di pixel rappresentati da questa scatola
    public int PixelTotali { get; private set; }
    // Serve a non esporre _Items nemmeno come readonly
    public int ItemsCount { get; private set; }

    public MedianCutBox(List<ColorItem> items) {
        _Items = items;
        ItemsCount = _Items.Count;
        CalculateBounds();
    }

    /// <summary>
    /// Calcola i limiti min/max lungo gli assi 3D e il numero totale di pixel.
    /// </summary>
    private void CalculateBounds() {
        if (_Items.Count == 0) return;

        MinX = int.MaxValue; MaxX = int.MinValue;
        MinY = int.MaxValue; MaxY = int.MinValue;
        MinZ = int.MaxValue; MaxZ = int.MinValue;
        PixelTotali = 0;

        foreach (var item in _Items) {
            if (item.Punto.X < MinX) MinX = item.Punto.X;
            if (item.Punto.X > MaxX) MaxX = item.Punto.X;

            if (item.Punto.Y < MinY) MinY = item.Punto.Y;
            if (item.Punto.Y > MaxY) MaxY = item.Punto.Y;

            if (item.Punto.Z < MinZ) MinZ = item.Punto.Z;
            if (item.Punto.Z > MaxZ) MaxZ = item.Punto.Z;

            PixelTotali += item.Frequenza;
        }
    }

    /// <summary>
    /// Restituisce la dimensione dell'asse più lungo (range massimo).
    /// </summary>
    public (int max, char asse) RangeMax() {
        int rangeX = MaxX - MinX;
        int rangeY = MaxY - MinY;
        int rangeZ = MaxZ - MinZ;

        if (rangeX >= rangeY && rangeX >= rangeZ) {
            return (rangeX, 'x');
        }
        else if (rangeY >= rangeX && rangeY >= rangeZ) {
            return (rangeY, 'y');
        }
        else {
            return (rangeZ, 'z');
        }
    }

    /// <summary>
    /// Taglia la scatola in due sotto-scatole lungo l'asse di massima varianza.
    /// </summary>
    public (MedianCutBox Box1, MedianCutBox? Box2) Split() {
        if (_Items.Count < 2) {
            //throw new InvalidOperationException("Impossibile dividere una scatola con meno di 2 colori.");
            return (this, null);
        }

        var (max, asse) = RangeMax();
        switch (asse) {
            case 'x':
                _Items.Sort(new ColorItemComparerX());
                break;
            case 'y':
                _Items.Sort(new ColorItemComparerY());
                break;
            default:
                _Items.Sort(new ColorItemComparerZ());
                break;
        }

        // Si trova il punto di taglio mediano (weighted median basata sulla frequenza dei pixel)
        int halfPixels = PixelTotali / 2;
        int accumulatedPixels = 0;
        int medianIndex = 0;

        for (int i = 0; i < _Items.Count; i++) {
            int curr = _Items[i].Frequenza;
            if (accumulatedPixels + curr <= PixelTotali - accumulatedPixels) {
                accumulatedPixels += curr;
            }
            else {
                medianIndex = i - 1;
                break;
            }
        }

        // Evita che una scatola rimanga vuota se il primo elemento da solo supera la metà dei pixel
        if (medianIndex == _Items.Count - 1)
            medianIndex = _Items.Count - 2;

        int splitCount = medianIndex + 1;

        // Si creano le due nuove sotto-scatole
        var leftItems = _Items.GetRange(0, splitCount);
        var rightItems = _Items.GetRange(splitCount, _Items.Count - splitCount);

        return (new MedianCutBox(leftItems), new MedianCutBox(rightItems));
    }

    /// <summary>
    /// Calcola il colore rappresentativo (medio ponderato) della scatola in formato RGB.
    /// </summary>
    public ColorRGB Centroide() {
        long sumR = 0, sumG = 0, sumB = 0;
        long totalCount = 0;

        foreach (var item in _Items) {
            sumR += (long)item.ColoreOriginale.Red * item.Frequenza;
            sumG += (long)item.ColoreOriginale.Green * item.Frequenza;
            sumB += (long)item.ColoreOriginale.Blue * item.Frequenza;
            totalCount += item.Frequenza;
        }

        if (totalCount == 0) return new ColorRGB(0, 0, 0);

        return new ColorRGB(
            (byte)(sumR / totalCount),
            (byte)(sumG / totalCount),
            (byte)(sumB / totalCount)
        );
    }
}

