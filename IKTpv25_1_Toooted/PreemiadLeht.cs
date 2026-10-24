using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Kliendikaardi taust taseme järgi:
//  Pronks – matt vask; Hõbe – harjatud hõbe + läige iga 4 s; Kuld – kuld + hologramm (liigub hiirega) + sädemed
public static class TaseKaart
{
    public static (Color A, Color B) Varvid(Tase t) => t switch
    {
        Tase.Kuld => (Color.FromArgb(167, 122, 30), Color.FromArgb(242, 210, 122)),
        Tase.Hobe => (Color.FromArgb(142, 149, 156), Color.FromArgb(228, 232, 235)),
        _ => (Color.FromArgb(138, 75, 42), Color.FromArgb(201, 133, 82))
    };

    // aeg = sekundid (animatsioon), hiir = 0..1 (hologrammi nihe)
    public static void JoonistaTaust(Graphics g, RectangleF r, Tase tase, double aeg, float hiir)
    {
        var (a, b) = Varvid(tase);
        using var tee = Ymardus.Tee(r, r.Height * 0.06f);
        using (var taust = new LinearGradientBrush(r, a, b, 30f))
            g.FillPath(taust, tee);

        var olek = g.Save();
        g.SetClip(tee);
        if (tase == Tase.Hobe)
        {
            // harjatud metall: peened horisontaalsed jooned
            using var p = new Pen(Color.FromArgb(22, Color.White));
            for (float y = r.Y; y < r.Bottom; y += 3) g.DrawLine(p, r.X, y, r.Right, y);
            Laige(g, r, aeg % 4.0 / 1.2, 70);
        }
        else if (tase == Tase.Kuld)
        {
            // hologrammi vöö (vikerkaar), liigub hiirega ja aeglaselt ise
            float x = r.X + (float)((hiir * 0.8 + 0.1 + Math.Sin(aeg * 0.6) * 0.05) * r.Width);
            var vool = new RectangleF(x - r.Width * 0.25f, r.Y, r.Width * 0.5f, r.Height);
            using (var holo = new LinearGradientBrush(vool, Color.Red, Color.Blue, 20f))
            {
                holo.InterpolationColors = new ColorBlend
                {
                    Colors = new[] { Color.FromArgb(0, 255, 0, 120), Color.FromArgb(70, 255, 80, 160), Color.FromArgb(70, 255, 230, 90), Color.FromArgb(70, 80, 230, 160), Color.FromArgb(70, 90, 140, 255), Color.FromArgb(0, 160, 90, 255) },
                    Positions = new[] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f }
                };
                g.FillRectangle(holo, vool);
            }
            // sädemed
            for (int i = 0; i < 7; i++)
            {
                double faas = (aeg * 1.3 + i * 0.37) % 1.0;
                float s = (float)(Math.Sin(faas * Math.PI) * 5);
                if (s < 0.5f) continue;
                float sx = r.X + r.Width * (0.12f + 0.11f * i), sy = r.Y + r.Height * (0.18f + 0.1f * (i * 3 % 6));
                using var b2 = new SolidBrush(Color.FromArgb(220, Color.White));
                g.FillPolygon(b2, new[] { new PointF(sx, sy - s * 2), new PointF(sx + s / 2, sy), new PointF(sx, sy + s * 2), new PointF(sx - s / 2, sy) });
                g.FillPolygon(b2, new[] { new PointF(sx - s * 2, sy), new PointF(sx, sy + s / 2), new PointF(sx + s * 2, sy), new PointF(sx, sy - s / 2) });
            }
            Laige(g, r, aeg % 5.0 / 1.5, 50);
        }
        using (var ring = new SolidBrush(Color.FromArgb(22, Color.White)))
            g.FillEllipse(ring, r.Right - r.Width * 0.42f, r.Y - r.Height * 0.43f, r.Width * 0.74f, r.Width * 0.74f);
        g.Restore(olek);
    }

    // Diagonaalne läige, mis jookseb üle kaardi (t: 0 → 1 = ühest servast teise)
    private static void Laige(Graphics g, RectangleF r, double t, int alfa)
    {
        if (t > 1) return;
        float x = r.X - r.Width * 0.4f + (float)t * r.Width * 1.8f;
        var vool = new RectangleF(x, r.Y, r.Width * 0.3f, r.Height);
        using var b = new LinearGradientBrush(vool, Color.FromArgb(0, Color.White), Color.FromArgb(0, Color.White), 0f)
        {
            InterpolationColors = new ColorBlend
            {
                Colors = new[] { Color.FromArgb(0, Color.White), Color.FromArgb(alfa, Color.White), Color.FromArgb(0, Color.White) },
                Positions = new[] { 0f, 0.5f, 1f }
            }
        };
        var olek = g.Save();
        g.TranslateTransform(vool.X + vool.Width / 2, vool.Y + vool.Height / 2);
        g.RotateTransform(18);
        g.TranslateTransform(-(vool.X + vool.Width / 2), -(vool.Y + vool.Height / 2));
        g.FillRectangle(b, RectangleF.Inflate(vool, 0, vool.Height));
        g.Restore(olek);
    }

    // Väike medal (ring + täht) taseme värvides
    public static void Medal(Graphics g, RectangleF r, Tase tase, double laige = -1)
    {
        var (a, b) = Varvid(tase);
        using (var br = new LinearGradientBrush(r, b, a, 45f)) g.FillEllipse(br, r);
        using (var p = new Pen(Color.FromArgb(160, Color.White), Math.Max(1.5f, r.Width / 18))) g.DrawEllipse(p, RectangleF.Inflate(r, -r.Width * 0.1f, -r.Width * 0.1f));
        using var f = new Font("Segoe UI Symbol", r.Height * 0.36f, FontStyle.Bold, GraphicsUnit.Pixel);
        TextRenderer.DrawText(g, "★", f, Rectangle.Round(r), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        if (laige >= 0 && laige <= 1)
        {
            using var tee = new GraphicsPath();
            tee.AddEllipse(r);
            var olek = g.Save();
            g.SetClip(tee);
            Laige(g, r, laige, 120);
            g.Restore(olek);
        }
    }
}

// ===================================================================
//  Preemiad: vasakul "SINU TASE", keskel nädala väljakutsed + kleepsualbum,
//  paremal "PÄEVA ÜLLATUS" (kingituse karp).
// ===================================================================
public class PreemiadLeht : Leht
{
    private readonly TaseVeerg taseVeerg = new() { Dock = DockStyle.Fill };
    private readonly FlowLayoutPanel keskSisu;
    private readonly YllatusPaneel yllatus = new() { Dock = DockStyle.Fill };
    public override string Pealkiri => "Preemiad";

    public PreemiadLeht()
    {
        Padding = new Padding(30, 4, 20, 20);
        var tabel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Teema.Taust };
        tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 290));
        tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 290));

        var vasak = new RoundedPanel { Dock = DockStyle.Fill, Radius = 24, FillColor = Color.FromArgb(27, 28, 31), BorderColor = Color.FromArgb(27, 28, 31), BackColor = Teema.Taust, Padding = new Padding(18), Margin = new Padding(0, 0, 12, 0) };
        vasak.Controls.Add(taseVeerg);
        tabel.Controls.Add(vasak, 0, 0);

        keskSisu = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Teema.Taust, Margin = new Padding(0, 0, 12, 0) };
        keskSisu.Resize += (s, e) => { foreach (Control c in keskSisu.Controls) c.Width = Math.Max(300, keskSisu.ClientSize.Width - 8); };
        tabel.Controls.Add(keskSisu, 1, 0);

        var parem = new RoundedPanel { Dock = DockStyle.Fill, Radius = 24, FillColor = Color.FromArgb(160, 35, 110), BorderColor = Color.FromArgb(160, 35, 110), BackColor = Teema.Taust, Padding = new Padding(18), Margin = new Padding(0) };
        parem.Controls.Add(yllatus);
        tabel.Controls.Add(parem, 2, 0);

        Controls.Add(tabel);
    }

    public override void Avatud() => Laadi();

    private void Laadi()
    {
        AktiivneKasutaja.Uuenda();
        if (AktiivneKasutaja.KlientId is not int kid)
        {
            keskSisu.Controls.Clear();
            keskSisu.Controls.Add(new Label { Text = "Preemiate jaoks on vaja kliendikaarti. Ava „Kliendikaart“ ja loo see.", AutoSize = true, Font = new Font("Segoe UI", 11F), ForeColor = Teema.TekstNork, BackColor = Teema.Taust, Margin = new Padding(0, 20, 0, 0) });
            return;
        }
        try
        {
            using var db = new ToodedContext();
            var tase = Preemiad.LeiaTase(db, kid);
            var valjakutsed = Preemiad.Valjakutsed(db, kid, DateTime.Now);
            var kleebised = Preemiad.Kleebised(db, kid);
            bool komplekt = db.Saavutused.Any(s => s.KlientId == kid && s.Kood == "KL:KOIK");
            var tana = Preemiad.TananeYllatus(db, kid, DateTime.Now);

            taseVeerg.Naita(tase);
            yllatus.Naita(tana);

            keskSisu.SuspendLayout();
            foreach (Control c in keskSisu.Controls.Cast<Control>().ToList()) c.Dispose();
            keskSisu.Controls.Add(Pealkiri2("Nädala väljakutsed"));
            for (int i = 0; i < valjakutsed.Count; i++)
                keskSisu.Controls.Add(new ValjakutseKaart(valjakutsed[i], i * 150) { Margin = new Padding(0, 0, 0, 10) });
            keskSisu.Controls.Add(Pealkiri2(Keel.F("Kleepsualbum  {0} / {1}", kleebised.Count(k => k.Saadud), kleebised.Count)));
            keskSisu.Controls.Add(new KleepsuAlbum(kleebised, komplekt));
            keskSisu.ResumeLayout();
            foreach (Control c in keskSisu.Controls) c.Width = Math.Max(300, keskSisu.ClientSize.Width - 8);
        }
        catch (Exception ex) { Teade.Show("Preemiate laadimine ebaõnnestus:\n" + ex.Message); }
    }

    private static Label Pealkiri2(string tekst) => new()
    {
        Text = tekst,
        AutoSize = false,
        Height = 40,
        Font = new Font("Segoe UI Semibold", 14F),
        ForeColor = Teema.Tekst,
        BackColor = Teema.Taust,
        TextAlign = ContentAlignment.BottomLeft,
        Margin = new Padding(0, 4, 0, 8)
    };

    // Kliendikaardilt (ka väljaspool poodi): poes avatakse leht, muidu eraldi aken
    public static void Ava(IWin32Window? omanik)
    {
        if (Application.OpenForms.OfType<PoodVorm>().FirstOrDefault() is PoodVorm pood && pood.Visible)
        {
            pood.AvaLeht(new PreemiadLeht());
            pood.Activate();
            return;
        }
        using var vorm = new Form
        {
            Text = Keel.T("Preemiad"),
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(1180, 720),
            BackColor = Teema.Taust,
            Font = UiStiil.Põhifont,
            ShowInTaskbar = false
        };
        var leht = new PreemiadLeht { Dock = DockStyle.Fill };
        vorm.Controls.Add(leht);
        UiStiil.LisaTagasiNupp(vorm);
        vorm.Load += (s, e) => { Teema.Rakenda(vorm); leht.Avatud(); };
        if (omanik != null) vorm.ShowDialog(omanik); else vorm.ShowDialog();
    }
}

// Vasak tume veerg: mini-kaart, edenemise rõngas, medal läikega, tasemete redel
public class TaseVeerg : Control
{
    private TaseInfo? tase;
    private double t = 1, aeg;

    public TaseVeerg()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.FromArgb(27, 28, 31);
        Animatsioon.Tsykkel(this, s => { aeg = s; Invalidate(); }, 40);
    }

    public void Naita(TaseInfo uus)
    {
        tase = uus;
        Animatsioon.Kaivita(this, 1600, x => t = Animatsioon.Sujuv(x));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        if (tase == null) return;
        var et = new System.Globalization.CultureInfo("et-EE");
        using var fp = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        using var f = new Font("Segoe UI", 9F);
        using var fb = new Font("Segoe UI Semibold", 10F);
        TextRenderer.DrawText(g, Keel.T("SINU TASE"), fp, new Point(0, 0), Color.FromArgb(170, 175, 182));

        // mini-kaart taseme kujundusega
        var kaart = new RectangleF(0, 24, Width - 2, (Width - 2) / 1.586f);
        TaseKaart.JoonistaTaust(g, kaart, tase.Tase, aeg, 0.5f);
        TextRenderer.DrawText(g, Keel.T(tase.Nimi).ToUpper(), fb, Point.Round(new PointF(kaart.X + 14, kaart.Y + 12)), Color.White);
        TextRenderer.DrawText(g, AktiivneKasutaja.Kuvanimi, f, Point.Round(new PointF(kaart.X + 14, kaart.Bottom - 28)), Color.White);

        // edenemise rõngas + medal
        float y0 = kaart.Bottom + 20, d = Math.Min(150, Width - 40);
        var ring = new RectangleF((Width - d) / 2, y0, d, d);
        using (var p = new Pen(Color.FromArgb(55, 56, 62), 12)) g.DrawEllipse(p, RectangleF.Inflate(ring, -6, -6));
        float osa = tase.Jargmine == null ? 1f : (float)(tase.Summa - Preemiad.Tasemed[(int)tase.Tase].Alates) / (float)(Preemiad.Tasemed[(int)tase.Tase + 1].Alates - Preemiad.Tasemed[(int)tase.Tase].Alates);
        using (var p = new Pen(TaseKaart.Varvid(tase.Tase).B, 12) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            if (osa * t > 0.005) g.DrawArc(p, RectangleF.Inflate(ring, -6, -6), -90, (float)(360 * Math.Clamp(osa, 0, 1) * t));
        TaseKaart.Medal(g, RectangleF.Inflate(ring, -30, -30), tase.Tase, aeg % 3.0 / 1.2);

        float y = ring.Bottom + 14;
        string jargmine = tase.Jargmine is Tase j
            ? Keel.F("{0}-tasemeni veel {1} oste", Keel.T(Preemiad.TaseNimi(j)), Ostukorv.Eur(tase.Jargmiseni))
            : Keel.T("Kõrgeim tase – aitäh!");
        TextRenderer.DrawText(g, jargmine, fb, new Rectangle(0, (int)y, Width, 22), Color.White, TextFormatFlags.HorizontalCenter);
        y += 36;

        // redel
        for (int i = Preemiad.Tasemed.Length - 1; i >= 0; i--)
        {
            var (tt, alates, pr) = Preemiad.Tasemed[i];
            var rr = new RectangleF(0, y, Width - 2, 40);
            bool praegu = tt == tase.Tase;
            if (praegu)
                using (var tee = Ymardus.Tee(rr, 12))
                using (var b = new SolidBrush(Color.White))
                    g.FillPath(b, tee);
            TaseKaart.Medal(g, new RectangleF(10, y + 8, 24, 24), tt);
            Color c = praegu ? Color.FromArgb(27, 28, 31) : Color.White;
            TextRenderer.DrawText(g, Keel.T(Preemiad.TaseNimi(tt)), fb, new Point(44, (int)y + 10), c);
            TextRenderer.DrawText(g, $"{(pr * 100).ToString("0.#", et)} %  ·  {Keel.F("alates {0}", Ostukorv.Eur(alates))}", f, new Rectangle(0, (int)y + 11, Width - 14, 20), praegu ? Color.FromArgb(90, 90, 100) : Color.FromArgb(170, 175, 182), TextFormatFlags.Right);
            y += 46;
        }
    }
}

// Nädala väljakutse: ikoon, tekst, edenemisriba (täitub viivitusega), preemia kiip; tehtud = roheline
public class ValjakutseKaart : Control
{
    private readonly Valjakutse v;
    private double t;

    public ValjakutseKaart(Valjakutse v, int viivitus)
    {
        this.v = v;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = 84;
        Animatsioon.Kaivita(this, 900, x => { t = Animatsioon.Sujuv(x); Invalidate(); }, null, viivitus);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        bool valmis = v.Valmis;
        var r = new RectangleF(0.5f, 0.5f, Width - 2, Height - 2);
        using (var tee = Ymardus.Tee(r, 20))
        using (var b = new SolidBrush(valmis ? Ymardus.Segu(Teema.Pind, Teema.Edu, 0.18f) : Teema.Pind))
            g.FillPath(b, tee);
        var ik = new RectangleF(16, 18, 46, 46);
        using (var b = new SolidBrush(valmis ? Teema.Edu : Teema.Paneel2)) g.FillEllipse(b, ik);
        Ikoonid.JoonistaEmoji(g, valmis ? "✓" : v.Ikoon, Rectangle.Round(ik), valmis ? Color.White : Teema.Tekst2, 15f);

        using var fb = new Font("Segoe UI Semibold", 10.5F);
        using var f = new Font("Segoe UI", 9F);
        TextRenderer.DrawText(g, Keel.T(v.Tekst), fb, new Point(76, 14), Teema.Tekst);
        string olek = valmis ? Keel.F("Tehtud! ✓ · +{0} saadud", Ostukorv.Eur(v.Boonus)) : $"{v.Tehtud} / {v.Vaja}";
        TextRenderer.DrawText(g, olek, f, new Point(76, 56), valmis ? Teema.Edu : Teema.TekstNork);

        var rada = new RectangleF(76, 40, Width - 76 - 110, 8);
        using (var tee = Ymardus.Tee(rada, 4))
        using (var b = new SolidBrush(Teema.Paneel2))
            g.FillPath(b, tee);
        float w = rada.Width * v.Tehtud / v.Vaja * (float)t;
        if (w > 2)
            using (var tee = Ymardus.Tee(new RectangleF(rada.X, rada.Y, w, rada.Height), 4))
            using (var b = new SolidBrush(valmis ? Teema.Edu : Teema.Aktsent))
                g.FillPath(b, tee);

        var kiip = new RectangleF(Width - 96, 26, 80, 30);
        using (var tee = Ymardus.Tee(kiip, 15))
        using (var b = new SolidBrush(valmis ? Teema.Edu : Teema.Teine))
            g.FillPath(b, tee);
        TextRenderer.DrawText(g, "+" + Ostukorv.Eur(v.Boonus), fb, Rectangle.Round(kiip), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

// Kleepsualbum 4×2: saadud = värviline ring valge äärise ja läikega (ilmub "pop" + pööre),
// lukus = hall punktiirne; hiire all kallutatud ja suurem. Täis komplekt = kuldne riba.
public class KleepsuAlbum : Control
{
    private readonly List<Kleeps> kleebised;
    private readonly bool komplekt;
    private double t;
    private int hiir = -1;
    private readonly ToolTip vihje = new();

    public KleepsuAlbum(List<Kleeps> kleebised, bool komplekt)
    {
        this.kleebised = kleebised;
        this.komplekt = komplekt;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = komplekt ? 330 : 280;
        int ms = 400 + kleebised.Count * 90;
        Animatsioon.Kaivita(this, ms, x => { t = x * ms; Invalidate(); });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) vihje.Dispose();
        base.Dispose(disposing);
    }

    private RectangleF Koht(int i)
    {
        float w = (Width - 2) / 4f, h = 130;
        return new RectangleF(i % 4 * w, (komplekt ? 50 : 0) + i / 4 * h, w, h);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int uus = Enumerable.Range(0, kleebised.Count).FirstOrDefault(i => Koht(i).Contains(e.Location), -1);
        if (uus != hiir)
        {
            hiir = uus;
            vihje.SetToolTip(this, uus >= 0 ? $"{Keel.T(kleebised[uus].Nimi)} – {Keel.T(kleebised[uus].Kirjeldus)}" : "");
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hiir = -1; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var f = new Font("Segoe UI Semibold", 8.5F);

        if (komplekt)
        {
            var riba = new RectangleF(0, 0, Width - 2, 40);
            using (var tee = Ymardus.Tee(riba, 20))
            using (var b = new LinearGradientBrush(riba, Color.FromArgb(167, 122, 30), Color.FromArgb(242, 210, 122), 0f))
                g.FillPath(b, tee);
            using var fb = new Font("Segoe UI Semibold", 11F);
            TextRenderer.DrawText(g, Keel.F("Kogu kogu! +{0}", Ostukorv.Eur(Preemiad.TaisKomplektiKingitus)), fb, Rectangle.Round(riba), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        for (int i = 0; i < kleebised.Count; i++)
        {
            var k = kleebised[i];
            var koht = Koht(i);
            double s = Math.Clamp((t - i * 90) / 400.0, 0, 1);
            if (s <= 0) continue;
            float suurus = 76 * (float)Animatsioon.Pop(s) * (hiir == i ? 1.1f : 1f);
            float nurk = (float)(-30 * (1 - s)) + (hiir == i ? -6 : 0);
            var c = new PointF(koht.X + koht.Width / 2, koht.Y + 48);
            var olek = g.Save();
            g.TranslateTransform(c.X, c.Y);
            g.RotateTransform(nurk);
            var r = new RectangleF(-suurus / 2, -suurus / 2, suurus, suurus);
            if (k.Saadud)
            {
                using (var vari = new SolidBrush(Color.FromArgb(40, 0, 0, 0))) g.FillEllipse(vari, r.X + 2, r.Y + 4, r.Width, r.Height);
                using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, r);
                var sise = RectangleF.Inflate(r, -5, -5);
                using (var b = new SolidBrush(k.Varv)) g.FillEllipse(b, sise);
                using (var laige = new LinearGradientBrush(sise, Color.FromArgb(90, Color.White), Color.FromArgb(0, Color.White), 90f))
                    g.FillEllipse(laige, sise.X + sise.Width * 0.15f, sise.Y + 2, sise.Width * 0.7f, sise.Height * 0.45f);
                using var fi = new Font("Segoe UI Emoji", Math.Max(6, suurus * 0.26f), FontStyle.Bold, GraphicsUnit.Pixel);
                TextRenderer.DrawText(g, k.Ikoon, fi, Rectangle.Round(sise), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
            else
            {
                using (var b = new SolidBrush(Teema.Paneel2)) g.FillEllipse(b, r);
                using (var p = new Pen(Teema.Raam, 2) { DashStyle = DashStyle.Dash }) g.DrawEllipse(p, RectangleF.Inflate(r, -3, -3));
                using var fi = new Font("Segoe UI Symbol", Math.Max(6, suurus * 0.24f), GraphicsUnit.Pixel);
                TextRenderer.DrawText(g, "🔒", fi, Rectangle.Round(r), Teema.TekstNork, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
            g.Restore(olek);
            TextRenderer.DrawText(g, Keel.T(k.Nimi), f, Rectangle.Round(new RectangleF(koht.X, koht.Y + 92, koht.Width, 20)), k.Saadud ? Teema.Tekst : Teema.TekstNork,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}

// Päeva üllatus: kingituse karp kõigub iga 2,4 s; "Ava kingitus" → kaas lendab, konfetti, toote kaart
public class YllatusPaneel : Control
{
    private (Toode Toode, int Protsent)? tana;
    private double aeg, avamine = -1;   // avamine: -1 = suletud, 0 → 1 animatsioon
    private readonly RoundedButton nupp;
    private readonly RoundedButton korvi;
    private static readonly Color Ploom = Color.FromArgb(160, 35, 110);

    public YllatusPaneel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Ploom;
        nupp = UiStiil.LooSuurNupp("Ava kingitus", (s, e) => Ava(), peamine: false);
        nupp.Height = 46;
        korvi = UiStiil.LooSuurNupp("Lisa korvi", (s, e) => LisaKorvi());
        korvi.Height = 40;
        korvi.Visible = false;
        Controls.Add(nupp);
        Controls.Add(korvi);
        Resize += (s, e) =>
        {
            nupp.Bounds = new Rectangle(0, Height - 50, Width, 46);
            korvi.Bounds = new Rectangle(14, Height - 116, Width - 28, 40);
        };
        Animatsioon.Tsykkel(this, s => { aeg = s; if (avamine < 0) Invalidate(); }, 40);
    }

    public void Naita((Toode, int)? uus)
    {
        tana = uus;
        avamine = uus == null ? -1 : 1;
        nupp.Text = Keel.T(uus == null ? "Ava kingitus" : "Avatud ✓");
        nupp.Enabled = uus == null && AktiivneKasutaja.KlientId != null;
        korvi.Visible = uus != null;
        Invalidate();
    }

    private void Ava()
    {
        if (AktiivneKasutaja.KlientId is not int kid) return;
        try
        {
            using var db = new ToodedContext();
            tana = Preemiad.AvaYllatus(db, kid, DateTime.Now);
            Hinnad.UnustaYllatus();
        }
        catch (Exception ex) { Teade.Show("Viga:\n" + ex.Message); return; }
        if (tana == null) { Teade.Toast("Täna pole üllatuseks sobivaid tooteid", TeateTyyp.Info); return; }
        nupp.Enabled = false;
        nupp.Text = Keel.T("Avatud ✓");
        Animatsioon.Kaivita(this, 1400, x => { avamine = x; Invalidate(); }, () => korvi.Visible = true);
    }

    private void LisaKorvi()
    {
        if (tana is not { } y) return;
        var t = y.Toode;
        string? viga = Ostukorv.LisaToode(t);
        Teade.Toast(viga ?? Keel.F("{0} lisati korvi", t.Toodenimetus), viga == null ? TeateTyyp.Edu : TeateTyyp.Hoiatus);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var fp = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        using var fb = new Font("Segoe UI Semibold", 13F);
        using var f = new Font("Segoe UI", 9F);
        TextRenderer.DrawText(g, Keel.T("PÄEVA ÜLLATUS"), fp, new Point(0, 0), Color.FromArgb(230, 200, 220));
        TextRenderer.DrawText(g, Keel.T("Kord päevas – juhuslik soodustus ühele tootele"), f, new Rectangle(0, 20, Width, 40), Color.White, TextFormatFlags.WordBreak);

        // Karp
        float cx = Width / 2f, cy = 190, w = 120;
        double kalle = avamine < 0 ? Math.Sin(Math.Clamp((aeg % 2.4) / 0.6, 0, 1) * Math.PI * 3) * 6 * (1 - Math.Clamp((aeg % 2.4) / 0.6, 0, 1)) : 0;
        var olek = g.Save();
        g.TranslateTransform(cx, cy + w / 2);
        g.RotateTransform((float)kalle);
        var karp = new RectangleF(-w / 2, -w * 0.75f, w, w * 0.75f);
        using (var b = new SolidBrush(Color.FromArgb(226, 6, 19))) g.FillRectangle(b, karp);
        using (var b = new SolidBrush(Color.FromArgb(245, 210, 106))) g.FillRectangle(b, -9, karp.Y, 18, karp.Height);

        // Kaas lendab üles ja pöörleb
        double a = avamine < 0 ? 0 : Math.Clamp(avamine / 0.45, 0, 1);
        float kaasY = (float)(karp.Y - 22 - a * 160), kaasNurk = (float)(a * -40);
        int alfa = (int)(255 * (1 - Math.Clamp((a - 0.6) / 0.4, 0, 1)));
        if (alfa > 0)
        {
            var o2 = g.Save();
            g.TranslateTransform(0, kaasY + 11);
            g.RotateTransform(kaasNurk);
            var kaas = new RectangleF(-w / 2 - 8, -11, w + 16, 22);
            using (var b = new SolidBrush(Color.FromArgb(alfa, 196, 0, 16))) g.FillRectangle(b, kaas);
            using (var b = new SolidBrush(Color.FromArgb(alfa, 245, 210, 106))) g.FillRectangle(b, -9, kaas.Y, 18, kaas.Height);
            using (var p = new Pen(Color.FromArgb(alfa, 245, 210, 106), 6))
            {
                g.DrawEllipse(p, -26, -30, 24, 20);
                g.DrawEllipse(p, 2, -30, 24, 20);
            }
            g.Restore(o2);
        }
        g.Restore(olek);

        // Konfetti (10 tükki)
        if (avamine > 0 && avamine < 1)
        {
            var v = new[] { Color.Gold, Color.White, Color.FromArgb(90, 200, 130), Color.FromArgb(80, 150, 230), Color.FromArgb(255, 120, 160) };
            for (int i = 0; i < 10; i++)
            {
                double nurk = -Math.PI / 2 + (i - 4.5) * 0.28;
                double kaugus = avamine * 170;
                float x = cx + (float)(Math.Cos(nurk) * kaugus), y = cy - 10 + (float)(Math.Sin(nurk) * kaugus + avamine * avamine * 160);
                var o3 = g.Save();
                g.TranslateTransform(x, y);
                g.RotateTransform((float)(avamine * 720 + i * 40));
                using var b = new SolidBrush(v[i % v.Length]);
                g.FillRectangle(b, -4, -2, 8, 4);
                g.Restore(o3);
            }
        }

        // Toote kaart hüppab välja
        if (tana is { } y2 && avamine >= 0.45)
        {
            double s = Animatsioon.Pop(Math.Clamp((avamine - 0.45) / 0.55, 0, 1));
            var kaart = new RectangleF(10, 300, Width - 20, 120);
            var o4 = g.Save();
            g.TranslateTransform(kaart.X + kaart.Width / 2, kaart.Y + kaart.Height / 2);
            g.ScaleTransform((float)s, (float)s);
            g.TranslateTransform(-(kaart.X + kaart.Width / 2), -(kaart.Y + kaart.Height / 2));
            using (var tee = Ymardus.Tee(kaart, 18))
            using (var b = new SolidBrush(Color.White))
                g.FillPath(b, tee);
            if (PildiVahemalu.Too(y2.Toode.Pilt) is Image pilt)
                PildiVahemalu.Joonista(g, pilt, new RectangleF(kaart.X + 10, kaart.Y + 10, 64, 64));
            TextRenderer.DrawText(g, $"−{y2.Protsent} %", fb, new Point((int)kaart.X + 84, (int)kaart.Y + 12), Color.FromArgb(226, 6, 19));
            TextRenderer.DrawText(g, y2.Toode.Toodenimetus, f, new Rectangle((int)kaart.X + 84, (int)kaart.Y + 40, (int)kaart.Width - 94, 36), Color.FromArgb(46, 46, 46), TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, Keel.T("ainult täna"), f, new Point((int)kaart.X + 84, (int)kaart.Bottom - 26), Color.FromArgb(160, 35, 110));
            g.Restore(o4);
        }
    }
}
