using System.Text.Json;

namespace IKTpv25_1_Toooted;

// "Minu lemmikud": tärniga märgitud tooted (fail AppData\Local\ToodedApp\lemmikud.json)
public static class Lemmikud
{
    private static HashSet<int>? ids;
    private static string Fail => Path.Combine(Kaustad.Juur, "lemmikud.json");

    public static event Action? Muutus;

    private static HashSet<int> Koik()
    {
        if (ids != null) return ids;
        try
        {
            ids = File.Exists(Fail)
                ? JsonSerializer.Deserialize<HashSet<int>>(File.ReadAllText(Fail)) ?? new()
                : new();
        }
        catch { ids = new(); }
        return ids;
    }

    public static bool On(int tooteId) => Koik().Contains(tooteId);

    public static void Vaheta(int tooteId)
    {
        var k = Koik();
        if (!k.Remove(tooteId)) k.Add(tooteId);
        try
        {
            Directory.CreateDirectory(Kaustad.Juur);
            File.WriteAllText(Fail, JsonSerializer.Serialize(k));
        }
        catch { }
        Muutus?.Invoke();
    }

    // ---- "Taas laos" teavitus ----
    // Lemmikud, mis olid viimati otsas (fail lemmikud_otsas.json). Kui mõni neist on jälle laos,
    // teatab pood sellest ja kaardil on kiip "Taas laos!" (kuni programmi sulgemiseni).
    private static string OtsasFail => Path.Combine(Kaustad.Juur, "lemmikud_otsas.json");
    private static readonly HashSet<int> taasLaos = new();

    public static bool TaasLaos(int tooteId) => taasLaos.Contains(tooteId);

    // Tagastab lemmikud, mis vahepeal said laoseisu juurde
    public static List<Toode> KontrolliLaoseisu(IEnumerable<Toode> tooted)
    {
        HashSet<int> otsas;
        try
        {
            otsas = File.Exists(OtsasFail)
                ? JsonSerializer.Deserialize<HashSet<int>>(File.ReadAllText(OtsasFail)) ?? new()
                : new();
        }
        catch { otsas = new(); }

        var lemmikud = tooted.Where(t => On(t.Id)).ToList();
        var tagasi = lemmikud.Where(t => t.Kogus > 0 && otsas.Contains(t.Id)).ToList();
        foreach (var t in tagasi) taasLaos.Add(t.Id);

        var uus = lemmikud.Where(t => t.Kogus <= 0).Select(t => t.Id).ToHashSet();
        if (!uus.SetEquals(otsas))
        {
            try
            {
                Directory.CreateDirectory(Kaustad.Juur);
                File.WriteAllText(OtsasFail, JsonSerializer.Serialize(uus));
            }
            catch { }
        }
        return tagasi;
    }
}

// Salvestatud ostukorvid (fail AppData\Local\ToodedApp\ostukorvid.json)
public class SalvestatudKorv
{
    public string Nimi { get; set; } = "";
    public DateTime Aeg { get; set; }
    public List<SalvestatudRida> Read { get; set; } = new();
}

public class SalvestatudRida
{
    public int Id { get; set; }
    public int Kogus { get; set; }
}

public static class SalvestatudKorvid
{
    private static string Fail => Path.Combine(Kaustad.Juur, "ostukorvid.json");

    public static List<SalvestatudKorv> Koik()
    {
        try
        {
            if (File.Exists(Fail))
                return JsonSerializer.Deserialize<List<SalvestatudKorv>>(File.ReadAllText(Fail)) ?? new();
        }
        catch { }
        return new();
    }

    private static void Kirjuta(List<SalvestatudKorv> korvid)
    {
        Directory.CreateDirectory(Kaustad.Juur);
        File.WriteAllText(Fail, JsonSerializer.Serialize(korvid, new JsonSerializerOptions { WriteIndented = true }));
    }

    // Salvestab praeguse ostukorvi
    public static void Salvesta(string nimi)
    {
        var korvid = Koik();
        korvid.RemoveAll(k => k.Nimi.Equals(nimi, StringComparison.CurrentCultureIgnoreCase));
        korvid.Insert(0, new SalvestatudKorv
        {
            Nimi = nimi,
            Aeg = DateTime.Now,
            Read = Ostukorv.Ridad.Select(r => new SalvestatudRida { Id = r.Id, Kogus = r.Kogus }).ToList()
        });
        Kirjuta(korvid);
    }

    public static void Kustuta(string nimi)
    {
        var korvid = Koik();
        korvid.RemoveAll(k => k.Nimi == nimi);
        Kirjuta(korvid);
    }
}
