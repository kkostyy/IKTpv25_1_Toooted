using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Näitab vea akna ja logifaili, mitte ei sulge programmi vaikselt
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) => NaitaViga(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (s, e) => NaitaViga(e.ExceptionObject as Exception);

        ApplicationConfiguration.Initialize();
        Teema.Lae();   // hele/tume teema ja poe värv (salvestatud valik)
        Keel.Lae();    // eesti / vene / inglise
        Application.AddMessageFilter(new DisainiKlahv());   // Ctrl+Shift+D igas aknas
        Application.AddMessageFilter(new TagasiHiir());     // hiire külgnupp = tagasi

        try
        {
            Kaustad.Loo();
            using var db = new ToodedContext();

            if (!db.Database.GetMigrations().Any())
            {
                Teade.Show(
                    "Migratsioone pole veel loodud.\n\n" +
                    "Tools → NuGet Package Manager → Package Manager Console:\n" +
                    "    Add-Migration InitialCreate\n\n" +
                    "Seejärel käivita programm uuesti (F5) – andmebaas luuakse ise.",
                    "Andmebaas puudub", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Sama mis Update-Database, aga iga käivitamisel: andmebaas tekib ka teises arvutis
            db.Database.Migrate();

            // Kontroll: kas andmebaas vastab programmile? Kui uus migratsioon on tegemata,
            // tuleb viga siin (mitte hiljem poes)
            _ = db.Toodetabel.AsNoTracking().FirstOrDefault();
            _ = db.Kategooriatabel.AsNoTracking().FirstOrDefault();
            _ = db.Kliendid.AsNoTracking().FirstOrDefault();
            _ = db.Ostud.AsNoTracking().FirstOrDefault();
            _ = db.OstuRead.AsNoTracking().FirstOrDefault();
            _ = db.Kasutajad.AsNoTracking().FirstOrDefault();
            _ = db.Ostunimekiri.AsNoTracking().FirstOrDefault();
            _ = db.Kampaaniad.AsNoTracking().FirstOrDefault();
            _ = db.Saavutused.AsNoTracking().FirstOrDefault();
            _ = db.Partiid.AsNoTracking().FirstOrDefault();

            LooAlgkasutajad(db);
            Kampaaniad.Uuenda(db, DateTime.Now);   // alanud/lõppenud kampaaniate hinnad
        }
        catch (Exception ex)
        {
            Teade.Show("Andmebaasiga ühendamine ebaõnnestus:\n" + ex.Message +
                "\n\nAndmebaas on programmist vanem (uued veerud/tabelid puuduvad).\n" +
                "Tools → NuGet Package Manager → Package Manager Console:\n" +
                "    Add-Migration Uuendus" + DateTime.Now.ToString("MMddHHmm") + "\n" +
                "ja käivita programm uuesti (F5).",
                "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Kõigepealt sisselogimine; roll määrab, mida kasutaja näeb
        using (var login = new LoginVorm())
        {
            login.StartPosition = FormStartPosition.CenterScreen;
            login.ShowInTaskbar = true;
            if (login.ShowDialog() != DialogResult.OK) return;
        }

        Application.Run(new Form1());
    }

    // Esimesed kasutajad luuakse käivitamisel, kui tabel on tühi.
    // HasData siia ei sobi: BCrypt annab iga kord uue räsi.
    private static void LooAlgkasutajad(ToodedContext db)
    {
        if (db.Kasutajad.Any()) return;

        db.Kasutajad.AddRange(
            new Kasutaja { Kasutajanimi = "admin", ParoolHash = PasswordHelper.Hash("admin123"), Roll = Kasutaja.Omanik },
            new Kasutaja { Kasutajanimi = "myyja", ParoolHash = PasswordHelper.Hash("myyja123"), Roll = Kasutaja.Myyja });
        db.SaveChanges();
    }

    private static void NaitaViga(Exception? ex)
    {
        string tekst = ex != null ? ex.ToString() : "Tundmatu viga";
        try
        {
            File.AppendAllText(Path.Combine(Kaustad.Juur, "viga.log"),
                DateTime.Now + Environment.NewLine + tekst + Environment.NewLine + Environment.NewLine);
        }
        catch { }
        MessageBox.Show(tekst, "Programmi viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

// Ctrl+Shift+D vahetab disaini (e-pood/Rimi <-> klassikaline) ükskõik millises aknas
internal sealed class DisainiKlahv : IMessageFilter
{
    private const int WM_KEYDOWN = 0x0100;

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg == WM_KEYDOWN && (Keys)(int)m.WParam == Keys.D &&
            Control.ModifierKeys == (Keys.Control | Keys.Shift))
        {
            Teema.VahetaDisain();
            Teade.Toast(Teema.OnRimi ? "Disain: e-pood (Rimi stiil)" : "Disain: klassikaline", TeateTyyp.Info, 1800);
            return true;   // klahvi rohkem edasi ei saadeta
        }
        return false;
    }
}
