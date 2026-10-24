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

    // ---- Prognoos ----

    // Käive päevade kaupa: [0] = algus, [paevi-1] = algus + paevi-1 (tarne ei lähe käibe sisse)
    public static decimal[] PaevaKaive(IEnumerable<Ost> ostud, DateTime algus, int paevi)
    {
        var k = new decimal[paevi];
        foreach (var o in ostud)
        {
            int i = (o.Aeg.Date - algus.Date).Days;
            if (i >= 0 && i < paevi) k[i] += o.Summa - o.TarneHind;
        }
        return k;
    }

    // Järgmise "paevi" päeva käibe prognoos. ajalugu = täispäevad, viimane element on eilne päev.
    // Iga päev = sama nädalapäeva kaalutud keskmine viimasest 4 nädalast (värskem nädal kaalub rohkem: 4,3,2,1)
    // × trend (viimased 14 päeva / eelnevad 14 päeva, piiratud 0,8…1,25).
    public static decimal[] Prognoos(decimal[] ajalugu, int paevi = 7)
    {
        int n = ajalugu.Length;
        var tulem = new decimal[paevi];
        if (n == 0) return tulem;

        decimal viimased = ajalugu.Skip(Math.Max(0, n - 14)).Sum();
        decimal eelnevad = ajalugu.Skip(Math.Max(0, n - 28)).Take(Math.Max(0, Math.Min(14, n - 14))).Sum();
        decimal trend = eelnevad > 0 ? Math.Clamp(viimased / eelnevad, 0.8m, 1.25m) : 1m;

        for (int d = 0; d < paevi; d++)
        {
            decimal summa = 0, kaal = 0;
            for (int nadal = 1; nadal <= 4; nadal++)
            {
                // tulevane päev on indeksil n + d; sama nädalapäev nädal(aid) varem
                int i = n + d - 7 * nadal;
                while (i >= n) i -= 7;   // prognoos üle nädala: kasutame ajaloost sama nädalapäeva
                if (i < 0) continue;
                decimal w = 5 - nadal;
                summa += ajalugu[i] * w;
                kaal += w;
            }
            tulem[d] = kaal > 0 ? Math.Round(summa / kaal * trend, 2) : 0;
        }
        return tulem;
    }

    public record Lopeb(Toode Toode, decimal PaevasTk, decimal PaeviJaanud);

    // Tooted, mis praeguse müügitempo järgi lõpevad "piir" päeva jooksul (kõige kiiremad ees)
    public static List<Lopeb> Lopevad(IEnumerable<Ost> ostud, IEnumerable<Toode> tooted, int paevi = 28, int piir = 14)
    {
        var myydud = ostud.SelectMany(o => o.Read).Where(r => r.ToodeId != null)
            .GroupBy(r => r.ToodeId!.Value).ToDictionary(g => g.Key, g => g.Sum(r => r.Kogus));
        var tulem = new List<Lopeb>();
        foreach (var t in tooted)
        {
            if (!myydud.TryGetValue(t.Id, out int tk) || tk <= 0) continue;
            decimal paevas = (decimal)tk / paevi;
            decimal jaanud = Math.Round(Math.Max(0, t.Kogus) / paevas, 1);
            if (jaanud <= piir) tulem.Add(new Lopeb(t, Math.Round(paevas, 1), jaanud));
        }
        return tulem.OrderBy(l => l.PaeviJaanud).ThenByDescending(l => l.PaevasTk).ToList();
    }

    // Muutus eelmise perioodiga võrreldes (%), null kui võrrelda pole millega
    public static decimal? Muutus(decimal praegu, decimal enne) => enne == 0 ? null : Math.Round((praegu - enne) / enne * 100, 0);
}
