using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Ostukorvi vaade: iga rida pisipildi, nime, koguse (− / +) ja summaga.
// Kasutatakse nii Poes kui ka Kassa vahelehel; uueneb ise, kui korv muutub.
public class KorvVaade : Panel
{
    private readonly FlowLayoutPanel read;
    private readonly TyhiKorv tyhi;

    public KorvVaade()
    {
        DoubleBuffered = true;
        BackColor = Teema.Pind;

        read = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Teema.Pind
        };
        tyhi = new TyhiKorv { Dock = DockStyle.Top, Height = 140 };
        Controls.Add(read);
        Controls.Add(tyhi);

        Ostukorv.Muutus += KorvMuutus;
        PildiVahemalu.Laetud += PildidLaetud;
        read.Resize += (s, e) => SobitaLaiused();
        Uuenda();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Ostukorv.Muutus -= KorvMuutus;
            PildiVahemalu.Laetud -= PildidLaetud;
        }
        base.Dispose(disposing);
    }

    // Uuendame veidi hiljem (BeginInvoke): nupp, millele klõpsati, võib olla just see rida, mis kustub
    private void KorvMuutus()
    {
        if (IsHandleCreated && !IsDisposed) BeginInvoke(new Action(Uuenda));
        else Uuenda();
    }

    private void PildidLaetud()
    {
        if (IsHandleCreated && !IsDisposed) BeginInvoke(new Action(() => read.Invalidate(true)));
    }

    public void Uuenda()
    {
        if (IsDisposed) return;
        read.SuspendLayout();
        foreach (Control c in read.Controls.Cast<Control>().ToList()) c.Dispose();

        foreach (var rida in Ostukorv.Ridad)
            read.Controls.Add(new KorviReaKontroll(rida) { Margin = new Padding(0) });

        bool onTyhi = Ostukorv.Ridad.Count == 0;
        tyhi.Visible = onTyhi;
        read.Visible = !onTyhi;
        SobitaLaiused();
        read.ResumeLayout();
    }

    private void SobitaLaiused()
    {
        int laius = Math.Max(200, read.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        foreach (Control c in read.Controls) c.Width = laius;
    }

    // Teema muutumisel
    public void VarviUmber()
    {
        BackColor = read.BackColor = Teema.Pind;
        tyhi.Invalidate();
        Uuenda();
    }
}

// Üks ostukorvi rida
public class KorviReaKontroll : Control
{
    private readonly KorvRida rida;
    private readonly RoundedButton miinus, pluss, eemalda;

    public KorviReaKontroll(KorvRida rida)
    {
        this.rida = rida;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = Teema.OnMoodne && rida.Pant > 0 ? 96 : 72;   // moodne: pandi alarida (lõuend N8)
        BackColor = Teema.Pind;

        miinus = VaikeNupp("−");
        pluss = VaikeNupp("+");
        eemalda = VaikeNupp("✕");
        eemalda.Ohtlik = true;

        miinus.Click += (s, e) => Ostukorv.Vahenda(rida);
        pluss.Click += (s, e) =>
        {
            string? viga = Ostukorv.Suurenda(rida);
            if (viga != null) Teade.Toast(viga, TeateTyyp.Hoiatus);
        };
        eemalda.Click += (s, e) => Ostukorv.Eemalda(rida);

        Controls.AddRange(new Control[] { miinus, pluss, eemalda });
        PaigutaNupud();
    }

    private static RoundedButton VaikeNupp(string tekst)
    {
        var n = new RoundedButton
        {
            Text = tekst,
            Size = new Size(28, 28),
            Radius = 14,
            Peamine = false,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        return n;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        PaigutaNupud();
    }

    private void PaigutaNupud()
    {
        if (miinus == null) return;
        miinus.Location = new Point(70, 36);
        pluss.Location = new Point(132, 36);
        eemalda.Location = new Point(Width - 36, 8);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Teema.Pind);

        // Pisipilt
        var kast = new RectangleF(8, 10, 52, 52);
        using (var path = Ymardus.Tee(kast, 8))
        using (var b = new SolidBrush(Teema.PildiTaust))
            g.FillPath(b, path);
        if (PildiVahemalu.Pisi(rida.Pilt) is Image pilt)
            PildiVahemalu.Joonista(g, pilt, RectangleF.Inflate(kast, -4, -4));

        // Nimi ja hind
        using (var paks = new Font("Segoe UI Semibold", 9.75F))
            TextRenderer.DrawText(g, rida.Nimi, paks, new Rectangle(70, 8, Width - 112, 22), Teema.Tekst,
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);

        // Kogus nuppude vahel
        using (var f = new Font("Segoe UI Semibold", 10F))
            TextRenderer.DrawText(g, rida.Kogus.ToString(), f, new Rectangle(98, 36, 34, 28), Teema.Tekst,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        using (var vaike = new Font("Segoe UI", 8F))
            TextRenderer.DrawText(g, "× " + Ostukorv.Eur(rida.Hind), vaike, new Rectangle(164, 36, 90, 28), Teema.TekstNork,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        // Rea summa paremal
        using (var summa = new Font("Segoe UI Semibold", 10.5F))
            TextRenderer.DrawText(g, Ostukorv.Eur(rida.Summa), summa, new Rectangle(Width - 110, 36, 102, 28), Teema.Tekst,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        if (Teema.OnMoodne && rida.Pant > 0) Pandirida(g, 70, 70, Width - 8, rida.Kogus, rida.Pant);

        using var joon = new Pen(Teema.Joon);
        g.DrawLine(joon, 8, Height - 1, Width - 8, Height - 1);
    }

    // Roheline "P" münt + "Pakendi tagatisraha · 2 × 0,10 €" ja summa paremal
    public static void Pandirida(Graphics g, int x, int y, int parem, int kogus, decimal pant)
    {
        var roheline = Color.FromArgb(17, 104, 58);
        var myynt = new RectangleF(x, y, 18, 18);
        using (var b = new SolidBrush(Color.FromArgb(20, 130, 70))) g.FillEllipse(b, myynt);
        using var fp = new Font("Segoe UI", 6.5F, FontStyle.Bold);
        TextRenderer.DrawText(g, "P", fp, Rectangle.Round(myynt), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        using var f = new Font("Segoe UI", 8F, FontStyle.Bold);
        TextRenderer.DrawText(g, $"{Keel.T("Pakendi tagatisraha")} · {kogus} × {Ostukorv.Eur(pant)}", f, new Rectangle(x + 24, y, parem - x - 90, 18), roheline,
            TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g, Ostukorv.Eur(pant * kogus), f, new Rectangle(parem - 90, y, 90, 18), roheline, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }
}

// "Sinu ostukorv on tühi": hele kast ostukorvi ikooniga
public class TyhiKorv : Control
{
    public TyhiKorv()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Teema.Pind);

        var kast = new RectangleF(0, 4, Width - 1, Height - 12);
        using (var path = Ymardus.Tee(kast, 6))
        using (var b = new SolidBrush(Teema.Tume ? Teema.Paneel2 : Color.FromArgb(238, 243, 244)))
            g.FillPath(b, path);

        Ikoonid.Joonista(g, Ikoonid.Ostukorv, new Rectangle(0, 22, Width, 44), Teema.TekstNork, 26f);
        using var f = new Font("Segoe UI", 11F);
        TextRenderer.DrawText(g, Keel.T("Sinu ostukorv on tühi"), f, new Rectangle(0, 74, Width, 30), Teema.Tekst2,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        using var v = new Font("Segoe UI", 8.5F);
        TextRenderer.DrawText(g, Keel.T("Lisa tooteid nupuga „Lisa ostukorvi“"), v, new Rectangle(0, 100, Width, 20), Teema.TekstNork,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
