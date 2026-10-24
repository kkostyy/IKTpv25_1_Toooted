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
