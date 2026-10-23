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

    // Boonust saab ainult toodete eest (tarne ei anna boonust)
    public static decimal Boonus(decimal toodeteEestMakstud) =>
        Math.Round(Math.Max(0, toodeteEestMakstud) * Form1.BoonusProtsent, 2, MidpointRounding.AwayFromZero);
}
