using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Andmebaasi uuendamine käivitamisel (db.Database.Migrate()), ka siis, kui vana tooded.db
// ei ole projekti migratsioonidega kooskõlas.
//
// Tüüpiline viga: "SQLite Error 1: 'table "Kasutajad" already exists'".
// Põhjus: tabelid on olemas, aga tabelis __EFMigrationsHistory pole projekti migratsioone
// (andmebaas loodi teiste, kohapeal tehtud migratsioonidega või EnsureCreated'iga).
// Siis üritab EF kõik tabelid uuesti luua. Add-Migration seda ei paranda.
//
// Lahendus:
//  1) iga migratsiooni kohta kontrollitakse, kas selle tabel/veerg on andmebaasis juba olemas –
//     kui on, kirjutatakse migratsioon ajalukku (seda ei käivitata uuesti);
//  2) kui Migrate() ikka ebaõnnestub (skeem pooleli), tõstetakse vana fail varukoopiaks
//     (tooded.db.vana-AAAAKKPP-TTMMSS) ja luuakse uus andmebaas.
public static class AndmebaasiParandus
{
    private const string Ajalugu = "__EFMigrationsHistory";
    private const string EfVersioon = "8.0.6";

    // Migratsiooni nimi (ID ilma kuupäevata) → tabel ja veerg, mille olemasolu näitab,
    // et migratsioon on juba tehtud (veerg null = piisab tabelist).
    // Migratsioonid, mis muudavad ainult andmeid (RimiKataloog), siin puuduvad – need käivitatakse.
    private static readonly Dictionary<string, (string Tabel, string? Veerg)> Tunnused = new()
    {
        ["InitialCreate"]    = ("OstuRead", null),
        ["RimiTooted"]       = ("Kliendid", "Email"),
        ["Ostjad"]           = ("Ostunimekiri", null),
        ["Tarne"]            = ("Kasutajad", "TsekkEmailile"),
        ["Lisafunktsioonid"] = ("Saavutused", null),
        ["TarneAeg"]         = ("Ostud", "TarneAeg"),
        ["PereKorv"]         = ("PereKorv", null),
    };

    // Kutsutakse Migrate() asemel. Tagastab teate kasutajale, kui andmebaas tuli uuesti luua.
    public static string? Uuenda()
    {
        try
        {
            using (var db = new ToodedContext())
            {
                MargiOlemasolevad(db);
                db.Database.Migrate();
            }
            return null;
        }
        catch (SqliteException ex) when (OnSkeemiKonflikt(ex))
        {
            string varukoopia = TeeVarukoopia();
            using (var db = new ToodedContext()) db.Database.Migrate();
            return "Vana andmebaas ei sobinud selle programmi versiooniga ja loodi uus.\n\n" +
                   "Vana fail on alles:\n" + varukoopia + "\n\n(" + ex.Message + ")";
        }
    }

    // Skeem on juba olemas, aga migratsiooni pole ajaloos
    private static bool OnSkeemiKonflikt(SqliteException ex) =>
        ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase);

    // Märgib ajalukku migratsioonid, mille tabelid/veerud on andmebaasis juba olemas
    private static void MargiOlemasolevad(ToodedContext db)
    {
        if (!File.Exists(Kaustad.AndmebaasiFail)) return;

        var yhendus = db.Database.GetDbConnection();
        db.Database.OpenConnection();
        try
        {
            if (!TabelOlemas(yhendus, "Kasutajad")) return;   // uus/tühi andmebaas – Migrate() teeb kõik

            Kaivita(yhendus,
                $"CREATE TABLE IF NOT EXISTS \"{Ajalugu}\" (" +
                "\"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, " +
                "\"ProductVersion\" TEXT NOT NULL)");

            var tehtud = new HashSet<string>(db.Database.GetAppliedMigrations());
            foreach (string id in db.Database.GetMigrations())
            {
                if (tehtud.Contains(id)) continue;
                string nimi = id.Substring(id.IndexOf('_') + 1);
                if (!Tunnused.TryGetValue(nimi, out var t)) continue;

                bool olemas = t.Veerg == null
                    ? TabelOlemas(yhendus, t.Tabel)
                    : VeergOlemas(yhendus, t.Tabel, t.Veerg);
                if (!olemas) continue;

                using var k = yhendus.CreateCommand();
                k.CommandText = $"INSERT OR IGNORE INTO \"{Ajalugu}\" (\"MigrationId\", \"ProductVersion\") VALUES ($id, $v)";
                AddParam(k, "$id", id);
                AddParam(k, "$v", EfVersioon);
                k.ExecuteNonQuery();
            }
        }
        finally
        {
            db.Database.CloseConnection();
        }
    }

    // Tõstab vana andmebaasi (ja WAL-failid) kõrvale; uus luuakse järgmise Migrate()-ga
    private static string TeeVarukoopia()
    {
        SqliteConnection.ClearAllPools();   // fail ei tohi olla lukus
        string siht = Kaustad.AndmebaasiFail + ".vana-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        File.Move(Kaustad.AndmebaasiFail, siht);
        foreach (string lisa in new[] { "-wal", "-shm" })
        {
            string f = Kaustad.AndmebaasiFail + lisa;
            if (File.Exists(f)) File.Move(f, siht + lisa);
        }
        return siht;
    }

    private static bool TabelOlemas(System.Data.Common.DbConnection yhendus, string tabel)
    {
        using var k = yhendus.CreateCommand();
        k.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $t";
        AddParam(k, "$t", tabel);
        return Convert.ToInt64(k.ExecuteScalar()) > 0;
    }

    private static bool VeergOlemas(System.Data.Common.DbConnection yhendus, string tabel, string veerg)
    {
        using var k = yhendus.CreateCommand();
        k.CommandText = "SELECT COUNT(*) FROM pragma_table_info($t) WHERE name = $v";
        AddParam(k, "$t", tabel);
        AddParam(k, "$v", veerg);
        return Convert.ToInt64(k.ExecuteScalar()) > 0;
    }

    private static void Kaivita(System.Data.Common.DbConnection yhendus, string sql)
    {
        using var k = yhendus.CreateCommand();
        k.CommandText = sql;
        k.ExecuteNonQuery();
    }

    private static void AddParam(System.Data.Common.DbCommand k, string nimi, object vaartus)
    {
        var p = k.CreateParameter();
        p.ParameterName = nimi;
        p.Value = vaartus;
        k.Parameters.Add(p);
    }
}
