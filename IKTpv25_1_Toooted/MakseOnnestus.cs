using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// "Makse õnnestus!" (nagu telefonis): pärast maksmist tume kaart rohelise linnukesega,
// summa, ostu nr, tarne, boonus ja tagasiraha. Nupud: "Vaata tšekki" (PDF) ja "Jätka ostlemist".
// Enter / Esc / klõps "Jätka" sulgeb.
public class MakseOnnestusVorm : Form
{
    private readonly Ost ost;
    private readonly string? pdf;
    private double pop, tekst;   // animatsioonid 0 → 1
    private int hiir = -1;       // 0 = tšekk, 1 = jätka

    private static readonly Color Taust = Color.FromArgb(27, 28, 31), Taust2 = Color.FromArgb(44, 46, 51),
        Roheline = Color.FromArgb(34, 160, 90), Nork = Color.FromArgb(190, 194, 200);

    public static void Naita(IWin32Window? omanik, Ost ost, string? pdf)
    {
        using var v = new MakseOnnestusVorm(ost, pdf);
        if (omanik != null) v.ShowDialog(omanik);
        else { v.StartPosition = FormStartPosition.CenterScreen; v.ShowDialog(); }
    }

    private MakseOnnestusVorm(Ost ost, string? pdf)
    {
        this.ost = ost;
        this.pdf = pdf;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        KeyPreview = true;
        ClientSize = new Size(440, 560);
        BackColor = Taust;
        Text = Keel.T("Makse õnnestus!");
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        using (var tee = Ymardus.Tee(new Rectangle(0, 0, Width, Height), 28)) Region = new Region(tee);
        Cursor = Cursors.Default;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Animatsioon.Kaivita(this, 520, t => { pop = t; Invalidate(); });
        Animatsioon.Kaivita(this, 420, t => { tekst = t; Invalidate(); }, viivitus: 180);
    }

    private Rectangle TsekiNupp => new(28, Height - 136, Width - 56, 50);
    private Rectangle JatkaNupp => new(28, Height - 76, Width - 56, 50);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Enter or Keys.Escape or Keys.Space) { Close(); e.Handled = true; }
        base.OnKeyDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int uus = pdf != null && TsekiNupp.Contains(e.Location) ? 0 : JatkaNupp.Contains(e.Location) ? 1 : -1;
        if (uus != hiir) { hiir = uus; Cursor = uus >= 0 ? Cursors.Hand : Cursors.Default; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (pdf != null && TsekiNupp.Contains(e.Location))
        {
            try { Arve.Ava(pdf); } catch (Exception ex) { Teade.Show("Tšeki avamine ebaõnnestus:\n" + ex.Message); }
        }
        else if (JatkaNupp.Contains(e.Location)) Close();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Taust);
        int w = Width;

        // Roheline ring + linnuke (hüppab sisse)
        float s = (float)Math.Max(0, Animatsioon.Pop(pop));
        float d = 110 * s, cx = w / 2f, cy = 112;
        if (d > 1)
        {
            using (var halo = new SolidBrush(Color.FromArgb((int)(40 * Math.Clamp(pop, 0, 1)), Roheline)))
                g.FillEllipse(halo, cx - d / 2 - 14, cy - d / 2 - 14, d + 28, d + 28);
            using (var b = new SolidBrush(Roheline)) g.FillEllipse(b, cx - d / 2, cy - d / 2, d, d);
            using var p = new Pen(Color.White, 7 * s) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            g.DrawLines(p, new[] { new PointF(cx - 22 * s, cy + 1 * s), new PointF(cx - 6 * s, cy + 17 * s), new PointF(cx + 24 * s, cy - 15 * s) });
        }

        // Tekstid tuhmuvad sisse
        int a = (int)(255 * Math.Clamp(tekst, 0, 1));
        var kesk = TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
        using (var f = new Font("Segoe UI", 20F, FontStyle.Bold))
            TextRenderer.DrawText(g, Keel.T("Makse õnnestus!"), f, new Rectangle(0, 186, w, 40), Color.FromArgb(a, Color.White), kesk);
        using (var f = new Font("Segoe UI", 34F, FontStyle.Bold))
            TextRenderer.DrawText(g, Ostukorv.Eur(ost.Summa), f, new Rectangle(0, 226, w, 64), Color.FromArgb(a, Color.White), kesk);
        using (var f = new Font("Segoe UI", 10F))
            TextRenderer.DrawText(g, Keel.F("Ost nr {0}", ost.Id) + " · " + ost.Aeg.ToString("dd.MM.yyyy HH:mm"), f,
                new Rectangle(0, 292, w, 22), Color.FromArgb(a, Nork), kesk);

        // Kiibid: tarne, boonus, tagasiraha
        var kiibid = new List<(string Tekst, Color Taust, Color Varv)>();
        if (ost.Tarneviis == Tarne.Kojuvedu)
            kiibid.Add((ost.TarneAeg is DateTime ta ? Keel.T("Kojuvedu") + " · " + Tarne.AknaTekst(ta, DateTime.Today) : Keel.T("Kojuvedu"), Taust2, Color.White));
        else if (ost.Tarneviis == Tarne.Kaasa)
            kiibid.Add((Keel.T("Poest kaasa"), Taust2, Color.White));
        if (ost.BoonusLisatud > 0)
            kiibid.Add(("+" + Ostukorv.Eur(ost.BoonusLisatud) + " " + Keel.T("boonust"), Color.FromArgb(30, 77, 51), Color.FromArgb(127, 217, 162)));
        decimal tagasi = ost.MakstudSumma - ost.Summa;
        if (tagasi > 0)
            kiibid.Add((Keel.F("Tagasiraha: {0}", Ostukorv.Eur(tagasi)), Color.FromArgb(77, 60, 20), Color.FromArgb(245, 215, 110)));
        using (var fk = new Font("Segoe UI Semibold", 9F))
        {
            int y = 330;
            foreach (var (t, tb, tv) in kiibid)
            {
                int kw = Math.Min(w - 56, TextRenderer.MeasureText(t, fk).Width + 28);
                var r = new Rectangle((w - kw) / 2, y, kw, 30);
                using (var tee = Ymardus.Tee(r, 15)) using (var b = new SolidBrush(Color.FromArgb(a, tb))) g.FillPath(b, tee);
                TextRenderer.DrawText(g, t, fk, r, Color.FromArgb(a, tv), kesk | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                y += 38;
            }
        }
        if (!string.IsNullOrEmpty(ost.Email))
            using (var f = new Font("Segoe UI", 9F))
                TextRenderer.DrawText(g, Keel.F("Tšekk saadetakse aadressile {0}", ost.Email), f,
                    new Rectangle(16, TsekiNupp.Y - 30, w - 32, 20), Color.FromArgb(a, Nork), kesk | TextFormatFlags.EndEllipsis);

        // Nupud
        using var fn = new Font("Segoe UI Semibold", 11F);
        if (pdf != null)
        {
            var r = TsekiNupp;
            using (var tee = Ymardus.Tee(r, 25)) using (var b = new SolidBrush(hiir == 0 ? Color.FromArgb(225, 228, 232) : Color.White)) g.FillPath(b, tee);
            TextRenderer.DrawText(g, Keel.T("Vaata tšekki"), fn, r, Taust, kesk | TextFormatFlags.VerticalCenter);
        }
        var j = JatkaNupp;
        using (var tee = Ymardus.Tee(j, 25))
        {
            if (hiir == 1) using (var b = new SolidBrush(Taust2)) g.FillPath(b, tee);
            using var p = new Pen(Color.FromArgb(90, 255, 255, 255), 1.5f);
            g.DrawPath(p, tee);
        }
        TextRenderer.DrawText(g, Keel.T("Jätka ostlemist"), fn, j, Color.White, kesk | TextFormatFlags.VerticalCenter);
    }
}
