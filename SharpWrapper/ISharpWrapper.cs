using ManipolazioneImg.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManipolazioneImg.SharpWrapper;

public interface ISharpWrapper: IDisposable {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pathFile"></param>
    public void CaricaImmagine(string pathFile);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mostraInConsole">Mostra gli avvisi di successo in console</param>
    /// <returns></returns>
    public Dictionary<ColorRGB, int> FrequenzaColori(bool mostraInConsole = false);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="percorsoFileDestinazione"></param>
    /// <param name="tavolozza"></param>
    /// <param name="spazio"></param>
    /// <param name="ditt"></param>
    /// <param name="usaSerpentina"></param>
    /// <param name="mostraInConsole"></param>
    public void SalvaImmagine(string percorsoFileDestinazione,
     List<ColorRGB> tavolozza, MetodoSpazioColore spazio, MetodoDithering ditt, bool usaSerpentina,
     bool mostraInConsole = false);
}
