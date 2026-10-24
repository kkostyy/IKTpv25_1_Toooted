namespace IKTpv25_1_Toooted;

// Toote leht: pilt, nimi, hind, "Lisa ostukorvi" ning "Toote andmed"
// (Tooteinfo: päritolumaa, kaubamärk, tootja, kogus, kirjeldus, koostisosad, lisainfo, toiteväärtus;
//  Soovitatud tooted: sama kategooria tooted)
public class ToodeVorm : Form
{
    private const int Laius = 740;

    // Kui kasutaja klõpsas "Veel tooteid kaubamärgilt ...", jääb siia kaubamärk (Pood otsib selle järgi)
    public string? OtsiKaubamarki { get; private set; }

    private Toode toode;
    private readonly List<Toode> koik;
    private readonly FlowLayoutPanel sisu;
    private Label? lblKorvis;
    private bool soovitatud;   // valitud vaheleht

    public ToodeVorm(Toode toode, List<Toode> koik)
    {
        this.toode = toode;
        this.koik = koik;

        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Font = UiStiil.Põhifont;
        BackColor = Teema.Taust;
        ClientSize = new Size(Laius + 70, 820);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        sisu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(24, 20, 24, 24),
            BackColor = Teema.Taust
        };
        Controls.Add(sisu);

        Ostukorv.Muutus += KorvMuutus;
        PildiVahemalu.Laetud += PildidLaetud;
        Ehita();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        UiStiil.LisaTagasiNupp(this);   // ← Tagasi poodi (ka Esc, Alt+←)
        Teema.Rakenda(this);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Ostukorv.Muutus -= KorvMuutus;
        PildiVahemalu.Laetud -= PildidLaetud;
        base.OnFormClosed(e);
    }

    private PictureBox? pildiKast;

    private void PildidLaetud()
    {
        if (!IsHandleCreated || IsDisposed) return;
        BeginInvoke(new Action(() =>
        {
            if (pildiKast != null && !pildiKast.IsDisposed && pildiKast.Image == null)
                pildiKast.Image = PildiVahemalu.Too(toode.Pilt);
            foreach (var k in sisu.Controls.OfType<Control>().SelectMany(Koik).OfType<TooteKaart>()) k.Invalidate();
        }));
    }

    private void KorvMuutus()
    {
        if (lblKorvis != null) lblKorvis.Text = KorvisTekst();
        foreach (var k in sisu.Controls.OfType<Control>().SelectMany(Koik).OfType<TooteKaart>()) k.Invalidate();
    }

    private static IEnumerable<Control> Koik(Control c) =>
        new[] { c }.Concat(c.Controls.Cast<Control>().SelectMany(Koik));

    private string KorvisTekst()
    {
        int n = Ostukorv.KorvisKogus(toode.Id);
        return n > 0 ? Keel.F("✓ Ostukorvis: {0} tk", n) : "";
    }

    // ---------------------------------------------------------------
    //  Lehe ehitamine (ka siis, kui avatakse soovitatud toode)
    // ---------------------------------------------------------------
    private void Ehita()
    {
        Text = toode.Toodenimetus;
        sisu.SuspendLayout();
        foreach (Control c in sisu.Controls.Cast<Control>().ToList()) c.Dispose();

        sisu.Controls.Add(YlemineKaart());
        sisu.Controls.Add(new Label
        {
            Text = "Toote andmed",
            AutoSize = true,
            Margin = new Padding(0, 18, 0, 10),
            Font = new Font("Segoe UI Semibold", 15F),
            ForeColor = Teema.Tekst
        });
        sisu.Controls.Add(AndmeteKaart());

        sisu.ResumeLayout();
        sisu.AutoScrollPosition = new Point(0, 0);
        Teema.Rakenda(sisu);
    }

    // Ehitame uuesti veidi hiljem: element, millele klõpsati, kustutatakse ehitamisel
    private void EhitaHiljem()
    {
        if (IsHandleCreated) BeginInvoke(new Action(Ehita));
        else Ehita();
    }

    private RoundedPanel YlemineKaart()
    {
        var kaart = new RoundedPanel { Size = new Size(Laius, 280), Radius = 6, Margin = new Padding(0) };

        var pilt = new PictureBox
        {
            Bounds = new Rectangle(24, 24, 250, 210),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = PildiVahemalu.Too(toode.Pilt),
            BackColor = Teema.Pind
        };
        kaart.Controls.Add(pilt);
        pildiKast = pilt;
        if (toode.Paritolumaa != null)
            kaart.Controls.Add(new Label { Text = Keel.F("Päritolu: {0}", Keel.T(toode.Paritolumaa)), Location = new Point(26, 246), AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Pind });

        const int x = 310;
        var nimi = new Label
        {
            Text = toode.Toodenimetus,
            Location = new Point(x, 26),
            AutoSize = true,
            MaximumSize = new Size(Laius - x - 24, 0),
            Font = new Font("Segoe UI Semibold", 15F),
            ForeColor = Teema.Tekst,
            BackColor = Teema.Pind
        };
        kaart.Controls.Add(nimi);
        int y = 26 + nimi.GetPreferredSize(new Size(Laius - x - 24, 0)).Height + 8;

        // Hind (joonistatakse)
        var hind = new Panel { Bounds = new Rectangle(x, y, Laius - x - 24, 62), BackColor = Teema.Pind };
        hind.Paint += (s, e) =>
        {
            var g = e.Graphics;
            bool soodus = toode.SoodusProtsent != null;
            int w = Ikoonid.JoonistaHind(g, toode.Hind, "tk", 0, 0, soodus ? Teema.Aktsent : Teema.Tekst, 24f);
            string lisa = "";
            if (toode.UhikuHind is decimal uh && toode.Uhik is string u && u != "tk") lisa = $"{Ikoonid.Hind(uh)}/{u}";
            using var v = new Font("Segoe UI", 9F);
            int lx = w + 10;
            if (soodus)
            {
                using var vana = new Font("Segoe UI Semibold", 9.5F, FontStyle.Strikeout);
                TextRenderer.DrawText(g, Ikoonid.Hind(toode.VanaHind!.Value), vana, new Point(lx, 12), Teema.TekstNork);
                lx += TextRenderer.MeasureText(Ikoonid.Hind(toode.VanaHind!.Value), vana).Width + 6;
            }
            TextRenderer.DrawText(g, lisa, v, new Point(lx, 13), Teema.TekstNork);
            TextRenderer.DrawText(g, toode.Kogus > 0 ? Keel.F("Laos {0} tk", toode.Kogus) : Keel.T("Pole laos"), v, new Point(lx, 34),
                toode.Kogus > 0 && toode.Kogus < Form1.VaikeLaoseis ? Teema.Hoiatus : Teema.TekstNork);
        };
        kaart.Controls.Add(hind);
        y += 70;

        var lisaNupp = UiStiil.LooSuurNupp("Lisa ostukorvi", (s, e) =>
        {
            string? viga = toode.Kogus <= 0 ? Keel.F("„{0}“ on otsas.", toode.Toodenimetus)
                : Ostukorv.Lisa(toode.Id, toode.Toodenimetus, toode.Hind, toode.Kogus, 1, toode.Pilt);
            if (viga != null) Teade.Toast(viga, TeateTyyp.Hoiatus);
        });
        lisaNupp.Bounds = new Rectangle(x, y, 190, 40);
        lisaNupp.Radius = 6;
        lisaNupp.Font = new Font("Segoe UI Semibold", 9.5F);
        lisaNupp.Enabled = toode.Kogus > 0;
        lisaNupp.Paint += (s, e) =>
            Ikoonid.Joonista(e.Graphics, Ikoonid.Ostukorv, new Rectangle(lisaNupp.Width - 36, 0, 26, lisaNupp.Height),
                lisaNupp.Enabled ? Color.White : Teema.TekstNork, 12f);
        kaart.Controls.Add(lisaNupp);

        lblKorvis = new Label { Text = KorvisTekst(), Location = new Point(x + 204, y + 11), AutoSize = true, ForeColor = Teema.Edu, BackColor = Teema.Pind, Font = new Font("Segoe UI Semibold", 9.5F) };
        kaart.Controls.Add(lblKorvis);
        y += 52;

        // Ostunimekirja (kui praegu ei osta, jääb meelde)
        var nimekirja = new LinkLabel
        {
            Text = "☐  Lisa ostunimekirja",
            Location = new Point(x, y),
            AutoSize = true,
            LinkBehavior = LinkBehavior.HoverUnderline,
            Font = new Font("Segoe UI Semibold", 9F),
            BackColor = Teema.Pind
        };
        nimekirja.LinkClicked += (s, e) =>
        {
            try
            {
                Ostunimekiri.Lisa(toode.Toodenimetus, 1, toode.Id);
                Teade.Toast(Keel.F("„{0}“ lisati ostunimekirja", toode.Toodenimetus), TeateTyyp.Edu, 1800);
            }
            catch (Exception ex) { Teade.Show("Viga lisamisel:\n" + ex.Message); }
        };
        kaart.Controls.Add(nimekirja);
        y += 30;

        if (!string.IsNullOrWhiteSpace(toode.Kaubamark))
        {
            var silt = new Label { Text = "Veel tooteid kaubamärgilt", Location = new Point(x, y), AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Pind };
            kaart.Controls.Add(silt);
            var link = new LinkLabel
            {
                Text = toode.Kaubamark,
                Location = new Point(x + silt.GetPreferredSize(Size.Empty).Width + 2, y),
                AutoSize = true,
                LinkBehavior = LinkBehavior.HoverUnderline,
                Font = new Font("Segoe UI Semibold", 9F),
                BackColor = Teema.Pind
            };
            link.LinkClicked += (s, e) => { OtsiKaubamarki = toode.Kaubamark; Close(); };
            kaart.Controls.Add(link);
        }

        kaart.Height = Math.Max(280, y + 40);   // pikk nimi ei lõika nuppe ära
        return kaart;
    }

    // ---------------------------------------------------------------
    //  Toote andmed: vahelehed
    // ---------------------------------------------------------------
    private RoundedPanel AndmeteKaart()
    {
        var kaart = new RoundedPanel
        {
            Width = Laius,
            Radius = 6,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(Laius, 120),
            Padding = new Padding(0, 0, 0, 20),
            Margin = new Padding(0)
        };

        var paned = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Location = new Point(1, 1),
            BackColor = Teema.Pind,
            Margin = new Padding(0)
        };

        // Vahelehtede päis
        var pais = new Panel { Size = new Size(Laius - 2, 48), BackColor = Teema.Pind, Margin = new Padding(0) };
        var tab1 = Vaheleht("Tooteinfo", 24, !soovitatud);
        var tab2 = Vaheleht("Soovitatud tooted", 24 + tab1.PreferredWidth + 28, soovitatud);
        tab1.Click += (s, e) => { soovitatud = false; EhitaHiljem(); };
        tab2.Click += (s, e) => { soovitatud = true; EhitaHiljem(); };
        pais.Controls.Add(tab1);
        pais.Controls.Add(tab2);
        pais.Paint += (s, e) =>
        {
            using var joon = new Pen(Teema.Joon);
            e.Graphics.DrawLine(joon, 0, pais.Height - 1, pais.Width, pais.Height - 1);
            var valitud = soovitatud ? tab2 : tab1;
            using var b = new SolidBrush(Teema.Tekst);
            e.Graphics.FillRectangle(b, valitud.Left, pais.Height - 3, valitud.Width, 3);
        };
        paned.Controls.Add(pais);

        paned.Controls.Add(soovitatud ? Soovitatud() : Tooteinfo());

        kaart.Controls.Add(paned);
        return kaart;
    }

    private static Label Vaheleht(string tekst, int x, bool valitud) => new()
    {
        Text = tekst,
        Location = new Point(x, 14),
        AutoSize = true,
        Cursor = Cursors.Hand,
        Font = new Font("Segoe UI Semibold", 9.5F, valitud ? FontStyle.Bold : FontStyle.Regular),
        ForeColor = valitud ? Teema.Tekst : Teema.Tekst2,
        BackColor = Teema.Pind
    };

    private Control Tooteinfo()
    {
        var p = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(24, 16, 24, 0),
            BackColor = Teema.Pind,
            Margin = new Padding(0)
        };

        void Info(string silt, string? vaartus)
        {
            if (string.IsNullOrWhiteSpace(vaartus)) return;
            var rida = new Panel { Size = new Size(440, 50), BackColor = Teema.Pind, Margin = new Padding(0) };
            rida.Controls.Add(new Label { Text = silt, Location = new Point(0, 6), AutoSize = true, Font = new Font("Segoe UI", 8F), ForeColor = Teema.Hoiatus, BackColor = Teema.Pind });
            rida.Controls.Add(new Label { Text = vaartus, Location = new Point(0, 22), AutoSize = true, Font = new Font("Segoe UI", 9.5F), ForeColor = Teema.Tekst, BackColor = Teema.Pind });
            rida.Paint += (s, e) => { using var j = new Pen(Teema.Joon); e.Graphics.DrawLine(j, 0, rida.Height - 1, rida.Width, rida.Height - 1); };
            p.Controls.Add(rida);
        }

        void Pealkiri(string tekst)
        {
            var l = new Label
            {
                Text = tekst,
                Size = new Size(440, 34),
                TextAlign = ContentAlignment.BottomLeft,
                Margin = new Padding(0, 14, 0, 8),
                Font = new Font("Segoe UI Semibold", 9.5F),
                ForeColor = Teema.Tekst,
                BackColor = Teema.Pind
            };
            l.Paint += (s, e) => { using var j = new Pen(Teema.Joon); e.Graphics.DrawLine(j, 0, l.Height - 1, l.Width, l.Height - 1); };
            p.Controls.Add(l);
        }

        void Tekst(string? tekst)
        {
            if (string.IsNullOrWhiteSpace(tekst)) return;
            foreach (string rida in tekst.Split('\n'))
                p.Controls.Add(new Label
                {
                    Text = rida.Trim(),
                    AutoSize = true,
                    MaximumSize = new Size(Laius - 60, 0),
                    Margin = new Padding(0, 6, 0, 6),
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = Teema.Tekst2,
                    BackColor = Teema.Pind
                });
        }

        Info("Päritolumaa", toode.Paritolumaa);
        Info("Kaubamärk", toode.Kaubamark);
        Info("Tootja", toode.Tootja);
        Info("Kogus", toode.Pakend);
        Info("Kategooria", toode.Kategooria?.Kategooria_nimetus);

        if (!string.IsNullOrWhiteSpace(toode.Kirjeldus)) { Pealkiri("Toote andmed"); Tekst(toode.Kirjeldus); }
        // Allergeenid: kiibid + koostis, kus allergeenid on punasel taustal
        var kiibid = DieediMargid.Kiibid(toode);
        if (kiibid.Count > 0)
        {
            Pealkiri("Allergeenid");
            var rida = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Teema.Pind, Margin = new Padding(0, 4, 0, 4), MaximumSize = new Size(Laius - 60, 0) };
            foreach (var (tekst, varv) in kiibid)
                rida.Controls.Add(new Label
                {
                    Text = tekst,
                    AutoSize = true,
                    Padding = new Padding(10, 4, 10, 4),
                    Margin = new Padding(0, 0, 8, 0),
                    Font = new Font("Segoe UI Semibold", 9F),
                    ForeColor = Color.White,
                    BackColor = varv
                });
            p.Controls.Add(rida);
        }
        if (!string.IsNullOrWhiteSpace(toode.Koostisosad))
        {
            Pealkiri("Koostisosad");
            p.Controls.Add(DieediMargid.Koostis(toode.Koostisosad.Trim(), Laius - 60));
        }
        if (!string.IsNullOrWhiteSpace(toode.Lisainfo)) { Pealkiri("Lisainfo"); Tekst(toode.Lisainfo); }

        if (!string.IsNullOrWhiteSpace(toode.Toitevaartus))
        {
            Pealkiri("Toitumisalane teave");
            Tekst("Kogus 100 g/ml kohta");

            var tabel = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Teema.Pind,
                Margin = new Padding(0, 8, 0, 0)
            };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));

            void TabeliRida(string a, string b, bool paks, bool alam)
            {
                var f = new Font("Segoe UI", 8.75F, paks ? FontStyle.Bold : FontStyle.Regular);
                tabel.Controls.Add(new Label { Text = (alam ? "      " : "") + a, AutoSize = true, Font = f, ForeColor = Teema.Tekst2, BackColor = Teema.Pind, Margin = new Padding(0, 4, 0, 4) });
                tabel.Controls.Add(new Label { Text = b, AutoSize = true, Font = f, ForeColor = Teema.Tekst2, BackColor = Teema.Pind, Margin = new Padding(0, 4, 0, 4) });
            }

            TabeliRida("Toiteväärtus", "Kogus 100 g/ml kohta", true, false);
            foreach (string rida in toode.Toitevaartus.Split('\n'))
            {
                var osad = rida.Split(';');
                if (osad.Length < 2) continue;
                bool alam = osad[0].TrimStart().StartsWith("-");
                TabeliRida(osad[0].Trim().TrimStart('-').Trim(), osad[1].Trim(), false, alam);
            }
            p.Controls.Add(tabel);
        }

        if (p.Controls.Count == 0) Tekst("Selle toote kohta pole veel andmeid lisatud.");
        return p;
    }

    private Control Soovitatud()
    {
        var p = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MaximumSize = new Size(Laius - 4, 0),
            Padding = new Padding(16, 14, 8, 0),
            BackColor = Teema.Pind,
            Margin = new Padding(0)
        };

        var tooted = koik.Where(t => t.KategooriaId == toode.KategooriaId && t.Id != toode.Id).Take(6).ToList();
        if (tooted.Count < 3)
            tooted.AddRange(koik.Where(t => t.Id != toode.Id && !tooted.Contains(t)).Take(3 - tooted.Count));

        foreach (var t in tooted)
        {
            var kaart = new TooteKaart(t);
            kaart.Lisa += x =>
            {
                string? viga = Ostukorv.Lisa(x.Id, x.Toodenimetus, x.Hind, x.Kogus, 1, x.Pilt);
                if (viga != null) Teade.Toast(viga, TeateTyyp.Hoiatus);
            };
            kaart.Vahenda += x =>
            {
                var rida = Ostukorv.Ridad.FirstOrDefault(k => k.Id == x.Id);
                if (rida != null) Ostukorv.Vahenda(rida);
            };
            kaart.Ava += x => { toode = x; soovitatud = false; EhitaHiljem(); };
            p.Controls.Add(kaart);
        }

        if (tooted.Count == 0)
            p.Controls.Add(new Label { Text = "Soovitusi pole.", AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Pind });
        return p;
    }
}
