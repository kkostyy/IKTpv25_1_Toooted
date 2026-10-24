namespace IKTpv25_1_Toooted;

// Demoandmed (omanik): kaks loendurit (tooted, ostud), edenemisriba, lisa / eemalda
public class DemoVorm : KujundusVorm
{
    private static bool Kataloog()
    {
        try { using var db = new ToodedContext(); return DemoAndmed.KasutaKataloogi(db); }
        catch { return false; }
    }

    private readonly Loendur tooteid = new("toodet"), oste = new("ostu");
    private readonly Edenemine riba = new() { Visible = false };
    private readonly Label lblOlek;
    private readonly RoundedButton btnLisa, btnEemalda;
    private readonly DemoSammud? sammud;   // moodne: tume veerg sammudega (lõuend N7)

    protected override bool TagasiNupuga => !Teema.OnMoodne;   // moodne: Tagasi on valge kaardi sees

    public DemoVorm() : base("Demoandmed", Teema.OnMoodne ? 1000 : 520, Teema.OnMoodne ? 640 : 380)
    {
        if (Teema.OnMoodne)
        {
            (lblOlek, btnLisa, btnEemalda, sammud) = EhitaMoodne();
            Load += (s, e) => Uuenda(false);
            return;
        }
        Pealkiri("Demoandmed");
        Controls.Add(new Label
        {
            Text = "Üks nupp lisab 3 klienti ja 60 ostu viimase 30 päeva jooksul (tühja poe korral ka 50 toodet), et programm ei näeks esitlusel tühi välja. Demoandmed on märgitud ja neid saab hiljem korraga eemaldada.",
            Location = new Point(20, 56), Size = new Size(480, 56), ForeColor = Teema.Tekst2
        });
        tooteid.Location = new Point(20, 120);
        oste.Location = new Point(270, 120);
        Controls.Add(tooteid);
        Controls.Add(oste);
        riba.Bounds = new Rectangle(20, 236, 480, 8);
        Controls.Add(riba);
        lblOlek = new Label { Location = new Point(20, 252), Size = new Size(480, 22), ForeColor = Teema.TekstNork };
        Controls.Add(lblOlek);

        btnLisa = (RoundedButton)Nupp("Lisa demoandmed", 20, 300, true);
        btnLisa.Click += (s, e) => Lisa();
        btnEemalda = (RoundedButton)Nupp("Eemalda demoandmed", 200, 300);
        btnEemalda.Ohtlik = true;
        btnEemalda.Click += (s, e) => Eemalda();

        Load += (s, e) => Uuenda(false);
    }

    private (Label, RoundedButton, RoundedButton, DemoSammud) EhitaMoodne()
    {
        BackColor = Teema.Taust;
        KeyPreview = true;
        var valge = new RoundedPanel { Bounds = new Rectangle(16, 16, 612, 608), Radius = 16, FillColor = Teema.Pind, BorderColor = Teema.Pind, BackColor = Teema.Taust };
        var tagasi = new TagasiNupp { Location = new Point(24, 24) };
        tagasi.Click += (s, e) => MineTagasi();
        valge.Controls.Add(tagasi);
        valge.Controls.Add(new Label { Text = "Demo-andmed", Location = new Point(tagasi.Right + 14, 20), AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Teema.Tekst, BackColor = Teema.Pind });
        valge.Controls.Add(new Label { Text = "Haldus · ainult omanikule", Location = new Point(tagasi.Right + 16, 52), AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Pind });
        valge.Controls.Add(new Label { Text = "Üks klõps täidab poe, et esitlusel ei oleks midagi tühja. Demo-read märgitakse sildiga DEMO ja need saab hiljem ühe nupuga eemaldada.", Location = new Point(24, 92), Size = new Size(564, 50), Font = new Font("Segoe UI", 10.5F), ForeColor = Teema.Tekst2, BackColor = Teema.Pind });

        var plaadid = new (string N, string T, string S, Color Taust, Color Tekst)[]
        {
            Kataloog()
                ? ("Rimi", "kataloogi tooted", "ostud päris toodetest", Color.FromArgb(226, 6, 19), Color.White)
                : (DemoAndmed.TooteArv.ToString(), "toodet", Keel.F("kõik {0} kategooriat", DemoAndmed.KategooriaArv), Color.FromArgb(226, 6, 19), Color.White),
            (DemoAndmed.OstuArv.ToString(), "ostu", "viimased 30 päeva", Color.FromArgb(27, 28, 31), Color.White),
            (DemoAndmed.KliendiArv.ToString(), "klienti", "kaartide ja boonusega", Teema.Paneel2, Teema.Tekst),
            Kataloog()
                ? ("10", "aeguvat partiid", "Ilma raiskamiseta vitriin", Color.FromArgb(246, 230, 239), Color.FromArgb(160, 35, 110))
                : (DemoAndmed.ToiduArv.ToString(), "toitumisteabega", "toiteväärtuse jaoks", Color.FromArgb(246, 230, 239), Color.FromArgb(160, 35, 110)),
            ("2", "tagastust", "ostude ajaloos", Color.FromArgb(255, 243, 224), Color.FromArgb(138, 75, 0)),
            Kataloog()
                ? ("6", "koos-ostu paari", "ostukorvi analüüsiks", Color.FromArgb(230, 244, 236), Color.FromArgb(17, 104, 58))
                : (DemoAndmed.PandiArv.ToString(), "pandipakendit", "joogid 0,10 €", Color.FromArgb(230, 244, 236), Color.FromArgb(17, 104, 58))
        };
        for (int i = 0; i < plaadid.Length; i++)
            valge.Controls.Add(new DemoPlaat(plaadid[i].N, plaadid[i].T, plaadid[i].S, plaadid[i].Taust, plaadid[i].Tekst, i * 60) { Bounds = new Rectangle(24 + i % 3 * 192, 156 + i / 3 * 104, 182, 94) });

        var olek = new Label { Location = new Point(24, 380), Size = new Size(564, 40), ForeColor = Teema.TekstNork, BackColor = Teema.Pind };
        valge.Controls.Add(olek);
        var lisa = UiStiil.LooSuurNupp("Loo demo-andmed", (s, e) => Lisa());
        lisa.Bounds = new Rectangle(24, 530, 380, 56);
        lisa.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        var eemalda = UiStiil.LooSuurNupp("Eemalda demo", (s, e) => Eemalda(), false);
        eemalda.Bounds = new Rectangle(416, 530, 172, 56);
        eemalda.Ohtlik = true;
        valge.Controls.Add(lisa);
        valge.Controls.Add(eemalda);
        Controls.Add(valge);

        var tume = Color.FromArgb(27, 28, 31);
        var parem = new RoundedPanel { Bounds = new Rectangle(644, 16, 340, 608), Radius = 16, FillColor = tume, BorderColor = tume, BackColor = Teema.Taust, Padding = new Padding(22) };
        var s2 = new DemoSammud { Dock = DockStyle.Fill };
        parem.Controls.Add(s2);
        Controls.Add(parem);
        return (olek, lisa, eemalda, s2);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (Teema.OnMoodne && (keyData == Keys.Escape || keyData == (Keys.Alt | Keys.Left))) { MineTagasi(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void Uuenda(bool animeeri)
    {
        using var db = new ToodedContext();
        var (t, o) = DemoAndmed.Arvud(db);
        if (sammud != null && !animeeri) sammud.Olemas = t > 0;
        tooteid.Naita(t, animeeri);
        oste.Naita(o, animeeri);
        btnLisa.Enabled = t == 0;
        btnEemalda.Enabled = t > 0 || o > 0;
    }

    private void Lisa()
    {
        btnLisa.Enabled = false;
        riba.Visible = true;
        riba.Osa = 0;
        sammud?.Alusta();
        lblOlek.Text = Keel.T("Loon demoandmeid...");
        Application.DoEvents();
        try
        {
            using var db = new ToodedContext();
            string tulemus = DemoAndmed.Lisa(db, osa => { riba.Osa = (float)osa; riba.Refresh(); if (sammud != null) { sammud.Osa = osa; sammud.Refresh(); } });
            sammud?.Valmis();
            lblOlek.Text = tulemus;
            Teade.Toast("Demoandmed lisatud");
        }
        catch (Exception ex) { lblOlek.Text = Keel.T("Viga:") + " " + ex.Message; }
        Uuenda(true);
    }

    private void Eemalda()
    {
        if (Teade.Show("Kas eemaldan kõik demoandmed (tooted, kliendid, ostud)?", "Demoandmed", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            using var db = new ToodedContext();
            lblOlek.Text = DemoAndmed.Eemalda(db);
            riba.Visible = false;
            if (sammud != null) sammud.Olemas = false;
            Teade.Toast("Demoandmed eemaldatud", TeateTyyp.Info);
        }
        catch (Exception ex) { lblOlek.Text = Keel.T("Viga:") + " " + ex.Message; }
        Uuenda(true);
    }
}

// Suur number, mis loeb animatsiooniga 0 → N
public class Loendur : Control
{
    private readonly string silt;
    private int siht, nahtav;

    public Loendur(string silt)
    {
        this.silt = silt;
        Size = new Size(230, 100);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public void Naita(int arv, bool animeeri)
    {
        int algus = animeeri ? nahtav : arv;
        siht = arv;
        Animatsioon.Kaivita(this, animeeri ? 900 : 0, t => { nahtav = (int)Math.Round(algus + (siht - algus) * Animatsioon.Sujuv(t)); Invalidate(); });
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var r = new RectangleF(0.5f, 0.5f, Width - 2, Height - 2);
        using (var tee = Ymardus.Tee(r, 20))
        using (var b = new SolidBrush(Teema.Paneel2))
            g.FillPath(b, tee);
        using var f = new Font("Segoe UI", 30F, FontStyle.Bold);
        using var fs = new Font("Segoe UI", 10F);
        TextRenderer.DrawText(g, nahtav.ToString(), f, new Point(16, 8), Teema.Aktsent);
        TextRenderer.DrawText(g, Keel.T(silt), fs, new Point(20, 66), Teema.Tekst2);
    }
}

// Demo plaat: suur number, mida luuakse, ja selgitus (ilmub üles libisedes)
public class DemoPlaat : Control
{
    private readonly string n, t, s;
    private readonly Color taust, tekst;
    private double a;

    public DemoPlaat(string n, string t, string s, Color taust, Color tekst, int viivitus)
    {
        this.n = n; this.t = t; this.s = s; this.taust = taust; this.tekst = tekst;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Animatsioon.Kaivita(this, 500, x => { a = Animatsioon.Sujuv(x); Invalidate(); }, null, viivitus);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        float dy = (float)(1 - a) * 14;
        var r = new RectangleF(0.5f, 0.5f + dy, Width - 2, Height - 2 - dy);
        using (var tee = Ymardus.Tee(r, 22)) using (var b = new SolidBrush(Color.FromArgb((int)(255 * a), taust))) g.FillPath(b, tee);
        if (a < 0.3) return;
        using var fn = new Font("Segoe UI", 21F, FontStyle.Bold);
        using var ft = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        using var fs = new Font("Segoe UI", 8F);
        TextRenderer.DrawText(g, n, fn, new Point(14, (int)(6 + dy)), tekst);
        TextRenderer.DrawText(g, Keel.T(t), ft, new Point(16, (int)(46 + dy)), tekst);
        TextRenderer.DrawText(g, Keel.T(s), fs, new Rectangle(16, (int)(66 + dy), Width - 20, 18), Color.FromArgb(180, tekst), TextFormatFlags.EndEllipsis);
    }
}

// Tume veerg: "MIS LUUAKSE" → "LOON ANDMEID…": roheline riba ja sammud, lõpus valge "Valmis!"
public class DemoSammud : Control
{
    private static readonly (double Lavi, string Tekst)[] Sammud =
    {
        (0.40, "Tooted (Rimi kataloog või {0} demotoodet)"), (0.55, "{1} klienti kliendikaardiga"), (0.80, "{2} ostu 30 päeva peale"), (1.0, "Statistika ja soojuskaart")
    };
    private double osa;
    private bool tootab, valmis, olemas;
    public double Osa { get => osa; set { osa = value; Invalidate(); } }
    public bool Olemas { get => olemas; set { olemas = value; if (!value) { valmis = tootab = false; osa = 0; } Invalidate(); } }

    public DemoSammud()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    public void Alusta() { tootab = true; valmis = false; osa = 0; Refresh(); }
    public void Valmis() { valmis = true; tootab = false; osa = 1; olemas = true; Animatsioon.Kaivita(this, 400, x => Invalidate()); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var tume = Color.FromArgb(27, 28, 31);
        g.Clear(tume);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var fp = new Font("Segoe UI", 8F, FontStyle.Bold);
        using var f = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        using var fs = new Font("Segoe UI", 9.5F);
        bool kaib = tootab || valmis || olemas;
        TextRenderer.DrawText(g, Keel.T(tootab ? "LOON ANDMEID…" : "MIS LUUAKSE"), fp, new Point(0, 0), Color.FromArgb(235, 120, 175));
        if (!kaib)
        {
            TextRenderer.DrawText(g, Keel.T("Vajuta „Loo demo-andmed“.") + "\n" + Keel.T("Võtab paar sekundit."), fs, new Rectangle(0, 0, Width, Height), Color.FromArgb(185, 189, 196),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            return;
        }
        var rada = new RectangleF(0, 30, Width - 1, 10);
        using (var tee = Ymardus.Tee(rada, 5)) using (var b = new SolidBrush(Color.FromArgb(42, 43, 47))) g.FillPath(b, tee);
        double nahtav = olemas && !tootab ? 1 : osa;
        if (rada.Width * nahtav > 10)
            using (var tee = Ymardus.Tee(new RectangleF(0, 30, (float)(rada.Width * nahtav), 10), 5)) using (var b = new SolidBrush(Color.FromArgb(90, 200, 130))) g.FillPath(b, tee);
        float y = 56;
        foreach (var (lavi, tekst) in Sammud)
        {
            bool tehtud = nahtav >= lavi - 0.001;
            var r = new RectangleF(0, y, Width - 1, 40);
            using (var tee = Ymardus.Tee(r, 20)) using (var b = new SolidBrush(tehtud ? Color.FromArgb(42, 43, 47) : Color.FromArgb(32, 33, 37))) g.FillPath(b, tee);
            var ring = new RectangleF(10, y + 10, 20, 20);
            using (var b = new SolidBrush(tehtud ? Color.FromArgb(90, 200, 130) : Color.FromArgb(60, 62, 68))) g.FillEllipse(b, ring);
            if (tehtud)
                using (var p = new Pen(tume, 2f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
                    g.DrawLines(p, new[] { new PointF(ring.X + 5, ring.Y + 10), new PointF(ring.X + 8.5f, ring.Y + 13.5f), new PointF(ring.X + 15, ring.Y + 6.5f) });
            TextRenderer.DrawText(g, Keel.F(tekst, DemoAndmed.TooteArv, DemoAndmed.KliendiArv, DemoAndmed.OstuArv), f, new Rectangle(40, (int)y, Width - 44, 40), tehtud ? Color.White : Color.FromArgb(120, 125, 132), TextFormatFlags.VerticalCenter);
            y += 48;
        }
        if (valmis || (olemas && !tootab))
        {
            var kast = new RectangleF(0, y + 10, Width - 1, 84);
            using (var tee = Ymardus.Tee(kast, 20)) using (var b = new SolidBrush(Color.White)) g.FillPath(b, tee);
            using var fb = new Font("Segoe UI", 13F, FontStyle.Bold);
            TextRenderer.DrawText(g, Keel.T("Valmis!"), fb, new Point(14, (int)y + 20), tume);
            TextRenderer.DrawText(g, Keel.T("Pood, ajalugu, statistika ja soojuskaart on täidetud."), fs, new Rectangle(14, (int)y + 48, Width - 28, 40), Color.FromArgb(70, 75, 82), TextFormatFlags.WordBreak);
        }
    }
}
