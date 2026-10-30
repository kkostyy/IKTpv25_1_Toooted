using System.Runtime.CompilerServices;
using System.Text.Json;

namespace IKTpv25_1_Toooted;

public enum KeeleKood { ET, RU, EN }

// Programmi keel: eesti (vaikimisi), vene, inglise.
// Kood on kirjutatud eesti keeles; Keel.T("eesti tekst") annab valitud keele teksti.
// Vormide sildid, nupud, menüüd ja tabelite päised tõlgitakse automaatselt (Teema.Rakenda kaudu).
public static partial class Keel
{
    private static KeeleKood praegu = KeeleKood.ET;
    [ThreadStatic] private static KeeleKood? ajutine;   // ainult selles lõimes (nt tšekk / e-kiri teises keeles)

    public static KeeleKood Praegu { get => ajutine ?? praegu; private set => praegu = value; }

    // Ajutiselt teine keel (tšekk ja e-kiri kliendi keeles, programmi keel ei muutu):
    //   using (Keel.Ajutiselt(KeeleKood.ET)) { ... }
    public static IDisposable Ajutiselt(KeeleKood k)
    {
        var vana = ajutine;
        ajutine = k;
        return new Taastaja(() => ajutine = vana);
    }

    private sealed class Taastaja : IDisposable
    {
        private Action? taasta;
        public Taastaja(Action taasta) => this.taasta = taasta;
        public void Dispose() { taasta?.Invoke(); taasta = null; }
    }

    public static string Nimi(KeeleKood k) => k switch { KeeleKood.RU => "Русский", KeeleKood.EN => "English", _ => "Eesti" };

    // ---- Tõlkimine ----
    public static string T(string et)
    {
        if (Praegu == KeeleKood.ET || string.IsNullOrEmpty(et)) return et;
        if (sonastik.TryGetValue(et, out var t)) return Vali(t, et);

        // SUURTÄHTEDES silt (nt kaardi pealkiri "OSTUKORV")
        if (et.Length > 1 && et == et.ToUpper() && sonastik.TryGetValue(et.ToLower(), out var t2))
            return Vali(t2, et).ToUpper();

        // Lõpus "..." või ":" (menüüd, sildid)
        foreach (string lopp in new[] { "...", "…", ":" })
            if (et.EndsWith(lopp) && sonastik.TryGetValue(et[..^lopp.Length], out var t3))
                return Vali(t3, et) + lopp;

        return et;
    }

    // Mall + väärtused: Keel.F("Laos {0} tk", 5)
    public static string F(string etMall, params object?[] vaartused) => string.Format(T(etMall), vaartused);

    private static string Vali((string ru, string en) t, string et) =>
        Praegu == KeeleKood.RU ? (t.ru.Length > 0 ? t.ru : et) : (t.en.Length > 0 ? t.en : et);

    // Pikem tekst (teated): terve tekst, muidu rida-realt, muidu tuntud algus ("Viga lisamisel:" + viga)
    public static string TolgiTekst(string tekst)
    {
        if (Praegu == KeeleKood.ET || string.IsNullOrEmpty(tekst)) return tekst;
        string kogu = T(tekst);
        if (!ReferenceEquals(kogu, tekst) && kogu != tekst) return kogu;

        var read = tekst.Split('\n');
        for (int i = 0; i < read.Length; i++)
        {
            string r = read[i];
            string tr = T(r.Trim());
            if (tr != r.Trim()) { read[i] = r.Replace(r.Trim(), tr); continue; }

            // tuntud algus: "Viga: ...", "Salvestamine ebaõnnestus: ..."
            foreach (var voti in algused)
                if (r.StartsWith(voti, StringComparison.Ordinal))
                {
                    read[i] = T(voti) + r[voti.Length..];
                    break;
                }
        }
        return string.Join("\n", read);
    }

    // ---- Vormide elementide tõlkimine (algne eestikeelne tekst jäetakse meelde) ----
    private class Algne { public string? Tekst, Viimane, Vihje, ViimaneVihje; }
    private static readonly ConditionalWeakTable<object, Algne> algsed = new();

    public static void TolgiElement(Control c)
    {
        if (c is TextBoxBase && c is not TextBox) return;
        var a = algsed.GetValue(c, k => new Algne());

        if (c is not TextBox && c is not ComboBox && c is not NumericUpDown)
        {
            // Kood muutis teksti vahepeal ise → see on uus algne tekst
            if (c.Text != a.Viimane) a.Tekst = c.Text;
            string alg = a.Tekst ?? "";
            string uus = alg.Contains('\n') ? TolgiTekst(alg) : T(alg);
            if (c.Text != uus) c.Text = uus;
            a.Viimane = uus;
        }
        if (c is TextBox t)
        {
            if (t.PlaceholderText != a.ViimaneVihje) a.Vihje = t.PlaceholderText;
            string uus = T(a.Vihje ?? "");
            if (t.PlaceholderText != uus) t.PlaceholderText = uus;
            a.ViimaneVihje = uus;
        }
    }

    public static void TolgiMenuu(ToolStripItem i)
    {
        if (i.Tag is KeeleKood) return;   // keelte nimed jäävad oma keelde
        var a = algsed.GetValue(i, k => new Algne());
        if (i.Text != a.Viimane) a.Tekst = i.Text;
        a.Viimane = T(a.Tekst ?? "");
        if (i.Text != a.Viimane) i.Text = a.Viimane;
        if (i is ToolStripMenuItem m)
            foreach (ToolStripItem alam in m.DropDownItems) TolgiMenuu(alam);
    }

    public static void TolgiTabel(DataGridView dgv)
    {
        foreach (DataGridViewColumn col in dgv.Columns)
        {
            var a = algsed.GetValue(col, k => new Algne());
            if (col.HeaderText != a.Viimane) a.Tekst = col.HeaderText;
            a.Viimane = T(a.Tekst ?? "");
            if (col.HeaderText != a.Viimane) col.HeaderText = a.Viimane;
        }
    }

    // ---- Keele vahetus ----
    public static event Action? Muutus;

    public static void Sea(KeeleKood k)
    {
        if (Praegu == k) return;
        Praegu = k;
        try
        {
            Directory.CreateDirectory(Kaustad.Juur);
            File.WriteAllText(Fail, JsonSerializer.Serialize(k.ToString()));
        }
        catch { }
        Muutus?.Invoke();
        Teema.TeavitaMuutusest();   // kõik avatud vormid värvivad ja tõlgivad end uuesti
    }

    private static string Fail => Path.Combine(Kaustad.Juur, "keel.json");

    public static void Lae()
    {
        try
        {
            if (File.Exists(Fail) && Enum.TryParse(JsonSerializer.Deserialize<string>(File.ReadAllText(Fail)), out KeeleKood k))
                Praegu = k;
        }
        catch { }
    }

    // ---- Sõnastik: eesti → (vene, inglise) ----
    private static readonly Dictionary<string, (string ru, string en)> sonastik = new(StringComparer.Ordinal);
    private static readonly List<string> algused = new();

    private static void L(string et, string ru, string en)
    {
        sonastik[et] = (ru, en);
        string vaike = et.ToLower();
        if (!sonastik.ContainsKey(vaike)) sonastik[vaike] = (ru.ToLower(), en.ToLower());
    }

    // Teadete algused, mille järel tuleb muutuv tekst (nt veateade)
    private static void A(string et, string ru, string en)
    {
        L(et, ru, en);
        algused.Add(et);
    }

    // Tõlked on failis Keel/tolked.json (programmi sisse põimitud ressurss):
    //   "eestikeelne tekst": { "ru": "vene", "en": "inglise" }
    //   "algus": true – teate algus, mille järel tuleb muutuv tekst (nt "Viga:" + veateade)
    // Uue tõlke lisamiseks lisa rida JSON-faili (programmi koodi pole vaja muuta).
    static Keel()
    {
        try
        {
            using var voog = typeof(Keel).Assembly.GetManifestResourceStream("tolked.json");
            if (voog == null) return;
            using var dok = JsonDocument.Parse(voog);
            foreach (var kirje in dok.RootElement.EnumerateObject())
            {
                string ru = kirje.Value.TryGetProperty("ru", out var r) ? r.GetString() ?? "" : "";
                string en = kirje.Value.TryGetProperty("en", out var e) ? e.GetString() ?? "" : "";
                bool algus = kirje.Value.TryGetProperty("algus", out var a) && a.ValueKind == JsonValueKind.True;
                if (algus) A(kirje.Name, ru, en); else L(kirje.Name, ru, en);
            }
        }
        catch { }   // vigane fail: programm töötab eesti keeles edasi
    }
}
