using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IKTpv25_1_Toooted;

// E-posti seaded: vaikimisi väärtused on allpool koodis. Menüüs Haldus → E-posti seaded
// sisestatud andmed salvestatakse faili AppData\Local\ToodedApp\email.json
// (parool krüpteeritult Windowsi DPAPI-ga) ja need on koodis olevatest tähtsamad.
public class EmailSeaded
{
    // Vaikimisi Gmail ja RAKENDUSE parool (juhend 4.2) – töötab kohe, midagi ei pea sisestama.
    // Menüüs Haldus → E-posti seaded saab need üle kirjutada.
    // NB! Enne git push'i asenda parool tekstiga "xxxx" (juhendi kontroll-loend).
    public const string VaikeSaatja = "kosgaponenko@gmail.com";
    private const string VaikeParool = "wtwu ddsx lgfe ingg";

    public string Saatja { get; set; } = "";              // nt pood@gmail.com
    public string ParoolKruptitud { get; set; } = "";     // Gmaili RAKENDUSE parool (16 märki), krüpteeritud
    public string Server { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    // Tšeki (PDF) ja e-kirja keel – ei sõltu programmi keelest (vaikimisi eesti)
    public string TsekiKeel { get; set; } = "ET";

    public KeeleKood TsekiKeeleKood => Enum.TryParse(TsekiKeel, out KeeleKood k) ? k : KeeleKood.ET;

    private static string Fail => Path.Combine(Kaustad.Juur, "email.json");

    private static bool VaikeParoolOlemas => VaikeParool.Length > 0 && !VaikeParool.StartsWith("xxxx");

    public bool OnSeadistatud => Saatja.Length > 0 && (ParoolKruptitud.Length > 0 || VaikeParoolOlemas);

    public static EmailSeaded Lae()
    {
        EmailSeaded seaded = new();
        try
        {
            if (File.Exists(Fail))
                seaded = JsonSerializer.Deserialize<EmailSeaded>(File.ReadAllText(Fail)) ?? new EmailSeaded();
        }
        catch { }

        if (seaded.Saatja.Length == 0) seaded.Saatja = VaikeSaatja;
        return seaded;
    }

    public void Salvesta()
    {
        Directory.CreateDirectory(Kaustad.Juur);
        File.WriteAllText(Fail, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void SeaParool(string parool)
    {
        byte[] kruptitud = ProtectedData.Protect(Encoding.UTF8.GetBytes(parool), null, DataProtectionScope.CurrentUser);
        ParoolKruptitud = Convert.ToBase64String(kruptitud);
    }

    // Salvestatud parool (kui on), muidu vaikimisi parool koodist
    private string Parool() => ParoolKruptitud.Length > 0 ? LoeParool() : VaikeParool;

    private string LoeParool()
    {
        byte[] baidid = ProtectedData.Unprotect(Convert.FromBase64String(ParoolKruptitud), null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(baidid);
    }

    // Saadab kirja (soovi korral manusega). Viskab erindi, kui saatmine ei õnnestu.
    public void Saada(string saaja, string teema, string sisu, string? manus = null)
    {
        if (!OnSeadistatud)
            throw new InvalidOperationException("E-posti seaded puuduvad (menüü Haldus → E-posti seaded).");

        using var kiri = new MailMessage(Saatja, saaja, teema, sisu);
        if (manus != null) kiri.Attachments.Add(new Attachment(manus));   // using vabastab faili

        string parool;
        try { parool = Parool().Replace(" ", ""); }
        catch (Exception ex) { throw new InvalidOperationException(Selgita(ex), ex); }
        using var smtp = new SmtpClient(Server, Port)
        {
            Credentials = new NetworkCredential(Saatja.Trim(), parool),
            EnableSsl = true,
            Timeout = 20000
        };
        Saada(smtp, kiri);
    }

    // HTML-kiri (bänner kirja sees, cid:banner) + lihttekst + PDF manusena
    public void SaadaHtml(string saaja, string teema, string tekst, string html, byte[]? banner, string? manus)
    {
        if (!OnSeadistatud)
            throw new InvalidOperationException("E-posti seaded puuduvad (menüü Haldus → E-posti seaded).");

        using var kiri = new MailMessage(new MailAddress(Saatja, Arve.PoeNimi), new MailAddress(saaja))
        {
            Subject = teema,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };
        kiri.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(tekst, Encoding.UTF8, "text/plain"));
        var htmlVaade = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, "text/html");
        if (banner != null)
        {
            var pilt = new LinkedResource(new MemoryStream(banner), "image/png")
            {
                ContentId = "banner",
                TransferEncoding = System.Net.Mime.TransferEncoding.Base64
            };
            htmlVaade.LinkedResources.Add(pilt);
        }
        kiri.AlternateViews.Add(htmlVaade);
        if (manus != null) kiri.Attachments.Add(new Attachment(manus));

        string parool;
        try { parool = Parool().Replace(" ", ""); }
        catch (Exception ex) { throw new InvalidOperationException(Selgita(ex), ex); }
        using var smtp = new SmtpClient(Server, Port)
        {
            Credentials = new NetworkCredential(Saatja.Trim(), parool),
            EnableSsl = true,
            Timeout = 30000
        };
        Saada(smtp, kiri);
    }

    // Saatmine + arusaadav veateade (SmtpException ütleb tavaliselt ainult "Failure sending mail")
    private void Saada(SmtpClient smtp, MailMessage kiri)
    {
        try { smtp.Send(kiri); }
        catch (Exception ex) { throw new InvalidOperationException(Selgita(ex), ex); }
    }

    private string Selgita(Exception ex)
    {
        // kogu vigade ahel: sageli on põhjus alles sisemises erindis
        var osad = new List<string>();
        for (var e = ex; e != null; e = e.InnerException) if (!osad.Contains(e.Message)) osad.Add(e.Message);
        string koik = string.Join(" → ", osad);
        string k = koik.ToLowerInvariant();

        string pohjus;
        if (k.Contains("5.7.0") || k.Contains("5.7.8") || k.Contains("535") || k.Contains("authentication") || k.Contains("username and password"))
            pohjus = Keel.F("Gmail ei aktsepteerinud kasutajat või parooli ({0}).", Saatja) + "\n" +
                     Keel.T("• Saatja aadress peab olema täpselt see Gmaili konto, millele rakenduse parool loodi.") + "\n" +
                     Keel.T("• Vaja on 16-märgilist RAKENDUSE parooli (myaccount.google.com → Turvalisus → Rakenduse paroolid), mitte tavalist parooli; kontol peab olema 2-astmeline kinnitus.") + "\n" +
                     Keel.T("• Kui parool on GitHubis avalikult nähtav, võib Google selle tühistada – loo uus.");
        else if (k.Contains("timed out") || k.Contains("timeout") || k.Contains("unable to connect") || k.Contains("actively refused") || k.Contains("no such host") || k.Contains("network"))
            pohjus = Keel.F("Ei saanud ühendust serveriga {0}:{1}.", Server, Port) + "\n" +
                     Keel.T("• Kontrolli internetiühendust.") + "\n" +
                     Keel.T("• Kooli-/töövõrk või viirusetõrje võib pordi 587 blokeerida – proovi teises võrgus (nt telefoni levialas).");
        else if (k.Contains("5.5.1") || k.Contains("5.1.") || k.Contains("recipient") || k.Contains("mailbox"))
            pohjus = Keel.T("Saaja aadress on vigane või seda postkasti pole.");
        else if (k.Contains("5.4.5") || k.Contains("daily") || k.Contains("limit"))
            pohjus = Keel.T("Gmaili päevane saatmislimiit on täis – proovi homme uuesti.");
        else if (k.Contains("key not valid") || k.Contains("cryptographic"))
            pohjus = Keel.T("Salvestatud parooli ei saa lugeda (teine Windowsi kasutaja või arvuti). Sisesta parool uuesti: Haldus → E-posti seaded.");
        else
            pohjus = Keel.T("E-kirja saatmine ebaõnnestus.");

        return pohjus + "\n\n" + Keel.T("Tehniline viga:") + " " + koik;
    }

    public static bool OnKorrektneAadress(string? aadress) =>
        !string.IsNullOrWhiteSpace(aadress)
        && MailAddress.TryCreate(aadress.Trim(), out var a)
        && a.Host.Contains('.');
}
