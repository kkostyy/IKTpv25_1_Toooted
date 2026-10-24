using System.Drawing.Drawing2D;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    // Ühine alus dialoogivormidele: teema värvid, ümardatud raamiga väljad
    public abstract class KujundusVorm : Form, ITagasi
    {
        private Label? pealkirjaSilt;

        // Kas aknal on nupp "← Tagasi" (sisselogimisel pole kuhugi tagasi minna)
        protected virtual bool TagasiNupuga => true;

        // Muutmisvormid: kas sulgemisel küsida, kui midagi on muudetud
        protected bool KysiSulgemisel { get; set; }
        protected bool Muudetud { get; set; }

        protected KujundusVorm(string pealkiri, int laius, int korgus)
        {
            Text = pealkiri;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = UiStiil.Põhifont;
            BackColor = Teema.Pind;
            ForeColor = Teema.Tekst;
            ClientSize = new Size(laius, korgus);
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);          // Load-sündmused (andmete laadimine) enne
            if (TagasiNupuga)
            {
                UiStiil.LisaTagasiNupp(this, pealkirjaSilt);
                KeyPreview = true;
                if (KysiSulgemisel) JalgiMuudatusi(this);
            }
            Teema.Rakenda(this);     // siis värvime kõik teema järgi
        }

        // ← Tagasi, Esc, Alt+←, hiire külgnupp: aken sulgub (muudatuste korral küsib enne)
        public bool MineTagasi()
        {
            if (Modal) DialogResult = DialogResult.Cancel;
            Close();
            return IsDisposed || !Visible;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (TagasiNupuga && (keyData == (Keys.Alt | Keys.Left) ||
                (keyData == Keys.Escape && CancelButton == null && !(ActiveControl is TextBox t && t.TextLength > 0 && !t.ReadOnly))))
            {
                MineTagasi();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void JalgiMuudatusi(Control juur)
        {
            foreach (Control c in juur.Controls)
            {
                switch (c)
                {
                    case TextBox t: t.TextChanged += (s, e) => Muudetud = true; break;
                    case ComboBox cb: cb.SelectedIndexChanged += (s, e) => Muudetud = true; break;
                    case NumericUpDown n: n.ValueChanged += (s, e) => Muudetud = true; break;
                }
                JalgiMuudatusi(c);
            }
        }

        // Salvestamata muudatused: küsitakse igal sulgemisviisil (Tagasi, Esc, ✕)
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (KysiSulgemisel && Muudetud && DialogResult != DialogResult.OK && e.CloseReason == CloseReason.UserClosing)
            {
                if (Teade.Show("Muudatused on salvestamata. Kas sulgen ikkagi?", "Tagasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    DialogResult = DialogResult.None;
                }
            }
            base.OnFormClosing(e);
        }

        protected Label Silt(string tekst, int x, int y)
        {
            var l = new Label { Text = tekst, AutoSize = true, Location = new Point(x, y), ForeColor = Teema.Tekst2 };
            Controls.Add(l);
            return l;
        }

        // Ümardatud raamiga tekstiväli (raam fookuses aktsentvärvi)
        protected TextBox Sisend(int x, int y, int laius, bool parool = false)
        {
            var raam = UiStiil.LooSisend(out TextBox t, laius, parool);
            raam.Location = new Point(x, y - 6);
            Controls.Add(raam);
            return t;
        }

        protected Button Nupp(string tekst, int x, int y, bool peamine = false)
        {
            var b = UiStiil.LooNupp(tekst, null, peamine);
            b.Location = new Point(x, y);
            Controls.Add(b);
            return b;
        }

        protected Label Pealkiri(string tekst)
        {
            var l = new Label
            {
                Text = tekst,
                AutoSize = true,
                Location = new Point(18, 14),
                Font = UiStiil.PealkirjaFont,
                ForeColor = Teema.Tekst
            };
            Controls.Add(l);
            pealkirjaSilt ??= l;
            return l;
        }

        protected Label Veateade(int x, int y, int laius)
        {
            var l = new Label { Location = new Point(x, y), Size = new Size(laius, 36), ForeColor = Teema.Viga };
            Controls.Add(l);
            return l;
        }

        // Ühe välja plokk (silt + raamiga väli) tabeli lahtrisse
        protected static Panel ValjaPlokk(string silt, out TextBox tekst, string vihje = "")
        {
            var plokk = new Panel { Dock = DockStyle.Fill, Height = 62, Margin = new Padding(0, 0, 12, 0), BackColor = Teema.Pind };
            var raam = UiStiil.LooSisend(out tekst, 200, false, vihje);
            raam.Dock = DockStyle.Top;
            var l = new Label { Text = silt, Dock = DockStyle.Top, Height = 22, ForeColor = Teema.Tekst2, BackColor = Teema.Pind };
            plokk.Controls.Add(raam);
            plokk.Controls.Add(l);
            return plokk;
        }
    }

    // ===================================================================
    //  Sisselogimine: vasakul poe "bränd", paremal vorm
    // ===================================================================
    public class LoginVorm : KujundusVorm
    {
        private readonly TextBox txtNimi, txtParool;
        private readonly Label lblViga;
        private readonly BrandPaneel bränd;
        private readonly LinkLabel teemaLink;

        protected override bool TagasiNupuga => false;

        public LoginVorm() : base(Keel.F("Sisselogimine – {0}", Arve.PoeNimi), 740, 450)
        {
            bränd = new BrandPaneel { Dock = DockStyle.Left, Width = 320 };
            Controls.Add(bränd);

            const int x = 368, w = 330;

            Controls.Add(new Label { Text = "Tere tulemast!", Location = new Point(x - 2, 44), AutoSize = true, Font = new Font("Segoe UI Semibold", 20F), ForeColor = Teema.Tekst });
            Controls.Add(new Label { Text = "Logi sisse, et kassat kasutada", Location = new Point(x, 88), AutoSize = true, ForeColor = Teema.TekstNork, Font = new Font("Segoe UI", 10F) });

            Silt("Kasutajanimi", x, 132);
            var raamNimi = UiStiil.LooSisend(out txtNimi, w, false, "nt admin");
            raamNimi.Location = new Point(x, 152);
            Controls.Add(raamNimi);

            Silt("Parool", x, 198);
            var raamParool = UiStiil.LooSisend(out txtParool, w, true, "••••••");
            raamParool.Location = new Point(x, 218);
            Controls.Add(raamParool);

            var naita = new CheckBox { Text = "Näita parooli", Location = new Point(x + 2, 258), AutoSize = true, ForeColor = Teema.Tekst2 };
            naita.CheckedChanged += (s, e) => txtParool.UseSystemPasswordChar = !naita.Checked;
            Controls.Add(naita);

            lblViga = new Label { Location = new Point(x, 284), Size = new Size(w, 22), ForeColor = Teema.Viga };
            Controls.Add(lblViga);

            var login = UiStiil.LooSuurNupp("Logi sisse", (s, e) => LogiSisse());
            login.Bounds = new Rectangle(x, 310, w, 44);
            Controls.Add(login);
            AcceptButton = login;   // Enter-klahv vajutab nuppu

            var registreeri = new LinkLabel
            {
                Text = "Pole kontot? Registreeru (kliendikaart kohe)",
                AutoSize = true,
                Location = new Point(x, 368),
                LinkBehavior = LinkBehavior.HoverUnderline
            };
            registreeri.LinkClicked += (s, e) =>
            {
                using var vorm = new RegistreerimineVorm(lubaOmanik: false);
                if (vorm.ShowDialog(this) == DialogResult.OK)
                {
                    txtNimi.Text = vorm.UusKasutajanimi;
                    txtParool.Clear();
                    txtParool.Focus();
                }
            };
            Controls.Add(registreeri);

            var vihje = Silt("Vaikimisi: admin / admin123  ·  myyja / myyja123", x, 404);
            vihje.ForeColor = Teema.TekstNork;

            // Teema vahetus otse sisselogimisaknas
            teemaLink = new LinkLabel { AutoSize = true, Location = new Point(640, 12), LinkBehavior = LinkBehavior.HoverUnderline };
            teemaLink.LinkClicked += (s, e) =>
            {
                Teema.Sea(!Teema.Tume, Teema.AktsendiNimi);
                Teema.Rakenda(this);
                MargiTeema();
            };
            Controls.Add(teemaLink);
            MargiTeema();

            // Keele valik (Eesti / Русский / English)
            var keeled = new KeeleLingid(Teema.Teine) { Location = new Point(368, 14) };
            Controls.Add(keeled);
            Keel.Muutus += KeelMuutus;
            FormClosed += (s, e) => Keel.Muutus -= KeelMuutus;

            Shown += (s, e) => txtNimi.Focus();
        }

        private void KeelMuutus()
        {
            Text = Keel.F("Sisselogimine – {0}", Arve.PoeNimi);
            Teema.Rakenda(this);
            MargiTeema();
        }

        private void MargiTeema()
        {
            teemaLink.Text = Keel.T(Teema.Tume ? "☀ Hele teema" : "☾ Tume teema");
            teemaLink.Left = ClientSize.Width - teemaLink.PreferredWidth - 16;
            bränd.Invalidate();
        }

        private void LogiSisse()
        {
            string nimi = txtNimi.Text.Trim().ToLower();
            if (nimi.Length == 0) { lblViga.Text = "Sisesta kasutajanimi."; txtNimi.Focus(); return; }

            try
            {
                using var db = new ToodedContext();
                var kasutaja = db.Kasutajad.Include(k => k.Klient).FirstOrDefault(k => k.Kasutajanimi == nimi);

                // Sama teade mõlemal juhul: võõras ei saa teada, kas selline kasutaja on olemas
                if (kasutaja == null || !PasswordHelper.Verify(txtParool.Text, kasutaja.ParoolHash))
                {
                    lblViga.Text = "Vale kasutajanimi või parool.";
                    txtParool.Clear();
                    txtParool.Focus();
                    return;
                }

                AktiivneKasutaja.Sea(kasutaja);
                DialogResult = DialogResult.OK;   // sulgeb vormi
            }
            catch (Exception ex)
            {
                lblViga.Text = "Viga: " + ex.Message;
            }
        }
    }

    // Sisselogimisakna vasak pool: aktsentvärvi taust, logo ja poe nimi
    public class BrandPaneel : Panel
    {
        public BrandPaneel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (Width <= 0 || Height <= 0) return;

            using (var taust = new LinearGradientBrush(ClientRectangle, Teema.Teine, Ymardus.Segu(Teema.Teine, Color.Black, 0.3f), 60f))
                g.FillRectangle(taust, ClientRectangle);

            // Dekoratiivsed ringid
            using (var ring = new SolidBrush(Color.FromArgb(24, Color.White)))
            {
                g.FillEllipse(ring, Width - 160, -70, 260, 260);
                g.FillEllipse(ring, -90, Height - 150, 220, 220);
            }

            // AKT logo
            Logo.Joonista(g, new RectangleF(40, 52, 190, 72), valgeTaustal: false);

            using (var nimi = new Font("Segoe UI Semibold", 19F))
                TextRenderer.DrawText(g, Keel.T("Kassa ja e-pood"), nimi, new Rectangle(40, 150, Width - 60, 40), Color.White,
                    TextFormatFlags.NoPrefix);
            using (var vaike = new Font("Segoe UI", 10.5F))
                TextRenderer.DrawText(g, Keel.T("Tooted  ·  Kassa  ·  Kliendid  ·  Statistika"), vaike, new Rectangle(42, 196, Width - 60, 24),
                    Color.FromArgb(225, Color.White), TextFormatFlags.NoPrefix);
            using (var allkiri = new Font("Segoe UI", 8.5F))
                TextRenderer.DrawText(g, "IKTpv25  ·  WinForms .NET 8 + EF Core", allkiri, new Rectangle(42, Height - 40, Width - 60, 20),
                    Color.FromArgb(170, Color.White), TextFormatFlags.NoPrefix);
        }
    }

    // ===================================================================
    //  Registreerimine. Sisselogimisaknast: ostja (klient) või müüja.
    //  Ostjale luuakse kohe kliendikaart; e-post jääb meelde ja täidetakse
    //  maksmisel ise (seal saab seda muuta).
    // ===================================================================
    public class RegistreerimineVorm : KujundusVorm
    {
        private readonly TextBox txtNimi, txtTaisNimi, txtEmail, txtParool, txtKorda;
        private readonly ComboBox cmbRoll;
        private readonly CheckBox chkKaart;
        private readonly Label lblViga, lblEmail;
        private readonly List<string> rollid = new();

        public string UusKasutajanimi { get; private set; } = "";

        public RegistreerimineVorm(bool lubaOmanik) : base("Registreerimine", 400, 600)
        {
            Pealkiri("Uus kasutaja");
            const int w = 360;

            Silt("Roll", 20, 60);
            cmbRoll = new ComboBox { Location = new Point(20, 80), Width = w, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            rollid.Add(Kasutaja.Ostja);
            rollid.Add(Kasutaja.Myyja);
            if (lubaOmanik) rollid.Add(Kasutaja.Omanik);
            foreach (var r in rollid) cmbRoll.Items.Add(Keel.T(r));
            cmbRoll.SelectedIndex = 0;
            cmbRoll.SelectedIndexChanged += (s, e) => RolliMuutus();
            Controls.Add(cmbRoll);

            Silt("Kasutajanimi", 20, 120);
            txtNimi = Sisend(20, 140, w);
            Silt("Ees- ja perekonnanimi", 20, 180);
            txtTaisNimi = Sisend(20, 200, w);
            lblEmail = Silt("E-post (sinna tuleb PDF-tšekk)", 20, 240);
            txtEmail = Sisend(20, 260, w);
            txtEmail.PlaceholderText = "nimi@gmail.com";
            Silt("Parool (vähemalt 4 märki)", 20, 300);
            txtParool = Sisend(20, 320, w, true);
            Silt("Korda parooli", 20, 360);
            txtKorda = Sisend(20, 380, w, true);

            chkKaart = new CheckBox { Text = "Loo kliendikaart", Location = new Point(22, 418), AutoSize = true, ForeColor = Teema.Tekst2 };
            Controls.Add(chkKaart);

            var info = Silt("Ostja saab kliendikaardi kohe. Igast ostust lisandub boonust.", 20, 446);
            info.ForeColor = Teema.TekstNork;
            info.MaximumSize = new Size(w, 0);

            lblViga = Veateade(20, 480, w);

            var registreeri = Nupp("Registreeri", 20, 532, true);
            registreeri.Click += (s, e) => Registreeri();
            AcceptButton = registreeri;

            var tuhista = Nupp("Tühista", 140, 532);
            tuhista.DialogResult = DialogResult.Cancel;
            CancelButton = tuhista;

            RolliMuutus();
        }

        private string ValitudRoll => cmbRoll.SelectedIndex >= 0 ? rollid[cmbRoll.SelectedIndex] : Kasutaja.Ostja;

        // Ostjal on kaart alati ja e-post kohustuslik; töötajal valikuline
        private void RolliMuutus()
        {
            bool ostja = ValitudRoll == Kasutaja.Ostja;
            if (ostja) chkKaart.Checked = true;
            chkKaart.Enabled = !ostja;
            lblEmail.Text = Keel.T(ostja ? "E-post (sinna tuleb PDF-tšekk)" : "E-post (valikuline)");
        }

        private void Registreeri()
        {
            string nimi = txtNimi.Text.Trim().ToLower();
            string taisNimi = txtTaisNimi.Text.Trim();
            string email = txtEmail.Text.Trim();
            string roll = ValitudRoll;

            if (nimi.Length < 3) { lblViga.Text = Keel.T("Kasutajanimi peab olema vähemalt 3 märki."); return; }
            if (roll == Kasutaja.Ostja && email.Length == 0) { lblViga.Text = Keel.T("Sisesta e-post – sinna saadetakse tšekid."); txtEmail.Focus(); return; }
            if (email.Length > 0 && !EmailSeaded.OnKorrektneAadress(email)) { lblViga.Text = Keel.T("E-posti aadress pole korrektne!"); txtEmail.Focus(); return; }
            if (txtParool.Text.Length < 4) { lblViga.Text = Keel.T("Parool peab olema vähemalt 4 märki."); return; }
            if (txtParool.Text != txtKorda.Text) { lblViga.Text = Keel.T("Paroolid ei kattu."); return; }

            try
            {
                using var db = new ToodedContext();
                if (db.Kasutajad.Any(k => k.Kasutajanimi == nimi))
                {
                    lblViga.Text = Keel.T("Selline kasutajanimi on juba olemas.");
                    return;
                }

                var uus = new Kasutaja
                {
                    Kasutajanimi = nimi,
                    ParoolHash = PasswordHelper.Hash(txtParool.Text),
                    Roll = roll,
                    Nimi = taisNimi.Length > 0 ? taisNimi : null,
                    Email = email.Length > 0 ? email : null
                };

                // Kliendikaart: sama e-postiga olemasolev klient seotakse, muidu luuakse uus
                if (chkKaart.Checked)
                {
                    Klient? klient = email.Length > 0 ? db.Kliendid.FirstOrDefault(k => k.Email == email) : null;
                    klient ??= Kliendikaart.Loo(db, taisNimi.Length > 0 ? taisNimi : nimi, email);
                    uus.Klient = klient;
                }

                db.Kasutajad.Add(uus);
                db.SaveChanges();

                UusKasutajanimi = nimi;
                if (uus.Klient != null)
                    Teade.Show(Keel.F("Kasutaja „{0}“ on loodud.", nimi) + "\n\n" +
                               Keel.F("Sinu kliendikaardi number: {0}", uus.Klient.Kaardinumber) + "\n" +
                               Keel.T("Maksmisel täidetakse kaart ja e-post ise."),
                               Keel.T("Registreerimine"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    Teade.Toast(Keel.F("Kasutaja „{0}“ ({1}) on loodud", nimi, Keel.T(roll)));
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                lblViga.Text = Keel.T("Viga:") + " " + ex.Message;
            }
        }
    }

    // ===================================================================
    //  Kliendid (administraatori paneel): nimi, kaardinumber, e-post, boonus
    // ===================================================================
    public class KlientVaade
    {
        public int Id { get; set; }
        public string Nimi { get; set; } = "";
        public string Kaardinumber { get; set; } = "";
        public string? Email { get; set; }
        public decimal Boonus { get; set; }
        public int Oste { get; set; }
    }

    public class KliendidVorm : KujundusVorm
    {
        private readonly DataGridView vork;
        private readonly TextBox txtNimi, txtKaart, txtBoonus, txtEmail, txtOtsi;
        private readonly Label lblArv;
        private List<KlientVaade> koik = new();

        public KliendidVorm() : base("Kliendid (administraatori paneel)", 920, 640)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimumSize = new Size(780, 560);
            BackColor = Teema.Taust;

            // ---- Ülal: pealkiri + otsing ----
            var yla = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Teema.Taust };
            yla.Controls.Add(new Label { Text = "Kliendid", Location = new Point(18, 14), AutoSize = true, Font = UiStiil.PealkirjaFont, ForeColor = Teema.Tekst, BackColor = Teema.Taust });
            lblArv = new Label { Location = new Point(140, 24), AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Taust };
            yla.Controls.Add(lblArv);
            var otsing = UiStiil.LooSisend(out txtOtsi, 300, false, "Otsi nime, kaardi või e-posti järgi...");
            otsing.Location = new Point(400, 16);
            txtOtsi.TextChanged += (s, e) => Naita();
            yla.Controls.Add(otsing);
            yla.Resize += (s, e) => otsing.Left = Math.Max(240, yla.Width - otsing.Width - 18);   // paremasse serva

            // ---- Keskel: tabel kaardis ----
            vork = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            vork.CellClick += Vork_CellClick;
            var tabeliKaart = UiStiil.LooKaardiPaneel("Kõik kliendid", vork);
            var keskel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 12), BackColor = Teema.Taust };
            tabeliKaart.Dock = DockStyle.Fill;
            keskel.Controls.Add(tabeliKaart);

            // ---- All: andmete kaart ----
            var valjad = new TableLayoutPanel { Dock = DockStyle.Top, Height = 64, ColumnCount = 4, RowCount = 1, BackColor = Teema.Pind };
            valjad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            valjad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            valjad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            valjad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16));
            valjad.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            valjad.Controls.Add(ValjaPlokk("Nimi", out txtNimi, "Mari Maasikas"), 0, 0);
            valjad.Controls.Add(ValjaPlokk("Kliendikaart", out txtKaart, "100001"), 1, 0);
            valjad.Controls.Add(ValjaPlokk("E-post (PDF-tšekk saadetakse siia)", out txtEmail, "mari@email.ee"), 2, 0);
            valjad.Controls.Add(ValjaPlokk("Boonus (€)", out txtBoonus, "0.00"), 3, 0);

            var nupud = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, BackColor = Teema.Pind, Padding = new Padding(0, 6, 0, 0) };
            nupud.Controls.Add(UiStiil.LooNupp("Lisa", (s, e) => Lisa(), true));
            nupud.Controls.Add(UiStiil.LooNupp("Uuenda", (s, e) => Uuenda()));
            var kustuta = (RoundedButton)UiStiil.LooNupp("Kustuta", (s, e) => Kustuta());
            kustuta.Ohtlik = true;
            nupud.Controls.Add(kustuta);
            nupud.Controls.Add(UiStiil.LooNupp("Tühjenda väljad", (s, e) => Tuhjenda()));
            nupud.Controls.Add(UiStiil.LooNupp("Uus kaardinumber", (s, e) => txtKaart.Text = UusKaardinumber()));
            txtKaart.PlaceholderText = "tühi = antakse ise";

            var vihje = new Label
            {
                Text = Keel.F("Iga ostuga lisandub kliendile boonust {0} % ostusummast. Boonust saab kassas kasutada allahindluseks.", (Form1.BoonusProtsent * 100).ToString("0")),
                Dock = DockStyle.Top,
                Height = 24,
                ForeColor = Teema.TekstNork,
                BackColor = Teema.Pind
            };

            var sisu = new Panel { BackColor = Teema.Pind };
            sisu.Controls.Add(vihje);
            sisu.Controls.Add(nupud);
            sisu.Controls.Add(valjad);
            var andmeKaart = UiStiil.LooKaardiPaneel("Kliendi andmed", sisu);
            var all = new Panel { Dock = DockStyle.Bottom, Height = 206, Padding = new Padding(16, 0, 16, 16), BackColor = Teema.Taust };
            andmeKaart.Dock = DockStyle.Fill;
            all.Controls.Add(andmeKaart);

            Controls.Add(keskel);
            Controls.Add(all);
            Controls.Add(yla);
            keskel.BringToFront();   // Fill dokitakse viimasena

            Load += (s, e) => Laadi();
        }

        private void Laadi()
        {
            using var db = new ToodedContext();
            koik = db.Kliendid
                .OrderBy(k => k.Nimi)
                .Select(k => new KlientVaade { Id = k.Id, Nimi = k.Nimi, Kaardinumber = k.Kaardinumber, Email = k.Email, Boonus = k.Boonus, Oste = k.Ostud.Count })
                .ToList();
            Naita();
        }

        private void Naita()
        {
            string otsi = txtOtsi.Text.Trim();
            var nimekiri = koik.Where(k => otsi.Length == 0
                    || k.Nimi.Contains(otsi, StringComparison.CurrentCultureIgnoreCase)
                    || k.Kaardinumber.Contains(otsi, StringComparison.CurrentCultureIgnoreCase)
                    || (k.Email ?? "").Contains(otsi, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            vork.DataSource = nimekiri;
            if (vork.Columns["Id"] is DataGridViewColumn id) id.Visible = false;
            if (vork.Columns["Boonus"] is DataGridViewColumn b) b.DefaultCellStyle.Format = "0.00 €";
            if (vork.Columns["Email"] is DataGridViewColumn em) { em.HeaderText = "E-post"; em.FillWeight = 160; }
            if (vork.Columns["Oste"] is DataGridViewColumn o) o.FillWeight = 50;
            vork.ClearSelection();
            lblArv.Text = $"{nimekiri.Count} / {koik.Count}";
        }

        private void Vork_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || vork.Rows[e.RowIndex].DataBoundItem is not KlientVaade k) return;
            txtNimi.Text = k.Nimi;
            txtKaart.Text = k.Kaardinumber;
            txtBoonus.Text = k.Boonus.ToString("0.00", CultureInfo.InvariantCulture);
            txtEmail.Text = k.Email ?? "";
        }

        private int? ValitudId()
        {
            if (vork.CurrentRow?.DataBoundItem is KlientVaade k) return k.Id;
            Teade.Show("Vali tabelist klient!");
            return null;
        }

        // Kontrollib väljad ja täidab kliendi andmed
        private bool LoeVorm(Klient k)
        {
            string nimi = txtNimi.Text.Trim();
            string kaart = txtKaart.Text.Trim();
            if (nimi.Length == 0) { Teade.Show("Sisesta kliendi nimi!"); txtNimi.Focus(); return false; }
            if (kaart.Length == 0) kaart = UusKaardinumber();   // uus klient saab kaardi kohe

            string b = txtBoonus.Text.Trim().Replace(',', '.');
            decimal boonus = 0;
            if (b.Length > 0 && (!decimal.TryParse(b, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out boonus) || boonus < 0))
            {
                Teade.Show("Boonus peab olema arv (0 või rohkem)!");
                txtBoonus.Focus();
                return false;
            }

            string email = txtEmail.Text.Trim();
            if (email.Length > 0 && !EmailSeaded.OnKorrektneAadress(email))
            {
                Teade.Show("E-posti aadress pole korrektne!");
                txtEmail.Focus();
                return false;
            }

            k.Nimi = nimi;
            k.Kaardinumber = kaart;
            k.Email = email.Length > 0 ? email : null;
            k.Boonus = Math.Round(boonus, 2);
            return true;
        }

        private void Lisa()
        {
            var uus = new Klient();
            if (!LoeVorm(uus)) return;
            try
            {
                using var db = new ToodedContext();
                if (db.Kliendid.Any(k => k.Kaardinumber == uus.Kaardinumber))
                {
                    Teade.Show("Selle kaardinumbriga klient on juba olemas.");
                    return;
                }
                db.Kliendid.Add(uus);
                db.SaveChanges();
                Teade.Toast(Keel.F("Klient „{0}“ lisatud", uus.Nimi) + "\n" + Keel.F("Kliendikaart: {0}", uus.Kaardinumber));
                Tuhjenda();
                Laadi();
            }
            catch (Exception ex) { Teade.Show("Viga lisamisel:\n" + ex.Message, "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void Uuenda()
        {
            if (ValitudId() is not int id) return;
            try
            {
                using var db = new ToodedContext();
                var klient = db.Kliendid.Find(id);
                if (klient == null || !LoeVorm(klient)) return;
                if (db.Kliendid.Any(k => k.Kaardinumber == klient.Kaardinumber && k.Id != id))
                {
                    Teade.Show("Selle kaardinumbriga on juba teine klient.");
                    return;
                }
                db.SaveChanges();
                Teade.Toast("Kliendi andmed uuendatud");
                Laadi();
            }
            catch (Exception ex) { Teade.Show("Viga uuendamisel:\n" + ex.Message, "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void Kustuta()
        {
            if (ValitudId() is not int id) return;
            if (Teade.Show("Kas kustutan valitud kliendi? (tema ostud jäävad alles)", "Kinnitus",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                using var db = new ToodedContext();
                var klient = db.Kliendid.Include(k => k.Ostud).FirstOrDefault(k => k.Id == id);
                if (klient != null)
                {
                    db.Kliendid.Remove(klient);   // ostude KlientId muutub NULL-iks
                    db.SaveChanges();
                }
                Teade.Toast("Klient kustutatud", TeateTyyp.Info);
                Tuhjenda();
                Laadi();
            }
            catch (Exception ex) { Teade.Show("Viga kustutamisel:\n" + ex.Message, "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void Tuhjenda()
        {
            txtNimi.Clear();
            txtKaart.Clear();
            txtBoonus.Clear();
            txtEmail.Clear();
            vork.ClearSelection();
        }

        // Järgmine vaba 6-kohaline kaardinumber
        private static string UusKaardinumber()
        {
            using var db = new ToodedContext();
            return Kliendikaart.UusNumber(db);
        }
    }

    // ===================================================================
    //  Müügistatistika: kaardid, graafik, väike laoseis, enim müüdud
    // ===================================================================
    public class StatistikaVorm : KujundusVorm
    {
        private readonly DataGridView ladu, top;
        private readonly MyygiGraafik graafik = new();
        private readonly Label lblTana = new(), lblKokku = new(), lblOste = new(), lblKliente = new();

        public StatistikaVorm() : base("Statistika", 1000, 720)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimumSize = new Size(840, 620);
            BackColor = Teema.Taust;

            // ---- Ülal: pealkiri + nupud ----
            var yla = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Teema.Taust };
            yla.Controls.Add(new Label { Text = "Müügistatistika", Location = new Point(18, 14), AutoSize = true, Font = UiStiil.PealkirjaFont, ForeColor = Teema.Tekst, BackColor = Teema.Taust });
            var nupud = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = Teema.Taust,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            var sulge = UiStiil.LooNupp("Sulge", null);
            sulge.DialogResult = DialogResult.Cancel;
            CancelButton = sulge;
            nupud.Controls.Add(sulge);
            nupud.Controls.Add(UiStiil.LooNupp("Ekspordi müük (CSV)", (s, e) => CsvEksport.Myyk(this)));
            nupud.Controls.Add(UiStiil.LooNupp("Värskenda", (s, e) => Laadi()));
            yla.Controls.Add(nupud);
            yla.Resize += (s, e) => nupud.Location = new Point(yla.Width - nupud.Width - 14, 16);

            // ---- Numbrikaardid ----
            var kaardid = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(12, 0, 12, 0), BackColor = Teema.Taust, WrapContents = false };
            kaardid.Controls.Add(NumbriKaart("Müük täna", lblTana));
            kaardid.Controls.Add(NumbriKaart("Müük kokku", lblKokku));
            kaardid.Controls.Add(NumbriKaart("Oste kokku", lblOste));
            kaardid.Controls.Add(NumbriKaart("Kliente", lblKliente));

            // ---- Graafik ja tabelid ----
            ladu = Tabel();
            top = Tabel();
            var tabel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(12, 4, 12, 12), BackColor = Teema.Taust };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
            tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 52));

            var graafikuKaart = UiStiil.LooKaardiPaneel("Müük viimase 14 päeva jooksul (€)", graafik);
            graafikuKaart.Dock = DockStyle.Fill;
            graafikuKaart.Margin = new Padding(4);
            tabel.Controls.Add(graafikuKaart, 0, 0);
            tabel.SetColumnSpan(graafikuKaart, 2);

            var laoKaart = UiStiil.LooKaardiPaneel(Keel.F("Väike laoseis (alla {0} tk) – telli juurde", Form1.VaikeLaoseis), ladu);
            laoKaart.Dock = DockStyle.Fill;
            laoKaart.Margin = new Padding(4);
            tabel.Controls.Add(laoKaart, 0, 1);

            var topKaart = UiStiil.LooKaardiPaneel("Enim müüdud tooted", top);
            topKaart.Dock = DockStyle.Fill;
            topKaart.Margin = new Padding(4);
            tabel.Controls.Add(topKaart, 1, 1);

            Controls.Add(tabel);
            Controls.Add(kaardid);
            Controls.Add(yla);
            tabel.BringToFront();

            // Tabelite veerud tekivad alles siis, kui vorm on laetud
            Load += (s, e) => Laadi();
        }

        private static DataGridView Tabel() => new()
        {
            AllowUserToAddRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        private static RoundedPanel NumbriKaart(string pealkiri, Label vaartus)
        {
            var kaart = new RoundedPanel { Size = new Size(226, 86), Margin = new Padding(4), Padding = new Padding(16, 12, 12, 8) };
            vaartus.Dock = DockStyle.Top;
            vaartus.Height = 40;
            vaartus.Font = new Font("Segoe UI Semibold", 18F);
            vaartus.ForeColor = Teema.Tekst;
            vaartus.BackColor = Teema.Pind;
            vaartus.Text = "–";
            var silt = new Label { Text = pealkiri.ToUpper(), Dock = DockStyle.Top, Height = 20, Font = UiStiil.AlapealkiriFont, ForeColor = Teema.TekstNork, BackColor = Teema.Pind };
            kaart.Controls.Add(vaartus);
            kaart.Controls.Add(silt);
            return kaart;
        }

        private void Laadi()
        {
            using var db = new ToodedContext();
            DateTime tana = DateTime.Today;

            // SQLite ei oska decimal-veerge SQL-is summeerida, seega arvutame mälus
            var ostud = db.Ostud.Where(o => !o.Tagastatud)
                .Select(o => new { o.Aeg, o.Summa })
                .ToList();

            lblTana.Text = Ostukorv.Eur(ostud.Where(o => o.Aeg >= tana).Sum(o => o.Summa));
            lblKokku.Text = Ostukorv.Eur(ostud.Sum(o => o.Summa));
            lblOste.Text = ostud.Count.ToString();
            lblKliente.Text = db.Kliendid.Count().ToString();

            // Graafik: viimased 14 päeva
            var paevad = Enumerable.Range(0, 14).Select(i => tana.AddDays(i - 13)).ToList();
            graafik.Sildid = paevad.Select(p => p.ToString("dd.MM")).ToArray();
            graafik.Vaartused = paevad.Select(p => ostud.Where(o => o.Aeg.Date == p).Sum(o => o.Summa)).ToArray();
            graafik.Invalidate();

            ladu.DataSource = db.Toodetabel
                .Where(t => t.Kogus < Form1.VaikeLaoseis)
                .OrderBy(t => t.Kogus)
                .Select(t => new { Toode = t.Toodenimetus, t.Kogus })
                .ToList();

            var read = db.OstuRead.Where(r => !r.Ost.Tagastatud)
                .Select(r => new { r.Toodenimetus, r.Kogus, r.Hind })
                .ToList();
            top.DataSource = read
                .GroupBy(r => r.Toodenimetus)
                .Select(g => new { Toode = g.Key, Müüdud = g.Sum(r => r.Kogus), Summa = g.Sum(r => r.Hind * r.Kogus) })
                .OrderByDescending(x => x.Müüdud)
                .Take(10)
                .ToList();
            if (top.Columns["Summa"] is DataGridViewColumn summa)
                summa.DefaultCellStyle.Format = "0.00 €";
        }
    }

    // Tulpdiagramm (joonistatakse ise, lisapakette pole vaja)
    public class MyygiGraafik : Control
    {
        public string[] Sildid { get; set; } = Array.Empty<string>();
        public decimal[] Vaartused { get; set; } = Array.Empty<decimal>();

        public MyygiGraafik()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Teema.Pind);

            if (Vaartused.Length == 0) return;

            decimal max = Math.Max(1m, Vaartused.Max());
            int all = 24, ulal = 20, vasak = 4;
            float laius = (Width - vasak * 2) / (float)Vaartused.Length;
            float korgus = Height - all - ulal;

            // Abijooned
            using (var abi = new Pen(Teema.Joon) { DashStyle = DashStyle.Dot })
                for (int i = 0; i <= 3; i++)
                {
                    float y = ulal + korgus * i / 3f;
                    g.DrawLine(abi, vasak, y, Width - vasak, y);
                }

            using var tulp = new SolidBrush(Color.FromArgb(Teema.Tume ? 150 : 120, Teema.Aktsent));
            using var tanaTulp = new SolidBrush(Teema.Aktsent);
            using var vaike = new Font("Segoe UI", 8F);
            var fmt = new StringFormat { Alignment = StringAlignment.Center };

            for (int i = 0; i < Vaartused.Length; i++)
            {
                float x = vasak + i * laius;
                float h = (float)(Vaartused[i] / max) * korgus;
                var r = new RectangleF(x + laius * 0.18f, ulal + korgus - h, laius * 0.64f, Math.Max(h, Vaartused[i] > 0 ? 3 : 0));
                if (r.Height > 0)
                    using (var path = Ymardus.Tee(r, Math.Min(6, r.Width / 2)))
                        g.FillPath(i == Vaartused.Length - 1 ? tanaTulp : tulp, path);

                using var tekst = new SolidBrush(i == Vaartused.Length - 1 ? Teema.Tekst : Teema.TekstNork);
                if (Vaartused[i] > 0)
                    g.DrawString(Vaartused[i].ToString("0.##"), vaike, tekst, x + laius / 2, r.Top - 15, fmt);
                if (i < Sildid.Length)
                    g.DrawString(i == Vaartused.Length - 1 ? Keel.T("täna") : Sildid[i], vaike, tekst, x + laius / 2, Height - all + 5, fmt);
            }
        }
    }
}
