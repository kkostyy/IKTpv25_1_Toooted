using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Kojuveo ajaakna valik: üleval 7 päeva "pillid", all päeva 2-tunnised aknad.
// Täis aknad on hallid ("täis"), möödunud aknaid ei näidata.
public class TarneAjaValija : Control
{
    private List<DateTime> aknad = new();
    private IReadOnlyDictionary<DateTime, int> hoivatud = new Dictionary<DateTime, int>();
    private DateTime tana = DateTime.Today;
    private DateTime paev = DateTime.Today;
    private Point hiir = new(-1, -1);

    // Valiku "pill" libiseb vanast kohast uude (päev ja kellaaeg eraldi)
    private RectangleF paevNaha, aknaNaha, paevAlgus, aknaAlgus;
    private double paevT = 1, aknaT = 1;
    private bool joonistatud;

    private static RectangleF Lerp(RectangleF a, RectangleF b, double t) => new(
        (float)(a.X + (b.X - a.X) * t), (float)(a.Y + (b.Y - a.Y) * t), (float)(a.Width + (b.Width - a.Width) * t), (float)(a.Height + (b.Height - a.Height) * t));

    private void Libista(bool paevaPill)
    {
        if (!joonistatud) return;
        if (paevaPill) { paevAlgus = paevNaha; paevT = 0; Animatsioon.Kaivita(this, 200, t => { paevT = Animatsioon.Sujuv(t); Invalidate(); }); }
        aknaAlgus = aknaNaha; aknaT = 0;
        Animatsioon.Kaivita(this, 200, t => { aknaT = Animatsioon.Sujuv(t); Invalidate(); });
    }

    public DateTime? Valitud { get; private set; }
    public event Action? Muutus;

    public TarneAjaValija()
    {
        Height = 104;
        Cursor = Cursors.Hand;
        Tag = "omavarv";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    public void Laadi(DateTime nyyd, IReadOnlyDictionary<DateTime, int> hoivatud)
    {
        this.hoivatud = hoivatud;
        tana = nyyd.Date;
        aknad = Tarne.Aknad(nyyd);
        Valitud = Tarne.EsimeneVaba(nyyd, hoivatud);
        paev = Valitud?.Date ?? tana;
        Invalidate();
        Muutus?.Invoke();
    }

    private List<DateTime> Paevad => aknad.Select(a => a.Date).Distinct().ToList();

    private RectangleF PaevaKast(int i, int n)
    {
        float w = (Width - 1 - (n - 1) * 8f) / Math.Max(1, n);
        return new RectangleF(i * (w + 8), 0, w, 46);
    }

    private RectangleF AknaKast(int i)
    {
        int n = Tarne.AknaAlgused.Length;
        float w = (Width - 1 - (n - 1) * 8f) / n;
        return new RectangleF(i * (w + 8), 58, w, 40);
    }

    protected override void OnMouseMove(MouseEventArgs e) { hiir = e.Location; Invalidate(); base.OnMouseMove(e); }
    protected override void OnMouseLeave(EventArgs e) { hiir = new(-1, -1); Invalidate(); base.OnMouseLeave(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (!Enabled) return;
        var paevad = Paevad;
        for (int i = 0; i < paevad.Count; i++)
            if (PaevaKast(i, paevad.Count).Contains(e.Location))
            {
                if (paevad[i] != paev) Libista(true);
                paev = paevad[i];
                // valitud aken jääb samale kellaajale, kui see on vaba
                var sama = Valitud is DateTime v ? paev.AddHours(v.Hour) : (DateTime?)null;
                Valitud = sama is DateTime s && aknad.Contains(s) && Tarne.Vaba(s, hoivatud)
                    ? s : aknad.Where(a => a.Date == paev).Cast<DateTime?>().FirstOrDefault(a => Tarne.Vaba(a!.Value, hoivatud));
                Invalidate();
                Muutus?.Invoke();
                return;
            }
        for (int i = 0; i < Tarne.AknaAlgused.Length; i++)
        {
            var a = paev.AddHours(Tarne.AknaAlgused[i]);
            if (AknaKast(i).Contains(e.Location) && aknad.Contains(a) && Tarne.Vaba(a, hoivatud))
            {
                if (Valitud != a) Libista(false);
                Valitud = a;
                Invalidate();
                Muutus?.Invoke();
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        int al = Enabled ? 255 : 110;
        Color A(Color c) => Color.FromArgb(al * c.A / 255, c);

        using var fNimi = new Font("Segoe UI Semibold", 9F);
        using var fKuup = new Font("Segoe UI", 8F);
        using var fAken = new Font("Segoe UI Semibold", 9.5F);
        using var fTais = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        var kesk = TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

        var paevad = Paevad;
        // kõigepealt tavalised taustad, siis libisev valik, siis tekstid
        for (int i = 0; i < paevad.Count; i++)
        {
            var r = PaevaKast(i, paevad.Count);
            using var tee = Ymardus.Tee(r, 14);
            using var b = new SolidBrush(A(r.Contains(hiir) && paevad[i] != paev ? Teema.Joon : Teema.Paneel2));
            g.FillPath(b, tee);
            if (paevad[i] == paev) paevNaha = Lerp(paevAlgus, r, paevT);
        }
        if (paevad.Contains(paev))
            using (var tee = Ymardus.Tee(paevNaha, 14)) using (var b = new SolidBrush(A(Teema.Tekst))) g.FillPath(b, tee);
        for (int i = 0; i < paevad.Count; i++)
        {
            var r = PaevaKast(i, paevad.Count);
            bool v = paevad[i] == paev;
            bool h = r.Contains(hiir);
            bool koikTaisSees = aknad.Where(a => a.Date == paevad[i]).All(a => !Tarne.Vaba(a, hoivatud));
            v = v && paevT > 0.5;   // tekst muutub valgeks, kui libisev pill on kohal
            var tv = v ? Teema.Pind : koikTaisSees ? Teema.TekstNork : Teema.Tekst;
            TextRenderer.DrawText(g, Tarne.PaevaNimi(paevad[i], tana), fNimi, new Rectangle((int)r.X, (int)r.Y + 6, (int)r.Width, 18), A(tv), kesk | TextFormatFlags.EndEllipsis);
            TextRenderer.DrawText(g, paevad[i].ToString("dd.MM"), fKuup, new Rectangle((int)r.X, (int)r.Y + 25, (int)r.Width, 16), A(v ? Teema.Pind : Teema.TekstNork), kesk);
        }

        RectangleF? valitudKast = null;
        for (int i = 0; i < Tarne.AknaAlgused.Length; i++)
        {
            var r = AknaKast(i);
            var a = paev.AddHours(Tarne.AknaAlgused[i]);
            bool olemas = aknad.Contains(a);
            bool vaba = olemas && Tarne.Vaba(a, hoivatud);
            bool v = Valitud == a;
            bool h = vaba && r.Contains(hiir);
            var taust = !olemas ? Teema.Pind : !vaba ? Teema.Paneel2 : h && !v ? Teema.AktsentHele : Teema.Pind;
            using (var tee = Ymardus.Tee(r, r.Height / 2))
            {
                using (var b = new SolidBrush(A(taust))) g.FillPath(b, tee);
                using (var p = new Pen(A(olemas ? Teema.Joon : Ymardus.Segu(Teema.Pind, Teema.Joon, 0.5f))) { DashStyle = olemas ? DashStyle.Solid : DashStyle.Dot })
                    g.DrawPath(p, tee);
            }
            if (v) { aknaNaha = Lerp(aknaAlgus, r, aknaT); valitudKast = r; continue; }   // joonistatakse pärast tsüklit
            string t = $"{Tarne.AknaAlgused[i]}–{Tarne.AknaAlgused[i] + Tarne.AknaTunde}";
            var tv = v ? Color.White : vaba ? Teema.Tekst : Teema.TekstNork;
            if (olemas && !vaba)
            {
                TextRenderer.DrawText(g, t, fAken, new Rectangle((int)r.X, (int)r.Y + 4, (int)r.Width, 18), A(tv), kesk);
                TextRenderer.DrawText(g, Keel.T("täis"), fTais, new Rectangle((int)r.X, (int)r.Y + 21, (int)r.Width, 14), A(Teema.Viga), kesk);
            }
            else
                TextRenderer.DrawText(g, t, fAken, Rectangle.Round(r), A(tv), kesk | TextFormatFlags.VerticalCenter);
        }
        if (valitudKast is RectangleF vk && Valitud is DateTime va)
        {
            using (var tee = Ymardus.Tee(aknaNaha, aknaNaha.Height / 2)) using (var b = new SolidBrush(A(Teema.Aktsent))) g.FillPath(b, tee);
            TextRenderer.DrawText(g, $"{va.Hour}–{va.Hour + Tarne.AknaTunde}", fAken, Rectangle.Round(vk), A(aknaT > 0.5 ? Color.White : Teema.Tekst), kesk | TextFormatFlags.VerticalCenter);
        }
        joonistatud = true;
    }
}
