using ManipolazioneImg.Colors;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg.SharpWrapper;

public class SkiaSharpWrapper : ISharpWrapper {
    private SKBitmap? bitmap;
    private SKEncodedImageFormat formatoImagine;
    public SkiaSharpWrapper(string pathFile) {
        this.CaricaImmagine(pathFile);
    }
    private static SKEncodedImageFormat OttieniFormatoImmagine(string percorsoFile) {
        string ext = Path.GetExtension(percorsoFile).ToLowerInvariant();
        return ext switch {
            ".png" => SKEncodedImageFormat.Png,
            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
            ".webp" => SKEncodedImageFormat.Webp,
            ".gif" => SKEncodedImageFormat.Gif,
            ".bmp" => SKEncodedImageFormat.Bmp,
            ".ico" => SKEncodedImageFormat.Ico,
            _ => SKEncodedImageFormat.Jpeg // Default di fallback
        };
    }
    public void CaricaImmagine(string pathFile) {
        FileInfo fi = new FileInfo(pathFile);
        if (!fi.Exists) {//File.Exists(percorsoFile)
            throw new FileNotFoundException($"Errore SkiaSharp: Immagine non trovata in {fi.FullName}");
        }
        // 1. Tentativo standard
        SKBitmap? skBmp = SKBitmap.Decode(fi.FullName);
        if (skBmp != null) {
            if (skBmp.ColorType == SKColorType.Rgba8888) {
                bitmap = skBmp;
                return;
            }
            var skBmp1 = new SKBitmap(skBmp.Width, skBmp.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            skBmp.CopyTo(skBmp1, SKColorType.Rgba8888);
            skBmp.Dispose();
            //Console.WriteLine("1. Tentativo");
            bitmap = skBmp1;
            formatoImagine = OttieniFormatoImmagine(fi.FullName);
            return;
        }

        // 2. Tentativo avanzato (SKCodec)
        using (var stream = File.OpenRead(fi.FullName))
        using (var codec = SKCodec.Create(stream)) {
            if (codec != null) {
                var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888);//, SKAlphaType.Premul
                var skBmp2 = new SKBitmap(info);
                if (codec.GetPixels(info, skBmp2.GetPixels()) == SKCodecResult.Success) {
                    Console.WriteLine("2. Tentativo");
                    bitmap = skBmp2;
                    formatoImagine = OttieniFormatoImmagine(fi.FullName);
                    return;
                }
                skBmp2.Dispose();
            }
        }        

        throw new InvalidOperationException($"Impossibile decodificare con SkiaSharp l'immagine\n {fi.FullName} !\n" +
            $"Prova la conversione con un programma di fotoritocco!");
    }

    public Dictionary<ColorRGB, int> FrequenzaColori(bool mostraInConsole = false) {
        if (bitmap == null)
            throw new InvalidOperationException("bitmap==null usa CaricaImmagine()!");
        if (mostraInConsole) {
            Console.WriteLine($"Dimensioni: {bitmap.Width}x{bitmap.Height} pixel");
            Console.WriteLine($"Formato colore nativo: {bitmap.ColorType}");
        }
        // Dict risultato
        Dictionary<ColorRGB, int> ris = new();

        // Accesso diretto alla memoria tramite ReadOnlySpan
        ReadOnlySpan<byte> pixelSpan = bitmap.GetPixelSpan();
        int totalPixels = bitmap.Width * bitmap.Height;

        // Ogni pixel nel formato Rgba8888 occupa esattamente 4 byte (R, G, B, A)
        const int bytesPerPixel = 4;

        // Scansione veloce dei pixel per contare le frequenze RGB
        for (int i = 0; i < totalPixels; i++) {
            int index = i * bytesPerPixel;

            byte r = pixelSpan[index];
            byte g = pixelSpan[index + 1];
            byte b = pixelSpan[index + 2];
            // L'Alpha (index + 3) viene ignorato

            ColorRGB color = new ColorRGB(r, g, b);

            if (ris.TryGetValue(color, out int count)) {
                ris[color] = count + 1;
            }
            else {
                ris[color] = 1;
            }
        }
        if (mostraInConsole) {
            Console.WriteLine($"{ris.Count} colori unici su {totalPixels}= {(ris.Count * 100.0 / totalPixels).ToString("f2")}% !");
            var maxFrequenza = ris.Values.Max();
            Console.WriteLine($"maxFrequenza: {maxFrequenza}");
            Console.WriteLine("Scansione dei pixel completata!");
        }

        return ris;
    }
    /// <summary>
    /// Salva Immagine con List come Queue
    /// </summary>
    /// <param name="percorsoFileDestinazione"></param>
    /// <param name="tavolozza"></param>
    /// <param name="spazio"></param>
    /// <param name="ditt"></param>
    /// <param name="usaSerpentina"></param>
    /// <param name="mostraInConsole"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void SalvaImmagine(string percorsoFileDestinazione,
     List<ColorRGB> tavolozza, MetodoSpazioColore spazio, MetodoDithering ditt, bool usaSerpentina,
     bool mostraInConsole = false) {
        if (bitmap == null)
            throw new InvalidOperationException("bitmap==null usa CaricaImmagine()!");
        // File I/O
        FileInfo fOut = new FileInfo(percorsoFileDestinazione);

        int width = bitmap.Width;
        int height = bitmap.Height;
        // Array di destinazione per i pixel elaborati
        SKColor[] outputPixels = new SKColor[width * height];
        // Accesso ultra-veloce alla memoria dell'immagine sorgente
        ReadOnlySpan<byte> inputBytes = bitmap.GetPixelSpan();

        // tavolozza
        //List<Point3D> tavolozza3D = ManipolazioneImg.Tavolozza3D(tavolozza, spazio);
        Dictionary<ColorRGB, ColorRGB> cacheColori = new();
        ColorRGB MappaColore(ColorRGB sorgente) {
            if (!cacheColori.TryGetValue(sorgente, out ColorRGB trovato)) {
                //int idNewColor = ManipolazioneImg.TrovaIDColore(newPixel, tavolozza3D, spazio);
                int id = ManipolazioneImg.TrovaIDColore(sorgente, tavolozza);
                trovato = tavolozza[id];
                cacheColori[sorgente] = trovato;
            }
            return trovato;
        }

        if (mostraInConsole)
            Console.WriteLine("pronto per il ciclo");
        if (ditt == MetodoDithering.nessuno) {
            // CASO DITHERING NULLO: Ciclo ultra-veloce
            for (int y = 0; y < height; y++) {
                int rigaOffset = y * width;
                for (int x = 0; x < width; x++) {
                    int byteIdx = (rigaOffset + x) * 4;
                    ColorRGB pixelOrig = new ColorRGB(inputBytes[byteIdx], inputBytes[byteIdx + 1], inputBytes[byteIdx + 2]);
                    ColorRGB pixelNuovo = MappaColore(pixelOrig);
                    outputPixels[rigaOffset + x] = new SKColor(pixelNuovo.Red, pixelNuovo.Green, pixelNuovo.Blue, inputBytes[byteIdx + 3]);
                }
            }
        }
        else {
            MatriceDithering matriceDithering = ditt switch {
                MetodoDithering.FloydSteinberg => MatriceDithering.FloydSteinberg,
                MetodoDithering.JarvisJudiceNinke => MatriceDithering.JarvisJudiceNinke,
                MetodoDithering.Stucki => MatriceDithering.Stucki,
                _ => throw new ArgumentOutOfRangeException(nameof(ditt))
            };
            // List come Queue
            List<PixelError[]> queue = new();

            for (int y = 0; y < height; y++) {
                if (mostraInConsole && y % 10 == 39)
                    Console.WriteLine("inizio la riga " + (y + 1));
                bool daDXaSX = usaSerpentina && (y % 2 == 1);
                int startX = daDXaSX ? width - 1 : 0;
                int endX = daDXaSX ? -1 : width;
                int stepX = daDXaSX ? -1 : 1;
                int rigaOffset = y * width;

                if (y == 0) {
                    for (int j = 0; j < matriceDithering.RigheBuffer - 1; j++) {
                        queue.Add(new PixelError[width]);
                    }
                }
                if (y < height - matriceDithering.RigheBuffer + 1) {
                    queue.Add(new PixelError[width]);
                }

                for (int x = startX; x != endX; x += stepX) {
                    int byteIdx = (rigaOffset + x) * 4;
                    PixelError err = queue[0][x];

                    // componenti con errore
                    int r = Math.Clamp(inputBytes[byteIdx] + err.Red / matriceDithering.Denominatore, 0, 255);
                    int g = Math.Clamp(inputBytes[byteIdx + 1] + err.Green / matriceDithering.Denominatore, 0, 255);
                    int b = Math.Clamp(inputBytes[byteIdx + 2] + err.Blue / matriceDithering.Denominatore, 0, 255);
                    
                    // pixel Nuovo
                    ColorRGB pixelNuovo = MappaColore(new ColorRGB((byte)r, (byte)g, (byte)b));

                    // Assegna il pixel nel buffer finale
                    outputPixels[rigaOffset + x] = new SKColor(pixelNuovo.Red, pixelNuovo.Green,
                        pixelNuovo.Blue, inputBytes[byteIdx + 3]);

                    // errore rimasto (Colore Originale - Colore Scelto) in RGB
                    int errR = r - pixelNuovo.Red;
                    int errG = g - pixelNuovo.Green;
                    int errB = b - pixelNuovo.Blue;

                    // Propagazione Errore: "Spalmare" l'errore sui pixel adiacenti
                    foreach (var item in matriceDithering.Celle) {
                        int newX = daDXaSX ? x - item.OffsetX : x + item.OffsetX;
                        if (0 <= newX && newX < width && item.OffsetY + y < height) {
                            queue[item.OffsetY][newX] = PixelError.Aggiungi(queue[item.OffsetY][newX], errR, errG, errB, item.Peso);
                        }
                    }
                }
                queue.RemoveAt(0);
            }
        }

        if (mostraInConsole)
            Console.WriteLine("Nuovi pixel pronti");
        using (SKBitmap bitmapElaborata = new SKBitmap(width, height)) {
            bitmapElaborata.Pixels = outputPixels; // Assegnazione diretta ultra-veloce!
            using (SKImage image = SKImage.FromBitmap(bitmapElaborata))
            using (SKData data = image.Encode(formatoImagine, 100))
            using (FileStream stream = File.OpenWrite(fOut.FullName)) {
                data.SaveTo(stream);
            }
        }
    }
    /// <summary>
    /// Salva Immagine con Buffer
    /// </summary>
    /// <param name="percorsoFileDestinazione"></param>
    /// <param name="tavolozza"></param>
    /// <param name="spazio"></param>
    /// <param name="ditt"></param>
    /// <param name="usaSerpentina"></param>
    /// <param name="mostraInConsole"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void SalvaImmagine2(string percorsoFileDestinazione,
     List<ColorRGB> tavolozza, MetodoSpazioColore spazio, MetodoDithering ditt, bool usaSerpentina,
     bool mostraInConsole = false) {
        if (bitmap == null)
            throw new InvalidOperationException("bitmap==null usa CaricaImmagine()!");
        // File I/O
        FileInfo fOut = new FileInfo(percorsoFileDestinazione);

        int width = bitmap.Width;
        int height = bitmap.Height;
        // Array di destinazione per i pixel elaborati
        SKColor[] outputPixels = new SKColor[width * height];
        // Accesso ultra-veloce alla memoria dell'immagine sorgente
        ReadOnlySpan<byte> inputBytes = bitmap.GetPixelSpan();

        // tavolozza
        //List<Point3D> tavolozza3D = ManipolazioneImg.Tavolozza3D(tavolozza, spazio);
        Dictionary<ColorRGB, ColorRGB> cacheColori = new();
        ColorRGB MappaColore(ColorRGB sorgente) {
            if (!cacheColori.TryGetValue(sorgente, out ColorRGB trovato)) {
                //int idNewColor = ManipolazioneImg.TrovaIDColore(newPixel, tavolozza3D, spazio);
                int id = ManipolazioneImg.TrovaIDColore(sorgente, tavolozza);
                trovato = tavolozza[id];
                cacheColori[sorgente] = trovato;
            }
            return trovato;
        }

        if (mostraInConsole)
            Console.WriteLine("pronto per il ciclo");
        if (ditt == MetodoDithering.nessuno) {
            // CASO DITHERING NULLO: Ciclo ultra-veloce
            for (int y = 0; y < height; y++) {
                int rowOffset = y * width;
                for (int x = 0; x < width; x++) {
                    int byteIdx = (rowOffset + x) * 4;
                    ColorRGB pixelOrig = new ColorRGB(inputBytes[byteIdx], inputBytes[byteIdx + 1], inputBytes[byteIdx + 2]);
                    ColorRGB pixelNuovo = MappaColore(pixelOrig);
                    outputPixels[rowOffset + x] = new SKColor(pixelNuovo.Red, pixelNuovo.Green, pixelNuovo.Blue, inputBytes[byteIdx + 3]);
                }
            }            
        }
        else {
            MatriceDithering matriceDithering = ditt switch {
                MetodoDithering.FloydSteinberg => MatriceDithering.FloydSteinberg,
                MetodoDithering.JarvisJudiceNinke => MatriceDithering.JarvisJudiceNinke,
                MetodoDithering.Stucki => MatriceDithering.Stucki,
                _ => throw new ArgumentOutOfRangeException(nameof(ditt))
            };
            // Matrice di errori
            PixelError[,] buffer = new PixelError[matriceDithering.RigheBuffer, width];

            for (int y = 0; y < height; y++) {
                if (mostraInConsole && (y % 10 == 39))
                    Console.WriteLine("inizio la riga " + (y + 1));
                bool daDXaSX = usaSerpentina && (y % 2 == 1);
                int startX = daDXaSX ? width - 1 : 0;
                int endX = daDXaSX ? -1 : width;
                int stepX = daDXaSX ? -1 : 1;
                int rigaOffset = y * width;
                int IdRigaCorr = y % matriceDithering.RigheBuffer;

                for (int x = startX; x != endX; x += stepX) {
                    int byteIdx = (rigaOffset + x) * 4;
                    PixelError err = buffer[IdRigaCorr, x];

                    // componenti con errore
                    int r = Math.Clamp(inputBytes[byteIdx] + err.Red / matriceDithering.Denominatore, 0, 255);
                    int g = Math.Clamp(inputBytes[byteIdx + 1] + err.Green / matriceDithering.Denominatore, 0, 255);
                    int b = Math.Clamp(inputBytes[byteIdx + 2] + err.Blue / matriceDithering.Denominatore, 0, 255);
                    
                    // pixel Nuovo
                    ColorRGB pixelNuovo = MappaColore(new ColorRGB((byte)r, (byte)g, (byte)b));

                    // Assegna il pixel nel buffer finale
                    outputPixels[rigaOffset + x] = new SKColor(pixelNuovo.Red, pixelNuovo.Green,
                        pixelNuovo.Blue, inputBytes[byteIdx + 3]);

                    // errore rimasto (Colore Originale - Colore Scelto) in RGB
                    r -= pixelNuovo.Red;
                    g -= pixelNuovo.Green;
                    b -= pixelNuovo.Blue;

                    // Propagazione Errore: "Spalmare" l'errore sui pixel adiacenti
                    foreach (var item in matriceDithering.Celle) {
                        int newX = daDXaSX ? x - item.OffsetX : x + item.OffsetX;
                        int newY = (item.OffsetY + y) % matriceDithering.RigheBuffer;
                        if (0 <= newX && newX < width && item.OffsetY + y < height) {
                            buffer[newY, newX] = PixelError.Aggiungi(buffer[newY, newX], r, g, b, item.Peso);
                        }
                    }

                    // Pulizia del valore usato nel buffer
                    buffer[IdRigaCorr, x] = new PixelError(0, 0, 0);
                }
            }
        }

        if (mostraInConsole)
            Console.WriteLine("Nuovi pixel pronti");
        using (SKBitmap bitmapElaborata = new SKBitmap(width, height)) {
            bitmapElaborata.Pixels = outputPixels; // Assegnazione diretta ultra-veloce!
            using (SKImage image = SKImage.FromBitmap(bitmapElaborata))
            using (SKData data = image.Encode(formatoImagine, 100))
            using (FileStream stream = File.OpenWrite(fOut.FullName)) {
                data.SaveTo(stream);
            }
        }
    }
    /// <summary>
    /// Salva Immagine che usa la matrice di errori
    /// </summary>
    /// <param name="percorsoFileDestinazione"></param>
    /// <param name="tavolozza"></param>
    /// <param name="spazio"></param>
    /// <param name="ditt"></param>
    /// <param name="usaSerpentina"></param>
    /// <param name="mostraInConsole"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void SalvaImmagine0(string percorsoFileDestinazione,
     List<ColorRGB> tavolozza, MetodoSpazioColore spazio, MetodoDithering ditt, bool usaSerpentina,
     bool mostraInConsole = false) {
        if (bitmap == null)
            throw new InvalidOperationException("bitmap==null usa CaricaImmagine()!");
        // File I/O
        FileInfo fOut = new FileInfo(percorsoFileDestinazione);

        int width = bitmap.Width;
        int height = bitmap.Height;
        // Array di destinazione per i pixel elaborati
        SKColor[] outputPixels = new SKColor[width * height];
        // Accesso ultra-veloce alla memoria dell'immagine sorgente
        ReadOnlySpan<byte> inputBytes = bitmap.GetPixelSpan();

        // tavolozza
        //List<Point3D> tavolozza3D = ManipolazioneImg.Tavolozza3D(tavolozza, spazio);
        Dictionary<ColorRGB, ColorRGB> cacheColori = new();
        ColorRGB MappaColore(ColorRGB sorgente) {
            if (!cacheColori.TryGetValue(sorgente, out ColorRGB trovato)) {
                //int idNewColor = ManipolazioneImg.TrovaIDColore(newPixel, tavolozza3D, spazio);
                int id = ManipolazioneImg.TrovaIDColore(sorgente, tavolozza);
                trovato = tavolozza[id];
                cacheColori[sorgente] = trovato;
            }
            return trovato;
        }

        MatriceDithering matriceDithering = ditt switch {
            MetodoDithering.FloydSteinberg => MatriceDithering.FloydSteinberg,
            MetodoDithering.JarvisJudiceNinke => MatriceDithering.JarvisJudiceNinke,
            MetodoDithering.Stucki => MatriceDithering.Stucki,
            _ => throw new ArgumentOutOfRangeException(nameof(ditt))
        };
        // Matrice di errori x 16
        PixelError[,] matriceErrori = new PixelError[width, height];
        if (mostraInConsole)
            Console.WriteLine("pronto per il ciclo");
        for (int y = 0; y < height; y++) {
            if (mostraInConsole && y % 10 == 39)
                Console.WriteLine("inizio la riga " + (y + 1));
            bool daDXaSX = usaSerpentina && (y % 2 == 1);
            int startX = daDXaSX ? width - 1 : 0;
            int endX = daDXaSX ? -1 : width;
            int stepX = daDXaSX ? -1 : 1;
            int rigaOffset = y * width;

            for (int x = startX; x != endX; x += stepX) {
                int byteIdx = (rigaOffset + x) * 4;                
                ColorRGB pixelNuovo;
                int idNewColor = -1;
                if (ditt == MetodoDithering.nessuno) {
                    pixelNuovo = new ColorRGB(inputBytes[byteIdx], inputBytes[byteIdx + 1], inputBytes[byteIdx + 2]);
                    pixelNuovo = MappaColore(pixelNuovo);
                }
                else {
                    PixelError err = matriceErrori[x, y];

                    // componenti con errore
                    int r = Math.Clamp(inputBytes[byteIdx] + err.Red / matriceDithering.Denominatore, 0, 255);
                    int g = Math.Clamp(inputBytes[byteIdx + 1] + err.Green / matriceDithering.Denominatore, 0, 255);
                    int b = Math.Clamp(inputBytes[byteIdx + 2] + err.Blue / matriceDithering.Denominatore, 0, 255);

                    // pixel Nuovo
                    pixelNuovo = MappaColore(new ColorRGB((byte)r, (byte)g, (byte)b));

                    // errore rimasto (Colore Originale - Colore Scelto) in RGB
                    r -= pixelNuovo.Red;
                    g -= pixelNuovo.Green;
                    b -= pixelNuovo.Blue;

                    // Propagazione Errore: "Spalmare" l'errore sui pixel adiacenti
                    int idDestra = daDXaSX ? x - 1 : x + 1;
                    int idSinistra = daDXaSX ? x + 1 : x - 1;
                    if (ditt == MetodoDithering.FloydSteinberg) {
                        //{{0, 0, 0},
                        // {0, x ,7},  /16
                        // {3, 5, 1}} 
                        if (0 <= idDestra && idDestra < width) {
                            // 7 destra
                            matriceErrori[idDestra, y] = PixelError.Aggiungi(matriceErrori[idDestra, y], r, g, b, 7);
                        }
                        if (y < height - 1) {
                            // 3 basso sx
                            if (0 <= idSinistra && idSinistra < width) {
                                matriceErrori[idSinistra, y + 1] = PixelError.Aggiungi(matriceErrori[idSinistra, y + 1], r, g, b, 3);
                            }
                            // 5 basso
                            matriceErrori[x, y + 1] = PixelError.Aggiungi(matriceErrori[x, y + 1], r, g, b, 5);
                            // 1 basso dx
                            if (0 <= idDestra && idDestra < width) {
                                matriceErrori[idDestra, y + 1] = PixelError.Aggiungi(matriceErrori[idDestra, y + 1], r, g, b);
                            }
                        }
                    }
                    else if (ditt == MetodoDithering.JarvisJudiceNinke) {
                        //MetodoDithering.JarvisJudiceNinke
                        //{{0, 0, x, 7, 5},
                        // {3, 5, 7, 5, 3},  /48
                        // {1, 3, 5, 3, 1}}
                        int idDestra2 = daDXaSX ? x - 2 : x + 2;
                        int idSinistra2 = daDXaSX ? x + 2 : x - 2;
                        if (0 <= idDestra && idDestra < width) {
                            // 7 destra
                            matriceErrori[idDestra, y] = PixelError.Aggiungi(matriceErrori[idDestra, y], r, g, b, 7);
                        }
                        if (0 <= idDestra2 && idDestra2 < width) {
                            // 5 destra2
                            matriceErrori[idDestra2, y] = PixelError.Aggiungi(matriceErrori[idDestra2, y], r, g, b, 5);
                        }
                        if (y < height - 1) {
                            // 3 basso sx2
                            if (0 <= idSinistra2 && idSinistra2 < width) {
                                matriceErrori[idSinistra2, y + 1] = PixelError.Aggiungi(matriceErrori[idSinistra2, y + 1], r, g, b, 3);
                            }
                            // 5 basso sx
                            if (0 <= idSinistra && idSinistra < width) {
                                matriceErrori[idSinistra, y + 1] = PixelError.Aggiungi(matriceErrori[idSinistra, y + 1], r, g, b, 5);
                            }
                            // 7 basso
                            matriceErrori[x, y + 1] = PixelError.Aggiungi(matriceErrori[x, y + 1], r, g, b, 7);
                            // 5 basso dx 
                            if (0 <= idDestra && idDestra < width) {
                                matriceErrori[idDestra, y + 1] = PixelError.Aggiungi(matriceErrori[idDestra, y + 1], r, g, b, 5);
                            }
                            // 3 basso dx2 
                            if (0 <= idDestra2 && idDestra2 < width) {
                                matriceErrori[idDestra2, y + 1] = PixelError.Aggiungi(matriceErrori[idDestra2, y + 1], r, g, b, 3);
                            }
                        }
                        if (y < height - 2) {
                            // 1 basso sx2
                            if (0 <= idSinistra2 && idSinistra2 < width) {
                                matriceErrori[idSinistra2, y + 1] = PixelError.Aggiungi(matriceErrori[idSinistra2, y + 1], r, g, b);
                            }
                            // 3 basso sx
                            if (0 <= idSinistra && idSinistra < width) {
                                matriceErrori[idSinistra, y + 1] = PixelError.Aggiungi(matriceErrori[idSinistra, y + 1], r, g, b, 3);
                            }
                            // 5 basso
                            matriceErrori[x, y + 1] = PixelError.Aggiungi(matriceErrori[x, y + 1], r, g, b, 5);
                            // 3 basso dx 
                            if (0 <= idDestra && idDestra < width) {
                                matriceErrori[idDestra, y + 1] = PixelError.Aggiungi(matriceErrori[idDestra, y + 1], r, g, b, 3);
                            }
                            // 1 basso dx2
                            if (0 <= idDestra2 && idDestra2 < width) {
                                matriceErrori[idDestra2, y + 1] = PixelError.Aggiungi(matriceErrori[idDestra2, y + 1], r, g, b);
                            }
                        }
                    }
                    else {
                        //MetodoDithering.Stucki
                        //{{0, 0, x, 8, 4},
                        // {2, 4, 8, 4, 2},  /48
                        // {1, 2, 4, 2, 1}}
                        int idDestra2 = daDXaSX ? x - 2 : x + 2;
                        int idSinistra2 = daDXaSX ? x + 2 : x - 2;
                        if (0 <= idDestra && idDestra < width) {
                            // 8 destra
                            matriceErrori[idDestra, y] = PixelError.Aggiungi(matriceErrori[idDestra, y], r, g, b, 8);
                        }
                        if (0 <= idDestra2 && idDestra2 < width) {
                            // 4 destra2
                            matriceErrori[idDestra2, y] = PixelError.Aggiungi(matriceErrori[idDestra2, y], r, g, b, 4);
                        }
                        if (y < height - 1) {
                            // 2 basso sx2
                            if (0 <= idSinistra2 && idSinistra2 < width) {
                                matriceErrori[idSinistra2, y + 1] = PixelError.Aggiungi(matriceErrori[idSinistra2, y + 1], r, g, b, 2);
                            }
                            // 4 basso sx
                            if (0 <= idSinistra && idSinistra < width) {
                                matriceErrori[idSinistra, y + 1] = PixelError.Aggiungi(matriceErrori[idSinistra, y + 1], r, g, b, 4);
                            }
                            // 8 basso
                            matriceErrori[x, y + 1] = PixelError.Aggiungi(matriceErrori[x, y + 1], r, g, b, 8);
                            // 4 basso dx 
                            if (0 <= idDestra && idDestra < width) {
                                matriceErrori[idDestra, y + 1] = PixelError.Aggiungi(matriceErrori[idDestra, y + 1], r, g, b, 4);
                            }
                            // 2 basso dx2 
                            if (0 <= idDestra2 && idDestra2 < width) {
                                matriceErrori[idDestra2, y + 1] = PixelError.Aggiungi(matriceErrori[idDestra2, y + 1], r, g, b, 2);
                            }
                        }
                        if (y < height - 2) {
                            // 1 basso sx2
                            if (0 <= idSinistra2 && idSinistra2 < width) {
                                matriceErrori[idSinistra2, y + 1] = PixelError.Aggiungi(matriceErrori[idSinistra2, y + 1], r, g, b);
                            }
                            // 2 basso sx
                            if (0 <= idSinistra && idSinistra < width) {
                                matriceErrori[idSinistra, y + 1] = PixelError.Aggiungi(matriceErrori[idSinistra, y + 1], r, g, b, 2);
                            }
                            // 4 basso
                            matriceErrori[x, y + 1] = PixelError.Aggiungi(matriceErrori[x, y + 1], r, g, b, 4);
                            // 2 basso dx 
                            if (0 <= idDestra && idDestra < width) {
                                matriceErrori[idDestra, y + 1] = PixelError.Aggiungi(matriceErrori[idDestra, y + 1], r, g, b, 2);
                            }
                            // 1 basso dx2
                            if (0 <= idDestra2 && idDestra2 < width) {
                                matriceErrori[idDestra2, y + 1] = PixelError.Aggiungi(matriceErrori[idDestra2, y + 1], r, g, b);
                            }
                        }
                    }
                }                 
                outputPixels[y * width + x] = new SKColor(pixelNuovo.Red, pixelNuovo.Green, pixelNuovo.Blue, inputBytes[byteIdx + 3]);
            }
        }
        if (mostraInConsole)
            Console.WriteLine("Nuovi pixel pronti");
        using (SKBitmap bitmapElaborata = new SKBitmap(width, height)) {
            bitmapElaborata.Pixels = outputPixels; // Assegnazione diretta ultra-veloce!
            using (SKImage image = SKImage.FromBitmap(bitmapElaborata))
            using (SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100))
            using (FileStream stream = File.OpenWrite(fOut.FullName)) {
                data.SaveTo(stream);
            }
        }
    }

    /// <summary>
    /// Prova della libreria SkiaSharp
    /// </summary>
    public static void Prova() {
        Console.WriteLine("=== Test Iniziale SkiaSharp ===");
        int width = 200;
        int height = 200;

        using (SKBitmap bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)) {
            using (SKCanvas canvas = new SKCanvas(bitmap)) {
                // Sfondo blu notte
                canvas.Clear(new SKColor(10, 20, 50));

                // Disegniamo un cerchio rosso al centro
                using (SKPaint paint = new SKPaint()) {
                    paint.Color = SKColors.Red;
                    paint.IsAntialias = true;
                    canvas.DrawCircle(width / 2f, height / 2f, 60f, paint);
                }
            }

            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_skiasharp.png");

            using (SKImage image = SKImage.FromBitmap(bitmap))
            using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (FileStream stream = File.OpenWrite(outputPath)) {
                data.SaveTo(stream);
            }

            Console.WriteLine($"[OK] Libreria SkiaSharp funzionante!");
            Console.WriteLine($"[OK] Immagine generata in: {outputPath}");
        }
    }

    public void Dispose() {
        bitmap?.Dispose();
    }
}
