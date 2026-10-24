using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    // Kassa vaheleht: Valin -> Ostan/Maksta (üks aken: klient, boonus, e-post, makse) -> tšekk e-postile.
    // Pood = visuaalne kassa.
    public partial class Form1
    {
        // Boonus: iga tootele kulutatud euro annab 1 sendi (1 %) boonust; boonusega saab järgmisel ostul maksta
        public const decimal BoonusProtsent = 0.01m;
        // Sellest väiksema laoseisu korral tuleb hoiatus
        public const int VaikeLaoseis = 10;

        // Viimane tehtud ost ja selle tšekk ("Saada arve" jaoks)
        private Ost? viimaneOst = null;
        private string? viimanePdf = null;

        // Ostukorvi kaart Kassa vahelehel (luuakse koodis, Designer-faili ei muudeta)
        private readonly RoundedPanel kaartKorv = new();
        private KorvVaade korvVaade = null!;

        private void SeadistaKassa()
        {
            btnValin.Click += (s, e) => Valin();
            btnOstan.Click += (s, e) => Vormista(null, this);
            btnMaksta.Click += (s, e) => Vormista(null, this);
            btnSaadaArve.Click += (s, e) => SaadaArve();
            btnPood.Click += (s, e) => AvaPood();

            LooKorviKaart();
            LooMenuu();

            Ostukorv.Muutus += UuendaKokku;
            FormClosed += (s, e) => Ostukorv.Muutus -= UuendaKokku;
            UuendaKokku();
        }

        // ---------------------------------------------------------------
        //  Ostukorvi kaart (näha Kassa vahelehel)
        // ---------------------------------------------------------------
        private void LooKorviKaart()
        {
            korvVaade = new KorvVaade();
            var sisu = UiStiil.LooKaardiPaneel("Ostukorv", korvVaade);

            kaartKorv.Dock = DockStyle.Top;
            kaartKorv.Height = 250;
            kaartKorv.Padding = new Padding(0);
            kaartKorv.BorderColor = Color.Transparent;   // raami joonistab sisemine kaart
            kaartKorv.Visible = false;
            sisu.Dock = DockStyle.Fill;
            kaartKorv.Controls.Add(sisu);

            pnlSisu.Controls.Add(kaartKorv);
            kaartKorv.SendToBack();   // dokitakse esimesena = kõige üles
        }

        private void UuendaKokku()
        {
            txtKokku.Text = Ostukorv.Eur(Ostukorv.Kokku + Ostukorv.PantKokku);   // koos pandiga
        }

        // ---------------------------------------------------------------
        //  Valin: lisa tabelis valitud toode ostukorvi
        // ---------------------------------------------------------------
        private void Valin()
        {
            if (ValitudId() is not int id) return;

            DataGridViewRow rida = dataGridTooted.CurrentRow!;
            string nimi = Convert.ToString(rida.Cells["Toodenimetus"].Value) ?? "";
            decimal hind = Convert.ToDecimal(rida.Cells["Hind"].Value);
            int laos = Convert.ToInt32(rida.Cells["Kogus"].Value);
            string? pilt = Convert.ToString(rida.Cells["Pilt"].Value);

            // Toode andmebaasist: lõpphind (soodustused) ja pakendi tagatisraha
            Toode? toode;
            using (var db = new ToodedContext()) toode = db.Toodetabel.AsNoTracking().FirstOrDefault(t => t.Id == id);
            string? viga = toode != null ? Ostukorv.LisaToode(toode) : Ostukorv.Lisa(id, nimi, hind, laos, 1, pilt);
            if (viga != null) Teade.Toast(viga, TeateTyyp.Hoiatus);
            else Teade.Toast(Keel.F("{0} lisati korvi", nimi), TeateTyyp.Edu, 1500);
        }

        // ---------------------------------------------------------------
        //  Ostan / Maksta: üks aken (klient, boonus, e-post, makse)
        //  Tagastab true, kui ost tehti (avalik, et ka Pood saaks seda kasutada)
        // ---------------------------------------------------------------
        public void Vormista(Action? tehtud = null, IWin32Window? omanik = null)
        {
            if (Ostukorv.Ridad.Count == 0)
            {
                Teade.Toast("Ostukorv on tühi. Lisa tooteid nupuga \"Valin\" või \"Pood\".", TeateTyyp.Hoiatus);
                return;
            }

            var vorm = new MaksmineVorm();
            // moodne: maksmine avaneb samas aknas (poe leht / peaakna sisu)
            Aken.Ava(omanik ?? Form.ActiveForm ?? (IWin32Window)this, vorm, () =>
            {
                if (vorm.DialogResult == DialogResult.OK && vorm.Tulemus != null && PealeMaksmist(vorm)) tehtud?.Invoke();
            });
        }

        private bool PealeMaksmist(MaksmineVorm vorm)
        {

            Ost ost = vorm.Tulemus;
            viimaneOst = ost;
            viimanePdf = vorm.PdfFail;

            Ostukorv.Tyhjenda();
            LaadiTooted();

            decimal tagasi = ost.MakstudSumma - ost.Summa;
            Teade.Toast(Keel.F("Ost nr {0} makstud: {1}", ost.Id, Ostukorv.Eur(ost.Summa)) +
                        (tagasi > 0 ? "\n" + Keel.F("Tagasiraha: {0}", Ostukorv.Eur(tagasi)) : "") +
                        (ost.Klient != null ? "\n" + Keel.F("Boonust lisandus {0}", Ostukorv.Eur(ost.BoonusLisatud)) : ""),
                        TeateTyyp.Edu, 5000);

            // Ostunimekiri: ostetud tooted saavad linnukese (ostja või töötaja oma kaardiga)
            if (AktiivneKasutaja.OnOstja || (ost.KlientId != null && ost.KlientId == AktiivneKasutaja.KlientId))
            {
                int margitud = Ostunimekiri.MargiOstetuks(ost);
                if (margitud > 0) Teade.Toast(Keel.F("Ostunimekirjas märgiti ostetuks: {0}", margitud), TeateTyyp.Info, 3000);
                AktiivneKasutaja.Uuenda();   // boonus muutus
            }

            // Kliendikaardi preemiad: väljakutsed, kleebised, uus tase
            if (ost.KlientId != null)
            {
                try
                {
                    using var db = new ToodedContext();
                    foreach (var tekst in Preemiad.KontrolliPealeOstu(db, ost))
                        Teade.Toast(tekst, TeateTyyp.Edu, 5000);
                    AktiivneKasutaja.Uuenda();
                }
                catch { }
            }

            if (vorm.VaheseLaoga.Count > 0 && !AktiivneKasutaja.OnOstja)
                Teade.Toast("Väike laoseis:\n" + string.Join("\n", vorm.VaheseLaoga), TeateTyyp.Hoiatus, 6000);

            // Tšekk e-postile taustal (programm ei jää ootama)
            if (ost.Email != null && viimanePdf != null)
                SaadaTaustal(ost, viimanePdf, ost.Email);

            return true;
        }

        // Saadab tšeki e-postiga taustal ja annab tulemusest teada
        private async void SaadaTaustal(Ost ost, string pdf, string email)
        {
            Teade.Toast(Keel.F("Saadan tšeki aadressile {0}...", email), TeateTyyp.Info, 2000);
            try
            {
                string tulemus = await Task.Run(() => Arve.Saada(email, pdf, ost));
                Teade.Toast(tulemus, TeateTyyp.Edu, 4000);
            }
            catch (Exception ex)
            {
                Teade.Show("Tšeki saatmine e-postile ebaõnnestus:\n" + ex.Message +
                           "\n\nProovi uuesti nupuga \"Saada arve\" või kontrolli Haldus → E-posti seaded.",
                           "E-post", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------------------------------------------------------------
        //  Saada arve: viimase ostu tšekk uuesti (soovi korral teisele aadressile)
        // ---------------------------------------------------------------
        public void SaadaArve()
        {
            if (viimaneOst == null)
            {
                Teade.Show("Selles seansis pole veel ostu tehtud.\nVanemaid tšekke leiad menüüst Haldus → Ostude ajalugu.",
                    "Saada arve");
                return;
            }

            if (viimanePdf == null || !File.Exists(viimanePdf))
            {
                try { viimanePdf = Arve.LooPdf(viimaneOst, viimaneOst.Makstud, viimaneOst.MakstudSumma); }
                catch (Exception ex) { Teade.Show("PDF-i loomine ebaõnnestus:\n" + ex.Message, "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            }

            string? email = KysiTekst("Saada arve",
                Keel.F("Ost nr {0} ({1}).", viimaneOst.Id, Ostukorv.Eur(viimaneOst.Summa)) + "\n" + Keel.T("Kliendi e-post (tühi = ava PDF):"),
                viimaneOst.Email ?? "");
            if (email == null) return;

            if (string.IsNullOrWhiteSpace(email))
            {
                Arve.Ava(viimanePdf);
                return;
            }
            if (!EmailSeaded.OnKorrektneAadress(email))
            {
                Teade.Show("E-posti aadress pole korrektne.", "E-post", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaadaTaustal(viimaneOst, viimanePdf, email.Trim());
        }

        // Kui ost tagastati ostude ajaloos
        private void OstTagastati(int ostId)
        {
            if (viimaneOst != null && viimaneOst.Id == ostId)
            {
                viimaneOst = null;
                viimanePdf = null;
            }
        }

        // ---------------------------------------------------------------
        //  Pood: visuaalne kassa
        // ---------------------------------------------------------------
        // Disaini järgi: e-poe (Rimi) pood või klassikaline pood.
        // Kui poes vajutati Ctrl+Shift+D, avatakse kohe teise disaini pood.
        private void AvaPood()
        {
            // Moodne: pood avaneb peaakna sees (üks aken); disaini vahetusel avatakse uuesti
            if (Teema.OnMoodne && Teema.OnRimi && Visible && VoibManustada)
            {
                var pood = new PoodVorm(this);
                moodneMenuu?.Vali("pood");   // menüüs pole ühtki punkti valitud – lahti on pood
                Aken.Ava(this, pood, () =>
                {
                    if (Equals(pood.Tag, "disain") && Teema.OnMoodne) BeginInvoke(new Action(AvaPood));
                    LaadiTooted();
                });
                return;
            }
            bool uuesti;
            do
            {
                using Form vorm = Teema.OnRimi ? new PoodVorm(this) : new PoodKlassikVorm(this);
                if (Visible) vorm.ShowDialog(this);
                else
                {
                    // Ostja: peaaken on peidetud, pood on omaette aken
                    vorm.StartPosition = FormStartPosition.CenterScreen;
                    vorm.ShowInTaskbar = true;
                    vorm.ShowDialog();
                }
                uuesti = Equals(vorm.Tag, "disain");
            } while (uuesti);
            LaadiTooted();   // laoseis võis muutuda
        }
    }
}
