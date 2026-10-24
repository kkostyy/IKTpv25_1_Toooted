using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Pere liikmete ringid initsiaalidega; ilmuvad järjest ("pop"), roheline punkt = praegu sisse logitud
public class PereAvatarid : Control
{
    private List<Kasutaja> liikmed = new();
    private double t = 1;
    private string voti = "";

    public PereAvatarid()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        Size = new Size(10, 44);
    }

    public void Naita(List<Kasutaja> uued)
    {
        string uusVoti = string.Join(",", uued.Select(k => k.Id));
        liikmed = uued;
        Width = Math.Max(10, uued.Count * 32 + 12);
        if (uusVoti != voti)
        {
            voti = uusVoti;
            int ms = 380 + uued.Count * 110;
            Animatsioon.Kaivita(this, ms, x => { t = x * ms; Invalidate(); });
        }
        else Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var f = new Font("Segoe UI Semibold", 9F);
        for (int i = 0; i < liikmed.Count; i++)
        {
            var k = liikmed[i];
            double s = Animatsioon.Pop(Math.Clamp((t - i * 110) / 380.0, 0, 1));
            if (s <= 0.01) continue;
            float d = (float)(38 * s), cx = 6 + i * 32 + 19, cy = 22;
            var r = new RectangleF(cx - d / 2, cy - d / 2, d, d);
            using (var b = new SolidBrush(Teema.Taust)) g.FillEllipse(b, RectangleF.Inflate(r, 2, 2));   // valge ääris
            using (var b = new SolidBrush(Pere.AvatariVarv(k.Id))) g.FillEllipse(b, r);
            TextRenderer.DrawText(g, Pere.Initsiaalid(k.Nimi ?? k.Kasutajanimi), f, Rectangle.Round(r), Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            if (k.Id == AktiivneKasutaja.Id)
            {
                using var pb = new SolidBrush(Teema.Taust);
                g.FillEllipse(pb, r.Right - 12, r.Bottom - 12, 12, 12);
                using var b = new SolidBrush(Color.FromArgb(90, 200, 130));
                g.FillEllipse(b, r.Right - 10, r.Bottom - 10, 8, 8);
            }
        }
    }
}

// "PERE KOOD": 7 plaati, esimesed 3 punased; kukuvad järjest oma kohale
public class KoodiPlaadid : Control
{
    private string kood = "";
    private double t = 1;

    public KoodiPlaadid()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = 92;
        BackColor = Color.FromArgb(27, 28, 31);
    }

    public void Naita(string? uus)
    {
        uus ??= "";
        if (uus == kood) return;
        kood = uus;
        Animatsioon.Kaivita(this, 420 + kood.Length * 80, x => { t = x * (420 + kood.Length * 80); Invalidate(); });
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var fp = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        TextRenderer.DrawText(g, Keel.T("PERE KOOD"), fp, new Point(0, 0), Color.FromArgb(170, 175, 182));
        if (kood.Length == 0) return;
        float w = Math.Min(34, (Width - 6 * 5) / 7f);
        using var f = new Font("Consolas", 15F, FontStyle.Bold);
        for (int i = 0; i < kood.Length; i++)
        {
            double s = Math.Clamp((t - i * 80) / 420.0, 0, 1);
            if (s <= 0) continue;
            float nihe = (float)((1 - Animatsioon.Pop(s)) * -24);
            var r = new RectangleF(i * (w + 5), 30 + nihe, w, 46);
            using (var tee = Ymardus.Tee(r, 10))
            using (var b = new SolidBrush(i < 3 ? Color.FromArgb(226, 6, 19) : Color.FromArgb(55, 56, 62)))
                g.FillPath(b, tee);
            TextRenderer.DrawText(g, kood[i].ToString(), f, Rectangle.Round(r), Color.FromArgb((int)(255 * s), Color.White),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}

// "Viimased tegevused": uued read libisevad paremalt sisse
public class TegevusteLoend : Control
{
    private List<(int Id, string Nimi, string Tekst, DateTime Aeg)> read = new();
    private readonly HashSet<string> nahtud = new();
    private readonly Dictionary<string, double> uued = new();

    public TegevusteLoend()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.FromArgb(27, 28, 31);
    }

    private static string Voti((int Id, string Nimi, string Tekst, DateTime Aeg) r) => $"{r.Id}|{r.Tekst}|{r.Aeg.Ticks}";

    public void Naita(List<(int, string, string, DateTime)> uuedRead)
    {
        bool esimene = nahtud.Count == 0 && read.Count == 0;
        read = uuedRead;
        foreach (var r in read)
        {
            string v = Voti(r);
            if (nahtud.Add(v) && !esimene) uued[v] = 0;
        }
        if (uued.Count > 0)
            Animatsioon.Kaivita(this, 500, x => { foreach (var k in uued.Keys.ToList()) uued[k] = x; if (x >= 1) uued.Clear(); Invalidate(); });
        else Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var fp = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        using var f = new Font("Segoe UI", 8.75F);
        using var fi = new Font("Segoe UI Semibold", 8F);
        using var fa = new Font("Segoe UI", 7.5F);
        TextRenderer.DrawText(g, Keel.T("VIIMASED TEGEVUSED"), fp, new Point(0, 0), Color.FromArgb(170, 175, 182));
        int y = 26;
        foreach (var r in read)
        {
            if (y + 40 > Height) break;
            float nihe = uued.TryGetValue(Voti(r), out double s) ? (float)((1 - Animatsioon.Sujuv(s)) * 60) : 0;
            var av = new RectangleF(nihe, y + 2, 30, 30);
            using (var b = new SolidBrush(Pere.AvatariVarv(r.Id))) g.FillEllipse(b, av);
            TextRenderer.DrawText(g, Pere.Initsiaalid(r.Nimi), fi, Rectangle.Round(av), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, $"{r.Nimi} {r.Tekst}", f, new Rectangle((int)(nihe + 40), y, Width - 40, 20), Color.White, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, r.Aeg.ToString("dd.MM HH:mm"), fa, new Point((int)(nihe + 40), y + 19), Color.FromArgb(150, 155, 162));
            y += 44;
        }
        if (read.Count == 0)
            TextRenderer.DrawText(g, Keel.T("Tegevusi pole veel"), f, new Point(0, 30), Color.FromArgb(150, 155, 162));
    }
}

// Pere dialoog: tume taust + kaart tõuseb alt üles; 7 koodilahtrit (liiguvad ise edasi, kleepimine toimib)
public class PereDialoog : Form
{
    private readonly TextBox[] lahtrid = new TextBox[Pere.KoodiPikkus];
    private readonly Label lblViga;
    private Form? vari;
    public bool Muudetud { get; private set; }

    public PereDialoog(bool peres)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        BackColor = Teema.Pind;
        ClientSize = new Size(460, 330);
        KeyPreview = true;
        Opacity = 0;
        Font = UiStiil.Põhifont;

        var tagasi = new TagasiNupp { Location = new Point(20, 18) };
        tagasi.Click += (s, e) => Close();
        Controls.Add(tagasi);
        Controls.Add(new Label { Text = "Liitu perega", Location = new Point(tagasi.Right + 14, 28), AutoSize = true, Font = new Font("Segoe UI Semibold", 15F), ForeColor = Teema.Tekst, BackColor = Teema.Pind });
        Controls.Add(new Label { Text = "Sisesta pereliikmelt saadud 7-märgiline kood. Kõik pereliikmed näevad sama ostunimekirja (samas programmis).", Location = new Point(24, 82), Size = new Size(412, 40), ForeColor = Teema.Tekst2, BackColor = Teema.Pind });

        for (int i = 0; i < lahtrid.Length; i++)
        {
            int n = i;
            var raam = new RoundedPanel { Location = new Point(24 + i * 60, 132), Size = new Size(52, 60), Radius = 12, FillColor = Teema.Paneel2, BorderColor = Teema.Raam, Padding = new Padding(4, 12, 4, 4), BackColor = Teema.Pind };
            var t = lahtrid[i] = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                MaxLength = 1,
                CharacterCasing = CharacterCasing.Upper,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Consolas", 18F, FontStyle.Bold),
                BackColor = Teema.Paneel2,
                ForeColor = i < 3 ? Teema.Aktsent : Teema.Tekst
            };
            t.TextChanged += (s, e) => { if (t.TextLength == 1 && n < lahtrid.Length - 1) lahtrid[n + 1].Focus(); lblViga!.Text = ""; };
            t.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Back && t.TextLength == 0 && n > 0) { lahtrid[n - 1].Focus(); lahtrid[n - 1].Clear(); e.SuppressKeyPress = true; }
                if (e.Control && e.KeyCode == Keys.V) { Kleebi(); e.SuppressKeyPress = true; }
                if (e.KeyCode == Keys.Enter) { Liitu(); e.SuppressKeyPress = true; }
            };
            UiStiil.LisaFookus(raam, t);
            raam.Controls.Add(t);
            Controls.Add(raam);
        }

        lblViga = new Label { Location = new Point(24, 200), Size = new Size(412, 22), ForeColor = Teema.Viga, BackColor = Teema.Pind };
        Controls.Add(lblViga);

        var liitu = UiStiil.LooSuurNupp("Liitu", (s, e) => Liitu());
        liitu.Bounds = new Rectangle(24, 228, 200, 46);
        Controls.Add(liitu);
        var loo = new LinkLabel { Text = "loo uus pere", Location = new Point(24, 290), AutoSize = true, BackColor = Teema.Pind };
        loo.LinkClicked += (s, e) => LooUus();
        Controls.Add(loo);
        if (peres)
        {
            var lahku = new LinkLabel { Text = "lahku perest", Location = new Point(150, 290), AutoSize = true, BackColor = Teema.Pind };
            lahku.LinkClicked += (s, e) =>
            {
                if (Teade.Show("Kas lahkud perest? Sinu ostunimekiri jääb alles.", "Pere", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                Pere.Lahku();
                Muudetud = true;
                Close();
            };
            Controls.Add(lahku);
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        using var tee = Ymardus.Tee(new Rectangle(0, 0, Width, Height), 24);
        Region = new Region(tee);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Teema.Rakenda(this);
        var omanik = Owner ?? ActiveForm;
        var ala = omanik?.Bounds ?? Screen.PrimaryScreen!.WorkingArea;

        // Tume kiht omaniku kohal
        vari = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            ShowInTaskbar = false,
            BackColor = Color.Black,
            Opacity = 0,
            Bounds = ala
        };
        vari.Show(omanik);
        vari.Enabled = false;   // klõps ei too kihti dialoogi ette

        int lopp = ala.Top + (ala.Height - Height) / 2;
        Left = ala.Left + (ala.Width - Width) / 2;
        Top = lopp + 80;
        Animatsioon.Kaivita(this, 420, t =>
        {
            double k = Animatsioon.Sujuv(t);
            Top = (int)(lopp + 80 * (1 - k));
            Opacity = k;
            if (vari != null && !vari.IsDisposed) vari.Opacity = 0.45 * k;
        });
        BeginInvoke(new Action(() => lahtrid[0].Focus()));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape || (e.Alt && e.KeyCode == Keys.Left)) { Close(); e.Handled = true; }
        base.OnKeyDown(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        vari?.Close();
        vari?.Dispose();
        base.OnFormClosed(e);
    }

    private void Kleebi()
    {
        string tekst;
        try { tekst = Clipboard.GetText(); } catch { return; }
        var margid = tekst.ToUpperInvariant().Where(char.IsLetterOrDigit).Take(lahtrid.Length).ToArray();
        for (int i = 0; i < lahtrid.Length; i++) lahtrid[i].Text = i < margid.Length ? margid[i].ToString() : "";
        lahtrid[Math.Min(margid.Length, lahtrid.Length - 1)].Focus();
    }

    private void Liitu()
    {
        string kood = string.Concat(lahtrid.Select(t => t.Text));
        string? viga = Pere.Liitu(kood);
        if (viga != null) { lblViga.Text = viga; return; }
        Teade.Toast("Liitusid perega – nimekiri on nüüd ühine");
        Muudetud = true;
        Close();
    }

    private void LooUus()
    {
        try
        {
            string kood = Pere.LooPere();
            try { Clipboard.SetText(kood); } catch { }
            Teade.Toast(Keel.F("Uus pere loodud. Kood {0} on kopeeritud – saada see pereliikmetele.", kood), TeateTyyp.Edu, 4500);
            Muudetud = true;
            Close();
        }
        catch (Exception ex) { lblViga.Text = ex.Message; }
    }
}
