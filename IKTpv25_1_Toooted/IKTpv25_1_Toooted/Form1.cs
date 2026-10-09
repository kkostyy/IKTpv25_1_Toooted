using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace IKTpv25_1_Toooted
{
    public partial class Form1 : Form
    {
        // Uus valitud pildifail (täistee); null = pilti ei muudeta
        private string uusPiltTee = null;

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
            Load += Form1_Load;

            btnPuhasta.Click += (s, e) => { Tuhjenda(); dataGridTooted.ClearSelection(); };

            // Wordi-laadsed vahekaardid
            tabToode.Click += (s, e) => ValiVaheleht(tabToode);
            tabKassa.Click += (s, e) => ValiVaheleht(tabKassa);

            SeadistaKassa();

            UiStiil.NupudUhelReal(this, pnlTop);
        }

        private void ValiVaheleht(RibbonTab tab)
        {
            tabToode.Valitud = tab == tabToode;
            tabKassa.Valitud = tab == tabKassa;
            pnlTop.Visible = tab == tabToode;
            pnlKassaRibbon.Visible = tab == tabKassa;
        }

        private static void LisaFookuseEfekt(RoundedPanel raam, Control sisu)
        {
            Color tavaline = Color.FromArgb(200, 204, 210);
            sisu.Enter += (s, e) => { raam.BorderColor = UiStiil.Sinine; raam.Invalidate(); };
            sisu.Leave += (s, e) => { raam.BorderColor = tavaline; raam.Invalidate(); };
            raam.Click += (s, e) => sisu.Focus();
        }

        // ---------------------------------------------------------------
        //  Käivitamine
        // ---------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                AndmeBaas.Valmista();
                LaadiKategooriad();
                LaadiTooted();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Andmebaasiga ühendamine ebaõnnestus:\n" + ex.Message,
                    "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------------------------------------------------------
        //  Andmete laadimine
        // ---------------------------------------------------------------
        private void LaadiKategooriad()
        {
            object eelmine = cmbKategooria.SelectedValue;

            var dt = new DataTable();
            using (var c = new SqlConnection(AndmeBaas.Uhendus))
            using (var da = new SqlDataAdapter("SELECT Id, Nimetus FROM Kategooriad ORDER BY Nimetus", c))
            {
                da.Fill(dt);
            }

            cmbKategooria.DataSource = dt;
            cmbKategooria.DisplayMember = "Nimetus";
            cmbKategooria.ValueMember = "Id";

            if (eelmine is int)
            {
                cmbKategooria.SelectedValue = eelmine;
            }
        }

        private void LaadiTooted()
        {
            var dt = new DataTable();
            using (var c = new SqlConnection(AndmeBaas.Uhendus))
            using (var da = new SqlDataAdapter(
                @"SELECT t.Id, t.Toodenimetus, t.Kogus, t.Hind, t.Pilt, t.Kategooria_Id, k.Nimetus AS Kategooria
                  FROM Tooted t LEFT JOIN Kategooriad k ON k.Id = t.Kategooria_Id
                  ORDER BY t.Id", c))
            {
                da.Fill(dt);
            }

            dataGridTooted.DataSource = dt;
            dataGridTooted.Columns["Id"].Visible = false;
            dataGridTooted.Columns["Kategooria_Id"].Visible = false;
            dataGridTooted.Columns["Toodenimetus"].HeaderText = "Toode";
            dataGridTooted.Columns["Hind"].DefaultCellStyle.Format = "0.00";
            dataGridTooted.ClearSelection();
        }

        // ---------------------------------------------------------------
        //  Sisestuskontroll
        // ---------------------------------------------------------------
        private void txtKogus_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void txtHind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar)) return;

            bool eraldaja = e.KeyChar == ',' || e.KeyChar == '.';
            bool juba = txtHind.Text.IndexOf(',') >= 0 || txtHind.Text.IndexOf('.') >= 0;
            if (!eraldaja || juba) e.Handled = true;
        }

        // Kontrollib kõik väljad; negatiivsed väärtused pole lubatud
        private bool LuguVaartused(out string nimi, out int kogus, out decimal hind, out int kategooriaId)
        {
            nimi = txtNimetus.Text.Trim();
            kogus = 0;
            hind = 0;
            kategooriaId = 0;

            if (nimi.Length == 0)
            {
                MessageBox.Show("Sisesta toote nimetus!");
                txtNimetus.Focus();
                return false;
            }

            if (!int.TryParse(txtKogus.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out kogus))
            {
                MessageBox.Show("Kogus peab olema täisarv (0 või rohkem)!");
                txtKogus.Focus();
                return false;
            }

            string hindTekst = txtHind.Text.Trim().Replace(',', '.');
            if (!decimal.TryParse(hindTekst, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out hind)
                || hind < 0 || hind > 99999999m)
            {
                MessageBox.Show("Hind peab olema arv (0 või rohkem)!");
                txtHind.Focus();
                return false;
            }
            hind = Math.Round(hind, 2);

            if (!(cmbKategooria.SelectedValue is int))
            {
                MessageBox.Show("Vali kategooria (või lisa uus)!");
                return false;
            }
            kategooriaId = (int)cmbKategooria.SelectedValue;
            return true;
        }

        // ---------------------------------------------------------------
        //  Toode: lisa / uuenda / kustuta
        // ---------------------------------------------------------------
        private void btnLisa_Click(object sender, EventArgs e)
        {
            string nimi; int kogus; decimal hind; int kat;
            if (!LuguVaartused(out nimi, out kogus, out hind, out kat)) return;

            try
            {
                string pilt = uusPiltTee != null ? KopeeriPilt(uusPiltTee) : "";

                using (var c = new SqlConnection(AndmeBaas.Uhendus))
                using (var cmd = new SqlCommand(
                    "INSERT INTO Tooted (Toodenimetus, Kogus, Hind, Pilt, Kategooria_Id) VALUES (@nimetus, @kogus, @hind, @pilt, @kat)", c))
                {
                    LisaParameetrid(cmd, nimi, kogus, hind, pilt, kat);
                    c.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Toode lisatud!");
                Tuhjenda();
                LaadiTooted();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga lisamisel:\n" + ex.Message);
            }
        }

        private void btnUuenda_Click(object sender, EventArgs e)
        {
            int id;
            if (!ValitudId(out id)) return;

            string nimi; int kogus; decimal hind; int kat;
            if (!LuguVaartused(out nimi, out kogus, out hind, out kat)) return;

            try
            {
                // Kui uut pilti ei valitud, jääb vana pilt alles (ISNULL)
                string pilt = uusPiltTee != null ? KopeeriPilt(uusPiltTee) : null;

                using (var c = new SqlConnection(AndmeBaas.Uhendus))
                using (var cmd = new SqlCommand(
                    "UPDATE Tooted SET Toodenimetus=@nimetus, Kogus=@kogus, Hind=@hind, Pilt=ISNULL(@pilt, Pilt), Kategooria_Id=@kat WHERE Id=@id", c))
                {
                    LisaParameetrid(cmd, nimi, kogus, hind, pilt, kat);
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    c.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Toode uuendatud!");
                uusPiltTee = null;
                LaadiTooted();
                ValiRida(id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga uuendamisel:\n" + ex.Message);
            }
        }

        private void btnKustuta_Click(object sender, EventArgs e)
        {
            int id;
            if (!ValitudId(out id)) return;

            if (MessageBox.Show("Kas kustutan valitud toote?", "Kinnitus",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                using (var c = new SqlConnection(AndmeBaas.Uhendus))
                using (var cmd = new SqlCommand("DELETE FROM Tooted WHERE Id=@id", c))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    c.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Toode kustutatud!");
                Tuhjenda();
                LaadiTooted();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga kustutamisel:\n" + ex.Message);
            }
        }

        private static void LisaParameetrid(SqlCommand cmd, string nimi, int kogus, decimal hind, string pilt, int kat)
        {
            cmd.Parameters.Add("@nimetus", SqlDbType.NVarChar, 100).Value = nimi;
            cmd.Parameters.Add("@kogus", SqlDbType.Int).Value = kogus;

            var hindParam = cmd.Parameters.Add("@hind", SqlDbType.Decimal);
            hindParam.Precision = 10;
            hindParam.Scale = 2;
            hindParam.Value = hind;

            cmd.Parameters.Add("@pilt", SqlDbType.NVarChar, 255).Value = (object)pilt ?? DBNull.Value;
            cmd.Parameters.Add("@kat", SqlDbType.Int).Value = kat;
        }

        private bool ValitudId(out int id)
        {
            id = 0;
            if (dataGridTooted.CurrentRow == null || dataGridTooted.CurrentRow.Cells["Id"].Value == DBNull.Value)
            {
                MessageBox.Show("Vali tabelist toode!");
                return false;
            }
            id = Convert.ToInt32(dataGridTooted.CurrentRow.Cells["Id"].Value);
            return true;
        }

        private void ValiRida(int id)
        {
            foreach (DataGridViewRow rida in dataGridTooted.Rows)
            {
                if (Convert.ToInt32(rida.Cells["Id"].Value) == id)
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
        private void dataGridTooted_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow rida = dataGridTooted.Rows[e.RowIndex];
            txtNimetus.Text = Convert.ToString(rida.Cells["Toodenimetus"].Value);
            txtKogus.Text = Convert.ToString(rida.Cells["Kogus"].Value);
            txtHind.Text = Convert.ToDecimal(rida.Cells["Hind"].Value).ToString("0.00", CultureInfo.InvariantCulture);

            if (rida.Cells["Kategooria_Id"].Value != DBNull.Value)
                cmbKategooria.SelectedValue = rida.Cells["Kategooria_Id"].Value;

            uusPiltTee = null;
            string pilt = Convert.ToString(rida.Cells["Pilt"].Value);
            NaitaPilti(string.IsNullOrEmpty(pilt) ? null : Path.Combine(AndmeBaas.PildiKaust, pilt));
        }

        // ---------------------------------------------------------------
        //  Pildid
        // ---------------------------------------------------------------
        private void btnOtsiFail_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Pildifailid|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    uusPiltTee = dlg.FileName;
                    NaitaPilti(uusPiltTee);
                }
            }
        }

        // Laeb pildi mällu, et fail ei jääks lukku (muidu ei saa faili hiljem uuendada/kustutada)
        private void NaitaPilti(string tee)
        {
            Image vana = picPilt.Image;
            picPilt.Image = null;
            if (vana != null) vana.Dispose();

            if (string.IsNullOrEmpty(tee) || !File.Exists(tee)) return;

            try
            {
                using (var fs = new FileStream(tee, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var img = Image.FromStream(fs))
                {
                    picPilt.Image = new Bitmap(img);
                }
            }
            catch
            {
                // vigane pildifail – jäta pilt tühjaks
            }
        }

        // Kopeerib pildi kausta "Pildid" ja tagastab failinime (see salvestatakse andmebaasi)
        private static string KopeeriPilt(string allikas)
        {
            Directory.CreateDirectory(AndmeBaas.PildiKaust);

            string nimi = Path.GetFileName(allikas);
            string siht = Path.Combine(AndmeBaas.PildiKaust, nimi);

            if (string.Equals(Path.GetFullPath(allikas), Path.GetFullPath(siht), StringComparison.OrdinalIgnoreCase))
                return nimi;

            if (File.Exists(siht))
            {
                nimi = Guid.NewGuid().ToString("N").Substring(0, 8) + "_" + nimi;
                siht = Path.Combine(AndmeBaas.PildiKaust, nimi);
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
        private void btnLisaKategooria_Click(object sender, EventArgs e)
        {
            string uus = KysiTekst("Lisa kategooria", "Sisesta uue kategooria nimi:");
            if (string.IsNullOrWhiteSpace(uus)) return;

            try
            {
                using (var c = new SqlConnection(AndmeBaas.Uhendus))
                using (var cmd = new SqlCommand("INSERT INTO Kategooriad (Nimetus) VALUES (@nimetus)", c))
                {
                    cmd.Parameters.Add("@nimetus", SqlDbType.NVarChar, 50).Value = uus.Trim();
                    c.Open();
                    cmd.ExecuteNonQuery();
                }

                LaadiKategooriad();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga kategooria lisamisel:\n" + ex.Message);
            }
        }

        private void btnKustutaKategooria_Click(object sender, EventArgs e)
        {
            if (!(cmbKategooria.SelectedValue is int))
            {
                MessageBox.Show("Vali kategooria!");
                return;
            }

            int id = (int)cmbKategooria.SelectedValue;
            string nimi = cmbKategooria.Text;

            if (MessageBox.Show("Kas kustutan kategooria \"" + nimi + "\"?", "Kinnitus",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                using (var c = new SqlConnection(AndmeBaas.Uhendus))
                using (var cmd = new SqlCommand("DELETE FROM Kategooriad WHERE Id=@id", c))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    c.Open();
                    cmd.ExecuteNonQuery();
                }

                LaadiKategooriad();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Seda kategooriat ei saa kustutada, sest selles on tooteid.\nKustuta või liiguta tooted enne teise kategooriasse.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga kategooria kustutamisel:\n" + ex.Message);
            }
        }

        // Väike dialoog teksti küsimiseks (asendab Microsoft.VisualBasic InputBox'i)
        private string KysiTekst(string pealkiri, string kysimus)
        {
            using (var dlg = new Form())
            {
                dlg.Text = pealkiri;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.ShowInTaskbar = false;
                dlg.Font = UiStiil.Põhifont;
                dlg.BackColor = Color.White;
                dlg.ClientSize = new Size(380, 130);

                var silt = new Label { Text = kysimus, AutoSize = true, Location = new Point(16, 16) };
                var tekst = new TextBox { Location = new Point(16, 44), Width = 348 };
                var ok = UiStiil.LooNupp("OK", null, true);
                var katkesta = UiStiil.LooNupp("Tühista", null);
                ok.DialogResult = DialogResult.OK;
                katkesta.DialogResult = DialogResult.Cancel;
                ok.Location = new Point(196, 84);
                katkesta.Location = new Point(280, 84);

                dlg.Controls.AddRange(new Control[] { silt, tekst, ok, katkesta });
                dlg.AcceptButton = ok;
                dlg.CancelButton = katkesta;

                return dlg.ShowDialog(this) == DialogResult.OK ? tekst.Text : null;
            }
        }
    }
}
