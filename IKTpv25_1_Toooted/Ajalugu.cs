using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    // Üks rida ostude ajaloo tabelis
    public class OstuVaade
    {
        public int Nr { get; set; }
        public DateTime Aeg { get; set; }
        public string Müüja { get; set; } = "";
        public string Klient { get; set; } = "";
        public decimal Summa { get; set; }
        public decimal Soodustus { get; set; }
        public string Staatus { get; set; } = "";
    }

    // ===================================================================
    //  Ostude ajalugu: tšeki korduv printimine, tagastamine (ainult Omanik), CSV
    // ===================================================================
    public partial class OstudVorm : KujundusVorm
    {
        private readonly bool lubaTagastus;
        private readonly DataGridView ostud, read;
        private readonly CheckBox naitaTagastatud;

        // Ostud, mis selles aknas tagastati (Kassa peab neist teadma)
        public List<int> TagastatudOstud { get; } = new();

        public OstudVorm(bool lubaTagastus) : base("Ostude ajalugu", 820, 600)
        {
            this.lubaTagastus = lubaTagastus;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimumSize = new Size(700, 500);

            Pealkiri("Ostude ajalugu");

            naitaTagastatud = new CheckBox { Text = "Näita ka tagastatud", AutoSize = true, Location = new Point(620, 22), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            naitaTagastatud.CheckedChanged += (s, e) => Laadi();
            Controls.Add(naitaTagastatud);

            ostud = Tabel(new Rectangle(20, 56, 780, 260));
            ostud.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ostud.SelectionChanged += (s, e) => NaitaRead();
            ostud.CellFormatting += Ostud_CellFormatting;

            var readSilt = Silt("Ostu read", 20, 326);
            read = Tabel(new Rectangle(20, 346, 780, 180));
            read.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            var nupud = new FlowLayoutPanel
            {
                Location = new Point(16, 540),
                Size = new Size(790, 44),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.White
            };
            nupud.Controls.Add(UiStiil.LooNupp("Osta uuesti", (s, e) => OstaUuesti(), true));
            nupud.Controls.Add(UiStiil.LooNupp("Tšekk (PDF)", (s, e) => PrindiTsekk()));
            var tagasta = UiStiil.LooNupp("Tagasta ost", (s, e) => Tagasta());
            tagasta.Visible = lubaTagastus;
            nupud.Controls.Add(tagasta);
            nupud.Controls.Add(UiStiil.LooNupp("Ekspordi müük (CSV)", (s, e) => CsvEksport.Myyk(this)));
            var sulge = UiStiil.LooNupp("Sulge", null);
            sulge.DialogResult = DialogResult.Cancel;
            nupud.Controls.Add(sulge);
            CancelButton = sulge;
            Controls.Add(nupud);
            if (Teema.OnMoodne) RakendaMoodne(nupud, readSilt);   // lõuend 3.1: tabel + tšekk paberil

            Load += (s, e) => Laadi();
        }

        private DataGridView Tabel(Rectangle r)
        {
            var vork = new DataGridView
            {
                Bounds = r,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                MultiSelect = false,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            Controls.Add(vork);
            return vork;
        }

        private void Laadi()
        {
            using var db = new ToodedContext();
            bool koik = naitaTagastatud.Checked;

            ostud.DataSource = db.Ostud
                .Where(o => koik || !o.Tagastatud)
                .OrderByDescending(o => o.Aeg)
                .Select(o => new OstuVaade
                {
                    Nr = o.Id,
                    Aeg = o.Aeg,
                    Müüja = o.Kasutaja,
                    Klient = o.Klient != null ? o.Klient.Nimi + " (" + o.Klient.Kaardinumber + ")" : "",
                    Summa = o.Summa,
                    Soodustus = o.Soodustus,
                    Staatus = o.Tagastatud ? "Tagastatud" : o.Makstud ? "Makstud" : "Maksmata"   // tõlgitakse kuvamisel
                })
                .ToList();

            if (ostud.Columns["Aeg"] is DataGridViewColumn aeg) aeg.DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
            if (ostud.Columns["Summa"] is DataGridViewColumn summa) summa.DefaultCellStyle.Format = "0.00 €";
            if (ostud.Columns["Soodustus"] is DataGridViewColumn sood) sood.DefaultCellStyle.Format = "0.00 €";
            if (ostud.Columns["Nr"] is DataGridViewColumn nr) nr.FillWeight = 40;

            NaitaRead();
        }

        private void Ostud_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.CellStyle == null) return;
            if (ostud.Rows[e.RowIndex].DataBoundItem is OstuVaade o)
            {
                if (o.Staatus == "Tagastatud") e.CellStyle.ForeColor = Color.Gray;
                else if (o.Staatus == "Maksmata") e.CellStyle.ForeColor = Color.FromArgb(180, 60, 0);
                if (ostud.Columns[e.ColumnIndex].DataPropertyName == "Staatus" && e.Value is string st)
                {
                    e.Value = Keel.T(st);
                    e.FormattingApplied = true;
                }
            }
        }

        private int? ValitudNr() =>
            ostud.CurrentRow?.DataBoundItem is OstuVaade o ? o.Nr : null;

        private void NaitaRead()
        {
            if (ValitudNr() is not int nr)
            {
                read.DataSource = null;
                return;
            }

            using var db = new ToodedContext();
            read.DataSource = db.OstuRead
                .Where(r => r.OstId == nr)
                .Select(r => new { Toode = r.Toodenimetus, r.Kogus, r.Hind })
                .ToList()
                .Select(r => new { r.Toode, r.Kogus, r.Hind, Summa = r.Hind * r.Kogus })
                .ToList();

            if (read.Columns["Hind"] is DataGridViewColumn h) h.DefaultCellStyle.Format = "0.00 €";
            if (read.Columns["Summa"] is DataGridViewColumn s) s.DefaultCellStyle.Format = "0.00 €";
        }

        private static Ost? LaeOst(ToodedContext db, int nr) =>
            db.Ostud.Include(o => o.Read).Include(o => o.Klient).FirstOrDefault(o => o.Id == nr);

        // Kõik valitud tšeki tooted uuesti ostukorvi (praeguse hinnaga, nii palju kui laos on)
        private void OstaUuesti()
        {
            if (ValitudNr() is not int nr) { Teade.Show("Vali tabelist ost."); return; }
            using var db = new ToodedContext();
            var read = db.OstuRead.AsNoTracking().Where(r => r.OstId == nr).ToList();
            var ids = read.Where(r => r.ToodeId != null).Select(r => r.ToodeId!.Value).ToList();
            var tooted = db.Toodetabel.AsNoTracking().Include(t => t.Kategooria).Where(t => ids.Contains(t.Id)).ToDictionary(t => t.Id);

            int lisatud = 0;
            var puudu = new List<string>();
            foreach (var r in read)
            {
                if (r.ToodeId is not int id || !tooted.TryGetValue(id, out var t)) { puudu.Add(r.Toodenimetus); continue; }
                int vaba = t.Kogus - Ostukorv.KorvisKogus(id);
                int kogus = Math.Min(r.Kogus, vaba);
                if (kogus <= 0 || Ostukorv.LisaToode(t, kogus) != null) { puudu.Add(r.Toodenimetus); continue; }
                lisatud += kogus;
                if (kogus < r.Kogus) puudu.Add($"{r.Toodenimetus} ({r.Kogus - kogus})");
            }

            if (lisatud == 0)
                Teade.Show(Keel.T("Ühtegi toodet ei saanud lisada – kõik on otsas või müügilt maas."), Keel.T("Osta uuesti"));
            else if (puudu.Count == 0)
                Teade.Toast(Keel.F("Ostukorvi lisatud {0} tk – sama, mis tšekil nr {1}", lisatud, nr));
            else
                Teade.Show(Keel.F("Ostukorvi lisatud {0} tk.", lisatud) + "\n\n" + Keel.T("Puudu (otsas või müügilt maas):") + "\n• " + string.Join("\n• ", puudu),
                    Keel.T("Osta uuesti"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PrindiTsekk()
        {
            if (ValitudNr() is not int nr) { Teade.Show("Vali ost."); return; }
            try
            {
                using var db = new ToodedContext();
                var ost = LaeOst(db, nr);
                if (ost == null) return;
                Arve.Ava(Arve.LooPdf(ost, ost.Makstud, ost.MakstudSumma));
            }
            catch (Exception ex) { Teade.Show("Tšeki loomine ebaõnnestus:\n" + ex.Message); }
        }

        // Tagastamine: kaup läheb tagasi lattu, kliendi boonus taastatakse
        private void Tagasta()
        {
            if (!lubaTagastus) return;
            if (ValitudNr() is not int nr) { Teade.Show("Vali ost."); return; }

            try
            {
                using var db = new ToodedContext();
                var ost = LaeOst(db, nr);
                if (ost == null) return;
                if (ost.Tagastatud) { Teade.Show("See ost on juba tagastatud."); return; }

                if (Teade.Show(Keel.F("Kas tagastan ostu nr {0} ({1})?", ost.Id, Ostukorv.Eur(ost.Summa)) + "\n" + Keel.T("Kaup läheb tagasi lattu."),
                        "Tagastus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                foreach (var rida in ost.Read)
                {
                    if (rida.ToodeId is int toodeId && db.Toodetabel.Find(toodeId) is Toode toode)
                        toode.Kogus += rida.Kogus;
                }

                if (ost.Klient != null)
                    ost.Klient.Boonus = Math.Max(0, ost.Klient.Boonus - ost.BoonusLisatud + ost.Soodustus);

                ost.Tagastatud = true;
                db.SaveChanges();

                TagastatudOstud.Add(ost.Id);
                Teade.Show(Keel.F("Ost nr {0} tagastatud. Kaup on tagasi laos.", ost.Id) +
                                (ost.Makstud ? "\n" + Keel.F("Tagasta kliendile {0}.", Ostukorv.Eur(ost.Summa)) : ""));
                Laadi();
            }
            catch (Exception ex) { Teade.Show("Tagastamine ebaõnnestus:\n" + ex.Message); }
        }
    }

    // ===================================================================
    //  Parooli muutmine (sisseloginud kasutaja)
    // ===================================================================
    public class ParooliMuutmineVorm : KujundusVorm
    {
        private readonly TextBox txtVana, txtUus, txtKorda;
        private readonly Label lblViga;

        public ParooliMuutmineVorm() : base("Muuda parooli", 340, 330)
        {
            Pealkiri(Keel.F("Muuda parooli – {0}", AktiivneKasutaja.Nimi));
            Silt("Praegune parool", 20, 64);
            txtVana = Sisend(20, 84, 300, true);
            Silt("Uus parool (vähemalt 4 märki)", 20, 120);
            txtUus = Sisend(20, 140, 300, true);
            Silt("Korda uut parooli", 20, 176);
            txtKorda = Sisend(20, 196, 300, true);
            lblViga = Veateade(20, 228, 300);

            var salvesta = Nupp("Salvesta", 20, 275, true);
            salvesta.Click += (s, e) => Salvesta();
            AcceptButton = salvesta;
            var tuhista = Nupp("Tühista", 120, 275);
            tuhista.DialogResult = DialogResult.Cancel;
            CancelButton = tuhista;
        }

        private void Salvesta()
        {
            if (txtUus.Text.Length < 4) { lblViga.Text = "Uus parool peab olema vähemalt 4 märki."; return; }
            if (txtUus.Text != txtKorda.Text) { lblViga.Text = "Uued paroolid ei kattu."; return; }

            try
            {
                using var db = new ToodedContext();
                var kasutaja = db.Kasutajad.FirstOrDefault(k => k.Kasutajanimi == AktiivneKasutaja.Nimi);
                if (kasutaja == null || !PasswordHelper.Verify(txtVana.Text, kasutaja.ParoolHash))
                {
                    lblViga.Text = "Praegune parool on vale.";
                    txtVana.Clear();
                    return;
                }

                kasutaja.ParoolHash = PasswordHelper.Hash(txtUus.Text);
                db.SaveChanges();
                Teade.Toast("Parool muudetud");
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { lblViga.Text = "Viga: " + ex.Message; }
        }
    }

    // ===================================================================
    //  CSV eksport (avaneb Excelis; eraldaja ; ja koma kümnendmurrus)
    // ===================================================================
    public static class CsvEksport
    {
        private static readonly CultureInfo Eesti = new("et-EE");

        public static void Tooted(IWin32Window omanik)
        {
            using var db = new ToodedContext();
            var tooted = db.Toodetabel
                .OrderBy(t => t.Id)
                .Select(t => new { t.Id, t.Toodenimetus, Kategooria = t.Kategooria.Kategooria_nimetus, t.Kogus, t.Hind })
                .ToList();

            Salvesta(omanik, $"Tooted_{DateTime.Now:yyyyMMdd}.csv",
                new[] { "Id", "Toode", "Kategooria", "Kogus", "Hind (€)", "Laos kokku (€)" },
                tooted.Select(t => new[]
                {
                    t.Id.ToString(), t.Toodenimetus, t.Kategooria, t.Kogus.ToString(),
                    t.Hind.ToString("0.00", Eesti), (t.Hind * t.Kogus).ToString("0.00", Eesti)
                }));
        }

        public static void Myyk(IWin32Window omanik)
        {
            using var db = new ToodedContext();
            var read = db.OstuRead
                .Select(r => new
                {
                    r.Ost.Id, r.Ost.Aeg, r.Ost.Kasutaja,
                    Klient = r.Ost.Klient != null ? r.Ost.Klient.Nimi : "",
                    r.Ost.Tagastatud, r.Toodenimetus, r.Kogus, r.Hind
                })
                .ToList()
                .OrderBy(r => r.Aeg);

            Salvesta(omanik, $"Myyk_{DateTime.Now:yyyyMMdd}.csv",
                new[] { "Ost nr", "Aeg", "Müüja", "Klient", "Toode", "Kogus", "Hind (€)", "Summa (€)", "Tagastatud" },
                read.Select(r => new[]
                {
                    r.Id.ToString(), r.Aeg.ToString("dd.MM.yyyy HH:mm"), r.Kasutaja, r.Klient, r.Toodenimetus,
                    r.Kogus.ToString(), r.Hind.ToString("0.00", Eesti), (r.Hind * r.Kogus).ToString("0.00", Eesti),
                    r.Tagastatud ? "jah" : "ei"
                }));
        }

        private static void Salvesta(IWin32Window omanik, string failinimi, string[] pais, IEnumerable<string[]> read)
        {
            using var dlg = new SaveFileDialog
            {
                FileName = failinimi,
                Filter = "CSV (Excel)|*.csv",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (dlg.ShowDialog(omanik) != DialogResult.OK) return;

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(";", pais.Select(Kaitse)));
            foreach (var rida in read)
                sb.AppendLine(string.Join(";", rida.Select(Kaitse)));

            try
            {
                // BOM, et Excel loeks täpitähed õigesti
                File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
                if (Teade.Show("Fail salvestatud:\n" + dlg.FileName + "\n\nKas avan?", "Eksport",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    Arve.Ava(dlg.FileName);
            }
            catch (Exception ex) { Teade.Show("Salvestamine ebaõnnestus:\n" + ex.Message); }
        }

        private static string Kaitse(string v) =>
            v.Contains(';') || v.Contains('"') || v.Contains('\n') ? "\"" + v.Replace("\"", "\"\"") + "\"" : v;
    }
}

namespace IKTpv25_1_Toooted
{
    // ===================================================================
    //  E-posti seaded: Gmaili aadress + rakenduse parool (ainult Omanik)
    // ===================================================================
    public class EmailSeadedVorm : KujundusVorm
    {
        private readonly TextBox txtSaatja, txtParool, txtTest;
        private readonly Label lblOlek;
        private readonly EmailSeaded seaded = EmailSeaded.Lae();

        public EmailSeadedVorm() : base("E-posti seaded", 420, 470)
        {
            KysiSulgemisel = true;
            Pealkiri("E-posti seaded");

            var juhis = Silt(Keel.TolgiTekst("Gmail ei luba saata tavalise parooliga. Vaja on RAKENDUSE parooli:\n" +
                             "1) Google'i kontol sisse lülitada kaheastmeline kinnitamine;\n" +
                             "2) myaccount.google.com → Turvalisus → Rakenduse paroolid;\n" +
                             "3) loo uus ja kopeeri 16-märgiline parool siia."), 20, 56);
            juhis.MaximumSize = new Size(380, 0);

            Silt("Saatja Gmail", 20, 140);
            txtSaatja = Sisend(20, 160, 380);
            txtSaatja.Text = seaded.Saatja;
            txtSaatja.PlaceholderText = "pood@gmail.com";

            Silt("Rakenduse parool (16 märki)", 20, 196);
            txtParool = Sisend(20, 216, 380, true);
            txtParool.PlaceholderText = seaded.OnSeadistatud ? "(salvestatud – jäta tühjaks, kui ei muuda)" : "xxxx xxxx xxxx xxxx";

            lblOlek = Silt(seaded.OnSeadistatud ? "✓ Seaded on salvestatud." : "Seaded puuduvad – tšekke ei saadeta automaatselt.", 20, 252);
            lblOlek.ForeColor = seaded.OnSeadistatud ? Color.DarkGreen : Color.FromArgb(200, 30, 30);

            var salvesta = Nupp("Salvesta", 20, 285, true);
            salvesta.Click += (s, e) => Salvesta();

            Silt("Testkiri (kontrolli, kas saatmine töötab)", 20, 340);
            txtTest = Sisend(20, 360, 270);
            txtTest.PlaceholderText = "sinu@email.ee";
            Nupp("Saada testkiri", 296, 355).Click += (s, e) => SaadaTest();

            Silt("Parool hoitakse krüpteeritult failis AppData\\Local\\ToodedApp\\email.json,\nmitte koodis – GitHubi see ei satu.", 20, 405)
                .ForeColor = Color.Gray;

            var sulge = Nupp("Sulge", 330, 285);
            sulge.DialogResult = DialogResult.Cancel;
            CancelButton = sulge;
        }

        private bool Salvesta()
        {
            string saatja = txtSaatja.Text.Trim();
            if (!EmailSeaded.OnKorrektneAadress(saatja))
            {
                Teade.Show("Sisesta korrektne Gmaili aadress.");
                return false;
            }
            if (txtParool.Text.Length == 0 && !seaded.OnSeadistatud)
            {
                Teade.Show("Sisesta rakenduse parool.");
                return false;
            }

            seaded.Saatja = saatja;
            if (txtParool.Text.Length > 0) seaded.SeaParool(txtParool.Text.Trim());
            seaded.Salvesta();
            Muudetud = false;

            txtParool.Clear();
            txtParool.PlaceholderText = "(salvestatud – jäta tühjaks, kui ei muuda)";
            lblOlek.Text = "✓ Seaded on salvestatud.";
            lblOlek.ForeColor = Color.DarkGreen;
            return true;
        }

        private void SaadaTest()
        {
            if (txtSaatja.Text.Trim() != seaded.Saatja || txtParool.Text.Length > 0)
                if (!Salvesta()) return;

            string saaja = txtTest.Text.Trim();
            if (!EmailSeaded.OnKorrektneAadress(saaja)) { Teade.Show("Sisesta korrektne testkirja aadress."); return; }

            Cursor = Cursors.WaitCursor;
            try
            {
                seaded.Saada(saaja, "Testkiri – " + Arve.PoeNimi,
                    "See on testkiri. Kui sa seda loed, töötab tšekkide saatmine.");
                Cursor = Cursors.Default;
                Teade.Show(Keel.F("Testkiri saadetud aadressile {0}", saaja));
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                Teade.Show("Saatmine ebaõnnestus:\n" + ex.Message +
                                "\n\nKontrolli, et kasutad RAKENDUSE parooli (mitte tavalist) ja kaheastmeline kinnitamine on sees.");
            }
        }
    }
}
