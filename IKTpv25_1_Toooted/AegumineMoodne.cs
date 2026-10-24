using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Partiid ja säilivusaeg moodsas disainis (lõuend N10): valgel kaardil
//  tume reegli riba ja partiide read (eluiga, hind, olek); paremal
//  "NII NÄEB OSTJA" – kõige kiiremini aeguva soodustusega toote eelvaade.
// ===================================================================
public partial class AegumineVorm
{
    private FlowLayoutPanel? moodneRead;
    private OstjaEelvaade? eelvaade;
    private Label? lblMoodneMaha;
    private PartiiRida? valitudRida;

    private void EhitaMoodne()
    {
        KeyPreview = true;
        Padding = new Padding(16);
        var valge = Teema.Pind;

        // ---- Paremal: ostja vaade ----
        var parem = new RoundedPanel { Dock = DockStyle.Right, Width = 340, Radius = 16, FillColor = valge, BorderColor = valge, BackColor = Teema.Taust, Padding = new Padding(22, 20, 22, 20) };
        var nii = new Label { Text = Keel.T("NII NÄEB OSTJA"), Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Teema.Teine, BackColor = valge };
        eelvaade = new OstjaEelvaade { Dock = DockStyle.Top, Height = 340 };
        var reeglid = new Label
        {
            Text = "• " + Keel.T("Varem aeguvat partiid müüakse esimesena (FEFO).") + "\n• " + Keel.F("Kassas ja tšekil: „Aegub … −{0} %“.", Aegumine.Protsent) + "\n• " + Keel.T("Aegunud partii eemaldatakse müügilt automaatselt."),
            Dock = DockStyle.Top, Height = 92, Padding = new Padding(0, 14, 0, 0), Font = new Font("Segoe UI", 9.5F), ForeColor = Teema.Tekst2, BackColor = valge
        };
        var mahaPealkiri = new Label { Text = Keel.T("MAHA KANTUD"), Dock = DockStyle.Top, Height = 26, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Teema.TekstNork, BackColor = valge };
        lblMoodneMaha = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8.5F), ForeColor = Teema.Tekst2, BackColor = valge };
        parem.Controls.Add(lblMoodneMaha);
        parem.Controls.Add(mahaPealkiri);
        parem.Controls.Add(reeglid);
        parem.Controls.Add(eelvaade);
        parem.Controls.Add(nii);

        // ---- Vasakul: päis, reegel, read ----
        var vasak = new RoundedPanel { Dock = DockStyle.Fill, Radius = 16, FillColor = valge, BorderColor = valge, BackColor = Teema.Taust, Padding = new Padding(24, 20, 24, 16) };
        var pais = new Panel { Dock = DockStyle.Top, Height = 62, BackColor = valge };
        var tagasi = new TagasiNupp { Location = new Point(0, 4) };
        tagasi.Click += (s, e) => MineTagasi();
        pais.Controls.Add(tagasi);
        pais.Controls.Add(new Label { Text = Keel.T("Partiid ja säilivusaeg"), Location = new Point(tagasi.Right + 14, 0), AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Teema.Tekst, BackColor = valge });
        pais.Controls.Add(new Label { Text = Keel.F("Täna {0:dd.MM.yyyy}", DateTime.Today), Location = new Point(tagasi.Right + 16, 34), AutoSize = true, ForeColor = Teema.TekstNork, BackColor = valge });
        var uus = UiStiil.LooNupp("+ Uus partii", (s, e) => { var d = new UusPartiiVorm(); Aken.Ava(this, d, () => { if (d.DialogResult == DialogResult.OK) LaadiMoodne(); }); }, true);
        var maha = (RoundedButton)UiStiil.LooNupp("Kanna valitud maha", (s, e) => KannaMahaMoodne());
        maha.Ohtlik = true;
        foreach (var b in new[] { uus, maha }) { b.AutoSize = false; b.Height = 44; b.Width = TextRenderer.MeasureText(Keel.T(b.Text), b.Font).Width + 40; b.Anchor = AnchorStyles.Top | AnchorStyles.Right; pais.Controls.Add(b); }
        pais.Resize += (s, e) => { uus.Location = new Point(pais.Width - uus.Width, 6); maha.Location = new Point(uus.Left - maha.Width - 10, 6); };

        var reegel = new ReegliRiba { Dock = DockStyle.Top, Height = 84 };
        var vahe = new Panel { Dock = DockStyle.Top, Height = 14, BackColor = valge };
        var veerud = new PartiiVeerud { Dock = DockStyle.Top, Height = 28 };
        moodneRead = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = valge };
        moodneRead.Resize += (s, e) => { foreach (Control c in moodneRead.Controls) c.Width = moodneRead.ClientSize.Width - 4; };
        vasak.Controls.Add(moodneRead);
        vasak.Controls.Add(veerud);
        vasak.Controls.Add(vahe);
        vasak.Controls.Add(reegel);
        vasak.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = valge });
        vasak.Controls.Add(pais);

        Controls.Add(vasak);
        Controls.Add(new Panel { Dock = DockStyle.Right, Width = 16, BackColor = Teema.Taust });
        Controls.Add(parem);
        vasak.BringToFront();
    }

    private void LaadiMoodne()
    {
        if (moodneRead == null) return;
        using var db = new ToodedContext();
        int mahaArv = Aegumine.Uuenda(db, DateTime.Now);
        if (mahaArv > 0) Teade.Toast(Keel.F("Aegunud kaupa maha kantud: {0} tk", mahaArv), TeateTyyp.Hoiatus);
        partiid = db.Partiid.Include(p => p.Toode).AsNoTracking().OrderBy(p => p.Aegub).ToList();

        moodneRead.SuspendLayout();
        foreach (Control c in moodneRead.Controls.Cast<Control>().ToList()) c.Dispose();
        valitudRida = null;
        int i = 0;
        foreach (var p in partiid.Where(p => p.Kogus > 0))
        {
            var r = new PartiiRida(p, i++ * 60) { Width = moodneRead.ClientSize.Width - 4 };
            r.Click += (s, e) => { if (valitudRida != null) valitudRida.Valitud = false; valitudRida = r; r.Valitud = true; };
            moodneRead.Controls.Add(r);
        }
        if (i == 0)
            moodneRead.Controls.Add(new Label { Text = Keel.T("Partiisid pole. Lisa uus partii üleval paremal."), AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Pind, Margin = new Padding(10, 16, 0, 0) });
        moodneRead.ResumeLayout();

        // ostja eelvaade: kõige kiiremini aeguv soodustusega partii (või lihtsalt esimene)
        var tana = DateTime.Today;
        var naide = partiid.Where(p => p.Kogus > 0 && (p.Aegub.Date - tana).TotalDays is >= 0 and <= Aegumine.PaeviEnne).FirstOrDefault()
                    ?? partiid.FirstOrDefault(p => p.Kogus > 0 && p.Aegub.Date >= tana);
        eelvaade?.Naita(naide);

        var mahaRead = partiid.Where(p => p.MahaKantud > 0).OrderByDescending(p => p.MahaKantudAeg).Take(8)
            .Select(p => $"{p.MahaKantudAeg:dd.MM}  {p.Toode.Toodenimetus}  −{p.MahaKantud} {Keel.T("tk")}");
        if (lblMoodneMaha != null) lblMoodneMaha.Text = string.Join("\n", mahaRead) is { Length: > 0 } m ? m : Keel.T("Mahakandmisi pole.");
    }

    private void KannaMahaMoodne()
    {
        if (valitudRida?.Partii is not Partii p) { Teade.Show("Vali tabelist partii."); return; }
        if (Teade.Show(Keel.F("Kas kannan maha {0} tk „{1}“?", p.Kogus, p.Toode.Toodenimetus), "Partii", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        using var db = new ToodedContext();
        Aegumine.KannaMaha(db, p.Id);
        LaadiMoodne();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (Teema.OnMoodne && (keyData == Keys.Escape || keyData == (Keys.Alt | Keys.Left))) { MineTagasi(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    // Ühine abi: päevi aegumiseni, soodushind, oleku kiibi tekst ja värvid
    internal static (double Paevi, bool Soodus, decimal Tava, decimal Hind) Hind(Partii p)
    {
        double paevi = (p.Aegub.Date - DateTime.Today).TotalDays;
        decimal tava = Kampaaniad.TavaHind(p.Toode);
        bool soodus = paevi >= 0 && paevi <= Aegumine.PaeviEnne;
        decimal hind = soodus ? Math.Round(tava * (100 - Aegumine.Protsent) / 100m, 2, MidpointRounding.AwayFromZero) : p.Toode.Hind;
        return (paevi, soodus, tava, hind);
    }

    internal static string JaanudTekst(double paevi) => paevi < 0 ? Keel.T("aegunud")
        : paevi == 0 ? Keel.T("täna viimane päev") : paevi == 1 ? Keel.T("veel 1 päev") : Keel.F("veel {0} päeva", paevi);
}

// Tume reegli riba: punane ring, "Automaatne allahindlus", protsent ja olek
public class ReegliRiba : Control
{
    public ReegliRiba()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var tume = Color.FromArgb(27, 28, 31);
        var r = new RectangleF(0, 0, Width - 1, Height - 1);
        using (var tee = Ymardus.Tee(r, 24)) using (var b = new SolidBrush(tume)) g.FillPath(b, tee);
        var ring = new RectangleF(18, (Height - 48) / 2f, 48, 48);
        using (var b = new SolidBrush(Color.FromArgb(226, 6, 19))) g.FillEllipse(b, ring);
        Ikoonid.Joonista(g, "", Rectangle.Round(ring), Color.White, 15f);   // kell
        using var fb = new Font("Segoe UI", 12F, FontStyle.Bold);
        using var f = new Font("Segoe UI", 9.5F);
        TextRenderer.DrawText(g, Keel.T("Automaatne allahindlus"), fb, new Point(82, 18), Color.White);
        TextRenderer.DrawText(g, Keel.F("{0} päeva enne säilivusaja lõppu · müük FEFO", Aegumine.PaeviEnne), f, new Point(84, 44), Color.FromArgb(185, 189, 196));
        // protsendi kiip + "Sees" lüliti (reegel on alati sees)
        using var fk = new Font("Segoe UI", 10F, FontStyle.Bold);
        string pr = $"−{Aegumine.Protsent} %";
        int kw = TextRenderer.MeasureText(pr, fk).Width + 28;
        var lyl = new RectangleF(Width - 18 - 52, (Height - 30) / 2f, 52, 30);
        using (var tee = Ymardus.Tee(lyl, 15)) using (var b = new SolidBrush(Color.FromArgb(20, 130, 70))) g.FillPath(b, tee);
        using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, lyl.Right - 27, lyl.Y + 3, 24, 24);
        var kiip = new RectangleF(lyl.X - 14 - kw, (Height - 36) / 2f, kw, 36);
        using (var tee = Ymardus.Tee(kiip, 18)) using (var b = new SolidBrush(Color.White)) g.FillPath(b, tee);
        TextRenderer.DrawText(g, pr, fk, Rectangle.Round(kiip), tume, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

// Veergude päis: Partii · Kogus · Säilib kuni · Hind · Olek
public class PartiiVeerud : Control
{
    public PartiiVeerud() { SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        using var f = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        var v = PartiiRida.Veerud(Width);
        Color c = Teema.TekstNork;
        TextRenderer.DrawText(g, Keel.T("Partii"), f, new Point(v.Nimi, 4), c);
        TextRenderer.DrawText(g, Keel.T("Kogus"), f, new Point(v.Kogus, 4), c);
        TextRenderer.DrawText(g, Keel.T("Säilib kuni"), f, new Point(v.Aeg, 4), c);
        TextRenderer.DrawText(g, Keel.T("Hind"), f, new Point(v.Hind, 4), c);
        TextRenderer.DrawText(g, Keel.T("Olek"), f, new Point(v.Olek, 4), c);
    }
}

// Partii rida: pilt, nimi + partii nr, kogus, kuupäev + järelejäänud päevad + eluea riba, hind, olek
public class PartiiRida : Control
{
    public Partii Partii { get; }
    private bool valitud, hover;
    private double t;
    public bool Valitud { get => valitud; set { valitud = value; Invalidate(); } }

    public PartiiRida(Partii p, int viivitus)
    {
        Partii = p;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = 68;
        Margin = new Padding(0, 0, 0, 2);
        Cursor = Cursors.Hand;
        Animatsioon.Kaivita(this, 700, x => { t = Animatsioon.Sujuv(x); Invalidate(); }, null, 200 + viivitus);
    }

    internal static (int Nimi, int Kogus, int Aeg, int Hind, int Olek) Veerud(int w)
    {
        int olek = w - 124, hind = olek - 140, aeg = hind - 230, kogus = aeg - 80;
        return (64, kogus, aeg, hind, olek);
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var (paevi, soodus, tava, hind) = AegumineVorm.Hind(Partii);
        bool aegunud = paevi < 0;
        var kast = new RectangleF(0.5f, 1, Width - 2, Height - 3);
        if (valitud || hover)
            using (var tee = Ymardus.Tee(kast, 18))
            {
                using (var b = new SolidBrush(Teema.Paneel2)) g.FillPath(b, tee);
                if (valitud) using (var p = new Pen(Teema.Teine, 2)) g.DrawPath(p, tee);
            }
        int alfa = aegunud ? 150 : 255;
        var v = Veerud(Width);
        var ruut = new RectangleF(10, (Height - 44) / 2f, 44, 44);
        using (var tee = Ymardus.Tee(ruut, 14)) using (var b = new SolidBrush(Teema.Paneel2)) g.FillPath(b, tee);
        if (PildiVahemalu.Too(Partii.Toode.Pilt) is Image pilt)
        {
            float k = Math.Min(36f / pilt.Width, 36f / pilt.Height);
            g.DrawImage(pilt, ruut.X + (44 - pilt.Width * k) / 2, ruut.Y + (44 - pilt.Height * k) / 2, pilt.Width * k, pilt.Height * k);
        }
        using var fb = new Font("Segoe UI", 10F, FontStyle.Bold);
        using var f = new Font("Segoe UI", 8.5F);
        using var fbs = new Font("Segoe UI", 9F, FontStyle.Bold);
        Color tekst = Color.FromArgb(alfa, Teema.Tekst), nork = Color.FromArgb(alfa, Teema.TekstNork), punane = Color.FromArgb(176, 4, 15);
        const TextFormatFlags L = TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;
        TextRenderer.DrawText(g, Partii.Toode.Toodenimetus, fb, new Rectangle(v.Nimi, 14, v.Kogus - v.Nimi - 10, 20), tekst, L);
        TextRenderer.DrawText(g, Keel.F("Partii nr {0}", Partii.Id), f, new Rectangle(v.Nimi, 36, v.Kogus - v.Nimi - 10, 18), nork, L);
        TextRenderer.DrawText(g, $"{Partii.Kogus} {Keel.T("tk")}", fb, new Point(v.Kogus, 24), tekst);

        string kp = Partii.Aegub.ToString("dd.MM") + " · ";
        int kw = TextRenderer.MeasureText(kp, fbs).Width;
        TextRenderer.DrawText(g, kp, fbs, new Point(v.Aeg, 14), tekst);
        TextRenderer.DrawText(g, AegumineVorm.JaanudTekst(paevi), fbs, new Point(v.Aeg + kw - 6, 14), paevi <= Aegumine.PaeviEnne ? punane : Teema.Tekst2);
        var rada = new RectangleF(v.Aeg, 40, 200, 6);
        using (var tee = Ymardus.Tee(rada, 3)) using (var b = new SolidBrush(Teema.Paneel2)) g.FillPath(b, tee);
        double kokku = Math.Max(1, (Partii.Aegub.Date - Partii.Lisatud.Date).TotalDays);
        float osa = aegunud ? 0 : (float)Math.Min(1, paevi / kokku + 0.04) * (float)t;
        Color eluVarv = paevi <= Aegumine.PaeviEnne ? Color.FromArgb(226, 6, 19) : paevi <= 4 ? Color.FromArgb(205, 115, 0) : Color.FromArgb(20, 130, 70);
        if (osa * rada.Width > 3)
            using (var tee = Ymardus.Tee(new RectangleF(rada.X, rada.Y, osa * rada.Width, 6), 3)) using (var b = new SolidBrush(eluVarv)) g.FillPath(b, tee);

        if (aegunud) TextRenderer.DrawText(g, "—", fb, new Point(v.Hind, 24), tekst);
        else
        {
            TextRenderer.DrawText(g, Ostukorv.Eur(hind), fb, new Point(v.Hind, soodus ? 14 : 24), soodus ? Color.FromArgb(226, 6, 19) : tekst);
            if (soodus) using (var fs = new Font("Segoe UI", 8.5F, FontStyle.Strikeout)) TextRenderer.DrawText(g, Ostukorv.Eur(tava), fs, new Point(v.Hind, 36), nork);
        }

        var (olek, taust, esi) = aegunud ? (Keel.T("Aegunud"), Color.FromArgb(27, 28, 31), Color.White)
            : soodus ? ($"−{Aegumine.Protsent} %", Color.FromArgb(226, 6, 19), Color.White)
            : paevi <= 4 ? (Keel.T("Varsti"), Color.FromArgb(255, 243, 224), Color.FromArgb(138, 75, 0))
            : (Keel.T("Värske"), Color.FromArgb(230, 244, 236), Color.FromArgb(17, 104, 58));
        int ow = TextRenderer.MeasureText(olek, fbs).Width + 24;
        var kiip = new RectangleF(v.Olek, (Height - 28) / 2f, ow, 28);
        using (var tee = Ymardus.Tee(kiip, 14)) using (var b = new SolidBrush(taust)) g.FillPath(b, tee);
        TextRenderer.DrawText(g, olek, fbs, Rectangle.Round(kiip), esi, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

// "Nii näeb ostja": hall kaart, valge pildiala punase lindiga, nimi, hind ja järelejäänud aeg
public class OstjaEelvaade : Control
{
    private Partii? p;
    private double t;

    public OstjaEelvaade()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    public void Naita(Partii? uus)
    {
        p = uus;
        Animatsioon.Kaivita(this, 600, x => { t = x; Invalidate(); });
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var kast = new RectangleF(0, 0, Width - 1, Height - 1);
        using (var tee = Ymardus.Tee(kast, 26)) using (var b = new SolidBrush(Teema.Paneel2)) g.FillPath(b, tee);
        using var f = new Font("Segoe UI", 9F);
        if (p == null)
        {
            TextRenderer.DrawText(g, Keel.T("Soodustusega partiisid praegu pole."), f, Rectangle.Round(kast), Teema.TekstNork, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }
        var (paevi, soodus, tava, hind) = AegumineVorm.Hind(p);
        var pildiala = new RectangleF(12, 12, Width - 25, 190);
        using (var tee = Ymardus.Tee(pildiala, 20)) using (var b = new SolidBrush(Color.White)) g.FillPath(b, tee);
        if (PildiVahemalu.Too(p.Toode.Pilt) is Image pilt)
        {
            float k = Math.Min(pildiala.Width * 0.7f / pilt.Width, 160f / pilt.Height);
            g.DrawImage(pilt, pildiala.X + (pildiala.Width - pilt.Width * k) / 2, pildiala.Y + (190 - pilt.Height * k) / 2, pilt.Width * k, pilt.Height * k);
        }
        using var fb = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        if (soodus)
        {
            // lint libiseb vasakult sisse
            string lint = Keel.F("−{0} % · parim enne {1:dd.MM}", Aegumine.Protsent, p.Aegub);
            using var fl = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            int lw = TextRenderer.MeasureText(lint, fl).Width + 30;
            float lx = -lw + (float)Animatsioon.Sujuv(t) * (lw + 2);
            var r = new RectangleF(lx, 30, lw, 34);
            using (var b = new SolidBrush(Color.FromArgb(226, 6, 19))) g.FillRectangle(b, r);
            TextRenderer.DrawText(g, lint, fl, Rectangle.Round(r), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        TextRenderer.DrawText(g, p.Toode.Toodenimetus, fb, new Rectangle(14, 212, Width - 28, 40), Teema.Tekst, TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
        using var fh = new Font("Segoe UI", 19F, FontStyle.Bold);
        string hs = Ostukorv.Eur(hind);
        TextRenderer.DrawText(g, hs, fh, new Point(12, 254), soodus ? Color.FromArgb(226, 6, 19) : Teema.Tekst);
        if (soodus)
            using (var fs = new Font("Segoe UI", 9.5F, FontStyle.Strikeout))
                TextRenderer.DrawText(g, Ostukorv.Eur(tava), fs, new Point(20 + TextRenderer.MeasureText(hs, fh).Width, 266), Teema.TekstNork);
        using (var b = new SolidBrush(Color.FromArgb(205, 115, 0))) g.FillEllipse(b, 16, 304, 8, 8);
        using var fo = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        TextRenderer.DrawText(g, Keel.F("Säilib: {0} · {1} tk", AegumineVorm.JaanudTekst(paevi), p.Kogus), fo, new Point(30, 299), Color.FromArgb(138, 75, 0));
    }
}

// Uus partii (väike aken): toode, kogus, aegumise kuupäev
public class UusPartiiVorm : KujundusVorm
{
    private readonly ComboBox cmbToode;
    private readonly NumericUpDown numKogus;
    private readonly DateTimePicker dtAegub;

    public UusPartiiVorm() : base("Uus partii", 420, 300)
    {
        Silt("Toode", 20, 16);
        cmbToode = new ComboBox { Location = new Point(20, 38), Width = 380, DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems, Font = new Font("Segoe UI", 10F), DisplayMember = "Value" };
        Controls.Add(cmbToode);
        Silt("Kogus", 20, 80);
        numKogus = new NumericUpDown { Location = new Point(20, 102), Width = 120, Minimum = 1, Maximum = 9999, Value = 10, Font = new Font("Segoe UI", 10F) };
        Controls.Add(numKogus);
        Silt("Aegub", 160, 80);
        dtAegub = new DateTimePicker { Location = new Point(160, 102), Width = 240, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy", Value = DateTime.Today.AddDays(5), Font = new Font("Segoe UI", 10F) };
        Controls.Add(dtAegub);
        var lisa = UiStiil.LooSuurNupp("Lisa partii", (s, e) => Lisa());
        lisa.Bounds = new Rectangle(20, 160, 380, 48);
        Controls.Add(lisa);
        AcceptButton = lisa;
        Load += (s, e) =>
        {
            using var db = new ToodedContext();
            foreach (var t in db.Toodetabel.AsNoTracking().OrderBy(t => t.Toodenimetus).Select(t => new { t.Id, t.Toodenimetus }).ToList())
                cmbToode.Items.Add(new KeyValuePair<int, string>(t.Id, t.Toodenimetus));
        };
    }

    private void Lisa()
    {
        if (cmbToode.SelectedItem is not KeyValuePair<int, string> valik) { Teade.Show("Vali toode.", "Partii"); return; }
        try
        {
            using var db = new ToodedContext();
            Aegumine.LisaPartii(db, valik.Key, (int)numKogus.Value, dtAegub.Value);
            Teade.Toast(Keel.F("Partii lisatud: {0} · {1} tk", valik.Value, numKogus.Value));
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { Teade.Show("Viga:\n" + ex.Message); }
    }
}
