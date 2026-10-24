namespace IKTpv25_1_Toooted;

// "Sarnased ja soodsamad": sama kategooria ja ühikuga tooted, mille kilo-/liitrihind on madalam
// ja nimes on vähemalt üks ühine sõna (nt "juust", "piim", "rukkileib").
public static class Sarnased
{
    private static readonly HashSet<string> Tyhisonad = new(StringComparer.OrdinalIgnoreCase) { "eesti", "suur", "väike", "klassikaline", "maitseline" };

    public static HashSet<string> Sonad(Toode t)
    {
        var s = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        foreach (var osa in t.Toodenimetus.Split(new[] { ' ', ',', '-', '/', '(', ')' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string w = new string(osa.Where(char.IsLetter).ToArray()).ToLowerInvariant();
            if (w.Length >= 4 && !Tyhisonad.Contains(w) && !string.Equals(w, t.Kaubamark, StringComparison.OrdinalIgnoreCase)) s.Add(w);
        }
        return s;
    }

    public static List<Toode> Leia(Toode toode, IEnumerable<Toode> koik, int max = 3)
    {
        if (toode.UhikuHind is not decimal minu || toode.Uhik is not ("kg" or "l")) return new();
        var minuSonad = Sonad(toode);
        if (minuSonad.Count == 0) return new();
        return koik
            .Where(t => t.Id != toode.Id && t.Kogus > 0 && t.KategooriaId == toode.KategooriaId && t.Uhik == toode.Uhik
                        && t.UhikuHind is decimal uh && uh < minu * 0.97m)
            .Select(t => (t, yhist: Sonad(t).Count(w => minuSonad.Any(m => Sarnane(w, m)))))
            .Where(x => x.yhist > 0)
            .OrderByDescending(x => x.yhist).ThenBy(x => x.t.UhikuHind)
            .Take(max)
            .Select(x => x.t)
            .ToList();
    }

    // Liitsõnad: "toorjuust" ~ "juust", "rukkileib" ~ "leib"
    public static bool Sarnane(string a, string b) =>
        a == b || (b.Length >= 4 && a.EndsWith(b)) || (a.Length >= 4 && b.EndsWith(a));

    // Kui palju protsenti on kilohind odavam
    public static int Odavam(Toode alus, Toode t) =>
        alus.UhikuHind is decimal a && t.UhikuHind is decimal b && a > 0 ? (int)Math.Round((1 - b / a) * 100) : 0;
}
