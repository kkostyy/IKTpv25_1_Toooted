using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

public class Toode
{
    public int Id { get; set; }
    public string Toodenimetus { get; set; } = "";
    public int Kogus { get; set; }                  // laoseis

    [Precision(10, 2)]
    public decimal Hind { get; set; }

    [Precision(10, 2)]
    public decimal? VanaHind { get; set; }          // kui on, näidatakse läbikriipsutatult (soodushind)

    public string? Pilt { get; set; }               // ? = pilt ei ole kohustuslik

    public int KategooriaId { get; set; }
    public Kategooria Kategooria { get; set; } = null!;

    // ---- Toote andmed (toote lehel "Tooteinfo") ----
    public string? Kaubamark { get; set; }
    public string? Tootja { get; set; }
    public string? Paritolumaa { get; set; }
    public string? Pakend { get; set; }             // nt "0.5 kg", "2 l", "10 tk"
    public string? Uhik { get; set; }               // kg / l / tk – ühikuhinna jaoks

    [Precision(10, 3)]
    public decimal? UhikuKogus { get; set; }        // nt 0.5 (kg) → ühikuhind = Hind / 0.5

    public string? Kirjeldus { get; set; }
    public string? Koostisosad { get; set; }
    public string? Lisainfo { get; set; }
    // Toitumisalane teave: iga rida "nimetus;väärtus", alamrida algab "-"-ga
    public string? Toitevaartus { get; set; }

    // Arvutatud (andmebaasi ei salvestata)
    public decimal? UhikuHind => UhikuKogus is decimal k && k > 0 ? Math.Round(Hind / k, 2) : null;
    public int? SoodusProtsent => VanaHind is decimal v && v > Hind ? (int)Math.Round((1 - Hind / v) * 100) : null;
}
