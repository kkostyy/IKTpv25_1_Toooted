using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Kliendi saavutus: nädala ülesanne, kleeps, päeva üllatus
public class Saavutus
{
    public int Id { get; set; }
    public int KlientId { get; set; }
    public Klient Klient { get; set; } = null!;
    public string Kood { get; set; } = "";           // nt "VK:eesti:2026-W43", "KL:esimene", "YL:2026-10-23:5:25"
    public DateTime Aeg { get; set; }

    [Precision(10, 2)]
    public decimal Boonus { get; set; }
}
