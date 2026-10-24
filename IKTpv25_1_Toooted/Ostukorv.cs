using System.Globalization;

namespace IKTpv25_1_Toooted;

public class KorvRida
{
    public int Id { get; set; }               // toote Id
    public string Nimi { get; set; } = "";
    public decimal Hind { get; set; }
    public int Kogus { get; set; }
    public int Laos { get; set; }             // laoseis lisamise hetkel (rohkem ei saa)
    public string? Pilt { get; set; }         // pildi failinimi (pisipildi jaoks)
    public decimal Pant { get; set; }         // pakendi tagatisraha tüki kohta (ei kuulu soodustuse ega boonuse alla)
    public decimal TavaHind { get; set; }     // hind ilma soodustuseta (tšekil "soodustus")
    public string? Soodus { get; set; }       // soodustuse põhjus, nt "Õhtune soodustus −50 %"

    public decimal Summa => Hind * Kogus;
    public decimal PantKokku => Pant * Kogus;

    public override string ToString() =>
        $"{Nimi} – {Kogus} tk × {Ostukorv.Eur(Hind)} = {Ostukorv.Eur(Summa)}";
}

// Üks ühine ostukorv kogu rakenduse peale: Kassa vaheleht ja Pood kasutavad sama korvi
public static class Ostukorv
{
    public static List<KorvRida> Ridad { get; } = new();

    public static decimal Kokku => Ridad.Sum(r => r.Summa);          // tooted (ilma pandita)
    public static decimal PantKokku => Ridad.Sum(r => r.PantKokku);  // pakendi tagatisraha
    public static int PandiTk => Ridad.Where(r => r.Pant > 0).Sum(r => r.Kogus);

    // Käivitub iga muudatuse järel, et vormid saaksid korvi uuesti näidata
    public static event Action? Muutus;

    // Eesti kombel koma: 1,25 €
    private static readonly CultureInfo Eesti = new("et-EE");
    public static string Eur(decimal v) => v.ToString("0.00", Eesti) + " €";

    // Lisab toote korvi. Tagastab veateate või null, kui kõik õnnestus.
    // Laos olevast kogusest rohkem lisada ei saa.
    public static int Kogus => Ridad.Sum(r => r.Kogus);

    public static int KorvisKogus(int tooteId) => Ridad.FirstOrDefault(r => r.Id == tooteId)?.Kogus ?? 0;

    // Toode korvi praeguse lõpphinnaga (kampaania, õhtune soodustus, säilivusaeg, üllatus) ja pandiga
    public static string? LisaToode(Toode t, int kogus = 1)
    {
        if (t.Kogus <= 0) return Keel.F("„{0}“ on otsas.", t.Toodenimetus);
        var h = Hinnad.Lopphind(t);
        string? viga = Lisa(t.Id, t.Toodenimetus, h.Hind, t.Kogus, kogus, t.Pilt, t.Pant);
        if (viga == null && Ridad.FirstOrDefault(r => r.Id == t.Id) is KorvRida rida)
        {
            rida.TavaHind = h.TavaHind;
            rida.Soodus = h.Soodus ? h.Allikas : null;
        }
        return viga;
    }

    public static string? Lisa(int id, string nimi, decimal hind, int laos, int kogus = 1, string? pilt = null, decimal pant = 0)
    {
        var olemas = Ridad.FirstOrDefault(r => r.Id == id);
        int korvis = olemas?.Kogus ?? 0;

        if (korvis + kogus > laos)
            return Keel.F("Toodet „{0}“ pole rohkem laos (laos: {1}, korvis: {2}).", nimi, laos, korvis);

        if (olemas != null)
        {
            olemas.Kogus += kogus;
            olemas.Hind = hind;
            olemas.Laos = laos;
            olemas.Pant = pant;
            if (pilt != null) olemas.Pilt = pilt;
        }
        else
        {
            Ridad.Add(new KorvRida { Id = id, Nimi = nimi, Hind = hind, Kogus = kogus, Laos = laos, Pilt = pilt, Pant = pant, TavaHind = hind });
        }

        Muutus?.Invoke();
        return null;
    }

    // Suurendab rea kogust ühe võrra (kui laos on)
    public static string? Suurenda(KorvRida rida) =>
        Lisa(rida.Id, rida.Nimi, rida.Hind, rida.Laos, 1, rida.Pilt, rida.Pant);

    // Vähendab kogust ühe võrra; viimase tüki puhul eemaldab rea
    public static void Vahenda(KorvRida rida)
    {
        if (rida.Kogus > 1)
        {
            rida.Kogus--;
            Muutus?.Invoke();
        }
        else
        {
            Eemalda(rida);
        }
    }

    public static void Eemalda(KorvRida rida)
    {
        if (Ridad.Remove(rida)) Muutus?.Invoke();
    }

    public static void Tyhjenda()
    {
        Ridad.Clear();
        Muutus?.Invoke();
    }
}

// Toodete pildid mälus (laetakse üks kord), et pood ja ostukorv ei peaks faile korduvalt lugema
// Veebipildid (e-poe tooted) laaditakse taustal üks kord alla ja salvestatakse kausta Images.
public static class PildiVahemalu
{
    private static readonly Dictionary<string, Image?> pildid = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> laadimisel = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(20) };

    // Käivitub (taustalõimes), kui mõni veebipilt on alla laaditud – vormid joonistavad end uuesti
    public static event Action? Laetud;

    public static Image? Too(string? failinimi)
    {
        if (string.IsNullOrEmpty(failinimi)) return null;
        lock (pildid)
        {
            if (pildid.TryGetValue(failinimi, out var olemas)) return olemas;
        }

        string? tee = Kaustad.PildiTee(failinimi);
        if (Kaustad.OnVeebiPilt(failinimi) && (tee == null || !File.Exists(tee)))
        {
            AlustaAllalaadimist(failinimi, tee!);
            return null;
        }

        var pilt = Kaustad.LaePilt(tee);
        lock (pildid) pildid[failinimi] = pilt;
        return pilt;
    }

    private static void AlustaAllalaadimist(string url, string tee)
    {
        lock (laadimisel)
        {
            if (!laadimisel.Add(url)) return;   // juba laadimisel
        }

        Task.Run(async () =>
        {
            try
            {
                byte[] baidid = await http.GetByteArrayAsync(url);
                Directory.CreateDirectory(Path.GetDirectoryName(tee)!);
                await File.WriteAllBytesAsync(tee, baidid);
                var pilt = Kaustad.LaePilt(tee);
                lock (pildid) pildid[url] = pilt;
            }
            catch
            {
                lock (pildid) pildid[url] = null;   // internetti pole – kaart näitab ikooni
            }
            finally
            {
                lock (laadimisel) laadimisel.Remove(url);
            }
            Laetud?.Invoke();
        });
    }

    // Joonistab pildi kasti sisse proportsioone säilitades
    public static void Joonista(Graphics g, Image pilt, RectangleF kast)
    {
        float suhe = Math.Min(kast.Width / pilt.Width, kast.Height / pilt.Height);
        float w = pilt.Width * suhe, h = pilt.Height * suhe;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.DrawImage(pilt, kast.X + (kast.Width - w) / 2, kast.Y + (kast.Height - h) / 2, w, h);
    }
}
