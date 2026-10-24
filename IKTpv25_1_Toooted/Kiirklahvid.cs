using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Kiirklahvid kõikjal: aknad (peaaken, pood) annavad oma klahvid,
//  üks sõnumifilter käivitab need. F1 näitab kõiki klahve.
// ===================================================================
public record Kiirklahv(Keys Klahv, string Ruhm, string Kirjeldus, Action Tegevus, Func<bool>? Lubatud = null);

public interface IKiirklahvid
{
    IEnumerable<Kiirklahv> Kiirklahvid();
}

public sealed class KiirklahvideFilter : IMessageFilter
{
    private const int WM_KEYDOWN = 0x0100, WM_SYSKEYDOWN = 0x0104;

    public static void Lisa() => Application.AddMessageFilter(new KiirklahvideFilter());

    // Üldised klahvid (töötavad igal pool; abis näidatakse esimesena)
    public static readonly (string Klahv, string Kirjeldus)[] Yldised =
    {
        ("F1", "Kõik kiirklahvid (see aken)"),
        ("Esc / Alt+←", "Tagasi eelmisele lehele"),
        ("Ctrl+Shift+M", "Vaheta disaini (moodne / klassikaline)"),
        ("Enter", "Kinnita / vali (dialoogides)"),
    };

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg != WM_KEYDOWN && m.Msg != WM_SYSKEYDOWN) return false;
        if (Form.ActiveForm is not Form vorm || vorm is KiirklahvideAken) return false;   // abiaken sulgeb end ise
        var klahv = (Keys)(int)m.WParam | Control.ModifierKeys;

        // ainult F-klahvid ja Ctrl-kombinatsioonid (tavaline tekst lahtrites jääb puutumata)
        var pohi = klahv & Keys.KeyCode;
        bool fKlahv = pohi is >= Keys.F1 and <= Keys.F12;
        if (!fKlahv && (klahv & Keys.Control) == 0) return false;
        if ((klahv & Keys.Control) != 0 && fKlahv) return false;   // Ctrl+F1…F12 on silumiseks

        var hostid = Hostid(vorm);
        if (klahv == Keys.F1)
        {
            // sama klahv mitmes aknas: kehtib esimene (nt poe Ctrl+F peaakna oma asemel)
            var koik = hostid.SelectMany(h => h.Kiirklahvid()).Where(k => k.Lubatud?.Invoke() ?? true)
                .GroupBy(k => k.Klahv).Select(g => g.First()).ToList();
            using var aken = new KiirklahvideAken(koik);
            aken.ShowDialog(vorm);
            return true;
        }
        foreach (var h in hostid)
            foreach (var k in h.Kiirklahvid())
                if (k.Klahv == klahv && (k.Lubatud?.Invoke() ?? true))
                {
                    k.Tegevus();
                    return true;
                }
        return false;
    }

    // Fookusega elemendist ülespoole: sisse manustatud pood enne peaakent
    private static List<IKiirklahvid> Hostid(Form vorm)
    {
        Control? c = vorm;
        while (c is ContainerControl cc && cc.ActiveControl != null && cc.ActiveControl != c) c = cc.ActiveControl;
        var tulem = new List<IKiirklahvid>();
        for (var p = c; p != null; p = p.Parent)
            if (p is IKiirklahvid k && !tulem.Contains(k)) tulem.Add(k);
        if (vorm is IKiirklahvid v && !tulem.Contains(v)) tulem.Add(v);
        return tulem;
    }

    public static string Nimi(Keys k)
    {
        var osad = new List<string>();
        if ((k & Keys.Control) != 0) osad.Add("Ctrl");
        if ((k & Keys.Shift) != 0) osad.Add("Shift");
        if ((k & Keys.Alt) != 0) osad.Add("Alt");
        var p = k & Keys.KeyCode;
        osad.Add(p switch
        {
            Keys.Return => "Enter",
            Keys.Oemcomma => ",",
            Keys.Left => "←",
            Keys.Right => "→",
            Keys.Delete => "Del",
            >= Keys.D0 and <= Keys.D9 => ((int)p - (int)Keys.D0).ToString(),
            _ => p.ToString()
        });
        return string.Join("+", osad);
    }
}

// Tume abiaken: rühmad ja klahvid "klahvikübaratena"
public class KiirklahvideAken : Form
{
    private readonly List<(string Ruhm, List<(string Klahv, string Tekst)> Read)> ruhmad = new();
    private const int Rida = 30, Pais = 34, Veerg = 380;

    public KiirklahvideAken(List<Kiirklahv> klahvid)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        KeyPreview = true;
        BackColor = Color.FromArgb(27, 28, 31);
        DoubleBuffered = true;
        Text = Keel.T("Kiirklahvid");

        ruhmad.Add((Keel.T("Üldine"), KiirklahvideFilter.Yldised.Select(y => (y.Klahv, Keel.T(y.Kirjeldus))).ToList()));
        foreach (var g in klahvid.GroupBy(k => k.Ruhm))
            ruhmad.Add((Keel.T(g.Key), g.Select(k => (KiirklahvideFilter.Nimi(k.Klahv), Keel.T(k.Kirjeldus))).ToList()));

        // kaks veergu: rühmad jagatakse nii, et veerud oleksid enam-vähem võrdsed
        int kokku = ruhmad.Sum(r => Pais + r.Read.Count * Rida + 16);
        int vasak = 0;
        foreach (var r in ruhmad) { int h = Pais + r.Read.Count * Rida + 16; if (vasak + h / 2 > kokku / 2) break; vasak += h; }
        int parem = kokku - vasak;
        ClientSize = new Size(Veerg * 2 + 72, Math.Max(vasak, parem) + 110);
        Region = new Region(Ymardus.Tee(new Rectangle(0, 0, Width, Height), 22));

        KeyDown += (s, e) => { if (e.KeyCode is Keys.Escape or Keys.F1 or Keys.Enter) Close(); };
        Click += (s, e) => Close();
        Deactivate += (s, e) => Close();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var fp = new Font("Segoe UI", 17F, FontStyle.Bold);
        using var fs = new Font("Segoe UI", 9F);
        using var fr = new Font("Segoe UI", 9F, FontStyle.Bold);
        using var fk = new Font("Segoe UI Semibold", 8.5F);
        using var ft = new Font("Segoe UI", 10F);
        TextRenderer.DrawText(g, Keel.T("Kiirklahvid"), fp, new Point(28, 20), Color.White);
        TextRenderer.DrawText(g, Keel.T("Sulge: Esc, F1 või klõps"), fs, new Rectangle(0, 30, Width - 28, 20), Color.FromArgb(150, 155, 162), TextFormatFlags.Right);

        int kokku = ruhmad.Sum(r => Pais + r.Read.Count * Rida + 16);
        int x = 28, y = 72, vasak = 0;
        bool teineVeerg = false;
        foreach (var (ruhm, read) in ruhmad)
        {
            int h = Pais + read.Count * Rida + 16;
            if (!teineVeerg && vasak > 0 && vasak + h / 2 > kokku / 2) { teineVeerg = true; x = 28 + Veerg + 16; y = 72; }
            vasak += h;
            TextRenderer.DrawText(g, ruhm.ToUpper(), fr, new Point(x, y + 6), Color.FromArgb(235, 120, 175));
            y += Pais;
            foreach (var (klahv, tekst) in read)
            {
                // klahvikübarad: "Ctrl" "+" "F"
                int kx = x;
                var variandid = klahv.Split(" / ");
                for (int vi = 0; vi < variandid.Length; vi++)
                {
                    if (vi > 0)
                    {
                        TextRenderer.DrawText(g, "/", fk, new Point(kx, y + 6), Color.FromArgb(150, 155, 162));
                        kx += 12;
                    }
                foreach (var osa in variandid[vi].Split('+'))
                {
                    var s = TextRenderer.MeasureText(g, osa, fk, Size.Empty, TextFormatFlags.NoPadding);
                    var r = new Rectangle(kx, y + 3, Math.Max(24, s.Width + 14), 22);
                    using (var tee = Ymardus.Tee(r, 6)) using (var b = new SolidBrush(Color.FromArgb(50, 52, 57))) g.FillPath(b, tee);
                    using (var p = new Pen(Color.FromArgb(70, 72, 78))) g.DrawLine(p, r.X + 4, r.Bottom, r.Right - 4, r.Bottom);
                    TextRenderer.DrawText(g, osa, fk, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                    kx = r.Right + 4;
                }
                }
                TextRenderer.DrawText(g, tekst, ft, new Rectangle(x + 150, y, Veerg - 150, Rida), Color.FromArgb(213, 216, 221),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                y += Rida;
            }
            y += 16;
        }
    }
}
