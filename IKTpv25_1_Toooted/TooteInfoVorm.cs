using System.Globalization;

namespace IKTpv25_1_Toooted;

// Toote lisaandmed, mida näidatakse poes toote lehel ("Toote andmed")
public class TooteInfoVorm : KujundusVorm
{
    private readonly int id;
    private readonly TextBox txtKaubamark, txtTootja, txtParitolu, txtPakend, txtUhikuKogus, txtVanaHind, txtOstuhind, txtPant;
    private readonly ComboBox cmbUhik;
    private readonly TextBox txtKirjeldus, txtKoostis, txtLisainfo, txtToitevaartus;
    private readonly List<(CheckBox Kast, DieediMark Mark)> margid = new();

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public TooteInfoVorm(int tooteId) : base("Tooteinfo", 720, 780)
    {
        KysiSulgemisel = true;
        id = tooteId;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(720, 680);
        var pealkiri = Pealkiri("Tooteinfo");

        // Lühikesed väljad kahes veerus
        Silt("Kaubamärk", 20, 60);       txtKaubamark = Sisend(20, 80, 320);
        Silt("Tootja", 370, 60);         txtTootja = Sisend(370, 80, 320);
        Silt("Päritolumaa", 20, 118);    txtParitolu = Sisend(20, 138, 320);
        Silt("Kogus pakendis (nt 0.5 kg, 2 l, 10 tk)", 370, 118); txtPakend = Sisend(370, 138, 320);

        Silt("Ühik (ühikuhinna jaoks)", 20, 176);
        cmbUhik = new ComboBox { Location = new Point(20, 194), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
        cmbUhik.Items.AddRange(new object[] { "tk", "kg", "l" });
        Controls.Add(cmbUhik);
        Silt("Ühikuid pakendis (nt 0.5)", 190, 176); txtUhikuKogus = Sisend(190, 196, 150);
        Silt("Vana hind € (soodustus, tühi = pole)", 370, 176); txtVanaHind = Sisend(370, 196, 320);

        txtKirjeldus = Mitmerealine("Toote kirjeldus", 236, 54);
        txtKoostis = Mitmerealine("Koostisosad", 318, 54);
        txtLisainfo = Mitmerealine("Lisainfo (iga info eraldi real)", 400, 54);
        txtToitevaartus = Mitmerealine("Toitumisalane teave – iga rida „nimetus;väärtus“, alamrida algab „-“-ga (nt „-millest suhkrud;2.6 g“)", 482, 100);

        // Dieedimärgised: admini märge on poe dieedifiltris tähtsam kui koostise analüüs
        Silt("Märgised", 20, 616);
        int mx = 100;
        foreach (var (nimi, mark) in new[] { ("Laktoosivaba", DieediMark.Laktoosivaba), ("Gluteenivaba", DieediMark.Gluteenivaba), ("Vegan", DieediMark.Vegan), ("Pähklivaba", DieediMark.Pahklivaba) })
        {
            var ch = new CheckBox { Text = nimi, Location = new Point(mx, 614), AutoSize = true, ForeColor = Teema.Tekst2 };
            Controls.Add(ch);
            margid.Add((ch, mark));
            mx += 130;
        }

        // Müügipaneeli marginaal ja pakendi tagatisraha
        Silt("Sisseostuhind € (marginaali jaoks)", 20, 650); txtOstuhind = Sisend(20, 670, 320);
        Silt("Pakendi tagatisraha € (pudel/purk 0.10)", 370, 650); txtPant = Sisend(370, 670, 320);

        var salvesta = Nupp("Salvesta", 20, 730, true);
        salvesta.Click += (s, e) => Salvesta();
        AcceptButton = null;   // Enter on mitmerealistes väljades reavahetus
        var tuhista = Nupp("Tühista", 120, 730);
        tuhista.DialogResult = DialogResult.Cancel;
        CancelButton = tuhista;

        Load += (s, e) => Laadi();
    }

    private TextBox Mitmerealine(string silt, int y, int korgus)
    {
        Silt(silt, 20, y);
        var raam = new RoundedPanel
        {
            Location = new Point(20, y + 20),
            Size = new Size(670, korgus),
            Radius = 8,
            Padding = new Padding(8, 6, 4, 4),
            BorderColor = Teema.Raam,
            FillColor = Teema.Pind,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        var t = new TextBox
        {
            Multiline = true,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical,
            AcceptsReturn = true,
            BackColor = Teema.Pind,
            ForeColor = Teema.Tekst,
            Font = new Font("Segoe UI", 9.5F)
        };
        raam.Controls.Add(t);
        UiStiil.LisaFookus(raam, t);
        Controls.Add(raam);
        return t;
    }

    private void Laadi()
    {
        using var db = new ToodedContext();
        var t = db.Toodetabel.Find(id);
        if (t == null) { Close(); return; }

        Text = "Tooteinfo – " + t.Toodenimetus;
        txtKaubamark.Text = t.Kaubamark ?? "";
        txtTootja.Text = t.Tootja ?? "";
        txtParitolu.Text = t.Paritolumaa ?? "";
        txtPakend.Text = t.Pakend ?? "";
        cmbUhik.SelectedItem = t.Uhik ?? "tk";
        if (cmbUhik.SelectedIndex < 0) cmbUhik.SelectedIndex = 0;
        txtUhikuKogus.Text = t.UhikuKogus?.ToString("0.###", Inv) ?? "";
        txtVanaHind.Text = t.VanaHind?.ToString("0.00", Inv) ?? "";
        txtKirjeldus.Text = Read(t.Kirjeldus);
        txtKoostis.Text = Read(t.Koostisosad);
        txtLisainfo.Text = Read(t.Lisainfo);
        txtToitevaartus.Text = Read(t.Toitevaartus);
        txtOstuhind.Text = t.Ostuhind?.ToString("0.00", Inv) ?? "";
        txtPant.Text = t.Pant > 0 ? t.Pant.ToString("0.00", Inv) : "";
        var m = Dieet.LoeMargid(t.Margised);
        foreach (var (kast, mark) in margid) kast.Checked = m.HasFlag(mark);
    }

    // TextBox vajab reavahetuseks \r\n, andmebaasis hoiame \n
    private static string Read(string? s) => (s ?? "").Replace("\r\n", "\n").Replace("\n", "\r\n");
    private static string? Puhas(string s)
    {
        s = s.Replace("\r\n", "\n").Trim();
        return s.Length == 0 ? null : s;
    }

    private static bool LoeArv(string tekst, out decimal? arv)
    {
        arv = null;
        tekst = tekst.Trim().Replace(',', '.');
        if (tekst.Length == 0) return true;
        if (decimal.TryParse(tekst, NumberStyles.AllowDecimalPoint, Inv, out decimal d) && d >= 0) { arv = d; return true; }
        return false;
    }

    private void Salvesta()
    {
        if (!LoeArv(txtUhikuKogus.Text, out decimal? uhikuKogus))
        { Teade.Show("Ühikuid pakendis peab olema arv (nt 0.5)."); return; }
        if (!LoeArv(txtVanaHind.Text, out decimal? vanaHind))
        { Teade.Show("Vana hind peab olema arv (nt 6.19) või tühi."); return; }
        if (!LoeArv(txtOstuhind.Text, out decimal? ostuhind) || !LoeArv(txtPant.Text, out decimal? pant))
        { Teade.Show("Sisseostuhind ja tagatisraha peavad olema arvud (nt 0.10) või tühjad."); return; }

        try
        {
            using var db = new ToodedContext();
            var t = db.Toodetabel.Find(id);
            if (t == null) return;

            t.Kaubamark = Puhas(txtKaubamark.Text);
            t.Tootja = Puhas(txtTootja.Text);
            t.Paritolumaa = Puhas(txtParitolu.Text);
            t.Pakend = Puhas(txtPakend.Text);
            t.Uhik = cmbUhik.SelectedItem?.ToString();
            t.UhikuKogus = uhikuKogus;
            t.VanaHind = vanaHind is decimal v && v > t.Hind ? Math.Round(v, 2) : null;
            t.Kirjeldus = Puhas(txtKirjeldus.Text);
            t.Koostisosad = Puhas(txtKoostis.Text);
            t.Lisainfo = Puhas(txtLisainfo.Text);
            t.Toitevaartus = Puhas(txtToitevaartus.Text);
            t.Ostuhind = ostuhind is decimal oh ? Math.Round(oh, 2) : null;
            t.Pant = pant is decimal pa ? Math.Round(pa, 2) : 0;
            var m = margid.Where(x => x.Kast.Checked).Aggregate(DieediMark.Puudub, (a, x) => a | x.Mark);
            t.Margised = m == DieediMark.Puudub ? null : Dieet.KirjutaMargid(m);
            db.SaveChanges();

            DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            Teade.Show("Salvestamine ebaõnnestus:\n" + ex.Message, "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
