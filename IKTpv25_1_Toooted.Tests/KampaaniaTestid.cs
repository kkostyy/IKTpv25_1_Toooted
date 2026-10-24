using IKTpv25_1_Toooted;
using Xunit;

namespace IKTpv25_1_Toooted.Tests;

public class KampaaniaTestid
{
    [Theory]
    [InlineData(1.69, 20, 1.35)]
    [InlineData(10.00, 50, 5.00)]
    [InlineData(0.99, 10, 0.89)]    // 0,891 → 0,89
    [InlineData(1.25, 10, 1.13)]    // 1,125 → 1,13 (pool ümardub üles)
    public void Soodushind_umardatakse_sendini(double tava, int protsent, double oodatud)
    {
        Assert.Equal((decimal)oodatud, Kampaaniad.SoodusHind((decimal)tava, protsent));
    }

    [Fact]
    public void Tavahind_on_labikriipsutatud_hind()
    {
        Assert.Equal(3.49m, Kampaaniad.TavaHind(new Toode { Hind = 1.49m, VanaHind = 3.49m }));
        Assert.Equal(1.49m, Kampaaniad.TavaHind(new Toode { Hind = 1.49m }));
    }

    [Fact]
    public void Kampaania_olek_aja_jargi()
    {
        var k = new Kampaania { Algus = new DateTime(2026, 5, 1), Lopp = new DateTime(2026, 5, 10) };
        Assert.Equal(KampaaniaOlek.Tulekul, Kampaaniad.Olek(k, new DateTime(2026, 4, 30)));
        Assert.Equal(KampaaniaOlek.Kaib, Kampaaniad.Olek(k, new DateTime(2026, 5, 5)));
        Assert.Equal(KampaaniaOlek.Loppenud, Kampaaniad.Olek(k, new DateTime(2026, 5, 11)));
    }
}
