using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

namespace IKTpv25_1_Toooted;

// "Minu dieet" poe lehe kohal: roheline ring lehega, dieedi kiibid, range režiim, loendur
public class DieediKaart : Panel
{
    private readonly FlowLayoutPanel kiibid;
    private readonly Label lblSobib, lblTeada;
    private Func<IReadOnlyList<Toode>> tooted = () => Array.Empty<Toode>();
    private int viimaneArv = -1;

    public DieediKaart()
    {
        if (Teema.OnMoodne)
        {
            (kiibid, lblSobib, lblTeada) = EhitaMoodne();
            MinuDieet.Muutus += Uuenda;
            Keel.Muutus += Uuenda;
            return;
        }
        Height = 76;
        Padding = new Padding(30, 4, 20, 8);
        BackColor = Teema.Taust;
        DoubleBuffered = true;

        var kaart = new RoundedPanel { Dock = DockStyle.Fill, Radius = 22, FillColor = Teema.Pind, BorderColor = Teema.Joon, BackColor = Teema.Taust, Padding = new Padding(64, 0, 16, 0) };
        kaart.Paint += (s, e) =>
        {
            // roheline ring lehega
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(14, (kaart.Height - 40) / 2, 40, 40);
            using (var b = new SolidBrush(Teema.Edu)) g.FillEllipse(b, r);
            Ikoonid.Joonista(g, "", r, Color.White, 13f);   // leht
        };
        var pealkiri = new Label { Text = "Minu dieet", AutoSize = true, Font = new Font("Segoe UI Semibold", 11.5F), ForeColor = Teema.Tekst, BackColor = Teema.Pind, Margin = new Padding(0, 19, 14, 0) };
        kiibid = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, BackColor = Teema.Pind, Padding = new Padding(0, 0, 0, 0) };
        kiibid.Controls.Add(pealkiri);
        foreach (var (mark, nimi) in new[] { (DieediMark.Laktoosivaba, "Laktoosivaba"), (DieediMark.Gluteenivaba, "Gluteenivaba"), (DieediMark.Vegan, "Vegan"), (DieediMark.Pahklivaba, "Pähklivaba"), (DieediMark.Eesti, "Eesti toode") })
        {
            var k = new DieediKiip(nimi, () => MinuDieet.Valik.HasFlag(mark)) { Margin = new Padding(0, 16, 8, 0) };
            k.Click += (s, e) => MinuDieet.Vaheta(mark);
            kiibid.Controls.Add(k);
        }
        var range = new DieediKiip("Range režiim", () => MinuDieet.Range, lyliti: true) { Margin = new Padding(14, 16, 8, 0) };
        range.Click += (s, e) => MinuDieet.SeaRange(!MinuDieet.Range);
        var peida = new DieediKiip("Peida sobimatud", () => MinuDieet.Peida, lyliti: true) { Margin = new Padding(0, 16, 8, 0) };
        peida.Click += (s, e) => MinuDieet.SeaPeida(!MinuDieet.Peida);
        kiibid.Controls.Add(range);
        kiibid.Controls.Add(peida);

        var parem = new Panel { Dock = DockStyle.Right, Width = 210, BackColor = Teema.Pind };
        lblSobib = new Label { Location = new Point(0, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 10.5F), ForeColor = Teema.Tekst, BackColor = Teema.Pind };
        lblTeada = new Label { Location = new Point(0, 34), AutoSize = true, Font = new Font("Segoe UI", 8.5F), ForeColor = Teema.TekstNork, BackColor = Teema.Pind };
        parem.Controls.Add(lblSobib);
        parem.Controls.Add(lblTeada);
        kaart.Controls.Add(kiibid);
        kaart.Controls.Add(parem);
        Controls.Add(kaart);

        MinuDieet.Muutus += Uuenda;
        Keel.Muutus += Uuenda;
    }

    // Moodne (lõuend N3): valge kaart kahe reaga – ülal pealkiri, loendur ja lülitid, all suured kiibid
    private (FlowLayoutPanel, Label, Label) EhitaMoodne()
    {
        Height = 150;
        Padding = new Padding(16, 6, 16, 8);
        BackColor = Teema.Taust;
        DoubleBuffered = true;
        var kaart = new RoundedPanel { Dock = DockStyle.Fill, Radius = 30, FillColor = Teema.Pind, BorderColor = Teema.Pind, BackColor = Teema.Taust };
        kaart.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(20, 16, 44, 44);
            using (var b = new SolidBrush(Teema.Edu)) g.FillEllipse(b, r);
            Ikoonid.Joonista(g, "\uE8BE", r, Color.White, 14f);   // leht
        };
        kaart.Controls.Add(new Label { Text = "Minu dieet", Location = new Point(76, 14), AutoSize = true, Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Teema.Tekst, BackColor = Teema.Pind });
        kaart.Controls.Add(new Label { Text = "Valik jääb meelde ja kehtib kogu poes", Location = new Point(78, 42), AutoSize = true, Font = new Font("Segoe UI", 8.5F), ForeColor = Teema.TekstNork, BackColor = Teema.Pind });

        var sobib = new Label { AutoSize = true, Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Teema.Edu, BackColor = Teema.Pind, Location = new Point(360, 14) };
        var teada = new Label { AutoSize = true, Font = new Font("Segoe UI", 8.5F), ForeColor = Teema.TekstNork, BackColor = Teema.Pind, Location = new Point(362, 42) };
        kaart.Controls.Add(sobib);
        kaart.Controls.Add(teada);

        var lylitid = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Teema.Pind, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        var range = new DieediKiip("Range režiim", () => MinuDieet.Range, lyliti: true) { Margin = new Padding(0, 0, 10, 0) };
        range.Click += (s, e) => MinuDieet.SeaRange(!MinuDieet.Range);
        var peida = new DieediKiip("Peida sobimatud", () => MinuDieet.Peida, lyliti: true) { Margin = new Padding(0, 0, 10, 0) };
        peida.Click += (s, e) => MinuDieet.SeaPeida(!MinuDieet.Peida);
        var info = new IkoonTekst("\uE946", "Kuidas?", Teema.Teine, new Font("Segoe UI Semibold", 9.5F)) { BackColor = Teema.Pind, Margin = new Padding(4, 10, 0, 0) };
        info.Click += (s, e) => Teade.Show(
            "1. SUURTÄHED koostises = allergeen (EL reegel): PIIM, NISU, RUKIS, MUNA…\n" +
            "2. „Võib sisaldada …“ lisainfos → hoiatus. Range režiimis peidetakse.\n" +
            "3. Admin saab märgised ka käsitsi panna (Toote andmed → Märgised).\n" +
            "4. Puu- ja köögiviljad sobivad vaikimisi kõigile dieetidele.", "Kuidas allergeenid leitakse");
        lylitid.Controls.Add(range);
        lylitid.Controls.Add(peida);
        lylitid.Controls.Add(info);
        kaart.Controls.Add(lylitid);
        kaart.Resize += (s, e) => lylitid.Location = new Point(kaart.Width - lylitid.Width - 20, 18);

        var rida = new FlowLayoutPanel { Location = new Point(18, 76), Size = new Size(1200, 50), WrapContents = false, BackColor = Teema.Pind, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        foreach (var (mark, nimi) in new[] { (DieediMark.Laktoosivaba, "Laktoosivaba"), (DieediMark.Gluteenivaba, "Gluteenivaba"), (DieediMark.Vegan, "Vegan"), (DieediMark.Pahklivaba, "Pähklivaba"), (DieediMark.Eesti, "Eesti toode") })
        {
            var k = new DieediKiip(nimi, () => MinuDieet.Valik.HasFlag(mark)) { Margin = new Padding(0, 0, 8, 0) };
            k.Click += (s, e) => MinuDieet.Vaheta(mark);
            rida.Controls.Add(k);
        }
        kaart.Controls.Add(rida);
        Controls.Add(kaart);
        return (rida, sobib, teada);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { MinuDieet.Muutus -= Uuenda; Keel.Muutus -= Uuenda; }
        base.Dispose(disposing);
    }

    public void SeaTooted(Func<IReadOnlyList<Toode>> allikas)
    {
        tooted = allikas;
        Uuenda();
    }

    public void Uuenda()
    {
        var koik = tooted();
        int sobib = koik.Count(MinuDieet.Sobib);
        int teada = koik.Count(t => Dieet.Analuusi(t).KoostisTeada);
        lblSobib.Text = Keel.F("Sulle sobib {0} / {1} toodet", sobib, koik.Count);
        if (Teema.OnMoodne) lblSobib.Left = Math.Max(360, (lblSobib.Parent?.Width ?? 900) / 2 - 140);
        lblTeada.Left = lblSobib.Left + 2;
        lblTeada.Text = Keel.F("Koostis on teada {0} tootel", teada);
        viimaneArv = sobib;
        foreach (var k in kiibid.Controls.OfType<DieediKiip>()) k.Invalidate();
    }
}

// Ümar valikukiip: valitud = roheline, valge linnuke "hüppab" sisse
public class DieediKiip : Control
{
    private readonly string tekst;
    private readonly Func<bool> valitud;
    private readonly bool lyliti;
    private bool hover, enne;
    private double pop = 1;

    public DieediKiip(string tekst, Func<bool> valitud, bool lyliti = false)
    {
        this.tekst = tekst;
        this.valitud = valitud;
        this.lyliti = lyliti;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        Font = new Font("Segoe UI Semibold", 9F);
        Cursor = Cursors.Hand;
        enne = valitud();
        Keel.Muutus += Sobita;
        Sobita();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) Keel.Muutus -= Sobita;
        base.Dispose(disposing);
    }

    private void Sobita()
    {
        if (Teema.OnMoodne)
        {
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Size = new Size(TextRenderer.MeasureText(Keel.T(tekst), Font).Width + (lyliti ? 66 : 52), 42);
        }
        else Size = new Size(TextRenderer.MeasureText(Keel.T(tekst), Font).Width + (lyliti ? 52 : 38), 32);
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnInvalidated(InvalidateEventArgs e)
    {
        bool nyyd = valitud();
        if (nyyd != enne)
        {
            enne = nyyd;
            Animatsioon.Kaivita(this, 320, t => { pop = t; Invalidate(); });
        }
        base.OnInvalidated(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        bool v = valitud();
        var r = new RectangleF(0.5f, 0.5f, Width - 2, Height - 2);

        if (Teema.OnMoodne) { JoonistaMoodne(g, v, r); return; }
        if (lyliti)
        {
            // väike lüliti + tekst
            var rada = new RectangleF(4, (Height - 18) / 2f, 32, 18);
            using (var tee = Ymardus.Tee(rada, 9))
            using (var b = new SolidBrush(v ? Teema.Edu : Teema.Raam))
                g.FillPath(b, tee);
            float s = (float)Animatsioon.Sujuv(pop), vasak = rada.X + 2, parem = rada.Right - 16;
            float x = vasak + (parem - vasak) * (v ? s : 1 - s);
            using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, x, rada.Y + 2, 14, 14);
            TextRenderer.DrawText(g, Keel.T(tekst), Font, new Rectangle(42, 0, Width - 42, Height), hover ? Teema.Tekst : Teema.Tekst2,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            return;
        }

        Color taust = v ? Teema.Edu : hover ? Teema.Paneel2 : Ymardus.Segu(Teema.Pind, Teema.Paneel2, 0.7f);
        using (var tee = Ymardus.Tee(r, r.Height / 2))
        using (var b = new SolidBrush(taust))
            g.FillPath(b, tee);

        if (v)
        {
            // linnuke "pop": kasvab üle 1 ja tuleb tagasi
            float k = (float)Animatsioon.Pop(pop);
            var c = new PointF(18, Height / 2f);
            using var p = new Pen(Color.White, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            g.DrawLines(p, new[] { new PointF(c.X - 5 * k, c.Y), new PointF(c.X - 1.5f * k, c.Y + 3.5f * k), new PointF(c.X + 5 * k, c.Y - 4 * k) });
        }
        else
        {
            using var b = new SolidBrush(Teema.Raam);
            g.FillEllipse(b, 13, Height / 2f - 4, 8, 8);
        }
        TextRenderer.DrawText(g, Keel.T(tekst), Font, new Rectangle(28, 0, Width - 30, Height), v ? Color.White : Teema.Tekst2,
            TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }

    // Moodne: suur roheline kiip valge ringiga (linnuke), lüliti 52×30 (range = ploom)
    private void JoonistaMoodne(Graphics g, bool v, RectangleF r)
    {
        float s = (float)Animatsioon.Sujuv(pop);
        if (lyliti)
        {
            var rada = new RectangleF(2, (Height - 30) / 2f, 52, 30);
            Color c = Ymardus.Segu(Color.FromArgb(205, 211, 215), Teema.Teine, v ? s : 1 - s);
            using (var tee = Ymardus.Tee(rada, 15)) using (var b = new SolidBrush(c)) g.FillPath(b, tee);
            float x = rada.X + 3 + 22 * (v ? s : 1 - s);
            using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, x, rada.Y + 3, 24, 24);
            TextRenderer.DrawText(g, Keel.T(tekst), Font, new Rectangle(62, 0, Width - 62, Height), Teema.Tekst, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            return;
        }
        if (v)
            for (int i = 0; i < 5; i++)   // roheline vari
                using (var vb = new SolidBrush(Color.FromArgb(10, 20, 130, 70)))
                using (var vt = Ymardus.Tee(RectangleF.Inflate(r, -i, -i + 2), r.Height / 2))
                    g.FillPath(vb, vt);
        Color taust = v ? Teema.Edu : hover ? Ymardus.Segu(Teema.Paneel2, Teema.Raam, 0.3f) : Teema.Paneel2;
        using (var tee = Ymardus.Tee(r, r.Height / 2)) using (var b = new SolidBrush(taust)) g.FillPath(b, tee);
        float k = v ? (float)Animatsioon.Pop(pop) : 0.85f;
        var c0 = new PointF(8 + 13, Height / 2f);
        using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, c0.X - 13 * k, c0.Y - 13 * k, 26 * k, 26 * k);
        using (var p = new Pen(v ? Teema.Edu : Color.FromArgb(205, 211, 215), 2.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            g.DrawLines(p, new[] { new PointF(c0.X - 5 * k, c0.Y), new PointF(c0.X - 1.5f * k, c0.Y + 3.5f * k), new PointF(c0.X + 5 * k, c0.Y - 4 * k) });
        TextRenderer.DrawText(g, Keel.T(tekst), Font, new Rectangle(42, 0, Width - 44, Height), v ? Color.White : Teema.Tekst, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}

// Joonistusabi toote kaardile ja toote lehele
public static class DieediMargid
{
    // Kuni 3 kiipi: roheline "✓ Laktoosivaba", punane "Sisaldab: piim", oranž "Võib sisaldada: nisu"
    public static List<(string Tekst, Color Varv)> Kiibid(Toode t)
    {
        var i = Dieet.Analuusi(t);
        var tulemus = new List<(string, Color)>();
        if (i.LaktoosiVaba && i.Sisaldab.Contains("piim")) tulemus.Add(("✓ " + Keel.T("Laktoosivaba"), Teema.Edu));
        else if (i.AdminiMargid != DieediMark.Puudub)
        {
            var esimene = new[] { DieediMark.Gluteenivaba, DieediMark.Vegan, DieediMark.Pahklivaba, DieediMark.Laktoosivaba }.FirstOrDefault(m => i.AdminiMargid.HasFlag(m));
            if (esimene != DieediMark.Puudub) tulemus.Add(("✓ " + Keel.T(esimene == DieediMark.Pahklivaba ? "Pähklivaba" : esimene.ToString()), Teema.Edu));
        }
        var sis = i.Sisaldab.Where(Dieet.OnAllergeen).Take(2).ToList();
        if (sis.Count > 0) tulemus.Add((Keel.F("Sisaldab: {0}", string.Join(", ", sis.Select(Keel.T))), Teema.Viga));
        var voib = i.VoibSisaldada.Where(Dieet.OnAllergeen).Take(2).ToList();
        if (voib.Count > 0) tulemus.Add((Keel.F("Võib sisaldada: {0}", string.Join(", ", voib.Select(Keel.T))), Teema.Hoiatus));
        return tulemus.Take(3).ToList();
    }

    public static void Joonista(Graphics g, Toode t, float x, float y, float maxLaius)
    {
        using var f = new Font("Segoe UI Semibold", 7.5F);
        foreach (var (tekst, varv) in Kiibid(t))
        {
            int w = Math.Min((int)maxLaius, TextRenderer.MeasureText(tekst, f).Width + 14);
            var r = new RectangleF(x, y, w, 20);
            using (var tee = Ymardus.Tee(r, 10))
            using (var b = new SolidBrush(varv))
                g.FillPath(b, tee);
            TextRenderer.DrawText(g, tekst, f, Rectangle.Round(new RectangleF(r.X + 7, r.Y, r.Width - 9, r.Height)), Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            y += 24;
        }
    }

    // Koostise tekst, kus allergeenid on punasel taustal (toote lehel)
    public static RichTextBox Koostis(string tekst, int laius)
    {
        var rtb = new RichTextBox
        {
            Text = tekst,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.None,
            BackColor = Teema.Pind,
            ForeColor = Teema.Tekst2,
            Font = new Font("Segoe UI", 9.5F),
            Width = laius,
            Margin = new Padding(0, 6, 0, 6),
            DetectUrls = false,
            TabStop = false
        };
        foreach (Match m in Regex.Matches(tekst, "[A-ZÕÄÖÜŠŽ]{3,}"))
        {
            rtb.Select(m.Index, m.Length);
            rtb.SelectionBackColor = Teema.VigaTaust2;
            rtb.SelectionColor = Teema.Viga;
            rtb.SelectionFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }
        rtb.Select(0, 0);
        // kõrgus teksti järgi
        rtb.ContentsResized += (s, e) => rtb.Height = e.NewRectangle.Height + 6;
        return rtb;
    }
}
