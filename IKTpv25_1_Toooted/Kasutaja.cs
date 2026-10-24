using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Rollid: "Omanik" näeb vahelehti Tooted ja Kassa, "Müüja" ainult Kassa,
// "Ostja" (klient) näeb ainult e-poodi; tal on alati kliendikaart.
[Index(nameof(Kasutajanimi), IsUnique = true)]   // sama nimega kahte kasutajat ei saa olla
[Index(nameof(PereKood))]
public class Kasutaja
{
    public const string Omanik = "Omanik";
    public const string Myyja = "Müüja";
    public const string Ostja = "Ostja";

    public int Id { get; set; }
    public string Kasutajanimi { get; set; } = "";   // hoitakse väiketähtedega
    public string ParoolHash { get; set; } = "";     // BCrypti räsi, mitte parool ise
    public string Roll { get; set; } = Myyja;

    public string? Nimi { get; set; }                // ees- ja perekonnanimi
    public string? Email { get; set; }               // tšekk saadetakse vaikimisi siia
    public string? Aadress { get; set; }             // kojuveo aadress (täidetakse maksmisel ise)
    public bool TsekkEmailile { get; set; } = true;  // "Soovin ostutšekke e-postile"

    // Kasutaja kliendikaart (ostjal luuakse registreerimisel automaatselt)
    public int? KlientId { get; set; }

    // Pere ühine ostunimekiri: sama koodiga kasutajad näevad sama nimekirja
    public string? PereKood { get; set; }
    public Klient? Klient { get; set; }
}
