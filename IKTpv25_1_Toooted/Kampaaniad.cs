using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

public enum KampaaniaOlek { Kaib, Tulekul, Loppenud }

// Kampaaniate loogika: alguses langeb toote hind, lõpus tuleb tagasi (vt Kampaania)
public static class Kampaaniad
{
    public static KampaaniaOlek Olek(Kampaania k, DateTime nyyd) =>
        nyyd < k.Algus ? KampaaniaOlek.Tulekul : nyyd > k.Lopp ? KampaaniaOlek.Loppenud : KampaaniaOlek.Kaib;

    public static string OlekuNimi(KampaaniaOlek o) => o switch
    {
        KampaaniaOlek.Kaib => "Käib",
        KampaaniaOlek.Tulekul => "Tulekul",
        _ => "Lõppenud"
    };

    // Toote tavahind (ilma soodustuseta): läbikriipsutatud hind või hind ise
    public static decimal TavaHind(Toode t) => t.VanaHind is decimal v && v > t.Hind ? v : t.Hind;

    public static decimal SoodusHind(decimal tavaHind, int protsent) =>
        Math.Round(tavaHind * (100 - protsent) / 100m, 2, MidpointRounding.AwayFromZero);

    // Lõppenud kampaaniate hind tagasi, alanud kampaaniate soodushind peale
    public static int Uuenda(ToodedContext db, DateTime nyyd)
    {
        int muudetud = 0;
        var koik = db.Kampaaniad.Include(k => k.Toode).ToList();

        foreach (var k in koik.Where(k => k.Rakendatud && (nyyd > k.Lopp || nyyd < k.Algus)))
        {
            Taasta(k);
            muudetud++;
        }
        foreach (var k in koik.Where(k => !k.Rakendatud && k.Automaatne && k.Algus <= nyyd && nyyd <= k.Lopp))
        {
            Rakenda(k);
            muudetud++;
        }
        if (muudetud > 0) db.SaveChanges();
        return muudetud;
    }

    // Lühike variant (käivitamisel, poe avamisel): vead ei takista programmi tööd
    public static void Uuenda()
    {
        try
        {
            using var db = new ToodedContext();
            Uuenda(db, DateTime.Now);
        }
        catch { }
    }

    private static void Rakenda(Kampaania k)
    {
        var t = k.Toode;
        decimal tava = TavaHind(t);
        k.TavaHind = t.Hind;                       // taastamiseks
        t.VanaHind = tava;                         // läbikriipsutatud
        t.Hind = Math.Min(t.Hind, SoodusHind(tava, k.SoodusProtsent));
        k.Rakendatud = true;
    }

    public static void Taasta(Kampaania k)
    {
        var t = k.Toode;
        if (k.TavaHind is decimal h)
        {
            t.Hind = h;
            if (t.VanaHind == h) t.VanaHind = null;   // enne kampaaniat soodustust polnud
        }
        k.Rakendatud = false;
    }

    // Kontroll enne salvestamist: null = korras, muidu veateade
    public static string? Kontrolli(ToodedContext db, Kampaania k)
    {
        if (string.IsNullOrWhiteSpace(k.Nimi)) return Keel.T("Sisesta kampaania nimi.");
        if (k.SoodusProtsent < 1 || k.SoodusProtsent > 90) return Keel.T("Soodustus peab olema 1–90 %.");
        if (k.Lopp <= k.Algus) return Keel.T("Lõpp peab olema pärast algust.");
        bool kattub = db.Kampaaniad.AsNoTracking()
            .Any(x => x.Id != k.Id && x.ToodeId == k.ToodeId && x.Algus < k.Lopp && k.Algus < x.Lopp);
        return kattub ? Keel.T("Sellel tootel on samal ajal juba kampaania.") : null;
    }

    // "Lõpeta kohe": lõpp praegu, hind tuleb tagasi
    public static void Lopeta(ToodedContext db, Kampaania k)
    {
        k.Lopp = DateTime.Now;
        if (k.Algus > k.Lopp) k.Algus = k.Lopp.AddMinutes(-1);
        if (k.Rakendatud) Taasta(k);
        db.SaveChanges();
    }

    public static void Kustuta(ToodedContext db, Kampaania k)
    {
        if (k.Rakendatud) Taasta(k);
        db.Kampaaniad.Remove(k);
        db.SaveChanges();
    }

    // Muudetud kampaania: vana soodustus maha, uus vajadusel peale
    public static void Salvesta(ToodedContext db, Kampaania k)
    {
        if (k.Rakendatud) Taasta(k);
        if (k.Id == 0) db.Kampaaniad.Add(k);
        db.SaveChanges();
        Uuenda(db, DateTime.Now);
    }
}
