using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// "Ilma raiskamiseta" vitriini päis: roheline kaart, mitu toodet on allahinnatud ja kui palju saab säästa
public class RaiskamiseBanner : Control
{
    private readonly int arv;
    private readonly decimal saast;

    public RaiskamiseBanner(int arv, decimal saast)
    {
        this.arv = arv;
        this.saast = saast;
        Height = 128;
        Margin = new Padding(0, 0, 0, 16);
        Tag = "omavarv";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        var r = new RectangleF(0, 0, Width - 1, Height - 1);
        using (var tee = Ymardus.Tee(r, 24))
        using (var b = new LinearGradientBrush(r, Color.FromArgb(17, 104, 58), Color.FromArgb(32, 140, 82), 0f))
            g.FillPath(b, tee);
        // dekoratiivsed ringid paremal
        using (var b = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
        {
            g.FillEllipse(b, Width - 190, -60, 220, 220);
            g.FillEllipse(b, Width - 330, 70, 120, 120);
        }

        var ring = new Rectangle(26, (Height - 64) / 2, 64, 64);
        using (var b = new SolidBrush(Color.FromArgb(40, 255, 255, 255))) g.FillEllipse(b, ring);
        Ikoonid.Joonista(g, "", ring, Color.White, 20f);   // leht

        using var fp = new Font("Segoe UI", 17F, FontStyle.Bold);
        using var ft = new Font("Segoe UI", 10F);
        using var fa = new Font("Segoe UI", 22F, FontStyle.Bold);
        using var fs = new Font("Segoe UI", 9F);
        int x = ring.Right + 22, paremal = 230;
        TextRenderer.DrawText(g, Keel.T("Päästa toit prügikastist"), fp, new Point(x, 22), Color.White);
        TextRenderer.DrawText(g, Keel.F("Need tooted aeguvad {0} päeva jooksul ja on −{1} % odavamad. Iga ost vähendab toidu raiskamist.", Aegumine.PaeviEnne, Aegumine.Protsent),
            ft, new Rectangle(x, 56, Math.Max(100, Width - x - paremal - 20), 48), Color.FromArgb(220, 240, 228), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);

        if (Width - x > paremal + 220)
        {
            int px = Width - paremal;
            TextRenderer.DrawText(g, arv.ToString(), fa, new Point(px, 20), Color.White);
            TextRenderer.DrawText(g, Keel.T("toodet −30 %"), fs, new Point(px + 4, 60), Color.FromArgb(220, 240, 228));
            TextRenderer.DrawText(g, Keel.F("säästad kuni {0}", Ostukorv.Eur(saast)), new Font("Segoe UI Semibold", 10F), new Point(px + 4, 82), Color.White);
        }
    }
}
