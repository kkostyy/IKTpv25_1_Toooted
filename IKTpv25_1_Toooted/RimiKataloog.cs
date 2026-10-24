using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// Rimi e-poe kataloog (fail Andmed\rimi_kataloog.json.gz, loodud skriptiga tools/rimi).
// Esimesel käivitamisel (või kui kataloogi versioon on uuem) kustutatakse vanad tooted ja
// laaditakse kõik Rimi tooted. Ostude ajalugu jääb alles (tšekil on nimi ja hind).
public static class RimiKataloog
{
    public static string Fail => Path.Combine(AppContext.BaseDirectory, "Andmed", "rimi_kataloog.json.gz");
    private static string Marker => Path.Combine(Kaustad.Juur, "kataloog.txt");

    public record KatKirje(int Id, string Nimi);

    public record TooteKirje(
        [property: JsonPropertyName("n")] string Nimi,
        [property: JsonPropertyName("h")] decimal Hind,
        [property: JsonPropertyName("v")] decimal? VanaHind,
        [property: JsonPropertyName("k")] int Kategooria,
        [property: JsonPropertyName("p")] string? Pilt,
        [property: JsonPropertyName("b")] string? Kaubamark,
        [property: JsonPropertyName("t")] string? Tootja,
        [property: JsonPropertyName("m")] string? Maa,
        [property: JsonPropertyName("pk")] string? Pakend,
        [property: JsonPropertyName("u")] string? Uhik,
        [property: JsonPropertyName("uk")] decimal? UhikuKogus,
        [property: JsonPropertyName("ki")] string? Kirjeldus,
        [property: JsonPropertyName("ko")] string? Koostis,
        [property: JsonPropertyName("li")] string? Lisainfo,
        [property: JsonPropertyName("tv")] string? Toitevaartus,
        [property: JsonPropertyName("pa")] decimal Pant,
        [property: JsonPropertyName("l")] int Laos,
        [property: JsonPropertyName("oh")] decimal? Ostuhind);

    public record Kataloog(string Versioon, List<KatKirje> Kategooriad, List<TooteKirje> Tooted);

    public static Kataloog? Loe(string fail)
    {
        if (!File.Exists(fail)) return null;
        using var f = File.OpenRead(fail);
        using var gz = new GZipStream(f, CompressionMode.Decompress);
        return JsonSerializer.Deserialize<Kataloog>(gz, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // Kas tuleb laadida: fail on olemas ja (versioon on uus või tooteid pole)
    public static bool VajabLaadimist(ToodedContext db, out string? versioon)
    {
        versioon = null;
        if (!File.Exists(Fail)) return false;
        string? tehtud = File.Exists(Marker) ? File.ReadAllText(Marker).Trim() : null;
        versioon = LoeVersioon();
        return versioon != null && (tehtud != versioon || !db.Toodetabel.Any());
    }

    private static string? LoeVersioon()
    {
        // versioon on JSON-i alguses: {"versioon":"2026-10-24",...}
        using var f = File.OpenRead(Fail);
        using var gz = new GZipStream(f, CompressionMode.Decompress);
        var puhver = new byte[200];
        int n = gz.ReadAtLeast(puhver, puhver.Length, throwOnEndOfStream: false);
        var m = System.Text.RegularExpressions.Regex.Match(System.Text.Encoding.UTF8.GetString(puhver, 0, n), "\"versioon\"\\s*:\\s*\"([^\"]+)\"");
        return m.Success ? m.Groups[1].Value : null;
    }

    // Vanad tooted (ka demotooted) välja, Rimi tooted sisse. Tagastab toodete arvu.
    public static int Laadi(ToodedContext db, Action<string>? olek = null)
    {
        var k = Loe(Fail) ?? throw new FileNotFoundException(Fail);
        olek?.Invoke(Keel.F("Laadin Rimi kataloogi: {0} toodet…", k.Tooted.Count));

        using var tehing = db.Database.BeginTransaction();
        // viited tootele: ostu read ja ostunimekiri jäävad alles (ToodeId = null), partiid ja kampaaniad kustuvad
        db.Database.ExecuteSqlRaw("UPDATE OstuRead SET ToodeId = NULL");
        db.Database.ExecuteSqlRaw("UPDATE Ostunimekiri SET ToodeId = NULL");
        db.Database.ExecuteSqlRaw("DELETE FROM Partiid");
        db.Database.ExecuteSqlRaw("DELETE FROM Kampaaniad");
        db.Database.ExecuteSqlRaw("DELETE FROM Toodetabel");

        var olemas = db.Kategooriatabel.ToDictionary(x => x.Id);
        foreach (var kat in k.Kategooriad)
        {
            if (olemas.TryGetValue(kat.Id, out var o)) o.Kategooria_nimetus = kat.Nimi;
            else db.Kategooriatabel.Add(new Kategooria { Id = kat.Id, Kategooria_nimetus = kat.Nimi });
        }
        db.SaveChanges();

        db.ChangeTracker.AutoDetectChangesEnabled = false;
        int i = 0;
        foreach (var osa in k.Tooted.Chunk(2000))
        {
            db.Toodetabel.AddRange(osa.Select(t => new Toode
            {
                Toodenimetus = t.Nimi,
                Hind = t.Hind,
                VanaHind = t.VanaHind,
                KategooriaId = t.Kategooria,
                Pilt = t.Pilt,
                Kaubamark = t.Kaubamark,
                Tootja = t.Tootja,
                Paritolumaa = t.Maa,
                Pakend = t.Pakend,
                Uhik = t.Uhik,
                UhikuKogus = t.UhikuKogus,
                Kirjeldus = t.Kirjeldus,
                Koostisosad = t.Koostis,
                Lisainfo = t.Lisainfo,
                Toitevaartus = t.Toitevaartus,
                Pant = t.Pant,
                Kogus = t.Laos,
                Ostuhind = t.Ostuhind
            }));
            db.SaveChanges();
            db.ChangeTracker.Clear();
            i += osa.Length;
            olek?.Invoke(Keel.F("Laadin Rimi kataloogi: {0} / {1}", i, k.Tooted.Count));
        }
        db.ChangeTracker.AutoDetectChangesEnabled = true;
        tehing.Commit();

        Directory.CreateDirectory(Kaustad.Juur);
        File.WriteAllText(Marker, k.Versioon);
        return k.Tooted.Count;
    }
}

// Väike aken laadimise ajaks (esimene käivitamine)
public class LaadimiseAken : Form
{
    private readonly Label silt;

    public LaadimiseAken()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(460, 120);
        BackColor = Color.FromArgb(27, 28, 31);
        ShowInTaskbar = true;
        Text = "AKT";
        Controls.Add(new Label { Text = "AKT", Location = new Point(24, 20), AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.White });
        silt = new Label { Location = new Point(24, 64), Size = new Size(420, 40), Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(200, 203, 208) };
        Controls.Add(silt);
    }

    public void Olek(string tekst)
    {
        silt.Text = tekst;
        Refresh();
        Application.DoEvents();
    }
}
