using IKTpv25_1_Toooted;
using Xunit;

namespace IKTpv25_1_Toooted.Tests;

public class TarneTestid
{
    [Fact]
    public void Aknad_algavad_vahemalt_kahe_tunni_parast()
    {
        var nyyd = new DateTime(2026, 10, 24, 13, 30, 0);
        var aknad = Tarne.Aknad(nyyd);
        Assert.Equal(new DateTime(2026, 10, 24, 16, 0, 0), aknad[0]);   // 14:00 on liiga vara (30 min)
        Assert.Equal(3 + 6 * 6, aknad.Count);                           // täna 16, 18, 20 + 6 päeva × 6
        Assert.All(aknad, a => Assert.True(a - nyyd >= TimeSpan.FromHours(2)));
    }

    [Fact]
    public void Ohtul_algab_valik_homsest()
    {
        var aknad = Tarne.Aknad(new DateTime(2026, 10, 24, 21, 0, 0));
        Assert.Equal(new DateTime(2026, 10, 25, 10, 0, 0), aknad[0]);
    }

    [Fact]
    public void Taisaken_jaetakse_vahele()
    {
        var nyyd = new DateTime(2026, 10, 24, 7, 0, 0);
        var hoivatud = new Dictionary<DateTime, int>
        {
            [new DateTime(2026, 10, 24, 10, 0, 0)] = Tarne.MaxAknas,
            [new DateTime(2026, 10, 24, 12, 0, 0)] = Tarne.MaxAknas - 1
        };
        Assert.False(Tarne.Vaba(new DateTime(2026, 10, 24, 10, 0, 0), hoivatud));
        Assert.Equal(new DateTime(2026, 10, 24, 12, 0, 0), Tarne.EsimeneVaba(nyyd, hoivatud));
    }

    [Fact]
    public void Akna_tekst()
    {
        var tana = new DateTime(2026, 10, 24);
        Assert.Equal("Homme 25.10 · 18–20", Tarne.AknaTekst(new DateTime(2026, 10, 25, 18, 0, 0), tana));
        Assert.Equal("Täna 24.10 · 10–12", Tarne.AknaTekst(new DateTime(2026, 10, 24, 10, 0, 0), tana));
    }
}
