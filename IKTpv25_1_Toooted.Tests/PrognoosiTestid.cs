using IKTpv25_1_Toooted;
using Xunit;

namespace IKTpv25_1_Toooted.Tests;

public class PrognoosiTestid
{
    private static Ost Ost(DateTime aeg, decimal summa, decimal tarne = 0, params (int Id, int Kogus)[] read) => new()
    {
        Aeg = aeg,
        Summa = summa,
        TarneHind = tarne,
        Read = read.Select(r => new OstuRida { ToodeId = r.Id, Kogus = r.Kogus, Hind = 1m }).ToList()
    };

    [Fact]
    public void Paeva_kaive_ilma_tarneta()
    {
        var algus = new DateTime(2026, 10, 1);
        var k = Myygianalyys.PaevaKaive(new[]
        {
            Ost(algus.AddHours(10), 10m),
            Ost(algus.AddHours(18), 13.99m, tarne: 3.99m),
            Ost(algus.AddDays(2).AddHours(9), 5m),
            Ost(algus.AddDays(-1), 100m),      // enne perioodi
            Ost(algus.AddDays(3), 100m)        // pärast perioodi
        }, algus, 3);
        Assert.Equal(new[] { 20m, 0m, 5m }, k);
    }

    [Fact]
    public void Stabiilne_myyk_annab_sama_prognoosi()
    {
        var ajalugu = Enumerable.Repeat(50m, 28).ToArray();
        Assert.All(Myygianalyys.Prognoos(ajalugu, 7), p => Assert.Equal(50m, p));
    }

    [Fact]
    public void Prognoos_arvestab_nadalapaeva()
    {
        // iga 7. päev (sama nädalapäev) on 100 €, muud päevad 10 €
        var ajalugu = Enumerable.Range(0, 28).Select(i => i % 7 == 0 ? 100m : 10m).ToArray();
        var p = Myygianalyys.Prognoos(ajalugu, 7);
        Assert.Equal(100m, p[0]);   // indeks 28 → sama nädalapäev mis 0, 7, 14, 21
        Assert.All(p.Skip(1), v => Assert.Equal(10m, v));
    }

    [Fact]
    public void Kasvav_trend_suurendab_prognoosi_piiratult()
    {
        var ajalugu = Enumerable.Repeat(10m, 14).Concat(Enumerable.Repeat(40m, 14)).ToArray();
        var p = Myygianalyys.Prognoos(ajalugu, 7);
        // kaalutud keskmine (40·4 + 40·3 + 10·2 + 10·1) / 10 = 31, trend piiratud 1,25 → 38,75
        Assert.Equal(38.75m, p[0]);
    }

    [Fact]
    public void Tyhi_ajalugu_annab_nullid()
    {
        Assert.All(Myygianalyys.Prognoos(Array.Empty<decimal>(), 7), p => Assert.Equal(0m, p));
    }

    [Fact]
    public void Peagi_loppevad_tooted_kiireimad_ees()
    {
        var tooted = new[]
        {
            new Toode { Id = 1, Toodenimetus = "Piim", Kogus = 4 },    // 28 tk / 28 p = 1 päevas → 4 päeva
            new Toode { Id = 2, Toodenimetus = "Leib", Kogus = 2 },    // 56 tk → 2 päevas → 1 päev
            new Toode { Id = 3, Toodenimetus = "Sool", Kogus = 100 },  // 28 tk → 100 päeva (ei lõpe)
            new Toode { Id = 4, Toodenimetus = "Kala", Kogus = 0 }     // ei müüdud → pole nimekirjas
        };
        var ostud = new[] { Ost(DateTime.Today, 0, 0, (1, 28), (2, 56), (3, 28)) };
        var l = Myygianalyys.Lopevad(ostud, tooted);
        Assert.Equal(new[] { 2, 1 }, l.Select(x => x.Toode.Id));
        Assert.Equal(1m, l[0].PaeviJaanud);
        Assert.Equal(4m, l[1].PaeviJaanud);
    }

    [Theory]
    [InlineData(150, 100, 50)]
    [InlineData(50, 100, -50)]
    public void Muutus_protsentides(double praegu, double enne, double oodatud)
    {
        Assert.Equal((decimal)oodatud, Myygianalyys.Muutus((decimal)praegu, (decimal)enne));
    }

    [Fact]
    public void Muutust_pole_kui_eelmine_on_null()
    {
        Assert.Null(Myygianalyys.Muutus(10m, 0m));
    }
}

public class AutotellimuseTestid
{
    private static Ost Ost(params (int Id, int Kogus)[] read) => new()
    {
        Aeg = DateTime.Today,
        Read = read.Select(r => new OstuRida { ToodeId = r.Id, Kogus = r.Kogus, Hind = 1m }).ToList()
    };

    [Fact]
    public void Tellib_kaetuse_jaoks_puuduva_koguse()
    {
        var tooted = new[]
        {
            new Toode { Id = 1, Toodenimetus = "Piim", Kogus = 5, Tootja = "Farmi" },      // 1 päevas, jätkub 5 p → 14+3−5 = 12
            new Toode { Id = 2, Toodenimetus = "Sool", Kogus = 100, Tootja = "Salvest" },  // jätkub 100 p → ei telli
            new Toode { Id = 3, Toodenimetus = "Leib", Kogus = 0, Kaubamark = "Leibur" },  // 2 päevas, otsas → 34
        };
        var r = Autotellimus.Arvuta(new[] { Ost((1, 28), (2, 28), (3, 56)) }, tooted, kate: 14);
        Assert.Equal(2, r.Count);
        var piim = r.Single(x => x.Toode.Id == 1);
        Assert.Equal(12, piim.Soovitus);
        Assert.Equal("Farmi", piim.Tarnija);
        var leib = r.Single(x => x.Toode.Id == 3);
        Assert.Equal(34, leib.Soovitus);
        Assert.Equal("Leibur", leib.Tarnija);   // tootja puudub → kaubamärk
    }

    [Fact]
    public void Pikem_kate_tellib_rohkem()
    {
        var t = new[] { new Toode { Id = 1, Toodenimetus = "Piim", Kogus = 5 } };
        var ostud = new[] { Ost((1, 28)) };
        Assert.Equal(5, Autotellimus.Arvuta(ostud, t, kate: 7).Single().Soovitus);    // 7+3−5
        Assert.Equal(28, Autotellimus.Arvuta(ostud, t, kate: 30).Single().Soovitus);  // 30+3−5
        Assert.Equal(Autotellimus.MuuTarnija, Autotellimus.Arvuta(ostud, t).Single().Tarnija);
    }
}
