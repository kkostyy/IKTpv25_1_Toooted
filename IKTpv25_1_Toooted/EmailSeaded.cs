using System.Data.Common;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// E-posti seaded hoitakse AINULT andmebaasis (tooded.db, tabel "Seaded").
// Koodis parooli ei ole: see sisestatakse menüüs Haldus → E-posti seaded.
// Parool on krüpteeritud Windowsi DPAPI-ga (mitte räsitud: SMTP vajab sisselogimiseks
// algset parooli, räsist seda tagasi ei saa).
public class EmailSeaded
{

    public string Saatja { get; set; } = "";              // nt pood@gmail.com
    public string ParoolKruptitud { get; set; } = "";     // Gmaili RAKENDUSE parool (16 märki), krüpteeritud
    public string Server { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    // Tšeki (PDF) ja e-kirja keel: ei sõltu programmi keelest (vaikimisi eesti)
    public string TsekiKeel { get; set; } = "ET";

    public KeeleKood TsekiKeeleKood => Enum.TryParse(TsekiKeel, out KeeleKood k) ? k : KeeleKood.ET;

    // Vana seadete fail (enne andmebaasi): loetakse üks kord sisse ja kustutatakse
    private static string VanaFail => Path.Combine(Kaustad.Juur, "email.json");

    private const string Tabel = "Seaded";

    public bool OnSeadistatud => Saatja.Length > 0 && ParoolKruptitud.Length > 0;

    public static EmailSeaded Lae()
    {
        EmailSeaded seaded = new();
        try
        {
            using var db = new ToodedContext();
            var yhendus = db.Database.GetDbConnection();
            db.Database.OpenConnection();
            try
            {
                LooTabel(yhendus);
                var andmed = LoeKoik(yhendus);
                if (andmed.Count == 0 && File.Exists(VanaFail))
                {
                    // ühekordne üleviimine email.json → andmebaas
                    seaded = JsonSerializer.Deserialize<EmailSeaded>(File.ReadAllText(VanaFail)) ?? new EmailSeaded();
                    Kirjuta(yhendus, seaded);
                    File.Delete(VanaFail);
                }
                else
                {
                    if (andmed.TryGetValue("Email.Saatja", out var v)) seaded.Saatja = v;
                    if (andmed.TryGetValue("Email.Parool", out v)) seaded.ParoolKruptitud = v;
                    if (andmed.TryGetValue("Email.Server", out v)) seaded.Server = v;
                    if (andmed.TryGetValue("Email.Port", out v) && int.TryParse(v, out int port)) seaded.Port = port;
                    if (andmed.TryGetValue("Email.TsekiKeel", out v)) seaded.TsekiKeel = v;
                }
            }
            finally { db.Database.CloseConnection(); }
        }
        catch { }

        return seaded;
    }

    public void Salvesta()
    {
        using var db = new ToodedContext();
        var yhendus = db.Database.GetDbConnection();
        db.Database.OpenConnection();
        try
        {
            LooTabel(yhendus);
            Kirjuta(yhendus, this);
        }
        finally { db.Database.CloseConnection(); }
    }

    // Võti-väärtus tabel; luuakse vajadusel (ei vaja eraldi migratsiooni)
    private static void LooTabel(DbConnection yhendus)
    {
        using var k = yhendus.CreateCommand();
        k.CommandText = $"CREATE TABLE IF NOT EXISTS \"{Tabel}\" (\"Votim\" TEXT NOT NULL PRIMARY KEY, \"Vaartus\" TEXT NOT NULL)";
        k.ExecuteNonQuery();
    }

    private static Dictionary<string, string> LoeKoik(DbConnection yhendus)
    {
        var tulemus = new Dictionary<string, string>();
        using var k = yhendus.CreateCommand();
        k.CommandText = $"SELECT \"Votim\", \"Vaartus\" FROM \"{Tabel}\" WHERE \"Votim\" LIKE 'Email.%'";
        using var r = k.ExecuteReader();
        while (r.Read()) tulemus[r.GetString(0)] = r.GetString(1);
        return tulemus;
    }

    private static void Kirjuta(DbConnection yhendus, EmailSeaded s)
    {
        using var tx = yhendus.BeginTransaction();
        foreach (var (votim, vaartus) in new[]
        {
            ("Email.Saatja", s.Saatja),
            ("Email.Parool", s.ParoolKruptitud),
            ("Email.Server", s.Server),
            ("Email.Port", s.Port.ToString()),
            ("Email.TsekiKeel", s.TsekiKeel),
        })
        {
            using var k = yhendus.CreateCommand();
            k.Transaction = tx;
            k.CommandText = $"INSERT OR REPLACE INTO \"{Tabel}\" (\"Votim\", \"Vaartus\") VALUES ($v, $a)";
            var p1 = k.CreateParameter(); p1.ParameterName = "$v"; p1.Value = votim; k.Parameters.Add(p1);
            var p2 = k.CreateParameter(); p2.ParameterName = "$a"; p2.Value = vaartus ?? ""; k.Parameters.Add(p2);
            k.ExecuteNonQuery();
        }
        tx.Commit();
    }

    public void SeaParool(string parool)
    {
        byte[] kruptitud = ProtectedData.Protect(Encoding.UTF8.GetBytes(parool), null, DataProtectionScope.CurrentUser);
        ParoolKruptitud = Convert.ToBase64String(kruptitud);
    }

    // Parool ainult andmebaasist (krüpteeritult)
    private string Parool() => LoeParool();

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
