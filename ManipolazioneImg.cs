using ManipolazioneImg.Colors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using Path = System.IO.Path;


namespace ManipolazioneImg;

public enum MetodoSpazioColore {
    RGB, HSL_conico, HSL_parabolico, CIELAB
}
public enum MetodoDithering {
    nessuno, FloydSteinberg, JarvisJudiceNinke, Stucki
}
public class ManipolazioneImg {
    /// <summary>
    /// Tavolozza dei colori Web Save
    /// </summary>
    /// <returns></returns>
    public static List<ColorRGB> TavolozzaWebSave() {
        List<ColorRGB> ris = new();
        foreach (var item in ColorRGBWeb.WebPalette) {
            if (!ris.Contains(item.Value)) {
                // evita le doppie varianti "*gray*"/"*grey*"
                ris.Add(item.Value);
            }
        }
        return ris;
    }
    /// <summary>
    /// Tavolozza dei colori tramite Median Cut
    /// </summary>
    /// <param name="spazio"></param>
    /// <param name="freqRgb"></param>
    /// <param name="maxColor"></param>
    /// <returns></returns>
    public static List<ColorRGB> TavolozzaMedianCut(MetodoSpazioColore spazio, Dictionary<ColorRGB, int> freqRgb, int maxColor = 256) {
        if(freqRgb.Count<= maxColor) {          
            return freqRgb.Keys.ToList();
        }
        
        int totFrequenze = freqRgb.Values.Sum();
        //Console.WriteLine($"totFrequenze {totFrequenze}");
        int dimMinimaColore = (int)Math.Floor((double)totFrequenze / maxColor);
        //Console.WriteLine($"dimMinimaColore {dimMinimaColore}");
        
        List<ColorRGB> ris = new();
        List<ColorItem> colors = new();

        foreach (var item in freqRgb) {
            if (item.Value >= dimMinimaColore) {
                // estrae i primi colori
                ris.Add(item.Key);
            }
            else {
                Point3D p3d = PixelToPoint3D(item.Key, spazio);                
                colors.Add(new ColorItem(p3d, item.Value, item.Key));
            }
        }
        // Calcola le scatole rimanenti
        maxColor -= ris.Count;
        List<MedianCutBox> boxes = new() {
            new MedianCutBox(colors)
        };

        while (boxes.Count < maxColor) {
            MedianCutBox? Range0 = boxes.Where(b => b.ItemsCount < 2).
                OrderBy(b => b.RangeMax().max).FirstOrDefault();
            if (Range0 != null) {
                // togli il MedianBox mono colore
                boxes.Remove(Range0);
                // calcolo del centroide
                ris.Add(Range0.Centroide());
                // aggiornamento ris/maxColor
                maxColor--;
                continue;
            }
            MedianCutBox? boxDaSplittare = boxes.Where(b => b.ItemsCount > 1).
                OrderByDescending(b => b.RangeMax().max).FirstOrDefault();
            if (boxDaSplittare == null) {
                break;
            }
            boxes.Remove(boxDaSplittare);
            var (box1, box2) = boxDaSplittare.Split();
            if (box2 == null)
                break;
            boxes.Add(box1);
            boxes.Add(box2);
        }
        // Per ogni scatola occorre trovare il centroide e convertirlo in RGB
        foreach (MedianCutBox box in boxes) {
            ris.Add(box.Centroide());
        }
        return ris;
    }

    /// <summary>
    /// Trasforma un pixel dal file in Point3D in base allo spazio
    /// </summary>
    /// <param name="colore"></param>
    /// <param name="mSpazio"></param>
    /// <returns></returns>
    public static Point3D PixelToPoint3D(ColorRGB colore, MetodoSpazioColore mSpazio) {
        return mSpazio switch {
            MetodoSpazioColore.HSL_conico => ColorConverter.RgbToHsl(colore).ToConicPoint3D(),
            MetodoSpazioColore.HSL_parabolico => ColorConverter.RgbToHsl(colore).ToPoint3D(),
            MetodoSpazioColore.CIELAB => ColorConverter.RgbToLab(colore).ToPoint3D(),
            _ => colore.ToPoint3D(),
        };
    }
    /// <summary>
    /// Converte una tavolozza allo spazio Point3D desiderato
    /// </summary>
    /// <param name="tavolozza"></param>
    /// <param name="mSpazio"></param>
    /// <returns></returns>
    public static List<Point3D> Tavolozza3D(List<ColorRGB> tavolozza, MetodoSpazioColore mSpazio) {
        List<Point3D> ris = new();
        tavolozza.ForEach(colore => ris.Add(ManipolazioneImg.PixelToPoint3D(colore, mSpazio)));
        return ris;
    }
    /// <summary>
    /// Trova l'id del colore più vicino in base alla tavolozza
    /// </summary>
    /// <param name="attuale"></param>
    /// <param name="tavolozza"></param>
    /// <param name="mSpazio"></param>
    /// <returns></returns>
    [Obsolete("Meglio usare solo lo spazio RGB per la sostituzione!", true)]
    public static int TrovaIDColore(ColorRGB attuale, List<Point3D> tavolozza, MetodoSpazioColore mSpazio) {
        Point3D corrente = ManipolazioneImg.PixelToPoint3D(attuale, mSpazio);
        int id = 0;
        double distanzaMinima = corrente.DistanceSqr(tavolozza[0]);
        for (int i = 1; i < tavolozza.Count; i++) {
            double distanza = corrente.DistanceSqr(tavolozza[i]);
            if (distanza < distanzaMinima) {
                distanzaMinima = distanza;
                id = i;
            }
        }
        return id;
    }
    /// <summary>
    /// Trova l'id del colore più vicino in base alla tavolozza
    /// </summary>
    /// <param name="attuale"></param>
    /// <param name="tavolozza"></param>
    /// <returns></returns>
    public static int TrovaIDColore(ColorRGB attuale, List<ColorRGB> tavolozza) {        
        int id = 0;
        double distanzaMinima = attuale.ToPoint3D().DistanceSqr(tavolozza[0].ToPoint3D());
        for (int i = 1; i < tavolozza.Count; i++) {
            double distanza = attuale.ToPoint3D().DistanceSqr(tavolozza[i].ToPoint3D());
            if (distanza < distanzaMinima) {
                distanzaMinima = distanza;
                id = i;
            }
        }
        return id;
    }
}
