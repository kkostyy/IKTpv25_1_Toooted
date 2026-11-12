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

    // Korvi hinnad uuesti praeguse aja järgi (nt kell sai 20:00). Tagastab true, kui midagi muutus.
    public static bool UuendaHinnad()
    {
        if (Ridad.Count == 0) return false;
        Dictionary<int, Toode> tooted;
        try
        {
            using var db = new ToodedContext();
            var ids = Ridad.Select(r => r.Id).ToList();
            tooted = db.Toodetabel.Where(t => ids.Contains(t.Id)).ToDictionary(t => t.Id);
        }
        catch { return false; }
        bool muutus = false;
        foreach (var r in Ridad)
        {
            if (!tooted.TryGetValue(r.Id, out var t)) continue;
            var h = Hinnad.Lopphind(t);
            if (h.Hind != r.Hind) muutus = true;
            r.Hind = h.Hind;
            r.TavaHind = h.TavaHind;
            r.Soodus = h.Soodus ? h.Allikas : null;
            r.Pant = t.Pant;
        }
        if (muutus) Muutus?.Invoke();
        return muutus;
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

// Toodete pildid mälus. Kiiruse jaoks:
//  * pilt vähendatakse laadimisel (max 300 px) ja hoitakse PArgb-vormingus – joonistamine on mitu korda kiirem
//    kui suure veebipildi skaleerimine igal kaardi joonistamisel;
//  * mälus on kuni MaxPilte pilti (vanimad unustatakse), muidu 9000 toote sirvimine sööb mälu;
//  * Pisi() laeb ka kettal oleva pildi taustalõimes (kerimine ei hakka), Too() laeb kohalikku faili kohe;
//  * veebist laaditakse korraga kuni 6 pilti; teade "Laetud" antakse koondatult (~ iga 120 ms), mitte iga pildi järel.
public static class PildiVahemalu
{
    private const int MaxKylg = 300, MaxPilte = 700;

    private static readonly Dictionary<string, LinkedListNode<(string Voti, Image? Pilt)>> pildid = new(StringComparer.OrdinalIgnoreCase);
    private static readonly LinkedList<(string Voti, Image? Pilt)> jarjekord = new();   // ees = viimati kasutatud
    private static readonly HashSet<string> laadimisel = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HttpClient http = LooHttp();
    // Ebaõnnestunud allalaadimine: millal uuesti proovida (mitte kogu seansiks tühjaks jätta)
    private static readonly Dictionary<string, DateTime> uuestiProovida = new(StringComparer.OrdinalIgnoreCase);
    private static readonly TimeSpan Ooteaeg = TimeSpan.FromSeconds(20);

    private static HttpClient LooHttp()
    {
        var h = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        // GDI+ ei oska WebP/AVIF-i – küsime ainult JPEG/PNG
        h.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AKT");
        h.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "image/jpeg,image/png;q=0.9");
        return h;
    }

    // Kas veebipilt ebaõnnestus hiljuti (siis ei proovi kohe uuesti)
    private static bool HiljutiEbaonnestus(string voti)
    {
        lock (lukk)
        {
            if (!uuestiProovida.TryGetValue(voti, out var aeg)) return false;
            if (DateTime.UtcNow < aeg) return true;
            uuestiProovida.Remove(voti);
            return false;
        }
    }
    private static readonly SemaphoreSlim veebiPiirang = new(6);
    private static readonly object lukk = new();
    private static System.Threading.Timer? teavitus;
    private static int ootelTeade;

    // Käivitub (taustalõimes), kui pilte on alla laaditud – vormid joonistavad end uuesti
    public static event Action? Laetud;

    private static bool Vahemalus(string voti, out Image? pilt)
    {
        lock (lukk)
        {
            if (pildid.TryGetValue(voti, out var solm))
            {
                if (solm != jarjekord.First) { jarjekord.Remove(solm); jarjekord.AddFirst(solm); }
                pilt = solm.Value.Pilt;
                return true;
            }
        }
        pilt = null;
        return false;
    }

    private static void Pane(string voti, Image? pilt)
    {
        lock (lukk)
        {
            if (pildid.TryGetValue(voti, out var vana)) jarjekord.Remove(vana);
            pildid[voti] = jarjekord.AddFirst((voti, pilt));
            // vanimad välja; Dispose'i ei kutsu – pilt võib olla veel PictureBoxis (GC vabastab)
            while (pildid.Count > MaxPilte && jarjekord.Last is { } viimane)
            {
                jarjekord.RemoveLast();
                pildid.Remove(viimane.Value.Voti);
            }
        }
    }

    // Pilt kohe: kohalik fail laetakse samas lõimes, veebipilt hakkab alla laadima (siis null)
    public static Image? Too(string? failinimi)
    {
        if (string.IsNullOrEmpty(failinimi)) return null;
        if (Vahemalus(failinimi, out var olemas)) return olemas;
        lock (lukk) if (laadimisel.Contains(failinimi)) return null;
        if (HiljutiEbaonnestus(failinimi)) return null;

        bool veeb = Kaustad.OnVeebiPilt(failinimi);
        string? tee = Kaustad.PildiTee(failinimi);
        if (tee == null || !File.Exists(tee))
        {
            if (veeb && tee != null) AlustaLaadimist(failinimi, tee, veeb: true);
            else Pane(failinimi, null);
            return null;
        }

        var pilt = Vahenda(Kaustad.LaePilt(tee));
        if (pilt == null && veeb)
        {
            // katkine koopia kettal (nt pooleli jäänud allalaadimine) – kustuta ja lae uuesti
            KustutaFail(tee);
            AlustaLaadimist(failinimi, tee, veeb: true);
            return null;
        }
        Pane(failinimi, pilt);
        return pilt;
    }

    // Kas pilt on alles tulekul (laadimisel või pole veel küsitud); vigane/puuduv pilt = false
    public static bool Laeb(string? failinimi)
    {
        if (string.IsNullOrEmpty(failinimi)) return false;
        lock (lukk)
            return laadimisel.Contains(failinimi) ||
                   (!pildid.ContainsKey(failinimi) && !uuestiProovida.ContainsKey(failinimi));
    }

    // Kaartide jaoks: ei loe kunagi UI-lõimes faili, vaid tagastab null ja laeb taustal ("Laetud" teatab)
    public static Image? Pisi(string? failinimi)
    {
        if (string.IsNullOrEmpty(failinimi)) return null;
        if (Vahemalus(failinimi, out var olemas)) return olemas;
        if (HiljutiEbaonnestus(failinimi)) return null;
        string? tee = Kaustad.PildiTee(failinimi);
        if (tee == null) return null;
        AlustaLaadimist(failinimi, tee, veeb: Kaustad.OnVeebiPilt(failinimi));
        return null;
    }

    private static void AlustaLaadimist(string voti, string tee, bool veeb)
    {
        lock (lukk)
        {
            if (!laadimisel.Add(voti)) return;   // juba laadimisel
        }

        Task.Run(async () =>
        {
            Image? pilt = null;
            try
            {
                if (File.Exists(tee))
                {
                    pilt = Vahenda(Kaustad.LaePilt(tee));
                    if (pilt == null && veeb) KustutaFail(tee);   // katkine koopia – lae uuesti
                }
                if (pilt == null && veeb)
                {
                    await veebiPiirang.WaitAsync();
                    try { pilt = await LaeVeebist(voti, tee); }
                    finally { veebiPiirang.Release(); }
                }
            }
            catch (Exception ex)
            {
                pilt = null;   // internetti pole – kaart näitab ikooni, mõne aja pärast proovitakse uuesti
                Logi(voti, ex);
            }
            finally
            {
                lock (lukk)
                {
                    laadimisel.Remove(voti);
                    if (pilt == null && veeb) uuestiProovida[voti] = DateTime.UtcNow + Ooteaeg;
                }
                if (pilt != null || !veeb) Pane(voti, pilt);
                Teavita();
            }
        });
    }

    // Laeb pildi veebist; kettale kirjutatakse ainult päris pilt (mitte HTML-veateade vms)
    private static async Task<Image?> LaeVeebist(string url, string tee)
    {
        Exception? viimane = null;
        for (int katse = 0; katse < 2; katse++)
        {
            try
            {
                byte[] baidid = await http.GetByteArrayAsync(url);
                Image? pilt;
                using (var ms = new MemoryStream(baidid))
                using (var ajutine = Image.FromStream(ms))   // viga, kui see pole pilt
                    pilt = new Bitmap(ajutine);

                Directory.CreateDirectory(Path.GetDirectoryName(tee)!);
                string ajutineFail = tee + ".tmp";
                await File.WriteAllBytesAsync(ajutineFail, baidid);
                File.Move(ajutineFail, tee, overwrite: true);
                return Vahenda(pilt);
            }
            catch (Exception ex)
            {
                viimane = ex;
                await Task.Delay(800);
            }
        }
        throw viimane!;
    }

    private static void KustutaFail(string tee)
    {
        try { File.Delete(tee); } catch { }
    }

    // Esimesed vead faili pildid_viga.txt (AppData\Local\ToodedApp) – näha, miks pildid ei tule
    private static int logitud;
    private static void Logi(string url, Exception ex)
    {
        if (Interlocked.Increment(ref logitud) > 20) return;
        try
        {
            File.AppendAllText(Path.Combine(Kaustad.Juur, "pildid_viga.txt"),
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {url}\n  {ex.GetType().Name}: {ex.Message}\n");
        }
        catch { }
    }

    // Kõik avatud aknad joonistavad end uuesti, kui pilte on juurde tulnud
    // (ka vaated, mis ise "Laetud" sündmust ei kuula)
    public static void UuendaAknadLaadimisel()
    {
        long viimane = 0;
        Laetud += () =>
        {
            // kuni ~3 korda sekundis (suure kataloogi laadimisel tuleb pilte pidevalt)
            long nyyd = Environment.TickCount64;
            if (nyyd - Interlocked.Read(ref viimane) < 300) { Teavita(); return; }
            Interlocked.Exchange(ref viimane, nyyd);

            Form? esimene;
            try { esimene = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.IsHandleCreated && !f.IsDisposed); }
            catch { return; }
            if (esimene == null) return;
            try
            {
                esimene.BeginInvoke(new Action(() =>
                {
                    // pood ja tooteleht värskendavad ainult ootavaid kaarte ise
                    foreach (Form f in Application.OpenForms.Cast<Form>().ToList())
                        if (!f.IsDisposed && f is not PoodVorm && f is not PoodKlassikVorm && f is not ToodeVorm)
                            f.Invalidate(true);
                }));
            }
            catch { }
        };
    }

    // Mitu pilti järjest → üks "Laetud" teade
    private static void Teavita()
    {
        if (Interlocked.Exchange(ref ootelTeade, 1) == 1) return;
        lock (lukk)
        {
            teavitus ??= new System.Threading.Timer(_ =>
            {
                Interlocked.Exchange(ref ootelTeade, 0);
                try { Laetud?.Invoke(); } catch { }
            });
            teavitus.Change(120, Timeout.Infinite);
        }
    }

    // Suur pilt → kuni 300 px, kiire joonistamise vorming (32bppPArgb)
    private static Image? Vahenda(Image? pilt)
    {
        if (pilt == null) return null;
        try
        {
            float suhe = Math.Min(1f, Math.Min((float)MaxKylg / pilt.Width, (float)MaxKylg / pilt.Height));
            int w = Math.Max(1, (int)Math.Round(pilt.Width * suhe)), h = Math.Max(1, (int)Math.Round(pilt.Height * suhe));
            var uus = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(uus))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.DrawImage(pilt, 0, 0, w, h);
            }
            pilt.Dispose();
            return uus;
        }
        catch
        {
            return pilt;
        }
    }

    // Joonistab pildi kasti sisse proportsioone säilitades (pilt on juba väike → kiire bilineaarne)
    public static void Joonista(Graphics g, Image pilt, RectangleF kast)
    {
        float suhe = Math.Min(kast.Width / pilt.Width, kast.Height / pilt.Height);
        float w = pilt.Width * suhe, h = pilt.Height * suhe;
        var enne = g.InterpolationMode;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
        g.DrawImage(pilt, kast.X + (kast.Width - w) / 2, kast.Y + (kast.Height - h) / 2, w, h);
        g.InterpolationMode = enne;
    }
}
