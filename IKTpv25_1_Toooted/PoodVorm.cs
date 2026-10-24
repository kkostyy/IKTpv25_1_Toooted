using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    // E-poe veebilehe stiilis pood:
    //  ülal lilla riba, päis (logo, otsing, kasutaja, ostukorvi nupp), menüüriba (Tooted, Kampaaniad, ...),
    //  vasakul kategooriate ikoonid, keskel avaleht (riiulid "Parimad pakkumised" jne) või tooteruudustik,
    //  paremal "Sinu ostukorv".
    public partial class PoodVorm : Form, ITagasi, IVormiHost
    {
        private enum Vaade { Avaleht, Kategooria, Kampaaniad, Odav, Lemmikud, Otsing, Muu }

        private readonly Form1 kassa;
        private readonly Panel sisuAla;                 // praegune leht (Navigaator)
        private readonly Navigaator lehed;
        private readonly TagasiNupp tagasiNupp;
        private readonly FlowLayoutPanel leivapuru;
        private readonly DieediKaart dieet = new() { Dock = DockStyle.Top };
        private bool peidetiEnne;
        private FlowLayoutPanel sisu = new();          // praeguse poe lehe sisu
        private readonly Panel rail;
        private readonly TextBox txtOtsi;
        private readonly Label lblKokku, lblPakkimine, lblTasuta, lblPant;
        private readonly Panel pandiRida;
        private readonly Edenemine tarneRiba = new();
        private readonly RoundedButton btnMaksma;
        private readonly KorviNupp korviNupp;
        private readonly List<IkoonTekst> navLingid = new();
        private readonly IkoonTekst lnkSalvesta, lnkTyhjenda, lnkToit;
        private readonly ToolTip vihjed = new();

        private List<Toode> koikTooted = new();
        private List<Kategooria> kategooriad = new();
        private Vaade vaade = Vaade.Avaleht;
        private KulgMenuu kulg = null!;
        private int? valitudKategooria;
        private int jarjestus;   // 0 nimi, 1 odavam, 2 kallim, 3 soodustus
        private Label lblTere = null!;
        private bool vaigista;   // otsingu välja tühjendamine koodist ei käivita otsingut

        public PoodVorm(Form1 kassa)
        {
            this.kassa = kassa;

            Text = Keel.F("{0} – e-pood", Arve.PoeNimi);
            StartPosition = FormStartPosition.CenterParent;
            Font = UiStiil.Põhifont;
            BackColor = Teema.Taust;
            ClientSize = new Size(1440, 880);
            MinimumSize = new Size(1180, 680);
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            // ================= 1. Lilla riba =================
            var lilla = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Teema.Teine };
            var tere = lblTere = new Label
            {
                Text = Keel.F("Tere, {0}!     Kliendikaardiga 1 € = 1 sent boonust  ·  tasuta kojuvedu alates {1}", AktiivneKasutaja.Kuvanimi, Ostukorv.Eur(Tarne.TasutaAlates)),
                AutoSize = true,
                Location = new Point(24, 13),
                Font = new Font("Segoe UI Semibold", 9.5F),
                ForeColor = Color.White,
                BackColor = Teema.Teine
            };
            var tsekk = new IkoonTekst("", "Viimane tšekk", Color.White, new Font("Segoe UI Semibold", 9.5F)) { BackColor = Teema.Teine };
            tsekk.Click += (s, e) => kassa.SaadaArve();
            var sulge = new IkoonTekst(Ikoonid.Sulge, "", Color.White, new Font("Segoe UI", 9.5F)) { BackColor = Teema.Teine };
            sulge.Click += (s, e) => Close();
            vihjed.SetToolTip(sulge, Keel.T("Sulge pood"));
            var keeled = new KeeleLingid(Color.White) { BackColor = Teema.Teine };
            lilla.Controls.AddRange(new Control[] { tere, tsekk, sulge, keeled });
            lilla.Resize += (s, e) =>
            {
                keeled.Location = new Point(Math.Max(tere.Right + 30, lilla.Width / 2 - keeled.Width / 2), (lilla.Height - keeled.Height) / 2);
                sulge.Location = new Point(lilla.Width - sulge.Width - 24, (lilla.Height - sulge.Height) / 2);
                tsekk.Location = new Point(sulge.Left - tsekk.Width - 28, (lilla.Height - tsekk.Height) / 2);
            };

            // ================= 2. Päis =================
            var pais = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Teema.Pind };
            var logo = new Logo { Location = new Point(24, 15), Size = new Size(150, 50) };
            logo.Click += (s, e) => NaitaAvaleht();
            var avaleht = new IkoonTekst("", "Avaleht", Teema.Teine, new Font("Segoe UI Semibold", 10.5F));
            avaleht.Click += (s, e) => NaitaAvaleht();

            var otsing = new RoundedPanel
            {
                Size = new Size(640, 48),
                Radius = 8,
                FillColor = Teema.Paneel2,
                BorderColor = Teema.Paneel2,
                BackColor = Teema.Paneel2,
                Padding = new Padding(22, 13, 4, 0)
            };
            txtOtsi = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                PlaceholderText = "Otsi toodet",
                Font = new Font("Segoe UI", 11.5F),
                BackColor = Teema.Paneel2,
                ForeColor = Teema.Tekst
            };
            var luup = new Label
            {
                Dock = DockStyle.Right,
                Width = 44,
                ForeColor = Teema.Teine,
                BackColor = Teema.Paneel2,
                Cursor = Cursors.Hand
            };
            luup.Paint += (s, e) => Ikoonid.Joonista(e.Graphics, Ikoonid.Otsi, new Rectangle(0, 0, luup.Width, 30), luup.ForeColor, 14f);
            luup.Click += (s, e) => Otsi();
            otsing.Controls.Add(txtOtsi);
            otsing.Controls.Add(luup);
            txtOtsi.TextChanged += TxtMuutus;
            txtOtsi.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) { txtOtsi.Clear(); e.SuppressKeyPress = true; }
                if (e.KeyCode == Keys.Enter) e.SuppressKeyPress = true;
            };

            var kasutaja = new IkoonTekst("", AktiivneKasutaja.Nimi, Teema.Teine, new Font("Segoe UI Semibold", 10.5F));
            vihjed.SetToolTip(kasutaja, Keel.F("Sisse logitud: {0} ({1})", AktiivneKasutaja.Nimi, Keel.T(AktiivneKasutaja.Roll)) + "\n" + Keel.T("Klõps: seaded"));
            kasutaja.Click += (s, e) => SeadedVorm.Ava(this);
            var nimekiri = new IkoonTekst("\uE8FD", "Ostunimekiri", Teema.Teine, new Font("Segoe UI Semibold", 10.5F));
            nimekiri.Click += (s, e) => AvaNimekiri();
            vihjed.SetToolTip(nimekiri, Keel.T("Mis on ostetud, mis veel mitte"));
            var seaded = new IkoonTekst("\uE713", "", Teema.Teine, new Font("Segoe UI", 11F));
            seaded.Click += (s, e) => SeadedVorm.Ava(this);
            vihjed.SetToolTip(seaded, Keel.T("Seaded"));
            korviNupp = new KorviNupp();
            korviNupp.Click += (s, e) => Vormista();
            vihjed.SetToolTip(korviNupp, Keel.T("Maksma"));

            pais.Controls.AddRange(new Control[] { logo, avaleht, otsing, nimekiri, seaded, kasutaja, korviNupp });
            pais.Resize += (s, e) =>
            {
                int w = pais.Width;
                korviNupp.Location = new Point(w - korviNupp.Width - 24, 16);
                kasutaja.Location = new Point(korviNupp.Left - kasutaja.Width - 30, (pais.Height - kasutaja.Height) / 2);
                seaded.Location = new Point(kasutaja.Left - seaded.Width - 22, (pais.Height - seaded.Height) / 2);
                nimekiri.Location = new Point(seaded.Left - nimekiri.Width - 26, (pais.Height - nimekiri.Height) / 2);
                avaleht.Location = new Point(logo.Right + 30, (pais.Height - avaleht.Height) / 2);
                int ox = avaleht.Right + 40;
                otsing.Location = new Point(ox, 16);
                otsing.Width = Math.Max(260, nimekiri.Left - 40 - ox);
            };
            pais.Paint += (s, e) => { using var p = new Pen(Teema.Joon); e.Graphics.DrawLine(p, 0, pais.Height - 1, pais.Width, pais.Height - 1); };

            // ================= 3. Menüüriba =================
            var nav = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, BackColor = Teema.Pind, Padding = new Padding(20, 16, 0, 0), WrapContents = false };
            NavLink(nav, "\uECA5", "Tooted", NaitaAvaleht, Vaade.Avaleht);
            NavLink(nav, "", "Kampaaniad", () => NaitaRuudustik(Vaade.Kampaaniad), Vaade.Kampaaniad);
            NavLink(nav, "🔥", "Tõesti odav!", () => NaitaRuudustik(Vaade.Odav), Vaade.Odav);
            NavLink(nav, "", "Minu lemmikud", () => NaitaRuudustik(Vaade.Lemmikud), Vaade.Lemmikud);
            nav.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(Teema.Tume ? 60 : 18, Color.Black), 1);
                for (int i = 0; i < 4; i++)
                {
                    p.Color = Color.FromArgb((Teema.Tume ? 50 : 14) - i * 3, Color.Black);
                    e.Graphics.DrawLine(p, 0, nav.Height - 1 - i, nav.Width, nav.Height - 1 - i);
                }
            };

            // ================= 4. Vasakul kategooriate ikoonid =================
            rail = new Panel { Dock = DockStyle.Left, Width = 56, BackColor = Teema.Pind, AutoScroll = true };

            // ================= 5. Paremal "Sinu ostukorv" =================
            var korv = new Panel { Dock = DockStyle.Right, Width = 390, BackColor = Teema.Pind };
            korviVeerg = korv;

            var korviPais = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = Teema.Pind };
            korviPais.Controls.Add(new Label
            {
                Text = "Sinu ostukorv",
                Location = new Point(18, 15),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 13F),
                ForeColor = Teema.Tekst,
                BackColor = Teema.Pind
            });
            var salvestatud = new IkoonTekst(Ikoonid.NoolParemale, "Salvestatud ostukorvid", Teema.Teine, new Font("Segoe UI Semibold", 10F), ikoonParemal: true);
            salvestatud.Click += (s, e) => NaitaSalvestatud(salvestatud);
            korviPais.Controls.Add(salvestatud);
            korviPais.Resize += (s, e) => salvestatud.Location = new Point(korviPais.Width - salvestatud.Width - 14, 17);
            korviPais.Paint += (s, e) => { using var p = new Pen(Teema.Joon); e.Graphics.DrawLine(p, 0, korviPais.Height - 1, korviPais.Width, korviPais.Height - 1); };

            var korviSisu = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 12, 0), BackColor = Teema.Pind };
            korviSisu.Controls.Add(new KorvVaade { Dock = DockStyle.Fill });

            var jalus = new Panel { Dock = DockStyle.Bottom, Height = 262, Padding = new Padding(16, 14, 16, 4), BackColor = Teema.Pind };
            jalus.Paint += (s, e) => { using var p = new Pen(Teema.Joon); e.Graphics.DrawLine(p, 0, 0, jalus.Width, 0); };
            var rida1 = Rida("Kohaletoimetamine", out lblPakkimine, new Font("Segoe UI", 10F), Teema.Tekst2, 28);
            // Tasuta tarne edenemisriba: "Lisa veel 7,40 € ja kojuvedu on tasuta"
            var tasutaRida = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Teema.Pind };
            lblTasuta = new Label { Dock = DockStyle.Top, Height = 22, Font = new Font("Segoe UI Semibold", 9F), ForeColor = Teema.Tekst2, BackColor = Teema.Pind };
            tarneRiba.Dock = DockStyle.Top;
            tarneRiba.Height = 8;
            tasutaRida.Controls.Add(tarneRiba);
            tasutaRida.Controls.Add(lblTasuta);
            pandiRida = Rida("Pakendi tagatisraha", out lblPant, new Font("Segoe UI", 10F), Teema.Tekst2, 26);
            pandiRida.Visible = false;
            var rida2 = Rida("Kokku", out lblKokku, new Font("Segoe UI Semibold", 14F), Teema.Tekst, 40);

            btnMaksma = UiStiil.LooSuurNupp("Maksma", (s, e) => Vormista());
            btnMaksma.Dock = DockStyle.Top;
            btnMaksma.Height = 48;
            btnMaksma.Radius = 6;
            btnMaksma.TextAlign = ContentAlignment.MiddleLeft;
            btnMaksma.Paint += (s, e) =>
                Ikoonid.Joonista(e.Graphics, Ikoonid.NoolParemale, new Rectangle(btnMaksma.Width - 40, 0, 30, btnMaksma.Height),
                    btnMaksma.Enabled ? Color.White : Teema.TekstNork, 10f);

            var alumised = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 70, BackColor = Teema.Pind, Padding = new Padding(14, 12, 0, 0), WrapContents = true };
            lnkSalvesta = new IkoonTekst("", "Salvesta ostukorv", Teema.Teine, new Font("Segoe UI Semibold", 9F)) { Margin = new Padding(0, 0, 26, 0) };
            lnkSalvesta.Click += (s, e) => SalvestaKorv();
            lnkTyhjenda = new IkoonTekst(Ikoonid.Prygikast, "Tühjenda ostukorv", Teema.Teine, new Font("Segoe UI Semibold", 9F));
            lnkTyhjenda.Click += (s, e) =>
            {
                if (Ostukorv.Ridad.Count > 0 &&
                    Teade.Show("Kas tühjendan ostukorvi?", "Ostukorv", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    Ostukorv.Tyhjenda();
            };
            lnkToit = new IkoonTekst("\uE9D2", "Toiteväärtus", Teema.Teine, new Font("Segoe UI Semibold", 9F)) { Margin = new Padding(0, 6, 0, 0) };
            lnkToit.Click += (s, e) => AvaToitevaartus();
            vihjed.SetToolTip(lnkToit, Keel.T("Kui palju energiat ja toitaineid on ostukorvis"));
            alumised.Controls.Add(lnkSalvesta);
            alumised.Controls.Add(lnkTyhjenda);
            alumised.Controls.Add(lnkToit);

            var vahe = new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Teema.Pind };
            jalus.Controls.Add(alumised);
            jalus.Controls.Add(btnMaksma);
            jalus.Controls.Add(vahe);
            jalus.Controls.Add(rida2);
            jalus.Controls.Add(pandiRida);
            jalus.Controls.Add(tasutaRida);
            jalus.Controls.Add(rida1);

            korv.Controls.Add(korviSisu);
            korv.Controls.Add(jalus);
            korv.Controls.Add(korviPais);
            korv.Paint += (s, e) => { using var p = new Pen(Teema.Joon); e.Graphics.DrawLine(p, 0, 0, 0, korv.Height); };

            // ================= 6. Keskel: lehed + riba "← Tagasi · leivapuru" =================
            var keskmine = new Panel { Dock = DockStyle.Fill, BackColor = Teema.Taust };
            sisuAla = new Panel { Dock = DockStyle.Fill, BackColor = Teema.Taust };
            lehed = new Navigaator(sisuAla);
            lehed.Muutus += NavMuutus;

            var navRiba = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Teema.Taust, Padding = new Padding(30, 0, 20, 0) };
            tagasiNupp = new TagasiNupp { Location = new Point(30, 12), Enabled = false };
            tagasiNupp.Click += (s, e) => MineTagasi();
            vihjed.SetToolTip(tagasiNupp, Keel.T("Tagasi") + "  (Esc, Alt+←)");
            leivapuru = new FlowLayoutPanel { Location = new Point(tagasiNupp.Right + 16, 20), Height = 32, AutoSize = true, WrapContents = false, BackColor = Teema.Taust };
            var vihje = new Label { Text = "Esc · Alt+←", AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Taust, Font = new Font("Segoe UI", 9F) };
            navRiba.Controls.AddRange(new Control[] { tagasiNupp, leivapuru, vihje });
            navRiba.Resize += (s, e) => vihje.Location = new Point(navRiba.Width - vihje.Width - 24, 26);
            tagasiNupp.SizeChanged += (s, e) => leivapuru.Left = tagasiNupp.Right + 16;
            if (Teema.OnMoodne)
            {
                // lõuend N1: valge "pill" riba – Tagasi, leivapuru ja klahvivihje hallil kiibil
                navRiba.Height = 80;
                navRiba.Padding = new Padding(16, 8, 16, 8);
                var pill = new RoundedPanel { Dock = DockStyle.Fill, Radius = 32, FillColor = Teema.Pind, BorderColor = Teema.Pind, BackColor = Teema.Taust };
                navRiba.Controls.Clear();
                vihje.Visible = false;   // asendub KlahviVihje-ga
                var klahvid = new KlahviVihje { Anchor = AnchorStyles.Top | AnchorStyles.Right };
                tagasiNupp.Location = new Point(8, 8);
                leivapuru.BackColor = Teema.Pind;
                leivapuru.Location = new Point(tagasiNupp.Right + 16, 16);
                pill.Controls.AddRange(new Control[] { tagasiNupp, leivapuru, klahvid });
                pill.Resize += (s, e) => klahvid.Location = new Point(pill.Width - klahvid.Width - 14, (pill.Height - klahvid.Height) / 2);
                navRiba.Controls.Add(pill);
            }
            keskmine.Controls.Add(sisuAla);
            keskmine.Controls.Add(dieet);
            keskmine.Controls.Add(navRiba);
            dieet.SeaTooted(() => koikTooted);

            // ================= 7. Külgmenüü (ümar "pill" paremas servas) =================
            kulg = new KulgMenuu();
            kulg.Lisa("avaleht", "\uE80F", "Avaleht", NaitaAvaleht);
            kulg.Lisa("otsi", "\uE721", "Otsi toodet", () => { txtOtsi.Focus(); txtOtsi.SelectAll(); });
            kulg.Lisa("lemmikud", "\uEB51", "Minu lemmikud", () => NaitaRuudustik(Vaade.Lemmikud));
            kulg.Lisa("kampaaniad", "\uE8EC", "Kampaaniad", () => NaitaRuudustik(Vaade.Kampaaniad));
            kulg.Lisa("kaart", "\uE8C7", "Kliendikaart", () => KliendikaartVorm.Ava(this));
            kulg.Lisa("preemiad", "\uE734", "Preemiad", () => { if (lehed.Praegune is not PreemiadLeht) AvaLeht(new PreemiadLeht()); });
            kulg.Lisa("nimekiri", "\uE8FD", "Ostunimekiri", AvaNimekiri, Ostunimekiri.OstmataArv);
            kulg.Lisa("korv", "\uE7BF", "Maksma", Vormista, () => Ostukorv.Ridad.Sum(r => r.Kogus));
            kulg.Lisa("seaded", "\uE713", "Seaded", () => SeadedVorm.Ava(this));
            kulg.Valju = Close;
            kulg.ValjuTekst = AktiivneKasutaja.OnOstja ? "Logi välja" : "Sulge pood";
            kulg.Profiil = () => SeadedVorm.Ava(this);
            kulg.LogoKlikk = NaitaAvaleht;
            kulg.Aktiivne = "avaleht";

            Controls.Add(keskmine);
            Controls.Add(rail);
            Controls.Add(korv);
            Controls.Add(kulg);
            Controls.Add(nav);
            Controls.Add(pais);
            Controls.Add(lilla);
            if (moodne) RakendaMoodne(lilla, pais, nav, keskmine, korv, korviPais);   // Ctrl+Shift+M
            keskmine.BringToFront();

            Ostukorv.Muutus += KorvMuutus;
            Lemmikud.Muutus += LemmikudMuutus;
            MinuDieet.Muutus += DieetMuutus;
            peidetiEnne = MinuDieet.Peida;
            PildiVahemalu.Laetud += PildidLaetud;
            Teema.Muutus += DisainMuutus;
            Load += (s, e) => { LaadiTooted(); NaitaAvaleht(); UuendaKokku(); };

            // Kord minutis: kas õhtune soodustus algas/lõppes → korvi hinnad ja kaartide "tempel"
            ohtuKaib = OhtuSoodustus.Kaib(Hinnad.Kell());
            var minut = new System.Windows.Forms.Timer { Interval = 30_000 };
            minut.Tick += (s, e) => KellMuutus();
            minut.Start();
            FormClosed += (s, e) => { minut.Stop(); minut.Dispose(); };
        }

        // ---------------------------------------------------------------
        //  Abielemendid
        // ---------------------------------------------------------------
        private void NavLink(FlowLayoutPanel nav, string ikoon, string tekst, Action tegevus, Vaade v)
        {
            var l = new IkoonTekst(ikoon, tekst, Teema.Tekst2, new Font("Segoe UI Semibold", 10.5F), emoji: ikoon == "🔥")
            {
                Margin = new Padding(0, 0, 30, 0),
                Tag = v
            };
            l.Click += (s, e) => tegevus();
            navLingid.Add(l);
            nav.Controls.Add(l);
        }

        private static Panel Rida(string silt, out Label vaartus, Font font, Color varv, int korgus)
        {
            var p = new Panel { Dock = DockStyle.Top, Height = korgus, BackColor = Teema.Pind };
            vaartus = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = font, ForeColor = varv, BackColor = Teema.Pind };
            var l = new Label { Text = silt, Dock = DockStyle.Left, Width = 170, TextAlign = ContentAlignment.MiddleLeft, Font = font, ForeColor = varv, BackColor = Teema.Pind };
            p.Controls.Add(vaartus);
            p.Controls.Add(l);
            return p;
        }

        protected override void OnLoad(EventArgs e)
        {
            Teema.Rakenda(this);
            base.OnLoad(e);
        }

        // ← Tagasi: eelmine leht (avalehel ei tee midagi)
        public bool MineTagasi() => lehed.Tagasi();

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool tekstis = ActiveControl is TextBoxBase || ActiveControl is ComboBox;
            if (keyData == (Keys.Alt | Keys.Left) ||
                (keyData == Keys.Escape && !(ActiveControl is TextBox t && t.TextLength > 0)) ||
                (keyData == Keys.Back && !tekstis))
            {
                if (MineTagasi()) return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F) { txtOtsi.Focus(); txtOtsi.SelectAll(); e.Handled = true; }
            base.OnKeyDown(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Ostukorv.Muutus -= KorvMuutus;
            Lemmikud.Muutus -= LemmikudMuutus;
            MinuDieet.Muutus -= DieetMuutus;
            PildiVahemalu.Laetud -= PildidLaetud;
            Teema.Muutus -= DisainMuutus;
            base.OnFormClosed(e);
        }

        private bool ohtuKaib;

        private void KellMuutus()
        {
            bool kaib = OhtuSoodustus.Kaib(Hinnad.Kell());
            Ostukorv.UuendaHinnad();
            if (kaib == ohtuKaib) return;
            ohtuKaib = kaib;
            foreach (var k in KoikKaardid())
                if (OhtuSoodustus.S.KategooriaId == k.Toode.KategooriaId) { if (kaib) k.Tempel(); else k.Invalidate(); }
            if (kaib) Teade.Toast(Keel.F("Õhtune küpsetiste tund algas: −{0} %", OhtuSoodustus.S.Protsent), TeateTyyp.Info, 4000);
        }

        private void KorvMuutus()
        {
            UuendaKokku();
            foreach (var k in KoikKaardid()) k.Invalidate();
        }

        // Ostunimekiri; pärast sulgemist võivad korvis olla uued tooted
        private void AvaNimekiri() => Aken.Ava(this, new OstunimekiriVorm());

        // Ctrl+Shift+D: pood suletakse ja Form1 avab klassikalise poe
        private void DisainMuutus()
        {
            if ((!Teema.OnRimi || Teema.OnMoodne != moodne) && IsHandleCreated)
            {
                Tag = "disain";
                BeginInvoke(new Action(Close));
            }
            else
            {
                Text = Keel.F("{0} – e-pood", Arve.PoeNimi);
                lblTere.Text = Keel.F("Tere, {0}!     Kliendikaardiga 1 € = 1 sent boonust  ·  tasuta kojuvedu alates {1}", AktiivneKasutaja.Kuvanimi, Ostukorv.Eur(Tarne.TasutaAlates));
                // Päiste elementide paigutus uuesti (tekstide laius muutus)
                foreach (var p in Alamad(this).OfType<Panel>().Where(p => p.Dock == DockStyle.Top).ToList())
                {
                    p.Height += 1;
                    p.Height -= 1;
                }
                MargiAegunuks();
                Varskenda();
                LooLeivapuru();
                Teema.Rakenda(this);
                Invalidate(true);
            }
        }

        // Veebist alla laaditud pilt valmis (tuleb taustalõimest)
        private void PildidLaetud()
        {
            if (!IsHandleCreated || IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                foreach (var k in KoikKaardid()) k.Invalidate();
            }));
        }

        private void LemmikudMuutus()
        {
            if (vaade == Vaade.Lemmikud && IsHandleCreated) BeginInvoke(new Action(Varskenda));
            else foreach (var k in KoikKaardid()) k.Invalidate();
        }

        // Dieedi valik muutus: sobimatud kaardid tuhmuvad (või peidetakse – siis joonistatakse uuesti)
        private void DieetMuutus()
        {
            if (MinuDieet.Peida || peidetiEnne)
            {
                MargiAegunuks();
                Varskenda();
            }
            else foreach (var k in KoikKaardid()) k.UuendaDieet();
            peidetiEnne = MinuDieet.Peida;
        }

        // Peida sobimatud: ainult dieediga sobivad tooted
        private static List<Toode> Filtreeri(IEnumerable<Toode> tooted) =>
            MinuDieet.Peida ? tooted.Where(MinuDieet.Sobib).ToList() : tooted.ToList();

        private IEnumerable<TooteKaart> KoikKaardid()
        {
            foreach (Control c in sisu.Controls)
                foreach (var k in Alamad(c).OfType<TooteKaart>()) yield return k;
        }

        private static IEnumerable<Control> Alamad(Control c) =>
            new[] { c }.Concat(c.Controls.Cast<Control>().SelectMany(Alamad));

        private void UuendaKokku()
        {
            // Kojuvedu: tasuline, alates 25 € tasuta (maksmisel saab valida ka "Poest kaasa")
            decimal tarne = Tarne.Arvuta(Ostukorv.Kokku);
            decimal puudu = Tarne.Puudu(Ostukorv.Kokku);
            lblPakkimine.Text = Ostukorv.Ridad.Count == 0 ? Ostukorv.Eur(0) : tarne == 0 ? Keel.T("TASUTA") : Ostukorv.Eur(tarne);
            lblPakkimine.ForeColor = tarne == 0 && Ostukorv.Ridad.Count > 0 ? Teema.Edu : Teema.Tekst2;
            lblTasuta.Text = puudu > 0
                ? Keel.F("Lisa veel {0} ja kojuvedu on tasuta", Ostukorv.Eur(puudu))
                : Keel.F("✓ Tasuta kojuvedu (ostukorv üle {0})", Ostukorv.Eur(Tarne.TasutaAlates));
            lblTasuta.ForeColor = puudu > 0 ? Teema.Tekst2 : Teema.Edu;
            tarneRiba.Osa = (float)(Ostukorv.Kokku / Tarne.TasutaAlates);
            // Pant: eraldi rida, kuulub kokkusummasse (tasuta tarne piir arvestab ainult tooteid)
            pandiRida.Visible = Ostukorv.PantKokku > 0;
            lblPant.Text = Keel.F("{0} tk · {1}", Ostukorv.PandiTk, Ostukorv.Eur(Ostukorv.PantKokku));
            lblKokku.Text = Ostukorv.Eur(Ostukorv.Kokku + Ostukorv.PantKokku + tarne);
            bool tyhi = Ostukorv.Ridad.Count == 0;
            btnMaksma.Enabled = !tyhi;
            lnkSalvesta.Enabled = lnkTyhjenda.Enabled = lnkToit.Enabled = !tyhi;
            korviNupp.Invalidate();
        }

        // ---------------------------------------------------------------
        //  Andmed
        // ---------------------------------------------------------------
        private void LaadiTooted()
        {
            Kampaaniad.Uuenda();
            using var db = new ToodedContext();
            koikTooted = db.Toodetabel.Include(t => t.Kategooria).AsNoTracking().ToList();
            kategooriad = db.Kategooriatabel.AsNoTracking().ToList().OrderBy(k => k.Id).ToList();
            LooRail();
            dieet.Uuenda();
        }

        private void LooRail()
        {
            rail.SuspendLayout();
            foreach (Control c in rail.Controls.Cast<Control>().ToList()) c.Dispose();
            for (int i = kategooriad.Count - 1; i >= 0; i--)
            {
                var k = kategooriad[i];
                var nupp = new RailNupp(k.Id, Ikoonid.KategooriaIkoon(k.Kategooria_nimetus)) { Dock = DockStyle.Top, Valitud = valitudKategooria == k.Id && vaade == Vaade.Kategooria };
                vihjed.SetToolTip(nupp, $"{Keel.T(k.Kategooria_nimetus)} ({koikTooted.Count(t => t.KategooriaId == k.Id)})");
                nupp.Click += (s, e) => NaitaKategooria(k.Id);
                rail.Controls.Add(nupp);
            }
            rail.ResumeLayout();
        }

        private void MargiValik()
        {
            MoodneValik();
            foreach (var l in navLingid)
            {
                l.Valitud = Equals(l.Tag, vaade);
                l.Invalidate();
            }
            foreach (var r in rail.Controls.OfType<RailNupp>())
            {
                r.Valitud = vaade == Vaade.Kategooria && r.KategooriaId == valitudKategooria;
                r.Invalidate();
            }
        }

        // ---------------------------------------------------------------
        //  Avaleht: riiulid
        // ---------------------------------------------------------------
        private void NaitaAvaleht()
        {
            if (lehed.Lehed.Count == 0)
                lehed.Ava(new PoeVaadeLeht("Avaleht", "avaleht", JoonistaAvaleht, () =>
                {
                    vaade = Vaade.Avaleht;
                    if (kulg != null) { kulg.Aktiivne = "avaleht"; kulg.Invalidate(); }
                    valitudKategooria = null;
                    TyhjendaOtsing();
                    MargiValik();
                }, SobitaLaiused));
            else lehed.Avalehele();
        }

        private void JoonistaAvaleht()
        {
            TyhjendaSisu();
            if (moodne) sisu.Controls.Add(LooBento());
            if (OhtuSoodustus.S.Sees) sisu.Controls.Add(new OhtuBanner());
            var soodus = Filtreeri(koikTooted.Where(t => t.SoodusProtsent != null).OrderByDescending(t => t.SoodusProtsent));
            if (soodus.Count > 0)
                sisu.Controls.Add(new Riiul("Parimad pakkumised", soodus, () => NaitaRuudustik(Vaade.Kampaaniad), Uhenda));

            foreach (var k in kategooriad)
            {
                var tooted = Filtreeri(koikTooted.Where(t => t.KategooriaId == k.Id).OrderBy(t => t.Toodenimetus));
                if (tooted.Count == 0) continue;
                int id = k.Id;
                sisu.Controls.Add(new Riiul(k.Kategooria_nimetus, tooted, () => NaitaKategooria(id), Uhenda));
            }

            if (sisu.Controls.Count == 0)
                sisu.Controls.Add(TyhiTekst("Poes pole veel tooteid. Lisa neid vahelehel „Tooted“."));
            LopetaSisu();
        }

        private void TxtMuutus(object? sender, EventArgs e)
        {
            if (!vaigista) Otsi();
        }

        // Otsingu lehele tagasi tulles: otsingusõna tagasi lahtrisse (otsingut uuesti ei käivitata)
        private void SeaOtsing(string tekst)
        {
            if (txtOtsi.Text.Trim() == tekst) return;
            vaigista = true;
            txtOtsi.Text = tekst;
            vaigista = false;
        }

        private void TyhjendaOtsing()
        {
            if (txtOtsi.Text.Length == 0) return;
            vaigista = true;
            txtOtsi.Clear();
            vaigista = false;
        }

        // ---------------------------------------------------------------
        //  Ruudustik: kategooria, kampaaniad, odavad, lemmikud, otsing
        // ---------------------------------------------------------------
        private void NaitaKategooria(int id)
        {
            valitudKategooria = id;
            NaitaRuudustik(Vaade.Kategooria);
        }

        private void Otsi()
        {
            if (txtOtsi.Text.Trim().Length == 0)
            {
                if (vaade == Vaade.Otsing) NaitaAvaleht();
                return;
            }
            NaitaRuudustik(Vaade.Otsing);
        }

        // Uus leht pinu otsa (sama leht uuesti = ainult värskendus; otsing asendab otsingut)
        private void NaitaRuudustik(Vaade v)
        {
            int? kat = v == Vaade.Kategooria ? valitudKategooria : null;
            string otsi = v == Vaade.Otsing ? txtOtsi.Text.Trim() : "";
            string voti = $"{v}:{kat}:{otsi}";
            if (lehed.Praegune is PoeVaadeLeht praegu && praegu.Voti == voti) { praegu.Joonista(); return; }

            string pealkiri = v switch
            {
                Vaade.Kategooria => kategooriad.FirstOrDefault(k => k.Id == kat)?.Kategooria_nimetus ?? "Tooted",
                Vaade.Kampaaniad => "Kampaaniad",
                Vaade.Odav => "Tõesti odav!",
                Vaade.Lemmikud => "Minu lemmikud",
                _ => Keel.F("Otsing: „{0}“", otsi)
            };
            var leht = new PoeVaadeLeht(pealkiri, voti, () => JoonistaRuudustik(v, kat, otsi), () =>
            {
                vaade = v;
                valitudKategooria = kat;
                if (kulg != null)
                {
                    kulg.Aktiivne = v == Vaade.Lemmikud ? "lemmikud" : v == Vaade.Kampaaniad ? "kampaaniad" : null;
                    kulg.Invalidate();
                }
                if (v == Vaade.Otsing) SeaOtsing(otsi); else TyhjendaOtsing();
                MargiValik();
            }, SobitaLaiused);

            if (v == Vaade.Otsing && lehed.Praegune is PoeVaadeLeht p && p.Voti.StartsWith(nameof(Vaade.Otsing)))
                lehed.Asenda(leht);
            else
                lehed.Ava(leht);
        }

        private void JoonistaRuudustik(Vaade v, int? valitudKategooria, string otsi)
        {
            IEnumerable<Toode> q = v switch
            {
                Vaade.Kategooria => koikTooted.Where(t => t.KategooriaId == valitudKategooria),
                Vaade.Kampaaniad => koikTooted.Where(t => t.SoodusProtsent != null),
                Vaade.Odav => koikTooted.Where(t => t.Kogus > 0).OrderBy(t => t.Hind).Take(12),
                Vaade.Lemmikud => koikTooted.Where(t => Lemmikud.On(t.Id)),
                _ => koikTooted.Where(t => t.Toodenimetus.Contains(otsi, StringComparison.CurrentCultureIgnoreCase)
                                        || (t.Kaubamark ?? "").Contains(otsi, StringComparison.CurrentCultureIgnoreCase)
                                        || (t.Kategooria?.Kategooria_nimetus ?? "").Contains(otsi, StringComparison.CurrentCultureIgnoreCase))
            };

            if (v != Vaade.Odav)
                q = jarjestus switch
                {
                    1 => q.OrderBy(t => t.Hind),
                    2 => q.OrderByDescending(t => t.Hind),
                    3 => q.OrderByDescending(t => t.SoodusProtsent ?? 0).ThenBy(t => t.Toodenimetus),
                    _ => q.OrderBy(t => t.Toodenimetus)
                };
            var tooted = Filtreeri(q);

            string pealkiri = v switch
            {
                Vaade.Kategooria => Keel.T(kategooriad.FirstOrDefault(k => k.Id == valitudKategooria)?.Kategooria_nimetus ?? "Tooted"),
                Vaade.Kampaaniad => Keel.T("Kampaaniad"),
                Vaade.Odav => Keel.T("Tõesti odav!"),
                Vaade.Lemmikud => Keel.T("Minu lemmikud"),
                _ => Keel.F("Otsingu tulemused: „{0}“", otsi)
            };

            TyhjendaSisu();
            if (v == Vaade.Kategooria && valitudKategooria == OhtuSoodustus.S.KategooriaId && OhtuSoodustus.S.Sees)
                sisu.Controls.Add(new OhtuBanner());
            sisu.Controls.Add(RuudustikuPais(pealkiri, tooted.Count, v != Vaade.Odav));

            var ruudustik = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                BackColor = Teema.Taust,
                Margin = new Padding(0),
                Tag = "ruudustik"
            };
            foreach (var t in tooted) ruudustik.Controls.Add(Uhenda(new TooteKaart(t) { Margin = new Padding(0, 0, 16, 16) }));
            sisu.Controls.Add(ruudustik);

            if (tooted.Count == 0)
                sisu.Controls.Add(TyhiTekst(v switch
                {
                    Vaade.Lemmikud => "Lemmikuid pole veel. Märgi toode tärniga ☆, et see siia lisada.",
                    Vaade.Kampaaniad => "Praegu kampaaniatooteid pole.",
                    Vaade.Otsing => "Midagi ei leitud. Proovi teist otsingusõna.",
                    _ => "Selles kategoorias pole veel tooteid."
                }));
            LopetaSisu();
        }

        private Control RuudustikuPais(string pealkiri, int arv, bool jarjestusega)
        {
            // Tagasi-nupp on lehe kohal ribal (Navigaator)
            var p = new Panel { Height = 48, BackColor = Teema.Taust, Margin = new Padding(0), Tag = "laius" };
            var l = new Label { Text = pealkiri, Location = new Point(0, 2), AutoSize = true, Font = new Font("Segoe UI Semibold", 20F), ForeColor = Teema.Tekst, BackColor = Teema.Taust };
            var arvL = new Label { Text = Keel.F("{0} toodet", arv), AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Taust, Font = new Font("Segoe UI", 10F) };
            p.Controls.AddRange(new Control[] { l, arvL });
            l.SizeChanged += (s, e) => arvL.Location = new Point(l.Right + 10, 16);
            arvL.Location = new Point(l.PreferredWidth + 10, 16);

            if (jarjestusega)
            {
                var cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200, Font = new Font("Segoe UI", 9.5F), FlatStyle = FlatStyle.Flat };
                cmb.Items.AddRange(new object[] { Keel.T("Järjesta: nimi A–Z"), Keel.T("Hind: odavam enne"), Keel.T("Hind: kallim enne"), Keel.T("Suurim soodustus") });
                cmb.SelectedIndex = jarjestus;
                cmb.SelectedIndexChanged += (s, e) =>
                {
                    jarjestus = cmb.SelectedIndex;
                    BeginInvoke(new Action(Varskenda));
                };
                p.Controls.Add(cmb);
                p.Resize += (s, e) => cmb.Location = new Point(p.Width - cmb.Width - 16, 10);
            }
            return p;
        }

        private static Label TyhiTekst(string tekst) => new()
        {
            Text = tekst,
            AutoSize = true,
            Margin = new Padding(0, 20, 0, 0),
            ForeColor = Teema.TekstNork,
            Font = new Font("Segoe UI", 11F)
        };

        private TooteKaart Uhenda(TooteKaart kaart)
        {
            kaart.Lisa += LisaKorvi;
            kaart.Vahenda += VahendaKorvis;
            kaart.Ava += AvaToode;
            return kaart;
        }

        private void TyhjendaSisu()
        {
            sisu.SuspendLayout();
            foreach (Control c in sisu.Controls.Cast<Control>().ToList()) c.Dispose();
        }

        private void LopetaSisu()
        {
            SobitaLaiused();
            Teema.Rakenda(sisu);
            sisu.ResumeLayout();
            sisu.AutoScrollPosition = new Point(0, 0);
        }

        private void SobitaLaiused() => SobitaLaiused(sisu);

        private static void SobitaLaiused(FlowLayoutPanel sisu)
        {
            int laius = Math.Max(400, sisu.ClientSize.Width - sisu.Padding.Horizontal - 4);
            foreach (Control c in sisu.Controls)
            {
                if (c is Riiul || Equals(c.Tag, "laius")) c.Width = laius;
                else if (c is FlowLayoutPanel f && Equals(f.Tag, "ruudustik")) f.MaximumSize = new Size(laius + 16, 0);
            }
        }

        // ---------------------------------------------------------------
        //  Navigeerimine: lehtede pinu, leivapuru, värskendus
        // ---------------------------------------------------------------

        // Uus leht poe sisualale (toiteväärtus, preemiad, pere nimekiri ...)
        // Moodne: vormid (seaded, kliendikaart, ajalugu ...) avanevad poe lehena, mitte eraldi aknas
        public bool VoibManustada => moodne && Visible;

        // Vormi enda "Tagasi" peidetakse; sama rea elemendid nihkuvad vasakule
        private static void PeidaOmaTagasi(Control juur)
        {
            foreach (Control c in juur.Controls.Cast<Control>().ToList())
            {
                if (c is TagasiNupp t && t.Parent is Control p)
                {
                    int nihe = t.Width + 14;
                    t.Visible = false;
                    // ainult Tagasi nupuga päis kaob täiesti
                    if (p.Name == "tagasiPais" && p.Controls.Cast<Control>().All(o => o == t || !o.Visible)) { p.Visible = false; continue; }
                    foreach (Control o in p.Controls)
                        if (o != t && o.Left > t.Left && Math.Abs(o.Top - t.Top) < 36 && o.Dock == DockStyle.None) o.Left -= nihe;
                }
                else PeidaOmaTagasi(c);
            }
        }

        public void Manusta(Form vorm, Action? suletud)
        {
            if (vorm is KujundusVorm k) k.IlmaTagasita = true;   // poe riba "Tagasi" on juba olemas
            else
            {
                PeidaOmaTagasi(vorm);
                // mõni vorm lisab Tagasi alles laadimisel (OnLoad) – peidame ka selle
                vorm.Load += (s, e) => vorm.BeginInvoke(new Action(() => PeidaOmaTagasi(vorm)));
            }
            var leht = new VormiLeht(vorm, suletud, () => lehed.Tagasi());
            // sama vorm juba lahti (nt Seaded kaks korda järjest): asendatakse, pinu ei kasva
            if (lehed.Praegune is VormiLeht vana && vana.VormiTyyp == vorm.GetType())
            {
                Teema.Rakenda(leht);
                lehed.Asenda(leht);
            }
            else AvaLeht(leht);
        }

        // Kompositsioon: lapselemendid joonistatakse koos (animatsioonid ei vilgu ega rebene)
        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; if (Teema.Komposiit) cp.ExStyle |= 0x02000000; return cp; }
        }

        public void AvaLeht(Leht leht)
        {
            Teema.Rakenda(leht);
            lehed.Ava(leht);
        }

        // Ostukorvi toiteväärtus (leht poe sees, "← Tagasi" viib tagasi)
        private void AvaToitevaartus()
        {
            if (lehed.Praegune is ToitevaartusLeht) return;
            AvaLeht(new ToitevaartusLeht(() => koikTooted.ToDictionary(t => t.Id)));
        }

        private void NavMuutus()
        {
            dieet.Visible = lehed.Praegune is PoeVaadeLeht;
            // maksmise ajal on kokkuvõte vormi sees – korvi veerg peidetakse (rohkem ruumi)
            if (moodne && korviVeerg != null) korviVeerg.Visible = !(lehed.Praegune is VormiLeht vl && vl.VormiTyyp == typeof(MaksmineVorm));
            if (lehed.Praegune is PoeVaadeLeht p) sisu = p.Sisu;
            else
            {
                vaade = Vaade.Muu;
                if (kulg != null) { kulg.Aktiivne = null; kulg.Invalidate(); }
                TyhjendaOtsing();
                MargiValik();
            }
            tagasiNupp.Enabled = lehed.SaabTagasi;
            LooLeivapuru();
        }

        // Avaleht › Kategooria › ...  (iga osa viib sellele tasemele tagasi)
        private void LooLeivapuru()
        {
            leivapuru.SuspendLayout();
            foreach (Control c in leivapuru.Controls.Cast<Control>().ToList()) c.Dispose();
            var f = new Font("Segoe UI Semibold", 10.5F);
            for (int i = 0; i < lehed.Lehed.Count; i++)
            {
                if (i > 0)
                    leivapuru.Controls.Add(new Label { Text = "›", AutoSize = true, Font = f, ForeColor = Teema.TekstNork, BackColor = leivapuru.BackColor, Margin = new Padding(6, 4, 6, 0) });
                int indeks = i;
                bool viimane = i == lehed.Lehed.Count - 1;
                var osa = new IkoonTekst("", lehed.Lehed[i].Pealkiri, viimane ? Teema.Tekst : Teema.Teine, f) { Margin = new Padding(0, 2, 0, 0), Valitud = viimane && i > 0, BackColor = leivapuru.BackColor };
                if (!viimane) osa.Click += (s, e) => lehed.Mine(indeks);
                else osa.Cursor = Cursors.Default;
                leivapuru.Controls.Add(osa);
            }
            leivapuru.ResumeLayout();
        }

        // Praegune poe leht joonistatakse uuesti (järjestus, lemmikud, laoseis)
        private void Varskenda()
        {
            if (lehed.Praegune is PoeVaadeLeht p) p.Joonista();
        }

        // Laoseis/teema muutus: peidetud lehed joonistatakse tagasi tulles uuesti
        private void MargiAegunuks()
        {
            foreach (var p in lehed.Lehed.OfType<PoeVaadeLeht>()) p.Aegunud = true;
        }

        // ---------------------------------------------------------------
        //  Ostukorv
        // ---------------------------------------------------------------
        private static void LisaKorvi(Toode t)
        {
            if (t.Kogus <= 0)
            {
                Teade.Toast(Keel.F("„{0}“ on otsas.", t.Toodenimetus), TeateTyyp.Hoiatus);
                return;
            }
            string? viga = Ostukorv.LisaToode(t);
            if (viga != null) Teade.Toast(viga, TeateTyyp.Hoiatus);
        }

        private static void VahendaKorvis(Toode t)
        {
            var rida = Ostukorv.Ridad.FirstOrDefault(r => r.Id == t.Id);
            if (rida != null) Ostukorv.Vahenda(rida);
        }

        private void AvaToode(Toode t)
        {
            var vorm = new ToodeVorm(t, koikTooted);
            Aken.Ava(this, vorm, () =>
            {
                if (vorm.OtsiKaubamarki != null)
                    txtOtsi.Text = vorm.OtsiKaubamarki;   // käivitab otsingu
            });
        }

        private void Vormista()
        {
            if (Ostukorv.Ridad.Count == 0)
            {
                Teade.Toast("Sinu ostukorv on tühi.", TeateTyyp.Info);
                return;
            }
            kassa.Vormista(() =>
            {
                LaadiTooted();   // laoseis muutus
                MargiAegunuks();
                Varskenda();
            }, this);
        }

        private void SalvestaKorv()
        {
            if (Ostukorv.Ridad.Count == 0) return;
            string? nimi = Teade.Kysi("Salvesta ostukorv", "Ostukorvi nimi:", $"{Keel.T("Ostukorv")} {DateTime.Now:dd.MM HH:mm}");
            if (string.IsNullOrWhiteSpace(nimi)) return;
            try
            {
                SalvestatudKorvid.Salvesta(nimi.Trim());
                Teade.Toast(Keel.F("Ostukorv „{0}“ salvestatud", nimi.Trim()));
            }
            catch (Exception ex) { Teade.Show("Salvestamine ebaõnnestus:\n" + ex.Message); }
        }

        private void NaitaSalvestatud(Control nupp)
        {
            var menuu = new ContextMenuStrip { ShowImageMargin = false, Font = new Font("Segoe UI", 9.5F) };
            var korvid = SalvestatudKorvid.Koik();
            if (korvid.Count == 0)
                menuu.Items.Add(new ToolStripMenuItem(Keel.T("Salvestatud ostukorve pole")) { Enabled = false });

            foreach (var k in korvid)
            {
                var item = new ToolStripMenuItem($"{k.Nimi}   ·   {Keel.F("{0} toodet", k.Read.Sum(r => r.Kogus))}   ·   {k.Aeg:dd.MM.yyyy}");
                var kustuta = new ToolStripMenuItem(Keel.T("Kustuta"));
                kustuta.Click += (s, e) => { SalvestatudKorvid.Kustuta(k.Nimi); Teade.Toast("Salvestatud ostukorv kustutatud", TeateTyyp.Info); };
                item.DropDownItems.Add(new ToolStripMenuItem(Keel.T("Lisa ostukorvi"), null, (s, e) => TaastaKorv(k)));
                item.DropDownItems.Add(kustuta);
                menuu.Items.Add(item);
            }
            foreach (ToolStripItem i in menuu.Items) i.ForeColor = Teema.Tekst;
            if (Teema.OnMoodne) MoodneMenyyJoonistaja.Rakenda(menuu);
            menuu.Closed += (s, e) => BeginInvoke(new Action(menuu.Dispose));
            menuu.Show(nupp, new Point(nupp.Width - 10, nupp.Height + 4), ToolStripDropDownDirection.BelowLeft);
        }

        private void TaastaKorv(SalvestatudKorv k)
        {
            int lisatud = 0, puudu = 0;
            foreach (var r in k.Read)
            {
                var t = koikTooted.FirstOrDefault(x => x.Id == r.Id);
                if (t == null || t.Kogus <= 0) { puudu++; continue; }
                int vaba = t.Kogus - Ostukorv.KorvisKogus(t.Id);
                int kogus = Math.Min(r.Kogus, vaba);
                if (kogus <= 0) { puudu++; continue; }
                if (Ostukorv.LisaToode(t, kogus) == null) lisatud++;
                if (kogus < r.Kogus) puudu++;
            }
            Teade.Toast(Keel.F("„{0}“: lisati {1} toodet", k.Nimi, lisatud) + (puudu > 0 ? Keel.F(", {0} pole laos piisavalt", puudu) : ""),
                puudu > 0 ? TeateTyyp.Hoiatus : TeateTyyp.Edu);
        }
    }

    // ===================================================================
    //  Poe leht: avaleht või tooteruudustik (sisu joonistab PoodVorm)
    // ===================================================================
    public class PoeVaadeLeht : Leht
    {
        private readonly string pealkiri;
        private readonly Action joonista, olek;
        private bool joonistatud;

        public FlowLayoutPanel Sisu { get; }
        public string Voti { get; }
        public bool Aegunud { get; set; }
        public override string Pealkiri => pealkiri;

        public PoeVaadeLeht(string pealkiri, string voti, Action joonista, Action olek, Action<FlowLayoutPanel> sobita)
        {
            this.pealkiri = pealkiri;
            this.joonista = joonista;
            this.olek = olek;
            Voti = voti;
            Sisu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(30, 4, 20, 30),
                BackColor = Teema.Taust
            };
            Sisu.Resize += (s, e) => sobita(Sisu);
            Controls.Add(Sisu);
        }

        public void Joonista()
        {
            joonistatud = true;
            Aegunud = false;
            joonista();
        }

        public override void Avatud()
        {
            olek();
            if (!joonistatud || Aegunud) Joonista();
        }
    }

    // ===================================================================
    //  Riiul: pealkiri + "Näita kõiki" + horisontaalne kaartide rida noolega
    // ===================================================================
    public class Riiul : Panel
    {
        private const int KaardiLaius = 222, Vahe = 18;
        private readonly Panel aken, liugur;
        private readonly RingNupp vasak, parem;
        private int nihe;

        public Riiul(string pealkiri, List<Toode> tooted, Action naitaKoiki, Func<TooteKaart, TooteKaart> uhenda)
        {
            Height = 470;
            Margin = new Padding(0, 0, 0, 22);
            BackColor = Teema.Taust;

            var l = new Label { Text = pealkiri, Location = new Point(0, 0), AutoSize = true, Font = new Font("Segoe UI Semibold", 20F), ForeColor = Teema.Tekst, BackColor = Teema.Taust };
            var koik = new IkoonTekst("", "Näita kõiki", Teema.Teine, new Font("Segoe UI Semibold", 11.5F));
            koik.Click += (s, e) => naitaKoiki();
            Controls.Add(l);
            Controls.Add(koik);

            aken = new Panel { Location = new Point(0, 56), Height = 400, BackColor = Teema.Taust };
            liugur = new Panel { Location = new Point(0, 0), Height = 400, BackColor = Teema.Taust };
            int x = 0;
            foreach (var t in tooted)
            {
                var k = uhenda(new TooteKaart(t));
                k.Location = new Point(x, 4);
                liugur.Controls.Add(k);
                x += KaardiLaius + Vahe;
            }
            liugur.Width = Math.Max(1, x - Vahe);
            aken.Controls.Add(liugur);
            Controls.Add(aken);

            vasak = new RingNupp(Ikoonid.NoolVasakule) { Visible = false };
            parem = new RingNupp(Ikoonid.NoolParemale);
            vasak.Click += (s, e) => Liiguta(-1);
            parem.Click += (s, e) => Liiguta(1);
            Controls.Add(vasak);
            Controls.Add(parem);
            vasak.BringToFront();
            parem.BringToFront();

            Resize += (s, e) =>
            {
                koik.Location = new Point(Width - koik.Width - 64, 12);
                aken.Width = Width - 60;
                Liiguta(0);
            };
        }

        private void Liiguta(int suund)
        {
            int samm = Math.Max(1, aken.Width / (KaardiLaius + Vahe)) * (KaardiLaius + Vahe);
            int max = Math.Max(0, liugur.Width - aken.Width);
            nihe = Math.Clamp(nihe + suund * samm, 0, max);
            liugur.Left = -nihe;

            vasak.Location = new Point(-2, aken.Top + 170);
            parem.Location = new Point(aken.Right + 8, aken.Top + 170);
            vasak.Visible = nihe > 0;
            parem.Visible = nihe < max;
        }
    }

    // Ümmargune valge noolenupp (riiuli kerimiseks)
    public class RingNupp : Control
    {
        private readonly string glyph;
        private bool hover;

        public RingNupp(string glyph)
        {
            this.glyph = glyph;
            Size = new Size(50, 50);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Teema.Taust);
            for (int i = 3; i >= 1; i--)
                using (var v = new SolidBrush(Color.FromArgb(Teema.Tume ? 40 : 12, Color.Black)))
                    g.FillEllipse(v, 3 - i * 0.5f, 3 + i * 0.6f, 42 + i, 42 + i);
            using (var b = new SolidBrush(hover ? Teema.PindAlt : Teema.Pind))
            using (var p = new Pen(Teema.Joon))
            {
                g.FillEllipse(b, 3, 2, 42, 42);
                g.DrawEllipse(p, 3, 2, 42, 42);
            }
            Ikoonid.Joonista(g, glyph, new Rectangle(3, 2, 42, 42), Teema.Teine, 11f);
        }
    }

    // ===================================================================
    //  Väikesed elemendid: logo, ikooniga tekst, ostukorvi nupp, kategooria ikoon
    // ===================================================================
    // AKT logo: punane silt, valge ostukott ja suured valged tähed "AKT", all väike "pood"
    public class Logo : Control
    {
        public Logo()
        {
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor ?? Teema.Pind);
            Joonista(e.Graphics, new RectangleF(0, 0, Width - 1, Height - 1));
        }

        // Joonistab logo antud ristkülikusse (kasutavad ka sisselogimisaken ja PDF-i päis)
        public static void Joonista(Graphics g, RectangleF r, bool valgeTaustal = true)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float h = r.Height;

            // Punane silt
            using (var path = Ymardus.Tee(r, h * 0.18f))
            using (var b = new SolidBrush(Teema.Aktsent))
                g.FillPath(b, path);
            if (!valgeTaustal)
                using (var p = new Pen(Color.FromArgb(90, Color.White), 1.5f))
                using (var path2 = Ymardus.Tee(r, h * 0.18f))
                    g.DrawPath(p, path2);

            // Ostukott
            float kx = r.X + h * 0.20f, ky = r.Y + h * 0.36f, kw = h * 0.46f, kh = h * 0.44f;
            using (var valge = new SolidBrush(Color.White))
            using (var sang = new Pen(Color.White, Math.Max(2f, h * 0.07f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var aktsent = new SolidBrush(Teema.Aktsent))
            {
                g.DrawArc(sang, kx + kw * 0.22f, ky - kh * 0.42f, kw * 0.56f, kh * 0.80f, 180, 180);
                using (var kott = Ymardus.Tee(new RectangleF(kx, ky, kw, kh), h * 0.06f))
                    g.FillPath(valge, kott);
                float t = h * 0.075f;
                g.FillEllipse(aktsent, kx + kw * 0.22f - t / 2, ky + kh * 0.22f, t, t);
                g.FillEllipse(aktsent, kx + kw * 0.78f - t / 2, ky + kh * 0.22f, t, t);
            }

            // AKT
            float tx = r.X + h * 0.82f;
            float tw = r.Right - tx - h * 0.12f;
            using (var f = new Font("Segoe UI Black", h * 0.50f, GraphicsUnit.Pixel))
                TextRenderer.DrawText(g, Arve.PoeNimi.ToUpper(), f,
                    new Rectangle((int)tx, (int)(r.Y + h * 0.02f), (int)tw, (int)(h * 0.66f)), Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
            using (var f2 = new Font("Segoe UI Semibold", Math.Max(7f, h * 0.20f), GraphicsUnit.Pixel))
                TextRenderer.DrawText(g, "p o o d", f2,
                    new Rectangle((int)tx + 2, (int)(r.Y + h * 0.62f), (int)tw, (int)(h * 0.30f)), Color.FromArgb(235, Color.White),
                    TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        }
    }

    // Ikoon + tekst (lingid, menüüriba)
    // "Esc  Alt+←  tagasi" hallil kiibil (klahvid valgetel nuppudel)
    public class KlahviVihje : Control
    {
        public KlahviVihje()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            Keel.Muutus += Sobita;
            Sobita();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Keel.Muutus -= Sobita;
            base.Dispose(disposing);
        }

        private void Sobita()
        {
            using var fk = new Font("Consolas", 8F);
            Size = new Size(14 + TextRenderer.MeasureText("Esc", fk).Width + 14 + 8 + TextRenderer.MeasureText("Alt+←", fk).Width + 14 + 8 + TextRenderer.MeasureText(Keel.T("tagasi"), Font).Width + 14, 36);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Pind);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 18)) using (var b = new SolidBrush(Teema.Paneel2)) g.FillPath(b, tee);
            using var fk = new Font("Consolas", 8F);
            float x = 14;
            foreach (var k in new[] { "Esc", "Alt+←" })
            {
                int w = TextRenderer.MeasureText(k, fk).Width + 10;
                var r = new RectangleF(x, 8, w, 20);
                using (var tee = Ymardus.Tee(r, 6))
                {
                    using (var b = new SolidBrush(Teema.Pind)) g.FillPath(b, tee);
                    using (var p = new Pen(Teema.Joon)) g.DrawPath(p, tee);
                }
                TextRenderer.DrawText(g, k, fk, Rectangle.Round(r), Teema.Tekst2, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                x += w + 8;
            }
            TextRenderer.DrawText(g, Keel.T("tagasi"), Font, new Rectangle((int)x, 0, Width - (int)x, Height), Teema.Tekst2, TextFormatFlags.VerticalCenter);
        }
    }

    public class IkoonTekst : Control
    {
        private readonly string ikoon, tekst;
        private readonly Color varv;
        private readonly bool emoji, ikoonParemal;
        private bool hover;
        public bool Valitud { get; set; }

        public IkoonTekst(string ikoon, string tekst, Color varv, Font font, bool emoji = false, bool ikoonParemal = false)
        {
            Keel.Muutus += Sobita;
            this.ikoon = ikoon;
            this.tekst = tekst;
            this.varv = varv;
            this.emoji = emoji;
            this.ikoonParemal = ikoonParemal;
            Font = font;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            Sobita();
        }

        private void Sobita()
        {
            var t = TextRenderer.MeasureText(Keel.T(tekst), Font);
            int ikooniLaius = ikoon.Length > 0 ? (tekst.Length > 0 ? 26 : 20) : 0;
            Size = new Size(t.Width + ikooniLaius, Math.Max(24, t.Height + 4));
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Keel.Muutus -= Sobita;
            base.Dispose(disposing);
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? BackColor);
            Color v = !Enabled ? Teema.Joon
                    : varv == Color.White ? Color.White
                    : Valitud || hover ? Teema.Teine
                    : varv;

            int ikooniLaius = ikoon.Length > 0 ? (tekst.Length > 0 ? 26 : 20) : 0;
            int tekstiX = ikoonParemal ? 0 : ikooniLaius;
            var ikooniKast = ikoonParemal ? new Rectangle(Width - 20, 0, 20, Height) : new Rectangle(0, 0, 20, Height);

            if (ikoon.Length > 0)
            {
                if (emoji) Ikoonid.JoonistaEmoji(g, ikoon, ikooniKast, Color.FromArgb(235, 90, 20), 11f);
                else Ikoonid.Joonista(g, ikoon, ikooniKast, v, ikoonParemal ? 9f : 11f);
            }
            using var f = new Font(Font, Valitud ? FontStyle.Bold : Font.Style | (hover && Enabled ? FontStyle.Underline : FontStyle.Regular));
            TextRenderer.DrawText(g, Keel.T(tekst), f, new Rectangle(tekstiX, 0, Width - ikooniLaius, Height), v,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        }
    }

    // Ostukorvi nupp päises: ikoon + arv + summa
    public class KorviNupp : Control
    {
        private bool hover;

        public KorviNupp()
        {
            Size = new Size(140, 48);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Teema.Pind);
            using (var path = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 8))
            using (var b = new SolidBrush(hover ? Ymardus.Segu(Teema.Paneel2, Teema.Teine, 0.08f) : Teema.Paneel2))
                g.FillPath(b, path);

            Ikoonid.Joonista(g, Ikoonid.Ostukorv, new Rectangle(10, 6, 34, Height - 6), Teema.Teine, 15f);

            // Arvu märk
            var mark = new RectangleF(28, 6, 20, 20);
            using (var b = new SolidBrush(Color.FromArgb(0, 150, 136)))
                g.FillEllipse(b, mark);
            using (var f = new Font("Segoe UI Semibold", 7.5F))
                TextRenderer.DrawText(g, Math.Min(99, Ostukorv.Kogus).ToString(), f, Rectangle.Round(mark), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            using var sf = new Font("Segoe UI Semibold", 11F);
            TextRenderer.DrawText(g, Ostukorv.Eur(Ostukorv.Kokku), sf, new Rectangle(54, 0, Width - 60, Height), Teema.Tekst,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }

    // Kategooria ikoon vasakul ribal
    public class RailNupp : Control
    {
        public int KategooriaId { get; }
        public bool Valitud { get; set; }
        private readonly string ikoon;
        private bool hover;

        public RailNupp(int id, string ikoon)
        {
            KategooriaId = id;
            this.ikoon = ikoon;
            Height = 48;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Valitud ? Ymardus.Segu(Teema.Pind, Teema.Teine, Teema.Tume ? 0.25f : 0.10f) : hover ? Teema.PindAlt : Teema.Pind);
            if (Valitud)
                using (var b = new SolidBrush(Teema.Teine))
                    g.FillRectangle(b, Width - 3, 0, 3, Height);
            Ikoonid.JoonistaEmoji(g, ikoon, new Rectangle(0, 0, Width, Height), Teema.Teine, 14f);
            using var p = new Pen(Teema.Joon);
            g.DrawLine(p, 8, Height - 1, Width - 8, Height - 1);
        }
    }

    // ===================================================================
    //  Tootekaart e-poe stiilis
    // ===================================================================
    public class TooteKaart : Control
    {
        public Toode Toode { get; }
        public event Action<Toode>? Lisa;
        public event Action<Toode>? Vahenda;
        public event Action<Toode>? Ava;

        private bool hover;
        private Point hiir;
        private double tempel = 1;  // õhtuse soodustuse "tempel" (0 → 1)
        private bool sobimatu;     // ei sobi "Minu dieet" valikuga
        private double tuhm;       // 0 → 1 (tuhmumise animatsioon)

        public TooteKaart(Toode toode)
        {
            Toode = toode;
            sobimatu = !MinuDieet.Sobib(toode);
            tuhm = sobimatu ? 1 : 0;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(222, 390);
            Margin = new Padding(5);
            Cursor = Cursors.Hand;
        }

        // Õhtune soodustus algas: "−50%" tempel lendab peale ja hind kukub
        public void Tempel() => Animatsioon.Kaivita(this, 650, t => { tempel = t; Invalidate(); });

        // Dieet muutus: kaart tuhmub (0,38 läbipaistvus, hall) või tuleb tagasi, 450 ms
        public void UuendaDieet()
        {
            bool uus = !MinuDieet.Sobib(Toode);
            if (uus == sobimatu) { Invalidate(); return; }
            sobimatu = uus;
            double algus = tuhm, siht = uus ? 1 : 0;
            Animatsioon.Kaivita(this, 450, t => { tuhm = algus + (siht - algus) * Animatsioon.Sujuv(t); Invalidate(); });
        }

        private RectangleF Kaart => new(1, 1, Width - 4, Height - 4);
        private Rectangle Nupp => !Teema.OnMoodne ? new((int)Kaart.X + 16, (int)Kaart.Bottom - 50, (int)Kaart.Width - 32, 34)
            : Ostukorv.KorvisKogus(Toode.Id) > 0 ? new((int)Kaart.Right - 122, (int)Kaart.Bottom - 56, 110, 44)
            : new((int)Kaart.Right - 56, (int)Kaart.Bottom - 56, 44, 44);   // moodne: ümar "+" või tume stepper
        private Rectangle Taht => new((int)Kaart.Right - 50, (int)Kaart.Bottom - 104, 32, 32);

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool enne = Nupp.Contains(hiir) || Taht.Contains(hiir);
            hiir = e.Location;
            if (enne != (Nupp.Contains(hiir) || Taht.Contains(hiir)) || Nupp.Contains(hiir)) Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left) return;

            if (Taht.Contains(e.Location))
            {
                Lemmikud.Vaheta(Toode.Id);
                Teade.Toast(Lemmikud.On(Toode.Id) ? "Lisatud lemmikutesse ★" : "Eemaldatud lemmikutest", TeateTyyp.Info, 1400);   // Toast tõlgib ise
                Invalidate();
                return;
            }

            var nupp = Nupp;
            if (!nupp.Contains(e.Location)) { Ava?.Invoke(Toode); return; }

            int korvis = Ostukorv.KorvisKogus(Toode.Id);
            if (korvis > 0)
            {
                if (e.X < nupp.X + 40) Vahenda?.Invoke(Toode);
                else if (e.X > nupp.Right - 40) Lisa?.Invoke(Toode);
            }
            else
            {
                Lisa?.Invoke(Toode);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Teema.Taust);

            var kaart = Kaart;
            bool laos = Toode.Kogus > 0;

            if (Teema.OnMoodne)
            {
                // Moodne: hall ümar kaart (raadius 24), pilt valgel ümaral alusel; hiire all tõuseb
                if (hover)
                    using (var vari = new SolidBrush(Color.FromArgb(22, 0, 0, 0)))
                    using (var vt = Ymardus.Tee(new RectangleF(kaart.X + 2, kaart.Y + 6, kaart.Width - 4, kaart.Height - 4), 24))
                        g.FillPath(vari, vt);
                if (hover) kaart.Offset(0, -3);
                using (var path = Ymardus.Tee(kaart, 24))
                using (var b = new SolidBrush(OhtuSoodustus.Kehtib(Toode, Hinnad.Kell()) && !Teema.Tume ? Color.FromArgb(255, 247, 224) : Teema.Paneel2))   // õhtu: kreemjas (N12)
                    g.FillPath(b, path);
                using (var path = Ymardus.Tee(new RectangleF(kaart.X + 10, kaart.Y + 10, kaart.Width - 20, 162), 18))
                using (var b = new SolidBrush(Teema.Pind))
                    g.FillPath(b, path);
            }
            else
            using (var path = Ymardus.Tee(kaart, 4))
            using (var b = new SolidBrush(Teema.Pind))
            using (var p = new Pen(hover ? Ymardus.Segu(Teema.Joon, Teema.Tekst, 0.15f) : Teema.Joon))
            {
                g.FillPath(b, path);
                g.DrawPath(p, path);
            }

            // Pilt (valge taust, keskel)
            var pildiKast = new RectangleF(kaart.X + 18, kaart.Y + 16, kaart.Width - 36, 150);
            if (PildiVahemalu.Too(Toode.Pilt) is Image pilt)
                PildiVahemalu.Joonista(g, pilt, pildiKast);
            else
                Ikoonid.JoonistaEmoji(g, Ikoonid.KategooriaIkoon(Toode.Kategooria?.Kategooria_nimetus),
                    Rectangle.Round(pildiKast), Teema.Joon, 40f);

            // Moodne: roheline pandimünt pildi paremas alanurgas (lõuend N8)
            if (Teema.OnMoodne && Toode.Pant > 0)
            {
                using var fm = new Font("Segoe UI", 8F, FontStyle.Bold);
                string pt = "+" + Ostukorv.Eur(Toode.Pant);
                int mw = TextRenderer.MeasureText(pt, fm).Width + 34;
                var munt = new RectangleF(pildiKast.Right - mw, pildiKast.Bottom - 30, mw, 28);
                using (var tee = Ymardus.Tee(munt, 14)) using (var b = new SolidBrush(Color.FromArgb(20, 130, 70))) g.FillPath(b, tee);
                var p0 = new RectangleF(munt.X + 4, munt.Y + 4, 20, 20);
                using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, p0);
                using (var fp = new Font("Segoe UI", 7F, FontStyle.Bold))
                    TextRenderer.DrawText(g, "P", fp, Rectangle.Round(p0), Color.FromArgb(20, 130, 70), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, pt, fm, new Rectangle((int)munt.X + 26, (int)munt.Y, mw - 28, 28), Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }

            // Soodustus: lilla ring paremal üleval (õhtune soodustus punane "tempel", mis lendab peale)
            var h = Hinnad.Lopphind(Toode);
            if (h.Soodus)
            {
                bool ohtu = OhtuSoodustus.Kehtib(Toode, Hinnad.Kell());
                float s = (float)(ohtu ? 1 + 1.2 * (1 - Animatsioon.Pop(tempel)) : 1);
                var ring = new RectangleF(kaart.Right - 50, kaart.Y + 14, 36, 36);
                var olek0 = g.Save();
                g.TranslateTransform(ring.X + 18, ring.Y + 18);
                g.ScaleTransform(s, s);
                if (ohtu) g.RotateTransform((float)(-20 * (1 - tempel)) - 6);
                g.TranslateTransform(-(ring.X + 18), -(ring.Y + 18));
                using var b = new SolidBrush(Color.FromArgb((int)(255 * Math.Clamp(tempel * 1.5, 0, 1)), ohtu ? Teema.Aktsent : Teema.Teine));
                g.FillEllipse(b, ring);
                using var f = new Font("Segoe UI Semibold", 7.5F);
                TextRenderer.DrawText(g, $"-{h.Protsent}%", f, Rectangle.Round(ring), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                g.Restore(olek0);
            }

            // Lipp + laoseis pildi all
            int my = (int)pildiKast.Bottom + 10;
            int mx = (int)kaart.X + 16;
            if ((Toode.Paritolumaa ?? "").Equals("Eesti", StringComparison.OrdinalIgnoreCase))
            {
                JoonistaLipp(g, mx, my);
                mx += 26;
            }
            if (!laos)
                VaikeMark(g, Keel.T("Otsas"), Teema.TekstNork, mx, my - 3);
            else if (Toode.Kogus < Form1.VaikeLaoseis)
                VaikeMark(g, Keel.F("Vähe: {0} tk", Toode.Kogus), Teema.Hoiatus, mx, my - 3);

            // Nimi keskel
            int x = (int)kaart.X + 16, w = (int)kaart.Width - 32;
            using (var nimi = new Font("Segoe UI", 10.5F))
                TextRenderer.DrawText(g, Toode.Toodenimetus, nimi, new Rectangle(x, my + 20, w, 62), Teema.Tekst2,
                    TextFormatFlags.WordBreak | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            // Hind
            // Hind (uus hind "kukub" ülalt, kui soodustus just algas)
            bool soodus = h.Soodus;
            int hinnaY = (int)kaart.Bottom - 108 - (int)((1 - Animatsioon.Pop(tempel)) * 14);
            int laius = Ikoonid.JoonistaHind(g, h.Hind, "tk", x, hinnaY, soodus ? Teema.Aktsent : Teema.Tekst, 25f);
            if (soodus)
            {
                using var vana = new Font("Segoe UI Semibold", 9.5F, FontStyle.Strikeout);
                TextRenderer.DrawText(g, Ikoonid.Hind(h.TavaHind), vana, new Point(x + laius + 4, hinnaY + 21), Teema.Tekst2);
            }
            // Moodne: õhtuse soodustuse toode enne algust – "20:00 → 0,85 €" ploomiga (lõuend N12)
            var ohtuS = OhtuSoodustus.S;
            if (Teema.OnMoodne && ohtuS.Sees && Toode.KategooriaId == ohtuS.KategooriaId && !OhtuSoodustus.Kaib(Hinnad.Kell()))
            {
                decimal pool = Math.Round(Kampaaniad.TavaHind(Toode) * (100 - ohtuS.Protsent) / 100m, 2, MidpointRounding.AwayFromZero);
                if (pool < h.Hind)
                {
                    // ploomi kiip pildi vasakus alanurgas (all paremal on koguse nupp)
                    using var fo = new Font("Segoe UI", 8F, FontStyle.Bold);
                    string ot = $"{ohtuS.Algus:hh\\:mm} → {Ostukorv.Eur(pool)}";
                    int ow = TextRenderer.MeasureText(ot, fo).Width + 16;
                    var okiip = new RectangleF(pildiKast.X, pildiKast.Bottom - 26, ow, 24);
                    using (var tee = Ymardus.Tee(okiip, 12)) using (var b = new SolidBrush(Color.FromArgb(246, 230, 239))) g.FillPath(b, tee);
                    TextRenderer.DrawText(g, ot, fo, Rectangle.Round(okiip), Teema.Teine, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
            }
            string? alumine = Toode.Pant > 0
                ? "+ " + Keel.F("Pakendi tagatisraha {0}", Ostukorv.Eur(Toode.Pant))
                : Toode.UhikuHind is decimal uh && Toode.Uhik is string u && u != "tk"
                ? $"{Ikoonid.Hind(uh)}/{u}"
                : Toode.Pakend;
            if (alumine != null)
            {
                using var v = new Font("Segoe UI", 9F);
                TextRenderer.DrawText(g, alumine.Replace("€", " €"), v, new Point(x, hinnaY + 42), Teema.TekstNork);
            }

            // Lemmiku tärn
            var taht = Taht;
            bool lemmik = Lemmikud.On(Toode.Id);
            using (var tp = Ymardus.Tee(taht, 4))
            using (var tb = new SolidBrush(hover && taht.Contains(hiir) ? Teema.Paneel2 : Ymardus.Segu(Teema.Pind, Teema.Paneel2, 0.6f)))
                g.FillPath(tb, tp);
            Ikoonid.Joonista(g, lemmik ? "" : "", taht, lemmik ? Teema.Teine : Teema.TekstNork, 10f);

            JoonistaNupp(g, laos);

            // Säilivusaja allahindlus: "−30 % · aegub 25.10"
            if (Aegumine.AllahindlusKuni(Toode.Id) is DateTime aegub)
            {
                using var fa = new Font("Segoe UI Semibold", 7.5F);
                string tekst = Keel.F("−{0} % · aegub {1:dd.MM}", Aegumine.Protsent, aegub);
                int aw = TextRenderer.MeasureText(tekst, fa).Width + 14;
                var ar = new RectangleF(kaart.X + 12, kaart.Y + 150, aw, 20);
                using (var tee = Ymardus.Tee(ar, 10))
                using (var b = new SolidBrush(Teema.Hoiatus))
                    g.FillPath(b, tee);
                TextRenderer.DrawText(g, tekst, fa, Rectangle.Round(ar), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // Dieedi kiibid pildi vasakus ülanurgas
            DieediMargid.Joonista(g, Toode, kaart.X + 12, kaart.Y + 12, kaart.Width - 70);

            // Ei sobi: kaart tuhmub (taustavärvi kiht) ja keskel must silt
            if (tuhm > 0.01)
            {
                using (var b = new SolidBrush(Color.FromArgb((int)(tuhm * 0.62 * 255), Teema.Taust)))
                using (var path = Ymardus.Tee(kaart, 4))
                    g.FillPath(b, path);
                using var f = new Font("Segoe UI Semibold", 9F);
                string tekst = Keel.T("Ei sobi");
                int tw = TextRenderer.MeasureText(tekst, f).Width + 22;
                var r = new RectangleF(kaart.X + (kaart.Width - tw) / 2, kaart.Y + 80, tw, 28);
                using (var tee = Ymardus.Tee(r, 14))
                using (var b = new SolidBrush(Color.FromArgb((int)(tuhm * 230), 27, 28, 31)))
                    g.FillPath(b, tee);
                TextRenderer.DrawText(g, tekst, f, Rectangle.Round(r), Color.FromArgb((int)(tuhm * 255), Color.White),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
        }

        private static void JoonistaLipp(Graphics g, int x, int y)
        {
            using var sinine = new SolidBrush(Color.FromArgb(0, 114, 206));
            using var must = new SolidBrush(Color.Black);
            using var valge = new SolidBrush(Color.White);
            g.FillRectangle(sinine, x, y, 18, 4);
            g.FillRectangle(must, x, y + 4, 18, 4);
            g.FillRectangle(valge, x, y + 8, 18, 4);
            using var p = new Pen(Color.FromArgb(200, 200, 200));
            g.DrawRectangle(p, x, y, 18, 12);
        }

        private static void VaikeMark(Graphics g, string tekst, Color varv, int x, int y)
        {
            using var f = new Font("Segoe UI Semibold", 8F);
            TextRenderer.DrawText(g, tekst, f, new Point(x, y), varv, TextFormatFlags.NoPrefix);
        }

        // "Lisa ostukorvi" või korvis olles "− N tk +"
        private void JoonistaNupp(Graphics g, bool laos)
        {
            var r = Nupp;
            bool hiirPeal = hover && r.Contains(hiir);
            int korvis = Ostukorv.KorvisKogus(Toode.Id);

            if (Teema.OnMoodne)
            {
                // ümar punane "+" või tume stepper "− N +"
                if (!laos)
                {
                    using var b0 = new SolidBrush(Teema.Joon);
                    g.FillEllipse(b0, r);
                    Ikoonid.Joonista(g, Ikoonid.Sulge, r, Teema.TekstNork, 9f);
                    return;
                }
                if (korvis == 0)
                {
                    using var b1 = new SolidBrush(hiirPeal ? Teema.AktsentTume : Teema.Aktsent);
                    var rr = hiirPeal ? Rectangle.Inflate(r, 2, 2) : r;
                    g.FillEllipse(b1, rr);
                    Ikoonid.Joonista(g, Ikoonid.Pluss, r, Color.White, 12f);
                    return;
                }
                using (var tp = Ymardus.Tee(r, r.Height / 2))
                using (var b2 = new SolidBrush(Color.FromArgb(27, 28, 31)))
                    g.FillPath(b2, tp);
                Ikoonid.Joonista(g, Ikoonid.Miinus, new Rectangle(r.X, r.Y, 40, r.Height), Color.White, 10f);
                Ikoonid.Joonista(g, Ikoonid.Pluss, new Rectangle(r.Right - 40, r.Y, 40, r.Height), Color.White, 10f);
                using (var f = new Font("Segoe UI", 10.5F, FontStyle.Bold))
                    TextRenderer.DrawText(g, korvis.ToString(), f, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            using var path = Ymardus.Tee(r, 5);
            if (!laos)
            {
                using var b = new SolidBrush(Teema.Paneel2);
                g.FillPath(b, path);
                using var f = new Font("Segoe UI Semibold", 9F);
                TextRenderer.DrawText(g, Keel.T("Pole laos"), f, r, Teema.TekstNork, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            if (korvis == 0)
            {
                using var b = new SolidBrush(hiirPeal ? Teema.AktsentTume : Teema.Aktsent);
                g.FillPath(b, path);
                using var f = new Font("Segoe UI Semibold", 8.75F);
                TextRenderer.DrawText(g, Keel.T("Lisa ostukorvi"), f, new Rectangle(r.X + 12, r.Y, r.Width - 40, r.Height), Color.White,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                Ikoonid.Joonista(g, Ikoonid.Ostukorv, new Rectangle(r.Right - 34, r.Y, 26, r.Height), Color.White, 12f);
                return;
            }

            // Stepper: − N tk +
            using (var b = new SolidBrush(Teema.Pind))
            using (var p = new Pen(Teema.Aktsent, 1.5f))
            {
                g.FillPath(b, path);
                g.DrawPath(p, path);
            }
            var vasak = new Rectangle(r.X, r.Y, 40, r.Height);
            var parem = new Rectangle(r.Right - 40, r.Y, 40, r.Height);
            using (var pp = Ymardus.Tee(new RectangleF(parem.X, parem.Y, parem.Width, parem.Height), 5))
            using (var b = new SolidBrush(hiirPeal && parem.Contains(hiir) ? Teema.AktsentTume : Teema.Aktsent))
                g.FillPath(b, pp);
            Ikoonid.Joonista(g, Ikoonid.Miinus, vasak, Teema.Aktsent, 10f);
            Ikoonid.Joonista(g, Ikoonid.Pluss, parem, Color.White, 10f);
            using (var f = new Font("Segoe UI Semibold", 10F))
                TextRenderer.DrawText(g, Keel.F("{0} tk", korvis), f, r, Teema.Tekst, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
