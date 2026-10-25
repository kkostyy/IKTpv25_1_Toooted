using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Ostukorvi analüüs (omanik): mida ostetakse koos (viimased 90 päeva).
//  Iga seose juures nupp "Loo kampaania": teine toode −10 % nädala jooksul.
// ===================================================================
public class OstukorviAnalyysVorm : KujundusVorm
{
    private readonly FlowLayoutPanel loend;
    private readonly Label lblOlek;
    private bool kategooriad;
    private List<Ost> ostud = new();
    private Dictionary<int, Toode> tooted = new();
    private Dictionary<int, string> kat = new();

    public OstukorviAnalyysVorm() : base("Mida ostetakse koos", 1000, 720)
    {
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(760, 500);
        BackColor = Teema.Taust;

        var yla = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = Teema.Taust };
        yla.Controls.Add(new Label { Text = "Mida ostetakse koos", Location = new Point(18, 14), AutoSize = true, Font = UiStiil.PealkirjaFont, ForeColor = Teema.Tekst, BackColor = Teema.Taust });
        lblOlek = new Label { Location = new Point(20, 52), AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Taust };
        yla.Controls.Add(lblOlek);
        var filter = new MoodneFilter { Location = new Point(18, 80) };
        filter.Sea(new[] { (Keel.T("Tooted"), -1), (Keel.T("Kategooriad"), -1) }, 0);
        filter.Valitud += i => { kategooriad = i == 1; Naita(); };
        yla.Controls.Add(filter);
        lblOlek.LocationChanged += (s, e) => filter.Left = lblOlek.Left - 2;

        loend = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true,
            Padding = new Padding(16, 4, 16, 16), BackColor = Teema.Taust
        };
        loend.Resize += (s, e) => { foreach (Control c in loend.Controls) c.Width = Math.Max(500, loend.ClientSize.Width - 36); };
        Controls.Add(loend);
        Controls.Add(yla);
        loend.BringToFront();

        Load += (s, e) => Laadi();
    }

    private void Laadi()
    {
        try
        {
            using var db = new ToodedContext();
            var alates = DateTime.Today.AddDays(-90);
            ostud = db.Ostud.AsNoTracking().Include(o => o.Read).Where(o => o.Aeg >= alates && !o.Tagastatud).ToList();
            var ids = ostud.SelectMany(o => o.Read).Where(r => r.ToodeId != null).Select(r => r.ToodeId!.Value).Distinct().ToList();
            tooted = db.Toodetabel.AsNoTracking().Include(t => t.Kategooria).Where(t => ids.Contains(t.Id)).ToDictionary(t => t.Id);
            kat = tooted.Values.ToDictionary(t => t.Id, t => t.Kategooria.Kategooria_nimetus);
        }
        catch (Exception ex) { Teade.Show("Andmete laadimine ebaõnnestus:\n" + ex.Message); }
        lblOlek.Text = Keel.F("Analüüsitud {0} ostu viimase 90 päeva jooksul. Tõus = mitu korda sagedamini kui juhuslikult.", ostud.Count);
        Naita();
    }

    private void Naita()
    {
        loend.SuspendLayout();
        foreach (Control c in loend.Controls.Cast<Control>().ToList()) c.Dispose();
        var reeglid = kategooriad
            ? Ostukorvianalyys.Kategooriad(ostud, kat)
            : Ostukorvianalyys.Tooted(ostud, id => tooted.TryGetValue(id, out var t) ? t.Toodenimetus : "#" + id);
        // A→B ja B→A on sama paar: näitame tugevama suuna
        var naha = new HashSet<string>();
        foreach (var r in reeglid.Take(60))
        {
            string voti = string.CompareOrdinal(r.A, r.B) < 0 ? r.A + "|" + r.B : r.B + "|" + r.A;
            if (!naha.Add(voti)) continue;
            var kaart = new ReegliKaart(r, kategooriad, naha.Count - 1) { Width = Math.Max(500, loend.ClientSize.Width - 36) };
            kaart.Kampaania += LooKampaania;
            loend.Controls.Add(kaart);
            if (naha.Count >= 25) break;
        }
        if (loend.Controls.Count == 0)
            loend.Controls.Add(new Label
            {
                Text = Keel.T("Seoseid veel pole – vaja on rohkem oste (vähemalt 3 ostu, kus tooted on koos). Proovi demoandmeid."),
                AutoSize = true, ForeColor = Teema.TekstNork, Margin = new Padding(4, 20, 0, 0)
            });
        loend.ResumeLayout();
        Teema.Rakenda(loend);
    }

    // Teine toode (või kategooria enimmüüdud toode) −10 % 7 päevaks
    private void LooKampaania(Ostukorvianalyys.Reegel r)
    {
        int? toodeId = r.ToodeB;
        if (toodeId == null)
            toodeId = ostud.SelectMany(o => o.Read).Where(x => x.ToodeId is int id && kat.GetValueOrDefault(id) == r.B)
                .GroupBy(x => x.ToodeId!.Value).OrderByDescending(g => g.Sum(x => x.Kogus)).Select(g => (int?)g.Key).FirstOrDefault();
        if (toodeId is not int tid || !tooted.TryGetValue(tid, out var t)) { Teade.Show("Sobivat toodet ei leitud."); return; }
        if (Teade.Show(Keel.F("Loon kampaania: „{0}“ −10 % järgmiseks 7 päevaks?\nPõhjus: kes ostab „{1}“, võtab {2} % juhtudest ka „{3}“.",
                t.Toodenimetus, r.A, Math.Round(r.Kindlus * 100), r.B), "Seotud kampaania", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        using var db = new ToodedContext();
        var k = new Kampaania
        {
            Nimi = Keel.F("Koos ostetakse: {0}", r.A.Length > 40 ? r.A[..40] + "…" : r.A),
            ToodeId = tid, SoodusProtsent = 10, Algus = DateTime.Now, Lopp = DateTime.Today.AddDays(8).AddMinutes(-1), Automaatne = true
        };
        if (Kampaaniad.Kontrolli(db, k) is string viga) { Teade.Show(viga, "Kampaania", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        Kampaaniad.Salvesta(db, k);
        Teade.Toast(Keel.F("Kampaania loodud: „{0}“ −10 %", t.Toodenimetus));
    }
}

// Üks seos kaardina: tõusu ring, lause, statistika ja nupp
public class ReegliKaart : Control
{
    private readonly Ostukorvianalyys.Reegel r;
    private readonly bool kategooria;
    private bool nuppHover;
    public event Action<Ostukorvianalyys.Reegel>? Kampaania;
    private readonly Ilmumine ilm;
    private readonly SujuvOlek nuppSujuv;

    public ReegliKaart(Ostukorvianalyys.Reegel r, bool kategooria, int jrk = 0)
    {
        ilm = new Ilmumine(this, Math.Min(jrk, 10) * 60);
        nuppSujuv = new SujuvOlek(this);
        this.r = r;
        this.kategooria = kategooria;
        Height = 92;
        Margin = new Padding(0, 0, 0, 10);
        Tag = "omavarv";
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    private Rectangle Nupp => new(Width - 190, (Height - 40) / 2, 172, 40);

    protected override void OnMouseMove(MouseEventArgs e)
    {
        bool h = Nupp.Contains(e.Location);
        if (h != nuppHover) { nuppHover = h; Cursor = h ? Cursors.Hand : Cursors.Default; nuppSujuv.Siht = h; }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { nuppHover = false; nuppSujuv.Siht = false; base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (Nupp.Contains(e.Location)) Kampaania?.Invoke(r);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 18)) using (var b = new SolidBrush(Teema.Pind)) g.FillPath(b, tee);

        // tõusu ring: mida tugevam seos, seda tumedam ploom
        var ring = new Rectangle(18, (Height - 56) / 2, 56, 56);
        float tugevus = (float)Math.Clamp((r.Tous - 1) / 3, 0.15, 1);
        using (var b = new SolidBrush(Ymardus.Segu(Color.FromArgb(246, 230, 239), Color.FromArgb(160, 35, 110), tugevus))) g.FillEllipse(b, ring);
        using var fr = new Font("Segoe UI", 11F, FontStyle.Bold);
        TextRenderer.DrawText(g, $"×{r.Tous:0.0}", fr, ring, tugevus > 0.5f ? Color.White : Color.FromArgb(160, 35, 110), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        using var f = new Font("Segoe UI", 10.5F);
        using var fb = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        using var fs = new Font("Segoe UI", 9F);
        int x = ring.Right + 18, laius = Nupp.Left - x - 16;
        var fl = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
        // "Kes ostab A, võtab 60 % juhtudest ka B"
        string a = Keel.T(r.A), b2 = Keel.T(r.B);
        int px = x;
        foreach (var (osa, paks) in new[] { (Keel.T("Kes ostab") + " ", false), (a, true), (", " + Keel.F("võtab {0} % juhtudest ka", Math.Round(r.Kindlus * 100)) + " ", false), (b2, true) })
        {
            var font = paks ? fb : f;
            int w = Math.Min(TextRenderer.MeasureText(g, osa, font, Size.Empty, fl).Width, Math.Max(40, x + laius - px));
            TextRenderer.DrawText(g, osa, font, new Rectangle(px, 22, w, 22), paks ? Teema.Tekst : Teema.Tekst2, fl | TextFormatFlags.EndEllipsis);
            px += w;
            if (px >= x + laius) break;
        }
        TextRenderer.DrawText(g, Keel.F("koos {0} ostus · {1} ostu sisaldas esimest · seos {2:0.0}× tugevam kui juhuslik", r.Koos, r.OsteA, r.Tous), fs,
            new Rectangle(x, 52, laius, 18), Teema.TekstNork, fl | TextFormatFlags.EndEllipsis);

        var n = Nupp;
        using (var tee = Ymardus.Tee(n, 20)) using (var bb = new SolidBrush(Ymardus.Segu(Teema.Aktsent, Teema.AktsentTume, (float)nuppSujuv.V))) g.FillPath(bb, tee);
        TextRenderer.DrawText(g, Keel.T(kategooria ? "Kampaania kategooriale" : "Loo kampaania −10 %"), fs, n, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        ilm.Kata(g);
    }
}
