using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Ostu vormistamine moodsas disainis (lõuend 2.2): vasakul valge kaart sammudega
// (kättesaamine, klient ja tšekk, makse), paremal tume kokkuvõte ja punane "Maksa".
public partial class MaksmineVorm
{
    private MaksmineKokkuvote? kokkuvote;

    private void RakendaMoodne(RoundedPanel korv, RoundedPanel klient, RoundedPanel tarne, RoundedPanel makse, Panel riba, Control tuhista)
    {
        SuspendLayout();
        ClientSize = new Size(1100, 800);
        FormBorderStyle = FormBorderStyle.Sizable;   // venib ka poe lehena
        MinimumSize = new Size(800, 640);
        BackColor = Teema.Taust;
        korv.Visible = false;
        riba.Visible = false;

        // ---- Vasak valge kaart ----
        var paa = new RoundedPanel { Bounds = new Rectangle(16, 16, 712, 768), Radius = 16, FillColor = Teema.Pind, BorderColor = Teema.Pind, BackColor = Teema.Taust };
        var tagasi = new TagasiNupp { Location = new Point(26, 24) };
        tagasi.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        paa.Controls.Add(tagasi);
        paa.Controls.Add(new Label { Text = "Vormista ost", Location = new Point(tagasi.Right + 14, 30), AutoSize = true, Font = new Font("Segoe UI", 19F, FontStyle.Bold), ForeColor = Teema.Tekst, BackColor = Teema.Pind });
        paa.Controls.Add(new MaksmiseSammud { Bounds = new Rectangle(26, 90, 660, 52) });

        int y = 158;
        var kaardid = new List<RoundedPanel>();
        foreach (var (kaart, h0) in new[] { (tarne, 118), (klient, 206), (makse, 170) })
        {
            int h = h0;
            // valikud muutuvad kõrgemateks "pillideks" – nende all olevad read nihkuvad allapoole
            if (kaart.Controls.OfType<RadioButton>().Any())
            {
                int rTop = kaart.Controls.OfType<RadioButton>().Max(r => r.Top);
                foreach (Control c in kaart.Controls) if (c is not RadioButton && c.Top > rTop + 4) c.Top += 14;
                h += 14;
            }
            kaardid.Add(kaart);
            Controls.Remove(kaart);
            kaart.FillColor = kaart.BorderColor = Teema.Pind;
            kaart.Bounds = new Rectangle(14, y, 680, h);
            foreach (var l in kaart.Controls.OfType<Label>().Where(l => l.Font == UiStiil.AlapealkiriFont))
            {
                l.Text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(l.Text.ToLower());
                l.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                l.ForeColor = Teema.Tekst;
            }
            paa.Controls.Add(kaart);
            y += h + 14;
        }
        Controls.Add(paa);

        // ---- Parem tume kokkuvõte ----
        kokkuvote = new MaksmineKokkuvote { Bounds = new Rectangle(744, 16, 340, 768) };
        riba.Controls.Remove(btnMaksa);
        btnMaksa.Text = Keel.T("Maksa");
        btnMaksa.Bounds = new Rectangle(20, 768 - 84, 300, 60);
        btnMaksa.TextAlign = ContentAlignment.MiddleLeft;
        btnMaksa.Font = new Font("Segoe UI", 12.5F, FontStyle.Bold);
        MoodneUi.NoolStiil(btnMaksa);
        kokkuvote.Controls.Add(btnMaksa);
        Controls.Add(kokkuvote);

        // paigutus akna laiuse järgi: kokkuvõte 260–340 px paremal, vasak kaart ülejäänud
        void Paiguta()
        {
            int W = ClientSize.Width, H = ClientSize.Height;
            int kw = Math.Clamp(W * 32 / 100, 260, 340), kx = W - 16 - kw;
            paa.Bounds = new Rectangle(16, 16, Math.Max(400, kx - 32), H - 32);
            foreach (var k in kaardid) k.Width = paa.Width - 28;
            foreach (var c in paa.Controls.OfType<MaksmiseSammud>()) c.Width = paa.Width - 52;
            kokkuvote.Bounds = new Rectangle(kx, 16, kw, H - 32);
            btnMaksa.Bounds = new Rectangle(20, kokkuvote.Height - 84, kw - 40, 60);
        }
        Resize += (s, e) => Paiguta();
        Paiguta();
        ResumeLayout(true);
    }
}

// Sammud: 1 Kättesaamine · 2 Klient · 3 Makse (hall "pill", aktiivne valge)
public class MaksmiseSammud : Control
{
    public MaksmiseSammud()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), (Height - 1) / 2f))
        using (var b = new SolidBrush(Teema.Paneel2))
            g.FillPath(b, tee);
        string[] sammud = { "Kättesaamine", "Klient ja tšekk", "Makse" };
        float w = (Width - 12) / 3f;
        using var f = new Font("Segoe UI Semibold", 10F);
        using var fn = new Font("Segoe UI", 9F, FontStyle.Bold);
        for (int i = 0; i < 3; i++)
        {
            var r = new RectangleF(6 + i * w, 6, w - 4, Height - 12);
            if (i == 0)
                using (var tee = Ymardus.Tee(r, r.Height / 2))
                using (var b = new SolidBrush(Teema.Pind))
                    g.FillPath(b, tee);
            var ring = new RectangleF(r.X + 10, r.Y + (r.Height - 26) / 2, 26, 26);
            using (var b = new SolidBrush(i == 0 ? Teema.Aktsent : Teema.Tekst)) g.FillEllipse(b, ring);
            TextRenderer.DrawText(g, (i + 1).ToString(), fn, Rectangle.Round(ring), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, Keel.T(sammud[i]), f, Rectangle.Round(new RectangleF(ring.Right + 8, r.Y, r.Width - 46, r.Height)), Teema.Tekst, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}

// Tume kokkuvõte: korvi read pildiga, vahesumma, tarne, boonus, pant, suur "Kokku"
public class MaksmineKokkuvote : Control
{
    private decimal soodustus, tarne, kokku;
    private double t;

    public MaksmineKokkuvote()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.FromArgb(27, 28, 31);
        Animatsioon.Kaivita(this, 700, x => { t = x; Invalidate(); });
    }

    public void Naita(decimal soodustus, decimal tarne, decimal kokku)
    {
        this.soodustus = soodustus;
        this.tarne = tarne;
        this.kokku = kokku;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 32))
        using (var b = new SolidBrush(BackColor))
            g.FillPath(b, tee);
        var hall = Color.FromArgb(185, 189, 196);
        using var fp = new Font("Segoe UI", 15F, FontStyle.Bold);
        using var f = new Font("Segoe UI", 9.5F);
        using var fb = new Font("Segoe UI Semibold", 9.5F);
        TextRenderer.DrawText(g, Keel.T("Ostukorv"), fp, new Point(20, 22), Color.White);
        string tk = Keel.F("{0} tk", Ostukorv.Kogus);
        int tw = TextRenderer.MeasureText(tk, fb).Width + 24;
        var kr = new RectangleF(Width - 20 - tw, 26, tw, 28);
        using (var tee = Ymardus.Tee(kr, 14)) using (var b = new SolidBrush(Color.White)) g.FillPath(b, tee);
        TextRenderer.DrawText(g, tk, fb, Rectangle.Round(kr), BackColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        int y = 70, i = 0;
        foreach (var r in Ostukorv.Ridad)
        {
            if (y + 60 > Height - 300) { TextRenderer.DrawText(g, Keel.F("+ veel {0}", Ostukorv.Ridad.Count - i), f, new Point(20, y), hall); break; }
            float nihe = (float)((1 - Animatsioon.Sujuv(Math.Clamp(t * 1.6 - i * 0.12, 0, 1))) * 20);
            var rida = new RectangleF(14 + nihe, y, Width - 28, 58);
            using (var tee = Ymardus.Tee(rida, 18)) using (var b = new SolidBrush(Color.FromArgb(14, 255, 255, 255))) g.FillPath(b, tee);
            var pilt = new RectangleF(rida.X + 6, rida.Y + 5, 48, 48);
            using (var tee = Ymardus.Tee(pilt, 14)) using (var b = new SolidBrush(Color.White)) g.FillPath(b, tee);
            if (PildiVahemalu.Too(r.Pilt) is Image im) PildiVahemalu.Joonista(g, im, RectangleF.Inflate(pilt, -5, -5));
            TextRenderer.DrawText(g, r.Nimi, fb, Rectangle.Round(new RectangleF(pilt.Right + 10, rida.Y + 9, rida.Width - 140, 20)), Color.White, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, $"{r.Kogus} × {Ostukorv.Eur(r.Hind)}" + (r.Soodus != null ? "  · " + r.Soodus : ""), f, Rectangle.Round(new RectangleF(pilt.Right + 10, rida.Y + 29, rida.Width - 140, 20)), hall, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, Ostukorv.Eur(r.Summa), fb, Rectangle.Round(new RectangleF(rida.X, rida.Y, rida.Width - 12, rida.Height)), Color.White, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            y += 66;
            i++;
        }

        // summad
        int sy = Height - 268;
        void Rida(string silt, string vaartus, Color c) { TextRenderer.DrawText(g, silt, f, new Point(20, sy), c); TextRenderer.DrawText(g, vaartus, f, new Rectangle(0, sy, Width - 20, 20), c, TextFormatFlags.Right); sy += 24; }
        Rida(Keel.T("Vahesumma"), Ostukorv.Eur(Ostukorv.Kokku), Color.FromArgb(213, 216, 221));
        if (Ostukorv.PantKokku > 0) Rida(Keel.T("Pakendi tagatisraha"), Ostukorv.Eur(Ostukorv.PantKokku), Color.FromArgb(213, 216, 221));
        Rida(Keel.T("Tarne"), tarne == 0 ? Keel.T("TASUTA") : Ostukorv.Eur(tarne), Color.FromArgb(213, 216, 221));
        if (soodustus > 0) Rida(Keel.T("Boonus kliendikaardilt"), "−" + Ostukorv.Eur(soodustus), Color.FromArgb(127, 217, 162));
        using (var p = new Pen(Color.FromArgb(62, 64, 70))) g.DrawLine(p, 20, Height - 160, Width - 20, Height - 160);
        using (var fk = new Font("Segoe UI", 12F, FontStyle.Bold)) TextRenderer.DrawText(g, Keel.T("Kokku"), fk, new Point(20, Height - 140), Color.White);
        using (var fs = new Font("Segoe UI", 24F, FontStyle.Bold)) TextRenderer.DrawText(g, Ostukorv.Eur(kokku), fs, new Rectangle(0, Height - 152, Width - 18, 50), Color.White, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }
}
