using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Hooajaline kujundus poe päises: Halloween (kõrvitsad, nahkhiired), jõulud ja talv (lumi, tulukesed),
// sõbrapäev (südamed). Lihavõttemunad: kõrvitsale klõpsamine, Konami kood ↑↑↓↓←→←→BA.
public enum Hooaeg { Puudub, Halloween, Joulud, Talv, Sobrapaev }

public static class Hooajad
{
    private static string Valjas => Path.Combine(Kaustad.Juur, "hooaeg_valjas");

    public static bool Sees
    {
        get => !File.Exists(Valjas);
        set
        {
            try
            {
                Directory.CreateDirectory(Kaustad.Juur);
                if (value) File.Delete(Valjas); else File.WriteAllText(Valjas, "1");
            }
            catch { }
        }
    }

    // AKT_HOOAEG=talv|joulud|halloween|sobrapaev testimiseks
    public static Hooaeg Praegu(DateTime d)
    {
        var env = Environment.GetEnvironmentVariable("AKT_HOOAEG");
        if (!string.IsNullOrEmpty(env) && Enum.TryParse<Hooaeg>(env, true, out var h)) return h;
        return Kuupaevast(d);
    }

    public static Hooaeg Kuupaevast(DateTime d) => (d.Month, d.Day) switch
    {
        (10, >= 15) or (11, <= 2) => Hooaeg.Halloween,
        (12, _) or (1, <= 6) => Hooaeg.Joulud,
        (2, >= 10 and <= 14) => Hooaeg.Sobrapaev,
        (1, _) or (2, _) => Hooaeg.Talv,
        _ => Hooaeg.Puudub
    };

    // Konami kood: ↑↑↓↓←→←→BA
    private static readonly Keys[] Konami = { Keys.Up, Keys.Up, Keys.Down, Keys.Down, Keys.Left, Keys.Right, Keys.Left, Keys.Right, Keys.B, Keys.A };
    private static int konamiSamm;
    public static event Action? KonamiLeitud;

    public static void Klahv(Keys k)
    {
        k &= Keys.KeyCode;
        if (k == Konami[konamiSamm]) konamiSamm++;
        else konamiSamm = k == Konami[0] ? 1 : 0;
        if (konamiSamm == Konami.Length) { konamiSamm = 0; KonamiLeitud?.Invoke(); }
    }
}

public class HooajaRiba : Control
{
    private record Osake(float X, float Y, float Vx, float Vy, float Suurus, Color Varv, float Elu);

    public Hooaeg Hooaeg { get; }
    public Action<string>? Otsi { get; set; }

    private readonly System.Windows.Forms.Timer taimer = new() { Interval = 40 };
    private readonly List<Osake> osakesed = new();
    private readonly Random juhus = new();
    private readonly HashSet<int> poksatud = new();
    private readonly Dictionary<int, float> poks = new();   // kõrvits → animatsioon 1 → 0
    private float aeg;
    private Point hiir = new(-1, -1);

    public HooajaRiba(Hooaeg? sund = null)
    {
        Hooaeg = sund ?? (Hooajad.Sees ? Hooajad.Praegu(DateTime.Now) : Hooaeg.Puudub);
        Height = 38;
        Tag = "omavarv";
        Visible = Hooaeg != Hooaeg.Puudub;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        taimer.Tick += (s, e) => Samm();
        if (Visible) taimer.Start();
        Hooajad.KonamiLeitud += Konfetti;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { taimer.Dispose(); Hooajad.KonamiLeitud -= Konfetti; }
        base.Dispose(disposing);
    }

    private (Color A, Color B, Color Tekst) Varvid => Hooaeg switch
    {
        Hooaeg.Halloween => (Color.FromArgb(43, 22, 64), Color.FromArgb(120, 45, 20), Color.FromArgb(255, 200, 120)),
        Hooaeg.Sobrapaev => (Color.FromArgb(160, 35, 110), Color.FromArgb(235, 120, 175), Color.White),
        _ => (Color.FromArgb(20, 40, 80), Color.FromArgb(40, 90, 150), Color.White)
    };

    private string Tekst => Hooaeg switch
    {
        Hooaeg.Halloween => "Halloween 31.10 – kõrvitsad, kommid ja kostüümid",
        Hooaeg.Joulud => "Häid pühi! Kingitused ja pühadetoidud",
        Hooaeg.Sobrapaev => "Head sõbrapäeva! Šokolaad ja lilled",
        _ => "Head talve! Soe kakao ja supid"
    };

    private string Otsing => Hooaeg switch
    {
        Hooaeg.Halloween => "kõrvits",
        Hooaeg.Joulud => "piparkook",
        Hooaeg.Sobrapaev => "šokolaad",
        _ => "kakao"
    };

    private int KorvitsaArv => Math.Max(3, Math.Min(7, (Width - 520) / 110));
    private RectangleF Korvits(int i)
    {
        float vahe = (Width - 470f) / KorvitsaArv;
        float bob = (float)Math.Sin(aeg * 2.2 + i * 1.3) * 2.5f;
        float s = 24 + (poks.TryGetValue(i, out var p) ? p * 10 : 0);
        return new RectangleF(450 + i * vahe + vahe / 2 - s / 2, (Height - s) / 2 + bob + 1, s, s);
    }

    private Rectangle TekstiKast => new(14, 0, 420, Height);

    private void Samm()
    {
        if (!Visible || !IsHandleCreated) return;
        aeg += 0.04f;
        // uued osakesed: lumi talvel/jõuludel, südamed sõbrapäeval
        if (Hooaeg is Hooaeg.Talv or Hooaeg.Joulud && osakesed.Count < Width / 14)
            osakesed.Add(new Osake(juhus.Next(Math.Max(1, Width)), -4, (float)(juhus.NextDouble() - 0.5) * 0.6f, 0.4f + (float)juhus.NextDouble() * 0.7f, 1.5f + (float)juhus.NextDouble() * 2.2f, Color.White, 999));
        if (Hooaeg == Hooaeg.Sobrapaev && osakesed.Count < Width / 40 && juhus.Next(4) == 0)
            osakesed.Add(new Osake(juhus.Next(Math.Max(1, Width)), Height + 4, 0, -0.5f - (float)juhus.NextDouble() * 0.5f, 5 + juhus.Next(4), Color.FromArgb(255, 210, 230), 999));
        for (int i = osakesed.Count - 1; i >= 0; i--)
        {
            var o = osakesed[i];
            float vy = o.Elu < 999 ? o.Vy + 0.25f : o.Vy;   // konfetti/kommid kukuvad
            o = o with { X = o.X + o.Vx + (o.Elu == 999 && Hooaeg != Hooaeg.Sobrapaev ? (float)Math.Sin(aeg * 3 + o.Y) * 0.3f : 0), Y = o.Y + vy, Vy = vy, Elu = o.Elu - (o.Elu < 999 ? 1 : 0) };
            if (o.Y > Height + 6 || o.Y < -8 || o.Elu <= 0) osakesed.RemoveAt(i); else osakesed[i] = o;
        }
        foreach (var k in poks.Keys.ToList()) { poks[k] -= 0.08f; if (poks[k] <= 0) poks.Remove(k); }
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        hiir = e.Location;
        bool kasi = TekstiKast.Contains(e.Location);
        if (Hooaeg == Hooaeg.Halloween) for (int i = 0; i < KorvitsaArv; i++) kasi |= Korvits(i).Contains(e.Location);
        Cursor = kasi ? Cursors.Hand : Cursors.Default;
        base.OnMouseMove(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (TekstiKast.Contains(e.Location)) { Otsi?.Invoke(Otsing); return; }
        if (Hooaeg == Hooaeg.Halloween)
        {
            for (int i = 0; i < KorvitsaArv; i++)
                if (Korvits(i).Contains(e.Location))
                {
                    poks[i] = 1;
                    Purse(Korvits(i), new[] { Color.FromArgb(255, 120, 0), Color.FromArgb(255, 220, 60), Color.FromArgb(150, 60, 200), Color.FromArgb(60, 200, 90) }, 18);
                    poksatud.Add(i);
                    if (poksatud.Count == KorvitsaArv)
                    {
                        poksatud.Clear();
                        Teade.Toast(Keel.T("Komm või pomm! Leidsid kõik kõrvitsad – sa oled tõeline kõrvitsakütt."), TeateTyyp.Edu, 4000);
                    }
                    return;
                }
        }
        else Purse(new RectangleF(e.X - 4, e.Y - 4, 8, 8), new[] { Color.White, Color.FromArgb(200, 225, 255) }, 14);   // lumepall
    }

    private void Purse(RectangleF koht, Color[] varvid, int arv)
    {
        for (int i = 0; i < arv; i++)
            osakesed.Add(new Osake(koht.X + koht.Width / 2, koht.Y + koht.Height / 2, (float)(juhus.NextDouble() - 0.5) * 7, -2 - (float)juhus.NextDouble() * 3,
                2 + (float)juhus.NextDouble() * 3, varvid[juhus.Next(varvid.Length)], 26 + juhus.Next(14)));
    }

    // Konami kood: konfetti üle kogu riba
    private void Konfetti()
    {
        if (!Visible || IsDisposed) return;
        var v = new[] { Color.FromArgb(226, 6, 19), Color.FromArgb(160, 35, 110), Color.FromArgb(245, 176, 74), Color.FromArgb(90, 200, 130), Color.FromArgb(40, 90, 180), Color.White };
        for (int x = 20; x < Width; x += 60) Purse(new RectangleF(x, Height / 2f, 4, 4), v, 6);
        Teade.Toast(Keel.T("↑↑↓↓←→←→BA – saladus leitud! Täna oled sa poe parim klient."), TeateTyyp.Edu, 4000);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        var (a, b, tv) = Varvid;
        var r = new RectangleF(0, 2, Width - 1, Height - 5);
        using (var tee = Ymardus.Tee(r, r.Height / 2))
        using (var br = new LinearGradientBrush(r, a, b, 0f))
            g.FillPath(br, tee);
        var vana = g.Clip;
        using (var klipp = Ymardus.Tee(r, r.Height / 2)) g.SetClip(klipp);

        if (Hooaeg == Hooaeg.Joulud)   // tulukesed vilguvad
            for (int x = 18, i = 0; x < Width; x += 26, i++)
            {
                bool poleb = ((int)(aeg * 3) + i) % 3 != 0;
                var c = new[] { Color.FromArgb(255, 80, 80), Color.FromArgb(255, 220, 80), Color.FromArgb(90, 220, 120), Color.FromArgb(120, 170, 255) }[i % 4];
                using var br2 = new SolidBrush(poleb ? c : Color.FromArgb(70, c));
                g.FillEllipse(br2, x, 4 + (float)Math.Sin(i) * 1.5f, 6, 6);
            }

        if (Hooaeg == Hooaeg.Halloween)
        {
            // nahkhiired lendavad
            for (int i = 0; i < 3; i++)
            {
                float x = (float)((aeg * (40 + i * 15) + i * 300) % (Width + 80)) - 40;
                float y = 8 + i * 7 + (float)Math.Sin(aeg * 4 + i) * 3;
                Nahkhiir(g, x, y, 0.8f + i * 0.15f, (float)Math.Sin(aeg * 18 + i));
            }
            for (int i = 0; i < KorvitsaArv; i++) Korvits(g, Korvits(i), Korvits(i).Contains(hiir));
        }

        foreach (var o in osakesed)
        {
            using var br3 = new SolidBrush(o.Varv);
            if (Hooaeg == Hooaeg.Sobrapaev && o.Elu == 999) Syda(g, o.X, o.Y, o.Suurus, br3);
            else g.FillEllipse(br3, o.X, o.Y, o.Suurus, o.Suurus);
        }
        g.Clip = vana;

        using var f = new Font("Segoe UI Semibold", 9.5F);
        var tk = TekstiKast;
        TextRenderer.DrawText(g, Keel.T(Tekst), f, tk, TekstiKast.Contains(hiir) ? Color.White : tv,
            TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
    }

    private static void Korvits(Graphics g, RectangleF r, bool hover)
    {
        var keha = RectangleF.Inflate(r, 0, -r.Height * 0.1f);
        using (var br = new SolidBrush(hover ? Color.FromArgb(255, 150, 30) : Color.FromArgb(240, 120, 20)))
        {
            g.FillEllipse(br, keha.X, keha.Y, keha.Width * 0.6f, keha.Height);
            g.FillEllipse(br, keha.X + keha.Width * 0.4f, keha.Y, keha.Width * 0.6f, keha.Height);
            g.FillEllipse(br, keha.X + keha.Width * 0.2f, keha.Y, keha.Width * 0.6f, keha.Height);
        }
        using (var br = new SolidBrush(Color.FromArgb(70, 140, 40))) g.FillRectangle(br, r.X + r.Width * 0.45f, r.Y, r.Width * 0.1f, r.Height * 0.2f);
        using var nagu = new SolidBrush(Color.FromArgb(60, 20, 0));
        float cx = keha.X + keha.Width / 2, cy = keha.Y + keha.Height * 0.42f, s = keha.Width;
        g.FillPolygon(nagu, new[] { new PointF(cx - s * 0.28f, cy), new PointF(cx - s * 0.14f, cy - s * 0.12f), new PointF(cx - s * 0.06f, cy) });
        g.FillPolygon(nagu, new[] { new PointF(cx + s * 0.06f, cy), new PointF(cx + s * 0.14f, cy - s * 0.12f), new PointF(cx + s * 0.28f, cy) });
        g.FillPie(nagu, cx - s * 0.25f, cy + s * 0.02f, s * 0.5f, s * 0.3f, 0, 180);
    }

    private static void Nahkhiir(Graphics g, float x, float y, float s, float tiib)
    {
        using var br = new SolidBrush(Color.FromArgb(20, 10, 30));
        float t = 4 + tiib * 3;
        g.FillPolygon(br, new[]
        {
            new PointF(x, y), new PointF(x - 7 * s, y - t * s), new PointF(x - 4 * s, y + 1 * s), new PointF(x - 10 * s, y - (t - 2) * s),
            new PointF(x - 6 * s, y + 3 * s), new PointF(x, y + 2 * s), new PointF(x + 6 * s, y + 3 * s), new PointF(x + 10 * s, y - (t - 2) * s),
            new PointF(x + 4 * s, y + 1 * s), new PointF(x + 7 * s, y - t * s)
        });
    }

    private static void Syda(Graphics g, float x, float y, float s, Brush br)
    {
        using var tee = new GraphicsPath();
        tee.AddBezier(x, y + s * 0.3f, x, y, x - s * 0.5f, y, x - s * 0.5f, y + s * 0.3f);
        tee.AddBezier(x - s * 0.5f, y + s * 0.3f, x - s * 0.5f, y + s * 0.6f, x, y + s * 0.8f, x, y + s);
        tee.AddBezier(x, y + s, x, y + s * 0.8f, x + s * 0.5f, y + s * 0.6f, x + s * 0.5f, y + s * 0.3f);
        tee.AddBezier(x + s * 0.5f, y + s * 0.3f, x + s * 0.5f, y, x, y, x, y + s * 0.3f);
        g.FillPath(br, tee);
    }
}
