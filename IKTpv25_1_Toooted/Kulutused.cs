namespace IKTpv25_1_Toooted;

// Ostude ajalugu statistikana: kui palju kulus igas kategoorias kuude kaupa (tarne ja pant ei lähe arvesse)
public static class Kulutused
{
    public const string Muu = "Muu";

    public record Kuu(DateTime Algus, Dictionary<string, decimal> Kategooriad)
    {
        public decimal Kokku => Kategooriad.Values.Sum();
    }

    public record Osa(string Nimi, decimal Summa, decimal Protsent);

    // Viimased "kuid" kuud (vanim ees), ka tühjad kuud
    public static List<Kuu> Arvuta(IEnumerable<Ost> ostud, IReadOnlyDictionary<int, string> kategooria, DateTime nyyd, int kuid = 6)
    {
        var esimene = new DateTime(nyyd.Year, nyyd.Month, 1).AddMonths(-(kuid - 1));
        var kuud = Enumerable.Range(0, kuid).Select(i => new Kuu(esimene.AddMonths(i), new Dictionary<string, decimal>())).ToList();
        foreach (var o in ostud.Where(o => !o.Tagastatud && o.Aeg >= esimene))
        {
            int i = (o.Aeg.Year - esimene.Year) * 12 + o.Aeg.Month - esimene.Month;
            if (i < 0 || i >= kuid) continue;
            foreach (var r in o.Read)
            {
                string k = r.ToodeId is int id && kategooria.TryGetValue(id, out var n) ? n : Muu;
                kuud[i].Kategooriad[k] = kuud[i].Kategooriad.GetValueOrDefault(k) + r.Hind * r.Kogus;
            }
        }
        return kuud;
    }

    // Kategooriate jaotus (suurim ees); väikesed (< 4 %) ja üle 7. koha → "Muu"
    public static List<Osa> Jaotus(IEnumerable<Kuu> kuud)
    {
        var kokku = new Dictionary<string, decimal>();
        foreach (var k in kuud)
            foreach (var (nimi, summa) in k.Kategooriad) kokku[nimi] = kokku.GetValueOrDefault(nimi) + summa;
        decimal koik = kokku.Values.Sum();
        if (koik <= 0) return new();
        var jarjest = kokku.Where(x => x.Key != Muu).OrderByDescending(x => x.Value).ToList();
        var suured = jarjest.Take(7).Where(x => x.Value / koik >= 0.04m).ToList();
        decimal muu = koik - suured.Sum(x => x.Value);
        var tulem = suured.Select(x => new Osa(x.Key, x.Value, Math.Round(x.Value / koik * 100, 1))).ToList();
        if (muu > 0) tulem.Add(new Osa(Muu, muu, Math.Round(muu / koik * 100, 1)));
        return tulem;
    }
}
