using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net;
using System.Text;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace IKTpv25_1_Toooted;

// Tšekk: kitsas kassatšekk (80 mm, nagu poe kassast) PDF-ina kausta AppData\Local\ToodedApp\Arved
// ja HTML-kiri ("Sinu ostutšekk" bänneriga), PDF manusena.
public static class Arve
{
    public const string PoeNimi = "AKT";
    public const decimal Kaibemaks = 0.24m;   // Eesti käibemaks 24 %

    // ===============================================================
    //  PDF – kassatšeki rull
    // ===============================================================
    public static string LooPdf(Ost ost, bool makstud, decimal makstudSumma)
    {
        Directory.CreateDirectory(Kaustad.Arved);
        string fail = Path.Combine(Kaustad.Arved, $"Arve_{ost.Id}_{ost.Aeg:yyyyMMdd_HHmmss}.pdf");

        // Esimene kord ainult mõõdame (kui pikk tšekk tuleb), teine kord joonistame
        var mootja = new Tsekk(null, 0);
        TsekiSisu(mootja, ost, makstud, makstudSumma);
        double korgus = mootja.Y + 24;

        using var dokument = new PdfDocument();
        dokument.Info.Title = Keel.F("Tšekk nr {0}", ost.Id);
        PdfPage leht = dokument.AddPage();
        leht.Width = XUnit.FromMillimeter(80);
        leht.Height = XUnit.FromPoint(Math.Max(korgus, 300));

        using (XGraphics gfx = XGraphics.FromPdfPage(leht))
        {
            var t = new Tsekk(gfx, leht.Width.Point);
            TsekiSisu(t, ost, makstud, makstudSumma);
        }

        dokument.Save(fail);
        return fail;
    }

    private static void TsekiSisu(Tsekk t, Ost ost, bool makstud, decimal makstudSumma)
    {
        string e(decimal v) => v.ToString("0.00", new System.Globalization.CultureInfo("et-EE"));

        t.Logo();
        t.Keskel(Keel.T("AKT e-pood ja kassa · õppeprojekt"));
        t.Keskel("IKTpv25 · Tallinn");
        t.Keskel(EmailSeaded.Lae().Saatja);
        t.Pealkiri(Keel.T("Digitaalne tšekk"));
        if (ost.Klient != null) t.Rida(Keel.F("KLIENT: {0}", Mask(ost.Klient.Kaardinumber)));
        t.Joon();

        foreach (var r in ost.Read)
        {
            t.Toode(r.Toodenimetus, e(r.Summa) + " A");
            if (r.Kogus > 1) t.Rida("  " + Keel.F("{0} x {1}", r.Kogus, e(r.Hind)));
        }
        if (ost.Tarneviis == Tarne.Kojuvedu)
        {
            t.Toode(Keel.T("Kohaletoimetamine"), e(ost.TarneHind) + " A");
            if (ost.TarneHind == 0) t.Rida("  " + Keel.F("Tasuta (ostukorv üle {0} EUR)", e(Tarne.TasutaAlates)));
        }
        t.Joon();

        if (ost.Soodustus > 0)
        {
            t.Rida(Keel.T("SINU SOODUSTUSED:"));
            t.Rida(Keel.T("Boonus kliendikaardilt"), "-" + e(ost.Soodustus));
            t.Kast(Keel.T("Oled säästnud"), e(ost.Soodustus));
            t.Joon();
        }

        bool kaart = ost.Makseviis != "Sularaha";
        t.Rida(Keel.T(kaart ? "KAARDIMAKSE" : "SULARAHA"));
        t.Rida(Keel.T("MÜÜK"));
        t.Rida(Keel.T("AEG"), ost.Aeg.ToString("yyyy-MM-dd HH:mm"));
        t.Rida(Keel.T("SUMMA:"), e(ost.Summa) + " EUR");
        if (!kaart && makstud)
        {
            t.Rida(Keel.T("Makstud:"), e(makstudSumma) + " EUR");
            t.Rida(Keel.T("Tagasiraha:"), e(makstudSumma - ost.Summa) + " EUR");
        }
        if (kaart && makstud) t.Keskel(Keel.T("MAKSE KINNITATUD"));
        t.Joon();

        if (ost.Klient != null)
        {
            t.Rida(Keel.T("Teenitud boonust (1 € = 1 sent)"), e(ost.BoonusLisatud));
            t.Rida(Keel.T("Sinu boonuse saldo"), e(ost.Klient.Boonus));
            t.Joon();
        }

        t.Suur(Keel.T("KOKKU"), e(ost.Summa) + " EUR");
        t.Rida(Keel.T(kaart ? "KAARDIMAKSE" : "SULARAHA"), e(ost.Summa) + " EUR");
        t.Vahe(4);

        // Käibemaksu tabel (hinnad sisaldavad 24 % käibemaksu)
        decimal ilma = Math.Round(ost.Summa / (1 + Kaibemaks), 2);
        t.Veerud(Keel.T("KM %"), Keel.T("KM-ga"), Keel.T("KM-ta"), Keel.T("KM"));
        t.Veerud("A " + e(Kaibemaks * 100) + "%", e(ost.Summa), e(ilma), e(ost.Summa - ilma));
        t.Vahe(4);
        t.Rida(Keel.F("KASSA: {0}", ost.Tarneviis == Tarne.Kojuvedu ? "WEB" : "01"), Keel.F("ARVE NR: {0}", ost.Id.ToString("D8")));
        t.Rida(Keel.F("KUUPÄEV: {0}", ost.Aeg.ToString("dd.MM.yyyy")), Keel.F("AEG: {0}", ost.Aeg.ToString("HH:mm:ss")));
        t.Rida(Keel.F("Müüja: {0}", ost.Kasutaja));

        if (!string.IsNullOrWhiteSpace(ost.Aadress))
        {
            t.Joon();
            t.Rida(Keel.T("KOJUVEO AADRESS:"));
            t.Toode(ost.Aadress!, "");
        }
        if (!makstud)
        {
            t.Joon();
            t.Keskel(Keel.T("Staatus: MAKSMATA"), paks: true);
        }
        t.Joon();
        t.Keskel(Keel.T("Aitäh ostu eest!"));
        t.Keskel(Keel.T("Säilitage tšekk"));
    }

    // Kaardinumbri keskosa peidetakse: 100005 → 10XX05
    private static string Mask(string n) =>
        n.Length <= 4 ? n : n[..2] + new string('X', n.Length - 4) + n[^2..];

    // Kassatšeki "printer": monospace-kiri, read vasakul + paremal
    private sealed class Tsekk
    {
        private readonly XGraphics? g;
        private readonly double vasak = 12, parem;
        private const double Suurus = 7.6;
        private static readonly XPdfFontOptions Unicode = new(PdfFontEncoding.Unicode);
        private static readonly XFont Font = new("Courier New", Suurus, XFontStyle.Regular, Unicode);
        private static readonly XFont Paks = new("Courier New", Suurus, XFontStyle.Bold, Unicode);
        private static readonly XFont SuurFont = new("Courier New", 11, XFontStyle.Bold, Unicode);
        public double Y { get; private set; } = 14;

        private double ReaKorgus => Suurus * 1.32;
        private double MarkLaius => Suurus * 0.6;   // Courier: iga märk 0,6 em
        private int Mahub => (int)((parem - vasak) / MarkLaius);

        public Tsekk(XGraphics? g, double lehtLaius)
        {
            this.g = g;
            parem = (lehtLaius > 0 ? lehtLaius : XUnit.FromMillimeter(80).Point) - 12;
        }

        public void Vahe(double y) => Y += y;

        // AKT logo mustvalgena (kassaprinter): must silt, valge kott ja tekst
        public void Logo()
        {
            const double w = 112, h = 42;
            if (g != null)
            {
                double x = (vasak + parem) / 2 - w / 2, y = Y;
                var silt = new XGraphicsPath();
                silt.AddRoundedRectangle(x, y, w, h, h * 0.36, h * 0.36);
                g.DrawPath(XBrushes.Black, silt);

                double kx = x + h * 0.20, ky = y + h * 0.36, kw = h * 0.46, kh = h * 0.44;
                g.DrawArc(new XPen(XColors.White, h * 0.07), kx + kw * 0.22, ky - kh * 0.42, kw * 0.56, kh * 0.80, 180, 180);
                g.DrawRectangle(XBrushes.White, kx, ky, kw, kh);
                double t = h * 0.075;
                g.DrawEllipse(XBrushes.Black, kx + kw * 0.22 - t / 2, ky + kh * 0.22, t, t);
                g.DrawEllipse(XBrushes.Black, kx + kw * 0.78 - t / 2, ky + kh * 0.22, t, t);

                double tx = x + h * 0.82;
                g.DrawString(PoeNimi, new XFont("Arial", h * 0.46, XFontStyle.Bold, Unicode), XBrushes.White, tx, y + h * 0.56);
                g.DrawString("p o o d", new XFont("Arial", h * 0.17, XFontStyle.Regular, Unicode), XBrushes.White, tx + 2, y + h * 0.82);
            }
            Y += h + 6;
        }

        public void Keskel(string tekst, bool paks = false)
        {
            foreach (var r in Murra(tekst, Mahub))
            {
                g?.DrawString(r, paks ? Paks : Font, XBrushes.Black, new XRect(vasak, Y, parem - vasak, ReaKorgus), XStringFormats.TopCenter);
                Y += ReaKorgus;
            }
        }

        public void Pealkiri(string tekst)
        {
            int kriipse = Math.Max(2, (Mahub - tekst.Length - 2) / 2);
            Keskel(new string('-', kriipse) + " " + tekst + " " + new string('-', kriipse));
        }

        public void Joon()
        {
            Y += 2;
            g?.DrawString(new string('-', Mahub), Font, XBrushes.Black, vasak, Y + Suurus);
            Y += ReaKorgus + 1;
        }

        public void Rida(string vasakul, string paremal = "", bool paks = false)
        {
            var f = paks ? Paks : Font;
            g?.DrawString(vasakul, f, XBrushes.Black, vasak, Y + Suurus);
            if (paremal.Length > 0)
                g?.DrawString(paremal, f, XBrushes.Black, new XRect(vasak, Y, parem - vasak, ReaKorgus), XStringFormats.TopRight);
            Y += ReaKorgus;
        }

        // Pikk toote nimi murtakse mitmele reale, summa viimase rea lõpus
        public void Toode(string nimi, string summa)
        {
            var read = Murra(nimi, Mahub - summa.Length - 2);
            for (int i = 0; i < read.Count; i++)
                Rida(read[i], i == read.Count - 1 ? summa : "");
        }

        public void Kast(string vasakul, string paremal)
        {
            Y += 2;
            double h = ReaKorgus * 1.7;
            g?.DrawRectangle(new XPen(XColors.Black, 0.8), vasak, Y, parem - vasak, h);
            g?.DrawString(vasakul, SuurFont, XBrushes.Black, vasak + 6, Y + h / 2 + 4);
            g?.DrawString(paremal, SuurFont, XBrushes.Black, new XRect(vasak, Y, parem - vasak - 6, h), XStringFormats.CenterRight);
            Y += h + 4;
        }

        public void Suur(string vasakul, string paremal)
        {
            Y += 2;
            g?.DrawString(vasakul, SuurFont, XBrushes.Black, vasak, Y + 11);
            g?.DrawString(paremal, SuurFont, XBrushes.Black, new XRect(vasak, Y, parem - vasak, 14), XStringFormats.TopRight);
            Y += 16;
        }

        public void Veerud(params string[] veerud)
        {
            double laius = (parem - vasak) / veerud.Length;
            for (int i = 0; i < veerud.Length; i++)
            {
                var koht = new XRect(vasak + i * laius, Y, laius, ReaKorgus);
                g?.DrawString(veerud[i], Font, XBrushes.Black, koht, i == 0 ? XStringFormats.TopLeft : XStringFormats.TopRight);
            }
            Y += ReaKorgus;
        }

        private static List<string> Murra(string tekst, int mahub)
        {
            mahub = Math.Max(8, mahub);
            var read = new List<string>();
            var rida = new StringBuilder();
            foreach (var sona in tekst.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string s = sona;
                while (s.Length > mahub) { if (rida.Length > 0) { read.Add(rida.ToString()); rida.Clear(); } read.Add(s[..mahub]); s = s[mahub..]; }
                if (rida.Length > 0 && rida.Length + 1 + s.Length > mahub) { read.Add(rida.ToString()); rida.Clear(); }
                if (rida.Length > 0) rida.Append(' ');
                rida.Append(s);
            }
            if (rida.Length > 0 || read.Count == 0) read.Add(rida.ToString());
            return read;
        }
    }

    // ===============================================================
    //  E-kiri
    // ===============================================================
    public static string KirjaTeema(Ost ost) => Keel.F("Sinu ostutšekk nr {0} – {1}", ost.Id, PoeNimi);

    // Lihttekst (e-posti programmidele, mis HTML-i ei näita)
    public static string KirjaSisu(Ost ost)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Tervitus(ost));
        sb.AppendLine();
        sb.AppendLine(Keel.F("Soovisid saada {0} ostutšeki e-postiga. Saadame tšeki kirja manusena (PDF).", PoeNimi));
        sb.AppendLine();
        sb.AppendLine(Keel.F("Ost nr {0} · {1}", ost.Id, ost.Aeg.ToString("dd.MM.yyyy HH:mm")));
        foreach (var r in ost.Read)
            sb.AppendLine($"  {r.Toodenimetus}  {r.Kogus} × {Ostukorv.Eur(r.Hind)} = {Ostukorv.Eur(r.Summa)}");
        if (ost.Tarneviis == Tarne.Kojuvedu) sb.AppendLine(Keel.F("Kohaletoimetamine: {0}", ost.TarneHind == 0 ? Keel.T("TASUTA") : Ostukorv.Eur(ost.TarneHind)));
        if (ost.Soodustus > 0) sb.AppendLine(Keel.F("Boonuse soodustus: -{0}", Ostukorv.Eur(ost.Soodustus)));
        sb.AppendLine(Keel.F("Kokku: {0}", Ostukorv.Eur(ost.Summa)));
        if (ost.Klient != null) sb.AppendLine(Keel.F("Boonust lisandus: {0}", Ostukorv.Eur(ost.BoonusLisatud)));
        sb.AppendLine();
        sb.AppendLine(Keel.T("Soovime sulle meeldivaid oste!"));
        sb.AppendLine(Keel.F("Sinu {0}", PoeNimi));
        return sb.ToString();
    }

    private static string Tervitus(Ost ost) =>
        ost.Klient != null && !string.IsNullOrWhiteSpace(ost.Klient.Nimi)
            ? Keel.F("Lugupeetud {0},", ost.Klient.Nimi)
            : Keel.T("Lugupeetud klient,");

    // HTML-kiri: bänner (cid:banner), tekst, ostu kokkuvõte, hall jalus
    public static string KirjaHtml(Ost ost, string saaja)
    {
        static string H(string s) => WebUtility.HtmlEncode(s);
        string tugi = EmailSeaded.Lae().Saatja;
        string aktsent = ColorTranslator.ToHtml(Teema.Aktsendid["Punane"].Hele);

        var read = new StringBuilder();
        void Rida(string silt, string vaartus, bool paks = false) =>
            read.Append($"<tr><td style=\"padding:6px 0;color:#666\">{H(silt)}</td>" +
                        $"<td style=\"padding:6px 0;text-align:right;{(paks ? "font-weight:bold;color:#222;font-size:16px" : "color:#333")}\">{H(vaartus)}</td></tr>");
        Rida(Keel.T("Ostu nr"), ost.Id.ToString());
        Rida(Keel.T("Kuupäev"), ost.Aeg.ToString("dd.MM.yyyy HH:mm"));
        Rida(Keel.T("Tooteid"), ost.Read.Sum(r => r.Kogus).ToString());
        if (ost.Tarneviis == Tarne.Kojuvedu)
            Rida(Keel.T("Kojuvedu"), (ost.TarneHind == 0 ? Keel.T("TASUTA") : Ostukorv.Eur(ost.TarneHind)) + (ost.Aadress != null ? " · " + ost.Aadress : ""));
        else
            Rida(Keel.T("Kättesaamine"), Keel.T("Poest kaasa"));
        if (ost.Soodustus > 0) Rida(Keel.T("Boonusega makstud"), "−" + Ostukorv.Eur(ost.Soodustus));
        if (ost.Klient != null) Rida(Keel.T("Teenitud boonust"), "+" + Ostukorv.Eur(ost.BoonusLisatud));
        Rida(Keel.T("Kokku"), Ostukorv.Eur(ost.Summa), true);

        return $@"<!doctype html>
<html><head><meta charset=""utf-8""></head>
<body style=""margin:0;padding:0;background:#ffffff;font-family:Arial,Helvetica,sans-serif"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0""><tr><td align=""center"">
<table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;width:100%"">
  <tr><td><img src=""cid:banner"" width=""600"" alt=""{H(Keel.T("Sinu ostutšekk"))}"" style=""display:block;width:100%;max-width:600px;border:0""></td></tr>
  <tr><td style=""padding:28px 24px 8px;color:#333;font-size:15px;line-height:1.6"">
    <h2 style=""margin:0 0 18px;font-size:20px;color:#222"">{H(Tervitus(ost))}</h2>
    <p style=""margin:0 0 16px;text-align:justify"">{H(Keel.F("Soovisid saada {0} ostutšeki e-postiga. Saadame tšeki kirja manusena (PDF).", PoeNimi))}</p>
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin:6px 0 20px;border-top:1px solid #e5e5e5;border-bottom:1px solid #e5e5e5;font-size:14px"">{read}</table>
    <p style=""margin:0 0 16px;text-align:justify"">{H(Keel.F("Kui sa ei soovi enam ostutšekke e-postiga saada, lülita {0} programmi Seadetes välja valik „Soovin ostutšekke e-postile“.", PoeNimi))}</p>
    <p style=""margin:0 0 28px;text-align:justify"">{H(Keel.F("Küsimuste korral kirjuta meile: {0}", ""))}<a href=""mailto:{H(tugi)}"" style=""color:{aktsent}"">{H(tugi)}</a></p>
    <p style=""margin:0 0 32px"">{H(Keel.T("Soovime sulle meeldivaid oste!"))}<br>{H(Keel.F("Sinu {0}", PoeNimi))}</p>
  </td></tr>
  <tr><td style=""background:#f2f2f2;padding:12px 16px;color:#8a8a8a;font-size:11px;line-height:1.5"">
    {H(Keel.F("Saatsime selle teate aadressile {0}. Kui soovid tšekke tulevikus teisele aadressile, muuda e-posti {1} Seadetes. Said selle kirja, sest andsid selleks nõusoleku.", saaja, PoeNimi))}
  </td></tr>
</table>
</td></tr></table>
</body></html>";
    }

    // Kirja bänner: hall taust, AKT logo, "Sinu ostutšekk" ja tšekirull paremal
    public static byte[] Banner()
    {
        const int W = 1200, Hh = 360;
        using var bmp = new Bitmap(W, Hh);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using (var taust = new LinearGradientBrush(new Rectangle(0, 0, W, Hh), Color.White, Color.FromArgb(222, 224, 226), 20f))
                g.FillRectangle(taust, 0, 0, W, Hh);
            // Heledam diagonaal nagu valgus
            using (var valgus = new SolidBrush(Color.FromArgb(70, Color.White)))
                g.FillPolygon(valgus, new[] { new PointF(380, 0), new PointF(620, 0), new PointF(420, Hh), new PointF(180, Hh) });

            Logo.Joonista(g, new RectangleF(48, 56, 250, 96), valgeTaustal: true);
            using (var f = new Font("Segoe UI Semibold", 34F))
            using (var b = new SolidBrush(Color.FromArgb(70, 70, 72)))
                g.DrawString(Keel.T("Sinu ostutšekk"), f, b, 52, 200);

            // Tšekirull: paber kaardub paremalt alla
            var olek = g.Save();
            g.TranslateTransform(930, 40);
            g.RotateTransform(-22);
            for (int i = 6; i > 0; i--)
                using (var vari = new SolidBrush(Color.FromArgb(10, 0, 0, 0)))
                    g.FillRectangle(vari, -6 + i, 20 + i * 2, 200, 420);
            using (var paber = new LinearGradientBrush(new Rectangle(0, 0, 200, 440), Color.White, Color.FromArgb(236, 236, 236), 0f))
                g.FillRectangle(paber, 0, 20, 200, 420);
            // Rull üleval
            using (var rull = new LinearGradientBrush(new Rectangle(-6, 0, 212, 44), Color.FromArgb(250, 250, 250), Color.FromArgb(200, 200, 200), 90f))
                g.FillEllipse(rull, -6, 0, 212, 44);
            using (var p = new Pen(Color.FromArgb(190, 190, 190)))
                g.DrawEllipse(p, -6, 0, 212, 44);
            // "Tekst" tšekil
            var rnd = new Random(7);
            using (var tint = new SolidBrush(Color.FromArgb(120, 120, 120)))
                for (int y = 60; y < 430; y += 13)
                {
                    int vasak = rnd.Next(12, 26);
                    g.FillRectangle(tint, vasak, y, rnd.Next(60, 120), 4);
                    if (rnd.Next(3) > 0) g.FillRectangle(tint, 150, y, rnd.Next(18, 36), 4);
                }
            g.Restore(olek);
        }
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    // Saadab tšeki e-postiga (HTML + PDF manusena). Tagastab teate kasutajale.
    // Kui e-posti seadeid pole, avatakse e-posti programm ja kaust, kus PDF on.
    public static string Saada(string saaja, string pdfFail, Ost ost)
    {
        var seaded = EmailSeaded.Lae();
        if (!seaded.OnSeadistatud)
        {
            string url = "mailto:" + saaja
                         + "?subject=" + Uri.EscapeDataString(KirjaTeema(ost))
                         + "&body=" + Uri.EscapeDataString(KirjaSisu(ost));
            Ava(url);
            Process.Start("explorer.exe", "/select,\"" + pdfFail + "\"");
            return "E-posti seaded puuduvad, seepärast avati e-posti programm.\n" +
                   "Lisa PDF manusena (fail on kaustas valitud):\n" + pdfFail +
                   "\n\nAutomaatseks saatmiseks täida menüüs Haldus → E-posti seaded.";
        }

        seaded.SaadaHtml(saaja, KirjaTeema(ost), KirjaSisu(ost), KirjaHtml(ost, saaja), Banner(), pdfFail);
        return Keel.F("Tšekk saadetud aadressile {0}", saaja);
    }

    // Avab faili või lingi vaikimisi programmiga (.NET 8: UseShellExecute = true)
    public static void Ava(string failVoiLink)
    {
        Process.Start(new ProcessStartInfo(failVoiLink) { UseShellExecute = true });
    }
}
