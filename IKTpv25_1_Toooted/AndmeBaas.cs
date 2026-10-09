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
                // Vaikimisi 10 populaarset Eesti toodet (lisatakse ainult siis, kui tabel on tühi)
                Kask(c, @"IF NOT EXISTS (SELECT 1 FROM Tooted)
                          INSERT INTO Tooted (Toodenimetus, Kogus, Hind, Pilt, Kategooria_Id) VALUES
                              (N'Rukkileib', 50, 1.29, N'rukkileib.png', (SELECT TOP 1 Id FROM Kategooriad WHERE Nimetus = N'Toidutooted')),
                              (N'Kama', 40, 1.89, N'kama.png', (SELECT TOP 1 Id FROM Kategooriad WHERE Nimetus = N'Toidutooted')),
                              (N'Kilu tomatikastmes', 60, 1.35, N'kilu.png', (SELECT TOP 1 Id FROM Kategooriad WHERE Nimetus = N'Toidutooted')),
                              (N'Verivorst', 25, 3.49, N'verivorst.png', (SELECT TOP 1 Id FROM Kategooriad WHERE Nimetus = N'Toidutooted')),
                              (N'Kali', 80, 1.19, N'kali.png', (SELECT TOP 1 Id FROM Kategooriad WHERE Nimetus = N'Joogid')),
                              (N'A. Le Coq Premium', 100, 1.49, N'alecoq.png', (SELECT TOP 1 Id FROM Kategooriad WHERE Nimetus = N'Joogid')),
                              (N'Vana Tallinn', 20, 12.99, N'vanatallinn.png', (SELECT TOP 1 Id FROM Kategooriad WHERE Nimetus = N'Joogid')),
                              (N'Kalev šokolaad', 70, 1.99, N'kalev_sokolaad.png', (SELECT TOP 1 Id FROM Kategooriad WHERE Nimetus = N'Maiustused')),
                              (N'Kohuke', 90, 0.89, N'kohuke.png', (SELECT TOP 1 Id FROM Kategooriad WHERE Nimetus = N'Maiustused')),
                              (N'Kalev Mesikäpp', 60, 0.99, N'mesikapp.png', (SELECT TOP 1 Id FROM Kategooriad WHERE Nimetus = N'Maiustused'))");
                // Olemasolevatele toodetele lisatakse pildid, kui pilt puudub
                Kask(c, "UPDATE Tooted SET Pilt = N'rukkileib.png' WHERE Toodenimetus = N'Rukkileib' AND (Pilt IS NULL OR Pilt = N'')");
                Kask(c, "UPDATE Tooted SET Pilt = N'kama.png' WHERE Toodenimetus = N'Kama' AND (Pilt IS NULL OR Pilt = N'')");
                Kask(c, "UPDATE Tooted SET Pilt = N'kilu.png' WHERE Toodenimetus = N'Kilu tomatikastmes' AND (Pilt IS NULL OR Pilt = N'')");
                Kask(c, "UPDATE Tooted SET Pilt = N'verivorst.png' WHERE Toodenimetus = N'Verivorst' AND (Pilt IS NULL OR Pilt = N'')");
                Kask(c, "UPDATE Tooted SET Pilt = N'kali.png' WHERE Toodenimetus = N'Kali' AND (Pilt IS NULL OR Pilt = N'')");
                Kask(c, "UPDATE Tooted SET Pilt = N'alecoq.png' WHERE Toodenimetus = N'A. Le Coq Premium' AND (Pilt IS NULL OR Pilt = N'')");
                Kask(c, "UPDATE Tooted SET Pilt = N'vanatallinn.png' WHERE Toodenimetus = N'Vana Tallinn' AND (Pilt IS NULL OR Pilt = N'')");
                Kask(c, "UPDATE Tooted SET Pilt = N'kalev_sokolaad.png' WHERE Toodenimetus = N'Kalev šokolaad' AND (Pilt IS NULL OR Pilt = N'')");
                Kask(c, "UPDATE Tooted SET Pilt = N'kohuke.png' WHERE Toodenimetus = N'Kohuke' AND (Pilt IS NULL OR Pilt = N'')");
                Kask(c, "UPDATE Tooted SET Pilt = N'mesikapp.png' WHERE Toodenimetus = N'Kalev Mesikäpp' AND (Pilt IS NULL OR Pilt = N'')");
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
