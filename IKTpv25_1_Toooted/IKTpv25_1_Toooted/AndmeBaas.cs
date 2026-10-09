using System;
using System.Data.SqlClient;
using System.IO;

namespace IKTpv25_1_Toooted
{
    // Andmebaasi ühendus + tabelite automaatne loomine
    public static class AndmeBaas
    {
        public const string Uhendus =
            @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Tooded.mdf;Integrated Security=True";

        public static string AndmeKaust
        {
            get
            {
                return (AppDomain.CurrentDomain.GetData("DataDirectory") as string)
                       ?? AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        // Siia kopeeritakse toodete pildid (andmebaasis hoitakse ainult failinime)
        public static string PildiKaust
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pildid"); }
        }

        public static void Valmista()
        {
            LooFailKuiPuudub();
            LooTabelid();
        }

        // Kui Tooded.mdf puudub (pole Visual Studios lisatud), luuakse see ise
        private static void LooFailKuiPuudub()
        {
            string fail = Path.Combine(AndmeKaust, "Tooded.mdf");
            if (File.Exists(fail)) return;

            Directory.CreateDirectory(AndmeKaust);
            string logi = Path.Combine(AndmeKaust, "Tooded_log.ldf");
            string nimi = "Tooded_" + Guid.NewGuid().ToString("N");

            using (var c = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=master"))
            {
                c.Open();
                Kask(c, "CREATE DATABASE [" + nimi + "] ON (NAME=N'" + nimi + "', FILENAME=N'" + Kaitse(fail) +
                        "') LOG ON (NAME=N'" + nimi + "_log', FILENAME=N'" + Kaitse(logi) + "')");
                Kask(c, "EXEC master.dbo.sp_detach_db @dbname=N'" + nimi + "', @skipchecks='true'");
            }
        }

        private static void LooTabelid()
        {
            using (var c = new SqlConnection(Uhendus))
            {
                c.Open();
                Kask(c, @"IF OBJECT_ID('dbo.Kategooriad','U') IS NULL
                          CREATE TABLE Kategooriad (
                              Id INT IDENTITY(1,1) PRIMARY KEY,
                              Nimetus NVARCHAR(50)
                          )");
                Kask(c, @"IF OBJECT_ID('dbo.Tooted','U') IS NULL
                          CREATE TABLE Tooted (
                              Id INT IDENTITY(1,1) PRIMARY KEY,
                              Toodenimetus NVARCHAR(100),
                              Kogus INT,
                              Hind DECIMAL(10,2),
                              Pilt NVARCHAR(255),
                              Kategooria_Id INT,
                              FOREIGN KEY (Kategooria_Id) REFERENCES Kategooriad(Id)
                          )");
                Kask(c, @"IF NOT EXISTS (SELECT 1 FROM Kategooriad)
                          INSERT INTO Kategooriad (Nimetus) VALUES (N'Toidutooted'), (N'Joogid'), (N'Maiustused')");
            }
        }

        private static void Kask(SqlConnection c, string sql)
        {
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static string Kaitse(string s)
        {
            return s.Replace("'", "''");
        }
    }
}
