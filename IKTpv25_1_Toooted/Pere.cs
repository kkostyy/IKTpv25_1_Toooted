using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Pere ühine ostunimekiri: sama PereKood-iga kasutajad näevad ja muudavad sama nimekirja.
// Kõik on samas (kohalikus) andmebaasis – "pere" = selle programmi kasutajakontod.
public static class Pere
{
    private const string Margid = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";   // ilma 0/O ja 1/I
    public const int KoodiPikkus = 7;

    public static string? MinuKood(ToodedContext db) =>
        db.Kasutajad.AsNoTracking().Where(k => k.Id == AktiivneKasutaja.Id).Select(k => k.PereKood).FirstOrDefault();

    // Kõik kasutajad, kelle nimekirja näen (pere või ainult mina)
    public static List<int> NahtavadKasutajad(ToodedContext db)
    {
        string? kood = MinuKood(db);
        if (string.IsNullOrEmpty(kood)) return new List<int> { AktiivneKasutaja.Id };
        var ids = db.Kasutajad.AsNoTracking().Where(k => k.PereKood == kood).Select(k => k.Id).ToList();
        if (!ids.Contains(AktiivneKasutaja.Id)) ids.Add(AktiivneKasutaja.Id);
        return ids;
    }

    public static List<Kasutaja> Liikmed(ToodedContext db)
    {
        string? kood = MinuKood(db);
        if (string.IsNullOrEmpty(kood)) return new List<Kasutaja>();
        return db.Kasutajad.AsNoTracking().Where(k => k.PereKood == kood).OrderBy(k => k.Id).ToList();
    }

    public static string LooKood(ToodedContext db)
    {
        var juhus = Random.Shared;
        string kood;
        do kood = new string(Enumerable.Range(0, KoodiPikkus).Select(_ => Margid[juhus.Next(Margid.Length)]).ToArray());
        while (db.Kasutajad.Any(k => k.PereKood == kood));
        return kood;
    }

    // Uus pere: kasutaja saab uue koodi
    public static string LooPere()
    {
        using var db = new ToodedContext();
        var k = db.Kasutajad.Find(AktiivneKasutaja.Id) ?? throw new InvalidOperationException(Keel.T("Kasutajat ei leitud."));
        k.PereKood = LooKood(db);
        db.SaveChanges();
        Ostunimekiri.TeavitaMuutusest();
        return k.PereKood;
    }

    // null = korras, muidu veateade
    public static string? Liitu(string kood)
    {
        kood = new string((kood ?? "").ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        if (kood.Length != KoodiPikkus) return Keel.F("Pere kood on {0} märki.", KoodiPikkus);
        using var db = new ToodedContext();
        if (!db.Kasutajad.Any(k => k.PereKood == kood && k.Id != AktiivneKasutaja.Id)) return Keel.T("Sellise koodiga peret ei leitud.");
        var mina = db.Kasutajad.Find(AktiivneKasutaja.Id);
        if (mina == null) return Keel.T("Kasutajat ei leitud.");
        mina.PereKood = kood;
        db.SaveChanges();
        Ostunimekiri.TeavitaMuutusest();
        return null;
    }

    public static void Lahku()
    {
        using var db = new ToodedContext();
        var mina = db.Kasutajad.Find(AktiivneKasutaja.Id);
        if (mina == null) return;
        mina.PereKood = null;
        db.SaveChanges();
        Ostunimekiri.TeavitaMuutusest();
    }

    public static string Nimi(Kasutaja k) => string.IsNullOrWhiteSpace(k.Nimi) ? k.Kasutajanimi : k.Nimi!.Split(' ')[0];

    public static string Initsiaalid(string nimi)
    {
        var osad = nimi.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return osad.Length switch
        {
            0 => "?",
            1 => osad[0][..Math.Min(2, osad[0].Length)].ToUpper(),
            _ => $"{char.ToUpper(osad[0][0])}{char.ToUpper(osad[^1][0])}"
        };
    }

    private static readonly Color[] AvatariVarvid =
    {
        Color.FromArgb(226, 6, 19), Color.FromArgb(160, 35, 110), Color.FromArgb(20, 130, 70),
        Color.FromArgb(205, 115, 0), Color.FromArgb(40, 110, 200), Color.FromArgb(90, 90, 100)
    };

    public static Color AvatariVarv(int kasutajaId) => AvatariVarvid[Math.Abs(kasutajaId) % AvatariVarvid.Length];
}
