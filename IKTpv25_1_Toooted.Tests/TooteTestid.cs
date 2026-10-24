using IKTpv25_1_Toooted;
using Xunit;

namespace IKTpv25_1_Toooted.Tests;

public class TooteTestid
{
    [Fact]
    public void Uhikuhind_jagab_hinna_kogusega()
    {
        var t = new Toode { Hind = 1.50m, UhikuKogus = 0.5m, Uhik = "kg" };
        Assert.Equal(3.00m, t.UhikuHind);
    }

    [Fact]
    public void Uhikuhind_puudub_kui_kogust_pole()
    {
        Assert.Null(new Toode { Hind = 1.50m }.UhikuHind);
        Assert.Null(new Toode { Hind = 1.50m, UhikuKogus = 0 }.UhikuHind);
    }

    [Theory]
    [InlineData(3.49, 1.49, 57)]
    [InlineData(2.00, 1.50, 25)]
    public void Soodusprotsent_vanast_hinnast(double vana, double uus, int oodatud)
    {
        var t = new Toode { Hind = (decimal)uus, VanaHind = (decimal)vana };
        Assert.Equal(oodatud, t.SoodusProtsent);
    }

    [Fact]
    public void Soodusprotsenti_pole_kui_vana_hind_pole_suurem()
    {
        Assert.Null(new Toode { Hind = 2m, VanaHind = 2m }.SoodusProtsent);
        Assert.Null(new Toode { Hind = 2m }.SoodusProtsent);
    }
}

public class SarnasteTestid
{
    private static Toode T(int id, string nimi, decimal hind, decimal kogus, string uhik = "kg", int kat = 1, int laos = 10) =>
        new() { Id = id, Toodenimetus = nimi, Hind = hind, UhikuKogus = kogus, Uhik = uhik, KategooriaId = kat, Kogus = laos };

    [Fact]
    public void Leiab_sama_toote_odavama_kilohinnaga()
    {
        var alus = T(1, "Juust Atleet viilutatud 500g", 2.99m, 0.5m);          // 5,98 €/kg
        var koik = new[]
        {
            alus,
            T(2, "Juust Eesti viilutatud 1kg", 4.99m, 1m),                       // 4,99 €/kg – sobib
            T(3, "Juust Edam 300g", 2.49m, 0.3m),                                // 8,30 €/kg – kallim
            T(4, "Kohupiim 400g", 1.20m, 0.4m),                                  // pole ühist sõna
            T(5, "Juust Gouda 1kg", 3.99m, 1m, laos: 0),                         // otsas
            T(6, "Juust röstsaia 1l", 1.00m, 1m, uhik: "l"),                     // teine ühik
            T(7, "Juust kodune 1kg", 3.50m, 1m, kat: 2),                         // teine kategooria
        };
        var leitud = Sarnased.Leia(alus, koik);
        Assert.Equal(new[] { 2 }, leitud.Select(t => t.Id));
        Assert.Equal(17, Sarnased.Odavam(alus, leitud[0]));
    }

    [Fact]
    public void Tykihinnaga_tootel_pole_vordlust()
    {
        var alus = T(1, "Munad M 10tk", 2.89m, 10m, uhik: "tk");
        Assert.Empty(Sarnased.Leia(alus, new[] { alus, T(2, "Munad L 10tk", 1.99m, 10m, uhik: "tk") }));
    }
}

public class LiitsonaTestid
{
    [Theory]
    [InlineData("toorjuust", "juust", true)]
    [InlineData("leib", "rukkileib", true)]
    [InlineData("piim", "piim", true)]
    [InlineData("juust", "jogurt", false)]
    public void Liitsona_sarnasus(string a, string b, bool oodatud) => Assert.Equal(oodatud, Sarnased.Sarnane(a, b));
}
