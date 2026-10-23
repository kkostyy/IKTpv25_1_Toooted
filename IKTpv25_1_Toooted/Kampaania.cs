using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Ajaline kampaania ühele tootele: alguses hind langeb protsendi võrra, lõpus tuleb tagasi
public class Kampaania
{
    public int Id { get; set; }
    public string Nimi { get; set; } = "";
    public int ToodeId { get; set; }
    public Toode Toode { get; set; } = null!;
    public int SoodusProtsent { get; set; }          // 1–90
    public DateTime Algus { get; set; }
    public DateTime Lopp { get; set; }
    public bool Automaatne { get; set; } = true;     // hind muutub ise alguses ja lõpus
    public bool Rakendatud { get; set; }             // soodushind on praegu tootel

    [Precision(10, 2)]
    public decimal? TavaHind { get; set; }           // hind enne kampaaniat
}
