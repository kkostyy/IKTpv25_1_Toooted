using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    // ===================================================================
    //  Seaded: kõik kasutaja seaded ühes aknas
    //  Profiil (nimi, e-post, kliendikaart) · Turvalisus · Välimus (teema, värv, disain, keel)
    // ===================================================================
    public partial class SeadedVorm : KujundusVorm
    {
        private readonly TextBox txtTaisNimi, txtEmail, txtAadress;
        private readonly CheckBox chkTsekk, chkUmarad;
        private readonly Label lblKaart, lblViga;
        private readonly RoundedButton btnLooKaart, btnNaitaKaart;
        private readonly ComboBox cmbTeema, cmbVarv, cmbDisain, cmbKeel;
        private bool taidan;   // valikute täitmine koodist ei rakenda midagi

        // Disaini vahetus tehakse alles pärast akna sulgemist (pood avaneb uues disainis)
        public Disain? UusDisain { get; private set; }

        // Avab seaded ja rakendab pärast sulgemist disaini vahetuse
        public static void Ava(IWin32Window? omanik)
        {
            var vorm = new SeadedVorm();
            Aken.Ava(omanik, vorm, () =>
            {
                if (vorm.UusDisain is Disain d && d != Teema.Stiil) Teema.SeaDisain(d);
            });
        }

        public SeadedVorm() : base("Seaded", 580, 752)
        {
            KysiSulgemisel = true;
            Pealkiri(Keel.F("Seaded – {0}", AktiivneKasutaja.Nimi));
            AktiivneKasutaja.Uuenda();
            const int x = 20, w = 520;

            // Väikesel ekraanil keritav
            int maks = (Screen.PrimaryScreen?.WorkingArea.Height ?? 900) - 80;
            if (maks < 752) { ClientSize = new Size(600, maks); AutoScroll = true; }

            // ---------------- Profiil ----------------
            Plokk("PROFIIL", 58);
            Silt("Kasutajanimi", x, 86);
            var nimi = Silt(AktiivneKasutaja.Nimi + "   ·   " + Keel.T(AktiivneKasutaja.Roll), 190, 86);
            nimi.ForeColor = Teema.Tekst;
            nimi.Font = new Font("Segoe UI Semibold", 9.5F);

            Silt("Ees- ja perekonnanimi", x, 116);
            txtTaisNimi = Sisend(x, 136, w);
            txtTaisNimi.Text = AktiivneKasutaja.TaisNimi ?? "";

            Silt("E-post (sinna tuleb PDF-tšekk)", x, 174);
            txtEmail = Sisend(x, 194, w);
            txtEmail.PlaceholderText = "nimi@gmail.com";
            txtEmail.Text = AktiivneKasutaja.Email ?? "";

            chkTsekk = new CheckBox { Text = "Soovin ostutšekke e-postile", Location = new Point(x + 2, 230), AutoSize = true, ForeColor = Teema.Tekst2, Checked = AktiivneKasutaja.TsekkEmailile };
            Controls.Add(chkTsekk);

            Silt("Kojuveo aadress", x, 262);
            txtAadress = Sisend(x, 282, w);
            txtAadress.PlaceholderText = "Tänav 1-2, Tallinn, 10111";
            txtAadress.Text = AktiivneKasutaja.Aadress ?? "";

            Silt("Kliendikaart", x, 326);
            lblKaart = Silt("", 130, 326);
            lblKaart.Font = new Font("Segoe UI Semibold", 9.5F);
            btnLooKaart = UiStiil.LooSuurNupp("Loo kliendikaart", (s, e) => LooKaart(), peamine: false);
            btnLooKaart.Bounds = new Rectangle(380, 318, 160, 34);
            btnNaitaKaart = UiStiil.LooSuurNupp("Ava kaart  ›", (s, e) => KliendikaartVorm.Ava(this), peamine: true);
            btnNaitaKaart.Bounds = new Rectangle(380, 318, 160, 34);
            Controls.Add(btnLooKaart);
            Controls.Add(btnNaitaKaart);
            NaitaKaart();

            // ---------------- Turvalisus ----------------
            Plokk("TURVALISUS", 368);
            var parool = Nupp("Muuda parooli...", x, 394);
            parool.Click += (s, e) => Aken.Ava(this, new ParooliMuutmineVorm());
            if (AktiivneKasutaja.OnOmanik)
            {
                var email = Nupp("E-posti seaded (saatja)...", x + 170, 394);
                email.Click += (s, e) => Aken.Ava(this, new EmailSeadedVorm());
            }

            // ---------------- Välimus ----------------
            Plokk("VÄLIMUS", 446);
            Silt("Teema", x, 476);
            cmbTeema = Valik(x, 496, 250);
            Silt("Poe värv", 290, 476);
            cmbVarv = Valik(290, 496, 250);
            Silt("Disain (Ctrl+Shift+D)", x, 536);
            cmbDisain = Valik(x, 556, 250);
            Silt("Keel", 290, 536);
            cmbKeel = Valik(290, 556, 250);
            chkUmarad = new CheckBox { Text = "Ümarad nurgad ja külgmenüü", Location = new Point(x + 2, 596), AutoSize = true, ForeColor = Teema.Tekst2, Checked = Teema.Umarad };
            chkUmarad.CheckedChanged += (s, e) => { if (!taidan) Teema.SeaUmarad(chkUmarad.Checked); };
            Controls.Add(chkUmarad);
            TaidaValikud();

            cmbTeema.SelectedIndexChanged += (s, e) => { if (!taidan) Teema.Sea(cmbTeema.SelectedIndex == 1, Teema.AktsendiNimi); };
            cmbVarv.SelectedIndexChanged += (s, e) =>
            {
                if (!taidan && cmbVarv.SelectedIndex >= 0)
                    Teema.Sea(Teema.Tume, Teema.Aktsendid.Keys.ElementAt(cmbVarv.SelectedIndex));
            };
            cmbDisain.SelectedIndexChanged += (s, e) =>
            {
                if (!taidan) UusDisain = cmbDisain.SelectedIndex switch { 1 => Disain.Klassikaline, 2 => Disain.Moodne, _ => Disain.Rimi };
            };
            cmbKeel.SelectedIndexChanged += (s, e) =>
            {
                if (!taidan && cmbKeel.SelectedIndex >= 0) Keel.Sea(Enum.GetValues<KeeleKood>()[cmbKeel.SelectedIndex]);
            };

            var info = Silt("Teema, värv ja keel muutuvad kohe. Disain vahetub pärast akna sulgemist.", x, 626);
            info.ForeColor = Teema.TekstNork;

            lblViga = Veateade(x, 650, w);

            var salvesta = Nupp("Salvesta", x, 696, true);
            salvesta.Click += (s, e) => Salvesta();
            AcceptButton = salvesta;
            var sulge = Nupp("Sulge", x + 120, 696);
            sulge.DialogResult = DialogResult.Cancel;
            CancelButton = sulge;

            if (Teema.OnMoodne) RakendaMoodne();   // lõuend 3.3: jaotised vasakul, segmendid, värvitäpid

            // Keele vahetusel tõlgitakse ka valikute tekstid
            Teema.Muutus += ValikudHiljem;
            FormClosed += (s, e) => Teema.Muutus -= ValikudHiljem;
        }

        private void Plokk(string pealkiri, int y)
        {
            Controls.Add(new Label { Text = pealkiri, Location = new Point(20, y), AutoSize = true, Font = UiStiil.AlapealkiriFont, ForeColor = Teema.Teine });
            var joon = new Panel { Location = new Point(20, y + 18), Size = new Size(520, 1), BackColor = Teema.Joon };
            Controls.Add(joon);
        }

        private ComboBox Valik(int x, int y, int w)
        {
            var c = new ComboBox { Location = new Point(x, y), Width = w, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            Controls.Add(c);
            return c;
        }

        // Mitte valiku enda sündmuse sees (combobox ei taha oma ridu muuta sel hetkel)
        private void ValikudHiljem()
        {
            if (IsHandleCreated && !IsDisposed) BeginInvoke(new Action(TaidaValikud));
        }

        private void TaidaValikud()
        {
            if (cmbTeema == null) return;
            taidan = true;
            try
            {
                Taida(cmbTeema, new[] { Keel.T("Hele teema"), Keel.T("Tume teema") }, Teema.Tume ? 1 : 0);
                Taida(cmbVarv, Teema.Aktsendid.Keys.Select(Keel.T).ToArray(), Teema.Aktsendid.Keys.ToList().IndexOf(Teema.AktsendiNimi));
                var disain = UusDisain ?? Teema.Stiil;
                Taida(cmbDisain, new[] { Keel.T("E-poe disain (Rimi)"), Keel.T("Klassikaline disain"), Keel.T("Moodne disain") },
                    disain switch { Disain.Klassikaline => 1, Disain.Moodne => 2, _ => 0 });
                var keeled = Enum.GetValues<KeeleKood>();
                Taida(cmbKeel, keeled.Select(Keel.Nimi).ToArray(), Array.IndexOf(keeled, Keel.Praegu));
            }
            finally { taidan = false; }
        }

        private static void Taida(ComboBox c, string[] tekstid, int valitud)
        {
            c.BeginUpdate();
            c.Items.Clear();
            c.Items.AddRange(tekstid);
            c.SelectedIndex = Math.Clamp(valitud, -1, tekstid.Length - 1);
            c.EndUpdate();
        }

        private void NaitaKaart()
        {
            bool on = AktiivneKasutaja.Kaardinumber != null;
            lblKaart.Text = on
                ? Keel.F("{0}   ·   boonus {1}", AktiivneKasutaja.Kaardinumber, Ostukorv.Eur(AktiivneKasutaja.Boonus))
                : Keel.T("puudub");
            lblKaart.ForeColor = on ? Teema.Edu : Teema.TekstNork;
            btnLooKaart.Visible = !on;
            btnNaitaKaart.Visible = on;
        }

        private void LooKaart()
        {
            try
            {
                using var db = new ToodedContext();
                var k = db.Kasutajad.Find(AktiivneKasutaja.Id);
                if (k == null) return;
                string nimi = txtTaisNimi.Text.Trim().Length > 0 ? txtTaisNimi.Text.Trim() : k.Kasutajanimi;
                k.Klient = Kliendikaart.Loo(db, nimi, txtEmail.Text.Trim().Length > 0 ? txtEmail.Text.Trim() : k.Email);
                db.SaveChanges();
                AktiivneKasutaja.Uuenda();
                NaitaKaart();
                Teade.Toast(Keel.F("Kliendikaart: {0}", AktiivneKasutaja.Kaardinumber));
            }
            catch (Exception ex) { lblViga.Text = Keel.T("Viga:") + " " + ex.Message; }
        }

        // Nimi ja e-post; e-post muutub ka kliendikaardil (sinna saadetakse tšekid)
        private void Salvesta()
        {
            string email = txtEmail.Text.Trim();
            string taisNimi = txtTaisNimi.Text.Trim();
            if (email.Length > 0 && !EmailSeaded.OnKorrektneAadress(email)) { lblViga.Text = Keel.T("E-posti aadress pole korrektne!"); txtEmail.Focus(); return; }
            if (AktiivneKasutaja.OnOstja && email.Length == 0) { lblViga.Text = Keel.T("Sisesta e-post – sinna saadetakse tšekid."); txtEmail.Focus(); return; }

            try
            {
                using var db = new ToodedContext();
                var k = db.Kasutajad.Include(x => x.Klient).FirstOrDefault(x => x.Id == AktiivneKasutaja.Id);
                if (k == null) { lblViga.Text = Keel.T("Kasutajat ei leitud."); return; }
                k.Nimi = taisNimi.Length > 0 ? taisNimi : null;
                k.Email = email.Length > 0 ? email : null;
                k.Aadress = txtAadress.Text.Trim().Length > 0 ? txtAadress.Text.Trim() : null;
                k.TsekkEmailile = chkTsekk.Checked;
                if (k.Klient != null)
                {
                    if (email.Length > 0) k.Klient.Email = email;
                    if (taisNimi.Length > 0) k.Klient.Nimi = taisNimi;
                }
                db.SaveChanges();
                AktiivneKasutaja.Sea(k);
                Teade.Toast("Seaded salvestatud");
                Muudetud = false;
                Teema.TeavitaMuutusest();   // tervitused (nimi) uuendatakse
                lblViga.Text = "";
            }
            catch (Exception ex) { lblViga.Text = Keel.T("Viga:") + " " + ex.Message; }
        }
    }

    // ===================================================================
    //  Kasutajad (omanik): eraldi tabel kõigist kontodest
    // ===================================================================
    public class KasutajaVaade
    {
        public int Id { get; set; }
        public string Kasutajanimi { get; set; } = "";
        public string? Nimi { get; set; }
        public string Roll { get; set; } = "";
        public string? Email { get; set; }
        public string? Kliendikaart { get; set; }
        public decimal? Boonus { get; set; }
    }

    public class KasutajadVorm : KujundusVorm
    {
        private readonly DataGridView vork;
        private readonly TextBox txtNimi, txtEmail, txtOtsi;
        private readonly ComboBox cmbRoll;
        private readonly Label lblArv, lblValitud;
        private readonly string[] rollid = { Kasutaja.Ostja, Kasutaja.Myyja, Kasutaja.Omanik };
        private List<KasutajaVaade> koik = new();

        public KasutajadVorm() : base("Kasutajad", 980, 640)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimumSize = new Size(820, 560);
            BackColor = Teema.Taust;

            var yla = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Teema.Taust };
            yla.Controls.Add(new Label { Text = "Kasutajad", Location = new Point(18, 14), AutoSize = true, Font = UiStiil.PealkirjaFont, ForeColor = Teema.Tekst, BackColor = Teema.Taust });
            lblArv = new Label { Location = new Point(160, 24), AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Taust };
            yla.Controls.Add(lblArv);
            var otsing = UiStiil.LooSisend(out txtOtsi, 300, false, "Otsi nime, rolli või e-posti järgi...");
            otsing.Location = new Point(400, 16);
            txtOtsi.TextChanged += (s, e) => Naita();
            yla.Controls.Add(otsing);
            yla.Resize += (s, e) => otsing.Left = Math.Max(260, yla.Width - otsing.Width - 18);

            vork = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            vork.CellClick += (s, e) => ValiRida(e.RowIndex);
            vork.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex >= 0 && vork.Columns[e.ColumnIndex].Name == "Roll" && e.Value is string r)
                {
                    e.Value = Keel.T(r);
                    e.FormattingApplied = true;
                }
            };
            var tabeliKaart = UiStiil.LooKaardiPaneel("Kõik kasutajad", vork);
            var keskel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 12), BackColor = Teema.Taust };
            tabeliKaart.Dock = DockStyle.Fill;
            keskel.Controls.Add(tabeliKaart);

            // ---- All: valitud kasutaja ----
            var valjad = new TableLayoutPanel { Dock = DockStyle.Top, Height = 64, ColumnCount = 3, RowCount = 1, BackColor = Teema.Pind };
            valjad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            valjad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            valjad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
            valjad.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            valjad.Controls.Add(ValjaPlokk("Ees- ja perekonnanimi", out txtNimi), 0, 0);
            valjad.Controls.Add(ValjaPlokk("E-post", out txtEmail, "nimi@gmail.com"), 1, 0);
            var rolliPlokk = new Panel { Dock = DockStyle.Fill, BackColor = Teema.Pind };
            cmbRoll = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            foreach (var r in rollid) cmbRoll.Items.Add(Keel.T(r));
            rolliPlokk.Controls.Add(cmbRoll);
            rolliPlokk.Controls.Add(new Label { Text = "Roll", Dock = DockStyle.Top, Height = 22, ForeColor = Teema.Tekst2, BackColor = Teema.Pind });
            valjad.Controls.Add(rolliPlokk, 2, 0);

            var nupud = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, BackColor = Teema.Pind, Padding = new Padding(0, 6, 0, 0) };
            nupud.Controls.Add(UiStiil.LooNupp("Uus kasutaja...", (s, e) => Uus(), true));
            nupud.Controls.Add(UiStiil.LooNupp("Salvesta", (s, e) => Salvesta()));
            nupud.Controls.Add(UiStiil.LooNupp("Uus parool...", (s, e) => UusParool()));
            nupud.Controls.Add(UiStiil.LooNupp("Loo kliendikaart", (s, e) => LooKaart()));
            var kustuta = (RoundedButton)UiStiil.LooNupp("Kustuta", (s, e) => Kustuta());
            kustuta.Ohtlik = true;
            nupud.Controls.Add(kustuta);

            lblValitud = new Label { Dock = DockStyle.Top, Height = 24, ForeColor = Teema.TekstNork, BackColor = Teema.Pind };

            var sisu = new Panel { BackColor = Teema.Pind };
            sisu.Controls.Add(lblValitud);
            sisu.Controls.Add(nupud);
            sisu.Controls.Add(valjad);
            var andmeKaart = UiStiil.LooKaardiPaneel("Valitud kasutaja", sisu);
            var all = new Panel { Dock = DockStyle.Bottom, Height = 206, Padding = new Padding(16, 0, 16, 16), BackColor = Teema.Taust };
            andmeKaart.Dock = DockStyle.Fill;
            all.Controls.Add(andmeKaart);

            Controls.Add(keskel);
            Controls.Add(all);
            Controls.Add(yla);
            keskel.BringToFront();

            Load += (s, e) => Laadi();
        }

        private void Laadi()
        {
            using var db = new ToodedContext();
            koik = db.Kasutajad.AsNoTracking()
                .OrderBy(k => k.Roll).ThenBy(k => k.Kasutajanimi)
                .Select(k => new KasutajaVaade
                {
                    Id = k.Id,
                    Kasutajanimi = k.Kasutajanimi,
                    Nimi = k.Nimi,
                    Roll = k.Roll,
                    Email = k.Email,
                    Kliendikaart = k.Klient != null ? k.Klient.Kaardinumber : null,
                    Boonus = k.Klient != null ? (decimal?)k.Klient.Boonus : null
                })
                .ToList();
            Naita();
        }

        private void Naita()
        {
            string otsi = txtOtsi.Text.Trim();
            var nimekiri = koik.Where(k => otsi.Length == 0
                    || k.Kasutajanimi.Contains(otsi, StringComparison.CurrentCultureIgnoreCase)
                    || (k.Nimi ?? "").Contains(otsi, StringComparison.CurrentCultureIgnoreCase)
                    || Keel.T(k.Roll).Contains(otsi, StringComparison.CurrentCultureIgnoreCase)
                    || (k.Email ?? "").Contains(otsi, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            vork.DataSource = nimekiri;
            if (vork.Columns["Id"] is DataGridViewColumn id) id.Visible = false;
            if (vork.Columns["Boonus"] is DataGridViewColumn b) b.DefaultCellStyle.Format = "0.00 €";
            if (vork.Columns["Email"] is DataGridViewColumn em) { em.HeaderText = "E-post"; em.FillWeight = 150; }
            if (vork.Columns["Kliendikaart"] is DataGridViewColumn kk) kk.FillWeight = 70;
            vork.ClearSelection();
            lblArv.Text = $"{nimekiri.Count} / {koik.Count}";
            lblValitud.Text = Keel.T("Vali tabelist kasutaja.");
        }

        private KasutajaVaade? Valitud => vork.CurrentRow?.DataBoundItem as KasutajaVaade;

        private void ValiRida(int rida)
        {
            if (rida < 0 || vork.Rows[rida].DataBoundItem is not KasutajaVaade k) return;
            txtNimi.Text = k.Nimi ?? "";
            txtEmail.Text = k.Email ?? "";
            cmbRoll.SelectedIndex = Array.IndexOf(rollid, k.Roll);
            lblValitud.Text = Keel.F("Valitud: {0}", k.Kasutajanimi) +
                (k.Kliendikaart != null ? "   ·   " + Keel.F("Kliendikaart: {0}", k.Kliendikaart) : "");
        }

        private KasutajaVaade? KysiValitud()
        {
            if (Valitud is KasutajaVaade k) return k;
            Teade.Show("Vali tabelist kasutaja.");
            return null;
        }

        private void Uus()
        {
            var vorm = new RegistreerimineVorm(lubaOmanik: true);
            Aken.Ava(this, vorm, () => { if (vorm.DialogResult == DialogResult.OK) Laadi(); });
        }

        private void Salvesta()
        {
            if (KysiValitud() is not KasutajaVaade v) return;
            string email = txtEmail.Text.Trim();
            if (email.Length > 0 && !EmailSeaded.OnKorrektneAadress(email)) { Teade.Show("E-posti aadress pole korrektne!"); return; }
            string roll = cmbRoll.SelectedIndex >= 0 ? rollid[cmbRoll.SelectedIndex] : v.Roll;

            try
            {
                using var db = new ToodedContext();
                var k = db.Kasutajad.Include(x => x.Klient).FirstOrDefault(x => x.Id == v.Id);
                if (k == null) return;

                // Viimast omanikku ei saa ära võtta (muidu keegi ei saa programmi hallata)
                if (k.Roll == Kasutaja.Omanik && roll != Kasutaja.Omanik &&
                    db.Kasutajad.Count(x => x.Roll == Kasutaja.Omanik) <= 1)
                {
                    Teade.Show("Vähemalt üks kasutaja peab olema omanik.", "Kasutajad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                k.Nimi = txtNimi.Text.Trim().Length > 0 ? txtNimi.Text.Trim() : null;
                k.Email = email.Length > 0 ? email : null;
                k.Roll = roll;
                if (roll == Kasutaja.Ostja && k.Klient == null)
                    k.Klient = Kliendikaart.Loo(db, k.Nimi ?? k.Kasutajanimi, k.Email);   // ostjal on alati kaart
                if (k.Klient != null && k.Email != null) k.Klient.Email = k.Email;
                db.SaveChanges();

                if (k.Id == AktiivneKasutaja.Id) AktiivneKasutaja.Uuenda();
                Teade.Toast(Keel.F("Kasutaja „{0}“ salvestatud", k.Kasutajanimi));
                Laadi();
            }
            catch (Exception ex) { Teade.Show("Viga:\n" + ex.Message, "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void UusParool()
        {
            if (KysiValitud() is not KasutajaVaade v) return;
            string? parool = Teade.Kysi("Uus parool", Keel.F("Uus parool kasutajale „{0}“ (vähemalt 4 märki):", v.Kasutajanimi));
            if (parool == null) return;
            if (parool.Length < 4) { Teade.Show("Parool peab olema vähemalt 4 märki."); return; }
            try
            {
                using var db = new ToodedContext();
                var k = db.Kasutajad.Find(v.Id);
                if (k == null) return;
                k.ParoolHash = PasswordHelper.Hash(parool);
                db.SaveChanges();
                Teade.Toast("Parool muudetud");
            }
            catch (Exception ex) { Teade.Show("Viga:\n" + ex.Message, "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void LooKaart()
        {
            if (KysiValitud() is not KasutajaVaade v) return;
            if (v.Kliendikaart != null) { Teade.Show(Keel.F("Kliendikaart: {0}", v.Kliendikaart)); return; }
            try
            {
                using var db = new ToodedContext();
                var k = db.Kasutajad.Find(v.Id);
                if (k == null) return;
                k.Klient = Kliendikaart.Loo(db, k.Nimi ?? k.Kasutajanimi, k.Email);
                db.SaveChanges();
                if (k.Id == AktiivneKasutaja.Id) AktiivneKasutaja.Uuenda();
                Teade.Toast(Keel.F("Kliendikaart: {0}", k.Klient.Kaardinumber));
                Laadi();
            }
            catch (Exception ex) { Teade.Show("Viga:\n" + ex.Message, "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void Kustuta()
        {
            if (KysiValitud() is not KasutajaVaade v) return;
            if (v.Id == AktiivneKasutaja.Id) { Teade.Show("Iseennast ei saa kustutada."); return; }
            if (Teade.Show(Keel.F("Kas kustutan kasutaja „{0}“? (kliendikaart ja ostud jäävad alles)", v.Kasutajanimi), "Kinnitus",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                using var db = new ToodedContext();
                var k = db.Kasutajad.Find(v.Id);
                if (k == null) return;
                if (k.Roll == Kasutaja.Omanik && db.Kasutajad.Count(x => x.Roll == Kasutaja.Omanik) <= 1)
                {
                    Teade.Show("Vähemalt üks kasutaja peab olema omanik.", "Kasutajad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                db.Kasutajad.Remove(k);   // tema ostunimekiri kustub ka
                db.SaveChanges();
                Teade.Toast("Kasutaja kustutatud", TeateTyyp.Info);
                Laadi();
            }
            catch (Exception ex) { Teade.Show("Viga:\n" + ex.Message, "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
