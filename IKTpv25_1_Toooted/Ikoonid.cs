using System.Globalization;

namespace IKTpv25_1_Toooted;

// Ikoonid joonistatakse programmi sees vektoritena (IkoonidVektor.cs), et need näeksid
// igas arvutis ühesugused välja; koodipunktid on samad mis Segoe MDL2 Assets fondis.
// Kategooriate ikoonid tulevad emoji-fondist (joonistatakse ühevärviliselt, nagu e-poe menüüs)
public static partial class Ikoonid
{
    public const string Ostukorv = "";
    public const string Prygikast = "";
    public const string NoolParemale = "";
    public const string NoolVasakule = "";
    public const string Otsi = "";
    public const string Pluss = "";
    public const string Miinus = "";
    public const string Sulge = "";

    private static readonly CultureInfo Eesti = new("et-EE");

    public static void Joonista(Graphics g, string glyph, Rectangle r, Color varv, float suurus = 12f)
    {
        if (JoonistaVektor(g, glyph, r, varv, suurus)) return;
        using var f = new Font("Segoe MDL2 Assets", suurus);
        TextRenderer.DrawText(g, glyph, f, r, varv,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
    }

    // Kategooria nime järgi sobiv ikoon
    public static string KategooriaIkoon(string? nimi)
    {
        string n = (nimi ?? "").ToLower();
        if (n.Length == 0) return "🛒";
        if (n.Contains("piim") || n.Contains("muna") || n.Contains("juust")) return "🥛";
        if (n.Contains("leib") || n.Contains("sai") || n.Contains("kondiit")) return "🍞";
        if (n.Contains("puuvil") || n.Contains("köögivil") || n.Contains("lill")) return "🍎";
        if (n.Contains("liha") || n.Contains("kala")) return "🍗";
        if (n.Contains("joog")) return "🥤";
        if (n.Contains("maius") || n.Contains("snäk") || n.Contains("komm")) return "🍬";
        if (n.Contains("külm")) return "❄";
        if (n.Contains("vegan")) return "🌱";
        if (n.Contains("valmis")) return "🍲";
        if (n.Contains("laps")) return "🧸";
        if (n.Contains("lemmik")) return "🐾";
        return "🛍";
    }

    public static void JoonistaEmoji(Graphics g, string emoji, Rectangle r, Color varv, float suurus = 13f)
    {
        using var f = new Font("Segoe UI Emoji", suurus);
        TextRenderer.DrawText(g, emoji, f, r, varv,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
    }

    // Hind e-poe stiilis: suur täisosa, väike sendiosa üleval, ühik sendi all.
    // Tagastab ploki laiuse.
    public static int JoonistaHind(Graphics g, decimal hind, string uhik, int x, int y, Color varv, float suurus = 26f)
    {
        int taisosa = (int)Math.Floor(hind);
        int sendid = (int)Math.Round((hind - taisosa) * 100);
        if (sendid == 100) { taisosa++; sendid = 0; }

        using var suur = new Font("Segoe UI Semibold", suurus);
        using var vaike = new Font("Segoe UI Semibold", suurus * 0.42f);
        using var uhikuFont = new Font("Segoe UI", suurus * 0.30f, FontStyle.Bold);

        var fl = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
        string t = taisosa.ToString(Eesti);
        var ts = TextRenderer.MeasureText(g, t, suur, Size.Empty, fl);
        TextRenderer.DrawText(g, t, suur, new Point(x, y), varv, fl);

        int sx = x + ts.Width + 1;
        int sy = y + (int)(suurus * 0.30f);
        string s = sendid.ToString("00");
        var ss = TextRenderer.MeasureText(g, s, vaike, Size.Empty, fl);
        TextRenderer.DrawText(g, s, vaike, new Point(sx, sy), varv, fl);

        string u = "€/" + uhik;
        var us = TextRenderer.MeasureText(g, u, uhikuFont, Size.Empty, fl);
        TextRenderer.DrawText(g, u, uhikuFont, new Point(sx, sy + ss.Height), varv, fl);

        return ts.Width + 1 + Math.Max(ss.Width, us.Width);
    }

    public static string Hind(decimal v) => v.ToString("0.00", Eesti) + "€";
}
