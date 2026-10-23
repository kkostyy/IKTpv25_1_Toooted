using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

[Index(nameof(Kaardinumber), IsUnique = true)]   // ühe kaardinumbriga ainult üks klient
public class Klient
{
    public int Id { get; set; }
    public string Nimi { get; set; } = "";
    public string Kaardinumber { get; set; } = "";
    public string? Email { get; set; }              // tšekk saadetakse siia

    [Precision(10, 2)]
    public decimal Boonus { get; set; }             // eurodes; kasutatakse allahindluseks

    public ICollection<Ost> Ostud { get; set; } = new List<Ost>();
}
