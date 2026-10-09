using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IKTpv25_1_Toooted
{
    // Kassa vaheleht: Valin -> Ostan -> Maksta -> Saada arve, Pood = ostukorv
    public partial class Form1
    {
        private class KorvRida
        {
            public int Id;
            public string Nimi;
            public decimal Hind;
            public int Kogus;
            public int Saadaval; // laoseis valimise hetkel
            public decimal Summa { get { return Hind * Kogus; } }
        }

        private readonly List<KorvRida> korv = new List<KorvRida>();

        // Viimane tehtud ost (arve ja makse jaoks)
        private List<KorvRida> viimaneOst = null;
        private decimal viimaneSumma = 0;
        private bool viimaneMakstud = false;
        private DateTime viimaneAeg;

        private static string Eur(decimal v)
        {
            return v.ToString("0.00", CultureInfo.InvariantCulture) + " €";
        }

        private void SeadistaKassa()
        {
            btnValin.Click += btnValin_Click;
            btnOstan.Click += btnOstan_Click;
            btnMaksta.Click += btnMaksta_Click;
            btnSaadaArve.Click += btnSaadaArve_Click;
            btnPood.Click += btnPood_Click;
            UuendaKokku();
        }

        private void UuendaKokku()
        {
            txtKokku.Text = Eur(korv.Sum(r => r.Summa));
        }

        // ---- Valin: lisa tabelis valitud toode ostukorvi ----
        private void btnValin_Click(object sender, EventArgs e)
        {
            int id;
            if (!ValitudId(out id)) return;

            DataGridViewRow rida = dataGridTooted.CurrentRow;
            string nimi = Convert.ToString(rida.Cells["Toodenimetus"].Value);
            decimal hind = Convert.ToDecimal(rida.Cells["Hind"].Value);
            int laos = Convert.ToInt32(rida.Cells["Kogus"].Value);

            KorvRida olemas = korv.FirstOrDefault(r => r.Id == id);
            int korvis = olemas != null ? olemas.Kogus : 0;
            if (korvis + 1 > laos)
            {
                MessageBox.Show("Toodet \"" + nimi + "\" pole rohkem laos (laos: " + laos + ").");
                return;
            }

            if (olemas != null)
            {
                olemas.Kogus++;
                olemas.Saadaval = laos;
            }
            else
            {
                korv.Add(new KorvRida { Id = id, Nimi = nimi, Hind = hind, Kogus = 1, Saadaval = laos });
            }
            UuendaKokku();
        }

        // ---- Ostan: vähenda laoseisu ja salvesta ost ----
        private void btnOstan_Click(object sender, EventArgs e)
        {
            if (korv.Count == 0)
            {
                MessageBox.Show("Ostukorv on tühi. Vali enne tooted nupuga \"Valin\".");
                return;
            }

            if (viimaneOst != null && !viimaneMakstud)
            {
                MessageBox.Show("Eelmine ost on veel maksmata. Vajuta \"Maksta\".");
                return;
            }

            try
            {
                using (var c = new SqlConnection(AndmeBaas.Uhendus))
                {
                    c.Open();
                    using (var tr = c.BeginTransaction())
                    {
                        foreach (KorvRida r in korv)
                        {
                            using (var cmd = new SqlCommand(
                                "UPDATE Tooted SET Kogus = Kogus - @k WHERE Id = @id AND Kogus >= @k", c, tr))
                            {
                                cmd.Parameters.Add("@k", SqlDbType.Int).Value = r.Kogus;
                                cmd.Parameters.Add("@id", SqlDbType.Int).Value = r.Id;
                                if (cmd.ExecuteNonQuery() == 0)
                                {
                                    tr.Rollback();
                                    MessageBox.Show("Toodet \"" + r.Nimi + "\" pole piisavalt laos. Ost katkestati.");
                                    return;
                                }
                            }
                        }
                        tr.Commit();
                    }
                }

                viimaneOst = korv.Select(r => new KorvRida
                {
                    Id = r.Id, Nimi = r.Nimi, Hind = r.Hind, Kogus = r.Kogus
                }).ToList();
                viimaneSumma = viimaneOst.Sum(r => r.Summa);
                viimaneMakstud = false;
                viimaneAeg = DateTime.Now;

                korv.Clear();
                UuendaKokku();
                LaadiTooted();

                MessageBox.Show("Ost tehtud. Summa: " + Eur(viimaneSumma) + "\nVajuta \"Maksta\".");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga ostmisel:\n" + ex.Message);
            }
        }

        // ---- Maksta ----
        private void btnMaksta_Click(object sender, EventArgs e)
        {
            if (viimaneOst == null)
            {
                MessageBox.Show("Pole midagi maksta. Tee enne ost nupuga \"Ostan\".");
                return;
            }
            if (viimaneMakstud)
            {
                MessageBox.Show("See ost on juba makstud.");
                return;
            }

            string sisend = KysiTekst("Maksmine", "Summa " + Eur(viimaneSumma) + ". Sisesta makstud summa:");
            if (sisend == null) return;

            decimal makstud;
            if (!decimal.TryParse(sisend.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out makstud) || makstud < 0)
            {
                MessageBox.Show("Sisesta korrektne summa.");
                return;
            }
            if (makstud < viimaneSumma)
            {
                MessageBox.Show("Makstud summa on liiga väike (puudu " + Eur(viimaneSumma - makstud) + ").");
                return;
            }

            viimaneMakstud = true;
            MessageBox.Show("Makstud!\nTagasiraha: " + Eur(makstud - viimaneSumma));
        }

        // ---- Saada arve: avab e-posti programmi valmis kirjaga ----
        private void btnSaadaArve_Click(object sender, EventArgs e)
        {
            if (viimaneOst == null)
            {
                MessageBox.Show("Arve jaoks tee enne ost nupuga \"Ostan\".");
                return;
            }

            string email = KysiTekst("Saada arve", "Kliendi e-posti aadress:");
            if (string.IsNullOrWhiteSpace(email)) return;
            email = email.Trim();
            if (email.IndexOf('@') < 1 || email.IndexOf('.', email.IndexOf('@')) < 0)
            {
                MessageBox.Show("E-posti aadress pole korrektne.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Arve " + viimaneAeg.ToString("yyyy-MM-dd HH:mm"));
            sb.AppendLine("----------------------------------------");
            foreach (KorvRida r in viimaneOst)
                sb.AppendLine(r.Nimi + "  " + r.Kogus + " x " + Eur(r.Hind) + " = " + Eur(r.Summa));
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Kokku: " + Eur(viimaneSumma));
            sb.AppendLine(viimaneMakstud ? "Staatus: MAKSTUD" : "Staatus: MAKSMATA");

            string url = "mailto:" + email + "?subject=" + Uri.EscapeDataString("Arve")
                         + "&body=" + Uri.EscapeDataString(sb.ToString());
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show("E-posti programmi ei õnnestunud avada:\n" + ex.Message);
            }
        }

        // ---- Pood: avab visuaalse kassa vormi (praegu ainult kujundus) ----
        private void btnPood_Click(object sender, EventArgs e)
        {
            using (var vorm = new PoodVorm())
            {
                vorm.ShowDialog(this);
            }
        }
    }
}
