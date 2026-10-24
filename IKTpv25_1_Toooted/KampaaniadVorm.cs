using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Kampaaniad (omanik): ülal filter, ajatelg (4 nädalat), tabel;
//  paremal "Uus kampaania" – toode, soodustus, periood, eelvaade.
// ===================================================================
public class KampaaniadVorm : KujundusVorm
{
    private readonly KampaaniaTelg telg = new() { Dock = DockStyle.Top, Height = 200 };
    private readonly DataGridView vork;
    private readonly FlowLayoutPanel filtrid;
    private readonly MoodneFilter? moodneFilter;   // moodne: vahelehed valgel "pillil" (lõuend N2)
    private readonly Label? lblOlek;
    private readonly TextBox txtNimi;
    private readonly ComboBox cmbToode;
    private readonly TrackBar riba;
    private readonly Label lblProtsent, lblEelvaade, lblVana, lblVorm;
    private readonly DateTimePicker dtAlgus, dtLopp;
    private readonly CheckBox chkAuto;
    private readonly RoundedButton btnLopeta, btnKustuta;

    private List<Kampaania> koik = new();
    private List<Toode> tooted = new();
    private KampaaniaOlek? filter;
    private int? muudetav;          // valitud kampaania Id (null = uus)

    public KampaaniadVorm() : base("Kampaaniad", 1200, 760)
    {
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(1080, 680);
        BackColor = Teema.Taust;

        // ---- Ülal: pealkiri + filtri "pillid" ----
        var yla = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Teema.Taust };
        yla.Controls.Add(new Label { Text = "Kampaaniad", Location = new Point(18, 14), AutoSize = true, Font = UiStiil.PealkirjaFont, ForeColor = Teema.Tekst, BackColor = Teema.Taust });
        filtrid = new FlowLayoutPanel { Location = new Point(200, 16), AutoSize = true, WrapContents = false, BackColor = Teema.Taust };
        yla.Controls.Add(filtrid);
        if (Teema.OnMoodne)
        {
            yla.Height = 76;
            filtrid.Visible = false;
            moodneFilter = new MoodneFilter { Anchor = AnchorStyles.Top | AnchorStyles.Right, Top = 14 };
            moodneFilter.Valitud += i => { filter = i switch { 1 => KampaaniaOlek.Kaib, 2 => KampaaniaOlek.Tulekul, 3 => KampaaniaOlek.Loppenud, _ => null }; Laadi(); };
            moodneFilter.Resize += (s, e) => moodneFilter.Left = yla.Width - moodneFilter.Width - 16;
            yla.Resize += (s, e) => moodneFilter.Left = yla.Width - moodneFilter.Width - 16;
            yla.Controls.Add(moodneFilter);
        }

        // ---- Paremal: uus / muudetav kampaania ----
        var parem = new Panel { Dock = DockStyle.Right, Width = 380, Padding = new Padding(0, 0, 16, 16), BackColor = Teema.Taust };
        var vorm = new Panel { BackColor = Teema.Pind };
        int x = 4, w = 330, y = 0;
        lblVorm = new Label { Text = "Uus kampaania", Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI Semibold", 13F), ForeColor = Teema.Tekst, BackColor = Teema.Pind };
        vorm.Controls.Add(lblVorm);
        if (Teema.OnMoodne)
        {
            lblVorm.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblOlek = new Label { AutoSize = false, Size = new Size(84, 26), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(160, 35, 110), BackColor = Color.FromArgb(246, 230, 239) };
            using (var t = Ymardus.Tee(new RectangleF(0, 0, lblOlek.Width, lblOlek.Height), 13)) lblOlek.Region = new Region(t);   // ümar kiip
            lblOlek.Location = new Point(x + w - 84 - 70, y + 6);
            vorm.Controls.Add(lblOlek);
        }
        var uus = new IkoonTekst(Ikoonid.Pluss, "Uus", Teema.Teine, new Font("Segoe UI Semibold", 9.5F)) { BackColor = Teema.Pind };
        uus.Location = new Point(x + w - uus.Width, y + 4);
        uus.Click += (s, e) => Tyhjenda();
        vorm.Controls.Add(uus);

        y += 40;
        vorm.Controls.Add(VormiSilt("Nimi", x, y));
        var nimiRaam = UiStiil.LooSisend(out txtNimi, w, false, "nt Nädala leivapakkumine");
        nimiRaam.Location = new Point(x, y + 20);
        vorm.Controls.Add(nimiRaam);

        y += 66;
        vorm.Controls.Add(VormiSilt("Toode", x, y));
        cmbToode = new ComboBox
        {
            Location = new Point(x, y + 22),
            Width = w,
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems,
            Font = new Font("Segoe UI", 10F),
            FlatStyle = FlatStyle.Flat
        };
        cmbToode.SelectedIndexChanged += (s, e) => Eelvaade();
        vorm.Controls.Add(cmbToode);

        y += 62;
        vorm.Controls.Add(VormiSilt("Soodustus", x, y));
        lblProtsent = new Label { Text = "−30%", Location = new Point(x + w - 120, y - 6), Size = new Size(120, 44), TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = Teema.Aktsent, BackColor = Teema.Pind };
        vorm.Controls.Add(lblProtsent);
        riba = new TrackBar { Location = new Point(x - 4, y + 38), Width = w + 8, Minimum = 5, Maximum = 70, Value = 30, TickFrequency = 5, SmallChange = 1, LargeChange = 5, BackColor = Teema.Pind };
        riba.ValueChanged += (s, e) => { lblProtsent.Text = $"−{riba.Value}%"; Eelvaade(); };
        vorm.Controls.Add(riba);

        y += 92;
        vorm.Controls.Add(VormiSilt("Algus", x, y));
        vorm.Controls.Add(VormiSilt("Lõpp", x + w / 2 + 6, y));
        dtAlgus = Kuupaev(x, y + 22, w / 2 - 6);
        dtLopp = Kuupaev(x + w / 2 + 6, y + 22, w / 2 - 6);
        vorm.Controls.Add(dtAlgus);
        vorm.Controls.Add(dtLopp);

        // Tume eelvaade: kuidas poes välja näeb
        y += 64;
        var eel = new RoundedPanel { Location = new Point(x, y), Size = new Size(w, 74), Radius = 16, FillColor = Color.FromArgb(27, 28, 31), BorderColor = Color.FromArgb(27, 28, 31), BackColor = Teema.Pind };
        eel.Controls.Add(new Label { Text = "POES NÄEB VÄLJA", Location = new Point(14, 10), AutoSize = true, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold), ForeColor = Color.FromArgb(170, 175, 182), BackColor = Color.FromArgb(27, 28, 31) });
        lblEelvaade = new Label { Location = new Point(14, 30), AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(27, 28, 31) };
        lblVana = new Label { AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Strikeout), ForeColor = Color.FromArgb(170, 175, 182), BackColor = Color.FromArgb(27, 28, 31) };
        lblEelvaade.SizeChanged += (s, e) => lblVana.Location = new Point(lblEelvaade.Right + 8, 40);
        eel.Controls.Add(lblEelvaade);
        eel.Controls.Add(lblVana);
        vorm.Controls.Add(eel);

        y += 86;
        chkAuto = new CheckBox { Text = "Automaatne (hind muutub alguses ja lõpus ise)", Location = new Point(x, y), AutoSize = true, Checked = true, ForeColor = Teema.Tekst2, BackColor = Teema.Pind };
        vorm.Controls.Add(chkAuto);

        y += 36;
        var salvesta = UiStiil.LooSuurNupp("Salvesta kampaania", (s, e) => Salvesta());
        salvesta.Location = new Point(x, y);
        salvesta.Width = w;
        vorm.Controls.Add(salvesta);
        y += 54;
        btnLopeta = (RoundedButton)UiStiil.LooNupp("Lõpeta kohe", (s, e) => Lopeta());
        btnLopeta.Location = new Point(x, y);
        btnKustuta = (RoundedButton)UiStiil.LooNupp("Kustuta", (s, e) => Kustuta());
        btnKustuta.Ohtlik = true;
        btnKustuta.Location = new Point(x + 140, y);
        vorm.Controls.Add(btnLopeta);
        vorm.Controls.Add(btnKustuta);

        var vormiKaart = UiStiil.LooKaardiPaneel(Teema.OnMoodne ? "" : "Haldus", vorm);
        vormiKaart.Dock = DockStyle.Fill;
        parem.Controls.Add(vormiKaart);

        // ---- Keskel: ajatelg + tabel ----
        vork = new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowTemplate = { Height = 48 }
        };
        vork.Columns.Add(new DataGridViewImageColumn { Name = "Pilt", HeaderText = "", ImageLayout = DataGridViewImageCellLayout.Zoom, FillWeight = 12, DefaultCellStyle = { NullValue = null } });
        vork.Columns.Add("Nimi", "Kampaania · toode");
        vork.Columns.Add("Protsent", "Soodustus");
        vork.Columns.Add("Periood", "Periood");
        vork.Columns.Add("Hind", "Uus / vana hind");
        vork.Columns.Add("Olek", "Olek");
        vork.Columns["Nimi"]!.FillWeight = 60;
        vork.Columns["Protsent"]!.FillWeight = 18;
        vork.Columns["Periood"]!.FillWeight = 40;
        vork.Columns["Hind"]!.FillWeight = 30;
        vork.Columns["Olek"]!.FillWeight = 22;
        vork.CellClick += (s, e) => { if (e.RowIndex >= 0 && vork.Rows[e.RowIndex].Tag is int id) Vali(id); };
        vork.CellPainting += OlekuKiip;
        if (Teema.OnMoodne)
        {
            vork.RowTemplate.Height = 62;
            vork.CellPainting += MoodneRakk;
        }

        var tabeliKaart = UiStiil.LooKaardiPaneel("Kõik kampaaniad", vork);
        tabeliKaart.Dock = DockStyle.Fill;
        var telgKaart = UiStiil.LooKaardiPaneel(Teema.OnMoodne ? Keel.F("Ajatelg · {0:dd.MM} – {1:dd.MM}", TelgAlgus, TelgAlgus.AddDays(27)) : "Ajatelg · 4 nädalat", telg);
        telgKaart.Dock = DockStyle.Top;
        telgKaart.Height = 236;
        var keskel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 16), BackColor = Teema.Taust };
        keskel.Controls.Add(tabeliKaart);
        keskel.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Teema.Taust });
        keskel.Controls.Add(telgKaart);

        Controls.Add(keskel);
        Controls.Add(parem);
        Controls.Add(yla);
        keskel.BringToFront();

        // "Käib" punkt vilgub tabelis
        Animatsioon.Tsykkel(vork, s => { vilgu = (int)(s * 2) % 2 == 0; vork.InvalidateColumn(vork.Columns["Olek"]!.Index); }, 500);

        Load += (s, e) => { LaadiTooted(); Laadi(); Tyhjenda(); };
    }

    private bool vilgu = true;

    private static DateTime TelgAlgus
    {
        get { var n = DateTime.Now; return n.Date.AddDays(-(((int)n.DayOfWeek + 6) % 7) - 7); }
    }

    // Moodne tabel: nimi paksult + toode hallilt, protsent punaselt, uus hind + vana läbi kriipsutatult
    private void MoodneRakk(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Graphics == null) return;
        string nimi = vork.Columns[e.ColumnIndex].Name;
        if (nimi is not ("Nimi" or "Protsent" or "Hind")) return;
        e.PaintBackground(e.CellBounds, true);
        var g = e.Graphics;
        var r = e.CellBounds;
        bool valitud = (e.State & DataGridViewElementStates.Selected) != 0;
        Color tekst = valitud ? (e.CellStyle?.SelectionForeColor ?? Teema.Tekst) : Teema.Tekst;
        Color nork = valitud ? tekst : Teema.TekstNork;
        string v = e.Value?.ToString() ?? "";
        using var fb = new Font("Segoe UI", 10F, FontStyle.Bold);
        using var f = new Font("Segoe UI", 8.5F);
        const TextFormatFlags L = TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;
        switch (nimi)
        {
            case "Nimi":
                var osad = v.Split('\n');
                TextRenderer.DrawText(g, osad[0], fb, new Rectangle(r.X + 6, r.Y + 8, r.Width - 10, 22), tekst, L);
                if (osad.Length > 1) TextRenderer.DrawText(g, osad[1], f, new Rectangle(r.X + 6, r.Y + 30, r.Width - 10, 20), nork, L);
                break;
            case "Protsent":
                using (var fp = new Font("Segoe UI", 11F, FontStyle.Bold))
                    TextRenderer.DrawText(g, v, fp, new Rectangle(r.X + 6, r.Y, r.Width - 8, r.Height), valitud ? tekst : Color.FromArgb(226, 6, 19), L);
                break;
            case "Hind":
                var h = v.Split('/');
                TextRenderer.DrawText(g, h[0].Trim(), fb, new Rectangle(r.X + 6, r.Y + 8, r.Width - 10, 22), tekst, L);
                if (h.Length > 1)
                    using (var fs = new Font("Segoe UI", 8.5F, FontStyle.Strikeout))
                        TextRenderer.DrawText(g, h[1].Trim(), fs, new Rectangle(r.X + 6, r.Y + 30, r.Width - 10, 20), nork, L);
                break;
        }
        e.Handled = true;
    }

    private static Label VormiSilt(string tekst, int x, int y) =>
        new() { Text = tekst, Location = new Point(x, y), AutoSize = true, ForeColor = Teema.Tekst2, BackColor = Teema.Pind };

    private static DateTimePicker Kuupaev(int x, int y, int w) => new()
    {
        Location = new Point(x, y),
        Width = w,
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "dd.MM.yyyy HH:mm",
        Font = new Font("Segoe UI", 10F)
    };

    // Number "hüppab" muutmisel (font kasvab ja tuleb tagasi)
    private System.Windows.Forms.Timer? popAnim;
    private void Pop(Label l)
    {
        popAnim?.Stop();
        popAnim = Animatsioon.Kaivita(l, 260, t =>
        {
            float k = 24F * (float)(1 + 0.18 * Math.Sin(Math.PI * t));
            var vana = l.Font;
            l.Font = new Font("Segoe UI", k, FontStyle.Bold);
            vana.Dispose();
        });
    }

    private void LaadiTooted()
    {
        using var db = new ToodedContext();
        tooted = db.Toodetabel.AsNoTracking().OrderBy(t => t.Toodenimetus).ToList();
        cmbToode.Items.Clear();
        foreach (var t in tooted) cmbToode.Items.Add(new TooteValik(t));
    }

    private sealed record TooteValik(Toode Toode)
    {
        public override string ToString() => Toode.Toodenimetus;
    }

    private void Laadi()
    {
        using var db = new ToodedContext();
        Kampaaniad.Uuenda(db, DateTime.Now);
        koik = db.Kampaaniad.Include(k => k.Toode).AsNoTracking().OrderBy(k => k.Algus).ToList();
        var nyyd = DateTime.Now;

        // Filtri "pillid" koos arvudega
        filtrid.SuspendLayout();
        foreach (Control c in filtrid.Controls.Cast<Control>().ToList()) c.Dispose();
        void Pill(string nimi, KampaaniaOlek? o)
        {
            int arv = o == null ? koik.Count : koik.Count(k => Kampaaniad.Olek(k, nyyd) == o);
            var b = (RoundedButton)UiStiil.LooNupp($"{Keel.T(nimi)}  {arv}", (s, e) => { filter = o; Laadi(); }, filter == o);
            b.Radius = 16;
            b.MinimumSize = new Size(0, 34);
            filtrid.Controls.Add(b);
        }
        Pill("Kõik", null);
        Pill("Käib", KampaaniaOlek.Kaib);
        Pill("Tulekul", KampaaniaOlek.Tulekul);
        Pill("Lõppenud", KampaaniaOlek.Loppenud);
        filtrid.ResumeLayout();
        moodneFilter?.Sea(new[]
        {
            ("Kõik", koik.Count),
            ("Käib", koik.Count(k => Kampaaniad.Olek(k, nyyd) == KampaaniaOlek.Kaib)),
            ("Tulekul", koik.Count(k => Kampaaniad.Olek(k, nyyd) == KampaaniaOlek.Tulekul)),
            ("Lõppenud", koik.Count(k => Kampaaniad.Olek(k, nyyd) == KampaaniaOlek.Loppenud))
        }, filter switch { KampaaniaOlek.Kaib => 1, KampaaniaOlek.Tulekul => 2, KampaaniaOlek.Loppenud => 3, _ => 0 });

        var naha = koik.Where(k => filter == null || Kampaaniad.Olek(k, nyyd) == filter).ToList();
        telg.Naita(naha, nyyd);

        vork.Rows.Clear();
        foreach (var k in naha)
        {
            decimal tava = Kampaaniad.TavaHind(k.Toode);
            decimal uusHind = Kampaaniad.SoodusHind(tava, k.SoodusProtsent);
            int i = vork.Rows.Add(PildiVahemalu.Too(k.Toode.Pilt), $"{k.Nimi}\n{k.Toode.Toodenimetus}", $"−{k.SoodusProtsent}%",
                $"{k.Algus:dd.MM HH:mm} – {k.Lopp:dd.MM HH:mm}", $"{Ostukorv.Eur(uusHind)}  /  {Ostukorv.Eur(tava)}",
                Keel.T(Kampaaniad.OlekuNimi(Kampaaniad.Olek(k, nyyd))));
            vork.Rows[i].Tag = k.Id;
        }
        vork.Columns["Nimi"]!.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
    }

    // Oleku kiip: värviline punkt + tekst (Käib punkt vilgub)
    private void OlekuKiip(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != vork.Columns["Olek"]!.Index || e.Graphics == null) return;
        e.PaintBackground(e.CellBounds, true);
        string tekst = e.Value?.ToString() ?? "";
        Color varv = tekst == Keel.T("Käib") ? Teema.Aktsent : tekst == Keel.T("Tulekul") ? Teema.Teine : Teema.TekstNork;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var f = e.CellStyle?.Font ?? Font;
        int tw = TextRenderer.MeasureText(tekst, f).Width + 30;
        var r = new Rectangle(e.CellBounds.X + 6, e.CellBounds.Y + (e.CellBounds.Height - 26) / 2, tw, 26);
        using (var tee = Ymardus.Tee(r, 13))
        using (var b = new SolidBrush(Ymardus.Segu(Teema.Pind, varv, 0.14f)))
            g.FillPath(b, tee);
        bool naita = tekst != Keel.T("Käib") || vilgu;
        if (naita)
            using (var b = new SolidBrush(varv))
                g.FillEllipse(b, r.X + 10, r.Y + 9, 8, 8);
        TextRenderer.DrawText(g, tekst, f, new Rectangle(r.X + 24, r.Y, r.Width - 24, r.Height), varv, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        e.Handled = true;
    }

    private Toode? ValitudToode => (cmbToode.SelectedItem as TooteValik)?.Toode
        ?? tooted.FirstOrDefault(t => t.Toodenimetus.Equals(cmbToode.Text.Trim(), StringComparison.CurrentCultureIgnoreCase));

    private void Eelvaade()
    {
        if (ValitudToode is not Toode t) { lblEelvaade.Text = "—"; lblVana.Text = ""; return; }
        decimal tava = Kampaaniad.TavaHind(t);
        lblEelvaade.Text = Ostukorv.Eur(Kampaaniad.SoodusHind(tava, riba.Value));
        lblVana.Text = Ostukorv.Eur(tava);
    }

    private void Tyhjenda()
    {
        muudetav = null;
        lblVorm.Text = Keel.T("Uus kampaania");
        if (lblOlek != null) lblOlek.Text = Keel.T("Mustand");
        txtNimi.Text = "";
        cmbToode.SelectedIndex = -1;
        cmbToode.Text = "";
        riba.Value = 30;
        var nyyd = DateTime.Now;
        dtAlgus.Value = new DateTime(nyyd.Year, nyyd.Month, nyyd.Day, nyyd.Hour, 0, 0);
        dtLopp.Value = dtAlgus.Value.AddDays(7);
        chkAuto.Checked = true;
        btnLopeta.Enabled = btnKustuta.Enabled = false;
        Eelvaade();
    }

    private void Vali(int id)
    {
        var k = koik.FirstOrDefault(x => x.Id == id);
        if (k == null) return;
        muudetav = id;
        lblVorm.Text = Keel.T("Muuda kampaaniat");
        if (lblOlek != null) lblOlek.Text = Keel.T(Kampaaniad.OlekuNimi(Kampaaniad.Olek(k, DateTime.Now)));
        txtNimi.Text = k.Nimi;
        cmbToode.SelectedItem = cmbToode.Items.Cast<TooteValik>().FirstOrDefault(v => v.Toode.Id == k.ToodeId);
        riba.Value = Math.Clamp(k.SoodusProtsent, riba.Minimum, riba.Maximum);
        dtAlgus.Value = k.Algus;
        dtLopp.Value = k.Lopp;
        chkAuto.Checked = k.Automaatne;
        btnLopeta.Enabled = Kampaaniad.Olek(k, DateTime.Now) != KampaaniaOlek.Loppenud;
        btnKustuta.Enabled = true;
        Eelvaade();
    }

    private void Salvesta()
    {
        if (ValitudToode is not Toode t) { Teade.Show("Vali toode.", "Kampaania"); return; }
        try
        {
            using var db = new ToodedContext();
            var k = muudetav is int id ? db.Kampaaniad.Include(x => x.Toode).First(x => x.Id == id) : new Kampaania();
            if (k.Rakendatud) Kampaaniad.Taasta(k);   // vana soodustus maha (ka teiselt tootelt)
            k.Nimi = txtNimi.Text.Trim();
            k.ToodeId = t.Id;
            k.Toode = db.Toodetabel.Find(t.Id)!;
            k.SoodusProtsent = riba.Value;
            k.Algus = dtAlgus.Value;
            k.Lopp = dtLopp.Value;
            k.Automaatne = chkAuto.Checked;
            string? viga = Kampaaniad.Kontrolli(db, k);
            if (viga != null) { Teade.Show(viga, "Kampaania", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            Kampaaniad.Salvesta(db, k);
            Teade.Toast(Keel.F("Kampaania „{0}“ salvestatud", k.Nimi));
            Laadi();
            Vali(k.Id);
        }
        catch (Exception ex) { Teade.Show("Salvestamine ebaõnnestus:\n" + ex.Message); }
    }

    private void Lopeta()
    {
        if (muudetav is not int id) return;
        using var db = new ToodedContext();
        var k = db.Kampaaniad.Include(x => x.Toode).FirstOrDefault(x => x.Id == id);
        if (k == null) return;
        Kampaaniad.Lopeta(db, k);
        Teade.Toast(Keel.F("Kampaania „{0}“ lõpetatud – hind on tagasi", k.Nimi), TeateTyyp.Info);
        Laadi();
        Vali(id);
    }

    private void Kustuta()
    {
        if (muudetav is not int id) return;
        if (Teade.Show("Kas kustutan kampaania?", "Kampaania", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        using var db = new ToodedContext();
        var k = db.Kampaaniad.Include(x => x.Toode).FirstOrDefault(x => x.Id == id);
        if (k == null) return;
        Kampaaniad.Kustuta(db, k);
        Laadi();
        Tyhjenda();
    }
}

// Ajatelg: 4 nädalat, iga kampaania on värviline "pill", punane joon = täna (pulseeriv punkt).
// Avamisel kasvavad ribad vasakult paremale (iga järgmine 90 ms hiljem).
public class KampaaniaTelg : Control
{
    private List<Kampaania> read = new();
    private DateTime algus, nyyd;
    private double kasv = 1;      // 0 → 1 (+ viivitus ridade kaupa)
    private double pulss;

    public KampaaniaTelg()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Animatsioon.Tsykkel(this, s => { pulss = s; Invalidate(); }, 40);
    }

    public void Naita(List<Kampaania> kampaaniad, DateTime praegu)
    {
        read = kampaaniad.Take(6).ToList();
        nyyd = praegu;
        var esmasp = praegu.Date.AddDays(-(((int)praegu.DayOfWeek + 6) % 7));
        algus = esmasp.AddDays(-7);   // eelmine nädal + 3 järgmist
        Animatsioon.Kaivita(this, 700 + read.Count * 90, t => { kasv = t * (700 + read.Count * 90); Invalidate(); });
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var f = new Font("Segoe UI", 8.5F);
        var fb = new Font("Segoe UI Semibold", 9F);
        float vasak = 0, laius = Width - 2, paev = laius / 28f;

        // Nädalate päis ja vöödid
        for (int n = 0; n < 4; n++)
        {
            var r = new RectangleF(vasak + n * 7 * paev, 0, 7 * paev, Height);
            if (n % 2 == 1) using (var b = new SolidBrush(Teema.PindAlt)) g.FillRectangle(b, r);
            TextRenderer.DrawText(g, $"{Keel.T("Nädal")} {System.Globalization.ISOWeek.GetWeekOfYear(algus.AddDays(n * 7))} · {algus.AddDays(n * 7):dd.MM}",
                f, Point.Round(new PointF(r.X + 6, 2)), Teema.TekstNork);
        }

        float rida = Math.Min(26, (Height - 30) / Math.Max(1, read.Count));
        for (int i = 0; i < read.Count; i++)
        {
            var k = read[i];
            float x1 = (float)(k.Algus - algus).TotalDays * paev, x2 = (float)(k.Lopp - algus).TotalDays * paev;
            x1 = Math.Clamp(x1, 0, laius); x2 = Math.Clamp(x2, 0, laius);
            if (x2 - x1 < 4) continue;
            double t = Math.Clamp((kasv - i * 90) / 700.0, 0, 1);
            float w = (float)((x2 - x1) * Animatsioon.Sujuv(t));
            var r = new RectangleF(x1, 24 + i * (rida + 4), Math.Max(w, 1), rida);
            var olek = Kampaaniad.Olek(k, nyyd);
            Color c = olek == KampaaniaOlek.Kaib ? Teema.Aktsent : olek == KampaaniaOlek.Tulekul ? Teema.Teine : Color.FromArgb(160, 165, 172);
            using (var tee = Ymardus.Tee(r, rida / 2))
            using (var b = new SolidBrush(c))
                g.FillPath(b, tee);
            if (w > 60)
                TextRenderer.DrawText(g, $"{k.Nimi} −{k.SoodusProtsent}%", fb, Rectangle.Round(new RectangleF(r.X + 10, r.Y, r.Width - 14, r.Height)), Color.White,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        // Täna
        float tx = (float)(nyyd - algus).TotalDays * paev;
        using (var p = new Pen(Teema.Aktsent, 2)) g.DrawLine(p, tx, 18, tx, Height);
        float puls = (float)(4 + 3 * (0.5 + 0.5 * Math.Sin(pulss * 4)));
        using (var b = new SolidBrush(Color.FromArgb(70, Teema.Aktsent))) g.FillEllipse(b, tx - puls - 2, 14 - puls, (puls + 2) * 2, (puls + 2) * 2);
        using (var b = new SolidBrush(Teema.Aktsent)) g.FillEllipse(b, tx - 4, 10, 8, 8);
        TextRenderer.DrawText(g, Keel.T("Täna"), fb, new Point((int)tx + 8, 4), Teema.Aktsent);

        if (read.Count == 0)
            TextRenderer.DrawText(g, Keel.T("Kampaaniaid pole – lisa paremal uus."), new Font("Segoe UI", 10F), new Rectangle(0, 30, Width, Height - 30), Teema.TekstNork,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        f.Dispose();
        fb.Dispose();
    }
}
