using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Pangaülekanne: raha tuleb otse poe pangakontole.
//  Ostja näeb pärast tellimist saajat, IBAN-i, summat ja selgitust + EPC QR-koodi
//  (pangaäpp täidab makse ise). Tellimus on "Maksmata", kuni omanik märgib selle
//  Ostude ajaloos makstuks (raha laekus kontole).
// ===================================================================
public class MakseSeaded
{
    public const string Pangaulekanne = "Pangaülekanne";

    // Vaikimisi saaja (muudetav: Haldus → Makse seaded, fail AppData\Local\ToodedApp\makse.json)
    public string Saaja { get; set; } = "KONSTANTIN GAPONENKO";
    public string Iban { get; set; } = "EE502200221075082971";

    private static string Fail => Path.Combine(Kaustad.Juur, "makse.json");

    public static MakseSeaded Lae()
    {
        try
        {
            if (File.Exists(Fail)) return JsonSerializer.Deserialize<MakseSeaded>(File.ReadAllText(Fail)) ?? new();
        }
        catch { }
        return new();
    }

    public void Salvesta()
    {
        Directory.CreateDirectory(Kaustad.Juur);
        File.WriteAllText(Fail, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    public string IbanLoetav => string.Join(" ", Enumerable.Range(0, (Iban.Length + 3) / 4).Select(i => Iban.Substring(i * 4, Math.Min(4, Iban.Length - i * 4))));

    public static string Selgitus(Ost ost) => $"{Arve.PoeNimi} tellimus {ost.Id}";

    // IBAN-i kontrollsumma (mod 97)
    public static bool IbanKorrektne(string? iban)
    {
        string s = (iban ?? "").Replace(" ", "").ToUpperInvariant();
        if (s.Length < 15 || s.Length > 34 || !s.All(char.IsLetterOrDigit)) return false;
        string r = s[4..] + s[..4];
        int jaak = 0;
        foreach (char c in r)
        {
            string osa = char.IsDigit(c) ? c.ToString() : (c - 'A' + 10).ToString();
            foreach (char d in osa) jaak = (jaak * 10 + (d - '0')) % 97;
        }
        return jaak == 1;
    }

    // EPC QR (SEPA ülekanne): pangaäpp täidab saaja, IBAN-i, summa ja selgituse
    public string EpcTekst(decimal summa, string selgitus) =>
        string.Join("\n", "BCD", "002", "1", "SCT", "", Saaja, Iban.Replace(" ", ""),
            "EUR" + summa.ToString("0.00", CultureInfo.InvariantCulture), "", "", selgitus);

    public Image? QrPilt(decimal summa, string selgitus)
    {
        try
        {
            using var gen = new QRCoder.QRCodeGenerator();
            using var andmed = gen.CreateQrCode(EpcTekst(summa, selgitus), QRCoder.QRCodeGenerator.ECCLevel.M);
            byte[] png = new QRCoder.PngByteQRCode(andmed).GetGraphic(8);
            using var ms = new MemoryStream(png);
            using var ajutine = Image.FromStream(ms);
            return new Bitmap(ajutine);
        }
        catch { return null; }
    }

    // Omanik: raha laekus → ost makstuks
    public static void MargiMakstuks(int ostId)
    {
        using var db = new ToodedContext();
        var ost = db.Ostud.FirstOrDefault(o => o.Id == ostId);
        if (ost == null || ost.Makstud) return;
        ost.Makstud = true;
        ost.MakstudSumma = ost.Summa;
        db.SaveChanges();
    }
}

// Pärast tellimist: rekvisiidid + QR-kood, iga väärtuse kõrval "Kopeeri"
public class PangaMakseVorm : Form
{
    private readonly Ost ost;
    private readonly string? pdf;
    private readonly MakseSeaded s = MakseSeaded.Lae();
    private readonly Image? qr;
    private readonly List<(string Silt, string Vaartus, Rectangle Nupp)> read = new();
    private int hiir = -1;   // 0..n-1 = kopeeri, 100 = arve, 101 = valmis
    private string? kopeeritud;

    private static readonly Color Taust = Color.FromArgb(27, 28, 31), Taust2 = Color.FromArgb(44, 46, 51),
        Nork = Color.FromArgb(190, 194, 200), Kollane = Color.FromArgb(245, 215, 110);

    public static void Naita(IWin32Window? omanik, Ost ost, string? pdf)
    {
        using var v = new PangaMakseVorm(ost, pdf);
        if (omanik != null) v.ShowDialog(omanik);
        else { v.StartPosition = FormStartPosition.CenterScreen; v.ShowDialog(); }
    }

    private PangaMakseVorm(Ost ost, string? pdf)
    {
        this.ost = ost;
        this.pdf = pdf;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        KeyPreview = true;
        ClientSize = new Size(500, 790);
        BackColor = Taust;
        Text = Keel.T("Pangaülekanne");
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        using (var tee = Ymardus.Tee(new Rectangle(0, 0, Width, Height), 28)) Region = new Region(tee);
        qr = s.QrPilt(ost.Summa, MakseSeaded.Selgitus(ost));

        int y = 392;
        foreach (var (silt, v) in new[]
        {
            (Keel.T("Saaja"), s.Saaja),
            ("IBAN", s.IbanLoetav),
            (Keel.T("Summa"), ost.Summa.ToString("0.00", CultureInfo.InvariantCulture) + " EUR"),
            (Keel.T("Selgitus"), MakseSeaded.Selgitus(ost)),
        })
        {
            read.Add((silt, v, new Rectangle(Width - 128, y + 8, 100, 32)));
            y += 52;
        }
        FormClosed += (o, e) => qr?.Dispose();
    }

    private Rectangle PankNupp => new(28, Height - 136, Width - 56, 50);
    private Rectangle ArveNupp => new(28, Height - 76, (Width - 66) / 2, 50);
    private Rectangle ValmisNupp => new(38 + (Width - 66) / 2, Height - 76, (Width - 66) / 2, 50);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Enter or Keys.Escape) { Close(); e.Handled = true; }
        base.OnKeyDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int uus = -1;
        for (int i = 0; i < read.Count; i++) if (read[i].Nupp.Contains(e.Location)) uus = i;
        if (pdf != null && ArveNupp.Contains(e.Location)) uus = 100;
        if (PankNupp.Contains(e.Location)) uus = 102;
        if (ValmisNupp.Contains(e.Location)) uus = 101;
        if (uus != hiir) { hiir = uus; Cursor = uus >= 0 ? Cursors.Hand : Cursors.Default; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        for (int i = 0; i < read.Count; i++)
            if (read[i].Nupp.Contains(e.Location))
            {
                // IBAN ja summa ilma tühikute / "EUR"-ita, et pangas saaks otse kleepida
                string tekst = read[i].Silt == "IBAN" ? s.Iban.Replace(" ", "")
                             : i == 2 ? ost.Summa.ToString("0.00", CultureInfo.InvariantCulture) : read[i].Vaartus;
                try { Clipboard.SetText(tekst); kopeeritud = read[i].Silt; Invalidate(); } catch { }
                return;
            }
        if (PankNupp.Contains(e.Location))
        {
            // IBAN lõikelauale ja Swedbanki internetipank brauseris
            try { Clipboard.SetText(s.Iban.Replace(" ", "")); kopeeritud = "IBAN"; Invalidate(); } catch { }
            try { Arve.Ava("https://www.swedbank.ee/private"); } catch (Exception ex) { Teade.Show("Brauseri avamine ebaõnnestus:\n" + ex.Message); }
            Teade.Toast(Keel.F("IBAN kopeeritud – kleebi pangaäpis. Summa {0}, selgitus „{1}“", Ostukorv.Eur(ost.Summa), MakseSeaded.Selgitus(ost)), TeateTyyp.Info, 6000);
            return;
        }
        if (pdf != null && ArveNupp.Contains(e.Location))
        {
            try { Arve.Ava(pdf); } catch (Exception ex) { Teade.Show("Arve avamine ebaõnnestus:\n" + ex.Message); }
        }
        else if (ValmisNupp.Contains(e.Location)) Close();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Taust);
        int w = Width;
        var kesk = TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;

        using (var f = new Font("Segoe UI", 18F, FontStyle.Bold))
            TextRenderer.DrawText(g, Keel.T("Tellimus on vormistatud"), f, new Rectangle(0, 26, w, 36), Color.White, kesk);
        // kollane kiip: ootab makset
        using (var fk = new Font("Segoe UI Semibold", 9.5F))
        {
            string t = Keel.F("Ootab makset · tellimus nr {0}", ost.Id);
            int kw = TextRenderer.MeasureText(t, fk).Width + 28;
            var r = new Rectangle((w - kw) / 2, 68, kw, 28);
            using (var tee = Ymardus.Tee(r, 14)) using (var b = new SolidBrush(Color.FromArgb(77, 60, 20))) g.FillPath(b, tee);
            TextRenderer.DrawText(g, t, fk, r, Kollane, kesk | TextFormatFlags.VerticalCenter);
        }

        // QR-kood valgel ruudul
        var qrKast = new Rectangle((w - 228) / 2, 110, 228, 228);
        using (var tee = Ymardus.Tee(qrKast, 18)) using (var b = new SolidBrush(Color.White)) g.FillPath(b, tee);
        if (qr != null)
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;   // terav QR
            g.DrawImage(qr, Rectangle.Inflate(qrKast, -12, -12));
        }
        using (var f = new Font("Segoe UI", 9F))
            TextRenderer.DrawText(g, Keel.T("Skaneeri pangaäpis või maksa internetipangas:"), f, new Rectangle(16, 348, w - 32, 22), Nork, kesk);

        // read: silt, väärtus, "Kopeeri"
        using var fs = new Font("Segoe UI", 8.5F);
        using var fv = new Font("Segoe UI Semibold", 11F);
        using var fn = new Font("Segoe UI Semibold", 9F);
        for (int i = 0; i < read.Count; i++)
        {
            var (silt, v, nupp) = read[i];
            int y = nupp.Y - 8;
            var rida = new Rectangle(24, y, w - 48, 48);
            using (var tee = Ymardus.Tee(rida, 14)) using (var b = new SolidBrush(Taust2)) g.FillPath(b, tee);
            TextRenderer.DrawText(g, silt, fs, new Point(40, y + 4), Nork);
            TextRenderer.DrawText(g, v, fv, new Rectangle(40, y + 20, nupp.X - 48, 24), Color.White, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            bool ok = kopeeritud == silt;
            using (var tee = Ymardus.Tee(nupp, 16))
            using (var b = new SolidBrush(ok ? Color.FromArgb(34, 160, 90) : hiir == i ? Color.FromArgb(70, 72, 78) : Color.FromArgb(58, 60, 66)))
                g.FillPath(b, tee);
            TextRenderer.DrawText(g, Keel.T(ok ? "Kopeeritud" : "Kopeeri"), fn, nupp, Color.White, kesk | TextFormatFlags.VerticalCenter);
        }

        using (var f = new Font("Segoe UI", 8.5F))
            TextRenderer.DrawText(g, Keel.T("Kaup saadetakse / antakse kätte pärast makse laekumist."), f,
                new Rectangle(16, PankNupp.Y - 30, w - 32, 20), Nork, kesk | TextFormatFlags.EndEllipsis);

        using var fb = new Font("Segoe UI Semibold", 11F);
        var pn = PankNupp;
        using (var tee = Ymardus.Tee(pn, 25)) using (var b = new SolidBrush(hiir == 102 ? Color.FromArgb(230, 110, 0) : Color.FromArgb(255, 121, 0))) g.FillPath(b, tee);
        TextRenderer.DrawText(g, Keel.T("Ava Swedbanki internetipank"), fb, pn, Color.White, kesk | TextFormatFlags.VerticalCenter);
        if (pdf != null)
        {
            var a = ArveNupp;
            using (var tee = Ymardus.Tee(a, 25))
            {
                if (hiir == 100) using (var b = new SolidBrush(Taust2)) g.FillPath(b, tee);
                using var p = new Pen(Color.FromArgb(90, 255, 255, 255), 1.5f);
                g.DrawPath(p, tee);
            }
            TextRenderer.DrawText(g, Keel.T("Vaata arvet"), fb, a, Color.White, kesk | TextFormatFlags.VerticalCenter);
        }
        var vn = pdf != null ? ValmisNupp : new Rectangle(28, Height - 76, Width - 56, 50);
        using (var tee = Ymardus.Tee(vn, 25)) using (var b = new SolidBrush(hiir == 101 ? Color.FromArgb(225, 228, 232) : Color.White)) g.FillPath(b, tee);
        TextRenderer.DrawText(g, Keel.T("Valmis"), fb, vn, Taust, kesk | TextFormatFlags.VerticalCenter);
    }
}

// Haldus → Makse seaded: saaja nimi ja IBAN
public class MakseSeadedVorm : KujundusVorm
{
    private readonly TextBox txtSaaja, txtIban;

    public MakseSeadedVorm() : base("Makse seaded", 420, 300)
    {
        var s = MakseSeaded.Lae();
        Pealkiri("Makse seaded");
        var juhis = Silt("Pangaülekanne: ostja maksab sellele kontole. Tellimus on „Maksmata“, kuni märgid selle Ostude ajaloos makstuks.", 20, 56);
        juhis.MaximumSize = new Size(380, 0);
        Silt("Saaja nimi", 20, 108);
        txtSaaja = Sisend(20, 128, 380);
        txtSaaja.Text = s.Saaja;
        Silt("IBAN", 20, 164);
        txtIban = Sisend(20, 184, 380);
        txtIban.Text = s.IbanLoetav;
        var salvesta = Nupp("Salvesta", 20, 236, true);
        salvesta.Click += (o, e) =>
        {
            string iban = txtIban.Text.Replace(" ", "").ToUpperInvariant();
            if (txtSaaja.Text.Trim().Length < 2) { Teade.Show("Sisesta saaja nimi."); return; }
            if (!MakseSeaded.IbanKorrektne(iban)) { Teade.Show("IBAN ei ole korrektne (kontrolli numbreid)."); return; }
            s.Saaja = txtSaaja.Text.Trim();
            s.Iban = iban;
            try { s.Salvesta(); Teade.Toast("Makse seaded salvestatud"); Close(); }
            catch (Exception ex) { Teade.Show("Salvestamine ebaõnnestus:\n" + ex.Message); }
        };
        var sulge = Nupp("Sulge", 330, 236);
        sulge.DialogResult = DialogResult.Cancel;
        CancelButton = sulge;
    }
}
