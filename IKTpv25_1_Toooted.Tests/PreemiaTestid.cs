using IKTpv25_1_Toooted;
using Xunit;

namespace IKTpv25_1_Toooted.Tests;

public class PreemiaTestid
{
    [Theory]
    [InlineData(0, Tase.Pronks)]
    [InlineData(99.99, Tase.Pronks)]
    [InlineData(100, Tase.Hobe)]
    [InlineData(299.99, Tase.Hobe)]
    [InlineData(300, Tase.Kuld)]
    [InlineData(5000, Tase.Kuld)]
    public void Tase_aasta_summa_jargi(double summa, Tase oodatud)
    {
        Assert.Equal(oodatud, Preemiad.TaseSummast((decimal)summa).Tase);
    }

    [Fact]
    public void Jargmise_tasemeni_jaanud_summa()
    {
        var t = Preemiad.TaseSummast(80m);
        Assert.Equal(Tase.Hobe, t.Jargmine);
        Assert.Equal(20m, t.Jargmiseni);
        Assert.Null(Preemiad.TaseSummast(400m).Jargmine);
    }

    [Fact]
    public void Kuldtasemel_on_suurim_boonus()
    {
        Assert.True(Preemiad.TaseSummast(300m).Protsent > Preemiad.TaseSummast(100m).Protsent);
        Assert.True(Preemiad.TaseSummast(100m).Protsent > Preemiad.TaseSummast(0m).Protsent);
    }

    [Fact]
    public void Nadala_kood_on_ISO_nadal()
    {
        Assert.Equal("2026-W01", Preemiad.NadalaKood(new DateTime(2026, 1, 1)));
        Assert.Equal("2026-W53", Preemiad.NadalaKood(new DateTime(2027, 1, 1)));
    }
}
