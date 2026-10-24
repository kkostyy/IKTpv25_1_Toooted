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

    // ---- Taustal laadimine: programm avaneb kohe, tooted lisanduvad 2000 kaupa ----
    public static bool Kaib { get; private set; }
    public static (int Tehtud, int Kokku) Viimane { get; private set; }
    public static event Action<int, int>? Edenemine;   // (tehtud, kokku) – taustalõimest!
    public static event Action? Valmis;                 // taustalõimest!

    public static void AlustaTaustal()
    {
        if (Kaib) return;
        Kaib = true;
        var loim = new Thread(() =>
        {
            try
            {
                using var db = new ToodedContext();
                Laadi(db, null, (t, k) => { Viimane = (t, k); Edenemine?.Invoke(t, k); });
            }
            catch (Exception ex)
            {
                try { File.AppendAllText(Path.Combine(Kaustad.Juur, "kataloog_viga.txt"), DateTime.Now + " " + ex + "\n"); } catch { }
            }
            finally
            {
                Kaib = false;
                Valmis?.Invoke();
            }
        }) { IsBackground = true, Name = "Rimi kataloog" };
        loim.Start();
    }

    // Vanad tooted (ka demotooted) välja, Rimi tooted sisse. Tagastab toodete arvu.
    public static int Laadi(ToodedContext db, Action<string>? olek = null, Action<int, int>? edenemine = null)
    {
        var k = Loe(Fail) ?? throw new FileNotFoundException(Fail);
        olek?.Invoke(Keel.F("Laadin Rimi kataloogi: {0} toodet…", k.Tooted.Count));

        var tehing = db.Database.BeginTransaction();
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
        tehing.Commit();   // vanad tooted on läinud; uued lisanduvad osade kaupa (iga osa eraldi tehing – programm saab samal ajal lugeda)
        tehing.Dispose();
        edenemine?.Invoke(0, k.Tooted.Count);

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
            edenemine?.Invoke(i, k.Tooted.Count);
        }
        db.ChangeTracker.AutoDetectChangesEnabled = true;

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

// Taustal laadimise edenemine: väike kaart peaakna paremas alanurgas, riba liigub sujuvalt.
// Kuulajad (pood, tootetabel) saavad UI-lõimes teate iga laaditud osa järel.
public class KataloogiEdenemine : Form
{
    private static KataloogiEdenemine? aken;
    private static readonly List<(Control kes, Action mida)> kuulajad = new();

    private readonly System.Windows.Forms.Timer taimer = new() { Interval = 15 };
    private double naidatud, siht;   // 0..1
    private int tehtud, kokku;
    private bool valmis;
    private int lopuKaadrid;

    protected override bool ShowWithoutActivation => true;
    protected override CreateParams CreateParams
    {
        get { var cp = base.CreateParams; cp.ExStyle |= 0x08000000; return cp; }   // ei võta fookust
    }

    // Kontroll saab teate (UI-lõimes) iga uue tooteosa järel ja lõpus; eemaldatakse kontrolli sulgemisel
    public static void Kuula(Control kes, Action mida)
    {
        lock (kuulajad) kuulajad.Add((kes, mida));
        kes.Disposed += (s, e) => { lock (kuulajad) kuulajad.RemoveAll(k => k.kes == kes); };
    }

    private static void Teavita()
    {
        List<(Control kes, Action mida)> koopia;
        lock (kuulajad) koopia = kuulajad.ToList();
        foreach (var (kes, mida) in koopia)
            if (kes.IsHandleCreated && !kes.IsDisposed)
                try { kes.BeginInvoke(mida); } catch { }
    }

    // Kutsutakse Program.cs-st enne peaakna avamist
    public static void Alusta()
    {
        if (!RimiKataloog.Kaib) return;
        RimiKataloog.Edenemine += (t, k) => { Teavita(); aken?.Uuenda(t, k, false); };
        RimiKataloog.Valmis += () => { Teavita(); aken?.Uuenda(0, 0, true); };
        Application.Idle += NaitaKuiVoimalik;
    }

    private static void NaitaKuiVoimalik(object? s, EventArgs e)
    {
        var pea = Application.OpenForms.OfType<Form1>().FirstOrDefault(f => f.Visible);
        if (pea == null) return;
        Application.Idle -= NaitaKuiVoimalik;
        if (!RimiKataloog.Kaib) { Teavita(); return; }   // jõudis enne peaakent valmis
        aken = new KataloogiEdenemine();
        aken.Show(pea);
        aken.Paiguta(pea);
        pea.LocationChanged += (_, _) => aken?.Paiguta(pea);
        pea.SizeChanged += (_, _) => aken?.Paiguta(pea);
    }

    private KataloogiEdenemine()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Size = new Size(340, 74);
        BackColor = Teema.Tume ? Color.FromArgb(58, 60, 66) : Color.FromArgb(38, 40, 44);
        Font = new Font("Segoe UI", 9.75F);
        DoubleBuffered = true;
        Opacity = 0;
        using (var p = Ymardus.Tee(new Rectangle(0, 0, Width, Height), 12)) Region = new Region(p);
        (tehtud, kokku) = RimiKataloog.Viimane;
        siht = naidatud = kokku > 0 ? (double)tehtud / kokku : 0;
        taimer.Tick += (s, e) => Samm();
        taimer.Start();
    }

    private void Paiguta(Form pea)
    {
        if (IsDisposed) return;
        var ala = pea.RectangleToScreen(pea.ClientRectangle);
        Location = new Point(ala.Right - Width - 20, ala.Bottom - Height - 20);
    }

    private void Uuenda(int t, int k, bool lopp)
    {
        if (IsDisposed || !IsHandleCreated) return;
        try
        {
            BeginInvoke(() =>
            {
                if (lopp) { valmis = true; siht = 1; return; }
                tehtud = t; kokku = k;
                siht = k > 0 ? (double)t / k : 0;
            });
        }
        catch { }
    }

    private void Samm()
    {
        // ilmub sujuvalt; riba liigub sihtväärtuse poole (ease-out), mitte hüppega
        if (!valmis && Opacity < 0.96) Opacity = Math.Min(0.96, Opacity + 0.06);
        naidatud += (siht - naidatud) * 0.08;
        if (valmis && siht - naidatud < 0.002)
        {
            naidatud = 1;
            if (++lopuKaadrid > 90) Opacity -= 0.05;   // ~1,3 s „Valmis“, siis kaob
            if (Opacity <= 0.05) { taimer.Stop(); Close(); return; }
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var roheline = Color.FromArgb(46, 160, 92);
        string pealkiri = valmis ? Keel.T("Rimi kataloog laaditud") : Keel.T("Laadin Rimi tooteid taustal…");
        string alam = valmis ? Keel.F("{0} toodet", kokku) : Keel.F("{0} / {1} toodet · võid samal ajal tööd teha", tehtud, kokku);
        TextRenderer.DrawText(g, pealkiri, new Font(Font, FontStyle.Bold), new Point(18, 12), Color.White, TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g, alam, Font, new Point(18, 33), Color.FromArgb(190, 194, 200), TextFormatFlags.NoPrefix);
        var riba = new Rectangle(18, Height - 14, Width - 36, 5);
        using (var p = Ymardus.Tee(riba, 2)) using (var b = new SolidBrush(Color.FromArgb(80, 84, 92))) g.FillPath(b, p);
        int w = Math.Max(5, (int)(riba.Width * naidatud));
        using (var p = Ymardus.Tee(new Rectangle(riba.X, riba.Y, w, riba.Height), 2)) using (var b = new SolidBrush(roheline)) g.FillPath(b, p);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { taimer.Dispose(); if (aken == this) aken = null; }
        base.Dispose(disposing);
    }
}
