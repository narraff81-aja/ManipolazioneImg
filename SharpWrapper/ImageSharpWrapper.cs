using ManipolazioneImg.Colors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using Path = System.IO.Path;

namespace ManipolazioneImg.SharpWrapper;

public class ImageSharpWrapper : ISharpWrapper {
    private Image<Rgba32>? bitmap;
    public ImageSharpWrapper(string pathFile) {
        this.CaricaImmagine(pathFile);
    }

    public void CaricaImmagine(string pathFile) {
        FileInfo fi = new FileInfo(pathFile);
        if (!fi.Exists) {//File.Exists(percorsoFile)
            throw new FileNotFoundException($"Errore ImageSharp: Immagine non trovata in {fi.FullName}");
        }
        // 1. Tentativo standard
        try {
            var image = Image.Load<Rgba32>(fi.FullName);
            bitmap = image;
        }
        catch (Exception) {
            throw new InvalidOperationException($"Impossibile decodificare con ImageSharp l'immagine\n {fi.FullName} !\n" +
            $"Prova la conversione con un programma di fotoritocco!");
        }
    }

    public Dictionary<ColorRGB, int> FrequenzaColori(bool mostraInConsole = false) {
        if (bitmap == null)
            throw new InvalidOperationException("bitmap==null usa CaricaImmagine()!");
        if (mostraInConsole) {
            Console.WriteLine($"Dimensioni: {bitmap.Width}x{bitmap.Height} pixel");
            Console.WriteLine($"Formato colore nativo: {bitmap.PixelType.ColorType}");
        }
        // Dict risultato
        Dictionary<ColorRGB, int> ris = new();
        int totalPixels = bitmap.Width * bitmap.Height;
        // Accesso ai pixel riga per riga in modo super veloce
        bitmap.ProcessPixelRows(accessor => {
            for (int y = 0; y < accessor.Height; y++) {
                Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                for (int x = 0; x < accessor.Width; x++) {
                    Rgba32 pixel = pixelRow[x];
                    ColorRGB color = new ColorRGB(pixel.R, pixel.G, pixel.B);

                    if (ris.TryGetValue(color, out int count)) {
                        ris[color] = count + 1;
                    }
                    else {
                        ris[color] = 1;
                    }
                }
            }
        });
        if (mostraInConsole) {
            Console.WriteLine($"{ris.Count} colori unici su {totalPixels}= {(ris.Count * 100.0 / totalPixels).ToString("f2")}% !");
            var maxFrequenza = ris.Values.Max();
            Console.WriteLine($"maxFrequenza: {maxFrequenza}");
            Console.WriteLine("Scansione dei pixel completata!");
        }

        return ris;
    }
    public void SalvaImmagine(string percorsoFileDestinazione,
     List<ColorRGB> tavolozza, MetodoSpazioColore spazio, MetodoDithering ditt, bool usaSerpentina,
     bool mostraInConsole = false) {
        if (bitmap == null)
            throw new InvalidOperationException("bitmap==null usa CaricaImmagine()!");
        // File I/O
        FileInfo fOut = new FileInfo(percorsoFileDestinazione);

        // Array di pixel
        int width = bitmap.Width;
        int height = bitmap.Height;
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
            bitmap.ProcessPixelRows(accessor => {
                for (int y = 0; y < height; y++) {                    
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++) {
                        ref Rgba32 pixel = ref row[x]; // Riferimento diretto in memoria
                        ColorRGB newPixel =MappaColore( new ColorRGB(pixel.R, pixel.G, pixel.B));
                        pixel.R = newPixel.Red;
                        pixel.G = newPixel.Green;
                        pixel.B = newPixel.Blue;

                    }
                }
            });
        }
        else {
            MatriceDithering matriceDithering = ditt switch {
                MetodoDithering.FloydSteinberg => MatriceDithering.FloydSteinberg,
                MetodoDithering.JarvisJudiceNinke => MatriceDithering.JarvisJudiceNinke,
                MetodoDithering.Stucki => MatriceDithering.Stucki,
                _ => throw new ArgumentOutOfRangeException(nameof(ditt))
            };
            // Buffer circolare: [rigaBuffer, x, canaleRGB]
            int[,,] errorBuffer = new int[matriceDithering.RigheBuffer, width, 3];

            bitmap.ProcessPixelRows(accessor => {
                for (int y = 0; y < height; y++) {
                    int currRowIndex = y % matriceDithering.RigheBuffer;
                    Span<Rgba32> row = accessor.GetRowSpan(y); // Accesso diretto alla riga Y
                    bool daDXaSX = usaSerpentina && (y % 2 == 1);                    
                    int startX = daDXaSX ? width - 1 : 0;
                    int endX = daDXaSX ? -1 : width;
                    int stepX = daDXaSX ? -1 : 1;

                    for (int x = startX; x != endX; x += stepX) {
                        ref Rgba32 pixel = ref row[x]; // Riferimento diretto in memoria

                        // Errore accumulato
                        int r = Math.Clamp(pixel.R + (errorBuffer[currRowIndex, x, 0] / matriceDithering.Denominatore), 0, 255);
                        int g = Math.Clamp(pixel.G + (errorBuffer[currRowIndex, x, 1] / matriceDithering.Denominatore), 0, 255);
                        int b = Math.Clamp(pixel.B + (errorBuffer[currRowIndex, x, 2] / matriceDithering.Denominatore), 0, 255);

                        // Trova colore più vicino
                        ColorRGB newPixel =MappaColore( new ColorRGB( (byte)r, (byte)g,(byte)b));


                        // Modifica diretta del pixel nella Bitmap
                        pixel.R = newPixel.Red;
                        pixel.G = newPixel.Green;
                        pixel.B = newPixel.Blue;

                        // Calcolo e diffusione errore residuo
                        int errR = (int)newPixel.Red - pixel.R;
                        int errG = (int)newPixel.Green - pixel.G;
                        int errB = (int)newPixel.Blue - pixel.B;

                        // Diffusione dell'errore (invertendo gli OffsetX se stiamo andando da destra a sinistra)
                        for (int k = 0; k < matriceDithering.Celle.Length; k++) {
                            var item = matriceDithering.Celle[k];

                            // Inversione speculare dell'asse X durante il ritorno della serpentina
                            int offsetX = daDXaSX ? -item.OffsetX : item.OffsetX;

                            int newX = x + offsetX;
                            int newY = y + item.OffsetY;

                            if (newX >= 0 && newX < width && newY < height) {
                                int newYID = newY % matriceDithering.RigheBuffer;
                                errorBuffer[newYID, newX, 0] += errR * item.Peso;
                                errorBuffer[newYID, newX, 1] += errG * item.Peso;
                                errorBuffer[newYID, newX, 2] += errB * item.Peso;
                            }
                        }

                        // Pulizia del valore usato nel buffer
                        errorBuffer[currRowIndex, x, 0] = 0;
                        errorBuffer[currRowIndex, x, 1] = 0;
                        errorBuffer[currRowIndex, x, 2] = 0;
                    }
                }
            });
        }
        bitmap.Save(fOut.FullName);
    }

    /// <summary>
    /// Prova della libreria SixLabors.ImageSharp
    /// </summary>
    public static void Prova() {
        // NuGet SixLabors.ImageSharp
        Console.WriteLine("=== Test Iniziale ImageSharp: negativo ===");
        string inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "ImgInput", "apnear40-bee-7418975_1920.jpg");
        using Image<Rgba32> image = Image.Load<Rgba32>(inputPath);

        // Accesso ai pixel riga per riga in modo super veloce
        image.ProcessPixelRows(accessor => {
            for (int y = 0; y < accessor.Height; y++) {
                Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                for (int x = 0; x < accessor.Width; x++) {
                    // Qui hai il controllo totale sul singolo pixel!
                    ref Rgba32 pixel = ref pixelRow[x];

                    byte r = pixel.R;
                    byte g = pixel.G;
                    byte b = pixel.B;

                    // calcola il negativo

                    pixel.R = (byte)(255 - r);
                    pixel.G = (byte)(255 - g);
                    pixel.B = (byte)(255 - b);
                }
            }
        });
        string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "apnear40-bee-7418975_1920_NEGATIVO.jpg");
        image.Save(outputPath);
        Console.WriteLine($"[OK] Libreria ImageSharp funzionante!");
        Console.WriteLine($"[OK] Immagine generata in: {outputPath}");
    }
    /// <summary>
    /// Prova di disegno della libreria SixLabors.ImageSharp 
    /// </summary>
    public static void Prova2() {
        Console.WriteLine("=== Test Iniziale ImageSharp: disegno cerchio ===");
        int width = 500;
        int height = 500;
        string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_imagesharp.png");

        // Crea un'immagine da zero con sfondo bianco
        using (Image<Rgba32> image = new Image<Rgba32>(width, height, new Rgba32(255, 255, 255, 255))) {//, Color.White
            float centerX = width / 2f;
            float centerY = height / 2f;
            float radius = 150f;

            // Crea la forma del cerchio
            IPath circle = new EllipsePolygon(centerX, centerY, radius);

            //Disegno del cerchio (Fill per riempimento, Draw per il bordo)
            image.Mutate(ctx => ctx.Paint(canvas => {
                canvas.Fill(new SolidBrush(Color.Red), circle);
                Pen outline = Pens.Solid(Color.Black, 4);
                canvas.Draw(outline, circle);
            }));

            image.Save(outputPath);
        }
        Console.WriteLine($"[OK] Libreria ImageSharp funzionante!");
        Console.WriteLine($"[OK] Immagine generata in: {outputPath}");
    }

    public static void Prova3(string inputPath, string outputPath) {
        // 1. Caricamento dell'immagine con gestione automatica del formato
        using (Image<Rgba32> image = Image.Load<Rgba32>(inputPath)) {
            int width = image.Width;
            int height = image.Height;

            // 2. Manipolazione diretta e ad alta velocità tramite ProcessPixelRows
            image.ProcessPixelRows(accessor => {
                for (int y = 0; y < accessor.Height; y++) {
                    // Span diretto sulla riga di memoria corrente (senza copie)
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                    for (int x = 0; x < pixelRow.Length; x++) {
                        // Esempio: Inversione del canale Rosso
                        pixelRow[x].R = (byte)(255 - pixelRow[x].R);
                    }
                }
            });

            // 3. Applicazione delle mutation nativi (ad es. un leggero contrasto)
            image.Mutate(ctx => ctx.Contrast(1.2f));

            // 4. Salvataggio
            image.Save(outputPath);
        }

        System.Console.WriteLine("Test completato con successo!");
    }

    public void Dispose() {
        bitmap.Dispose();
    }
}
