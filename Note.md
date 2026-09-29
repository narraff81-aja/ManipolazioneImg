# Annotazioni
### 1. Obiettivo del progetto
Questo progetto non intende assolutamente sostituire ciò che si può fare con un programma di fotoritocco, 
ma si intende gestire al meglio la memoria per eseguire le operazioni che le farebbe un programma di fotoritocco.

In ogni caso la riduzione dei colori di un immagine a max 256 è SEMPRE un peggioramento della qualità 
dell'immagine rispetto all'originale, l'uso del dithering mitiga un po' ma sempre l'immagine risultante
è scadente e sempre sarà un'immagine più pesante in termini di Kb poiché non può essere applicata una 
compressione seria nell'immagine stessa, specialmente per file jpg.
Un guadagno di peso è per il PNG ad 8 bit (indicizzato).

## 2. Spazi colori
Sebbene nel progetto ho gestito i seguenti spazi colori: RGB, HSL, HSB, XYZ D65, CIE LAB D65, XYZ D50, 
CIE LAB D50 e le loro conversioni, per individuare la quantizzazione del colore (color mapping) utilizzo 
solo RGB o HSL o CIE LAB D65.

Per quanto riguarda lo spazio HSL non utilizzo le coordinate (x,y,z) dello spazio cilindrico ma doppio conico, 
ovvero il raggio S è max a L=50% e si riduce verso l'estremo nero (L=0%) e verso il bianco (L=100%).
Un'altra buona alternativa non è il doppio conico, ma due "tronchi di paraboloide" (o paraboloidi combinati) 
quindi il fattore di correzione per il raggio S è sempre: 0 a L=0%, 1 a L=50% e 0 a L=100%, ma varia negli 
altri valori di L.
Per HSL ho pensato persino all'ellissoide al posto del doppio cono, ma per la percezione dei colori rischia 
di essere troppo gonfia vicino ai poli e quindi l'ho scartata!
Per lo spazio RGB e CIELAB non ci sono problemi di coordinate.

Per individuare il colore più vicino alla tavolozza dei 256 colori basta calcolare la distanza euclidea minima.
In ogni caso non mi sono portato appresso il componente Alpha, poiché dopo le modifiche RGB di ogni pixel, 
l'Alpha sarà lo stesso dell'originale!

Quindi, riassumendo, gli spazi colore a disposizione sono: RGB, HSL-conico, HSL-parabolico, CIELAB.

## 3. Quantizzazione del colore (color mapping)
Ho previsto 2 tipi di Quantizzazione del colore:
1 - utilizzare i soli 147 colori web/X11 safe per evitare ulteriori calcoli, ma se l'immagine di per sé
    possiede un colore specifico (es: cielo stellato o sabbia del deserto o un fiore fotografato in primo 
    piano), il risultato avrà una qualità peggiore dell'altro metodo;
2 - vera Quantizzazione del Colore con Median Cut per calcolare i 256 colori più rappresentativi.

Nel mio algoritmo del Median Cut (con lo spazio colore scelto) non procedo subito a splittare ricorsivamente 
in 2 le box degli spazi colori, ma se c'è uno (o più) colore con una frequenza che supera il 
numero di pixel / 256 è già un box a se stante e verrà subito escluso dall'algoritmo del Median Cut, ovvero 
occorrerà trovare solo i 255 colori mancanti (o meno se ci sono più colori con alta frequenza).
Ovviamente se per qualche ragione l'immagine ha già max 256 colori unici, il Median Cut restituiròà quelli!
Prima di tutto ciò trovo, ovviamente, il dizionario delle frequenze di ogni colori per ridurre i calcoli!
Insomma una indicizzazione della tavolozza (o Color Indexing)!

## 4. Metodi di Dithering
Ho previsto di utilizzare vari metodi di Dithering: nessuno, Floyd-Steinberg, Jarvis-Judice-Ninke, Stucki. 
C'è la possibilità o meno di usare il metodo a serpentina, cioè analizzare i pixel da sinistra a destra 
nelle righe a id pari (0;2;4;...) e viceversa nelle righe ad id dispari (1;3;5;...). Ciò permette di spalmare 
meglio l'errore ed evitare striature diagonali.

## 5. Note di Revisione
All'inizio ho usato la classe MedianBox con l'uso di LINQ ma rallentava parecchio l'algoritmo soprattutto 
usando le funzioni Add() e Remove(). Ho optato per la classe MedianCutBox!
Le classi SkiaSharpWrapper e ImageSharpWrapper non implementavano IDisposable, ciò portava ad un'utilizzo 
eccessivo di memoria che non veniva rilasciata neanche creando un solo file, chiudendo il programma e ripetendo
l'azione.
In SkiaSharpWrapper utilizzavo GetPixel() e SetPixel() che è molto lento rispetto al veloce GetPixelSpan().
Anche la funzione interna MappaColore() velocizza la ricerca del colore più vicino nella tavolozza. Ripetere 
l'operazione di ricerca per colori uguali è computazionalmente pesante, il Dictionary aiuta molto. 