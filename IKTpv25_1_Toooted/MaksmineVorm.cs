using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Ostu vormistamine ühes aknas: kliendikaart + boonus, e-post tšeki jaoks, makseviis,
// saadud summa ja tagasiraha. "Maksa" salvestab ostu (laoseis väheneb) ja loob PDF-tšeki.
public class MaksmineVorm : Form
{
    // Tulemus (kui DialogResult == OK)
    public Ost? Tulemus { get; private set; }
    public string? PdfFail { get; private set; }
    public List<string> VaheseLaoga { get; } = new();

    private readonly TextBox txtKaart, txtEmail, txtSaadud, txtAadress;
    private readonly RadioButton rbPoest, rbKojuvedu;
    private readonly Label lblTarne;
    private readonly Edenemine tarneRiba = new();
    private readonly Label lblKlient, lblVahesumma, lblSoodustus, lblMaksta, lblTagasi;
    private readonly CheckBox chkBoonus;
    private readonly RadioButton rbSularaha, rbKaart;
    private readonly FlowLayoutPanel kiirNupud;
    private readonly RoundedButton btnMaksa;

    // Leitud klient (andmebaasist, ilma jälgimiseta)
    private int? klientId;
    private decimal klientBoonus;

    public MaksmineVorm()
    {
        Text = "Vormista ost";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Font = UiStiil.Põhifont;
        BackColor = Teema.Taust;
        ClientSize = new Size(620, 772);

        // ================= 1. Ostukorv =================
        var korv = Kaart("Ostukorv", 16, 16, 588, 134);
        var list = new ListBox
        {
            Location = new Point(14, 36),
            Size = new Size(560, 60),
            BorderStyle = BorderStyle.None,
            BackColor = Teema.Pind,
            ForeColor = Teema.Tekst2,
            SelectionMode = SelectionMode.None,
            IntegralHeight = false
        };
        foreach (var r in Ostukorv.Ridad) list.Items.Add(r);
        lblVahesumma = new Label { Location = new Point(14, 102), Size = new Size(560, 22), TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI Semibold", 10F), BackColor = Teema.Pind, ForeColor = Teema.Tekst };
        korv.Controls.Add(list);
        korv.Controls.Add(lblVahesumma);

        // ================= 2. Klient ja tšekk =================
        var klient = Kaart("Klient ja tšekk", 16, 162, 588, 206);
        klient.Controls.Add(Silt("Kliendikaardi number (valikuline)", 14, 36));
        var raamKaart = UiStiil.LooSisend(out txtKaart, 200, false, "nt 100001");
        raamKaart.Location = new Point(14, 56);
        klient.Controls.Add(raamKaart);
        txtKaart.TextChanged += (s, e) => OtsiKlient();

        // Kassapidaja saab uue kliendi kohe luua – kaart antakse ise
        var uusKlient = new LinkLabel { Text = "+ Uus klient (kaart kohe)", Location = new Point(400, 36), AutoSize = true, BackColor = Teema.Pind, LinkBehavior = LinkBehavior.HoverUnderline, Visible = !AktiivneKasutaja.OnOstja };
        uusKlient.LinkClicked += (s, e) => LooUusKlient();
        klient.Controls.Add(uusKlient);

        lblKlient = new Label { Location = new Point(228, 56), Size = new Size(346, 34), TextAlign = ContentAlignment.MiddleLeft, BackColor = Teema.Pind, Font = new Font("Segoe UI Semibold", 9.5F) };
        klient.Controls.Add(lblKlient);

        chkBoonus = new CheckBox { Location = new Point(16, 98), AutoSize = true, Visible = false, BackColor = Teema.Pind, ForeColor = Teema.Tekst };
        chkBoonus.CheckedChanged += (s, e) => Arvuta();
        klient.Controls.Add(chkBoonus);

        klient.Controls.Add(Silt("E-post, kuhu PDF-tšekk saata (tühi = ei saadeta)", 14, 128));
        var raamEmail = UiStiil.LooSisend(out txtEmail, 560, false, "klient@email.ee");
        raamEmail.Location = new Point(14, 148);
        klient.Controls.Add(raamEmail);

        // ================= 3. Kättesaamine =================
        var tarne = Kaart("Kättesaamine", 16, 380, 588, 118);
        rbPoest = new RadioButton { Text = "Poest kaasa (tasuta)", Location = new Point(16, 34), AutoSize = true, Checked = true, BackColor = Teema.Pind, ForeColor = Teema.Tekst, Font = new Font("Segoe UI", 10F) };
        rbKojuvedu = new RadioButton { Location = new Point(220, 34), AutoSize = true, BackColor = Teema.Pind, ForeColor = Teema.Tekst, Font = new Font("Segoe UI", 10F) };
        rbKojuvedu.Text = Keel.F("Kojuvedu {0} (tasuta alates {1})", Ostukorv.Eur(Tarne.Hind), Ostukorv.Eur(Tarne.TasutaAlates));
        rbKojuvedu.CheckedChanged += (s, e) => { txtAadress!.Enabled = rbKojuvedu.Checked; Arvuta(); };
        var raamAadress = UiStiil.LooSisend(out txtAadress, 400, false, "Tänav 1-2, Tallinn, 10111");
        raamAadress.Location = new Point(14, 66);
        txtAadress.Enabled = false;
        lblTarne = new Label { Location = new Point(424, 64), Size = new Size(152, 20), TextAlign = ContentAlignment.MiddleRight, BackColor = Teema.Pind, Font = new Font("Segoe UI Semibold", 9F) };
        tarneRiba.Location = new Point(428, 90);
        tarneRiba.Size = new Size(146, 6);
        tarne.Controls.AddRange(new Control[] { rbPoest, rbKojuvedu, raamAadress, lblTarne, tarneRiba });

        // ================= 4. Makse =================
        var makse = Kaart("Makse", 16, 510, 588, 170);
        rbSularaha = new RadioButton { Text = "Sularaha", Location = new Point(16, 36), AutoSize = true, Checked = true, BackColor = Teema.Pind, ForeColor = Teema.Tekst, Font = new Font("Segoe UI", 10F) };
        rbKaart = new RadioButton { Text = "Pangakaart", Location = new Point(130, 36), AutoSize = true, BackColor = Teema.Pind, ForeColor = Teema.Tekst, Font = new Font("Segoe UI", 10F) };
        rbSularaha.CheckedChanged += (s, e) => MakseviisMuutus();
        makse.Controls.Add(rbSularaha);
        makse.Controls.Add(rbKaart);

        makse.Controls.Add(Silt("Saadud summa (€)", 14, 70));
        var raamSaadud = UiStiil.LooSisend(out txtSaadud, 150, false, "0.00");
        raamSaadud.Location = new Point(14, 90);
        makse.Controls.Add(raamSaadud);
        txtSaadud.TextChanged += (s, e) => Arvuta();
        txtSaadud.KeyPress += (s, e) =>
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != '.')
                e.Handled = true;
        };

        kiirNupud = new FlowLayoutPanel { Location = new Point(172, 88), Size = new Size(404, 40), BackColor = Teema.Pind, WrapContents = false };
        kiirNupud.Controls.Add(UiStiil.LooNupp("Täpne", (s, e) => txtSaadud.Text = Summa().ToString("0.00", CultureInfo.InvariantCulture)));
        foreach (int raha in new[] { 5, 10, 20, 50 })
            kiirNupud.Controls.Add(UiStiil.LooNupp(raha + " €", (s, e) => txtSaadud.Text = raha.ToString()));
        makse.Controls.Add(kiirNupud);

        lblTagasi = new Label { Location = new Point(14, 132), Size = new Size(560, 26), Font = new Font("Segoe UI Semibold", 11F), BackColor = Teema.Pind };
        makse.Controls.Add(lblTagasi);

        // ================= Alumine riba =================
        var riba = new Panel { Dock = DockStyle.Bottom, Height = 84, BackColor = Teema.Paneel };
        var maksta = new Label { Text = "MAKSTA", Location = new Point(20, 12), AutoSize = true, Font = UiStiil.AlapealkiriFont, ForeColor = Teema.TekstNork, BackColor = Teema.Paneel };
        lblSoodustus = new Label { Location = new Point(130, 12), Size = new Size(200, 18), ForeColor = Teema.Edu, BackColor = Teema.Paneel };
        lblMaksta = new Label { Location = new Point(16, 30), AutoSize = true, Font = new Font("Segoe UI Semibold", 22F), ForeColor = Teema.Tekst, BackColor = Teema.Paneel };

        btnMaksa = UiStiil.LooSuurNupp("Maksa  ✓", (s, e) => Maksa());
        btnMaksa.Size = new Size(170, 48);
        btnMaksa.Location = new Point(ClientSize.Width - 186, 18);
        var tuhista = UiStiil.LooNupp("Tühista", null);
        tuhista.MinimumSize = new Size(100, 48);
        tuhista.Location = new Point(ClientSize.Width - 300, 18);
        tuhista.DialogResult = DialogResult.Cancel;

        riba.Controls.AddRange(new Control[] { maksta, lblSoodustus, lblMaksta, tuhista, btnMaksa });

        Controls.AddRange(new Control[] { korv, klient, tarne, makse, riba });
        AcceptButton = btnMaksa;
        CancelButton = tuhista;

        Arvuta();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        UiStiil.LisaTagasiNupp(this);   // ← Tagasi = tühista
        Teema.Rakenda(this);
        txtSaadud.Text = Summa().ToString("0.00", CultureInfo.InvariantCulture);

        // Ostja (registreeritud klient): kaart ja e-post registreerimisest, e-posti saab siin muuta
        if (AktiivneKasutaja.OnOstja)
        {
            AktiivneKasutaja.Uuenda();
            if (AktiivneKasutaja.TsekkEmailile) txtEmail.Text = AktiivneKasutaja.Email ?? "";
            if (AktiivneKasutaja.Kaardinumber != null) txtKaart.Text = AktiivneKasutaja.Kaardinumber;
            rbKaart.Checked = true;   // e-poes makstakse kaardiga
            rbKojuvedu.Checked = true; // e-poest tuuakse koju (aadress seadetest)
            txtAadress.Text = AktiivneKasutaja.Aadress ?? "";
            txtEmail.Focus();
        }
        else txtKaart.Focus();
    }

    // Uus klient otse kassas: nimi + e-post, kaardinumber antakse automaatselt
    private void LooUusKlient()
    {
        string? nimi = Teade.Kysi("Uus klient", "Kliendi nimi:");
        if (string.IsNullOrWhiteSpace(nimi)) return;
        string? email = Teade.Kysi("Uus klient", "Kliendi e-post (tšeki jaoks, võib jätta tühjaks):", txtEmail.Text.Trim());
        if (email == null) return;
        email = email.Trim();
        if (email.Length > 0 && !EmailSeaded.OnKorrektneAadress(email))
        {
            Teade.Show("E-posti aadress pole korrektne!", "E-post", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            using var db = new ToodedContext();
            var k = Kliendikaart.Loo(db, nimi.Trim(), email);
            db.SaveChanges();
            if (email.Length > 0) txtEmail.Text = email;
            txtKaart.Text = k.Kaardinumber;   // OtsiKlient leiab ta kohe
            Teade.Toast(Keel.F("Klient „{0}“ lisatud", k.Nimi) + "\n" + Keel.F("Kliendikaart: {0}", k.Kaardinumber));
        }
        catch (Exception ex) { Teade.Show("Viga lisamisel:\n" + ex.Message, "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static RoundedPanel Kaart(string pealkiri, int x, int y, int w, int h)
    {
        var k = new RoundedPanel { Location = new Point(x, y), Size = new Size(w, h), FillColor = Teema.Pind, BorderColor = Teema.Joon, BackColor = Teema.Pind };
        k.Controls.Add(new Label
        {
            Text = pealkiri.ToUpper(),
            Location = new Point(14, 10),
            AutoSize = true,
            Font = UiStiil.AlapealkiriFont,
            ForeColor = Teema.TekstNork,
            BackColor = Teema.Pind
        });
        return k;
    }

    private static Label Silt(string tekst, int x, int y) =>
        new() { Text = tekst, Location = new Point(x, y), AutoSize = true, ForeColor = Teema.Tekst2, BackColor = Teema.Pind };

    // ---------------------------------------------------------------
    //  Klient
    // ---------------------------------------------------------------
    private void OtsiKlient()
    {
        string kaart = txtKaart.Text.Trim();
        klientId = null;
        klientBoonus = 0;
        chkBoonus.Visible = false;
        chkBoonus.Checked = false;

        if (kaart.Length == 0)
        {
            lblKlient.Text = "";
        }
        else
        {
            try
            {
                using var db = new ToodedContext();
                var k = db.Kliendid.AsNoTracking().FirstOrDefault(x => x.Kaardinumber == kaart);
                if (k == null)
                {
                    lblKlient.Text = Keel.T("Kaarti ei leitud");
                    lblKlient.ForeColor = Teema.Viga;
                }
                else
                {
                    klientId = k.Id;
                    klientBoonus = k.Boonus;
                    lblKlient.Text = Keel.F("✓ {0}   ·   boonus {1}", k.Nimi, Ostukorv.Eur(k.Boonus));
                    lblKlient.ForeColor = Teema.Edu;
                    if (txtEmail.Text.Trim().Length == 0 && !string.IsNullOrEmpty(k.Email))
                        txtEmail.Text = k.Email;
                    if (k.Boonus > 0)
                    {
                        chkBoonus.Text = Keel.F("Kasuta boonust allahindluseks (−{0})", Ostukorv.Eur(Math.Min(k.Boonus, Ostukorv.Kokku)));
                        chkBoonus.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                lblKlient.Text = "Viga: " + ex.Message;
                lblKlient.ForeColor = Teema.Viga;
            }
        }
        Arvuta();
    }

    // ---------------------------------------------------------------
    //  Summad
    // ---------------------------------------------------------------
    private decimal Soodustus() =>
        chkBoonus.Checked && klientId != null ? Math.Min(klientBoonus, Ostukorv.Kokku) : 0;

    private decimal TarneHind() => rbKojuvedu != null && rbKojuvedu.Checked ? Tarne.Arvuta(Ostukorv.Kokku) : 0;

    private decimal Summa() => Ostukorv.Kokku - Soodustus() + TarneHind();

    private bool LoeSaadud(out decimal saadud) =>
        decimal.TryParse(txtSaadud.Text.Trim().Replace(',', '.'), NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out saadud);

    private void MakseviisMuutus()
    {
        bool sularaha = rbSularaha.Checked;
        txtSaadud.Enabled = sularaha;
        kiirNupud.Enabled = sularaha;
        if (!sularaha) txtSaadud.Text = Summa().ToString("0.00", CultureInfo.InvariantCulture);
        Arvuta();
    }

    private void Arvuta()
    {
        if (lblMaksta == null) return;   // konstruktor pole veel lõpus

        decimal soodustus = Soodustus();
        decimal summa = Summa();
        lblVahesumma.Text = Keel.F("Vahesumma: {0}", Ostukorv.Eur(Ostukorv.Kokku)) +
            (rbKojuvedu.Checked ? "   ·   " + Keel.F("Tarne: {0}", TarneHind() == 0 ? Keel.T("TASUTA") : Ostukorv.Eur(TarneHind())) : "");

        // Kui palju on veel puudu tasuta tarneni
        decimal puudu = Tarne.Puudu(Ostukorv.Kokku);
        lblTarne.Text = puudu > 0 ? Keel.F("veel {0} → tasuta", Ostukorv.Eur(puudu)) : Keel.T("✓ Tarne tasuta");
        lblTarne.ForeColor = puudu > 0 ? Teema.Tekst2 : Teema.Edu;
        tarneRiba.Osa = (float)(Ostukorv.Kokku / Tarne.TasutaAlates);
        lblSoodustus.Text = soodustus > 0 ? Keel.F("boonus −{0}", Ostukorv.Eur(soodustus)) : "";
        lblMaksta.Text = Ostukorv.Eur(summa);

        if (rbKaart.Checked)
        {
            txtSaadud.Text = summa.ToString("0.00", CultureInfo.InvariantCulture);
            lblTagasi.Text = Keel.T("Makse pangakaardiga");
            lblTagasi.ForeColor = Teema.Tekst2;
            return;
        }

        if (!LoeSaadud(out decimal saadud))
        {
            lblTagasi.Text = Keel.T("Sisesta saadud summa");
            lblTagasi.ForeColor = Teema.TekstNork;
        }
        else if (saadud < summa)
        {
            lblTagasi.Text = Keel.F("Puudu: {0}", Ostukorv.Eur(summa - saadud));
            lblTagasi.ForeColor = Teema.Viga;
        }
        else
        {
            lblTagasi.Text = Keel.F("Tagasiraha: {0}", Ostukorv.Eur(saadud - summa));
            lblTagasi.ForeColor = Teema.Edu;
        }
    }

    // ---------------------------------------------------------------
    //  Maksmine: ost + makse + PDF
    // ---------------------------------------------------------------
    private void Maksa()
    {
        if (Ostukorv.Ridad.Count == 0) { Teade.Show("Ostukorv on tühi."); return; }

        if (txtKaart.Text.Trim().Length > 0 && klientId == null)
        {
            Teade.Show("Sellise numbriga kliendikaarti ei leitud.\nTühjenda väli, kui klient kaarti ei kasuta.", "Kliendikaart",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtKaart.Focus();
            return;
        }

        string email = txtEmail.Text.Trim();
        if (email.Length > 0 && !EmailSeaded.OnKorrektneAadress(email))
        {
            Teade.Show("E-posti aadress pole korrektne.", "E-post", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtEmail.Focus();
            return;
        }

        string aadress = txtAadress.Text.Trim();
        if (rbKojuvedu.Checked && aadress.Length < 5)
        {
            Teade.Show("Sisesta kojuveo aadress.", "Kättesaamine", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtAadress.Focus();
            return;
        }

        decimal soodustus = Soodustus();
        decimal tarneHind = TarneHind();
        decimal summa = Summa();
        decimal saadud = summa;
        if (rbSularaha.Checked)
        {
            if (!LoeSaadud(out saadud) || saadud < summa)
            {
                Teade.Show(Keel.F("Saadud summa peab olema vähemalt {0}.", Ostukorv.Eur(summa)), "Makse",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSaadud.Focus();
                txtSaadud.SelectAll();
                return;
            }
        }

        btnMaksa.Enabled = false;
        try
        {
            using var db = new ToodedContext();
            Klient? klient = klientId is int id ? db.Kliendid.Find(id) : null;

            var ost = new Ost
            {
                Aeg = DateTime.Now,
                Vahesumma = Ostukorv.Kokku,
                Soodustus = soodustus,
                Summa = summa,
                Kasutaja = AktiivneKasutaja.OnOstja ? "e-pood" : AktiivneKasutaja.Nimi,
                Email = email.Length > 0 ? email : null,
                Makstud = true,
                MakstudSumma = saadud,
                Makseviis = rbKaart.Checked ? "Kaart" : "Sularaha",
                Tarneviis = rbKojuvedu.Checked ? Tarne.Kojuvedu : Tarne.Kaasa,
                TarneHind = tarneHind,
                Aadress = rbKojuvedu.Checked ? aadress : null,
                Klient = klient
            };

            // Laoseisu vähendamine. SaveChanges() salvestab kõik ühe tehinguna:
            // kui mõnda toodet pole piisavalt, ei muudeta midagi.
            foreach (KorvRida r in Ostukorv.Ridad)
            {
                Toode? toode = db.Toodetabel.Find(r.Id);
                if (toode == null || toode.Kogus < r.Kogus)
                {
                    Teade.Show(Keel.F("Toodet „{0}“ pole piisavalt laos (laos: {1}).", r.Nimi, toode?.Kogus ?? 0) + "\n" + Keel.T("Vähenda kogust korvis."),
                        "Laoseis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnMaksa.Enabled = true;
                    return;
                }
                toode.Kogus -= r.Kogus;
                if (toode.Kogus < Form1.VaikeLaoseis) VaheseLaoga.Add($"{toode.Toodenimetus}: {toode.Kogus} tk");

                ost.Read.Add(new OstuRida { ToodeId = r.Id, Toodenimetus = r.Nimi, Kogus = r.Kogus, Hind = r.Hind });
            }

            if (klient != null)
            {
                decimal protsent = Preemiad.LeiaTase(db, klient.Id).Protsent;   // Pronks 1 %, Hõbe 1,5 %, Kuld 2 %
                ost.BoonusLisatud = Tarne.Boonus(ost.Summa - ost.TarneHind, protsent);
                klient.Boonus = klient.Boonus - soodustus + ost.BoonusLisatud;
                if (string.IsNullOrEmpty(klient.Email) && ost.Email != null) klient.Email = ost.Email;   // jätame meelde
            }

            // Ostja aadress jääb meelde (järgmisel korral täidetakse ise)
            if (rbKojuvedu.Checked && AktiivneKasutaja.Id > 0 &&
                db.Kasutajad.Find(AktiivneKasutaja.Id) is Kasutaja kasutaja && string.IsNullOrWhiteSpace(kasutaja.Aadress))
            {
                kasutaja.Aadress = aadress;
                AktiivneKasutaja.Aadress = aadress;
            }

            db.Ostud.Add(ost);
            db.SaveChanges();
            Tulemus = ost;

            try { PdfFail = Arve.LooPdf(ost, true, saadud); }
            catch (Exception ex)
            {
                Teade.Show("Ost on salvestatud, kuid PDF-tšeki loomine ebaõnnestus:\n" + ex.Message, "PDF",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            btnMaksa.Enabled = true;
            Teade.Show("Viga ostu salvestamisel:\n" + ex.Message, "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
