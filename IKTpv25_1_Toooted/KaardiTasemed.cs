using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Kaardi tasemed (lõuend N11): suur kaart valitud taseme kujundusega,
//  eelised, 12 kuu ostude riba (Pronks → Hõbe → Kuld); paremal tume
//  veerg kõigi tasemete minikaartidega (klõps = eelvaade).
// ===================================================================
public class KaardiTasemedVorm : Form, ITagasi
{
    private readonly TaseInfo tase;
    private Tase vaade;
    private readonly TasemeKaart kaart;
    private readonly Label lblNimi;
    private readonly FlowLayoutPanel eelised;
    private readonly List<TasemeMini> minid = new();

    public static readonly Dictionary<Tase, string[]> Eelised = new()
    {
        [Tase.Pronks] = new[] { "1 % boonust igalt eurolt", "Nädala väljakutsed", "Päeva üllatus" },
        [Tase.Hobe] = new[] { "1,5 % boonust", "Läikiv hõbekaart", "Nädala väljakutsed ja päeva üllatus" },
        [Tase.Kuld] = new[] { "2 % boonust", "Hologramm-kaart sädelusega", "Kõik preemiad" }
    };

    public KaardiTasemedVorm()
    {
        Text = Keel.T("Kaardi tasemed");
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = MinimizeBox = false;
        ShowInTaskbar = false;
        KeyPreview = true;
        Font = UiStiil.Põhifont;
        BackColor = Color.FromArgb(233, 237, 239);
        ClientSize = new Size(1120, 640);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        tase = Preemiad.TaseSummast(0);
        try
        {
            using var db = new ToodedContext();
            if (AktiivneKasutaja.KlientId is int kid) tase = Preemiad.LeiaTase(db, kid);
        }
        catch { }
        vaade = tase.Tase;
        Color valge = Color.White, tume = Color.FromArgb(27, 28, 31);

        var vasak = new RoundedPanel { Bounds = new Rectangle(16, 16, 772, 608), Radius = 16, FillColor = valge, BorderColor = valge, BackColor = BackColor };
        var tagasi = new TagasiNupp { Location = new Point(28, 24) };
        tagasi.Click += (s, e) => Close();
        vasak.Controls.Add(tagasi);
        vasak.Controls.Add(new Label { Text = Keel.T("Kaardi tasemed"), Location = new Point(tagasi.Right + 14, 20), AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = tume, BackColor = valge });
        vasak.Controls.Add(new Label { Text = Keel.T("Mida rohkem ostad, seda suurem boonus ja ilusam kaart"), Location = new Point(tagasi.Right + 16, 52), AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(100, 106, 115), BackColor = valge });

        var ala = new RoundedPanel { Bounds = new Rectangle(28, 92, 716, 360), Radius = 13, FillColor = Color.FromArgb(246, 248, 249), BorderColor = Color.FromArgb(246, 248, 249), BackColor = valge };
        kaart = new TasemeKaart { Bounds = new Rectangle(24, 30, 440, 300) };
        ala.Controls.Add(kaart);
        lblNimi = new Label { Location = new Point(488, 70), AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = tume, BackColor = ala.FillColor };
        ala.Controls.Add(lblNimi);
        eelised = new FlowLayoutPanel { Location = new Point(488, 110), Size = new Size(220, 200), FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = ala.FillColor };
        ala.Controls.Add(eelised);
        vasak.Controls.Add(ala);

        var et = new System.Globalization.CultureInfo("et-EE");
        vasak.Controls.Add(new Label { Text = Keel.T("Ostud viimase 12 kuu jooksul"), Location = new Point(28, 476), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = tume, BackColor = valge });
        var summa = new Label { Text = Ostukorv.Eur(tase.Summa), AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(160, 35, 110), BackColor = valge };
        vasak.Controls.Add(summa);
        summa.Location = new Point(744 - summa.PreferredWidth, 468);
        vasak.Controls.Add(new TasemeRiba(tase.Summa) { Bounds = new Rectangle(28, 512, 716, 66) });

        // ---- Paremal: kõik tasemed ----
        var parem = new RoundedPanel { Bounds = new Rectangle(804, 16, 300, 608), Radius = 16, FillColor = tume, BorderColor = tume, BackColor = BackColor };
        parem.Controls.Add(new Label { Text = Keel.T("KÕIK TASEMED"), Location = new Point(22, 24), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Color.FromArgb(235, 120, 175), BackColor = tume });
        for (int i = 0; i < Preemiad.Tasemed.Length; i++)
        {
            var (t, alates, pr) = Preemiad.Tasemed[i];
            var m = new TasemeMini(t, $"{Keel.F("alates {0}", Ostukorv.Eur(alates))} · {(pr * 100).ToString("0.#", et)} %", t <= tase.Tase) { Bounds = new Rectangle(14, 56 + i * 96, 272, 86) };
            m.Click += (s, e) => Vali(m.Tase);
            minid.Add(m);
            parem.Controls.Add(m);
        }
        parem.Controls.Add(new Label { Text = Keel.T("Tase arvutatakse viimase 12 kuu ostudest. Kõrgem tase = rohkem boonust igalt eurolt."), Location = new Point(22, 520), Size = new Size(256, 60), Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(156, 161, 168), BackColor = tume });

        Controls.Add(vasak);
        Controls.Add(parem);
        Vali(vaade);
    }

    private void Vali(Tase t)
    {
        vaade = t;
        kaart.Naita(t);
        lblNimi.Text = Keel.T(Preemiad.TaseNimi(t));
        eelised.Controls.Clear();
        int i = 0;
        foreach (var e in Eelised[t])
        {
            var rida = new EeliseRida(Keel.T(e), i++ * 90) { Margin = new Padding(0, 0, 0, 8) };
            eelised.Controls.Add(rida);
        }
        foreach (var m in minid) m.Valitud = m.Tase == t;
    }

    public bool MineTagasi() { Close(); return true; }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape || (e.Alt && e.KeyCode == Keys.Left)) { Close(); e.Handled = true; }
        base.OnKeyDown(e);
    }

    public static void Ava(IWin32Window? omanik)
    {
        using var v = new KaardiTasemedVorm();
        if (omanik != null) v.ShowDialog(omanik); else { v.StartPosition = FormStartPosition.CenterScreen; v.ShowDialog(); }
    }
}

// Suur kaart valitud taseme kujundusega; vahetus = kaart "hüppab" sisse
public class TasemeKaart : Control
{
    private Tase tase;
    private double aeg, hypp = 1;
    private float hiir = 0.5f;

    public TasemeKaart()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Animatsioon.Tsykkel(this, s => { aeg = s; Invalidate(); }, 40);
    }

    public void Naita(Tase t)
    {
        tase = t;
        Animatsioon.Kaivita(this, 420, x => hypp = x);
    }

    protected override void OnMouseMove(MouseEventArgs e) { hiir = e.X / (float)Math.Max(1, Width); base.OnMouseMove(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Color.White);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        float k = (float)(0.9 + 0.1 * Animatsioon.Pop(hypp));
        float w = 420 * k, h = 265 * k;
        var r = new RectangleF((Width - w) / 2, (Height - h) / 2 - 6, w, h);
        for (int i = 0; i < 8; i++)
            using (var vari = new SolidBrush(Color.FromArgb(10 + (int)tase * 3, 0, 0, 0)))
            using (var vt = Ymardus.Tee(RectangleF.Inflate(new RectangleF(r.X, r.Y + 16, r.Width, r.Height), 10 - i, 8 - i), 22))
                g.FillPath(vari, vt);
        if (tase == Tase.Kuld)
            using (var tee = Ymardus.Tee(RectangleF.Inflate(r, 4, 4), 22)) using (var p = new Pen(Color.FromArgb(232, 185, 49), 2)) g.DrawPath(p, tee);
        TaseKaart.JoonistaTaust(g, r, tase, aeg, hiir);
        Color tint = tase == Tase.Pronks ? Color.FromArgb(255, 244, 234) : tase == Tase.Hobe ? Color.FromArgb(27, 28, 31) : Color.FromArgb(58, 38, 0);
        using var fh = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        TextRenderer.DrawText(g, "A K T      P O O D      K L I E N T", fh, Point.Round(new PointF(r.X + 24, r.Y + 24)), tint);
        string nimi = Keel.T(Preemiad.TaseNimi(tase)).ToUpper();
        int bw = TextRenderer.MeasureText(nimi, fh).Width + 22;
        var badge = new RectangleF(r.Right - 24 - bw, r.Y + 20, bw, 26);
        Color bc = tase == Tase.Pronks ? Color.FromArgb(51, 255, 255, 255) : tase == Tase.Hobe ? Color.FromArgb(27, 28, 31) : Color.FromArgb(226, 6, 19);
        using (var tee = Ymardus.Tee(badge, 13)) using (var b = new SolidBrush(bc)) g.FillPath(b, tee);
        TextRenderer.DrawText(g, nimi, fh, Rectangle.Round(badge), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        var kiip = new RectangleF(r.X + 24, r.Y + 66, 44, 34);
        using (var tee = Ymardus.Tee(kiip, 7)) using (var p = new Pen(Color.FromArgb(120, tint))) g.DrawPath(p, tee);
        using var fn = new Font("Segoe UI", 9F, FontStyle.Bold);
        using var fnr = new Font("Segoe UI", 15F);
        using var fb = new Font("Segoe UI", 20F, FontStyle.Bold);
        using var fs = new Font("Segoe UI", 7F, FontStyle.Bold);
        TextRenderer.DrawText(g, AktiivneKasutaja.Kuvanimi.ToUpper(), fn, Point.Round(new PointF(r.X + 24, r.Bottom - 70)), tint);
        TextRenderer.DrawText(g, string.Join(" ", (AktiivneKasutaja.Kaardinumber ?? "").ToCharArray()), fnr, Point.Round(new PointF(r.X + 22, r.Bottom - 50)), tint);
        var pr = Preemiad.Tasemed[(int)tase].Protsent;
        string p2 = (pr * 100).ToString("0.#", new System.Globalization.CultureInfo("et-EE")) + " %";
        int pw = TextRenderer.MeasureText(p2, fb).Width;
        TextRenderer.DrawText(g, Keel.T("BOONUS"), fs, new Rectangle((int)r.Right - 144, (int)r.Bottom - 70, 120, 14), tint, TextFormatFlags.Right);
        TextRenderer.DrawText(g, p2, fb, new Point((int)r.Right - 20 - pw, (int)r.Bottom - 56), tint);
    }
}

// Eelise rida: roheline ring linnukesega, ilmub viivitusega
public class EeliseRida : Control
{
    private readonly string tekst;
    private double t;

    public EeliseRida(string tekst, int viivitus)
    {
        this.tekst = tekst;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Size = new Size(220, 30);
        Font = new Font("Segoe UI", 9.5F);
        Animatsioon.Kaivita(this, 420, x => { t = x; Invalidate(); }, null, viivitus);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Color.White);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        float s = (float)Animatsioon.Sujuv(t), dy = (1 - s) * 10;
        var ring = new RectangleF(0, 3 + dy, 24, 24);
        using (var b = new SolidBrush(Color.FromArgb((int)(255 * s), 230, 244, 236))) g.FillEllipse(b, ring);
        using (var p = new Pen(Color.FromArgb((int)(255 * s), 20, 130, 70), 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            g.DrawLines(p, new[] { new PointF(ring.X + 7, ring.Y + 12), new PointF(ring.X + 10.5f, ring.Y + 15.5f), new PointF(ring.X + 17, ring.Y + 8.5f) });
        TextRenderer.DrawText(g, tekst, Font, new Rectangle(34, (int)dy, Width - 34, Height), Color.FromArgb((int)(255 * s), 27, 28, 31), TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

// 12 kuu ostude riba: vask → hõbe → kuld, valge marker ploomi rõngaga
public class TasemeRiba : Control
{
    private readonly decimal summa;
    private double t;
    private const float Max = 400f;

    public TasemeRiba(decimal summa)
    {
        this.summa = summa;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Animatsioon.Kaivita(this, 1200, x => { t = Animatsioon.Sujuv(x); Invalidate(); });
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Color.White);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        float w = Width - 2;
        var riba = new RectangleF(0, 10, w, 14);
        using (var tee = Ymardus.Tee(riba, 7))
        {
            var olek = g.Save();
            g.SetClip(tee);
            float x1 = w * 100 / Max, x2 = w * 300 / Max;
            using (var b = new SolidBrush(Color.FromArgb(192, 132, 87))) g.FillRectangle(b, 0, riba.Y, x1, riba.Height);
            using (var b = new SolidBrush(Color.FromArgb(201, 205, 211))) g.FillRectangle(b, x1, riba.Y, x2 - x1, riba.Height);
            using (var b = new SolidBrush(Color.FromArgb(232, 185, 49))) g.FillRectangle(b, x2, riba.Y, w - x2, riba.Height);
            g.Restore(olek);
        }
        float mx = (float)(Math.Min(1.0, (double)summa / Max) * t) * w;
        mx = Math.Clamp(mx, 13, w - 13);
        using (var b = new SolidBrush(Color.FromArgb(160, 35, 110))) g.FillEllipse(b, mx - 17, 0, 34, 34);
        using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, mx - 13, 4, 26, 26);
        using var f = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        Color c = Color.FromArgb(100, 106, 115);
        TextRenderer.DrawText(g, "0 € · " + Keel.T("Pronks"), f, new Point(0, 42), c);
        TextRenderer.DrawText(g, "100 € · " + Keel.T("Hõbe"), f, new Point((int)(w * 100 / Max), 42), c);
        TextRenderer.DrawText(g, "300 € · " + Keel.T("Kuld"), f, new Point((int)(w * 300 / Max), 42), c);
    }
}

// Minikaart tumedas veerus: väike taseme kaart + nimi; valitud = valge rõngas
public class TasemeMini : Control
{
    public Tase Tase { get; }
    private readonly string info;
    private readonly bool kattesaadav;
    private bool valitud, hover;
    private double aeg;
    public bool Valitud { get => valitud; set { valitud = value; Invalidate(); } }

    public TasemeMini(Tase tase, string info, bool kattesaadav)
    {
        Tase = tase;
        this.info = info;
        this.kattesaadav = kattesaadav;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Cursor = Cursors.Hand;
        Animatsioon.Tsykkel(this, s => { aeg = s; if (Tase != Tase.Pronks) Invalidate(); }, 60);
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var tume = Color.FromArgb(27, 28, 31);
        g.Clear(Parent?.BackColor ?? tume);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = new RectangleF(1, 1, Width - 3, Height - 3);
        if (valitud || hover)
            using (var tee = Ymardus.Tee(r, 22))
            {
                using (var b = new SolidBrush(Color.FromArgb(42, 43, 47))) g.FillPath(b, tee);
                if (valitud) using (var p = new Pen(Color.White, 2)) g.DrawPath(p, tee);
            }
        var mini = new RectangleF(12, (Height - 60) / 2f, 96, 60);
        TaseKaart.JoonistaTaust(g, mini, Tase, aeg, 0.5f);
        int alfa = kattesaadav || valitud ? 255 : 140;
        using var fb = new Font("Segoe UI", 11F, FontStyle.Bold);
        using var f = new Font("Segoe UI", 8.5F);
        TextRenderer.DrawText(g, Keel.T(Preemiad.TaseNimi(Tase)), fb, new Point(122, 22), Color.FromArgb(alfa, Color.White));
        TextRenderer.DrawText(g, info, f, new Point(122, 46), Color.FromArgb(alfa * 3 / 4, Color.White));
    }
}
