namespace IKTpv25_1_Toooted;

// Ühtne lõpphinna arvutus: kampaania (juba Toode.Hind-is), säilivusaja allahindlus,
// õhtune küpsetiste soodustus ja päeva üllatus. Soodustused ei liitu – kehtib suurim.
public static class Hinnad
{
    public record Tulemus(decimal Hind, decimal TavaHind, string? Allikas)
    {
        public bool Soodus => Hind < TavaHind;
        public int Protsent => TavaHind > 0 ? (int)Math.Round((1 - Hind / TavaHind) * 100) : 0;
    }

    // Kell on vahetatav (testimiseks: Hinnad.Kell = () => new DateTime(2026, 10, 24, 20, 5, 0))
    public static Func<DateTime> Kell { get; set; } = () => DateTime.Now;

    // Tänane üllatus aktiivsele kliendile (vahemälu, uuendatakse kord päevas või avamisel)
    private static (DateTime Paev, int? KlientId, int ToodeId, int Protsent)? yllatus;
    private static bool yllatusLaetud;

    public static void UnustaYllatus() => yllatusLaetud = false;

    private static (int ToodeId, int Protsent)? TananeYllatus(DateTime aeg)
    {
        if (AktiivneKasutaja.KlientId is not int kid) return null;
        if (!yllatusLaetud || yllatus?.Paev != aeg.Date || yllatus?.KlientId != kid)
        {
            yllatusLaetud = true;
            yllatus = (aeg.Date, kid, 0, 0);
            try
            {
                using var db = new ToodedContext();
                if (Preemiad.TananeYllatus(db, kid, aeg) is { } y) yllatus = (aeg.Date, kid, y.Toode.Id, y.Protsent);
            }
            catch { }
        }
        return yllatus is { ToodeId: > 0 } y2 ? (y2.ToodeId, y2.Protsent) : null;
    }

    private static decimal Ymarda(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    public static Tulemus Lopphind(Toode t) => Lopphind(t, Kell());

    public static Tulemus Lopphind(Toode t, DateTime aeg)
    {
        decimal tava = Kampaaniad.TavaHind(t);
        var valikud = new List<(decimal Hind, string? Allikas)> { (t.Hind, t.Hind < tava ? "Kampaania" : null) };

        foreach (var (protsent, allikas) in LisaSoodustused(t, aeg))
            valikud.Add((Ymarda(tava * (100 - protsent) / 100m), allikas));

        var parim = valikud.OrderBy(v => v.Hind).First();   // suurim soodustus võidab
        return new Tulemus(parim.Hind, tava, parim.Allikas);
    }

    // Soodustused protsentides tavahinnast
    private static IEnumerable<(int Protsent, string Allikas)> LisaSoodustused(Toode t, DateTime aeg)
    {
        if (TananeYllatus(aeg) is { } y && y.ToodeId == t.Id)
            yield return (y.Protsent, Keel.F("Päeva üllatus −{0} %", y.Protsent));
    }
}
