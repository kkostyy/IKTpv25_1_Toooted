using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Müügiprognoos: viimase 14 päeva tegelik käive (täis tulbad) + järgmise 7 päeva prognoos
// (heledad triibulised tulbad). Üleval oodatav nädala käive ja muutus viimase 7 päevaga.
public class PrognoosiGraafik : Control
{
    private decimal[] tegelik = Array.Empty<decimal>(), prognoos = Array.Empty<decimal>();
    private DateTime algus;          // tegeliku[0] kuupäev
    private double t = 1;
    private int hiir = -1;
    private readonly ToolTip vihje = new();

    public PrognoosiGraafik()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) vihje.Dispose();
        base.Dispose(disposing);
    }

    public void Naita(decimal[] tegelik, decimal[] prognoos, DateTime algus)
    {
        this.tegelik = tegelik;
        this.prognoos = prognoos;
        this.algus = algus;
        Animatsioon.Kaivita(this, 900, x => { t = Animatsioon.Sujuv(x); Invalidate(); });
    }

    private int Kokku => tegelik.Length + prognoos.Length;
    private decimal Vaartus(int i) => i < tegelik.Length ? tegelik[i] : prognoos[i - tegelik.Length];
    private const int Yla = 44, All = 22;

    private RectangleF Tulp(int i, decimal max)
    {
        float w = (Width - 4f) / Math.Max(1, Kokku);
        float korgus = Height - Yla - All;
        float h = max > 0 ? (float)(Vaartus(i) / max) * korgus * (float)t : 0;
        return new RectangleF(2 + i * w + w * 0.18f, Yla + korgus - h, w * 0.64f, h);
    }

    private decimal Max => Kokku == 0 ? 0 : Math.Max(1, Enumerable.Range(0, Kokku).Max(Vaartus)) * 1.08m;

    protected override void OnMouseMove(MouseEventArgs e)
    {
        float w = (Width - 4f) / Math.Max(1, Kokku);
        int uus = e.Y > Yla - 10 && Kokku > 0 ? Math.Clamp((int)((e.X - 2) / w), 0, Kokku - 1) : -1;
        if (uus != hiir)
        {
            hiir = uus;
            if (uus >= 0)
            {
                var p = algus.AddDays(uus);
                string kes = uus < tegelik.Length ? Keel.T("tegelik") : Keel.T("prognoos");
                vihje.SetToolTip(this, $"{p:dd.MM} {Keel.T(PaevaNimi(p))} · {kes}: {Ostukorv.Eur(Vaartus(uus))}");
            }
            else vihje.SetToolTip(this, "");
            Invalidate();
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hiir = -1; Invalidate(); base.OnMouseLeave(e); }

    private static readonly string[] PaevaNimed = { "Pühapäev", "Esmaspäev", "Teisipäev", "Kolmapäev", "Neljapäev", "Reede", "Laupäev" };
    private static string PaevaNimi(DateTime p) => PaevaNimed[(int)p.DayOfWeek];

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var f = new Font("Segoe UI", 8F);
        using var fb = new Font("Segoe UI", 15F, FontStyle.Bold);
        using var fm = new Font("Segoe UI Semibold", 9F);

        if (tegelik.Sum() == 0)
        {
            TextRenderer.DrawText(g, Keel.T("Prognoosiks on vaja vähemalt paari nädala müüke."), fm, new Point(0, 8), Teema.TekstNork);
            return;
        }

        // Kokkuvõte üleval
        decimal oodatav = prognoos.Sum();
        decimal viimane7 = tegelik.Skip(Math.Max(0, tegelik.Length - 7)).Sum();
        string pealkiri = Ostukorv.Eur(oodatav);
        TextRenderer.DrawText(g, pealkiri, fb, new Point(0, 0), Teema.Tekst);
        int px = TextRenderer.MeasureText(pealkiri, fb).Width + 6;
        TextRenderer.DrawText(g, Keel.T("oodatav käive järgmise 7 päeva jooksul"), fm, new Point(px, 8), Teema.TekstNork);
        if (Myygianalyys.Muutus(oodatav, viimane7) is decimal m)
            TextRenderer.DrawText(g, $"{(m >= 0 ? "▲" : "▼")} {Math.Abs(m)} %  " + Keel.T("võrreldes viimase 7 päevaga"), fm,
                new Rectangle(0, 8, Width - 2, 18), m >= 0 ? Teema.Edu : Teema.Viga, TextFormatFlags.Right);

        decimal max = Max;
        var tulpVarv = Teema.OnMoodne ? Teema.Teine : Teema.Aktsent;
        var heleVarv = Ymardus.Segu(Teema.Pind, tulpVarv, 0.35f);
        float korgus = Height - Yla - All;

        // Eraldusjoon "täna"
        float w = (Width - 4f) / Math.Max(1, Kokku);
        float jx = 2 + tegelik.Length * w - 1;
        using (var p = new Pen(Teema.Joon) { DashStyle = DashStyle.Dash })
            g.DrawLine(p, jx, Yla - 6, jx, Yla + korgus);
        TextRenderer.DrawText(g, Keel.T("prognoos") + " →", f, new Point((int)jx + 6, Yla - 14), Teema.TekstNork);

        for (int i = 0; i < Kokku; i++)
        {
            var r = Tulp(i, max);
            bool prog = i >= tegelik.Length;
            if (r.Height >= 1)
            {
                using var tee = Ymardus.Tee(r, Math.Min(6, r.Width / 2));
                if (prog)
                {
                    using var hb = new HatchBrush(HatchStyle.WideUpwardDiagonal, Ymardus.Segu(heleVarv, tulpVarv, 0.35f), heleVarv);
                    g.FillPath(hb, tee);
                    using var pen = new Pen(tulpVarv, 1.2f) { DashStyle = DashStyle.Dash };
                    g.DrawPath(pen, tee);
                }
                else
                    using (var b = new SolidBrush(hiir == i ? Ymardus.Segu(tulpVarv, Teema.Tekst, 0.25f) : tulpVarv)) g.FillPath(b, tee);
            }
            var paev = algus.AddDays(i);
            bool tana = paev.Date == DateTime.Today;
            string silt = Keel.T(PaevaNimi(paev))[..2];
            TextRenderer.DrawText(g, silt, tana ? fm : f, new Rectangle((int)(2 + i * w), (int)(Yla + korgus + 4), (int)w, 16),
                tana ? Teema.Tekst : Teema.TekstNork, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
        }
    }
}

// Tooted, mis müügitempo järgi peagi otsa saavad
public class LopevadNimekiri : Control
{
    private List<Myygianalyys.Lopeb> read = new();

    public LopevadNimekiri()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    public void Naita(List<Myygianalyys.Lopeb> uued)
    {
        read = uued;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var f = new Font("Segoe UI Semibold", 9F);
        using var fs = new Font("Segoe UI", 8F);
        using var fb = new Font("Segoe UI", 9F, FontStyle.Bold);
        if (read.Count == 0)
        {
            TextRenderer.DrawText(g, Keel.T("Järgmise 2 nädala jooksul ei lõpe ükski toode."), f, new Rectangle(0, 6, Width, 40), Teema.Edu, TextFormatFlags.WordBreak);
            return;
        }
        int y = 2;
        foreach (var r in read)
        {
            if (y + 40 > Height) break;
            var varv = r.PaeviJaanud < 3 ? Teema.Viga : r.PaeviJaanud < 7 ? Teema.Hoiatus : Teema.TekstNork;
            string paevi = r.Toode.Kogus <= 0 ? Keel.T("otsas") : Keel.F("~{0} päeva", r.PaeviJaanud.ToString("0.#"));
            int pw = TextRenderer.MeasureText(paevi, fb).Width + 18;
            var kiip = new RectangleF(Width - pw - 2, y + 4, pw, 22);
            using (var tee = Ymardus.Tee(kiip, 11)) using (var b = new SolidBrush(Ymardus.Segu(Teema.Pind, varv, 0.16f))) g.FillPath(b, tee);
            TextRenderer.DrawText(g, paevi, fb, Rectangle.Round(kiip), varv, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(g, r.Toode.Toodenimetus, f, new Rectangle(0, y, Width - pw - 12, 18), Teema.Tekst, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, Keel.F("laos {0} tk · müüakse {1} tk päevas", r.Toode.Kogus, r.PaevasTk.ToString("0.#")), fs,
                new Rectangle(0, y + 18, Width - pw - 12, 16), Teema.TekstNork, TextFormatFlags.EndEllipsis);
            y += 42;
        }
    }
}
