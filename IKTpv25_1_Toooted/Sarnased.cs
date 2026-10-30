using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Üks rida: pilt, nimi, kilohind (roheline "−X %"), hind ja "+" nupp
public class SoodsamRida : Control
{
    private readonly Toode alus, toode;
    private bool hover, plussHover;
    private readonly SujuvOlek hoverSujuv, plussSujuv;
    private readonly Ilmumine ilm;
    public event Action<Toode>? Ava, Lisa;

    public SoodsamRida(Toode alus, Toode toode, int laius, int jrk = 0)
    {
        hoverSujuv = new SujuvOlek(this);
        plussSujuv = new SujuvOlek(this);
        ilm = new Ilmumine(this, 80 + jrk * 70);
        this.alus = alus;
        this.toode = toode;
        Size = new Size(laius, 68);
        Margin = new Padding(0);
        Cursor = Cursors.Hand;
        Tag = "omavarv";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        PildiVahemalu.Laetud += Laetud;
    }

    private void Laetud() { if (IsHandleCreated) BeginInvoke(Invalidate); }

    protected override void Dispose(bool disposing)
    {
        if (disposing) PildiVahemalu.Laetud -= Laetud;
        base.Dispose(disposing);
    }

    private Rectangle Pluss => new(Width - 54, (Height - 38) / 2, 38, 38);

    protected override void OnMouseMove(MouseEventArgs e)
    {
        bool p = Pluss.Contains(e.Location);
        if (!hover || p != plussHover) { hover = true; plussHover = p; hoverSujuv.Siht = true; plussSujuv.Siht = p; }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hover = plussHover = false; hoverSujuv.Siht = plussSujuv.Siht = false; base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button != MouseButtons.Left) return;
        if (Pluss.Contains(e.Location)) Lisa?.Invoke(toode); else Ava?.Invoke(toode);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Teema.Pind);
        if (hoverSujuv.V > 0)
            using (var tee = Ymardus.Tee(new Rectangle(6, 2, Width - 12, Height - 4), 12)) using (var b = new SolidBrush(Ymardus.Segu(Teema.Pind, Teema.Paneel2, (float)hoverSujuv.V))) g.FillPath(b, tee);

        var pk = new Rectangle(16, 10, 48, 48);
        using (var tee = Ymardus.Tee(pk, 10)) using (var b = new SolidBrush(Teema.PildiTaust)) g.FillPath(b, tee);
        if (PildiVahemalu.Pisi(toode.Pilt) is Image pilt) PildiVahemalu.Joonista(g, pilt, RectangleF.Inflate(pk, -3, -3));
        else Ikoonid.JoonistaEmoji(g, Ikoonid.KategooriaIkoon(toode.Kategooria?.Kategooria_nimetus), pk, Teema.TekstNork, 14f);

        using var fNimi = new Font("Segoe UI Semibold", 10F);
        using var fVaike = new Font("Segoe UI", 8.5F);
        using var fRoh = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        using var fHind = new Font("Segoe UI", 11.5F, FontStyle.Bold);
        var fl = TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;

        int hindX = Width - 70 - 90;
        int x = pk.Right + 14;
        TextRenderer.DrawText(g, toode.Toodenimetus, fNimi, new Rectangle(x, 14, hindX - x - 8, 20), Teema.Tekst, fl);
        string kilo = $"{Ikoonid.Hind(toode.UhikuHind ?? 0)}/{toode.Uhik}".Replace("€", " €");
        TextRenderer.DrawText(g, kilo, fVaike, new Point(x, 38), Teema.TekstNork, fl);
        var mf = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
        int kw = TextRenderer.MeasureText(g, kilo, fVaike, Size.Empty, mf).Width;
        string vahe = Keel.F("{0} % odavam kilohind", Sarnased.Odavam(alus, toode));
        var kiip = new Rectangle(x + kw + 8, 35, TextRenderer.MeasureText(g, vahe, fRoh, Size.Empty, mf).Width + 16, 20);
        using (var tee = Ymardus.Tee(kiip, 10)) using (var b = new SolidBrush(Ymardus.Segu(Teema.Pind, Teema.Edu, 0.14f))) g.FillPath(b, tee);
        TextRenderer.DrawText(g, vahe, fRoh, kiip, Teema.Edu, fl | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(g, Ostukorv.Eur(toode.Hind), fHind, new Rectangle(hindX, 0, 90, Height), Teema.Tekst,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        var p = Pluss;
        using (var b = new SolidBrush(Ymardus.Segu(Teema.Aktsent, Teema.AktsentTume, (float)plussSujuv.V))) g.FillEllipse(b, p);
        Ikoonid.Joonista(g, Ikoonid.Pluss, p, Color.White, 10f);
        ilm.Kata(g);
    }
}
