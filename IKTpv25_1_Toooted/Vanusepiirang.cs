using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Vanusepiirang: alkohol 18+, energiajoogid 16+.
//  Maksmisel küsitakse ostja sünnikuupäeva (e-poes kinnitab ostja, kuller kontrollib dokumenti;
//  kassas kontrollib müüja dokumenti). Alaealisele piiranguga tooteid ei müüda.
// ===================================================================
public static class Vanusepiirang
{
    public const int Alkohol = 18, Energiajook = 16;

    // Vanusepiir või null (piiranguta)
    public static int? Piir(string nimi, string? kategooria)
    {
        nimi ??= "";
        bool alkoholivaba = nimi.StartsWith("Alkoholivaba", StringComparison.OrdinalIgnoreCase)
                            || nimi.Contains("0,0%") || nimi.Contains("0.0%");
        if (!alkoholivaba && (kategooria ?? "").StartsWith("Alkohol", StringComparison.OrdinalIgnoreCase)) return Alkohol;
        if (nimi.Contains("energiajook", StringComparison.OrdinalIgnoreCase) || nimi.Contains("red bull", StringComparison.OrdinalIgnoreCase))
            return Energiajook;
        return null;
    }

    public static int? Piir(Toode t) => Piir(t.Toodenimetus, t.Kategooria?.Kategooria_nimetus);

    // Ostukorvi piiranguga tooted (andmebaasist, sest korvireal kategooriat pole)
    public static List<(string Nimi, int Piir)> Korvis()
    {
        var ids = Ostukorv.Ridad.Select(r => r.Id).ToList();
        if (ids.Count == 0) return new();
        using var db = new ToodedContext();
        return db.Toodetabel.AsNoTracking().Where(t => ids.Contains(t.Id))
            .Select(t => new { t.Toodenimetus, Kat = t.Kategooria.Kategooria_nimetus }).ToList()
            .Select(t => (t.Toodenimetus, Piir(t.Toodenimetus, t.Kat)))
            .Where(x => x.Item2 != null).Select(x => (x.Toodenimetus, x.Item2!.Value)).ToList();
    }

    public static int Vanus(DateTime synd, DateTime tana)
    {
        int v = tana.Year - synd.Year;
        if (synd.Date > tana.Date.AddYears(-v)) v--;
        return v;
    }

    // Selle seansi jooksul meelde jäetud sünnikuupäev (kasutaja kaupa) – ei pea iga kord uuesti valima
    private static readonly Dictionary<string, DateTime> meeles = new();
    internal static DateTime? Meeles => meeles.TryGetValue(AktiivneKasutaja.Nimi ?? "", out var d) ? d : null;
    internal static void JataMeelde(DateTime d) { if (AktiivneKasutaja.OnOstja) meeles[AktiivneKasutaja.Nimi ?? ""] = d; }

    // Maksmise eel: true = võib jätkata (piiranguga tooteid pole või vanus sobib)
    public static bool Kontrolli(IWin32Window? omanik)
    {
        List<(string Nimi, int Piir)> tooted;
        try { tooted = Korvis(); } catch { return true; }
        if (tooted.Count == 0) return true;
        using var d = new VanuseKontrollVorm(tooted);
        return d.ShowDialog(omanik) == DialogResult.OK;
    }
}

public class VanuseKontrollVorm : KujundusVorm
{
    private readonly List<(string Nimi, int Piir)> tooted;
    private readonly DateTimePicker dtSynd;
    private readonly CheckBox chkKinnitus;
    private readonly int piir;

    protected override bool TagasiNupuga => false;

    public VanuseKontrollVorm(List<(string Nimi, int Piir)> tooted) : base("Vanuse kontroll", 460, 430)
    {
        this.tooted = tooted;
        piir = tooted.Max(t => t.Piir);
        Pealkiri(Keel.F("Vanuse kontroll · {0}+", piir));

        var nimekiri = string.Join("\n", tooted.Take(6).Select(t => $"• {t.Nimi}  ({t.Piir}+)")) + (tooted.Count > 6 ? "\n" + Keel.F("ja veel {0}", tooted.Count - 6) : "");
        var silt = Silt(Keel.T("Ostukorvis on vanusepiiranguga tooted (alkohol 18+, energiajoogid 16+):") + "\n" + nimekiri, 20, 56);
        silt.MaximumSize = new Size(420, 0);
        silt.AutoSize = true;
        int y = Math.Max(176, silt.Top + silt.PreferredHeight + 12);

        Silt(AktiivneKasutaja.OnOstja ? "Sinu sünnikuupäev" : "Ostja sünnikuupäev (dokumendilt)", 20, y);
        dtSynd = new DateTimePicker
        {
            Location = new Point(20, y + 20), Width = 200, Format = DateTimePickerFormat.Custom, CustomFormat = "dd.MM.yyyy",
            MaxDate = DateTime.Today, MinDate = DateTime.Today.AddYears(-120), Font = new Font("Segoe UI", 10F),
            Value = Vanusepiirang.Meeles ?? DateTime.Today.AddYears(-piir)
        };
        Controls.Add(dtSynd);

        chkKinnitus = new CheckBox
        {
            Location = new Point(20, y + 64), Size = new Size(420, 44), BackColor = Teema.Pind, ForeColor = Teema.Tekst,
            Text = AktiivneKasutaja.OnOstja
                ? Keel.T("Kinnitan, et andmed on õiged. Kauba üleandmisel kontrollitakse isikut tõendavat dokumenti.")
                : Keel.T("Kontrollisin ostja isikut tõendavat dokumenti.")
        };
        Controls.Add(chkKinnitus);

        var ok = Nupp("Kinnita", 20, y + 124, true);
        ok.Click += (s, e) => Kinnita();
        var eemalda = Nupp("Eemalda need tooted", 120, y + 124);
        eemalda.Click += (s, e) => Eemalda();
        var tyhista = Nupp("Tühista", 350, y + 124);
        tyhista.DialogResult = DialogResult.Cancel;
        CancelButton = tyhista;
        ClientSize = new Size(460, y + 180);
    }

    private void Kinnita()
    {
        int vanus = Vanusepiirang.Vanus(dtSynd.Value, DateTime.Today);
        var keelatud = tooted.Where(t => vanus < t.Piir).ToList();
        if (keelatud.Count > 0)
        {
            Teade.Show(Keel.F("Ostja on {0}-aastane. Neid tooteid ei saa osta:", vanus) + "\n• " +
                       string.Join("\n• ", keelatud.Select(t => $"{t.Nimi} ({t.Piir}+)")) + "\n\n" +
                       Keel.T("Eemalda need tooted ostukorvist ja proovi uuesti."),
                "Vanusepiirang", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!chkKinnitus.Checked) { Teade.Show(chkKinnitus.Text, "Vanusepiirang", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        Vanusepiirang.JataMeelde(dtSynd.Value.Date);
        DialogResult = DialogResult.OK;
        Close();
    }

    // Piiranguga tooted korvist välja; kui midagi jääb alles, võib maksmist jätkata
    private void Eemalda()
    {
        var nimed = tooted.Select(t => t.Nimi).ToHashSet();
        foreach (var r in Ostukorv.Ridad.Where(r => nimed.Contains(r.Nimi)).ToList()) Ostukorv.Eemalda(r);
        Teade.Toast(Keel.F("Ostukorvist eemaldati {0} toodet", nimed.Count), TeateTyyp.Info);
        DialogResult = Ostukorv.Ridad.Count > 0 ? DialogResult.OK : DialogResult.Cancel;
        Close();
    }
}
