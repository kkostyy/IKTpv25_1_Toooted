namespace IKTpv25_1_Toooted;

// "Eesti   Русский   English" – keele valik (nagu e-poe lilla riba ülal)
public class KeeleLingid : Control
{
    private readonly Color varv;
    private readonly Rectangle[] alad = new Rectangle[3];
    private int hover = -1;
    private static readonly KeeleKood[] keeled = { KeeleKood.ET, KeeleKood.RU, KeeleKood.EN };

    public KeeleLingid(Color varv)
    {
        this.varv = varv;
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI Semibold", 9.5F);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        Paiguta();
        Keel.Muutus += Invalidate;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) Keel.Muutus -= Invalidate;
        base.Dispose(disposing);
    }

    private void Paiguta()
    {
        int x = 0, h = 0;
        using var paks = new Font(Font, FontStyle.Bold | FontStyle.Underline);
        for (int i = 0; i < keeled.Length; i++)
        {
            var s = TextRenderer.MeasureText(Keel.Nimi(keeled[i]), paks);
            alad[i] = new Rectangle(x, 0, s.Width, s.Height);
            x += s.Width + 14;
            h = Math.Max(h, s.Height);
        }
        Size = new Size(x - 14, h + 2);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int uus = Array.FindIndex(alad, a => a.Contains(e.Location));
        if (uus != hover) { hover = uus; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hover = -1; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        int i = Array.FindIndex(alad, a => a.Contains(e.Location));
        if (i >= 0) Keel.Sea(keeled[i]);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? BackColor);
        for (int i = 0; i < keeled.Length; i++)
        {
            bool valitud = Keel.Praegu == keeled[i];
            var stiil = valitud ? FontStyle.Bold : hover == i ? FontStyle.Underline : FontStyle.Regular;
            using var f = new Font(Font, stiil);
            Color v = valitud ? varv : Color.FromArgb(valitud ? 255 : 210, varv);
            TextRenderer.DrawText(e.Graphics, Keel.Nimi(keeled[i]), f, alad[i], v,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
