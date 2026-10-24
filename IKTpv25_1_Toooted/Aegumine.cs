using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Säilivusaeg: partii, mis aegub 2 päeva jooksul, saab −30 %; aegunud partii kantakse maha.
// Müük käib FEFO järgi (enne läheb partii, mis aegub varem).
public static class Aegumine
{
    public const int PaeviEnne = 2;
    public const int Protsent = 30;

    // toote Id → varaseima allahinnatud partii aegumise kuupäev (Hinnad loeb siit)
    private static Dictionary<int, DateTime> allahinnatud = new();
    private static DateTime viimati = DateTime.MinValue;

    public static DateTime? AllahindlusKuni(int toodeId)
    {
        if (DateTime.Now - viimati > TimeSpan.FromHours(1)) Uuenda();
        return allahinnatud.TryGetValue(toodeId, out var a) ? a : null;
    }

    public static void Uuenda()
    {
        try
        {
            using var db = new ToodedContext();
            Uuenda(db, DateTime.Now);
        }
        catch { viimati = DateTime.Now; }
    }

    // Tagastab maha kantud kogus
    public static int Uuenda(ToodedContext db, DateTime nyyd)
    {
        viimati = nyyd;
        var tana = nyyd.Date;
        var partiid = db.Partiid.Where(p => p.Kogus > 0).ToList();
        int maha = 0;
        foreach (var p in partiid.Where(p => p.Aegub.Date < tana))
        {
            p.MahaKantud += p.Kogus;
            maha += p.Kogus;
            p.Kogus = 0;
            p.MahaKantudAeg = nyyd;
        }
        if (maha > 0)
        {
            db.SaveChanges();
            SobitaLaoseis(db, partiid.Where(p => p.MahaKantudAeg == nyyd).Select(p => p.ToodeId).Distinct());
        }

        allahinnatud = partiid.Where(p => p.Kogus > 0 && p.Aegub.Date >= tana && (p.Aegub.Date - tana).TotalDays <= PaeviEnne)
            .GroupBy(p => p.ToodeId)
            .ToDictionary(g => g.Key, g => g.Min(p => p.Aegub));
        return maha;
    }

    // Toote laoseis = partiide summa (kui tootel on partiisid)
    public static void SobitaLaoseis(ToodedContext db, IEnumerable<int> tooteIdd)
    {
        foreach (int id in tooteIdd.ToList())
        {
            var t = db.Toodetabel.Find(id);
            if (t == null || !db.Partiid.Any(p => p.ToodeId == id)) continue;
            t.Kogus = db.Partiid.Where(p => p.ToodeId == id).Sum(p => p.Kogus);
        }
        db.SaveChanges();
    }

    public static Partii LisaPartii(ToodedContext db, int toodeId, int kogus, DateTime aegub)
    {
        var t = db.Toodetabel.Find(toodeId) ?? throw new InvalidOperationException(Keel.T("Toodet ei leitud."));
        // Esimene partii: senine laoseis jääb eraldi partiina alles (ilma kindla kuupäevata = kaugel tulevikus)
        if (!db.Partiid.Any(p => p.ToodeId == toodeId) && t.Kogus > 0)
            db.Partiid.Add(new Partii { ToodeId = toodeId, Kogus = t.Kogus, Aegub = DateTime.Today.AddYears(1) });
        var uus = new Partii { ToodeId = toodeId, Kogus = kogus, Aegub = aegub.Date, Lisatud = DateTime.Now };
        db.Partiid.Add(uus);
        db.SaveChanges();
        SobitaLaoseis(db, new[] { toodeId });
        Uuenda(db, DateTime.Now);
        return uus;
    }

    // Müük: kogus võetakse kõige varem aeguvatest partiidest (FEFO). Salvestamine jääb kutsujale.
    public static void VotaPartiidest(ToodedContext db, int toodeId, int kogus)
    {
        foreach (var p in db.Partiid.Where(p => p.ToodeId == toodeId && p.Kogus > 0).OrderBy(p => p.Aegub).ToList())
        {
            if (kogus <= 0) break;
            int v = Math.Min(kogus, p.Kogus);
            p.Kogus -= v;
            kogus -= v;
        }
    }

    // Käsitsi mahakandmine (aegunud või rikutud partii)
    public static void KannaMaha(ToodedContext db, int partiiId)
    {
        var p = db.Partiid.Find(partiiId);
        if (p == null || p.Kogus <= 0) return;
        p.MahaKantud += p.Kogus;
        p.Kogus = 0;
        p.MahaKantudAeg = DateTime.Now;
        db.SaveChanges();
        SobitaLaoseis(db, new[] { p.ToodeId });
        Uuenda(db, DateTime.Now);
    }
}
