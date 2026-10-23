using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Üks tehtud ost (tšekk). Vajalik arve ja statistika jaoks.
public class Ost
{
    public int Id { get; set; }
    public DateTime Aeg { get; set; }

    [Precision(10, 2)] public decimal Vahesumma { get; set; }       // ostukorvi summa
    [Precision(10, 2)] public decimal Soodustus { get; set; }       // boonusest kasutatud
    [Precision(10, 2)] public decimal Summa { get; set; }           // makstav summa
    [Precision(10, 2)] public decimal BoonusLisatud { get; set; }   // kliendile juurde

    public string Kasutaja { get; set; } = "";                      // kes müüs
    public string? Email { get; set; }                              // kuhu tšekk saadetakse

    public bool Makstud { get; set; }
    [Precision(10, 2)] public decimal MakstudSumma { get; set; }
    public bool Tagastatud { get; set; }                            // ost tühistati, kaup tagasi lattu

    public string? Makseviis { get; set; }                          // "Kaart" / "Sularaha"
    public string? Tarneviis { get; set; }                          // "Kaasa" (poest) / "Kojuvedu"
    [Precision(10, 2)] public decimal TarneHind { get; set; }       // 0 = tasuta
    public string? Aadress { get; set; }                            // kojuveo aadress

    public int? KlientId { get; set; }                              // ? = klient ei ole kohustuslik
    public Klient? Klient { get; set; }

    public List<OstuRida> Read { get; set; } = new();
}

public class OstuRida
{
    public int Id { get; set; }
    public int OstId { get; set; }
    public Ost Ost { get; set; } = null!;

    public int? ToodeId { get; set; }                               // null, kui toode on kustutatud

    // Toote nimi ja hind salvestatakse ostu hetkel (toode võidakse hiljem muuta või kustutada)
    public string Toodenimetus { get; set; } = "";
    public int Kogus { get; set; }

    [Precision(10, 2)]
    public decimal Hind { get; set; }

    public decimal Summa => Hind * Kogus;
}
