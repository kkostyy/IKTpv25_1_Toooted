namespace IKTpv25_1_Toooted;

// "Mida ostetakse koos": seosreeglid ostude põhjal (A → B).
// Toetus = mitmes ostus mõlemad; kindlus = P(B | A); tõus (lift) = kindlus / P(B) – üle 1 tähendab tõelist seost.
public static class Ostukorvianalyys
{
    public record Reegel(string A, string B, int Koos, int OsteA, double Kindlus, double Tous, int? ToodeA = null, int? ToodeB = null);

    // Üldine: iga ost on võtmete hulk (toote Id-d või kategooriad)
    public static List<Reegel> Reeglid<T>(IEnumerable<HashSet<T>> ostud, Func<T, string> nimi, int minKoos = 3, double minTous = 1.1) where T : notnull
    {
        var hulgad = ostud.Where(h => h.Count > 0).ToList();
        int n = hulgad.Count;
        if (n == 0) return new();
        var yks = new Dictionary<T, int>();
        var paar = new Dictionary<(T, T), int>();
        foreach (var h in hulgad)
        {
            foreach (var a in h) yks[a] = yks.GetValueOrDefault(a) + 1;
            foreach (var a in h)
                foreach (var b in h)
                    if (!a.Equals(b)) paar[(a, b)] = paar.GetValueOrDefault((a, b)) + 1;
        }
        var tulem = new List<Reegel>();
        foreach (var ((a, b), koos) in paar)
        {
            if (koos < minKoos) continue;
            double kindlus = (double)koos / yks[a];
            double tous = kindlus / ((double)yks[b] / n);
            if (tous < minTous) continue;
            tulem.Add(new Reegel(nimi(a), nimi(b), koos, yks[a], kindlus, tous,
                a is int ia ? ia : null, b is int ib ? ib : null));
        }
        return tulem.OrderByDescending(r => r.Tous * Math.Sqrt(r.Koos)).ThenByDescending(r => r.Kindlus).ToList();
    }

    public static List<Reegel> Tooted(IEnumerable<Ost> ostud, Func<int, string> nimi, int minKoos = 3) =>
        Reeglid(ostud.Where(o => !o.Tagastatud).Select(o => o.Read.Where(r => r.ToodeId != null).Select(r => r.ToodeId!.Value).ToHashSet()), nimi, minKoos);

    public static List<Reegel> Kategooriad(IEnumerable<Ost> ostud, IReadOnlyDictionary<int, string> kategooria, int minKoos = 3) =>
        Reeglid(ostud.Where(o => !o.Tagastatud).Select(o => o.Read.Where(r => r.ToodeId is int id && kategooria.ContainsKey(id))
            .Select(r => kategooria[r.ToodeId!.Value]).ToHashSet()), s => s, minKoos);

    // Tooted, mida selle tootega kõige sagedamini koos osteti (toote lehe "Sageli ostetakse koos")
    public static List<int> KoosOstetud(int toodeId, IEnumerable<Ost> ostud, int max = 4) =>
        ostud.Where(o => !o.Tagastatud && o.Read.Any(r => r.ToodeId == toodeId))
            .SelectMany(o => o.Read.Where(r => r.ToodeId is int id && id != toodeId).Select(r => r.ToodeId!.Value).Distinct())
            .GroupBy(id => id).Where(g => g.Count() >= 2)
            .OrderByDescending(g => g.Count()).Take(max).Select(g => g.Key).ToList();
}
