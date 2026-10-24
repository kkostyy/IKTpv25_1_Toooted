namespace IKTpv25_1_Toooted;

// Kohaletoimetamine: tasuline, ostukorvist alates 25 € tasuta. Poest kaasa võttes alati tasuta.
public static class Tarne
{
    public const string Kaasa = "Kaasa";
    public const string Kojuvedu = "Kojuvedu";

    public const decimal Hind = 3.99m;
    public const decimal TasutaAlates = 25m;

    // Ostukorvi (toodete) summa järgi
    public static decimal Arvuta(decimal korviSumma) =>
        korviSumma <= 0 || korviSumma >= TasutaAlates ? 0 : Hind;

    // Kui palju on veel puudu tasuta tarneni (0 = juba tasuta)
    public static decimal Puudu(decimal korviSumma) => Math.Max(0, TasutaAlates - korviSumma);

    // ---- Kojuveo ajaaknad: 2 tundi, 10–22, 7 päeva ette; aknas kuni 3 tellimust ----
    public const int AknaTunde = 2;
    public const int MaxAknas = 3;
    public static readonly int[] AknaAlgused = { 10, 12, 14, 16, 18, 20 };
    public static readonly TimeSpan MinEtteaeg = TimeSpan.FromHours(2);

    // Kõik valitavad aknad alates "nyyd" (esimene peab algama vähemalt 2 h pärast)
    public static List<DateTime> Aknad(DateTime nyyd, int paevi = 7) =>
        Enumerable.Range(0, paevi)
            .SelectMany(p => AknaAlgused.Select(t => nyyd.Date.AddDays(p).AddHours(t)))
            .Where(a => a - nyyd >= MinEtteaeg)
            .ToList();

    public static bool Vaba(DateTime aken, IReadOnlyDictionary<DateTime, int> hoivatud) =>
        hoivatud.GetValueOrDefault(aken) < MaxAknas;

    // Esimene vaba aken (või null)
    public static DateTime? EsimeneVaba(DateTime nyyd, IReadOnlyDictionary<DateTime, int> hoivatud) =>
        Aknad(nyyd).Cast<DateTime?>().FirstOrDefault(a => Vaba(a!.Value, hoivatud));

    // Tellimuste arv akende kaupa (tagastatud ostud ei loe)
    public static Dictionary<DateTime, int> Hoivatud(ToodedContext db, DateTime alates) =>
        db.Ostud.Where(o => o.TarneAeg != null && !o.Tagastatud && o.TarneAeg >= alates.Date)
            .GroupBy(o => o.TarneAeg!.Value).Select(g => new { g.Key, Arv = g.Count() })
            .ToDictionary(x => x.Key, x => x.Arv);

    private static readonly string[] PaevaNimed = { "Pühapäev", "Esmaspäev", "Teisipäev", "Kolmapäev", "Neljapäev", "Reede", "Laupäev" };

    public static string PaevaNimi(DateTime paev, DateTime tana) =>
        paev.Date == tana.Date ? Keel.T("Täna") : paev.Date == tana.Date.AddDays(1) ? Keel.T("Homme") : Keel.T(PaevaNimed[(int)paev.DayOfWeek]);

    // "Homme 26.10 · 18–20"
    public static string AknaTekst(DateTime aken, DateTime tana) =>
        $"{PaevaNimi(aken, tana)} {aken:dd.MM} · {aken.Hour}–{aken.Hour + AknaTunde}";

    // Boonust saab ainult toodete eest (tarne ega pant boonust ei anna); protsent sõltub kaardi tasemest
    public static decimal Boonus(decimal toodeteEestMakstud, decimal protsent) =>
        Math.Round(Math.Max(0, toodeteEestMakstud) * protsent, 2, MidpointRounding.AwayFromZero);
}
