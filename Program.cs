using ManipolazioneImg.Colors;
using ManipolazioneImg.SharpWrapper;
using SixLabors.ImageSharp.Formats.Bmp;
using SkiaSharp;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using ColorConverter = ManipolazioneImg.Colors.ColorConverter;

namespace ManipolazioneImg;

internal class Program {
    static void Menu() {
        Console.WriteLine("*** Manipolazione Immagini ***");
        Console.WriteLine();

        Console.WriteLine("** Scegli l'id della libreria da usare (0 di default) **");
        Console.WriteLine($"\t0 - SkiaSharp");
        Console.WriteLine($"\t1 - ImageSharp");
        var stringa = Console.ReadLine();
        if (int.TryParse(stringa, out var id)) {
            id = Math.Clamp(id, 0, 1);
        }
        else {
            id = 0;
        }
        bool usoSkiaSharp = id == 0;
        Console.WriteLine($"libreria scelta: {(usoSkiaSharp ? "SkiaSharp" : "ImageSharp")}");
        Console.WriteLine();

        Console.WriteLine("** Scegli l'id di un file (0 di default) **");
        string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImgInput");
        string[] imgs = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly);//non BMP ////"*.bmp"//"*.jpg"
        for (int i = 0; i < imgs.Length; i++) {
            FileInfo fi = new FileInfo(imgs[i]);
            Console.WriteLine($"\t{i} - {fi.Name}");
        }
        stringa = Console.ReadLine();
        if (int.TryParse(stringa, out id)) {
            id = Math.Clamp(id, 0, imgs.Length - 1);
        }
        else {
            id = 0;
        }
        Console.WriteLine($"id scelto: {id}");
        string file = imgs[id];
        string fileDestinaz = file.Replace("ImgInput", "ImgOutput");
        ISharpWrapper bmp = usoSkiaSharp ? new SkiaSharpWrapper(file) : new ImageSharpWrapper(file);
        //non si può usare using()... per la riassegnazione successiva
        if (bmp == null) {
            Console.Write("Vuoi tentare di usare l'altra libreia? [S/n]? ");
            stringa = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine();
            bool switchLibreria = stringa.ToUpper() != "N";
            if (switchLibreria) {
                bmp = !usoSkiaSharp ? new SkiaSharpWrapper(file) : new ImageSharpWrapper(file);
                if (bmp == null) {
                    throw new FileLoadException("Prova con un altra immagine!");
                }
            }
            else {
                throw new FileLoadException($"Prova a cambiare libreria! Usa {(!usoSkiaSharp ? "SkiaSharp" : "ImageSharp")}");
            }
        }

        Dictionary<ColorRGB, int> freqRGB = bmp.FrequenzaColori();
        Console.WriteLine($"L'immagine ha {freqRGB.Count} colori unici e frequenza massima {freqRGB.Values.Max()}");
        Console.WriteLine();

        Console.WriteLine("** Scegli lo spazio colore su cui esegui la riduzione di colori (1 di default) **");
        var values = Enum.GetNames(typeof(MetodoSpazioColore));
        for (int i = 0; i < values.Length; i++) {
            Console.WriteLine($"\t{i} - {values[i]}");
        }
        stringa = Console.ReadLine();
        if (int.TryParse(stringa, out id)) {
            id = Math.Clamp(id, 0, values.Length - 1);
        }
        else {
            id = 1;
        }
        MetodoSpazioColore mSpazio = (MetodoSpazioColore)id;
        Console.WriteLine($"Spazio Colore scelto: {mSpazio}");
        Console.WriteLine();

        Console.WriteLine("** Scegli il metodo di dithering (1 di default) **");
        values = Enum.GetNames(typeof(MetodoDithering));
        for (int i = 0; i < values.Length; i++) {
            Console.WriteLine($"\t{i} - {values[i]}");
        }
        stringa = Console.ReadLine();
        if (int.TryParse(stringa, out id)) {
            id = Math.Clamp(id, 0, values.Length - 1);
        }
        else {
            id = 1;
        }
        MetodoDithering mDithering = (MetodoDithering)id;
        Console.WriteLine($"Metodo Dithering scelto: {mDithering}");
        bool usaSerpentina = false;
        if (mDithering != MetodoDithering.nessuno) {
            Console.Write("Vuoi usare il metodo serpentine? [S/n]? ");
            stringa = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine();
            usaSerpentina = stringa.ToUpper() != "N";
        }
        Console.WriteLine($"{(usaSerpentina ? "Usa" : "Non usa")} serpentine.");
        Console.WriteLine();

        Console.WriteLine("** Palette ridotta a 256 colori **");
        bool usaPaletteWeb = false;
        Console.Write("Vuoi usare la palette con i colori web safe? [s/N]? ");
        stringa = Console.ReadKey().KeyChar.ToString();
        Console.WriteLine();
        usaPaletteWeb = stringa.ToUpper() == "S" || stringa.ToUpper() == "Y";//Y di yes
        Console.WriteLine(usaPaletteWeb ?
            "Usa la Palette con i colori Web safe." :
            "Vengono calcolati i 256 colori con la Median Cut.");
        Console.WriteLine();

        Console.WriteLine("** Inizio dei calcoli **");
        List<ColorRGB> tavolozza = usaPaletteWeb ? ManipolazioneImg.TavolozzaWebSave() : ManipolazioneImg.TavolozzaMedianCut(mSpazio, freqRGB, 256);
        bmp.SalvaImmagine(fileDestinaz, tavolozza, mSpazio, mDithering, usaSerpentina);
        bmp.Dispose();
        Console.WriteLine($"{fileDestinaz} salvato!");

    }
    static void Main(string[] args) {
        //EsempiColoriEDistanze();
        //Esempi8Colori();
        //ProvaMedianBox();
        //SkiaSharpWrapper.Prova();
        //ImageSharpWrapper.Prova();
        //ImageSharpWrapper.Prova2();
        //ConversioneApe(2);
        //ConversioneApe2();
        //Conversione();
        Menu();
    }
    static void ConversioneApe(int idSalvataggio = 0) {
        string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImgInput", "apnear40-bee-7418975_1920.jpg");
        string fileOut = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImgOutput", "apnear40-bee-7418975_1920.jpg");
        //Console.WriteLine(File.Exists(file));
        using (SkiaSharpWrapper ssw = new SkiaSharpWrapper(file)) {
            Dictionary<ColorRGB, int> freqRGB = ssw.FrequenzaColori();
            /*List<string> righeCSV = new();
            foreach(var item in freqRGB) {
                righeCSV.Add($"{item.Key.Red}|{item.Key.Green}|{item.Key.Blue}|{item.Value}");
            }
            File.WriteAllLines(fileOut.Replace(".jpg", ".csv"), righeCSV);
            Console.WriteLine($"{righeCSV.Count} freqRGB salvate!");*/
            MetodoSpazioColore mSpazio = MetodoSpazioColore.RGB;
            List<ColorRGB> tavolozza = ManipolazioneImg.TavolozzaMedianCut(mSpazio, freqRGB, 256);
            Console.WriteLine($"Tavolozza: {tavolozza.Count} colori");
            MetodoDithering mDithering = MetodoDithering.JarvisJudiceNinke;
            bool usaSerpentina = true;
            Stopwatch stopwatch = new Stopwatch();
            TimeSpan ts;
            stopwatch.Start();
            switch (idSalvataggio) {
                case 1:
                    ssw.SalvaImmagine0(fileOut.Replace(".jpg", "_0.jpg"), tavolozza, mSpazio, mDithering, usaSerpentina, true);
                    break;
                case 2:
                    ssw.SalvaImmagine2(fileOut.Replace(".jpg", "_2.jpg"), tavolozza, mSpazio, mDithering, usaSerpentina, true);
                    break;
                default:
                    ssw.SalvaImmagine(fileOut, tavolozza, mSpazio, mDithering, usaSerpentina, true);
                    break;
            }
            stopwatch.Stop();
            ts = stopwatch.Elapsed;
            Console.WriteLine($"Skia#: SalvaImmagine ha impiegato {ts.TotalMilliseconds} ms");
        }
        Console.WriteLine($"ok: {file}");
    }
    static void ConversioneApe2() {
        string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImgInput", "apnear40-bee-7418975_1920.jpg");
        string fileOut = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImgOutput", "apnear40-bee-7418975_1920_i.jpg");
        //Console.WriteLine(File.Exists(file));
        using (ImageSharpWrapper isw = new ImageSharpWrapper(file)) {
            Dictionary<ColorRGB, int> freqRGB = isw.FrequenzaColori();
            /*List<string> righeCSV = new();
            foreach(var item in freqRGB) {
                righeCSV.Add($"{item.Key.Red}|{item.Key.Green}|{item.Key.Blue}|{item.Value}");
            }
            File.WriteAllLines(fileOut.Replace(".jpg", ".csv"), righeCSV);
            Console.WriteLine($"{righeCSV.Count} freqRGB salvate!");*/
            MetodoSpazioColore mSpazio = MetodoSpazioColore.RGB;
            List<ColorRGB> tavolozza = ManipolazioneImg.TavolozzaMedianCut(mSpazio, freqRGB, 256);
            Console.WriteLine($"Tavolozza: {tavolozza.Count} colori");
            MetodoDithering mDithering = MetodoDithering.FloydSteinberg;
            bool usaSerpentina = true;
            Stopwatch stopwatch = new Stopwatch();
            TimeSpan ts;
            stopwatch.Start();
            isw.SalvaImmagine(fileOut, tavolozza, mSpazio, mDithering, usaSerpentina, true);
            stopwatch.Stop();
            ts = stopwatch.Elapsed;
            Console.WriteLine($"Image#: SalvaImmagine ha impiegato {ts.TotalMilliseconds} ms");/**/
        }
        Console.WriteLine($"ok: {file}");
    }
    static void Conversione() {
        // Path relativo alla cartella di esecuzione (grazie alla regola nel .csproj)
        string assets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ImgInput");
        string[] imgs = Directory.GetFiles(assets, "*.bmp", SearchOption.AllDirectories);//non BMP ////"*.bmp"//"*.jpg"
        foreach (string file in imgs) {
            try {
                using (ISharpWrapper isw = new SkiaSharpWrapper(file)) {
                    //isw =new ImageSharpWrapper(file);
                    MetodoSpazioColore mSpazio = MetodoSpazioColore.RGB;
                    MetodoDithering mDithering = MetodoDithering.JarvisJudiceNinke;
                    bool usaSerpentina = true;
                    // 1) Frequenze colori.
                    // Dall'Immagine di partenza: Leggi i pixel in formato sRGB dal file con SkiaSharp.
                    Dictionary<ColorRGB, int> freqRGB = isw.FrequenzaColori(true);

                    // 2) Calcolo della Tavolozza
                    // Per il Median Cut: Converti gli sRGB in Point3D (tramite LAB o HSL Parabolico)
                    // per riempire il dizionario e dividere lo spazio in scatole.
                    // Per la Tavolozza finale (256 colori): Una volta isolata una "scatola" di colori,
                    // la cosa più intelligente e precisa da fare è fare la media aritmetica dei colori
                    // sRGB originali contenuti in quella scatola, ottenendo direttamente il colore finale
                    // come ColorRGB.
                    List<ColorRGB> tavolozza = ManipolazioneImg.TavolozzaMedianCut(mSpazio, freqRGB, 256);
                    //List<ColorRGB> tavolozza = ManipolazioneImg.TavolozzaWebSave();
                    Console.WriteLine($"Tavolozza fissa: {tavolozza.Count}");

                    // 3) Applicazione della Quantizzazione e del Dittering
                    isw.SalvaImmagine(file.Replace("Assets", "Output"),
                        tavolozza, mSpazio, mDithering, usaSerpentina, true);
                }
                Console.WriteLine($"ok: {file}");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            //break;//per solo l'ape
        }
    }
    [Obsolete]
    static void ProvaMedianBox() {
        MedianBox mb = new MedianBox();
        Console.WriteLine(mb.RangeX);
        Console.WriteLine(mb.RangeY);
        Console.WriteLine(mb.RangeZ);
        Console.WriteLine(mb.RangeMax);
        Console.WriteLine(mb.TotFrequenze); Console.WriteLine();
        Point3D p = new Point3D(255, 125, 12);
        mb.Add(p, 100);
        Console.WriteLine(mb.RangeX);
        Console.WriteLine(mb.RangeY);
        Console.WriteLine(mb.RangeZ);
        Console.WriteLine(mb.RangeMax);
        Console.WriteLine(mb.TotFrequenze); Console.WriteLine();
        p = new Point3D(5, 158, 185);
        mb.Add(p, 75);
        Console.WriteLine(mb.RangeX);
        Console.WriteLine(mb.RangeY);
        Console.WriteLine(mb.RangeZ);
        Console.WriteLine(mb.RangeMax);
        Console.WriteLine(mb.TotFrequenze); Console.WriteLine();
        p = new Point3D(255, 125, 12);
        mb.Add(p, 7);
        Console.WriteLine(mb.RangeX);
        Console.WriteLine(mb.RangeY);
        Console.WriteLine(mb.RangeZ);
        Console.WriteLine(mb.RangeMax);
        Console.WriteLine(mb.TotFrequenze); Console.WriteLine();
        Console.WriteLine(mb.Remove(p)); Console.WriteLine();
        Console.WriteLine(mb.RangeX);
        Console.WriteLine(mb.RangeY);
        Console.WriteLine(mb.RangeZ);
        Console.WriteLine(mb.RangeMax);
        Console.WriteLine(mb.TotFrequenze);
    }

    static void EsempiColoriEDistanze() {
        ColorRGB c1 = new ColorRGB();
        ColorRGB c2 = new ColorRGB(255, 255, 255);
        ColorRGB c3 = new ColorRGB(127, 9, 255);
        Console.WriteLine(c1.ToString());
        Console.WriteLine(c2.ToString());
        Console.WriteLine(c3.ToString());
        Console.WriteLine(c1.ToHexString());
        Console.WriteLine(c2.ToHexString());
        Console.WriteLine(c3.ToHexString());
        Point3D p1 = new Point3D();
        Point3D p2 = new Point3D(255, 255, 255);
        Point3D p3 = new Point3D(127, 9, 255);
        Console.WriteLine(p1.ToString());
        Console.WriteLine(p2.ToString());
        Console.WriteLine(p3.ToString());

        Console.WriteLine(Point3D.Distance(c1.ToPoint3D(), c2.ToPoint3D()));
        Console.WriteLine(Point3D.Distance(c1.ToPoint3D(), c3.ToPoint3D()));
        Console.WriteLine(Point3D.Distance(c2.ToPoint3D(), c1.ToPoint3D()));
        Console.WriteLine(Point3D.Distance(c2.ToPoint3D(), c3.ToPoint3D()));
        Console.WriteLine(Point3D.Distance(c3.ToPoint3D(), c1.ToPoint3D()));
        Console.WriteLine(Point3D.Distance(c3.ToPoint3D(), c2.ToPoint3D()));
        ColorRGB rgb = new ColorRGB(127, 255, 212);//rgb(127, 255, 212)
        Console.WriteLine(rgb);
        ColorHSL hsl = ColorConverter.RgbToHsl(rgb);//hsl(159.844, 100%, 75%)
        Console.WriteLine(hsl);
        ColorHSB hsb = ColorConverter.RgbToHsb(rgb); //hsb(159.84, 50.20%, 100%) <-        
        Console.WriteLine(hsb);//hwb(159.844, 50%, 0%)
        ColorXYZ xyz = ColorConverter.RgbToXyz(rgb);//xyz(56.4%, 80.8%, 74.9%)
        Console.WriteLine(xyz);
        ColorLAB lab = ColorConverter.XyzToLab(xyz);//lab(92,03%, -45,52%, 9,72%) //D65 <-        
        Console.WriteLine(lab);//lab(91.865%, -35.878%, 7.323%) //D50
        c1 = new ColorRGB(127, 9, 255);
        c2 = new ColorRGB(127, 9, 255);
        Console.WriteLine(c1.Equals(c2));
    }

    static void Esempi8Colori() {
        List<string> nomi = new();
        List<(byte, byte, byte)> rgb = new();
        List<(double, double, double)> xyz = new();
        List<(double, double, double)> lab = new();
        /*nomi.Add("Nero");
        rgb.Add((0, 0, 0));
        xyz.Add((0.0000, 0.0000, 0.0000));
        lab.Add((0.00, 0.00, 0.00)) ; 
        nomi.Add("Bianco");
        rgb.Add((255, 255, 255));
        xyz.Add((0.9505, 1.0000,  1.0888));
        lab.Add((100.00, 0.00, 0.00));
        nomi.Add("Grigio Medio");
        rgb.Add((128, 128, 128));
        xyz.Add((0.2034, 0.2140,  0.2330));
        lab.Add((53.39, 0.00, 0.00));
        nomi.Add("Rosso Puro");
        rgb.Add((255, 0, 0));
        xyz.Add((0.4125,  0.2127,  0.0193));
        lab.Add((53.23, 80.11, 67.22));
        nomi.Add("Verde Puro");
        rgb.Add((0, 255, 0));
        xyz.Add((0.3576,  0.7152,  0.1192));
        lab.Add((87.74, -86.18, 83.18));
        nomi.Add("Blu Puro");
        rgb.Add((0, 0, 255));
        xyz.Add((0.1804,  0.0722, 0.9503));
        lab.Add((32.30, 79.20, -107.86));
        nomi.Add("Giallo");
        rgb.Add((255, 255, 0));
        xyz.Add((0.7700, 0.9278, 0.1385));
        lab.Add((97.14, -21.56, 94.48));
        nomi.Add("Rosa Pastello");
        rgb.Add((255, 192, 203));
        xyz.Add((0.6128, 0.5510, 0.6015));
        lab.Add((79.10, 25.59, 2.91));*/


        nomi.Add("Ciano Puro");
        rgb.Add((0, 255, 255));
        xyz.Add((0.5380, 0.7873, 1.0695));
        lab.Add((91.11, -48.08, -14.13));
        nomi.Add("Magenta Puro");
        rgb.Add((255, 0, 255));
        xyz.Add((0.5929, 0.2848, 0.9696));
        lab.Add((60.32, 98.23, -60.83));
        nomi.Add("Arancione");
        rgb.Add((255, 165, 0));
        xyz.Add((0.5457, 0.4274, 0.0658));
        lab.Add((71.38, 31.51, 75.52));
        nomi.Add("Viola Scuro");
        rgb.Add((128, 0, 128));
        xyz.Add((0.1283, 0.0616, 0.2098));
        lab.Add((29.80, 58.74, -36.46));
        nomi.Add("Verde Oliva");
        rgb.Add((128, 128, 0));
        xyz.Add((0.1664, 0.2005, 0.0299));
        lab.Add((51.89, -12.92, 56.54));
        nomi.Add("Grigio Chiaro");
        rgb.Add((192, 192, 192));
        xyz.Add((0.5058, 0.5323, 0.5796));
        lab.Add((77.96, -0.00, 0.00));
        nomi.Add("Marrone");
        rgb.Add((165, 42, 42));
        xyz.Add((0.1808, 0.1118, 0.0326));
        lab.Add((39.88, 50.08, 29.47));
        nomi.Add("Blu Notte");
        rgb.Add((25, 25, 112));
        xyz.Add((0.0177, 0.0116, 0.1506));
        lab.Add((10.25, 22.09, -44.20));


        for (int i = 0; i < nomi.Count; i++) {
            Console.WriteLine();
            Console.WriteLine(nomi[i]);
            ColorRGB a = new ColorRGB(rgb[i].Item1, rgb[i].Item2, rgb[i].Item3);
            Console.WriteLine(a.ToString());
            ColorXYZ b = ColorConverter.RgbToXyz(a);
            Console.WriteLine(b.ToString());
            Console.WriteLine(xyz[i]);
            ColorLAB c = ColorConverter.XyzToLab(b);
            Console.WriteLine(c.ToString());
            Console.WriteLine(lab[i]);
        }

    }



}
