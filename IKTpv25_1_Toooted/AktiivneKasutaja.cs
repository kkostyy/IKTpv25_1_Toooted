using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Sisseloginud kasutaja andmed, kättesaadavad igast vormist
public static class AktiivneKasutaja
{
    public static int Id { get; set; }
    public static string Nimi { get; set; } = "";        // kasutajanimi (sisselogimiseks)
    public static string Roll { get; set; } = "";
    public static string? TaisNimi { get; set; }
    public static string? Email { get; set; }
    public static string? Aadress { get; set; }
    public static bool TsekkEmailile { get; set; } = true;
    public static int? KlientId { get; set; }
    public static string? Kaardinumber { get; set; }
    public static decimal Boonus { get; set; }

    public static bool OnOmanik => Roll == Kasutaja.Omanik;
    public static bool OnOstja => Roll == Kasutaja.Ostja;

    // Tervitustes: päris nimi, kui see on olemas
    public static string Kuvanimi => string.IsNullOrWhiteSpace(TaisNimi) ? Nimi : TaisNimi!;

    public static void Sea(Kasutaja k)
    {
        Id = k.Id;
        Nimi = k.Kasutajanimi;
        Roll = k.Roll;
        TaisNimi = k.Nimi;
        Email = k.Email;
        Aadress = k.Aadress;
        TsekkEmailile = k.TsekkEmailile;
        KlientId = k.KlientId;
        Kaardinumber = k.Klient?.Kaardinumber;
        Boonus = k.Klient?.Boonus ?? 0;
    }

    // Loeb andmed andmebaasist uuesti (nt pärast ostu boonus muutus)
    public static void Uuenda()
    {
        if (Id == 0) return;
        try
        {
            using var db = new ToodedContext();
            var k = db.Kasutajad.AsNoTracking().Include(x => x.Klient).FirstOrDefault(x => x.Id == Id);
            if (k != null) Sea(k);
        }
        catch { }
    }
}

// Kliendikaardi numbrid: järgmine vaba 6-kohaline number
public static class Kliendikaart
{
    public static string UusNumber(ToodedContext db)
    {
        var numbrid = db.Kliendid.Select(k => k.Kaardinumber).ToList();
        int suurim = numbrid.Select(n => int.TryParse(n, out int x) ? x : 0).DefaultIfEmpty(100000).Max();
        return Math.Max(suurim + 1, 100001).ToString();
    }

    // Loob uue kliendi koos kaardiga (salvestamine jääb kutsujale)
    public static Klient Loo(ToodedContext db, string nimi, string? email)
    {
        var k = new Klient { Nimi = nimi, Kaardinumber = UusNumber(db), Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim() };
        db.Kliendid.Add(k);
        return k;
    }
}
