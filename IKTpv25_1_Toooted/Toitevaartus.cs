using System.Globalization;
using System.Text.RegularExpressions;

namespace IKTpv25_1_Toooted;

// Ostukorvi toiteväärtus kokku (100 g/ml andmete ja pakendi kaalu järgi)
public class KorviToitevaartus
{
    public decimal Kcal, Rasvad, Kyllastunud, Susivesikud, Suhkrud, Valgud, Sool;
    public Dictionary<int, decimal> KcalToote { get; } = new();   // toote Id → kcal kokku
    public List<string> IlmaAndmeteta { get; } = new();

    public decimal Paevi => Toitevaartus.Paevakogus.Kcal > 0 ? Kcal / Toitevaartus.Paevakogus.Kcal : 0;
}

public static class Toitevaartus
{
    // Päevane võrdluskogus (RI, täiskasvanu)
    public static readonly (decimal Kcal, decimal Rasvad, decimal Kyllastunud, decimal Susivesikud, decimal Suhkrud, decimal Valgud, decimal Sool)
        Paevakogus = (2000, 70, 20, 260, 90, 50, 6);

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    // "energiasisaldus;1490 kJ / 360 kcal\nrasvad;27 g\n-millest küllastunud rasvhapped;17 g ..."
    // → { "energiasisaldus": 360, "rasvad": 27, "millest küllastunud rasvhapped": 17, ... }
    public static Dictionary<string, decimal> Parsi(string? tekst)
    {
        var tulemus = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var rida in (tekst ?? "").Split('\n'))
        {
            var osad = rida.Split(';');
            if (osad.Length < 2) continue;
            string nimi = osad[0].Trim().TrimStart('-').Trim().ToLowerInvariant();
            string v = osad[1];
            var m = Regex.Match(v, @"(\d+(?:[.,]\d+)?)\s*kcal", RegexOptions.IgnoreCase);
            if (!m.Success) m = Regex.Match(v, @"(\d+(?:[.,]\d+)?)\s*g\b", RegexOptions.IgnoreCase);
            if (!m.Success) m = Regex.Match(v, @"(\d+(?:[.,]\d+)?)");
            if (m.Success && decimal.TryParse(m.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, Inv, out decimal d))
                tulemus[nimi] = d;
        }
        return tulemus;
    }

    // Pakendi kaal grammides (liiter = 1000 g); tükikaubal null
    public static decimal? KaalGrammides(Toode t)
    {
        string uhik = (t.Uhik ?? "").Trim().ToLowerInvariant();
        if ((uhik == "kg" || uhik == "l") && t.UhikuKogus is decimal k && k > 0) return k * 1000;
        var m = Regex.Match(t.Pakend ?? "", @"(\d+(?:[.,]\d+)?)\s*(kg|l|g|ml)\b", RegexOptions.IgnoreCase);
        if (!m.Success || !decimal.TryParse(m.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, Inv, out decimal arv)) return null;
        return m.Groups[2].Value.ToLowerInvariant() switch { "kg" or "l" => arv * 1000, _ => arv };
    }

    private static decimal Vali(Dictionary<string, decimal> d, params string[] nimed)
    {
        foreach (var n in nimed)
            foreach (var kv in d)
                if (kv.Key.Contains(n)) return kv.Value;
        return 0;
    }

    public static KorviToitevaartus Kokkuvote(IEnumerable<KorvRida> read, IDictionary<int, Toode> tooted)
    {
        var k = new KorviToitevaartus();
        foreach (var r in read)
        {
            if (!tooted.TryGetValue(r.Id, out var t)) { k.IlmaAndmeteta.Add(r.Nimi); continue; }
            var d = Parsi(t.Toitevaartus);
            decimal? kaal = KaalGrammides(t);
            if (d.Count == 0 || kaal == null) { k.IlmaAndmeteta.Add(t.Toodenimetus); continue; }

            decimal kordaja = kaal.Value / 100m * r.Kogus;
            decimal kcal = Vali(d, "energia") * kordaja;
            k.Kcal += kcal;
            k.Rasvad += Vali(d, "rasvad") * kordaja;
            k.Kyllastunud += Vali(d, "küllastunud") * kordaja;
            k.Susivesikud += Vali(d, "süsivesikud") * kordaja;
            k.Suhkrud += Vali(d, "suhkrud") * kordaja;
            k.Valgud += Vali(d, "valgud") * kordaja;
            k.Sool += Vali(d, "sool") * kordaja;
            k.KcalToote[r.Id] = k.KcalToote.GetValueOrDefault(r.Id) + kcal;
        }
        return k;
    }

    // Ühe toote kcal korvis (null = andmed puuduvad)
    public static decimal? TooteKcal(Toode t, int kogus)
    {
        var d = Parsi(t.Toitevaartus);
        decimal? kaal = KaalGrammides(t);
        if (d.Count == 0 || kaal == null) return null;
        return Vali(d, "energia") * kaal.Value / 100m * kogus;
    }
}
