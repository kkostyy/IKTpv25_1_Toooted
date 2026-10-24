using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Pere ühine ostukorv: sama PereKood-iga kasutajad näevad, kes mida lisas.
// Andmebaasis (tabel PereKorv); avatud aknad kontrollivad muudatusi iga 2 sekundi järel ("reaalajas").
public static class PereKorv
{
    public static event Action? Muutus;
    private static System.Windows.Forms.Timer? taimer;
    private static string allkiri = "";
    private static int viimaneId;

    public static string? Kood { get; private set; }
    public static List<PereKorviRida> Read { get; private set; } = new();

    // Käivitab jälgimise (poe avamisel); kood loetakse kasutajalt
    public static void Alusta()
    {
        Varskenda(teavita: false);
        if (taimer != null) return;
        taimer = new System.Windows.Forms.Timer { Interval = 2000 };
        taimer.Tick += (s, e) => Varskenda(teavita: true);
        taimer.Start();
    }

    public static void Peata()
    {
        taimer?.Stop();
        taimer?.Dispose();
        taimer = null;
    }

    public static void Varskenda(bool teavita)
    {
        try
        {
            using var db = new ToodedContext();
            Kood = Pere.MinuKood(db);
            var uued = string.IsNullOrEmpty(Kood) ? new List<PereKorviRida>()
                : db.PereKorv.AsNoTracking().Include(r => r.Toode).ThenInclude(t => t.Kategooria)
                    .Where(r => r.PereKood == Kood).OrderBy(r => r.Aeg).ToList();
            string uus = string.Join(";", uued.Select(r => $"{r.Id}:{r.Kogus}"));
            if (uus == allkiri) return;
            // teised pereliikmed lisasid midagi → teade
            if (teavita)
                foreach (var r in uued.Where(r => r.Id > viimaneId && r.KasutajaId != AktiivneKasutaja.Id))
                    Teade.Toast(Keel.F("{0} lisas pere korvi: {1}", r.Lisaja, r.Toode.Toodenimetus), TeateTyyp.Info, 3500);
            allkiri = uus;
            viimaneId = uued.Count > 0 ? Math.Max(viimaneId, uued.Max(r => r.Id)) : viimaneId;
            Read = uued;
            Muutus?.Invoke();
        }
        catch { /* andmebaas hõivatud – järgmine kord uuesti */ }
    }

    public static string? Lisa(Toode t, int kogus = 1)
    {
        if (string.IsNullOrEmpty(Kood)) return Keel.T("Sa ei ole veel peres. Loo pere või liitu ostunimekirja juures.");
        using var db = new ToodedContext();
        var olemas = db.PereKorv.FirstOrDefault(r => r.PereKood == Kood && r.ToodeId == t.Id && r.KasutajaId == AktiivneKasutaja.Id);
        if (olemas != null) { olemas.Kogus += kogus; olemas.Aeg = DateTime.Now; }
        else db.PereKorv.Add(new PereKorviRida
        {
            PereKood = Kood!, ToodeId = t.Id, Kogus = kogus, KasutajaId = AktiivneKasutaja.Id,
            Lisaja = EesNimi()
        });
        db.SaveChanges();
        Varskenda(teavita: false);
        return null;
    }

    public static void Muuda(int reaId, int muutus)
    {
        using var db = new ToodedContext();
        var r = db.PereKorv.Find(reaId);
        if (r == null) return;
        r.Kogus += muutus;
        if (r.Kogus <= 0) db.PereKorv.Remove(r);
        db.SaveChanges();
        Varskenda(teavita: false);
    }

    // Kogu pere korv minu ostukorvi (maksmiseks); tagastab lisatud tükid ja puuduvad tooted
    public static (int Lisatud, List<string> Puudu) VotaKoik()
    {
        int lisatud = 0;
        var puudu = new List<string>();
        using var db = new ToodedContext();
        foreach (var r in db.PereKorv.Include(r => r.Toode).Where(r => r.PereKood == Kood).ToList())
        {
            int vaba = r.Toode.Kogus - Ostukorv.KorvisKogus(r.ToodeId);
            int k = Math.Min(r.Kogus, vaba);
            if (k > 0 && Ostukorv.LisaToode(r.Toode, k) == null) lisatud += k;
            if (k < r.Kogus) puudu.Add(r.Toode.Toodenimetus);
            db.PereKorv.Remove(r);
        }
        db.SaveChanges();
        Varskenda(teavita: false);
        return (lisatud, puudu);
    }

    // Minu praegune ostukorv pere korvi (nt "lisa ka see, mida mina tahan")
    public static int JagaMinuKorv()
    {
        int tk = 0;
        using var db = new ToodedContext();
        foreach (var r in Ostukorv.Ridad.ToList())
            if (db.Toodetabel.AsNoTracking().FirstOrDefault(t => t.Id == r.Id) is Toode t && Lisa(t, r.Kogus) == null) tk += r.Kogus;
        Ostukorv.Tyhjenda();
        return tk;
    }

    private static string EesNimi() => AktiivneKasutaja.Kuvanimi.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "?";
}
