using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    public partial class Form1 : Form, IVormiHost
    {
        // Uus valitud pildifail (täistee); null = pilti ei muudeta
        private string? uusPiltTee = null;

        public Form1()
        {
            InitializeComponent();

            // Fookuses läheb sisestusvälja ümardatud äär siniseks
            LisaFookuseEfekt(pnlNimetus, txtNimetus);
            LisaFookuseEfekt(pnlKogus, txtKogus);
            LisaFookuseEfekt(pnlHind, txtHind);
            LisaFookuseEfekt(pnlKategooria, cmbKategooria);

            // Sisestuskontroll: Kogus ainult numbrid, Hind numbrid + üks koma/punkt
            txtKogus.KeyPress += txtKogus_KeyPress;
            txtHind.KeyPress += txtHind_KeyPress;

            // Nuppude ühendamine
            btnLisa.Click += btnLisa_Click;
            btnUuenda.Click += btnUuenda_Click;
            btnKustuta.Click += btnKustuta_Click;
            btnOtsiFail.Click += btnOtsiFail_Click;
            btnLisaKategooria.Click += btnLisaKategooria_Click;
            btnKustutaKategooria.Click += btnKustutaKategooria_Click;
            dataGridTooted.CellClick += dataGridTooted_CellClick;
            dataGridTooted.CellFormatting += dataGridTooted_CellFormatting;
            dataGridTooted.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0 && tabKassa.Valitud) Valin(); };
            Load += Form1_Load;

            btnPuhasta.Click += (s, e) => { Tuhjenda(); dataGridTooted.ClearSelection(); };

            // Wordi-laadsed vahekaardid
            tabToode.Click += (s, e) => ValiVaheleht(tabToode);
            tabKassa.Click += (s, e) => ValiVaheleht(tabKassa);

            // Toote lisaandmed (kaubamärk, koostisosad, toiteväärtus ...) – nupp tööriistaribal
            var btnTooteinfo = UiStiil.LooNupp("Tooteinfo…", (s, e) => MuudaTooteinfot());
            btnTooteinfo.Margin = btnLisa.Margin;
            ridaToode.Controls.Add(btnTooteinfo);

            LooOtsing();
            SeadistaKassa();
            if (Teema.OnMoodne) RakendaMoodne();   // Ctrl+Shift+M: tume menüü vasakul, KPI-d, paneel paremal

            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            // Teema (hele/tume + poe värv): värvime kõik elemendid, ka Designeris loodud
            Teema.Rakenda(this);
            Teema.Muutus += VarviUmber;
            FormClosed += (s, e) => Teema.Muutus -= VarviUmber;

            UiStiil.NupudUhelReal(this, pnlTop);
        }

        private void MuudaTooteinfot()
        {
            if (ValitudId() is not int id) return;
            var vorm = new TooteInfoVorm(id);
            Aken.Ava(this, vorm, () =>
            {
                if (vorm.DialogResult != DialogResult.OK) return;
                Teade.Toast("Tooteinfo salvestatud");
                LaadiTooted();
            });
        }

        // Teema muutus menüüst Vaade: värvime kõik ümber
        private void VarviUmber()
        {
            KontrolliMoodneVahetust();
            if (moodne) BackColor = Teema.Taust;
            NaitaDisain();
            UuendaKeeleTekstid();
            Teema.Rakenda(this);
            korvVaade.VarviUmber();
            dataGridTooted.Invalidate();
        }

        private void ValiVaheleht(RibbonTab tab)
        {
            tabToode.Valitud = tab == tabToode;
            tabKassa.Valitud = tab == tabKassa;
            pnlTop.Visible = !moodne && tab == tabToode;   // moodsas disainis vana lint ei ilmu (ka hetkeks mitte)
            pnlKassaRibbon.Visible = !moodne && tab == tabKassa;

            // Kassa vahelehel on toote sisestuse asemel ostukorv
            kaartSisestus.Visible = tab == tabToode;
            kaartKorv.Visible = tab == tabKassa;
            MargiKulg(tab == tabToode ? "tooted" : "kassa");
            MoodneVaheleht(tab == tabToode);
        }

        private static void LisaFookuseEfekt(RoundedPanel raam, Control sisu)
        {
            UiStiil.LisaFookus(raam, sisu);
        }

        // ---------------------------------------------------------------
        //  Käivitamine (andmebaas luuakse Program.cs-is: db.Database.Migrate())
        // ---------------------------------------------------------------
        private void Form1_Load(object? sender, EventArgs e)
        {
            try
            {
                LaadiKategooriad();
                LaadiTooted();
            }
            catch (Exception ex)
            {
                Teade.Show("Andmete laadimine ebaõnnestus:\n" + ex.Message,
                    "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------------------------------------------------------
        //  Andmete laadimine (EF Core)
        // ---------------------------------------------------------------
        private void LaadiKategooriad()
        {
            object? eelmine = cmbKategooria.SelectedValue;

            using var db = new ToodedContext();
            cmbKategooria.DataSource = db.Kategooriatabel
                .OrderBy(k => k.Kategooria_nimetus)
                .ToList();
            cmbKategooria.DisplayMember = nameof(Kategooria.Kategooria_nimetus);
            cmbKategooria.ValueMember = nameof(Kategooria.Id);

            if (eelmine is int)
            {
                cmbKategooria.SelectedValue = eelmine;
            }
        }

        private void LaadiTooted()
        {
            using var db = new ToodedContext();
            koikTooted = db.Toodetabel
                .OrderBy(t => t.Id)
                .Select(t => new TooteVaade
                {
                    Id = t.Id,
                    Toodenimetus = t.Toodenimetus,
                    Kogus = t.Kogus,
                    Hind = t.Hind,
                    Pilt = t.Pilt,
                    KategooriaId = t.KategooriaId,                // vajalik ComboBoxi jaoks
                    Kategooria = t.Kategooria.Kategooria_nimetus
                })
                .ToList();

            UuendaOtsinguKategooriad();
            NaitaTooted();   // arvestab otsingut (Form1Otsing.cs)
            UuendaMoodneKpi();
        }

        // Väikese laoseisuga tooted on tabelis punaka taustaga (hoiatus)
        private void dataGridTooted_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.CellStyle == null) return;
            var kogus = dataGridTooted.Rows[e.RowIndex].Cells["Kogus"].Value;
            if (kogus is int k && k < VaikeLaoseis)
            {
                e.CellStyle.BackColor = k == 0 ? Teema.VigaTaust2 : Teema.VigaTaust;
                e.CellStyle.ForeColor = Teema.Viga;
            }
        }

        // ---------------------------------------------------------------
        //  Sisestuskontroll
        // ---------------------------------------------------------------
        private void txtKogus_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void txtHind_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar)) return;

            bool eraldaja = e.KeyChar == ',' || e.KeyChar == '.';
            bool juba = txtHind.Text.IndexOf(',') >= 0 || txtHind.Text.IndexOf('.') >= 0;
            if (!eraldaja || juba) e.Handled = true;
        }

        // Kontrollib kõik väljad ja täidab toote andmed vormilt.
        // Tagastab false, kui midagi on valesti. Negatiivsed väärtused pole lubatud.
        private bool LoeVorm(Toode toode)
        {
            string nimi = txtNimetus.Text.Trim();
            if (nimi.Length == 0)
            {
                Teade.Show("Sisesta toote nimetus!");
                txtNimetus.Focus();
                return false;
            }

            if (!int.TryParse(txtKogus.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int kogus))
            {
                Teade.Show("Kogus peab olema täisarv (0 või rohkem)!");
                txtKogus.Focus();
                return false;
            }

            // Lubame nii koma kui punkti: 2,50 või 2.50
            string hindTekst = txtHind.Text.Trim().Replace(',', '.');
            if (!decimal.TryParse(hindTekst, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal hind)
                || hind < 0 || hind > 99999999m)
            {
                Teade.Show("Hind peab olema arv (0 või rohkem)!");
                txtHind.Focus();
                return false;
            }

            if (cmbKategooria.SelectedValue is not int kategooriaId)
            {
                Teade.Show("Vali kategooria (või lisa uus)!");
                return false;
            }

            toode.Toodenimetus = nimi;
            toode.Kogus = kogus;
            toode.Hind = Math.Round(hind, 2);
            toode.KategooriaId = kategooriaId;

            // Kui uut pilti ei valitud, jääb vana pilt alles
            if (uusPiltTee != null)
                toode.Pilt = KopeeriPilt(uusPiltTee);

            return true;
        }

        // ---------------------------------------------------------------
        //  Toode: lisa / uuenda / kustuta
        // ---------------------------------------------------------------
        private void btnLisa_Click(object? sender, EventArgs e)
        {
            try
            {
                var uus = new Toode();
                if (!LoeVorm(uus)) return;

                using var db = new ToodedContext();
                db.Toodetabel.Add(uus);
                db.SaveChanges();

                Teade.Toast(Keel.F("Toode „{0}“ lisatud", uus.Toodenimetus));
                Tuhjenda();
                LaadiTooted();
            }
            catch (Exception ex)
            {
                Teade.Show("Viga lisamisel:\n" + ex.Message);
            }
        }

        private void btnUuenda_Click(object? sender, EventArgs e)
        {
            if (ValitudId() is not int id) return;

            try
            {
                using var db = new ToodedContext();
                var toode = db.Toodetabel.Find(id);
                if (toode == null)
                {
                    Teade.Show("Toodet ei leitud (võib-olla juba kustutatud).");
                    LaadiTooted();
                    return;
                }
                if (!LoeVorm(toode)) return;

                db.SaveChanges();

                Teade.Toast("Toode uuendatud");
                uusPiltTee = null;
                LaadiTooted();
                ValiRida(id);
            }
            catch (Exception ex)
            {
                Teade.Show("Viga uuendamisel:\n" + ex.Message);
            }
        }

        private void btnKustuta_Click(object? sender, EventArgs e)
        {
            if (ValitudId() is not int id) return;

            if (Teade.Show("Kas kustutan valitud toote?", "Kinnitus",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                using var db = new ToodedContext();
                var toode = db.Toodetabel.Find(id);
                if (toode != null)
                {
                    db.Toodetabel.Remove(toode);
                    db.SaveChanges();
                }

                Teade.Toast("Toode kustutatud", TeateTyyp.Info);
                Tuhjenda();
                LaadiTooted();
            }
            catch (Exception ex)
            {
                Teade.Show("Viga kustutamisel:\n" + ex.Message);
            }
        }

        private int? ValitudId()
        {
            if (dataGridTooted.CurrentRow?.Cells["Id"].Value is int id)
                return id;

            Teade.Show("Vali tabelist toode!");
            return null;
        }

        private void ValiRida(int id)
        {
            foreach (DataGridViewRow rida in dataGridTooted.Rows)
            {
                if (rida.Cells["Id"].Value is int reaId && reaId == id)
                {
                    dataGridTooted.CurrentCell = rida.Cells["Toodenimetus"];
                    rida.Selected = true;
                    break;
                }
            }
        }

        // ---------------------------------------------------------------
        //  Tabeli klikk -> näita andmed väljadel
        // ---------------------------------------------------------------
        private void dataGridTooted_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;                         // klikk päisel

            DataGridViewRow rida = dataGridTooted.Rows[e.RowIndex];
            txtNimetus.Text = Convert.ToString(rida.Cells["Toodenimetus"].Value);
            txtKogus.Text = Convert.ToString(rida.Cells["Kogus"].Value);
            txtHind.Text = Convert.ToDecimal(rida.Cells["Hind"].Value).ToString("0.00", CultureInfo.InvariantCulture);
            cmbKategooria.SelectedValue = rida.Cells["KategooriaId"].Value;

            uusPiltTee = null;                                  // olemasolevat pilti ei kopeerita uuesti
            string? pildiNimi = Convert.ToString(rida.Cells["Pilt"].Value);
            PildiVahemalu.Too(pildiNimi);   // veebipildi puhul alustab allalaadimist
            NaitaPilti(Kaustad.PildiTee(pildiNimi));
        }

        // ---------------------------------------------------------------
        //  Pildid
        // ---------------------------------------------------------------
        private void btnOtsiFail_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "Pildifailid|*.jpg;*.jpeg;*.png;*.bmp;*.gif" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                uusPiltTee = dlg.FileName;
                NaitaPilti(uusPiltTee);
            }
        }

        // Laeb pildi mällu, et fail ei jääks lukku (muidu ei saa faili hiljem uuendada/kustutada)
        private void NaitaPilti(string? tee)
        {
            Image? vana = picPilt.Image;
            picPilt.Image = Kaustad.LaePilt(tee);
            vana?.Dispose();
        }

        // Kopeerib pildi kausta AppData\Local\ToodedApp\Images ja tagastab failinime
        // (andmebaasi salvestatakse ainult failinimi)
        private static string KopeeriPilt(string allikas)
        {
            Directory.CreateDirectory(Kaustad.Pildid);

            string nimi = Path.GetFileName(allikas);
            string siht = Path.Combine(Kaustad.Pildid, nimi);

            if (string.Equals(Path.GetFullPath(allikas), Path.GetFullPath(siht), StringComparison.OrdinalIgnoreCase))
                return nimi;

            if (File.Exists(siht))
            {
                nimi = Guid.NewGuid().ToString("N").Substring(0, 8) + "_" + nimi;
                siht = Path.Combine(Kaustad.Pildid, nimi);
            }

            File.Copy(allikas, siht);
            return nimi;
        }

        private void Tuhjenda()
        {
            txtNimetus.Clear();
            txtKogus.Clear();
            txtHind.Clear();
            uusPiltTee = null;
            NaitaPilti(null);
        }

        // ---------------------------------------------------------------
        //  Kategooriad
        // ---------------------------------------------------------------
        private void btnLisaKategooria_Click(object? sender, EventArgs e)
        {
            string? uus = KysiTekst("Lisa kategooria", "Sisesta uue kategooria nimi:");
            if (string.IsNullOrWhiteSpace(uus)) return;
            uus = uus.Trim();

            try
            {
                using var db = new ToodedContext();
                if (db.Kategooriatabel.Any(k => k.Kategooria_nimetus == uus))
                {
                    Teade.Show("Selline kategooria on juba olemas.");
                    return;
                }

                db.Kategooriatabel.Add(new Kategooria { Kategooria_nimetus = uus });
                db.SaveChanges();

                LaadiKategooriad();
            }
            catch (Exception ex)
            {
                Teade.Show("Viga kategooria lisamisel:\n" + ex.Message);
            }
        }

        private void btnKustutaKategooria_Click(object? sender, EventArgs e)
        {
            if (cmbKategooria.SelectedValue is not int id)
            {
                Teade.Show("Vali kategooria!");
                return;
            }

            string nimi = cmbKategooria.Text;

            if (Teade.Show(Keel.F("Kas kustutan kategooria „{0}“?", nimi), "Kinnitus",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                using var db = new ToodedContext();
                if (db.Toodetabel.Any(t => t.KategooriaId == id))
                {
                    Teade.Show("Seda kategooriat ei saa kustutada, sest selles on tooteid.\nKustuta või liiguta tooted enne teise kategooriasse.");
                    return;
                }

                var kategooria = db.Kategooriatabel.Find(id);
                if (kategooria != null)
                {
                    db.Kategooriatabel.Remove(kategooria);
                    db.SaveChanges();
                }

                LaadiKategooriad();
            }
            catch (Exception ex)
            {
                Teade.Show("Viga kategooria kustutamisel:\n" + ex.Message);
            }
        }

        // Väike dialoog teksti küsimiseks (asendab Microsoft.VisualBasic InputBox'i).
        // Tagastab null, kui vajutati "Tühista".
        private string? KysiTekst(string pealkiri, string kysimus, string algtekst = "")
        {
            using var dlg = new Form();
            dlg.Text = pealkiri;
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.MaximizeBox = false;
            dlg.MinimizeBox = false;
            dlg.ShowInTaskbar = false;
            dlg.Font = UiStiil.Põhifont;
            dlg.BackColor = Color.White;

            var silt = new Label { Text = kysimus, AutoSize = true, MaximumSize = new Size(348, 0), Location = new Point(16, 16) };
            int sildiKorgus = silt.GetPreferredSize(new Size(348, 0)).Height;
            var tekst = new TextBox { Location = new Point(16, 16 + sildiKorgus + 10), Width = 348, Text = algtekst };
            var ok = UiStiil.LooNupp("OK", null, true);
            var katkesta = UiStiil.LooNupp("Tühista", null);
            ok.DialogResult = DialogResult.OK;
            katkesta.DialogResult = DialogResult.Cancel;
            ok.Location = new Point(196, tekst.Bottom + 14);
            katkesta.Location = new Point(280, tekst.Bottom + 14);
            dlg.ClientSize = new Size(380, tekst.Bottom + 60);

            dlg.Controls.AddRange(new Control[] { silt, tekst, ok, katkesta });
            dlg.AcceptButton = ok;
            dlg.CancelButton = katkesta;
            Teema.Rakenda(dlg);

            // Omanik on aktiivne aken (nt Pood), et dialoog avaneks selle peale
            IWin32Window omanik = Form.ActiveForm ?? (IWin32Window)this;
            return dlg.ShowDialog(omanik) == DialogResult.OK ? tekst.Text : null;
        }
    }
}
