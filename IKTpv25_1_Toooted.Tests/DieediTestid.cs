using IKTpv25_1_Toooted;
using Xunit;

namespace IKTpv25_1_Toooted.Tests;

public class DieediTestid
{
    private static Toode Toode(string? koostis, string? lisainfo = null, string? margised = null, string nimi = "Toode") =>
        new() { Toodenimetus = nimi, Koostisosad = koostis, Lisainfo = lisainfo, Margised = margised, KategooriaId = 1 };

    [Fact]
    public void Margid_loetakse_ja_kirjutatakse()
    {
        var m = Dieet.LoeMargid("Laktoosivaba; vegan;pähklivaba");
        Assert.Equal(DieediMark.Laktoosivaba | DieediMark.Vegan | DieediMark.Pahklivaba, m);
        Assert.Equal("laktoosivaba;vegan;pahklivaba", Dieet.KirjutaMargid(m));
    }

    [Fact]
    public void Piimaga_toode_ei_ole_laktoosivaba()
    {
        var t = Toode("täisPIIM, suhkur");
        Assert.False(Dieet.Sobib(t, DieediMark.Laktoosivaba, range: false));
        Assert.True(Dieet.Sobib(t, DieediMark.Gluteenivaba, range: false));
    }

    [Fact]
    public void Tundmatu_koostisega_toode_ei_sobi()
    {
        Assert.False(Dieet.Sobib(Toode(null), DieediMark.Vegan, range: false));
        Assert.True(Dieet.Sobib(Toode(null), DieediMark.Puudub, range: false));
    }

    [Fact]
    public void Admini_mark_on_tahtsam_kui_koostis()
    {
        Assert.True(Dieet.Sobib(Toode(null, margised: "gluteenivaba"), DieediMark.Gluteenivaba, range: false));
    }

    [Fact]
    public void Range_reziim_arvestab_voib_sisaldada()
    {
        var t = Toode("kakaovõi, suhkur", lisainfo: "Võib sisaldada sarapuupähkli jälgi.");
        Assert.True(Dieet.Sobib(t, DieediMark.Pahklivaba, range: false));
        Assert.False(Dieet.Sobib(t, DieediMark.Pahklivaba, range: true));
    }
}
