using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Pere korv poe ostukorvi veerus: kes mida lisas (avatar + nimi + aeg), +/−, "Võta kõik minu korvi".
// Uueneb ise (PereKorv kontrollib andmebaasi iga 2 s järel). Peres mitteolevale kasutajale on peidus.
public class PereKorviVaade : Control
{
    private const int Pais = 46, Rida = 46, MaxRidu = 4;
    private static readonly Color[] Varvid = { Color.FromArgb(160, 35, 110), Color.FromArgb(226, 6, 19), Color.FromArgb(20, 130, 70), Color.FromArgb(205, 115, 0), Color.FromArgb(40, 90, 180) };
    private Point hiir = new(-1, -1);
    // uus rida (pereliige lisas) helendab hetke; kõrgus muutub sujuvalt
    private readonly HashSet<int> tuntud = new();
    private readonly Dictionary<int, double> valgus = new();
    private bool esimene = true;

    public PereKorviVaade()
    {
        Tag = "omavarv";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        PereKorv.Muutus += Uuenda;
        Uuenda();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) PereKorv.Muutus -= Uuenda;
        base.Dispose(disposing);
    }

    private void Uuenda()
    {
        Visible = !string.IsNullOrEmpty(PereKorv.Kood);
        int n = Math.Min(MaxRidu, PereKorv.Read.Count);
        int siht = Pais + (PereKorv.Read.Count == 0 ? 34 : n * Rida + (PereKorv.Read.Count > MaxRidu ? 24 : 0)) + 12;
        if (esimene || !IsHandleCreated) Height = siht;
        else if (siht != Height) { int alg = Height; Animatsioon.Kaivita(this, 220, t => Height = alg + (int)Math.Round((siht - alg) * Animatsioon.Sujuv(t))); }
        foreach (var r in PereKorv.Read)
            if (tuntud.Add(r.Id) && !esimene)
            {
                int id = r.Id;
                Animatsioon.Kaivita(this, 1400, t => { valgus[id] = 1 - t; Invalidate(); }, () => valgus.Remove(id));
            }
        esimene = false;
        Invalidate();
    }

    private Rectangle VotaKoik => new(Width - 130, 10, 118, 28);
    private Rectangle ReaKast(int i) => new(10, Pais + i * Rida, Width - 20, Rida - 4);
    private Rectangle Miinus(int i) { var r = ReaKast(i); return new Rectangle(r.Right - 64, r.Y + 8, 26, 26); }
    private Rectangle Pluss(int i) { var r = ReaKast(i); return new Rectangle(r.Right - 32, r.Y + 8, 26, 26); }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        hiir = e.Location;
        bool kasi = VotaKoik.Contains(e.Location) && PereKorv.Read.Count > 0;
        for (int i = 0; i < Math.Min(MaxRidu, PereKorv.Read.Count); i++) kasi |= Miinus(i).Contains(e.Location) || Pluss(i).Contains(e.Location);
        Cursor = kasi ? Cursors.Hand : Cursors.Default;
        Invalidate();
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hiir = new(-1, -1); Invalidate(); base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (VotaKoik.Contains(e.Location) && PereKorv.Read.Count > 0)
        {
            var (lisatud, puudu) = PereKorv.VotaKoik();
            Teade.Toast(Keel.F("Pere korvist minu korvi: {0} tk", lisatud) + (puudu.Count > 0 ? " · " + Keel.F("puudu: {0}", string.Join(", ", puudu)) : ""),
                puudu.Count > 0 ? TeateTyyp.Hoiatus : TeateTyyp.Edu, 3500);
            return;
        }
        for (int i = 0; i < Math.Min(MaxRidu, PereKorv.Read.Count); i++)
        {
            var r = PereKorv.Read[i];
            if (Miinus(i).Contains(e.Location)) { PereKorv.Muuda(r.Id, -1); return; }
            if (Pluss(i).Contains(e.Location)) { PereKorv.Muuda(r.Id, +1); return; }
        }
    }

    public static string Hiljuti(DateTime aeg)
    {
        var v = DateTime.Now - aeg;
        if (v.TotalMinutes < 1) return Keel.T("just praegu");
        if (v.TotalMinutes < 60) return Keel.F("{0} min tagasi", (int)v.TotalMinutes);
        if (v.TotalHours < 24) return Keel.F("{0} h tagasi", (int)v.TotalHours);
        return aeg.ToString("dd.MM");
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Teema.Pind);
        var taust = new RectangleF(6, 4, Width - 12, Height - 10);
        using (var tee = Ymardus.Tee(taust, 14)) using (var b = new SolidBrush(Ymardus.Segu(Teema.Pind, Teema.Teine, 0.07f))) g.FillPath(b, tee);

        using var fp = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        using var fs = new Font("Segoe UI", 8.5F);
        using var fn = new Font("Segoe UI Semibold", 9F);
        using var fi = new Font("Segoe UI", 8F, FontStyle.Bold);
        var fl = TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;

        Ikoonid.Joonista(g, "", new Rectangle(16, 10, 24, 28), Teema.Teine, 11f);
        int tk = PereKorv.Read.Sum(r => r.Kogus);
        TextRenderer.DrawText(g, Keel.T("Pere korv"), fp, new Point(44, 14), Teema.Tekst);
        int pw = TextRenderer.MeasureText(g, Keel.T("Pere korv"), fp, Size.Empty, TextFormatFlags.NoPadding).Width;
        TextRenderer.DrawText(g, Keel.F("· {0} tk", tk), fs, new Point(48 + pw, 17), Teema.TekstNork);
        if (PereKorv.Read.Count > 0)
        {
            var v = VotaKoik;
            using (var tee = Ymardus.Tee(v, 14)) using (var b = new SolidBrush(v.Contains(hiir) ? Teema.AktsentTume : Teema.Teine)) g.FillPath(b, tee);
            TextRenderer.DrawText(g, Keel.T("Võta kõik →"), fi, v, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        if (PereKorv.Read.Count == 0)
        {
            TextRenderer.DrawText(g, Keel.T("Tühi. Toote lehel: „Lisa pere korvi“."), fs, new Rectangle(18, Pais, Width - 36, 24), Teema.TekstNork, fl);
            return;
        }
        for (int i = 0; i < Math.Min(MaxRidu, PereKorv.Read.Count); i++)
        {
            var r = PereKorv.Read[i];
            var k = ReaKast(i);
            if (valgus.TryGetValue(r.Id, out var vg) && vg > 0)
                using (var tee = Ymardus.Tee(k, 12)) using (var b = new SolidBrush(Color.FromArgb((int)(150 * vg), 255, 214, 120))) g.FillPath(b, tee);
            var av = new Rectangle(k.X + 6, k.Y + 6, 30, 30);
            using (var b = new SolidBrush(Varvid[Math.Abs(r.KasutajaId) % Varvid.Length])) g.FillEllipse(b, av);
            TextRenderer.DrawText(g, Pere.Initsiaalid(r.Lisaja), fi, av, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            int tx = av.Right + 10, tw = Miinus(i).X - tx - 8;
            TextRenderer.DrawText(g, $"{r.Kogus} × {r.Toode.Toodenimetus}", fn, new Rectangle(tx, k.Y + 5, tw, 18), Teema.Tekst, fl);
            bool mina = r.KasutajaId == AktiivneKasutaja.Id;
            TextRenderer.DrawText(g, (mina ? Keel.T("sina") : r.Lisaja) + " · " + Hiljuti(r.Aeg), fs, new Rectangle(tx, k.Y + 23, tw, 16), mina ? Teema.TekstNork : Teema.Teine, fl);
            foreach (var (nupp, glyph) in new[] { (Miinus(i), Ikoonid.Miinus), (Pluss(i), Ikoonid.Pluss) })
            {
                using (var b = new SolidBrush(nupp.Contains(hiir) ? Teema.Joon : Teema.Paneel2)) g.FillEllipse(b, nupp);
                Ikoonid.Joonista(g, glyph, nupp, Teema.Tekst, 7.5f);
            }
        }
        if (PereKorv.Read.Count > MaxRidu)
            TextRenderer.DrawText(g, Keel.F("+ veel {0}", PereKorv.Read.Count - MaxRidu), fs, new Point(56, Pais + MaxRidu * Rida + 2), Teema.TekstNork);
    }
}
