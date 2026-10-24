using IKTpv25_1_Toooted;
using Xunit;

namespace IKTpv25_1_Toooted.Tests;

// Ostukorv on staatiline: kõik selle testid on ühes klassis (xUnit käivitab need järjest)
public class OstukorviTestid
{
    public OstukorviTestid() => Ostukorv.Tyhjenda();

    [Fact]
    public void Lisamine_ja_summa()
    {
        Assert.Null(Ostukorv.Lisa(1, "Piim", 1.39m, laos: 10, kogus: 2));
        Assert.Null(Ostukorv.Lisa(2, "Leib", 1.69m, laos: 5));
        Assert.Equal(2, Ostukorv.Ridad.Count);
        Assert.Equal(3, Ostukorv.Kogus);
        Assert.Equal(4.47m, Ostukorv.Kokku);
    }

    [Fact]
    public void Sama_toode_suurendab_kogust()
    {
        Ostukorv.Lisa(1, "Piim", 1.39m, laos: 10);
        Ostukorv.Lisa(1, "Piim", 1.39m, laos: 10, kogus: 3);
        Assert.Single(Ostukorv.Ridad);
        Assert.Equal(4, Ostukorv.KorvisKogus(1));
    }

    [Fact]
    public void Laoseisust_rohkem_ei_saa_lisada()
    {
        Assert.Null(Ostukorv.Lisa(1, "Piim", 1.39m, laos: 2, kogus: 2));
        Assert.NotNull(Ostukorv.Lisa(1, "Piim", 1.39m, laos: 2));
        Assert.Equal(2, Ostukorv.KorvisKogus(1));
    }

    [Fact]
    public void Vahendamine_eemaldab_viimase_tuki()
    {
        Ostukorv.Lisa(1, "Piim", 1.39m, laos: 10, kogus: 2);
        var rida = Ostukorv.Ridad[0];
        Ostukorv.Vahenda(rida);
        Assert.Equal(1, rida.Kogus);
        Ostukorv.Vahenda(rida);
        Assert.Empty(Ostukorv.Ridad);
    }

    [Fact]
    public void Pant_ei_kuulu_kaupade_summasse()
    {
        Ostukorv.Lisa(3, "Limonaad", 1.00m, laos: 10, kogus: 3, pant: 0.10m);
        Assert.Equal(3.00m, Ostukorv.Kokku);
        Assert.Equal(0.30m, Ostukorv.PantKokku);
        Assert.Equal(3, Ostukorv.PandiTk);
    }

    [Fact]
    public void Muutuse_syndmus_kaivitub()
    {
        int kordi = 0;
        void Loe() => kordi++;
        Ostukorv.Muutus += Loe;
        try
        {
            Ostukorv.Lisa(1, "Piim", 1.39m, laos: 10);
            Ostukorv.Vahenda(Ostukorv.Ridad[0]);
        }
        finally { Ostukorv.Muutus -= Loe; }
        Assert.Equal(2, kordi);
    }
}
