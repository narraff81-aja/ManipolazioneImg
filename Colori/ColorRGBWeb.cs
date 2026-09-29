using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ManipolazioneImg.Colors;

public class ColorRGBWeb {
    public static readonly Dictionary<string, ColorRGB> WebPalette = new();

    public static string NomeByRGB(byte red, byte green, byte blue) {
        foreach (var item in WebPalette) {
            if (item.Value.Red == red && item.Value.Green == green && item.Value.Blue == blue)
                return item.Key;
        }
        return "<NO Name>";
    }
    public static string NomeByHEX(string hexColor) {
        var c= ColorRGB.FromHexString(hexColor);
        foreach (var item in WebPalette) {
            if (item.Value.Red == c.Red && item.Value.Green == c.Green && item.Value.Blue == c.Blue)
                return item.Key;
        }
        return "<NO Name>";
    }

    public static ColorRGB RGBByNome(string nome) { 
        if(string.IsNullOrWhiteSpace(nome))
            throw new ArgumentNullException("<NO Name>");
        nome = Regex.Replace(nome, "[^a-zA-Z]", "");
        nome =nome.ToLower().Trim();
        foreach (var item in WebPalette.Keys) {
            if(item == nome) {
                return WebPalette[item];
            }
        }
        throw new ArgumentNullException("<NO Name>");
    }

    static ColorRGBWeb() {
        WebPalette.Add("aliceblue", new ColorRGB(240, 248, 255));
        WebPalette.Add("antiquewhite", new ColorRGB(250, 235, 215));
        WebPalette.Add("aqua", new ColorRGB(0, 255, 255));
        WebPalette.Add("aquamarine", new ColorRGB(127, 255, 212));
        WebPalette.Add("azure", new ColorRGB(240, 255, 255));
        WebPalette.Add("beige", new ColorRGB(245, 245, 220));
        WebPalette.Add("bisque", new ColorRGB(255, 228, 196));
        WebPalette.Add("black", new ColorRGB(0, 0, 0));
        WebPalette.Add("blanchedalmond", new ColorRGB(255, 235, 205));
        WebPalette.Add("blue", new ColorRGB(0, 0, 255));
        WebPalette.Add("blueviolet", new ColorRGB(138, 43, 226));
        WebPalette.Add("brown", new ColorRGB(165, 42, 42));
        WebPalette.Add("burlywood", new ColorRGB(222, 184, 135));
        WebPalette.Add("cadetblue", new ColorRGB(95, 158, 160));
        WebPalette.Add("chartreuse", new ColorRGB(127, 255, 0));
        WebPalette.Add("chocolate", new ColorRGB(210, 105, 30));
        WebPalette.Add("coral", new ColorRGB(255, 127, 80));
        WebPalette.Add("cornflowerblue", new ColorRGB(100, 149, 237));
        WebPalette.Add("cornsilk", new ColorRGB(255, 248, 220));
        WebPalette.Add("crimson", new ColorRGB(220, 20, 60));
        WebPalette.Add("cyan", new ColorRGB(0, 255, 255));
        WebPalette.Add("darkblue", new ColorRGB(0, 0, 139));
        WebPalette.Add("darkcyan", new ColorRGB(0, 139, 139));
        WebPalette.Add("darkgoldenrod", new ColorRGB(184, 134, 11));
        WebPalette.Add("darkgray", new ColorRGB(169, 169, 169));
        WebPalette.Add("darkgreen", new ColorRGB(0, 100, 0));
        WebPalette.Add("darkgrey", new ColorRGB(169, 169, 169));
        WebPalette.Add("darkkhaki", new ColorRGB(189, 183, 107));
        WebPalette.Add("darkmagenta", new ColorRGB(139, 0, 139));
        WebPalette.Add("darkolivegreen", new ColorRGB(85, 107, 47));
        WebPalette.Add("darkorange", new ColorRGB(255, 140, 0));
        WebPalette.Add("darkorchid", new ColorRGB(153, 50, 204));
        WebPalette.Add("darkred", new ColorRGB(139, 0, 0));
        WebPalette.Add("darksalmon", new ColorRGB(233, 150, 122));
        WebPalette.Add("darkseagreen", new ColorRGB(143, 188, 143));
        WebPalette.Add("darkslateblue", new ColorRGB(72, 61, 139));
        WebPalette.Add("darkslategray", new ColorRGB(47, 79, 79));
        WebPalette.Add("darkslategrey", new ColorRGB(47, 79, 79));
        WebPalette.Add("darkturquoise", new ColorRGB(0, 206, 209));
        WebPalette.Add("darkviolet", new ColorRGB(148, 0, 211));
        WebPalette.Add("deeppink", new ColorRGB(255, 20, 147));
        WebPalette.Add("deepskyblue", new ColorRGB(0, 191, 255));
        WebPalette.Add("dimgray", new ColorRGB(105, 105, 105));
        WebPalette.Add("dimgrey", new ColorRGB(105, 105, 105));
        WebPalette.Add("dodgerblue", new ColorRGB(30, 144, 255));
        WebPalette.Add("firebrick", new ColorRGB(178, 34, 34));
        WebPalette.Add("floralwhite", new ColorRGB(255, 250, 240));
        WebPalette.Add("forestgreen", new ColorRGB(34, 139, 34));
        WebPalette.Add("fuchsia", new ColorRGB(255, 0, 255));
        WebPalette.Add("gainsboro", new ColorRGB(220, 220, 220));
        WebPalette.Add("ghostwhite", new ColorRGB(248, 248, 255));
        WebPalette.Add("gold", new ColorRGB(255, 215, 0));
        WebPalette.Add("goldenrod", new ColorRGB(218, 165, 32));
        WebPalette.Add("gray", new ColorRGB(128, 128, 128));
        WebPalette.Add("green", new ColorRGB(0, 128, 0));
        WebPalette.Add("greenyellow", new ColorRGB(173, 255, 47));
        WebPalette.Add("grey", new ColorRGB(128, 128, 128));
        WebPalette.Add("honeydew", new ColorRGB(240, 255, 240));
        WebPalette.Add("hotpink", new ColorRGB(255, 105, 180));
        WebPalette.Add("indianred", new ColorRGB(205, 92, 92));
        WebPalette.Add("indigo", new ColorRGB(75, 0, 130));
        WebPalette.Add("ivory", new ColorRGB(255, 255, 240));
        WebPalette.Add("khaki", new ColorRGB(240, 230, 140));
        WebPalette.Add("lavender", new ColorRGB(230, 230, 250));
        WebPalette.Add("lavenderblush", new ColorRGB(255, 240, 245));
        WebPalette.Add("lawngreen", new ColorRGB(124, 252, 0));
        WebPalette.Add("lemonchiffon", new ColorRGB(255, 250, 205));
        WebPalette.Add("lightblue", new ColorRGB(173, 216, 230));
        WebPalette.Add("lightcoral", new ColorRGB(240, 128, 128));
        WebPalette.Add("lightcyan", new ColorRGB(224, 255, 255));
        WebPalette.Add("lightgoldenrodyellow", new ColorRGB(250, 250, 210));
        WebPalette.Add("lightgray", new ColorRGB(211, 211, 211));
        WebPalette.Add("lightgreen", new ColorRGB(144, 238, 144));
        WebPalette.Add("lightgrey", new ColorRGB(211, 211, 211));
        WebPalette.Add("lightpink", new ColorRGB(255, 182, 193));
        WebPalette.Add("lightsalmon", new ColorRGB(255, 160, 122));
        WebPalette.Add("lightseagreen", new ColorRGB(32, 178, 170));
        WebPalette.Add("lightskyblue", new ColorRGB(135, 206, 250));
        WebPalette.Add("lightslategray", new ColorRGB(119, 136, 153));
        WebPalette.Add("lightslategrey", new ColorRGB(119, 136, 153));
        WebPalette.Add("lightsteelblue", new ColorRGB(176, 196, 222));
        WebPalette.Add("lightyellow", new ColorRGB(255, 255, 224));
        WebPalette.Add("lime", new ColorRGB(0, 255, 0));
        WebPalette.Add("limegreen", new ColorRGB(50, 205, 50));
        WebPalette.Add("linen", new ColorRGB(250, 240, 230));
        WebPalette.Add("magenta", new ColorRGB(255, 0, 255));
        WebPalette.Add("maroon", new ColorRGB(128, 0, 0));
        WebPalette.Add("mediumaquamarine", new ColorRGB(102, 205, 170));
        WebPalette.Add("mediumblue", new ColorRGB(0, 0, 205));
        WebPalette.Add("mediumorchid", new ColorRGB(186, 85, 211));
        WebPalette.Add("mediumpurple", new ColorRGB(147, 112, 219));
        WebPalette.Add("mediumseagreen", new ColorRGB(60, 179, 113));
        WebPalette.Add("mediumslateblue", new ColorRGB(123, 104, 238));
        WebPalette.Add("mediumspringgreen", new ColorRGB(0, 250, 154));
        WebPalette.Add("mediumturquoise", new ColorRGB(72, 209, 204));
        WebPalette.Add("mediumvioletred", new ColorRGB(199, 21, 133));
        WebPalette.Add("midnightblue", new ColorRGB(25, 25, 112));
        WebPalette.Add("mintcream", new ColorRGB(245, 255, 250));
        WebPalette.Add("mistyrose", new ColorRGB(255, 228, 225));
        WebPalette.Add("moccasin", new ColorRGB(255, 228, 181));
        WebPalette.Add("navajowhite", new ColorRGB(255, 222, 173));
        WebPalette.Add("navy", new ColorRGB(0, 0, 128));
        WebPalette.Add("oldlace", new ColorRGB(253, 245, 230));
        WebPalette.Add("olive", new ColorRGB(128, 128, 0));
        WebPalette.Add("olivedrab", new ColorRGB(107, 142, 35));
        WebPalette.Add("orange", new ColorRGB(255, 165, 0));
        WebPalette.Add("orangered", new ColorRGB(255, 69, 0));
        WebPalette.Add("orchid", new ColorRGB(218, 112, 214));
        WebPalette.Add("palegoldenrod", new ColorRGB(238, 232, 170));
        WebPalette.Add("palegreen", new ColorRGB(152, 251, 152));
        WebPalette.Add("paleturquoise", new ColorRGB(175, 238, 238));
        WebPalette.Add("palevioletred", new ColorRGB(219, 112, 147));
        WebPalette.Add("papayawhip", new ColorRGB(255, 239, 213));
        WebPalette.Add("peachpuff", new ColorRGB(255, 218, 185));
        WebPalette.Add("peru", new ColorRGB(205, 133, 63));
        WebPalette.Add("pink", new ColorRGB(255, 192, 203));
        WebPalette.Add("plum", new ColorRGB(221, 160, 221));
        WebPalette.Add("powderblue", new ColorRGB(176, 224, 230));
        WebPalette.Add("purple", new ColorRGB(128, 0, 128));
        WebPalette.Add("red", new ColorRGB(255, 0, 0));
        WebPalette.Add("rosybrown", new ColorRGB(188, 143, 143));
        WebPalette.Add("royalblue", new ColorRGB(65, 105, 225));
        WebPalette.Add("saddlebrown", new ColorRGB(139, 69, 19));
        WebPalette.Add("salmon", new ColorRGB(250, 128, 114));
        WebPalette.Add("sandybrown", new ColorRGB(244, 164, 96));
        WebPalette.Add("seagreen", new ColorRGB(46, 139, 87));
        WebPalette.Add("seashell", new ColorRGB(255, 245, 238));
        WebPalette.Add("sienna", new ColorRGB(160, 82, 45));
        WebPalette.Add("silver", new ColorRGB(192, 192, 192));
        WebPalette.Add("skyblue", new ColorRGB(135, 206, 235));
        WebPalette.Add("slateblue", new ColorRGB(106, 90, 205));
        WebPalette.Add("slategray", new ColorRGB(112, 128, 144));
        WebPalette.Add("slategrey", new ColorRGB(112, 128, 144));
        WebPalette.Add("snow", new ColorRGB(255, 250, 250));
        WebPalette.Add("springgreen", new ColorRGB(0, 255, 127));
        WebPalette.Add("steelblue", new ColorRGB(70, 130, 180));
        WebPalette.Add("tan", new ColorRGB(210, 180, 140));
        WebPalette.Add("teal", new ColorRGB(0, 128, 128));
        WebPalette.Add("thistle", new ColorRGB(216, 191, 216));
        WebPalette.Add("tomato", new ColorRGB(255, 99, 71));
        WebPalette.Add("turquoise", new ColorRGB(64, 224, 208));
        WebPalette.Add("violet", new ColorRGB(238, 130, 238));
        WebPalette.Add("wheat", new ColorRGB(245, 222, 179));
        WebPalette.Add("white", new ColorRGB(255, 255, 255));
        WebPalette.Add("whitesmoke", new ColorRGB(245, 245, 245));
        WebPalette.Add("yellow", new ColorRGB(255, 255, 0));
        WebPalette.Add("yellowgreen", new ColorRGB(154, 205, 50));
    }
}
