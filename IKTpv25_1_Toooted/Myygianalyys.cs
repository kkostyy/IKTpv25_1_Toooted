using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

public record MyygiKpi(decimal Kaive, int Oste, decimal Keskmine, decimal Marginaal);

public record KategooriaMarginaal(string Nimi, decimal Kaive, decimal Marginaal, bool Hinnanguline);

// Müügianalüüs (puhtad arvutused): soojuskaart, KPI-d, kategooriate marginaal
public static class Myygianalyys
{
    public const int AlgusTund = 8, LoppTund = 22;          // soojuskaardi tunnid 8–22
    public const decimal HinnangulineMarginaal = 0.30m;     // kui sisseostuhinda pole

    public static List<Ost> Ostud(ToodedContext db, DateTime alates, DateTime kuni) =>
        db.Ostud.AsNoTracking().Include(o => o.Read)
            .Where(o => !o.Tagastatud && o.Aeg >= alates && o.Aeg < kuni)
            .ToList();

    // [päev 0 = esmaspäev .. 6 = pühapäev, tund AlgusTund..LoppTund] → (ostude arv, summa)
    public static (int Arv, decimal Summa)[,] Soojuskaart(IEnumerable<Ost> ostud)
    {
        var k = new (int, decimal)[7, LoppTund - AlgusTund + 1];
        foreach (var o in ostud)
        {
            int p = ((int)o.Aeg.DayOfWeek + 6) % 7;
            int t = Math.Clamp(o.Aeg.Hour, AlgusTund, LoppTund) - AlgusTund;
            k[p, t] = (k[p, t].Item1 + 1, k[p, t].Item2 + o.Summa - o.TarneHind);
        }
        return k;
    }

    public static decimal RidaMarginaal(OstuRida r, IDictionary<int, Toode> tooted, out bool hinnanguline)
    {
        hinnanguline = true;
        if (r.ToodeId is int id && tooted.TryGetValue(id, out var t) && t.Ostuhind is decimal oh)
        {
            hinnanguline = false;
            return (r.Hind - oh) * r.Kogus;
        }
        return r.Hind * r.Kogus * HinnangulineMarginaal;
    }

    public static MyygiKpi Kpi(List<Ost> ostud, IDictionary<int, Toode> tooted)
    {
        decimal kaive = ostud.Sum(o => o.Summa - o.TarneHind);
        decimal marg = ostud.SelectMany(o => o.Read).Sum(r => RidaMarginaal(r, tooted, out _));
        return new MyygiKpi(kaive, ostud.Count, ostud.Count > 0 ? Math.Round(kaive / ostud.Count, 2) : 0, marg);
    }

    public static List<KategooriaMarginaal> Kategooriad(List<Ost> ostud, IDictionary<int, Toode> tooted, Dictionary<int, string> nimed)
    {
        var kokku = new Dictionary<int, (decimal Kaive, decimal Marg, bool Hinn)>();
        foreach (var r in ostud.SelectMany(o => o.Read))
        {
            int kat = r.ToodeId is int id && tooted.TryGetValue(id, out var t) ? t.KategooriaId : 0;
            decimal m = RidaMarginaal(r, tooted, out bool h);
            var v = kokku.GetValueOrDefault(kat);
            kokku[kat] = (v.Kaive + r.Summa, v.Marg + m, v.Hinn || h);
        }
        return kokku.Select(kv => new KategooriaMarginaal(nimed.GetValueOrDefault(kv.Key, "Muu"), kv.Value.Kaive, kv.Value.Marg, kv.Value.Hinn))
            .OrderByDescending(k => k.Marginaal).ToList();
    }

    // Muutus eelmise perioodiga võrreldes (%), null kui võrrelda pole millega
    public static decimal? Muutus(decimal praegu, decimal enne) => enne == 0 ? null : Math.Round((praegu - enne) / enne * 100, 0);
}
