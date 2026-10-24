using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Ostukorvi toiteväärtus (poe leht): vasakul korv sammunuppudega,
//  keskel energia rõngas + toitainete ribad, paremal "Kust kalorid tulevad".
// ===================================================================
public class ToitevaartusLeht : Leht
{
    private readonly Func<IDictionary<int, Toode>> tooted;
    private readonly FlowLayoutPanel korviRead;
    private readonly Label lblPuudu, lblPaevi;
    private readonly ToiteRong rong = new() { Size = new Size(250, 250) };
    private readonly ToiteRibad ribad = new() { Height = 300 };
    private readonly KaloriteAllikad allikad = new();
    private readonly FlowLayoutPanel legend;
    private readonly Label? lblTk;   // moodne: tume "N tk" kiip korvi pealkirja kõrval

    public override string Pealkiri => "Toiteväärtus";

    public ToitevaartusLeht(Func<IDictionary<int, Toode>> tooted)
    {
        this.tooted = tooted;
        AutoScroll = true;
        Padding = new Padding(30, 4, 20, 20);

        var tabel = new TableLayoutPanel { Dock = DockStyle.Top, Height = 700, ColumnCount = 3, RowCount = 2, BackColor = Teema.Taust };
        tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31));
        tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 41));
        tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        tabel.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var pealkiri = new Label { Text = "Ostukorvi toiteväärtus", AutoSize = true, Font = new Font("Segoe UI Semibold", 20F), ForeColor = Teema.Tekst, BackColor = Teema.Taust, Margin = new Padding(0, 4, 0, 0) };
        tabel.Controls.Add(pealkiri, 0, 0);
        tabel.SetColumnSpan(pealkiri, 3);
        bool m = Teema.OnMoodne;
        if (m)
        {
            // lõuend N4: paks pealkiri + hall "päevanorm" kiip
            pealkiri.Font = new Font("Segoe UI", 21F, FontStyle.Bold);
            tabel.SetColumnSpan(pealkiri, 2);
            var norm = new Label { Text = Keel.T("päevanorm = 2000 kcal täiskasvanu"), AutoSize = false, Size = new Size(250, 32), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Teema.Tekst, BackColor = Teema.Pind, Anchor = AnchorStyles.Right, Margin = new Padding(0, 12, 0, 0) };
            using (var t = Ymardus.Tee(new RectangleF(0, 0, 250, 32), 16)) norm.Region = new Region(t);
            tabel.Controls.Add(norm, 2, 0);
        }

        // ---- Vasakul: korv ----
        var vasak = new RoundedPanel { Dock = DockStyle.Fill, Radius = 24, FillColor = Teema.Pind, BorderColor = Teema.OnMoodne ? Teema.Pind : Teema.Joon, BackColor = Teema.Taust, Padding = new Padding(16, 14, 12, 14), Margin = new Padding(0, 0, 12, 0) };
        korviRead = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Teema.Pind };
        lblPuudu = new Label { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(10, 8, 10, 8), Font = new Font("Segoe UI", 8.75F), ForeColor = Teema.Hoiatus, BackColor = Ymardus.Segu(Teema.Pind, Teema.Hoiatus, 0.12f), Visible = false };
        var vasakPealkiri = new Label { Text = "OSTUKORV", Dock = DockStyle.Top, Height = 26, Font = UiStiil.AlapealkiriFont, ForeColor = Teema.TekstNork, BackColor = Teema.Pind };
        vasak.Controls.Add(korviRead);
        vasak.Controls.Add(lblPuudu);
        vasak.Controls.Add(vasakPealkiri);
        if (m)
        {
            vasakPealkiri.Text = Keel.T("Ostukorv");
            vasakPealkiri.Height = 44;
            vasakPealkiri.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            vasakPealkiri.ForeColor = Teema.Tekst;
            lblTk = new Label { AutoSize = false, Size = new Size(58, 28), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(27, 28, 31), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            using (var t = Ymardus.Tee(new RectangleF(0, 0, 58, 28), 14)) lblTk.Region = new Region(t);
            vasakPealkiri.Controls.Add(lblTk);
            vasakPealkiri.Resize += (s, e) => lblTk.Location = new Point(vasakPealkiri.Width - 62, 6);
            lblPuudu.Height = 56;
            lblPuudu.ForeColor = Color.FromArgb(138, 75, 0);
            lblPuudu.BackColor = Color.FromArgb(255, 243, 224);
            lblPuudu.Padding = new Padding(14, 10, 14, 8);
        }
        tabel.Controls.Add(vasak, 0, 1);

        // ---- Keskel: rõngas, päevad, legend, ribad ----
        var kesk = new RoundedPanel { Dock = DockStyle.Fill, Radius = 24, FillColor = Teema.Pind, BorderColor = Teema.OnMoodne ? Teema.Pind : Teema.Joon, BackColor = Teema.Taust, Padding = new Padding(18, 14, 18, 14), Margin = new Padding(0, 0, 12, 0) };
        var keskSisu = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Teema.Pind };
        var ylemine = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Teema.Pind, Margin = new Padding(0) };
        var paremal = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Teema.Pind, Margin = new Padding(16, 30, 0, 0) };
        lblPaevi = new Label { AutoSize = true, Font = new Font("Segoe UI", 34F, FontStyle.Bold), ForeColor = Teema.Tekst, BackColor = Teema.Pind, Margin = new Padding(0) };
        if (m) { lblPaevi.ForeColor = Teema.Teine; lblPaevi.Font = new Font("Segoe UI", 38F, FontStyle.Bold); }
        paremal.Controls.Add(lblPaevi);
        paremal.Controls.Add(new Label { Text = "päeva energiat ühele inimesele", AutoSize = true, MaximumSize = new Size(160, 0), ForeColor = Teema.Tekst2, BackColor = Teema.Pind, Font = new Font("Segoe UI", 9.5F), Margin = new Padding(2, 0, 0, 12) });
        legend = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, BackColor = Teema.Pind, Margin = new Padding(0) };
        paremal.Controls.Add(legend);
        ylemine.Controls.Add(rong);
        ylemine.Controls.Add(paremal);
        keskSisu.Controls.Add(ylemine);
        keskSisu.Controls.Add(ribad);
        keskSisu.Resize += (s, e) => ribad.Width = Math.Max(260, keskSisu.ClientSize.Width - 8);
        kesk.Controls.Add(keskSisu);
        tabel.Controls.Add(kesk, 1, 1);

        // ---- Paremal: tume paneel ----
        var parem = new RoundedPanel { Dock = DockStyle.Fill, Radius = 24, FillColor = Color.FromArgb(27, 28, 31), BorderColor = Color.FromArgb(27, 28, 31), BackColor = Teema.Taust, Padding = new Padding(18, 16, 18, 16), Margin = new Padding(0) };
        allikad.Dock = DockStyle.Fill;
        parem.Controls.Add(allikad);
        tabel.Controls.Add(parem, 2, 1);

        Controls.Add(tabel);
        Resize += (s, e) => tabel.Height = Math.Max(600, ClientSize.Height - 10);

        Ostukorv.Muutus += KorvMuutus;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) Ostukorv.Muutus -= KorvMuutus;
        base.Dispose(disposing);
    }

    private void KorvMuutus()
    {
        if (IsHandleCreated && !IsDisposed) BeginInvoke(new Action(() => Uuenda(false)));
    }

    public override void Avatud() => Uuenda(true);

    private void Uuenda(bool avamine)
    {
        var td = tooted();
        var k = Toitevaartus.Kokkuvote(Ostukorv.Ridad, td);

        // Korvi read
        korviRead.SuspendLayout();
        foreach (Control c in korviRead.Controls.Cast<Control>().ToList()) c.Dispose();
        foreach (var r in Ostukorv.Ridad.ToList())
        {
            decimal? kcal = td.TryGetValue(r.Id, out var t) ? Toitevaartus.TooteKcal(t, r.Kogus) : null;
            korviRead.Controls.Add(Teema.OnMoodne ? new MoodneToiteRida(r, kcal) { Width = Math.Max(240, korviRead.ClientSize.Width - 6) } : KorviRida(r, kcal));
        }
        if (lblTk != null) lblTk.Text = Keel.F("{0} tk", Ostukorv.Ridad.Sum(r => r.Kogus));
        if (Ostukorv.Ridad.Count == 0)
            korviRead.Controls.Add(new Label { Text = "Ostukorv on tühi – lisa tooteid, et näha toiteväärtust.", AutoSize = true, MaximumSize = new Size(240, 0), ForeColor = Teema.TekstNork, BackColor = Teema.Pind, Margin = new Padding(0, 10, 0, 0) });
        korviRead.ResumeLayout();
        Teema.Rakenda(korviRead);

        lblPuudu.Visible = k.IlmaAndmeteta.Count > 0;
        lblPuudu.Text = Keel.T("Toiteväärtus puudub:") + " " + string.Join(", ", k.IlmaAndmeteta);

        lblPaevi.Text = k.Paevi.ToString("0.0", new System.Globalization.CultureInfo("et-EE"));

        // energia jaotus: rasv 9, süsivesikud 4, valgud 4 kcal/g
        decimal er = k.Rasvad * 9, es = k.Susivesikud * 4, ev = k.Valgud * 4, ek = er + es + ev;
        var osad = new[] { ((float)(ek > 0 ? er / ek : 0), Teema.Aktsent, "Rasvad"), ((float)(ek > 0 ? es / ek : 0), Color.FromArgb(240, 150, 40), "Süsivesikud"), ((float)(ek > 0 ? ev / ek : 0), Teema.Edu, "Valgud") };
        rong.Naita(osad, k.Kcal, avamine);
        legend.Controls.Clear();
        foreach (var (osa, varv, nimi) in osad)
            legend.Controls.Add(new LegendiRida(varv, $"{Keel.T(nimi)}  {osa * 100:0} %"));

        var pk = Toitevaartus.Paevakogus;
        ribad.Naita(new[]
        {
            ("Energia", k.Kcal, pk.Kcal, "kcal", false),
            ("Rasvad", k.Rasvad, pk.Rasvad, "g", false),
            ("millest küllastunud", k.Kyllastunud, pk.Kyllastunud, "g", true),
            ("Süsivesikud", k.Susivesikud, pk.Susivesikud, "g", false),
            ("millest suhkrud", k.Suhkrud, pk.Suhkrud, "g", true),
            ("Valgud", k.Valgud, pk.Valgud, "g", false),
            ("Sool", k.Sool, pk.Sool, "g", true),
        });

        allikad.Naita(Ostukorv.Ridad.Where(r => k.KcalToote.ContainsKey(r.Id)).Select(r => (r.Nimi, k.KcalToote[r.Id])).OrderByDescending(x => x.Item2).ToList(), k.Kcal, avamine);
    }

    private Control KorviRida(KorvRida r, decimal? kcal)
    {
        var p = new Panel { Width = 260, Height = 64, BackColor = Teema.Pind, Margin = new Padding(0, 0, 0, 6) };
        p.Controls.Add(new Label { Text = r.Nimi, Location = new Point(0, 2), Size = new Size(160, 36), Font = new Font("Segoe UI Semibold", 9F), ForeColor = Teema.Tekst, BackColor = Teema.Pind });
        p.Controls.Add(new Label
        {
            Text = kcal is decimal kc ? $"{kc:N0} kcal".Replace(",", " ") : Keel.T("andmed puuduvad"),
            Location = new Point(0, 40),
            AutoSize = true,
            ForeColor = kcal == null ? Teema.Hoiatus : Teema.TekstNork,
            BackColor = Teema.Pind,
            Font = new Font("Segoe UI", 8.5F)
        });
        var miinus = (RoundedButton)UiStiil.LooNupp("−", (s, e) => Ostukorv.Vahenda(r));
        var arv = new Label { Text = r.Kogus.ToString(), Size = new Size(26, 32), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI Semibold", 10F), ForeColor = Teema.Tekst, BackColor = Teema.Pind };
        var pluss = (RoundedButton)UiStiil.LooNupp("+", (s, e) => { if (Ostukorv.Suurenda(r) is string viga) Teade.Toast(viga, TeateTyyp.Hoiatus); });
        foreach (var b in new[] { miinus, pluss }) { b.AutoSize = false; b.Size = new Size(32, 32); b.Radius = 16; b.Padding = Padding.Empty; }
        miinus.Location = new Point(164, 4);
        arv.Location = new Point(196, 4);
        pluss.Location = new Point(222, 4);
        p.Controls.AddRange(new Control[] { miinus, arv, pluss });
        return p;
    }
}

// Moodne korvirida (lõuend N4): hall ümar rida, valge pildiruut, nimi, valge −/+ "pill" ja kcal
public class MoodneToiteRida : Control
{
    private readonly KorvRida rida;
    private readonly decimal? kcal;
    private RectangleF miinus, pluss;

    public MoodneToiteRida(KorvRida rida, decimal? kcal)
    {
        this.rida = rida;
        this.kcal = kcal;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = 70;
        Margin = new Padding(0, 0, 0, 8);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        Cursor = miinus.Contains(e.Location) || pluss.Contains(e.Location) ? Cursors.Hand : Cursors.Default;
        base.OnMouseMove(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        if (miinus.Contains(e.Location)) Ostukorv.Vahenda(rida);
        else if (pluss.Contains(e.Location) && Ostukorv.Suurenda(rida) is string viga) Teade.Toast(viga, TeateTyyp.Hoiatus);
        base.OnMouseClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 22)) using (var b = new SolidBrush(Teema.Paneel2)) g.FillPath(b, tee);
        var ruut = new RectangleF(8, 9, 52, 52);
        using (var tee = Ymardus.Tee(ruut, 16)) using (var b = new SolidBrush(Teema.Pind)) g.FillPath(b, tee);
        if (PildiVahemalu.Too(rida.Pilt) is Image pilt)
        {
            float k = Math.Min(40f / pilt.Width, 40f / pilt.Height);
            g.DrawImage(pilt, ruut.X + (52 - pilt.Width * k) / 2, ruut.Y + (52 - pilt.Height * k) / 2, pilt.Width * k, pilt.Height * k);
        }
        using var fb = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        using var f = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        TextRenderer.DrawText(g, rida.Nimi, fb, new Rectangle(70, 8, Width - 78, 20), Teema.Tekst, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        var pill = new RectangleF(70, 33, 90, 30);
        using (var tee = Ymardus.Tee(pill, 15)) using (var b = new SolidBrush(Teema.Pind)) g.FillPath(b, tee);
        miinus = new RectangleF(pill.X, pill.Y, 30, 30);
        pluss = new RectangleF(pill.Right - 30, pill.Y, 30, 30);
        using var fs = new Font("Segoe UI", 11F, FontStyle.Bold);
        TextRenderer.DrawText(g, "−", fs, Rectangle.Round(miinus), Teema.Tekst, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, "+", fs, Rectangle.Round(pluss), Teema.Tekst, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, rida.Kogus.ToString(), fb, new Rectangle((int)pill.X + 30, (int)pill.Y, 30, 30), Teema.Tekst, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        string k2 = kcal is decimal kc ? $"{kc:N0} kcal".Replace(",", " ") : Keel.T("andmed puuduvad");
        TextRenderer.DrawText(g, k2, f, new Rectangle(168, 33, Width - 172, 30), kcal == null ? Color.FromArgb(138, 75, 0) : Teema.Tekst2, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}

// Legend: värviline punkt + tekst
public class LegendiRida : Control
{
    private readonly Color varv;
    public LegendiRida(Color varv, string tekst)
    {
        this.varv = varv;
        Text = tekst;
        Font = new Font("Segoe UI", 9F);
        Size = new Size(TextRenderer.MeasureText(tekst, Font).Width + 24, 22);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Margin = new Padding(0, 0, 0, 2);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? Teema.Pind);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (var b = new SolidBrush(varv)) e.Graphics.FillEllipse(b, 2, 6, 10, 10);
        TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(18, 0, Width - 18, Height), Teema.Tekst2, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}

// Energia rõngas (paksus 26): rasvad / süsivesikud / valgud; avamisel keerleb lahti 1,2 s
public class ToiteRong : Control
{
    private (float Osa, Color Varv, string Nimi)[] osad = Array.Empty<(float, Color, string)>();
    private decimal kcal;
    private double avatud = 1;

    public ToiteRong()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Margin = new Padding(0);
    }

    public void Naita((float, Color, string)[] uued, decimal kokku, bool animeeri)
    {
        osad = uued;
        kcal = kokku;
        if (animeeri) Animatsioon.Kaivita(this, 1200, t => { avatud = Animatsioon.Sujuv(t); Invalidate(); });
        else Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        const float paksus = 26;
        var r = new RectangleF(paksus / 2 + 2, paksus / 2 + 2, Width - paksus - 4, Height - paksus - 4);
        using (var p = new Pen(Teema.Paneel2, paksus)) g.DrawEllipse(p, r);

        float algus = -90 + (float)((1 - avatud) * -180), kokku = (float)(360 * avatud);
        foreach (var (osa, varv, _) in osad)
        {
            float nurk = osa * kokku;
            if (nurk < 0.5f) continue;
            using var p = new Pen(varv, paksus) { StartCap = LineCap.Flat, EndCap = LineCap.Flat };
            g.DrawArc(p, r, algus, Math.Max(0.5f, nurk - 1.5f));
            algus += nurk;
        }

        string arv = ((int)Math.Round(kcal * (decimal)avatud)).ToString("N0", new System.Globalization.CultureInfo("et-EE"));
        using var f1 = new Font("Segoe UI", 24F, FontStyle.Bold);
        using var f2 = new Font("Segoe UI", 9F);
        TextRenderer.DrawText(g, arv, f1, new Rectangle(0, Height / 2 - 30, Width, 40), Teema.Tekst, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, Keel.T("kcal kokku"), f2, new Rectangle(0, Height / 2 + 10, Width, 20), Teema.TekstNork, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

// Toitainete ribad: grammid ja "×" päevasest kogusest; suhkur/küllastunud/sool üle 1,5× = punane kiip.
// Iga muudatuse järel liiguvad ribad uue laiuseni (700 ms).
public class ToiteRibad : Control
{
    private (string Nimi, decimal Vaartus, decimal Paev, string Uhik, bool Hoiata)[] read = Array.Empty<(string, decimal, decimal, string, bool)>();
    private float[] vanad = Array.Empty<float>(), uued = Array.Empty<float>();
    private double t = 1;

    public ToiteRibad()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Margin = new Padding(0, 12, 0, 0);
    }

    public void Naita((string, decimal, decimal, string, bool)[] uuedRead)
    {
        vanad = Enumerable.Range(0, uuedRead.Length).Select(i => i < uued.Length ? Hetk(i) : 0f).ToArray();
        read = uuedRead;
        uued = read.Select(r => r.Item3 > 0 ? (float)Math.Min(2m, r.Item2 / r.Item3) : 0f).ToArray();
        Height = read.Length * 40 + 4;
        Animatsioon.Kaivita(this, 700, x => { t = Animatsioon.Sujuv(x); Invalidate(); });
    }

    private float Hetk(int i) => i < vanad.Length ? vanad[i] + (uued[i] - vanad[i]) * (float)t : uued[i];

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var f = new Font("Segoe UI", 9F);
        using var fb = new Font("Segoe UI Semibold", 9F);
        var et = new System.Globalization.CultureInfo("et-EE");
        for (int i = 0; i < read.Length; i++)
        {
            var (nimi, v, paev, uhik, hoiata) = read[i];
            int y = i * 40;
            decimal kord = paev > 0 ? v / paev : 0;
            bool liiga = hoiata && kord > 1.5m;
            TextRenderer.DrawText(g, Keel.T(nimi), nimi.StartsWith("millest") ? f : fb, new Point(0, y), Teema.Tekst2);
            string paremal = $"{v.ToString(uhik == "kcal" ? "N0" : "0.0", et)} {uhik}";
            TextRenderer.DrawText(g, paremal, f, new Rectangle(0, y, Width - 70, 18), Teema.Tekst2, TextFormatFlags.Right);

            // "× päevast" kiip
            string kiip = $"{kord.ToString("0.0", et)}×";
            var kr = new RectangleF(Width - 60, y - 1, 58, 20);
            using (var tee = Ymardus.Tee(kr, 10))
            using (var b = new SolidBrush(liiga ? Teema.Viga : Teema.Paneel2))
                g.FillPath(b, tee);
            TextRenderer.DrawText(g, kiip, fb, Rectangle.Round(kr), liiga ? Color.White : Teema.Tekst2, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // riba (1× = pool laiust, 2× = täis)
            var rada = new RectangleF(0, y + 22, Width - 2, 10);
            using (var tee = Ymardus.Tee(rada, 5))
            using (var b = new SolidBrush(Teema.Paneel2))
                g.FillPath(b, tee);
            float w = rada.Width * Hetk(i) / 2f;
            if (w > 2)
                using (var tee = Ymardus.Tee(new RectangleF(rada.X, rada.Y, w, rada.Height), 5))
                using (var b = new SolidBrush(liiga ? Teema.Viga : i == 0 ? Teema.Teine : Teema.Edu))
                    g.FillPath(b, tee);
            using (var p = new Pen(Color.FromArgb(120, Teema.TekstNork)) { DashStyle = DashStyle.Dot })
                g.DrawLine(p, rada.X + rada.Width / 2, rada.Y - 2, rada.X + rada.Width / 2, rada.Bottom + 2);   // päevane kogus
        }
    }
}

// Tume paneel: liitriba ja toodete osakaal kaloritest
public class KaloriteAllikad : Control
{
    private List<(string Nimi, decimal Kcal)> read = new();
    private decimal kokku;
    private double t = 1;
    private static readonly Color[] Varvid = { Color.FromArgb(226, 6, 19), Color.FromArgb(240, 150, 40), Color.FromArgb(90, 200, 130), Color.FromArgb(160, 35, 110), Color.FromArgb(80, 150, 230), Color.FromArgb(200, 200, 205) };

    public KaloriteAllikad()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.FromArgb(27, 28, 31);
    }

    public void Naita(List<(string, decimal)> uued, decimal kcal, bool animeeri)
    {
        read = uued;
        kokku = kcal;
        if (animeeri) Animatsioon.Kaivita(this, 900, x => { t = Animatsioon.Sujuv(x); Invalidate(); });
        else Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var fp = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        using var f = new Font("Segoe UI", 9F);
        using var fb = new Font("Segoe UI Semibold", 9F);
        TextRenderer.DrawText(g, Keel.T("KUST KALORID TULEVAD"), fp, new Point(0, 0), Color.FromArgb(170, 175, 182));

        var riba = new RectangleF(0, 28, Width - 2, 16);
        using (var tee = Ymardus.Tee(riba, 8))
        using (var b = new SolidBrush(Color.FromArgb(55, 56, 62)))
            g.FillPath(b, tee);
        float x = 0;
        var state = g.Save();
        using (var klipp = Ymardus.Tee(riba, 8)) g.SetClip(klipp);
        for (int i = 0; i < read.Count && kokku > 0; i++)
        {
            float w = (float)(read[i].Kcal / kokku) * riba.Width * (float)t;
            using var b = new SolidBrush(Varvid[Math.Min(i, Varvid.Length - 1)]);
            g.FillRectangle(b, x, riba.Y, w, riba.Height);
            x += w;
        }
        g.Restore(state);

        int y = 60;
        for (int i = 0; i < read.Count; i++)
        {
            var (nimi, kcal) = read[i];
            using (var b = new SolidBrush(Varvid[Math.Min(i, Varvid.Length - 1)])) g.FillEllipse(b, 0, y + 4, 10, 10);
            int pr = kokku > 0 ? (int)Math.Round(kcal / kokku * 100) : 0;
            TextRenderer.DrawText(g, nimi, f, new Rectangle(18, y, Width - 70, 36), Color.White, TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, $"{pr} %", fb, new Rectangle(Width - 50, y, 48, 18), Color.White, TextFormatFlags.Right);
            y += 42;
        }
        if (read.Count == 0)
            TextRenderer.DrawText(g, Keel.T("Andmeid pole"), f, new Point(0, 60), Color.FromArgb(170, 175, 182));
    }
}
