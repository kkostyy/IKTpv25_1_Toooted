using System.Text.Json;
using System.Text.RegularExpressions;

namespace IKTpv25_1_Toooted;

[Flags]
public enum DieediMark { Puudub = 0, Laktoosivaba = 1, Gluteenivaba = 2, Vegan = 4, Pahklivaba = 8, Eesti = 16 }

// Mida toote koostisest teada saime
public class DieediInfo
{
    public HashSet<string> Sisaldab { get; } = new();        // allergeenid / loomsed koostisosad
    public HashSet<string> VoibSisaldada { get; } = new();   // "Võib sisaldada ... jälgi"
    public bool KoostisTeada { get; set; }
    public bool LaktoosiVaba { get; set; }                   // "laktoosivaba" (piim võib olla, laktoosi pole)
    public bool Eesti { get; set; }
    public DieediMark AdminiMargid { get; set; }             // Toode.Margised
}

// Dieedi ja allergeenide analüüs toote koostisest (EL reegel: allergeenid on SUURTÄHTEDES)
public static class Dieet
{
    // Allergeenide rühmad (nimi tšekil/kaardil) ja nende sõnatüved
    private static readonly (string Ruhm, string[] Tyved)[] Ruhmad =
    {
        ("piim", new[] { "piim", "koor", "juust", "vadak", "laktoos", "jogurt", "kohupiim", "või " }),
        ("nisu", new[] { "nisu", "spelt" }),
        ("rukis", new[] { "rukis", "rukki" }),
        ("oder", new[] { "oder", "odra" }),
        ("kaer", new[] { "kaer" }),
        ("gluteen", new[] { "gluteen" }),
        ("muna", new[] { "muna" }),
        ("soja", new[] { "soja" }),
        ("kala", new[] { "kala" }),
        ("pähklid", new[] { "pähk", "sarapuu", "mandel", "mandli", "kašu", "india pähk", "pistaatsia", "kreeka pähk" }),
        ("seller", new[] { "seller" }),
        ("sinep", new[] { "sinep" }),
        ("seesam", new[] { "seesam" }),
        ("liha", new[] { "liha", "sealiha", "veise", "kana ", "kanaliha", "peekon", "vorst", "sink" }),
        ("mesi", new[] { "mesi", "mett" }),
    };

    private static readonly HashSet<string> Gluteen = new() { "nisu", "rukis", "oder", "kaer", "gluteen" };
    private static readonly HashSet<string> Loomne = new() { "piim", "muna", "kala", "liha", "mesi" };

    // Allergeenid, mida kaardil näidatakse (EL 14 hulgast)
    public static bool OnAllergeen(string ruhm) => ruhm is not ("liha" or "mesi");

    private static readonly Dictionary<int, (string Voti, DieediInfo Info)> vahemalu = new();

    public static DieediInfo Analuusi(Toode t)
    {
        string voti = $"{t.Koostisosad}|{t.Lisainfo}|{t.Kirjeldus}|{t.Margised}|{t.Paritolumaa}|{t.KategooriaId}|{t.Toodenimetus}";
        if (t.Id != 0 && vahemalu.TryGetValue(t.Id, out var v) && v.Voti == voti) return v.Info;

        var info = new DieediInfo
        {
            Eesti = (t.Paritolumaa ?? "").Trim().Equals("Eesti", StringComparison.OrdinalIgnoreCase),
            AdminiMargid = LoeMargid(t.Margised)
        };

        string koostis = t.Koostisosad ?? "";
        if (koostis.Trim().Length > 0)
        {
            info.KoostisTeada = true;
            // 1) SUURTÄHTEDES osad (ka sõna sees: täisPIIM, RUKKIjahu)
            foreach (Match m in Regex.Matches(koostis, "[A-ZÕÄÖÜŠŽ]{3,}"))
                if (Ruhm(m.Value.ToLowerInvariant()) is string r) info.Sisaldab.Add(r);
            // 2) tuntud sõnad väiketähtedes
            string vaike = " " + koostis.ToLowerInvariant().Replace(",", " ").Replace("(", " ").Replace(")", " ").Replace(".", " ") + " ";
            foreach (var (ruhm, tyved) in Ruhmad)
                if (tyved.Any(vaike.Contains)) info.Sisaldab.Add(ruhm);
        }

        // 3) "Võib sisaldada ... jälgi/jääke"
        foreach (var rida in (t.Lisainfo ?? "").Split('\n'))
        {
            int i = rida.IndexOf("võib sisaldada", StringComparison.OrdinalIgnoreCase);
            if (i < 0) continue;
            string rest = " " + rida[(i + 14)..].ToLowerInvariant() + " ";
            foreach (var (ruhm, tyved) in Ruhmad)
                if (tyved.Any(rest.Contains) && !info.Sisaldab.Contains(ruhm)) info.VoibSisaldada.Add(ruhm);
        }

        // 4) laktoosivaba
        string kogu = $"{t.Toodenimetus} {t.Kirjeldus} {t.Lisainfo}".ToLowerInvariant();
        info.LaktoosiVaba = kogu.Contains("laktoosivaba") || info.AdminiMargid.HasFlag(DieediMark.Laktoosivaba);

        // 5) puu- ja köögiviljad sobivad kõigile dieetidele
        if (t.KategooriaId == 3 || (t.Kategooria?.Kategooria_nimetus ?? "").StartsWith("Puuviljad", StringComparison.OrdinalIgnoreCase))
            info.KoostisTeada = true;

        if (t.Id != 0) vahemalu[t.Id] = (voti, info);
        return info;
    }

    private static string? Ruhm(string sona)
    {
        foreach (var (ruhm, tyved) in Ruhmad)
            if (tyved.Any(ty => sona.StartsWith(ty.Trim()) || (ty.Trim().Length >= 4 && sona.Contains(ty.Trim())))) return ruhm;
        return null;
    }

    public static DieediMark LoeMargid(string? margised)
    {
        var m = DieediMark.Puudub;
        foreach (var osa in (margised ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            m |= osa.ToLowerInvariant() switch
            {
                "laktoosivaba" => DieediMark.Laktoosivaba,
                "gluteenivaba" => DieediMark.Gluteenivaba,
                "vegan" => DieediMark.Vegan,
                "pahklivaba" or "pähklivaba" => DieediMark.Pahklivaba,
                _ => DieediMark.Puudub
            };
        return m;
    }

    public static string KirjutaMargid(DieediMark m) => string.Join(";", new[]
    {
        m.HasFlag(DieediMark.Laktoosivaba) ? "laktoosivaba" : null,
        m.HasFlag(DieediMark.Gluteenivaba) ? "gluteenivaba" : null,
        m.HasFlag(DieediMark.Vegan) ? "vegan" : null,
        m.HasFlag(DieediMark.Pahklivaba) ? "pahklivaba" : null
    }.Where(x => x != null));

    // Kas toode sobib valitud dieediga? Tundmatu koostisega toode ei sobi (ohutum).
    // Range režiimis ei sobi ka "võib sisaldada".
    public static bool Sobib(Toode t, DieediMark valik, bool range)
    {
        if (valik == DieediMark.Puudub) return true;
        var i = Analuusi(t);
        if (valik.HasFlag(DieediMark.Eesti) && !i.Eesti) return false;

        bool Kontroll(DieediMark mark, Func<string, bool> keelatud)
        {
            if (!valik.HasFlag(mark)) return true;
            if (i.AdminiMargid.HasFlag(mark)) return true;   // admini märge on kõige tähtsam
            if (!i.KoostisTeada) return false;
            if (i.Sisaldab.Any(keelatud)) return false;
            return !range || !i.VoibSisaldada.Any(keelatud);
        }

        return Kontroll(DieediMark.Laktoosivaba, r => r == "piim" && !i.LaktoosiVaba)
            && Kontroll(DieediMark.Gluteenivaba, Gluteen.Contains)
            && Kontroll(DieediMark.Vegan, Loomne.Contains)
            && Kontroll(DieediMark.Pahklivaba, r => r == "pähklid");
    }
}

// Kasutaja valitud dieet (fail AppData\Local\ToodedApp\dieet.json)
public static class MinuDieet
{
    private sealed class Andmed
    {
        public DieediMark Valik { get; set; }
        public bool Range { get; set; }
        public bool Peida { get; set; }
    }

    private static Andmed? andmed;
    private static string Fail => Path.Combine(Kaustad.Juur, "dieet.json");

    public static event Action? Muutus;

    private static Andmed A
    {
        get
        {
            if (andmed != null) return andmed;
            try { andmed = File.Exists(Fail) ? JsonSerializer.Deserialize<Andmed>(File.ReadAllText(Fail)) ?? new() : new(); }
            catch { andmed = new(); }
            return andmed;
        }
    }

    public static DieediMark Valik => A.Valik;
    public static bool Range => A.Range;
    public static bool Peida => A.Peida;   // sobimatud tooted peidetakse (muidu tuhmid)

    public static bool Sobib(Toode t) => Dieet.Sobib(t, A.Valik, A.Range);

    public static void Vaheta(DieediMark m) { A.Valik ^= m; Salvesta(); }
    public static void SeaRange(bool r) { A.Range = r; Salvesta(); }
    public static void SeaPeida(bool p) { A.Peida = p; Salvesta(); }

    private static void Salvesta()
    {
        try
        {
            Directory.CreateDirectory(Kaustad.Juur);
            File.WriteAllText(Fail, JsonSerializer.Serialize(A));
        }
        catch { }
        Muutus?.Invoke();
    }
}
