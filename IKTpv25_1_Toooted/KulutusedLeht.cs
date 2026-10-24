using System.Drawing.Drawing2D;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// "Minu kulutused": ostude ajalugu statistikana – sõõrikdiagramm kategooriate kaupa,
// kuude valik ja virnastatud tulbad kuude kaupa. Ostjal oma ostud, töötajal kogu poe müük.
public class KulutusedLeht : Leht
{
    private readonly KulutusteVaade vaade = new() { Dock = DockStyle.Fill };
    public override string Pealkiri => Oma ? "Minu kulutused" : "Poe kulutused";

    // Ostja näeb oma oste, töötaja (omanik/müüja) kogu poe müüki
    private static bool Oma => AktiivneKasutaja.OnOstja && AktiivneKasutaja.KlientId != null;

    public KulutusedLeht()
    {
        Padding = new Padding(30, 4, 20, 20);
        Controls.Add(vaade);
    }

    public override void Avatud()
    {
        try
        {
            using var db = new ToodedContext();
            int? klient = Oma ? AktiivneKasutaja.KlientId : null;
            var ostud = db.Ostud.AsNoTracking().Include(o => o.Read)
                .Where(o => klient == null || o.KlientId == klient)
                .Where(o => o.Aeg >= DateTime.Today.AddMonths(-7))
                .ToList();
            var kat = db.Toodetabel.AsNoTracking().Select(t => new { t.Id, t.Kategooria.Kategooria_nimetus })
                .ToDictionary(x => x.Id, x => x.Kategooria_nimetus);
            vaade.Naita(Kulutused.Arvuta(ostud, kat, DateTime.Now), klient != null);
        }
        catch (Exception ex) { Teade.Show("Andmete laadimine ebaõnnestus:\n" + ex.Message); }
    }
}

public class KulutusteVaade : Control
{
    private static readonly Color[] Varvid =
    {
        Color.FromArgb(160, 35, 110), Color.FromArgb(226, 6, 19), Color.FromArgb(20, 130, 70), Color.FromArgb(245, 176, 74),
        Color.FromArgb(40, 90, 180), Color.FromArgb(235, 120, 175), Color.FromArgb(90, 200, 130), Color.FromArgb(120, 70, 190),
        Color.FromArgb(0, 150, 170), Color.FromArgb(200, 90, 40), Color.FromArgb(110, 140, 40), Color.FromArgb(180, 150, 0),
        Color.FromArgb(150, 155, 162)   // viimane = "Muu"
    };

    private List<Kulutused.Kuu> kuud = new();
    private bool oma;
    private int valitud = -1;          // -1 = kõik kuud
    private double t = 1;
    private Point hiir = new(-1, -1);
    private readonly ToolTip vihje = new();
    private int vihjeKuu = -2;
    private readonly Dictionary<string, Color> varv = new();

    public KulutusteVaade()
    {
        Tag = "omavarv";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) vihje.Dispose();
        base.Dispose(disposing);
    }

    public void Naita(List<Kulutused.Kuu> uued, bool omad)
    {
        kuud = uued;
        oma = omad;
        varv.Clear();
        int i = 0;
        // iga kategooria saab oma värvi (suurim kulu esimesena), et ka üksiku kuu vaates poleks halle
        foreach (var nimi in kuud.SelectMany(k => k.Kategooriad).Where(x => x.Key != Kulutused.Muu)
                     .GroupBy(x => x.Key).OrderByDescending(g => g.Sum(x => x.Value)).Select(g => g.Key))
            varv[nimi] = Varvid[i++ % (Varvid.Length - 1)];
        Animeeri();
    }

    private void Animeeri() => Animatsioon.Kaivita(this, 700, x => { t = Animatsioon.Sujuv(x); Invalidate(); });

    private Color Varv(string nimi) => varv.TryGetValue(nimi, out var c) ? c : Varvid[^1];

    private static CultureInfo Kultuur => Keel.Praegu switch { KeeleKood.RU => new("ru-RU"), KeeleKood.EN => new("en-GB"), _ => new("et-EE") };
    private static string KuuNimi(DateTime d, bool aastaga = false) => d.ToString(aastaga ? "MMMM yyyy" : "MMM", Kultuur).TrimEnd('.');

    // ---- paigutus ----
    private const int KiipY = 64, KaardiY = 116;
    private int KaardiKorgus => Math.Max(250, Math.Min(330, (Height - KaardiY - 40) * 55 / 100));
    private RectangleF Vasak => new(0, KaardiY, Width * 0.42f - 8, KaardiKorgus);
    private RectangleF Parem => new(Width * 0.42f + 8, KaardiY, Width * 0.58f - 9, KaardiKorgus);
    private RectangleF All => new(0, KaardiY + KaardiKorgus + 16, Width - 1, Math.Max(180, Height - KaardiY - KaardiKorgus - 17));

    private List<RectangleF> Kiibid(Graphics g, Font f)
    {
        var tulem = new List<RectangleF>();
        float x = 0;
        foreach (var tekst in new[] { Keel.F("{0} kuud", kuud.Count) }.Concat(kuud.Select(k => KuuNimi(k.Algus))))
        {
            float w = TextRenderer.MeasureText(g, tekst, f, Size.Empty, TextFormatFlags.NoPadding).Width + 30;
            tulem.Add(new RectangleF(x, KiipY, w, 36));
            x += w + 8;
        }
        return tulem;
    }

    private RectangleF Tulp(int i)
    {
        var a = All;
        float laius = (a.Width - 60) / Math.Max(1, kuud.Count);
        return new RectangleF(a.X + 40 + i * laius + laius * 0.25f, a.Y + 50, laius * 0.5f, a.Height - 84);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        hiir = e.Location;
        int k = -2;
        for (int i = 0; i < kuud.Count; i++) if (Tulp(i).Contains(e.Location)) k = i;
        if (k != vihjeKuu)
        {
            vihjeKuu = k;
            if (k >= 0)
            {
                var kuu = kuud[k];
                vihje.SetToolTip(this, KuuNimi(kuu.Algus, true) + ": " + Ostukorv.Eur(kuu.Kokku) + "\n" +
                    string.Join("\n", kuu.Kategooriad.OrderByDescending(x => x.Value).Take(5).Select(x => $"{Keel.T(x.Key)}: {Ostukorv.Eur(x.Value)}")));
            }
            else vihje.SetToolTip(this, "");
        }
        Invalidate();
        base.OnMouseMove(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        using var g = CreateGraphics();
        using var f = new Font("Segoe UI Semibold", 9.5F);
        var kiibid = Kiibid(g, f);
        for (int i = 0; i < kiibid.Count; i++)
            if (kiibid[i].Contains(e.Location)) { valitud = i - 1; Animeeri(); return; }
        for (int i = 0; i < kuud.Count; i++)
            if (Tulp(i).Contains(e.Location)) { valitud = valitud == i ? -1 : i; Animeeri(); return; }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        using var fp = new Font("Segoe UI", 20F, FontStyle.Bold);
        using var fs = new Font("Segoe UI", 9.5F);
        using var fk = new Font("Segoe UI Semibold", 9.5F);
        using var fb = new Font("Segoe UI", 11F, FontStyle.Bold);
        using var fsuur = new Font("Segoe UI", 20F, FontStyle.Bold);
        var fl = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

        TextRenderer.DrawText(g, Keel.T(oma ? "Minu kulutused" : "Poe kulutused"), fp, new Point(0, 0), Teema.Tekst);
        TextRenderer.DrawText(g, Keel.T(oma ? "Kuhu raha läheb – sinu ostud kategooriate kaupa" : "Kogu poe müük kategooriate kaupa"), fs, new Point(2, 38), Teema.TekstNork);

        // kuude kiibid
        var kiibid = Kiibid(g, fk);
        for (int i = 0; i < kiibid.Count; i++)
        {
            bool v = i - 1 == valitud, h = kiibid[i].Contains(hiir);
            using (var tee = Ymardus.Tee(kiibid[i], 18)) using (var b = new SolidBrush(v ? Teema.Tekst : h ? Teema.Joon : Teema.Pind)) g.FillPath(b, tee);
            string tekst = i == 0 ? Keel.F("{0} kuud", kuud.Count) : KuuNimi(kuud[i - 1].Algus);
            TextRenderer.DrawText(g, tekst, fk, Rectangle.Round(kiibid[i]), v ? Teema.Pind : Teema.Tekst, fl | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        var valik = valitud >= 0 && valitud < kuud.Count ? new List<Kulutused.Kuu> { kuud[valitud] } : kuud;
        var jaotus = Kulutused.Jaotus(valik);
        decimal kokku = valik.Sum(k => k.Kokku);

        // ---- sõõrik ----
        var vk = Vasak;
        using (var tee = Ymardus.Tee(vk, 22)) using (var b = new SolidBrush(Teema.Pind)) g.FillPath(b, tee);
        float d = Math.Min(vk.Width - 60, vk.Height - 60);
        var ring = new RectangleF(vk.X + (vk.Width - d) / 2, vk.Y + (vk.Height - d) / 2, d, d);
        float paksus = d * 0.18f;
        var sise = RectangleF.Inflate(ring, -paksus / 2, -paksus / 2);
        if (jaotus.Count == 0)
            using (var p = new Pen(Teema.Paneel2, paksus)) g.DrawEllipse(p, sise);
        else
        {
            float algus = -90;
            foreach (var o in jaotus)
            {
                float nurk = (float)(o.Protsent / 100m) * 360f * (float)t;
                using var p = new Pen(Varv(o.Nimi), paksus);
                if (nurk > 0.5f) g.DrawArc(p, sise, algus, Math.Max(0.5f, nurk - 1.2f));
                algus += nurk;
            }
        }
        var kesk = Rectangle.Round(RectangleF.Inflate(ring, -paksus, -paksus));
        TextRenderer.DrawText(g, Ostukorv.Eur(kokku), fsuur, new Rectangle(kesk.X, kesk.Y + kesk.Height / 2 - 26, kesk.Width, 32), Teema.Tekst, fl | TextFormatFlags.HorizontalCenter);
        TextRenderer.DrawText(g, valitud >= 0 ? KuuNimi(kuud[valitud].Algus, true) : Keel.F("viimased {0} kuud", kuud.Count), fs,
            new Rectangle(kesk.X, kesk.Y + kesk.Height / 2 + 8, kesk.Width, 20), Teema.TekstNork, fl | TextFormatFlags.HorizontalCenter);

        // ---- legend ----
        var pk = Parem;
        using (var tee = Ymardus.Tee(pk, 22)) using (var b = new SolidBrush(Teema.Pind)) g.FillPath(b, tee);
        float y = pk.Y + 22;
        if (jaotus.Count == 0)
            TextRenderer.DrawText(g, Keel.T("Selles perioodis oste pole."), fk, new Point((int)pk.X + 22, (int)y), Teema.TekstNork);
        else
        {
            var suurim = jaotus.First(o => o.Nimi != Kulutused.Muu || jaotus.Count == 1);
            TextRenderer.DrawText(g, Keel.F("Kõige rohkem: {0} – {1} %", Keel.T(suurim.Nimi), suurim.Protsent.ToString("0", Eesti)), fb,
                new Rectangle((int)pk.X + 22, (int)y, (int)pk.Width - 44, 22), Varv(suurim.Nimi), fl | TextFormatFlags.EndEllipsis);
            y += 34;
            float rh = Math.Min(34, (pk.Bottom - y - 14) / Math.Max(1, jaotus.Count));
            foreach (var o in jaotus)
            {
                using (var b = new SolidBrush(Varv(o.Nimi))) g.FillEllipse(b, pk.X + 22, y + rh / 2 - 6, 12, 12);
                var riba = new RectangleF(pk.X + pk.Width * 0.42f, y + rh / 2 - 4, pk.Width * 0.18f, 8);
                TextRenderer.DrawText(g, Keel.T(o.Nimi), fk, new Rectangle((int)pk.X + 44, (int)y, (int)(riba.X - pk.X - 54), (int)rh), Teema.Tekst, fl | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                using (var tee = Ymardus.Tee(riba, 4)) using (var b = new SolidBrush(Teema.Paneel2)) g.FillPath(b, tee);
                float w = riba.Width * (float)(o.Protsent / 100m) * (float)t;
                if (w > 2) using (var tee = Ymardus.Tee(new RectangleF(riba.X, riba.Y, w, riba.Height), 4)) using (var b = new SolidBrush(Varv(o.Nimi))) g.FillPath(b, tee);
                TextRenderer.DrawText(g, $"{Ostukorv.Eur(o.Summa)}  ·  {o.Protsent.ToString("0", Eesti)} %", fs, new Rectangle((int)riba.Right + 10, (int)y, (int)(pk.Right - riba.Right - 32), (int)rh), Teema.Tekst2,
                    fl | TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
                y += rh;
            }
        }

        // ---- kuude tulbad ----
        var a = All;
        using (var tee = Ymardus.Tee(a, 22)) using (var b = new SolidBrush(Teema.Pind)) g.FillPath(b, tee);
        TextRenderer.DrawText(g, Keel.T("Kulutused kuude kaupa"), fb, new Point((int)a.X + 22, (int)a.Y + 16), Teema.Tekst);
        if (kuud.Count >= 2 && kuud[^2].Kokku > 0)
        {
            decimal m = Math.Round((kuud[^1].Kokku - kuud[^2].Kokku) / kuud[^2].Kokku * 100);
            TextRenderer.DrawText(g, Keel.F("{0} {1} % eelmise kuuga", m >= 0 ? "▲" : "▼", Math.Abs(m)), fk,
                new Rectangle((int)a.X, (int)a.Y + 18, (int)a.Width - 22, 20), m > 0 ? Teema.Viga : Teema.Edu, fl | TextFormatFlags.Right);
        }
        decimal max = Math.Max(1, kuud.Count == 0 ? 1 : kuud.Max(k => k.Kokku));
        for (int i = 0; i < kuud.Count; i++)
        {
            var tp = Tulp(i);
            float alus = tp.Bottom;
            foreach (var (nimi, summa) in kuud[i].Kategooriad.OrderByDescending(x => x.Value))
            {
                float h = (float)(summa / max) * tp.Height * (float)t;
                if (h < 0.5f) continue;
                using var b = new SolidBrush(Color.FromArgb(valitud < 0 || valitud == i ? 255 : 90, Varv(nimi)));
                g.FillRectangle(b, tp.X, alus - h, tp.Width, h);
                alus -= h;
            }
            if (tp.Contains(hiir)) using (var p = new Pen(Teema.Tekst, 2)) g.DrawRectangle(p, tp.X - 2, alus - 2, tp.Width + 4, tp.Bottom - alus + 4);
            TextRenderer.DrawText(g, KuuNimi(kuud[i].Algus), fs, new Rectangle((int)tp.X - 20, (int)tp.Bottom + 6, (int)tp.Width + 40, 18),
                valitud == i ? Teema.Tekst : Teema.TekstNork, fl | TextFormatFlags.HorizontalCenter);
            if (kuud[i].Kokku > 0)
                TextRenderer.DrawText(g, Ostukorv.Eur(kuud[i].Kokku), new Font("Segoe UI", 8F), new Rectangle((int)tp.X - 30, (int)alus - 18, (int)tp.Width + 60, 16),
                    Teema.Tekst2, fl | TextFormatFlags.HorizontalCenter);
        }
    }

    private static readonly CultureInfo Eesti = new("et-EE");
}
