using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Partiid ja säilivusaeg (omanik): ajatelg 14 päeva, partiide tabel
// (roheline / kollane ≤ 2 päeva / punane aegunud), uus partii, mahakandmine.
public partial class AegumineVorm : KujundusVorm
{
    private List<Partii> partiid = new();

    protected override bool TagasiNupuga => false;   // Tagasi on valge kaardi sees

    public AegumineVorm() : base("Partiid ja säilivusaeg", 1320, 800)
    {
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(960, 600);
        BackColor = Teema.Taust;
        EhitaMoodne();
        Load += (s, e) => LaadiMoodne();
    }

    public static Color Varv(Partii p)
    {
        double paevi = (p.Aegub.Date - DateTime.Today).TotalDays;
        return paevi < 0 ? Teema.Viga : paevi <= Aegumine.PaeviEnne ? Teema.Hoiatus : Teema.Edu;
    }
}
