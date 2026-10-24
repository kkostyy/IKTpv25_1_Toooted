using System.Drawing.Drawing2D;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Müügipaneel (omanik): periood 7/30/90 päeva, 4 KPI-d muutusega,
//  soojuskaart "tund × nädalapäev" ja kõige kasumlikumad kategooriad.
// ===================================================================
public class MyygipaneelVorm : KujundusVorm
{
    private readonly FlowLayoutPanel kpiRida, perioodid;
    private readonly Soojuskaart kaart = new() { Dock = DockStyle.Fill };
    private readonly KategooriaRibad ribad = new() { Dock = DockStyle.Fill };
    private int paevi = 30;
    private readonly MoodneFilter? moodneFilter;   // lõuend N9: perioodid valgel "pillil"

    public MyygipaneelVorm() : base("Müügipaneel", 1180, 760)
    {
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(1000, 640);
        BackColor = Teema.Taust;

        var yla = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Teema.Taust };
        yla.Controls.Add(new Label { Text = "Müügipaneel", Location = new Point(18, 14), AutoSize = true, Font = UiStiil.PealkirjaFont, ForeColor = Teema.Tekst, BackColor = Teema.Taust });
        perioodid = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Teema.Taust };
        yla.Controls.Add(perioodid);
        yla.Resize += (s, e) => perioodid.Location = new Point(yla.Width - perioodid.Width - 18, 14);
        if (Teema.OnMoodne)
        {
            yla.Height = 76;
            perioodid.Visible = false;
            moodneFilter = new MoodneFilter { Top = 14 };
            moodneFilter.Sea(new[] { (Keel.F("{0} päeva", 7), -1), (Keel.F("{0} päeva", 30), -1), (Keel.F("{0} päeva", 90), -1) }, 1);
            moodneFilter.Valitud += i => { paevi = new[] { 7, 30, 90 }[i]; Laadi(); };
            yla.Controls.Add(moodneFilter);
            yla.Resize += (s, e) => moodneFilter.Left = yla.Width - moodneFilter.Width - 18;
        }

        kpiRida = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 116, Padding = new Padding(16, 0, 16, 8), BackColor = Teema.Taust, WrapContents = false };

        var tabel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(16, 0, 16, 16), BackColor = Teema.Taust };
        tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        var kaartKaart = UiStiil.LooKaardiPaneel("Ostud: tund × nädalapäev", kaart);
        kaartKaart.Dock = DockStyle.Fill;
        kaartKaart.Margin = new Padding(0, 0, 12, 0);
        var ribaKaart = UiStiil.LooKaardiPaneel("Kõige kasumlikumad kategooriad", ribad);
        ribaKaart.Dock = DockStyle.Fill;
        if (Teema.OnMoodne)
        {
            // tume kategooriate kaart
            var tume = Color.FromArgb(27, 28, 31);
            ribaKaart.FillColor = ribaKaart.BorderColor = tume;
            foreach (Control c in ribaKaart.Controls) { c.BackColor = tume; if (c is Label l) l.ForeColor = Color.White; }
            ribad.Tume = true;
        }
        tabel.Controls.Add(kaartKaart, 0, 0);
        tabel.Controls.Add(ribaKaart, 1, 0);

        Controls.Add(tabel);
        Controls.Add(kpiRida);
        Controls.Add(yla);
        tabel.BringToFront();

        Load += (s, e) => Laadi();
    }

    private void Laadi()
    {
        perioodid.SuspendLayout();
        foreach (Control c in perioodid.Controls.Cast<Control>().ToList()) c.Dispose();
        foreach (int p in new[] { 7, 30, 90 })
        {
            var b = (RoundedButton)UiStiil.LooNupp(Keel.F("{0} päeva", p), (s, e) => { paevi = p; Laadi(); }, p == paevi);
            b.Radius = 16;
            perioodid.Controls.Add(b);
        }
        perioodid.ResumeLayout();

        try
        {
            using var db = new ToodedContext();
            var nyyd = DateTime.Now;
            var ostud = Myygianalyys.Ostud(db, nyyd.AddDays(-paevi), nyyd.AddMinutes(1));
            var enne = Myygianalyys.Ostud(db, nyyd.AddDays(-2 * paevi), nyyd.AddDays(-paevi));
            var tooted = db.Toodetabel.AsNoTracking().ToDictionary(t => t.Id);
            var nimed = db.Kategooriatabel.AsNoTracking().ToDictionary(k => k.Id, k => k.Kategooria_nimetus);

            var k = Myygianalyys.Kpi(ostud, tooted);
            var k0 = Myygianalyys.Kpi(enne, tooted);
            kpiRida.SuspendLayout();
            foreach (Control c in kpiRida.Controls.Cast<Control>().ToList()) c.Dispose();
            kpiRida.Controls.Add(new KpiPlaat("Käive", Ostukorv.Eur(k.Kaive), Myygianalyys.Muutus(k.Kaive, k0.Kaive), 0, 0));
            kpiRida.Controls.Add(new KpiPlaat("Oste", k.Oste.ToString(), Myygianalyys.Muutus(k.Oste, k0.Oste), 80, 1));
            kpiRida.Controls.Add(new KpiPlaat("Keskmine ost", Ostukorv.Eur(k.Keskmine), Myygianalyys.Muutus(k.Keskmine, k0.Keskmine), 160, 2));
            kpiRida.Controls.Add(new KpiPlaat("Marginaal", Ostukorv.Eur(k.Marginaal), Myygianalyys.Muutus(k.Marginaal, k0.Marginaal), 240, 3));
            kpiRida.ResumeLayout();
            Teema.Rakenda(kpiRida);
            if (Teema.OnMoodne)
            {
                // plaadid jagavad rea laiuse võrdselt
                int w = (kpiRida.ClientSize.Width - kpiRida.Padding.Horizontal - 36) / 4;
                foreach (Control c in kpiRida.Controls) c.Width = Math.Max(200, w);
            }

            kaart.Naita(Myygianalyys.Soojuskaart(ostud));
            ribad.Naita(Myygianalyys.Kategooriad(ostud, tooted, nimed));
        }
        catch (Exception ex) { Teade.Show("Andmete laadimine ebaõnnestus:\n" + ex.Message); }
    }
}

// KPI plaat: pealkiri, suur number, muutus ▲/▼ (ilmub viivitusega alt)
public class KpiPlaat : Control
{
    private readonly string silt, vaartus;
    private readonly decimal? muutus;
    private double t;

    private readonly int jrk;
    // moodne: plaadid eri värvi (tume, valge, roosa, roheline)
    private static readonly (Color Taust, Color Tekst)[] MoodsadVarvid =
    {
        (Color.FromArgb(27, 28, 31), Color.White), (Color.White, Color.FromArgb(27, 28, 31)),
        (Color.FromArgb(246, 230, 239), Color.FromArgb(160, 35, 110)), (Color.FromArgb(230, 244, 236), Color.FromArgb(17, 104, 58))
    };

    public KpiPlaat(string silt, string vaartus, decimal? muutus, int viivitus, int jrk = -1)
    {
        this.jrk = jrk;
        this.silt = silt;
        this.vaartus = vaartus;
        this.muutus = muutus;
        Size = new Size(250, 100);
        Margin = new Padding(0, 0, 12, 0);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Animatsioon.Kaivita(this, 500, x => { t = Animatsioon.Sujuv(x); Invalidate(); }, null, viivitus);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        float nihe = (float)((1 - t) * 14);
        var r = new RectangleF(0.5f, 0.5f + nihe, Width - 2, Height - 2 - nihe);
        if (Teema.OnMoodne && jrk >= 0)
        {
            var (taust, tekst) = MoodsadVarvid[jrk % 4];
            int al = (int)(255 * t);
            using (var tee = Ymardus.Tee(r, 24)) using (var b = new SolidBrush(Color.FromArgb(al, taust))) g.FillPath(b, tee);
            using var fs2 = new Font("Segoe UI", 8.5F);
            using var fv2 = new Font("Segoe UI", 21F, FontStyle.Bold);
            using var fm2 = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            TextRenderer.DrawText(g, Keel.T(silt), fs2, Point.Round(new PointF(18, 14 + nihe)), Color.FromArgb(al * 3 / 4, tekst));
            TextRenderer.DrawText(g, vaartus, fv2, Point.Round(new PointF(14, 32 + nihe)), Color.FromArgb(al, tekst));
            string m2 = muutus is decimal mm ? $"{(mm >= 0 ? "▲" : "▼")} {Math.Abs(mm)} %  " + Keel.T("eelmise perioodiga") : Keel.T("eelmist perioodi pole");
            TextRenderer.DrawText(g, m2, fm2, Point.Round(new PointF(18, 74 + nihe)), Color.FromArgb(al * 4 / 5, tekst));
            return;
        }
        using (var tee = Ymardus.Tee(r, 20))
        using (var b = new SolidBrush(Ymardus.Segu(Teema.Taust, Teema.Pind, (float)t)))
            g.FillPath(b, tee);
        using var fs = new Font("Segoe UI", 8F, FontStyle.Bold);
        using var fv = new Font("Segoe UI", 20F, FontStyle.Bold);
        using var fm = new Font("Segoe UI Semibold", 9F);
        var a = (int)(255 * t);
        TextRenderer.DrawText(g, Keel.T(silt).ToUpper(), fs, Point.Round(new PointF(16, 12 + nihe)), Color.FromArgb(a, Teema.TekstNork));
        TextRenderer.DrawText(g, vaartus, fv, Point.Round(new PointF(12, 30 + nihe)), Color.FromArgb(a, Teema.Tekst));
        if (muutus is decimal m)
        {
            bool yles = m >= 0;
            TextRenderer.DrawText(g, $"{(yles ? "▲" : "▼")} {Math.Abs(m)} %  " + Keel.T("eelmise perioodiga"), fm, Point.Round(new PointF(16, 72 + nihe)),
                Color.FromArgb(a, yles ? Teema.Edu : Teema.Viga));
        }
        else TextRenderer.DrawText(g, Keel.T("eelmist perioodi pole"), fm, Point.Round(new PointF(16, 72 + nihe)), Color.FromArgb(a, Teema.TekstNork));
    }
}

// Soojuskaart: 7 rida (E–P) × tunnid 8–22. Värv kvantiilide järgi Paneel2 → Aktsent.
// Lahtrid ilmuvad lainena (diagonaali viivitus); hiire all vihje "Neljapäev 18:00 · 12 ostu · 184,30 €".
public class Soojuskaart : Control
{
    private (int Arv, decimal Summa)[,] andmed = new (int, decimal)[7, 15];
    private decimal[] piirid = Array.Empty<decimal>();
    private double t = 1;
    private (int P, int T) hiir = (-1, -1);
    private readonly ToolTip vihje = new();
    private static readonly string[] Paevad = { "E", "T", "K", "N", "R", "L", "P" };
    private static readonly string[] PaevaNimed = { "Esmaspäev", "Teisipäev", "Kolmapäev", "Neljapäev", "Reede", "Laupäev", "Pühapäev" };

    public Soojuskaart()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) vihje.Dispose();
        base.Dispose(disposing);
    }

    public void Naita((int, decimal)[,] uued)
    {
        andmed = uued;
        var vaartused = new List<decimal>();
        foreach (var v in andmed) if (v.Item2 > 0) vaartused.Add(v.Item2);
        vaartused.Sort();
        piirid = vaartused.Count == 0 ? Array.Empty<decimal>() : new[] { 0.2, 0.4, 0.6, 0.8 }.Select(q => vaartused[(int)(q * (vaartused.Count - 1))]).ToArray();
        Animatsioon.Kaivita(this, 1100, x => { t = x * 1100; Invalidate(); });
    }

    private int Tunde => andmed.GetLength(1);
    private RectangleF Lahter(int p, int tund)
    {
        float vasak = 30, yla = 22, w = (Width - vasak - 4) / Tunde, h = (Height - yla - 4 - (Teema.OnMoodne ? 34 : 0)) / 7f;
        return new RectangleF(vasak + tund * w + 2, yla + p * h + 2, w - 4, h - 4);
    }

    private int Aste(decimal v)
    {
        if (v <= 0) return 0;
        int i = 1;
        foreach (var p in piirid) if (v > p) i++;
        return Math.Min(i, 5);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        (int, int) uus = (-1, -1);
        for (int p = 0; p < 7; p++)
            for (int tt = 0; tt < Tunde; tt++)
                if (Lahter(p, tt).Contains(e.Location)) uus = (p, tt);
        if (uus != hiir)
        {
            hiir = uus;
            if (uus.Item1 >= 0)
            {
                var (arv, summa) = andmed[uus.Item1, uus.Item2];
                vihje.SetToolTip(this, $"{Keel.T(PaevaNimed[uus.Item1])} {Myygianalyys.AlgusTund + uus.Item2}:00 · {Keel.F("{0} ostu", arv)} · {Ostukorv.Eur(summa)}");
            }
            else vihje.SetToolTip(this, "");
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var f = new Font("Segoe UI", 8F);
        for (int tt = 0; tt < Tunde; tt += 2)
        {
            var r = Lahter(0, tt);
            TextRenderer.DrawText(g, (Myygianalyys.AlgusTund + tt).ToString(), f, new Rectangle((int)r.X, 2, (int)r.Width, 16), Teema.TekstNork, TextFormatFlags.HorizontalCenter);
        }
        for (int p = 0; p < 7; p++)
        {
            var r0 = Lahter(p, 0);
            string luhend = Keel.Praegu == KeeleKood.ET ? Paevad[p] : Keel.T(PaevaNimed[p])[..2];
            TextRenderer.DrawText(g, luhend, f, new Rectangle(0, (int)r0.Y, 26, (int)r0.Height), Teema.TekstNork, TextFormatFlags.VerticalCenter);
            for (int tt = 0; tt < Tunde; tt++)
            {
                var r = Lahter(p, tt);
                double s = Math.Clamp((t - (p + tt) * 35) / 300.0, 0, 1);   // laine diagonaalis
                if (s <= 0) continue;
                int aste = Aste(andmed[p, tt].Summa);
                Color c = Teema.OnMoodne ? Ploom(aste / 5f) : aste == 0 ? Teema.Paneel2 : Ymardus.Segu(Teema.AktsentHele, Teema.Aktsent, (aste - 1) / 4f);
                var rr = RectangleF.Inflate(r, -(float)(r.Width * (1 - s) / 2), -(float)(r.Height * (1 - s) / 2));
                using var tee = Ymardus.Tee(rr, Math.Min(Teema.OnMoodne ? 10 : 6, rr.Height / 2));
                using (var b = new SolidBrush(c)) g.FillPath(b, tee);
                if (hiir == (p, tt)) using (var pen = new Pen(Teema.Tekst, 2)) g.DrawPath(pen, tee);
            }
        }
        if (Teema.OnMoodne) Legend(g);
    }

    // Moodne skaala: helehall → roosa → ploom → tumeploom
    private static Color Ploom(float t)
    {
        Color[] a = { Color.FromArgb(241, 243, 244), Color.FromArgb(246, 230, 239), Color.FromArgb(235, 120, 175), Color.FromArgb(160, 35, 110), Color.FromArgb(91, 18, 64) };
        float x = Math.Min(3.999f, Math.Max(0, t) * 4);
        int i = (int)x;
        return Ymardus.Segu(a[i], a[i + 1], x - i);
    }

    // "Vähem ▢▢▢▢▢ Rohkem" ja tipptunnid
    private void Legend(Graphics g)
    {
        using var f = new Font("Segoe UI", 8.5F);
        using var fb = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        int y = Height - 24;
        TextRenderer.DrawText(g, Keel.T("Vähem"), f, new Point(30, y), Teema.TekstNork);
        int x = 30 + TextRenderer.MeasureText(Keel.T("Vähem"), f).Width + 4;
        for (int i = 0; i < 5; i++)
        {
            using (var tee = Ymardus.Tee(new RectangleF(x, y + 2, 22, 14), 4)) using (var b = new SolidBrush(Ploom(i / 4f))) g.FillPath(b, tee);
            x += 26;
        }
        TextRenderer.DrawText(g, Keel.T("Rohkem"), f, new Point(x + 2, y), Teema.TekstNork);
        // kaks kõige tihedamat lahtrit
        var tipp = new List<(int P, int T, decimal S)>();
        for (int p = 0; p < 7; p++) for (int tt = 0; tt < Tunde; tt++) if (andmed[p, tt].Summa > 0) tipp.Add((p, tt, andmed[p, tt].Summa));
        if (tipp.Count == 0) return;
        string tekst = Keel.T("Tipp:") + " " + string.Join(", ", tipp.OrderByDescending(v => v.S).Take(2).Select(v =>
            $"{(Keel.Praegu == KeeleKood.ET ? Paevad[v.P] : Keel.T(PaevaNimed[v.P])[..2])} {Myygianalyys.AlgusTund + v.T}:00"));
        TextRenderer.DrawText(g, tekst, fb, new Rectangle(0, y, Width - 4, 18), Teema.Teine, TextFormatFlags.Right);
    }
}

// Kategooriad marginaali järgi: horisontaalsed ribad kasvavad vasakult paremale
public class KategooriaRibad : Control
{
    private List<KategooriaMarginaal> read = new();
    private double t = 1;
    public bool Tume { get; set; }   // moodne: tumedal kaardil, värvilised ribad ja marginaali %
    private static readonly Color[] Varvid = { Color.FromArgb(226, 6, 19), Color.FromArgb(160, 35, 110), Color.FromArgb(90, 200, 130), Color.FromArgb(245, 176, 74), Color.FromArgb(235, 120, 175) };

    public KategooriaRibad()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    public void Naita(List<KategooriaMarginaal> uued)
    {
        read = uued;
        Animatsioon.Kaivita(this, 900, x => { t = Animatsioon.Sujuv(x); Invalidate(); });
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var f = new Font("Segoe UI Semibold", 9F);
        using var fs = new Font("Segoe UI", 8F);
        if (read.Count == 0)
        {
            TextRenderer.DrawText(g, Keel.T("Selles perioodis müüki pole."), f, new Point(0, 10), Teema.TekstNork);
            return;
        }
        decimal max = Math.Max(0.01m, read.Max(r => r.Marginaal));
        int y = 4;
        if (Tume) { JoonistaTume(g, max); return; }
        foreach (var r in read)
        {
            if (y + 50 > Height) break;
            TextRenderer.DrawText(g, Keel.T(r.Nimi), f, new Rectangle(0, y, Width - 90, 18), Teema.Tekst, TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, Ostukorv.Eur(r.Marginaal), f, new Rectangle(0, y, Width - 2, 18), Teema.Tekst, TextFormatFlags.Right);
            var rada = new RectangleF(0, y + 22, Width - 2, 10);
            using (var tee = Ymardus.Tee(rada, 5)) using (var b = new SolidBrush(Teema.Paneel2)) g.FillPath(b, tee);
            float w = (float)((double)(r.Marginaal / max) * t) * rada.Width;
            if (w > 2)
                using (var tee = Ymardus.Tee(new RectangleF(rada.X, rada.Y, w, rada.Height), 5))
                using (var b = new SolidBrush(Teema.Teine))
                    g.FillPath(b, tee);
            string all = Keel.F("käive {0}", Ostukorv.Eur(r.Kaive)) + (r.Hinnanguline ? "  ·  " + Keel.T("hinnanguline") : "");
            TextRenderer.DrawText(g, all, fs, new Point(0, y + 34), Teema.TekstNork);
            y += 56;
        }
    }

    private void JoonistaTume(Graphics g, decimal max)
    {
        using var f = new Font("Segoe UI", 9.5F);
        using var fb = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        using var fs = new Font("Segoe UI", 8F);
        int y = 6, i = 0;
        foreach (var r in read)
        {
            if (y + 56 > Height - 50) break;
            string pr = r.Kaive > 0 ? $"{Math.Round(r.Marginaal / r.Kaive * 100)} %" : "";
            TextRenderer.DrawText(g, Keel.T(r.Nimi), f, new Rectangle(0, y, Width - 150, 20), Color.FromArgb(213, 216, 221), TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, Ostukorv.Eur(r.Marginaal), fb, new Rectangle(0, y, Width - 70, 20), Color.White, TextFormatFlags.Right);
            TextRenderer.DrawText(g, pr, fb, new Rectangle(0, y, Width - 2, 20), Color.FromArgb(127, 217, 162), TextFormatFlags.Right);
            var rada = new RectangleF(0, y + 26, Width - 2, 12);
            using (var tee = Ymardus.Tee(rada, 6)) using (var b = new SolidBrush(Color.FromArgb(42, 43, 47))) g.FillPath(b, tee);
            float w = (float)((double)(r.Marginaal / max) * t) * rada.Width;
            if (w > 3)
                using (var tee = Ymardus.Tee(new RectangleF(rada.X, rada.Y, w, rada.Height), 6))
                using (var b = new SolidBrush(Varvid[i % Varvid.Length]))
                    g.FillPath(b, tee);
            y += 58;
            i++;
        }
        TextRenderer.DrawText(g, Keel.T("Marginaal = (müügihind − sisseostuhind) / müügihind. Sisseostuhind lisatakse toote andmetesse."), fs,
            new Rectangle(0, Height - 44, Width - 2, 42), Color.FromArgb(156, 161, 168), TextFormatFlags.WordBreak);
    }
}
