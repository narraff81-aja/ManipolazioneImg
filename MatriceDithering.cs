using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg;

public class MatriceDithering {
    public readonly CellaMatriceDithering[]? Celle;
    public readonly int Denominatore;
    public readonly int RigheBuffer;
    public static readonly MatriceDithering FloydSteinberg;
    public static readonly MatriceDithering JarvisJudiceNinke;
    public static readonly MatriceDithering Stucki;

    private MatriceDithering(MetodoDithering metodo) {
        switch (metodo) {
            case MetodoDithering.FloydSteinberg:
                /* 
                 {{0, C, 7},  *  1/16
                 {3, 5, 1}}
                 */
                Denominatore = 16;
                RigheBuffer = 2;
                Celle = new CellaMatriceDithering[]{
                    new(1, 0, 7),
                    new(-1, 1, 3), new(0, 1, 5), new(1, 1, 1)
                };
                break;
            case MetodoDithering.JarvisJudiceNinke:
                /* 
                 {{0, 0, C, 7, 5},
                 {3, 5, 7, 5, 3},  * 1/48
                 {1, 3, 5, 3, 1}}
                 */
                Denominatore = 48;
                RigheBuffer = 3;
                Celle = new CellaMatriceDithering[]{
                    new(1, 0, 7), new(2, 0, 5),
                    new(-2, 1, 3), new(-1, 1, 5), new(0, 1, 7), new(1, 1, 5), new(2, 1, 3),
                    new(-2, 2, 1), new(-1, 2, 3), new(0, 2, 5), new(1, 2, 3), new(2, 2, 1)
                };
                break;
            case MetodoDithering.Stucki:
                /* 
                 {{0, 0, C, 8, 4},
                 {2, 4, 8, 4, 2},   * 1/42
                 {1, 2, 4, 2, 1}}
                 */
                Denominatore = 42;
                RigheBuffer = 3;
                Celle = new CellaMatriceDithering[] {
                    new(1, 0, 8), new(2, 0, 4),
                    new(-2, 1, 2), new(-1, 1, 4), new(0, 1, 8), new(1, 1, 4), new(2, 1, 2),
                    new(-2, 2, 1), new(-1, 2, 2), new(0, 2, 4), new(1, 2, 2), new(2, 2, 1)
                };
                break;
            default:// MetodoDithering.nessuno
                break;
        }
    }

    static MatriceDithering() {
        FloydSteinberg = new(MetodoDithering.FloydSteinberg);
        JarvisJudiceNinke = new(MetodoDithering.JarvisJudiceNinke);
        Stucki = new(MetodoDithering.Stucki);
    }
}
public struct CellaMatriceDithering {
    public readonly int OffsetX;
    public readonly int OffsetY;
    public readonly int Peso;
    public CellaMatriceDithering(int offsetX, int offsetY, int peso) {
        OffsetX = offsetX;
        OffsetY = offsetY;
        Peso = peso;
    }
}
