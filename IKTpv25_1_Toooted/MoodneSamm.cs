using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Moodne arvu valija "−  10  +" (asendab vana NumericUpDown / DateTimePicker kerimisnoolte):
// ümar pill, nupud reageerivad hiirele, väärtus "hüppab" sujuvalt, toimib ka hiireratta ja nooltega.
public class MoodneSamm : Control
{
    private decimal vaartus;
    private int hiirAll = -1, hiirKohal = -1;   // 0 = miinus, 1 = pluss
    private double hupe = 1;                    // väärtuse animatsioon 0..1
    private int suund;
    private System.Windows.Forms.Timer? anim;
    private readonly System.Windows.Forms.Timer kordus = new() { Interval = 400 };

    public decimal Minimum { get; set; }
    public decimal Maximum { get; set; } = 100;
    public decimal Samm { get; set; } = 1;
    public Func<decimal, string> Formaat { get; set; } = v => v.ToString("0");
    public event EventHandler? ValueChanged;

    public decimal Value
    {
        get => vaartus;
        set
        {
            var uus = Math.Clamp(value, Minimum, Maximum);
            if (uus == vaartus) return;
            suund = Math.Sign(uus - vaartus);
            vaartus = uus;
            anim?.Stop();
            anim = Animatsioon.Kaivita(this, 160, t => { hupe = Animatsioon.Sujuv(t); Invalidate(); });
            ValueChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public MoodneSamm()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable, true);
        Size = new Size(140, 40);
        Font = new Font("Segoe UI Semibold", 10.5F);
        Cursor = Cursors.Hand;
        TabStop = true;
        // nupu all hoidmine: kordab (esimene 400 ms, siis kiiremini)
        kordus.Tick += (s, e) => { kordus.Interval = 70; Muuda(hiirAll == 0 ? -1 : 1); };
    }

    private Rectangle Nupp(int i) => i == 0 ? new Rectangle(4, 4, Height - 8, Height - 8) : new Rectangle(Width - Height + 4, 4, Height - 8, Height - 8);

    private void Muuda(int s) => Value = vaartus + s * Samm;

    private int Kus(Point p) => Nupp(0).Contains(p) ? 0 : Nupp(1).Contains(p) ? 1 : -1;

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        hiirAll = Kus(e.Location);
        if (hiirAll < 0) return;
        Muuda(hiirAll == 0 ? -1 : 1);
        kordus.Interval = 400;
        kordus.Start();
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); kordus.Stop(); hiirAll = -1; Invalidate(); }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int k = Kus(e.Location);
        if (k != hiirKohal) { hiirKohal = k; Invalidate(); }
    }

    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hiirKohal = -1; kordus.Stop(); hiirAll = -1; Invalidate(); }

    protected override void OnMouseWheel(MouseEventArgs e) { base.OnMouseWheel(e); Muuda(Math.Sign(e.Delta)); }

    protected override bool IsInputKey(Keys k) => k is Keys.Up or Keys.Down or Keys.Left or Keys.Right || base.IsInputKey(k);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode is Keys.Up or Keys.Right) Muuda(1);
        else if (e.KeyCode is Keys.Down or Keys.Left) Muuda(-1);
    }

    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        var r = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var tee = Ymardus.Tee(r, Height / 2))
        {
            using var b = new SolidBrush(Teema.Paneel);
            g.FillPath(b, tee);
            if (Focused) { using var p = new Pen(Teema.Aktsent, 1.5f); g.DrawPath(p, tee); }
        }
        for (int i = 0; i < 2; i++)
        {
            var n = Nupp(i);
            bool lubatud = i == 0 ? vaartus > Minimum : vaartus < Maximum;
            var taust = !lubatud ? Teema.Paneel : hiirAll == i ? Teema.Raam : hiirKohal == i ? Teema.Pind : Teema.Paneel2;
            using (var b = new SolidBrush(taust)) g.FillEllipse(b, n);
            using var pliiats = new Pen(lubatud ? Teema.Tekst : Teema.TekstNork, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            int cx = n.X + n.Width / 2, cy = n.Y + n.Height / 2, p = n.Width / 5;
            g.DrawLine(pliiats, cx - p, cy, cx + p, cy);
            if (i == 1) g.DrawLine(pliiats, cx, cy - p, cx, cy + p);
        }
        // väärtus libiseb sisse muutuse suunas
        var tekstiAla = new Rectangle(Height, 0, Width - 2 * Height, Height);
        int nihe = (int)((1 - hupe) * 8) * -suund;
        var varv = Color.FromArgb((int)(150 + 105 * hupe), Teema.Tekst);
        TextRenderer.DrawText(g, Formaat(vaartus), Font, new Rectangle(tekstiAla.X, tekstiAla.Y + nihe, tekstiAla.Width, tekstiAla.Height), varv,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { kordus.Dispose(); anim?.Stop(); }
        base.Dispose(disposing);
    }
}
