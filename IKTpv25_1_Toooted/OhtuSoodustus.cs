using System.Text.Json;

namespace IKTpv25_1_Toooted;

// Õhtune küpsetiste soodustus: iga päev alates kella 20:00 kuni poe sulgemiseni (00:00)
// on kategooria "Leivad, saiad, kondiitritooted" 2 korda odavam. Seaded failis ohtu.json.
public static class OhtuSoodustus
{
    public sealed class Seaded
    {
        public bool Sees { get; set; } = true;
        public TimeSpan Algus { get; set; } = new(20, 0, 0);
        public int Protsent { get; set; } = 50;
        public int KategooriaId { get; set; } = 2;
    }

    private static Seaded? seaded;
    private static string Fail => Path.Combine(Kaustad.Juur, "ohtu.json");

    public static Seaded S
    {
        get
        {
            if (seaded != null) return seaded;
            try { seaded = File.Exists(Fail) ? JsonSerializer.Deserialize<Seaded>(File.ReadAllText(Fail)) ?? new() : new(); }
            catch { seaded = new(); }
            return seaded;
        }
    }

    public static void Salvesta(Seaded uus)
    {
        seaded = uus;
        try
        {
            Directory.CreateDirectory(Kaustad.Juur);
            File.WriteAllText(Fail, JsonSerializer.Serialize(uus));
        }
        catch { }
    }

    public static bool Kaib(DateTime aeg) => S.Sees && aeg.TimeOfDay >= S.Algus;

    public static bool Kehtib(Toode t, DateTime aeg) => Kaib(aeg) && t.KategooriaId == S.KategooriaId;

    // Kui palju on alguseni (null = juba käib või väljas)
    public static TimeSpan? Alguseni(DateTime aeg) => !S.Sees || Kaib(aeg) ? null : S.Algus - aeg.TimeOfDay;
}
