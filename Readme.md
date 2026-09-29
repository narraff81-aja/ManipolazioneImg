# ManipolazioneImg (ImageDitheringCLI)

Tool a riga di comando (CLI) ad alte prestazioni in C# (.NET) per la riduzione della tavolozza dei colori, il dithering spaziale e l'analisi colorimetrica multi-spazio.

## 📌 Descrizione
L'applicazione offre un flusso guidato per convertire ed elaborare immagini ad alta risoluzione, riducendo lo spazio colore tramite algoritmi di clustering (**Median Cut**) e applicando la diffusione dell'errore (**Error Diffusion Dithering**).

Il sistema integra un'architettura **multi-engine** per astrarre le librerie grafiche sottostanti e include opzioni di profilazione temporale (benchmark in ms) per confrontare le prestazioni di calcolo e di allocazione memoria tra le varie librerie e strategie di rendering.

## ⚙️ Caratteristiche Tecniche e Spazi Colore
- **Architettura Wrapper / Adapter:**
  - Interfaccia `ISharpWrapper` con implementazioni trasparenti per **SkiaSharp** e **SixLabors.ImageSharp**.
  - Meccanismo di *fallback* automatico o guidato da console in caso di errori di decodifica sui formati imprevisti o header estesi.
- **Quantizzazione & Clustering:**
  - **Median Cut Algorithm:** Estrazione automatica dei colori dominanti a partire dal dizionario delle frequenze RGB dell'immagine.
  - **Conversione Spazi Colore:** Supporto alle trasformazioni matriciali sRGB ↔ XYZ ↔ LAB, HSL e HSB per il calcolo di distanza Euclidea accuratezza percepita.
  - Supporto alternativo a palette fisse (es. Web-Safe palette).
- **Dithering Spaziale:**
  - Algoritmi di diffusione dell'errore (Floyd-Steinberg, Jarvis-Judice-Ninke, Stucki).
  - Modalità di scansione alternata (*Serpentine Dithering*) per prevenire artefatti Direzionali.
- **Performance & Benchmark:**
  - Profilatura con `Stopwatch` per la valutazione al millisecondo delle routine di salvataggio e trasformazione.

## 🛠️ Tech Stack
* **Linguaggio:** C# (.NET Framework)
* **Librerie Grafiche:** SkiaSharp, SixLabors.ImageSharp
* **Interfaccia:** Interactive Console CLI