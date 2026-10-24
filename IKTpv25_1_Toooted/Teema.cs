using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace IKTpv25_1_Toooted;

public enum Disain { Rimi, Klassikaline, Moodne }

// Värviroll: mida see värv kasutajaliideses tähendab (sama roll = sama värv igas teemas)
public enum Roll
{
    Aktsent, AktsentHele, Taust, Paneel, Paneel2, Pind, PindAlt, PildiTaust,
    Joon, Raam, Tekst, Tekst2, TekstNork, Viga, Hoiatus, Edu, Valge
}

// Hele/tume teema + poe värv (aktsent). Valik salvestatakse faili AppData\Local\ToodedApp\teema.json.
// Teema.Rakenda(vorm) värvib kõik elemendid ümber – ka Designeris loodud (Designer-faili ei muudeta).
public static class Teema
{
    // ---- Poe värvid: (hele teema, tume teema) ----
    public static readonly Dictionary<string, (Color Hele, Color Tume)> Aktsendid = new()
    {
        ["Punane"] = (Color.FromArgb(226, 6, 19), Color.FromArgb(240, 80, 90)),
        ["Roheline"] = (Color.FromArgb(16, 140, 80), Color.FromArgb(52, 190, 120)),
        ["Sinine"] = (Color.FromArgb(0, 120, 215), Color.FromArgb(76, 160, 240)),
        ["Oranž"] = (Color.FromArgb(215, 95, 10), Color.FromArgb(245, 145, 60)),
        ["Lilla"] = (Color.FromArgb(120, 70, 200), Color.FromArgb(165, 125, 240)),
    };

    public static bool Tume { get; private set; }

    // Disain: e-poe (Rimi) stiil või esimene, klassikaline (sinine, Wordi-laadne). Vahetus: Ctrl+Shift+D
    public static Disain Stiil { get; private set; } = Disain.Rimi;
    public static bool OnRimi => Stiil != Disain.Klassikaline;   // e-poe disain (ka Moodne põhineb sellel)
    // Moodne: kaardid ilma raamita, suured ümarused, nupud "pillid", vajutus väikseks (Ctrl+Shift+M)
    public static bool OnMoodne => Stiil == Disain.Moodne;

    // WS_EX_COMPOSITED (kogu aken joonistatakse korraga, animatsioonid ei rebene); AKT_KOMPOSIIT=0 lülitab välja
    public static bool Komposiit => OnMoodne && Environment.GetEnvironmentVariable("AKT_KOMPOSIIT") != "0";
    public static string AktsendiNimi { get; private set; } = "Punane";

    // Ümarad nurgad (suuremad raadiused) ja külgmenüü
    public static bool Umarad { get; private set; } = true;

    public static void SeaUmarad(bool umarad)
    {
        Umarad = umarad;
        Salvesta();
        Muutus?.Invoke();
    }

    // Käivitub, kui teema muutub (avatud vormid värvivad end ümber)
    public static event Action? Muutus;

    // ---- Värvid ----
    public static Color Aktsent => Tume ? Aktsendid[AktsendiNimi].Tume : Aktsendid[AktsendiNimi].Hele;
    public static Color AktsentHele => Ymardus.Segu(Pind, Aktsent, Tume ? 0.32f : 0.14f);
    public static Color AktsentTume => Ymardus.Segu(Aktsent, Color.Black, 0.18f);

    // Teine värv: lingid, kategooriate menüü (nagu e-poodides lilla-punane)
    // Teine värv: Rimi stiilis lilla (lingid, kategooriad), klassikalises stiilis sama mis aktsent
    public static Color Teine => !OnRimi ? Aktsent : Tume ? Color.FromArgb(235, 120, 175) : Color.FromArgb(160, 35, 110);

    // Heledad toonid: e-pood = sinakashall taust; klassikaline = esimese kujunduse hallid
    public static Color Taust => Tume ? Color.FromArgb(24, 25, 28) : OnMoodne ? Color.FromArgb(233, 237, 239) : OnRimi ? Color.FromArgb(238, 241, 242) : Color.FromArgb(235, 237, 240);
    public static Color Paneel => Tume ? Color.FromArgb(34, 35, 39) : OnRimi ? Color.FromArgb(246, 248, 249) : Color.FromArgb(245, 246, 248);
    public static Color Paneel2 => Tume ? Color.FromArgb(50, 52, 57) : OnMoodne ? Color.FromArgb(241, 243, 244) : OnRimi ? Color.FromArgb(232, 237, 239) : Color.FromArgb(232, 234, 238);
    public static Color Pind => Tume ? Color.FromArgb(40, 41, 46) : Color.White;
    public static Color PindAlt => Tume ? Color.FromArgb(46, 47, 52) : OnRimi ? Color.FromArgb(247, 249, 250) : Color.FromArgb(248, 249, 251);
    public static Color PildiTaust => Tume ? Color.FromArgb(54, 56, 61) : OnRimi ? Color.FromArgb(240, 243, 244) : Color.FromArgb(240, 242, 245);
    public static Color Joon => Tume ? Color.FromArgb(62, 64, 70) : OnMoodne ? Color.FromArgb(226, 230, 232) : OnRimi ? Color.FromArgb(221, 226, 229) : Color.FromArgb(210, 214, 220);
    public static Color Raam => Tume ? Color.FromArgb(82, 85, 92) : OnRimi ? Color.FromArgb(205, 211, 215) : Color.FromArgb(200, 204, 210);
    public static Color Tekst => Tume ? Color.FromArgb(234, 236, 239) : OnMoodne ? Color.FromArgb(27, 28, 31) : OnRimi ? Color.FromArgb(46, 46, 46) : Color.FromArgb(40, 40, 40);

    // Vana kujunduse (Designer-faili) värvid → roll
    private static readonly Dictionary<int, Roll> vanadVarvid = new()
    {
        [Color.FromArgb(235, 237, 240).ToArgb()] = Roll.Taust,
        [Color.FromArgb(245, 246, 248).ToArgb()] = Roll.Paneel,
        [Color.FromArgb(232, 234, 238).ToArgb()] = Roll.Paneel2,
        [Color.FromArgb(248, 249, 251).ToArgb()] = Roll.PindAlt,
        [Color.FromArgb(240, 242, 245).ToArgb()] = Roll.PildiTaust,
        [Color.FromArgb(210, 214, 220).ToArgb()] = Roll.Joon,
        [Color.FromArgb(200, 204, 210).ToArgb()] = Roll.Raam,
        [Color.FromArgb(40, 40, 40).ToArgb()] = Roll.Tekst,
    };
    public static Color Tekst2 => Tume ? Color.FromArgb(200, 203, 208) : Color.FromArgb(70, 75, 82);
    public static Color TekstNork => Tume ? Color.FromArgb(150, 155, 162) : Color.FromArgb(110, 116, 125);
    public static Color Viga => Tume ? Color.FromArgb(240, 105, 105) : Color.FromArgb(200, 30, 30);
    public static Color Hoiatus => Tume ? Color.FromArgb(240, 170, 70) : Color.FromArgb(205, 115, 0);
    public static Color Edu => Tume ? Color.FromArgb(90, 200, 130) : Color.FromArgb(20, 130, 70);
    public static Color VigaTaust => Tume ? Color.FromArgb(78, 40, 42) : Color.FromArgb(255, 235, 235);
    public static Color VigaTaust2 => Tume ? Color.FromArgb(105, 40, 44) : Color.FromArgb(255, 205, 205);

    public static Color Varv(Roll r) => r switch
    {
        Roll.Aktsent => Aktsent,
        Roll.AktsentHele => AktsentHele,
        Roll.Taust => Taust,
        Roll.Paneel => Paneel,
        Roll.Paneel2 => Paneel2,
        Roll.Pind => Pind,
        Roll.PindAlt => PindAlt,
        Roll.PildiTaust => PildiTaust,
        Roll.Joon => Joon,
        Roll.Raam => Raam,
        Roll.Tekst => Tekst,
        Roll.Tekst2 => Tekst2,
        Roll.TekstNork => TekstNork,
        Roll.Viga => Viga,
        Roll.Hoiatus => Hoiatus,
        Roll.Edu => Edu,
        _ => Color.White
    };

    public static bool OnAktsent(Color c) =>
        Aktsendid.Values.Any(a => a.Hele.ToArgb() == c.ToArgb() || a.Tume.ToArgb() == c.ToArgb());

    // Leiab värvi rolli (algne hele kujundus või praegune teema)
    private static Roll? LeiaRoll(Color c, bool onTekst)
    {
        if (c.IsEmpty || c == Color.Transparent) return null;
        if (OnAktsent(c)) return Roll.Aktsent;

        int argb = c.ToArgb();
        if (onTekst)
        {
            if (argb == Color.White.ToArgb()) return Roll.Valge;
            if (argb == Color.Black.ToArgb() || argb == SystemColors.ControlText.ToArgb() ||
                argb == SystemColors.WindowText.ToArgb()) return Roll.Tekst;
            if (argb == Color.Gray.ToArgb() || argb == Color.DimGray.ToArgb()) return Roll.TekstNork;
            if (argb == Color.DarkGreen.ToArgb()) return Roll.Edu;
        }
        else
        {
            if (argb == Color.White.ToArgb() || argb == SystemColors.Window.ToArgb()) return Roll.Pind;
            if (argb == SystemColors.Control.ToArgb()) return Roll.Taust;
            if (argb == Color.FromArgb(214, 232, 250).ToArgb()) return Roll.AktsentHele;
        }

        if (vanadVarvid.TryGetValue(argb, out Roll vana)) return vana;

        // Võrdleme kõigi teemade ja mõlema disaini värvidega
        bool praegu = Tume;
        Disain praeguneStiil = Stiil;
        Roll? leitud = null;
        foreach (Disain d in new[] { Disain.Rimi, Disain.Klassikaline, Disain.Moodne })
        {
            foreach (bool tume in new[] { false, true })
            {
                Tume = tume;
                Stiil = d;
                foreach (Roll r in Enum.GetValues<Roll>())
                {
                    if (r is Roll.Aktsent or Roll.Valge) continue;
                    if (Varv(r).ToArgb() == argb) { leitud = r; break; }
                }
                if (leitud != null) break;
            }
            if (leitud != null) break;
        }
        Tume = praegu;
        Stiil = praeguneStiil;
        // tume taust (nt kaardi tume värv = teksti värv) ei tohi tumedas teemas valgeks minna
        if (!onTekst && leitud is Roll.Tekst or Roll.Tekst2 or Roll.TekstNork) return null;
        return leitud;
    }

    // ---- Iga elemendi algsed rollid (leitakse esimesel korral) ----
    private class Rollid
    {
        public Roll? Taust, Tekst, Taide, Aar;
    }
    private static readonly ConditionalWeakTable<Control, Rollid> rollid = new();

    // Värvib vormi (või paneeli) koos kõigi alamelementidega praeguse teema järgi
    public static void Rakenda(Control juur)
    {
        // Tag "omavarv": element värvib end ise (nt tume tootekaart) – ainult tõlgime
        if (juur.Tag as string == "omavarv") { TolgiPuu(juur); return; }
        try { RakendaUks(juur); }
        catch (Exception ex)
        {
            // üks vigane element ei tohi kogu ülejäänud akna värvimist katkestada
            if (Environment.GetEnvironmentVariable("AKT_SILUMINE") == "1")
                File.AppendAllText(Path.Combine(Path.GetTempPath(), "akt_viga.txt"), $"{juur.GetType().Name} '{juur.Name}': {ex}\n");
        }
        foreach (Control c in juur.Controls)
            Rakenda(c);
        if (juur is Form) juur.Invalidate(true);
    }

    private static void TolgiPuu(Control c)
    {
        Keel.TolgiElement(c);
        foreach (Control l in c.Controls) TolgiPuu(l);
    }

    private static readonly System.Reflection.MethodInfo? SeaStiil =
        typeof(Control).GetMethod("SetStyle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

    private static void RakendaUks(Control c)
    {
        Keel.TolgiElement(c);   // tekstid valitud keelde

        // Moodne: tavalised paneelid joonistavad tausta puhvrisse ja suuruse muutusel uuesti (muidu jäi riba joonistamata)
        if (OnMoodne && c is Panel and not RoundedPanel && c.GetType().Namespace?.StartsWith("System.Windows.Forms") == true)
            SeaStiil?.Invoke(c, new object[] { ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true });

        var r = rollid.GetValue(c, k => new Rollid
        {
            Taust = LeiaRoll(k.BackColor, false),
            Tekst = LeiaRoll(k.ForeColor, true),
            Taide = k is RoundedPanel rp1 ? LeiaRoll(rp1.FillColor, false) : null,
            Aar = k is RoundedPanel rp2 ? LeiaRoll(rp2.BorderColor, false) : null
        });

        switch (c)
        {
            case Form f:
                f.BackColor = Varv(r.Taust ?? Roll.Taust);
                f.ForeColor = Tekst;
                TumeTiitliriba(f);
                return;

            case RoundedButton:
                c.Invalidate();   // joonistab end ise teema värvidega
                return;

            case RoundedPanel p:
                if (r.Taide is Roll t) p.FillColor = Varv(t);
                if (r.Aar is Roll a) p.BorderColor = Varv(a);
                p.BackColor = p.FillColor;
                break;

            case DataGridView dgv:
                StiliseeriTabel(dgv);
                return;

            case ToolStrip ts:
                ts.BackColor = Paneel;
                ts.ForeColor = Tekst;
                foreach (ToolStripItem i in ts.Items) { Keel.TolgiMenuu(i); VarviMenuu(i); }
                return;

            case LinkLabel ll:
                ll.LinkColor = Teine;
                ll.ActiveLinkColor = Aktsent;
                ll.VisitedLinkColor = Teine;
                break;

            case ComboBox cb:
                cb.FlatStyle = FlatStyle.Flat;
                break;
        }

        if (c is not RoundedPanel && r.Taust is Roll tr) c.BackColor = Varv(tr);
        if (r.Tekst is Roll tk && tk != Roll.Valge) c.ForeColor = Varv(tk);
        c.Invalidate();
    }

    private static void VarviMenuu(ToolStripItem i)
    {
        i.ForeColor = Tekst;
        if (i is ToolStripMenuItem m)
            foreach (ToolStripItem alam in m.DropDownItems) VarviMenuu(alam);
    }

    // Ühtne tabelite välimus
    private static readonly ConditionalWeakTable<DataGridView, object> tolkeKuulajad = new();

    public static void StiliseeriTabel(DataGridView dgv)
    {
        // Veerupäised tõlgitakse ka siis, kui andmed hiljem uuesti laaditakse
        Keel.TolgiTabel(dgv);
        if (!tolkeKuulajad.TryGetValue(dgv, out _))
        {
            tolkeKuulajad.Add(dgv, new object());
            dgv.DataBindingComplete += (s, e) => Keel.TolgiTabel(dgv);
        }

        dgv.BackgroundColor = Pind;
        dgv.BorderStyle = BorderStyle.None;
        dgv.GridColor = Joon;
        dgv.EnableHeadersVisualStyles = false;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dgv.RowHeadersVisible = false;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgv.ColumnHeadersHeight = OnMoodne ? 42 : 36;
        dgv.RowTemplate.Height = OnMoodne ? 44 : 32;   // moodne: õhulisemad read

        // NB: getter annab poolikult täidetud stiili puhul KOOPIA – muudame koopiat ja omistame tagasi
        var pais = new DataGridViewCellStyle(dgv.ColumnHeadersDefaultCellStyle);
        pais.BackColor = Paneel;
        pais.ForeColor = TekstNork;
        pais.SelectionBackColor = Paneel;
        pais.SelectionForeColor = TekstNork;
        pais.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        pais.Padding = new Padding(6, 0, 0, 0);

        var rida = new DataGridViewCellStyle(dgv.DefaultCellStyle);
        rida.BackColor = Pind;
        rida.ForeColor = Tekst;
        rida.SelectionBackColor = AktsentHele;
        rida.SelectionForeColor = Tekst;
        rida.Padding = new Padding(6, 0, 0, 0);

        dgv.AlternatingRowsDefaultCellStyle.BackColor = PindAlt;
        dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = AktsentHele;
        dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Tekst;

        if (OnMoodne)
        {
            dgv.GridColor = Pind;                                   // jooni pole, ainult vaheldumisi taust
            pais.BackColor = pais.SelectionBackColor = Pind;
            rida.SelectionBackColor = Paneel2;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Ymardus.Segu(Pind, Paneel2, 0.5f);
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Paneel2;
        }
        dgv.ColumnHeadersDefaultCellStyle = pais;
        dgv.DefaultCellStyle = rida;
        foreach (DataGridViewRow r in dgv.Rows) r.Height = OnMoodne ? 44 : 32;
        dgv.Invalidate();
    }

    // ---- Tume tiitliriba (Windows 10 20H1+ / Windows 11) ----
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

    private static void TumeTiitliriba(Form f)
    {
        if (!f.IsHandleCreated)
        {
            f.HandleCreated -= Vorm_HandleCreated;
            f.HandleCreated += Vorm_HandleCreated;
            return;
        }
        try
        {
            int v = Tume ? 1 : 0;
            if (DwmSetWindowAttribute(f.Handle, 20, ref v, sizeof(int)) != 0)
                DwmSetWindowAttribute(f.Handle, 19, ref v, sizeof(int));
        }
        catch { /* vanem Windows – ei tee midagi */ }
    }

    private static void Vorm_HandleCreated(object? sender, EventArgs e)
    {
        if (sender is Form f) TumeTiitliriba(f);
    }

    // ---- Muutmine ja salvestamine ----
    // Keele vahetusel: kõik kuulajad (vormid) värvivad ja tõlgivad end uuesti
    public static void TeavitaMuutusest() => Muutus?.Invoke();

    public static void Sea(bool tume, string aktsent)
    {
        Tume = tume;
        if (Aktsendid.ContainsKey(aktsent)) AktsendiNimi = aktsent;
        Salvesta();
        Muutus?.Invoke();
    }

    // Ctrl+Shift+D: e-pood <-> klassikaline. Iga disain saab oma värvi (punane / sinine).
    public static void VahetaDisain()
    {
        Stiil = OnRimi ? Disain.Klassikaline : Disain.Rimi;
        AktsendiNimi = OnRimi ? "Punane" : "Sinine";
        Salvesta();
        Muutus?.Invoke();
    }

    // Ctrl+Shift+M: uus moodne disain <-> tavaline e-poe disain
    public static void VahetaMoodne() => SeaDisain(OnMoodne ? Disain.Rimi : Disain.Moodne);

    public static void SeaDisain(Disain d)
    {
        if (d == Stiil) return;
        Stiil = d;
        AktsendiNimi = d == Disain.Klassikaline ? "Sinine" : "Punane";
        Salvesta();
        Muutus?.Invoke();
    }

    private class Seaded
    {
        public bool Tume { get; set; }
        public string Aktsent { get; set; } = "Punane";
        public Disain Stiil { get; set; } = Disain.Rimi;
        public bool Umarad { get; set; } = true;
    }

    private static string Fail => Path.Combine(Kaustad.Juur, "teema.json");

    public static void Lae()
    {
        try
        {
            if (File.Exists(Fail))
            {
                var s = JsonSerializer.Deserialize<Seaded>(File.ReadAllText(Fail));
                if (s != null)
                {
                    Tume = s.Tume;
                    Stiil = s.Stiil;
                    Umarad = s.Umarad;
                    if (Aktsendid.ContainsKey(s.Aktsent)) AktsendiNimi = s.Aktsent;
                }
            }
        }
        catch { }

        // Menüüde ja olekuriba värvid
        ToolStripManager.Renderer = new ToolStripProfessionalRenderer(new TeemaMenuuVarvid());
    }

    private static void Salvesta()
    {
        try
        {
            Directory.CreateDirectory(Kaustad.Juur);
            File.WriteAllText(Fail, JsonSerializer.Serialize(new Seaded { Tume = Tume, Aktsent = AktsendiNimi, Stiil = Stiil, Umarad = Umarad }));
        }
        catch { }
    }
}

// Menüüde värvid teema järgi
public class TeemaMenuuVarvid : ProfessionalColorTable
{
    public TeemaMenuuVarvid() { UseSystemColors = false; }

    public override Color MenuStripGradientBegin => Teema.Paneel;
    public override Color MenuStripGradientEnd => Teema.Paneel;
    public override Color ToolStripDropDownBackground => Teema.Pind;
    public override Color ImageMarginGradientBegin => Teema.Pind;
    public override Color ImageMarginGradientMiddle => Teema.Pind;
    public override Color ImageMarginGradientEnd => Teema.Pind;
    public override Color MenuItemSelected => Teema.AktsentHele;
    public override Color MenuItemSelectedGradientBegin => Teema.AktsentHele;
    public override Color MenuItemSelectedGradientEnd => Teema.AktsentHele;
    public override Color MenuItemPressedGradientBegin => Teema.Paneel2;
    public override Color MenuItemPressedGradientMiddle => Teema.Paneel2;
    public override Color MenuItemPressedGradientEnd => Teema.Paneel2;
    public override Color MenuItemBorder => Teema.AktsentHele;
    public override Color MenuBorder => Teema.Joon;
    public override Color SeparatorDark => Teema.Joon;
    public override Color SeparatorLight => Teema.Pind;
    public override Color StatusStripGradientBegin => Teema.Paneel;
    public override Color StatusStripGradientEnd => Teema.Paneel;
    public override Color ToolStripBorder => Teema.Joon;
    public override Color CheckBackground => Teema.AktsentHele;
    public override Color CheckSelectedBackground => Teema.AktsentHele;
    public override Color CheckPressedBackground => Teema.AktsentHele;
    public override Color ButtonSelectedHighlight => Teema.AktsentHele;
    public override Color ButtonSelectedBorder => Teema.Aktsent;
}
