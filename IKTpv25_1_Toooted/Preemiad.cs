using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

public enum Tase { Pronks, Hobe, Kuld }

public record TaseInfo(Tase Tase, decimal Protsent, decimal Summa, Tase? Jargmine, decimal Jargmiseni)
{
    public string Nimi => Preemiad.TaseNimi(Tase);
}

public record Valjakutse(string Id, string Ikoon, string Tekst, int Tehtud, int Vaja, decimal Boonus, bool Saadud)
{
    public bool Valmis => Tehtud >= Vaja;
}

public record Kleeps(string Id, string Ikoon, string Nimi, string Kirjeldus, Color Varv, bool Saadud);

// Kliendikaardi mängulised osad: tasemed, nädala väljakutsed, kleebised, päeva üllatus
public static class Preemiad
{
    // ---- Tasemed: viimase 12 kuu ostude summa (ilma tarneta, tagastusteta) ----
    public static readonly (Tase Tase, decimal Alates, decimal Protsent)[] Tasemed =
    {
        (Tase.Pronks, 0m, 0.01m), (Tase.Hobe, 100m, 0.015m), (Tase.Kuld, 300m, 0.02m)
    };

    public const decimal TaisKomplektiKingitus = 2.00m;

    public static string TaseNimi(Tase t) => t switch { Tase.Hobe => "Hõbe", Tase.Kuld => "Kuld", _ => "Pronks" };

    public static TaseInfo TaseSummast(decimal summa)
    {
        int i = Tasemed.Length - 1;
        while (i > 0 && summa < Tasemed[i].Alates) i--;
        var praegu = Tasemed[i];
        if (i == Tasemed.Length - 1) return new TaseInfo(praegu.Tase, praegu.Protsent, summa, null, 0);
        var j = Tasemed[i + 1];
        return new TaseInfo(praegu.Tase, praegu.Protsent, summa, j.Tase, j.Alates - summa);
    }

    public static decimal AastaSumma(ToodedContext db, int klientId)
    {
        var alates = DateTime.Now.AddMonths(-12);
        return db.Ostud.AsNoTracking()
            .Where(o => o.KlientId == klientId && !o.Tagastatud && o.Aeg >= alates)
            .Select(o => new { o.Summa, o.TarneHind })
            .AsEnumerable()
            .Sum(o => o.Summa - o.TarneHind);
    }

    public static TaseInfo LeiaTase(ToodedContext db, int klientId) => TaseSummast(AastaSumma(db, klientId));

    // ---- Nädala väljakutsed (ISO nädal) ----
    public static string NadalaKood(DateTime aeg) => $"{ISOWeek.GetYear(aeg)}-W{ISOWeek.GetWeekOfYear(aeg):00}";

    private static (DateTime Algus, DateTime Lopp) Nadal(DateTime aeg)
    {
        var algus = ISOWeek.ToDateTime(ISOWeek.GetYear(aeg), ISOWeek.GetWeekOfYear(aeg), DayOfWeek.Monday);
        return (algus, algus.AddDays(7));
    }

    public static List<Valjakutse> Valjakutsed(ToodedContext db, int klientId, DateTime aeg)
    {
        var (algus, lopp) = Nadal(aeg);
        var ostud = db.Ostud.AsNoTracking().Include(o => o.Read)
            .Where(o => o.KlientId == klientId && !o.Tagastatud && o.Aeg >= algus && o.Aeg < lopp).ToList();
        var ids = ostud.SelectMany(o => o.Read).Where(r => r.ToodeId != null).Select(r => r.ToodeId!.Value).Distinct().ToList();
        var tooted = db.Toodetabel.AsNoTracking().Where(t => ids.Contains(t.Id)).ToDictionary(t => t.Id);
        int eesti = 0, varske = 0;
        foreach (var r in ostud.SelectMany(o => o.Read))
            if (r.ToodeId is int id && tooted.TryGetValue(id, out var t))
            {
                if ((t.Paritolumaa ?? "").Trim().Equals("Eesti", StringComparison.OrdinalIgnoreCase)) eesti += r.Kogus;
                if (t.KategooriaId == 3) varske += r.Kogus;
            }

        string nadal = NadalaKood(aeg);
        var saadud = db.Saavutused.AsNoTracking().Where(s => s.KlientId == klientId && s.Kood.StartsWith("VK:") && s.Kood.EndsWith(nadal))
            .Select(s => s.Kood).ToHashSet();
        Valjakutse V(string id, string ikoon, string tekst, int tehtud, int vaja, decimal boonus) =>
            new(id, ikoon, tekst, Math.Min(tehtud, vaja), vaja, boonus, saadud.Contains($"VK:{id}:{nadal}"));
        return new List<Valjakutse>
        {
            V("eesti", "🇪🇪", "Eesti nädal: osta 5 Eesti toodet", eesti, 5, 0.50m),
            V("varske", "🥕", "Värske valik: 3 puu- või köögivilja", varske, 3, 0.30m),
            V("kolm", "🛒", "Kolm korda: tee 3 ostu", ostud.Count, 3, 0.50m),
        };
    }

    // ---- Kleebised (püsivad) ----
    public static List<Kleeps> Kleebised(ToodedContext db, int klientId)
    {
        var ostud = db.Ostud.AsNoTracking().Include(o => o.Read).Where(o => o.KlientId == klientId && !o.Tagastatud).ToList();
        var read = ostud.SelectMany(o => o.Read).ToList();
        var ids = read.Where(r => r.ToodeId != null).Select(r => r.ToodeId!.Value).Distinct().ToList();
        var tooted = db.Toodetabel.AsNoTracking().Where(t => ids.Contains(t.Id)).ToDictionary(t => t.Id);
        int eesti = read.Where(r => r.ToodeId is int id && tooted.TryGetValue(id, out var t) && (t.Paritolumaa ?? "").Trim().Equals("Eesti", StringComparison.OrdinalIgnoreCase)).Sum(r => r.Kogus);
        int varske = read.Where(r => r.ToodeId is int id && tooted.TryGetValue(id, out var t) && t.KategooriaId == 3).Sum(r => r.Kogus);
        decimal saastetud = read.Sum(r => r.TavaHind is decimal th && th > r.Hind ? (th - r.Hind) * r.Kogus : 0) + ostud.Sum(o => o.Soodustus);
        int kategooriaid = db.Kategooriatabel.Count();
        int ostetudKat = tooted.Values.Select(t => t.KategooriaId).Distinct().Count();
        bool hommik = ostud.Any(o => o.Aeg.Hour < 9);
        int? kasutajaId = db.Kasutajad.AsNoTracking().Where(k => k.KlientId == klientId).Select(k => (int?)k.Id).FirstOrDefault();
        int pereOstud = kasutajaId is int kid ? db.Ostunimekiri.Count(r => r.Ostetud && r.OstisKasutajaId == kid) : 0;

        return new List<Kleeps>
        {
            new("esimene", "★", "Esimene ost", "Tee esimene ost", Color.FromArgb(226, 6, 19), ostud.Count >= 1),
            new("kymme", "10", "10 ostu", "Tee 10 ostu", Color.FromArgb(160, 35, 110), ostud.Count >= 10),
            new("patrioot", "EE", "Eesti patrioot", "Osta 50 Eesti toodet", Color.FromArgb(0, 114, 206), eesti >= 50),
            new("tervislik", "🍎", "Tervislik", "Osta 20 puu- või köögivilja", Color.FromArgb(20, 130, 70), varske >= 20),
            new("saastja", "€", "Säästja", "Säästa soodustustega 10 €", Color.FromArgb(205, 115, 0), saastetud >= 10),
            new("ring", "◎", "Kogu ring", "Osta igast kategooriast", Color.FromArgb(90, 90, 200), kategooriaid > 0 && ostetudKat >= kategooriaid),
            new("hommik", "☀", "Hommikune", "Tee ost enne kella 9", Color.FromArgb(230, 160, 20), hommik),
            new("pere", "♥", "Pere ostleja", "Osta 10 rida pere nimekirjast", Color.FromArgb(200, 60, 120), pereOstud >= 10),
        };
    }

    // ---- Päeva üllatus: kord päevas juhuslik soodustus ühele tootele (ainult täna, ainult sellele kliendile) ----
    public static (Toode Toode, int Protsent)? TananeYllatus(ToodedContext db, int klientId, DateTime aeg)
    {
        string eesliide = $"YL:{aeg:yyyy-MM-dd}:";
        var kood = db.Saavutused.AsNoTracking().Where(s => s.KlientId == klientId && s.Kood.StartsWith(eesliide)).Select(s => s.Kood).FirstOrDefault();
        if (kood == null) return null;
        var osad = kood.Split(':');
        if (osad.Length < 4 || !int.TryParse(osad[2], out int toodeId) || !int.TryParse(osad[3], out int pr)) return null;
        var toode = db.Toodetabel.AsNoTracking().FirstOrDefault(t => t.Id == toodeId);
        return toode == null ? null : (toode, pr);
    }

    public static (Toode Toode, int Protsent)? AvaYllatus(ToodedContext db, int klientId, DateTime aeg)
    {
        if (TananeYllatus(db, klientId, aeg) is { } olemas) return olemas;
        var valik = db.Toodetabel.AsNoTracking().Where(t => t.Kogus > 0).Select(t => t.Id).ToList();
        if (valik.Count == 0) return null;
        int toodeId = valik[Random.Shared.Next(valik.Count)];
        int pr = new[] { 10, 15, 20, 25 }[Random.Shared.Next(4)];
        db.Saavutused.Add(new Saavutus { KlientId = klientId, Kood = $"YL:{aeg:yyyy-MM-dd}:{toodeId}:{pr}", Aeg = aeg });
        db.SaveChanges();
        return TananeYllatus(db, klientId, aeg);
    }

    // ---- Pärast ostu: uued preemiad (tekstid teavitusteks) ----
    public static List<string> KontrolliPealeOstu(ToodedContext db, Ost ost)
    {
        var uued = new List<string>();
        if (ost.KlientId is not int kid) return uued;
        var enne = TaseSummast(AastaSumma(db, kid) - (ost.Summa - ost.TarneHind));
        var klient = db.Kliendid.Find(kid);
        if (klient == null) return uued;
        var olemas = db.Saavutused.Where(s => s.KlientId == kid).Select(s => s.Kood).ToHashSet();

        void Lisa(string kood, decimal boonus, string tekst)
        {
            if (olemas.Contains(kood)) return;
            db.Saavutused.Add(new Saavutus { KlientId = kid, Kood = kood, Aeg = DateTime.Now, Boonus = boonus });
            klient.Boonus += boonus;
            olemas.Add(kood);
            uued.Add(tekst);
        }

        string nadal = NadalaKood(ost.Aeg);
        foreach (var v in Valjakutsed(db, kid, ost.Aeg).Where(v => v.Valmis))
            Lisa($"VK:{v.Id}:{nadal}", v.Boonus, Keel.F("Väljakutse tehtud: {0} · +{1}", Keel.T(v.Tekst), Ostukorv.Eur(v.Boonus)));

        var kleebised = Kleebised(db, kid);
        foreach (var k in kleebised.Where(k => k.Saadud))
            Lisa($"KL:{k.Id}", 0, Keel.F("Uus kleeps: {0}", Keel.T(k.Nimi)));
        if (kleebised.All(k => k.Saadud))
            Lisa("KL:KOIK", TaisKomplektiKingitus, Keel.F("Kogu kogu! Kõik kleebised koos · +{0}", Ostukorv.Eur(TaisKomplektiKingitus)));

        var parast = LeiaTase(db, kid);
        if (parast.Tase > enne.Tase)
            uued.Add(Keel.F("Uus tase: {0} · {1} %", Keel.T(parast.Nimi), (parast.Protsent * 100).ToString("0.#", new CultureInfo("et-EE"))));

        db.SaveChanges();
        return uued;
    }
}
