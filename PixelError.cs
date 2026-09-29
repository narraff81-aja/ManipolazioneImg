using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg;

public struct PixelError {
    /// <summary>
    /// Red 0.0-255.0
    /// </summary>
    public int Red;
    /// <summary>
    /// Green 0.0-255.0
    /// </summary>
    public int Green;
    /// <summary>
    /// Blue 0.0-255.0
    /// </summary>
    public int Blue;

    public PixelError(int red, int green, int blue) {
        this.Red = red;
        this.Green = green;
        this.Blue = blue;
    }

    public static PixelError Aggiungi(PixelError rgb, int red, int green, int blue, int peso=1) {
        return new PixelError(rgb.Red + red * peso, rgb.Green + green * peso, rgb.Blue + blue * peso);
    }
    [Obsolete("Poco sicuro per una matrice")]
    public void Aggiungi(int red, int green, int blue, int peso = 1) {
        this.Red += red * peso;
        this.Green += green * peso;
        this.Blue += blue * peso;
    }
}
