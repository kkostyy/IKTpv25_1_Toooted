using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Moodsa disaini (Ctrl+Shift+M) ühised osad: tume brändipaneel
//  hõljuvate kaartidega, keelevalik "pillina", noolega peanupp.
// ===================================================================

// Tume paneel (raadius 32): AKT logo, kaks hõljuvat kliendikaarti, "Tere tulemast!"
public class MoodneBrand : Control
{
    private double aeg;
    private readonly string pealkiri, alapealkiri;

    public MoodneBrand(string pealkiri, string alapealkiri)
    {
        this.pealkiri = pealkiri;
        this.alapealkiri = alapealkiri;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Animatsioon.Tsykkel(this, s => { aeg = s; Invalidate(); }, 40);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = new RectangleF(0, 0, Width - 1, Height - 1);
        using (var tee = Ymardus.Tee(r, 32))
        using (var b = new SolidBrush(Color.FromArgb(27, 28, 31)))
            g.FillPath(b, tee);

        // logo: punane ring kotiga + "AKT"
        var ring = new RectangleF(32, 32, 44, 44);
        using (var b = new SolidBrush(Color.FromArgb(226, 6, 19))) g.FillEllipse(b, ring);
        using (var p = new Pen(Color.White, 2.2f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
        {
            g.DrawPolygon(p, new[] { new PointF(ring.X + 13, ring.Y + 17), new PointF(ring.X + 31, ring.Y + 17), new PointF(ring.X + 30, ring.Y + 33), new PointF(ring.X + 14, ring.Y + 33) });
            g.DrawArc(p, ring.X + 17, ring.Y + 10, 10, 12, 180, 180);
        }
        using (var f = new Font("Segoe UI", 13.5F, FontStyle.Bold))
            TextRenderer.DrawText(g, "A K T", f, new Point(86, 42), Color.White);

        // hõljuvad kaardid (aeglane kõikumine) + joon, mis "joonistub" ise
        double joon = Math.Clamp(aeg / 2.4, 0, 1);
        Kaart(g, 110, 40 + 120, 9 - 3 * Math.Sin(aeg * 0.9), (float)(Math.Sin(aeg * 0.8) * 10),
            Color.FromArgb(160, 35, 110), Color.FromArgb(110, 20, 80), joon, false);
        Kaart(g, 30, 120 + 120, -12 + 4 * Math.Sin(aeg * 0.75), (float)(Math.Sin(aeg * 0.9 + 1) * -14),
            Color.FromArgb(226, 6, 19), Color.FromArgb(226, 6, 19), joon, true);

        using (var f = new Font("Segoe UI", 25F, FontStyle.Bold))
            TextRenderer.DrawText(g, Keel.T(pealkiri), f, new Point(28, Height - 132), Color.White);
        using (var f = new Font("Segoe UI", 10.5F))
            TextRenderer.DrawText(g, Keel.T(alapealkiri), f, new Rectangle(32, Height - 80, Width - 64, 50), Color.FromArgb(185, 189, 196),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
    }

    private static void Kaart(Graphics g, float x, float y, double nurk, float nihe, Color a, Color b, double joon, bool tekstiga)
    {
        var olek = g.Save();
        g.TranslateTransform(x + 125, y + 79 + nihe);
        g.RotateTransform((float)nurk);
        var r = new RectangleF(-125, -79, 250, 158);
        for (int i = 0; i < 6; i++)
            using (var vari = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
            using (var vt = Ymardus.Tee(RectangleF.Inflate(new RectangleF(r.X, r.Y + 14, r.Width, r.Height), 8 - i, 6 - i), 16))
                g.FillPath(vari, vt);
        using (var tee = Ymardus.Tee(r, 16))
        using (var br = new LinearGradientBrush(r, a, b, 60f))
        {
            g.FillPath(br, tee);
            g.SetClip(tee);
        }
        using (var p = new Pen(Color.FromArgb(170, 255, 255, 255), 1.2f))
        {
            var algus = new PointF(r.X + 85, r.Bottom + 20);
            var lopp = new PointF(r.Right + 20, r.Y + 25);
            g.DrawLine(p, algus, new PointF(algus.X + (lopp.X - algus.X) * (float)joon, algus.Y + (lopp.Y - algus.Y) * (float)joon));
        }
        if (tekstiga)
        {
            using var f = new Font("Segoe UI", 7F, FontStyle.Bold);
            TextRenderer.DrawText(g, "AKT     POOD     KLIENT", f, Point.Round(new PointF(r.X + 16, r.Y + 14)), Color.White);
            using var fn = new Font("Segoe UI", 10F);
            TextRenderer.DrawText(g, "1 0 0 0 0 1", fn, Point.Round(new PointF(r.X + 16, r.Bottom - 30)), Color.White);
            using var fb = new Font("Segoe UI", 12F, FontStyle.Bold);
            TextRenderer.DrawText(g, "0,42 €", fb, Point.Round(new PointF(r.Right - 72, r.Bottom - 32)), Color.White);
        }
        g.Restore(olek);
    }
}

// Keelevalik: hall "pill", valitud keel tume (ET / RU / EN)
public class KeelePill : Control
{
    private static readonly (KeeleKood Kood, string Tekst)[] Keeled = { (KeeleKood.ET, "ET"), (KeeleKood.RU, "RU"), (KeeleKood.EN, "EN") };
    private int hiir = -1;

    public KeelePill()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Size = new Size(4 + 3 * 46, 38);
        Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        Cursor = Cursors.Hand;
    }

    private Rectangle Osa(int i) => new(4 + i * 46, 4, 44, 30);

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int uus = Enumerable.Range(0, 3).FirstOrDefault(i => Osa(i).Contains(e.Location), -1);
        if (uus != hiir) { hiir = uus; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hiir = -1; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        for (int i = 0; i < 3; i++)
            if (Osa(i).Contains(e.Location)) { Keel.Sea(Keeled[i].Kood); Invalidate(); }
        base.OnMouseClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), (Height - 1) / 2f))
        using (var b = new SolidBrush(Teema.Paneel2))
            g.FillPath(b, tee);
        for (int i = 0; i < 3; i++)
        {
            var r = Osa(i);
            bool valitud = Keel.Praegu == Keeled[i].Kood;
            if (valitud || hiir == i)
                using (var tee = Ymardus.Tee(r, r.Height / 2))
                using (var b = new SolidBrush(valitud ? Teema.Tekst : Teema.Joon))
                    g.FillPath(b, tee);
            TextRenderer.DrawText(g, Keeled[i].Tekst, Font, r, valitud ? Teema.Pind : Teema.Tekst, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}

public static class MoodneUi
{
    // Lihtne "voolav" paigutus: elemendid reas vasakult paremale, mis ei mahu, läheb uuele reale
    public static void Voog(Control konteiner, int serv, int vahe)
    {
        int x = serv, y = 4, rea = 0, laius = konteiner.ClientSize.Width - serv;
        foreach (Control c in konteiner.Controls)
        {
            if (!c.Visible) continue;
            if (x > serv && x + c.Width > laius) { x = serv; y += rea + vahe; rea = 0; }
            c.Location = new Point(x, y);
            x += c.Width + vahe;
            rea = Math.Max(rea, c.Height);
        }
    }

    // Suur "pill" sisestusväli (kõrgus 52, hall taust, fookuses ploomi raam)
    public static RoundedPanel Sisend(out TextBox tekst, int laius, bool parool = false, string vihje = "")
    {
        var raam = new RoundedPanel
        {
            Size = new Size(laius, 52),
            Radius = 13,
            Padding = new Padding(20, 15, 20, 0),
            FillColor = Teema.Paneel2,
            BorderColor = Teema.Paneel2,
            BackColor = Teema.Pind
        };
        var t = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            UseSystemPasswordChar = parool,
            PlaceholderText = vihje,
            BackColor = Teema.Paneel2,
            ForeColor = Teema.Tekst,
            Font = new Font("Segoe UI", 11.5F)
        };
        raam.Controls.Add(t);
        t.Enter += (s, e) => { raam.FillColor = Teema.Pind; raam.BorderColor = Teema.Teine; t.BackColor = Teema.Pind; raam.Invalidate(); };
        t.Leave += (s, e) => { raam.FillColor = Teema.Paneel2; raam.BorderColor = Teema.Paneel2; t.BackColor = Teema.Paneel2; raam.Invalidate(); };
        raam.Click += (s, e) => t.Focus();
        tekst = t;
        return raam;
    }

    // Punane "pill" nupp, paremal valge ring noolega (nool liigub hiire all paremale)
    public static RoundedButton NoolNupp(string tekst, EventHandler kasitleja)
    {
        var nupp = UiStiil.LooSuurNupp(tekst, kasitleja);
        nupp.Height = 56;
        nupp.TextAlign = ContentAlignment.MiddleLeft;
        nupp.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        NoolStiil(nupp);
        return nupp;
    }

    // Olemasolevale nupule valge ring noolega paremale (nool liigub hiire all)
    public static void NoolStiil(RoundedButton nupp)
    {
        float nool = 0;
        System.Windows.Forms.Timer? anim = null;
        void Liigu(float siht)
        {
            anim?.Stop();
            float algus = nool;
            anim = Animatsioon.Kaivita(nupp, 250, t => { nool = algus + (siht - algus) * (float)Animatsioon.Sujuv(t); nupp.Invalidate(); });
        }
        nupp.MouseEnter += (s, e) => Liigu(1);
        nupp.MouseLeave += (s, e) => Liigu(0);
        nupp.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int d = nupp.Height - 18;
            var ring = new Rectangle(nupp.Width - d - 9, (nupp.Height - d) / 2, d, d);
            using (var b = new SolidBrush(nupp.Enabled ? Color.White : Teema.Joon)) g.FillEllipse(b, ring);
            Ikoonid.Joonista(g, Ikoonid.NoolParemale, new Rectangle(ring.X + (int)(nool * 4), ring.Y, ring.Width, ring.Height),
                nupp.Enabled ? Teema.Aktsent : Teema.TekstNork, 11f);
        };
    }
}

// Moodsa disaini üldised "kosmeetika" abid (kõik dialoogid)
public static class MoodneStiil
{
    // CheckBox → lüliti, RadioButton → "pill"; rekursiivselt kõigile elementidele
    public static void Rakenda(Control juur)
    {
        foreach (Control c in juur.Controls)
        {
            if (c is CheckBox cb && cb.Tag as string != "moodne") Lyliti(cb);
            else if (c is ComboBox co && co.Tag as string != "moodne") { co.Tag = "moodne"; co.FlatStyle = FlatStyle.Flat; co.BackColor = Teema.Paneel2; }
            else if (c is RadioButton rb && rb.Tag as string != "moodne") Pill(rb);
            Rakenda(c);
        }
    }

    private static void Lyliti(CheckBox cb)
    {
        cb.Tag = "moodne";
        cb.AutoSize = false;
        cb.Height = Math.Max(cb.Height, 30);
        cb.Width = TextRenderer.MeasureText(cb.Text, cb.Font).Width + 60;
        cb.Cursor = Cursors.Hand;
        double asend = cb.Checked ? 1 : 0;
        cb.CheckedChanged += (s, e) =>
        {
            double algus = asend, siht = cb.Checked ? 1 : 0;
            Animatsioon.Kaivita(cb, 200, t => { asend = algus + (siht - algus) * Animatsioon.Sujuv(t); cb.Invalidate(); });
        };
        cb.TextChanged += (s, e) => cb.Width = TextRenderer.MeasureText(Keel.T(cb.Text), cb.Font).Width + 60;
        cb.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(cb.Parent?.BackColor ?? Teema.Pind);
            var rada = new RectangleF(1, (cb.Height - 22) / 2f, 40, 22);
            Color c = Ymardus.Segu(Teema.Raam, Teema.Edu, (float)asend);
            if (!cb.Enabled) c = Teema.Joon;
            using (var tee = Ymardus.Tee(rada, 11)) using (var b = new SolidBrush(c)) g.FillPath(b, tee);
            float x = rada.X + 2 + (float)(asend * 18);
            using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, x, rada.Y + 2, 18, 18);
            TextRenderer.DrawText(g, cb.Text, cb.Font, new Rectangle(50, 0, cb.Width - 50, cb.Height), cb.Enabled ? Teema.Tekst : Teema.TekstNork,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        };
    }

    private static void Pill(RadioButton rb)
    {
        rb.Tag = "moodne";
        rb.AutoSize = false;
        rb.Height = 38;
        rb.Width = TextRenderer.MeasureText(rb.Text, rb.Font).Width + 52;
        rb.Cursor = Cursors.Hand;
        rb.TextChanged += (s, e) => rb.Width = TextRenderer.MeasureText(Keel.T(rb.Text), rb.Font).Width + 52;
        rb.CheckedChanged += (s, e) => rb.Invalidate();
        rb.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(rb.Parent?.BackColor ?? Teema.Pind);
            var r = new RectangleF(0.5f, 0.5f, rb.Width - 2, rb.Height - 2);
            using (var tee = Ymardus.Tee(r, r.Height / 2))
            using (var b = new SolidBrush(rb.Checked ? Ymardus.Segu(Teema.Pind, Teema.Teine, 0.1f) : Teema.Paneel2))
            {
                g.FillPath(b, tee);
                if (rb.Checked) using (var p = new Pen(Teema.Teine, 2)) g.DrawPath(p, tee);
            }
            var ring = new RectangleF(14, (rb.Height - 16) / 2f, 16, 16);
            using (var p = new Pen(rb.Checked ? Teema.Teine : Teema.Raam, 2)) g.DrawEllipse(p, ring);
            if (rb.Checked) using (var b = new SolidBrush(Teema.Teine)) g.FillEllipse(b, RectangleF.Inflate(ring, -4, -4));
            TextRenderer.DrawText(g, rb.Text, rb.Font, new Rectangle(38, 0, rb.Width - 40, rb.Height), rb.Enabled ? Teema.Tekst : Teema.TekstNork,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        };
    }

    // Vormi dokitud osad valgele ümarale kaardile hallil taustal (lõuendi "bento" stiil)
    public static RoundedPanel KaardiSisse(Form vorm, params Control[] osad)
    {
        vorm.BackColor = Teema.Taust;
        vorm.Padding = new Padding(16);
        var kaart = new RoundedPanel { Dock = DockStyle.Fill, Radius = 16, FillColor = Teema.Pind, BorderColor = Teema.Pind, BackColor = Teema.Taust, Padding = new Padding(12, 14, 12, 14) };
        foreach (var o in osad.Reverse())
        {
            vorm.Controls.Remove(o);
            if (o.BackColor == Teema.Taust) o.BackColor = Teema.Pind;
            foreach (Control l in o.Controls) if (l.BackColor == Teema.Taust) l.BackColor = Teema.Pind;
            kaart.Controls.Add(o);
        }
        // õige dokkimise järjekord: Fill viimasena
        foreach (var o in osad.Where(o => o.Dock == DockStyle.Fill)) o.BringToFront();
        vorm.Controls.Add(kaart);
        kaart.BringToFront();
        return kaart;
    }
}

// Filtri vahelehed valgel "pillil": valitud = tume, kõrval arvumärk (valitud = punane)
public class MoodneFilter : Control
{
    private readonly List<(string Nimi, int Arv)> osad = new();
    private readonly List<RectangleF> kohad = new();
    private int valitud, hover = -1;
    public event Action<int>? Valitud;

    public MoodneFilter()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        Height = 48;
        Font = new Font("Segoe UI", 9.5F);
        Cursor = Cursors.Hand;
    }

    public void Sea(IEnumerable<(string Nimi, int Arv)> uued, int valik)
    {
        osad.Clear();
        osad.AddRange(uued);
        valitud = valik;
        kohad.Clear();
        using var fb = new Font(Font, FontStyle.Bold);
        float x = 4;
        foreach (var (nimi, arv) in osad)
        {
            float w = TextRenderer.MeasureText(Keel.T(nimi), fb).Width + 28 + (arv >= 0 ? TextRenderer.MeasureText(arv.ToString(), fb).Width + 18 : 0);
            kohad.Add(new RectangleF(x, 4, w, Height - 8));
            x += w + 4;
        }
        Width = (int)x + 4;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int h = kohad.FindIndex(r => r.Contains(e.Location));
        if (h != hover) { hover = h; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hover = -1; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        int i = kohad.FindIndex(r => r.Contains(e.Location));
        if (i >= 0) { valitud = i; Invalidate(); Valitud?.Invoke(i); }
        base.OnMouseClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), (Height - 1) / 2f))
        using (var b = new SolidBrush(Teema.Pind))
            g.FillPath(b, tee);
        using var fb = new Font(Font, FontStyle.Bold);
        for (int i = 0; i < osad.Count; i++)
        {
            var r = kohad[i];
            bool v = i == valitud;
            if (v || i == hover)
                using (var tee = Ymardus.Tee(r, r.Height / 2))
                using (var b = new SolidBrush(v ? Teema.Tekst : Teema.Paneel2))
                    g.FillPath(b, tee);
            var f = v ? fb : Font;
            string t = Keel.T(osad[i].Nimi);
            int tw = TextRenderer.MeasureText(t, fb).Width;
            TextRenderer.DrawText(g, t, f, new Rectangle((int)r.X + 14, (int)r.Y, tw + 4, (int)r.Height), v ? Teema.Pind : Teema.Tekst, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            if (osad[i].Arv < 0) continue;
            string a = osad[i].Arv.ToString();
            int aw = Math.Max(20, TextRenderer.MeasureText(a, fb).Width + 8);
            var m = new RectangleF(r.X + 14 + tw + 6, r.Y + (r.Height - 20) / 2, aw, 20);
            using (var tee = Ymardus.Tee(m, 10))
            using (var b = new SolidBrush(v ? Color.FromArgb(226, 6, 19) : Teema.Paneel2))
                g.FillPath(b, tee);
            using var fa = new Font("Segoe UI", 8F, FontStyle.Bold);
            TextRenderer.DrawText(g, a, fa, Rectangle.Round(m), v ? Color.White : Teema.Tekst2, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}

// Moodne hüpikmenüü: valge ümar kast, read on "pillid" (hiire all hall), valitud valik = ploomi täpp
public class MoodneMenyyJoonistaja : ToolStripProfessionalRenderer
{
    public static void Rakenda(ContextMenuStrip m)
    {
        m.Renderer = new MoodneMenyyJoonistaja();
        m.ShowImageMargin = false;
        m.ShowCheckMargin = m.Items.OfType<ToolStripMenuItem>().Any(i => i.Checked);
        m.Padding = new Padding(6, 8, 6, 8);
        m.BackColor = Teema.Pind;
        m.Font = new Font("Segoe UI", 10F);
        m.DropShadowEnabled = true;
        foreach (ToolStripItem i in m.Items)
        {
            i.ForeColor = Teema.Tekst;
            if (i is ToolStripMenuItem mi) { mi.Padding = new Padding(6, 7, 14, 7); mi.AutoSize = true; }
        }
        // ümarad nurgad
        m.Opened += (s, e) => { using var tee = Ymardus.Tee(new RectangleF(0, 0, m.Width, m.Height), 16); m.Region = new Region(tee); };
    }

    public MoodneMenyyJoonistaja() : base(new ProfessionalColorTable()) { RoundedEdges = false; }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        using var b = new SolidBrush(Teema.Pind);
        e.Graphics.FillRectangle(b, e.AffectedBounds);
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var tee = Ymardus.Tee(new RectangleF(0.5f, 0.5f, e.ToolStrip.Width - 1.5f, e.ToolStrip.Height - 1.5f), 16);
        using var p = new Pen(Teema.Joon);
        g.DrawPath(p, tee);
    }

    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) { }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (!e.Item.Selected || !e.Item.Enabled) return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = new RectangleF(2, 1, e.Item.Width - 4, e.Item.Height - 2);
        using var tee = Ymardus.Tee(r, r.Height / 2);
        using var b = new SolidBrush(Teema.Paneel2);
        g.FillPath(b, tee);
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Enabled ? Teema.Tekst : Teema.TekstNork;
        e.TextFont = ((ToolStripMenuItem)e.Item).Checked ? new Font(e.TextFont ?? e.Item.Font, FontStyle.Bold) : e.TextFont;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = e.ImageRectangle;
        using var b = new SolidBrush(Teema.Teine);
        g.FillEllipse(b, r.X + r.Width / 2f - 4, r.Y + r.Height / 2f - 4, 8, 8);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        int y = e.Item.Height / 2;
        using var p = new Pen(Teema.Joon);
        e.Graphics.DrawLine(p, 12, y, e.Item.Width - 12, y);
    }
}

