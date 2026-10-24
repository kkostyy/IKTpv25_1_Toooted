using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Partiid ja säilivusaeg (omanik): ajatelg 14 päeva, partiide tabel
//  (roheline / kollane ≤ 2 päeva / punane aegunud), uus partii, mahakandmine.
// ===================================================================
public class AegumineVorm : KujundusVorm
{
    private readonly PartiiTelg telg = new() { Dock = DockStyle.Fill };
    private readonly DataGridView vork;
    private readonly ComboBox cmbToode;
    private readonly NumericUpDown numKogus;
    private readonly DateTimePicker dtAegub;
    private readonly Label lblMaha;
    private List<Partii> partiid = new();

    public AegumineVorm() : base("Partiid ja säilivusaeg", 1120, 720)
    {
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(960, 600);
        BackColor = Teema.Taust;

        var yla = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Teema.Taust };
        yla.Controls.Add(new Label { Text = "Partiid ja säilivusaeg", Location = new Point(18, 14), AutoSize = true, Font = UiStiil.PealkirjaFont, ForeColor = Teema.Tekst, BackColor = Teema.Taust });
        yla.Controls.Add(new Label { Text = Keel.F("{0} päeva enne aegumist −{1} % · aegunud kantakse maha · müük FEFO", Aegumine.PaeviEnne, Aegumine.Protsent), Location = new Point(300, 24), AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Taust });

        // Paremal: uus partii + mahakantud
        var parem = new Panel { Dock = DockStyle.Right, Width = 330, Padding = new Padding(0, 0, 16, 16), BackColor = Teema.Taust };
        var vorm = new Panel { BackColor = Teema.Pind };
        vorm.Controls.Add(new Label { Text = "Toode", Location = new Point(4, 0), AutoSize = true, ForeColor = Teema.Tekst2, BackColor = Teema.Pind });
        cmbToode = new ComboBox { Location = new Point(4, 20), Width = 280, DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems, Font = new Font("Segoe UI", 10F) };
        vorm.Controls.Add(cmbToode);
        vorm.Controls.Add(new Label { Text = "Kogus", Location = new Point(4, 60), AutoSize = true, ForeColor = Teema.Tekst2, BackColor = Teema.Pind });
        numKogus = new NumericUpDown { Location = new Point(4, 80), Width = 100, Minimum = 1, Maximum = 9999, Value = 10, Font = new Font("Segoe UI", 10F) };
        vorm.Controls.Add(numKogus);
        vorm.Controls.Add(new Label { Text = "Aegub", Location = new Point(124, 60), AutoSize = true, ForeColor = Teema.Tekst2, BackColor = Teema.Pind });
        dtAegub = new DateTimePicker { Location = new Point(124, 80), Width = 160, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy", Value = DateTime.Today.AddDays(5), Font = new Font("Segoe UI", 10F) };
        vorm.Controls.Add(dtAegub);
        var lisa = UiStiil.LooSuurNupp("Lisa partii", (s, e) => LisaPartii());
        lisa.Bounds = new Rectangle(4, 124, 280, 44);
        vorm.Controls.Add(lisa);
        var maha = (RoundedButton)UiStiil.LooNupp("Kanna valitud maha", (s, e) => KannaMaha());
        maha.Ohtlik = true;
        maha.Location = new Point(4, 182);
        vorm.Controls.Add(maha);
        vorm.Controls.Add(new Label { Text = "MAHA KANTUD", Location = new Point(4, 236), AutoSize = true, Font = UiStiil.AlapealkiriFont, ForeColor = Teema.TekstNork, BackColor = Teema.Pind });
        lblMaha = new Label { Location = new Point(4, 258), Size = new Size(280, 220), ForeColor = Teema.Tekst2, BackColor = Teema.Pind };
        vorm.Controls.Add(lblMaha);
        var vormiKaart = UiStiil.LooKaardiPaneel("Uus partii", vorm);
        vormiKaart.Dock = DockStyle.Fill;
        parem.Controls.Add(vormiKaart);

        vork = new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        vork.Columns.Add("Toode", "Toode");
        vork.Columns.Add("Kogus", "Kogus");
        vork.Columns.Add("Aegub", "Aegub");
        vork.Columns.Add("Olek", "Olek");
        vork.Columns["Toode"]!.FillWeight = 220;
        vork.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || vork.Rows[e.RowIndex].Tag is not Partii p || e.CellStyle == null || vork.Columns[e.ColumnIndex].Name != "Olek") return;
            e.CellStyle.ForeColor = Varv(p);
            e.CellStyle.Font = new Font(vork.Font, FontStyle.Bold);
        };

        var telgKaart = UiStiil.LooKaardiPaneel("Ajatelg · 14 päeva", telg);
        telgKaart.Dock = DockStyle.Top;
        telgKaart.Height = 200;
        var tabeliKaart = UiStiil.LooKaardiPaneel("Partiid laos", vork);
        tabeliKaart.Dock = DockStyle.Fill;
        var keskel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 16), BackColor = Teema.Taust };
        keskel.Controls.Add(tabeliKaart);
        keskel.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Teema.Taust });
        keskel.Controls.Add(telgKaart);

        Controls.Add(keskel);
        Controls.Add(parem);
        Controls.Add(yla);
        keskel.BringToFront();
        Load += (s, e) => Laadi();
    }

    public static Color Varv(Partii p)
    {
        double paevi = (p.Aegub.Date - DateTime.Today).TotalDays;
        return paevi < 0 ? Teema.Viga : paevi <= Aegumine.PaeviEnne ? Teema.Hoiatus : Teema.Edu;
    }

    private void Laadi()
    {
        using var db = new ToodedContext();
        int maha = Aegumine.Uuenda(db, DateTime.Now);
        if (maha > 0) Teade.Toast(Keel.F("Aegunud kaupa maha kantud: {0} tk", maha), TeateTyyp.Hoiatus);

        if (cmbToode.Items.Count == 0)
            foreach (var t in db.Toodetabel.AsNoTracking().OrderBy(t => t.Toodenimetus).Select(t => new { t.Id, t.Toodenimetus }).ToList())
                cmbToode.Items.Add(new KeyValuePair<int, string>(t.Id, t.Toodenimetus));
        cmbToode.DisplayMember = "Value";

        partiid = db.Partiid.Include(p => p.Toode).AsNoTracking().OrderBy(p => p.Aegub).ToList();
        vork.Rows.Clear();
        foreach (var p in partiid.Where(p => p.Kogus > 0))
        {
            double paevi = (p.Aegub.Date - DateTime.Today).TotalDays;
            string olek = paevi < 0 ? Keel.T("Aegunud") : paevi <= Aegumine.PaeviEnne ? Keel.F("−{0} % · {1} p", Aegumine.Protsent, paevi) : Keel.F("{0} päeva", paevi);
            int i = vork.Rows.Add(p.Toode.Toodenimetus, p.Kogus, p.Aegub.ToString("dd.MM.yyyy"), olek);
            vork.Rows[i].Tag = p;
        }
        telg.Naita(partiid.Where(p => p.Kogus > 0).ToList());
        var mahaRead = partiid.Where(p => p.MahaKantud > 0).OrderByDescending(p => p.MahaKantudAeg).Take(10)
            .Select(p => $"{p.MahaKantudAeg:dd.MM}  {p.Toode.Toodenimetus}  −{p.MahaKantud} tk");
        lblMaha.Text = string.Join("\n", mahaRead) is { Length: > 0 } m ? m : Keel.T("Mahakandmisi pole.");
    }

    private void LisaPartii()
    {
        if (cmbToode.SelectedItem is not KeyValuePair<int, string> valik) { Teade.Show("Vali toode.", "Partii"); return; }
        try
        {
            using var db = new ToodedContext();
            Aegumine.LisaPartii(db, valik.Key, (int)numKogus.Value, dtAegub.Value);
            Teade.Toast(Keel.F("Partii lisatud: {0} · {1} tk", valik.Value, numKogus.Value));
            Laadi();
        }
        catch (Exception ex) { Teade.Show("Viga:\n" + ex.Message); }
    }

    private void KannaMaha()
    {
        if (vork.CurrentRow?.Tag is not Partii p) { Teade.Show("Vali tabelist partii."); return; }
        if (Teade.Show(Keel.F("Kas kannan maha {0} tk „{1}“?", p.Kogus, p.Toode.Toodenimetus), "Partii", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        using var db = new ToodedContext();
        Aegumine.KannaMaha(db, p.Id);
        Laadi();
    }
}

// 14 päeva ajatelg: iga partii on täpp oma päeval (roheline / kollane / punane), täpid ilmuvad järjest
public class PartiiTelg : Control
{
    private List<Partii> partiid = new();
    private double t = 1;

    public PartiiTelg()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    public void Naita(List<Partii> uued)
    {
        partiid = uued;
        Animatsioon.Kaivita(this, 800, x => { t = x; Invalidate(); });
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var f = new Font("Segoe UI", 8F);
        using var fb = new Font("Segoe UI Semibold", 7.5F);
        float w = (Width - 2) / 15f;
        var tana = DateTime.Today;
        for (int d = -1; d < 14; d++)
        {
            float x = (d + 1) * w;
            var paev = tana.AddDays(d);
            Color taust = d < 0 ? Teema.VigaTaust : d <= Aegumine.PaeviEnne ? Ymardus.Segu(Teema.Pind, Teema.Hoiatus, 0.12f) : Teema.PindAlt;
            using (var b = new SolidBrush(taust)) g.FillRectangle(b, x + 1, 18, w - 2, Height - 18);
            TextRenderer.DrawText(g, d == 0 ? Keel.T("Täna") : paev.ToString("dd.MM"), d == 0 ? fb : f, new Rectangle((int)x, 0, (int)w, 16), d == 0 ? Teema.Aktsent : Teema.TekstNork, TextFormatFlags.HorizontalCenter);
            var paeva = partiid.Where(p => d < 0 ? p.Aegub.Date < tana : p.Aegub.Date == paev).ToList();
            for (int i = 0; i < paeva.Count && i < 6; i++)
            {
                double s = Animatsioon.Pop(Math.Clamp(t * 1.6 - (d + 1) * 0.04, 0, 1));
                float r = (float)(9 * s);
                var c = new PointF(x + w / 2, 34 + i * 24);
                using var b = new SolidBrush(AegumineVorm.Varv(paeva[i]));
                g.FillEllipse(b, c.X - r, c.Y - r, r * 2, r * 2);
                TextRenderer.DrawText(g, paeva[i].Kogus.ToString(), fb, Rectangle.Round(new RectangleF(c.X - 12, c.Y - 9, 24, 18)), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
