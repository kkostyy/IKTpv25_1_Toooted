using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Otsingu soovitused: kirjutamise ajal avaneb otsingulahtri all nimekiri
// (pilt, nimi, pakend ja hind). Nooleklahvid + Enter või hiireklõps avavad toote.
public class OtsinguSoovitused : Control
{
    private const int ReaKorgus = 58, JaluseKorgus = 40, Max = 6;

    private readonly TextBox txt;
    private readonly Func<IEnumerable<Toode>> tooted;
    private readonly Action<Toode> ava;
    private readonly Action koik;
    private readonly Action? pakkumised;
    private List<Toode> vasted = new();
    private int kokku, valitud = -1, hover = -1;
    private bool teisedOtsivad;   // tühi lahter: "Teised otsivad" (populaarsed pakkumised, nagu telefonis)

    public OtsinguSoovitused(TextBox txt, Func<IEnumerable<Toode>> tooted, Action<Toode> ava, Action koik, Action? pakkumised = null)
    {
        this.txt = txt;
        this.tooted = tooted;
        this.ava = ava;
        this.koik = koik;
        this.pakkumised = pakkumised;
        Visible = false;
        Tag = "omavarv";
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        SetStyle(ControlStyles.Selectable, false);   // klõps ei võta lahtrilt fookust

        txt.TextChanged += (s, e) => Uuenda();
        txt.KeyDown += Klahv;
        txt.Leave += (s, e) => Peida();
        txt.Enter += (s, e) => Uuenda();
        PildiVahemalu.Laetud += PiltLaetud;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) PildiVahemalu.Laetud -= PiltLaetud;
        base.Dispose(disposing);
    }

    private void PiltLaetud()
    {
        if (IsHandleCreated && Visible) BeginInvoke(Invalidate);
    }

    public void Peida()
    {
        Visible = false;
        valitud = hover = -1;
    }

    private void Uuenda()
    {
        string s = txt.Text.Trim();
        if (!txt.Focused) { Peida(); return; }
        if (s.Length == 0)
        {
            if (pakkumised == null) { Peida(); return; }
            // Teised otsivad: suurima soodustusega laos olevad tooted
            teisedOtsivad = true;
            vasted = tooted().Where(t => t.Kogus > 0 && t.SoodusProtsent != null)
                .OrderByDescending(t => t.SoodusProtsent).ThenBy(t => t.Toodenimetus).Take(5).ToList();
            if (vasted.Count == 0) { Peida(); return; }
            kokku = vasted.Count;
            valitud = -1;
            Paiguta();
            Visible = true;
            BringToFront();
            Invalidate();
            return;
        }
        teisedOtsivad = false;

        var leitud = tooted()
            .Select(t => (t, skoor: Skoor(t, s)))
            .Where(x => x.skoor > 0)
            .OrderByDescending(x => x.skoor).ThenByDescending(x => x.t.Kogus > 0).ThenBy(x => x.t.Toodenimetus)
            .Select(x => x.t)
            .ToList();
        kokku = leitud.Count;
        vasted = leitud.Take(Max).ToList();
        valitud = -1;
        Paiguta();
        Visible = true;
        BringToFront();
        Invalidate();
    }

    // Nime algus > sõna algus nimes > sisaldab nimes > kaubamärk/kategooria
    private static int Skoor(Toode t, string s)
    {
        var v = StringComparison.OrdinalIgnoreCase;   // ordinaalne on mitu korda kiirem (9000 toodet iga tähe järel)
        string n = t.Toodenimetus;
        if (n.StartsWith(s, v)) return 4;
        int i = n.IndexOf(s, v);
        if (i > 0 && !char.IsLetterOrDigit(n[i - 1])) return 3;
        if (i > 0) return 2;
        if ((t.Kaubamark ?? "").Contains(s, v) || (t.Kategooria?.Kategooria_nimetus ?? "").Contains(s, v)) return 1;
        return 0;
    }

    private void Paiguta()
    {
        if (txt.FindForm() is not Form vorm) return;
        if (Parent != vorm) { Parent?.Controls.Remove(this); vorm.Controls.Add(this); }

        // Otsingukast on lahtri vanem (ümardatud paneel), selle alla
        Control kast = txt.Parent is Control p && p.Width < txt.Width + 140 ? p : txt;
        var nurk = vorm.PointToClient(kast.PointToScreen(new Point(0, kast.Height + 6)));
        int laius = Math.Max(kast.Width, 420);
        int korgus = Math.Max(1, vasted.Count) * ReaKorgus + JaluseKorgus + 12;
        int x = Math.Max(8, Math.Min(nurk.X, vorm.ClientSize.Width - laius - 8));
        Bounds = new Rectangle(x, nurk.Y, laius, korgus);

        using var gp = Ymardus.Tee(new Rectangle(0, 0, Width, Height), 14);
        Region = new Region(gp);
    }

    private Rectangle Rida(int i) => new(6, 6 + i * ReaKorgus, Width - 12, ReaKorgus);
    private Rectangle Jalus => new(6, 6 + Math.Max(1, vasted.Count) * ReaKorgus, Width - 12, JaluseKorgus);

    private void Klahv(object? sender, KeyEventArgs e)
    {
        if (!Visible) return;
        int n = vasted.Count;
        switch (e.KeyCode)
        {
            case Keys.Down when n > 0:
                valitud = (valitud + 1) % n; e.Handled = e.SuppressKeyPress = true; Invalidate(); break;
            case Keys.Up when n > 0:
                valitud = valitud <= 0 ? n - 1 : valitud - 1; e.Handled = e.SuppressKeyPress = true; Invalidate(); break;
            case Keys.Enter:
                e.Handled = e.SuppressKeyPress = true;
                if (valitud >= 0 && valitud < n) Vali(vasted[valitud]);
                else if (teisedOtsivad) { Peida(); pakkumised?.Invoke(); }
                else { Peida(); koik(); }
                break;
            case Keys.Escape:
                e.Handled = e.SuppressKeyPress = true; Peida(); break;
        }
    }

    private void Vali(Toode t)
    {
        Peida();
        ava(t);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        int h = -1;
        for (int i = 0; i < vasted.Count; i++) if (Rida(i).Contains(e.Location)) h = i;
        if (h < 0 && Jalus.Contains(e.Location)) h = Max;
        if (h != hover) { hover = h; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(EventArgs e) { hover = -1; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        for (int i = 0; i < vasted.Count; i++)
            if (Rida(i).Contains(e.Location)) { Vali(vasted[i]); return; }
        if (Jalus.Contains(e.Location))
        {
            Peida();
            if (teisedOtsivad) pakkumised?.Invoke(); else { koik(); txt.Focus(); }
        }
        base.OnMouseClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Teema.Pind);
        using (var raam = new Pen(Teema.Joon))
        using (var gp = Ymardus.Tee(new Rectangle(0, 0, Width - 1, Height - 1), 14))
            g.DrawPath(raam, gp);

        using var fNimi = new Font("Segoe UI Semibold", 10F);
        using var fVaike = new Font("Segoe UI", 8.5F);
        using var fHind = new Font("Segoe UI", 11F, FontStyle.Bold);
        var vasak = TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
        var parem = TextFormatFlags.NoPrefix | TextFormatFlags.Right | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

        if (vasted.Count == 0)
            TextRenderer.DrawText(g, Keel.F("„{0}“ – vasteid pole", txt.Text.Trim()), fNimi, Rida(0), Teema.TekstNork,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix);

        for (int i = 0; i < vasted.Count; i++)
        {
            var t = vasted[i];
            var r = Rida(i);
            if (i == valitud || i == hover)
            {
                using var b = new SolidBrush(i == valitud ? Teema.AktsentHele : Teema.Paneel2);
                using var gp = Ymardus.Tee(r, 10);
                g.FillPath(b, gp);
            }

            // Pilt
            var pk = new Rectangle(r.X + 8, r.Y + 7, 44, 44);
            using (var b = new SolidBrush(Teema.PildiTaust))
            using (var gp = Ymardus.Tee(pk, 8))
                g.FillPath(b, gp);
            if (PildiVahemalu.Pisi(t.Pilt) is Image pilt)
                PildiVahemalu.Joonista(g, pilt, RectangleF.Inflate(pk, -2, -2));
            else Ikoonid.JoonistaEmoji(g, Ikoonid.KategooriaIkoon(t.Kategooria?.Kategooria_nimetus), pk, Teema.TekstNork, 15f);

            // Hind paremal
            string hind = Ikoonid.Hind(t.Hind) + (teisedOtsivad && t.SoodusProtsent is int sp ? $"  −{sp}%" : "");
            var hs = TextRenderer.MeasureText(g, hind, fHind, Size.Empty, parem);
            int hindX = r.Right - 12 - Math.Max(hs.Width, 70);
            TextRenderer.DrawText(g, hind, fHind, new Rectangle(hindX, r.Y + 10, r.Right - 12 - hindX, 22),
                t.SoodusProtsent != null ? Teema.Aktsent : Teema.Tekst, parem);
            string all = t.UhikuHind is decimal uh && !string.IsNullOrEmpty(t.Uhik) ? $"{Ikoonid.Hind(uh)}/{t.Uhik}"
                       : t.VanaHind is decimal vh ? Ikoonid.Hind(vh) : "";
            if (all.Length > 0)
                TextRenderer.DrawText(g, all, fVaike, new Rectangle(hindX - 20, r.Y + 33, r.Right - 12 - hindX + 20, 16), Teema.TekstNork, parem);

            // Nimi + alarida
            int tx = pk.Right + 12;
            var nimiR = new Rectangle(tx, r.Y + 11, hindX - tx - 10, 20);
            JoonistaEsiletostetud(g, t.Toodenimetus, teisedOtsivad ? "" : txt.Text.Trim(), fNimi, nimiR);
            string info = string.Join(" · ", new[] { t.Kaubamark, t.Pakend }.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (t.Kogus <= 0) info = Keel.T("Pole laos") + (info.Length > 0 ? " · " + info : "");
            TextRenderer.DrawText(g, info, fVaike, new Rectangle(tx, r.Y + 33, hindX - tx - 10, 16),
                t.Kogus <= 0 ? Teema.Viga : Teema.TekstNork, vasak);
        }

        // Jalus: kõik tulemused
        var j = Jalus;
        using (var p = new Pen(Teema.Joon)) g.DrawLine(p, j.X + 8, j.Y + 2, j.Right - 8, j.Y + 2);
        var jv = hover == Max ? Teema.Aktsent : Teema.Teine;
        string jt = teisedOtsivad ? Keel.T("Teised otsivad") + " · " + Keel.T("Vaata kõiki pakkumisi") : Keel.F("Näita kõiki tulemusi ({0})", kokku);
        var js = TextRenderer.MeasureText(g, jt, fNimi, Size.Empty, TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        int jx = j.X + (j.Width - js.Width - 22) / 2;
        TextRenderer.DrawText(g, jt, fNimi, new Rectangle(jx, j.Y + 4, js.Width + 2, j.Height - 4), jv,
            TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        Ikoonid.Joonista(g, Ikoonid.NoolParemale, new Rectangle(jx + js.Width + 4, j.Y + 4, 18, j.Height - 4), jv, 9f);
    }

    // Nimi, kus otsitav osa on paksus kirjas
    private static void JoonistaEsiletostetud(Graphics g, string nimi, string otsi, Font f, Rectangle r)
    {
        var fl = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
        int i = otsi.Length > 0 ? nimi.IndexOf(otsi, StringComparison.CurrentCultureIgnoreCase) : -1;
        var nimiMoot = TextRenderer.MeasureText(g, nimi, f, Size.Empty, fl);
        if (i < 0 || nimiMoot.Width > r.Width)
        {
            TextRenderer.DrawText(g, nimi, f, r, Teema.Tekst, fl | TextFormatFlags.EndEllipsis);
            return;
        }
        using var paks = new Font(f.FontFamily, f.Size, FontStyle.Bold);
        int x = r.X;
        foreach (var (osa, font) in new[] { (nimi[..i], f), (nimi.Substring(i, otsi.Length), paks), (nimi[(i + otsi.Length)..], f) })
        {
            if (osa.Length == 0) continue;
            var s = TextRenderer.MeasureText(g, osa, font, Size.Empty, fl);
            TextRenderer.DrawText(g, osa, font, new Rectangle(x, r.Y, Math.Max(0, r.Right - x), r.Height), font == paks ? Teema.Aktsent : Teema.Tekst, fl);
            x += s.Width;
        }
    }
}
