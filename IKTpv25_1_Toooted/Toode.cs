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

    // Dieedimärgised (admin): "laktoosivaba;gluteenivaba;vegan;pahklivaba"
    public string? Margised { get; set; }

    [Precision(10, 2)]
    public decimal Pant { get; set; }               // pakendi tagatisraha (pudel/purk 0,10 €)

    [Precision(10, 2)]
    public decimal? Ostuhind { get; set; }          // sisseostuhind marginaali jaoks

    public bool Demo { get; set; }                  // demoandmed (saab korraga kustutada)

    // Arvutatud (andmebaasi ei salvestata)
    public decimal? UhikuHind => UhikuKogus is decimal k && k > 0 ? Math.Round(Hind / k, 2) : null;
    public int? SoodusProtsent => VanaHind is decimal v && v > Hind ? (int)Math.Round((1 - Hind / v) * 100) : null;
}

public static class TooteParingud
{
    // Ilma pikkade tekstideta (kirjeldus, koostis, lisainfo, toiteväärtus): 9000 toote laadimine on mitu korda kiirem.
    // Ainult seal, kus neid välju ega Kategooria objekti ei kasutata (müügipaneel, autotellimus).
    public static IQueryable<Toode> Kerged(this IQueryable<Toode> q) => q.Select(t => new Toode
    {
        Id = t.Id, Toodenimetus = t.Toodenimetus, Kogus = t.Kogus, Hind = t.Hind, VanaHind = t.VanaHind, Pilt = t.Pilt,
        KategooriaId = t.KategooriaId, Kaubamark = t.Kaubamark, Tootja = t.Tootja, Paritolumaa = t.Paritolumaa,
        Pakend = t.Pakend, Uhik = t.Uhik, UhikuKogus = t.UhikuKogus, Margised = t.Margised, Pant = t.Pant,
        Ostuhind = t.Ostuhind, Demo = t.Demo
    });
}
