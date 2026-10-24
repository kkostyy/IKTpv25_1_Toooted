using IKTpv25_1_Toooted;
using Xunit;

namespace IKTpv25_1_Toooted.Tests;

public class ParooliTestid
{
    [Fact]
    public void Rasi_kontrollib_oiget_parooli()
    {
        string rasi = PasswordHelper.Hash("admin123");
        Assert.NotEqual("admin123", rasi);
        Assert.True(PasswordHelper.Verify("admin123", rasi));
        Assert.False(PasswordHelper.Verify("vale", rasi));
    }

    [Fact]
    public void Sama_parool_annab_eri_rasi()
    {
        Assert.NotEqual(PasswordHelper.Hash("x"), PasswordHelper.Hash("x"));
    }
}
