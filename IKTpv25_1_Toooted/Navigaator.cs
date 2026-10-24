using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Vorm või leht, kust saab ühe sammu tagasi minna (nupp, Esc, Alt+←, hiire külgnupp)
public interface ITagasi
{
    bool MineTagasi();
}

// Üks leht poe sisualal (avaleht, kategooria, toiteväärtus, preemiad ...)
public abstract class Leht : UserControl
{
    protected Leht()
    {
        BackColor = Teema.Taust;
        DoubleBuffered = true;
    }

    public virtual string Pealkiri => "";

    // Leht tuli nähtavale (esimest korda või tagasi tulles)
    public virtual void Avatud() { }

    // Leht eemaldati pinust
    public virtual void Suletud() { }
}

// Lehtede pinu: Ava viib edasi, Tagasi ühe sammu tagasi. Üleminek libiseb (450 ms).
public class Navigaator
{
    private readonly Control ala;
    private readonly List<Leht> pinu = new();
    private System.Windows.Forms.Timer? libisemine;

    public event Action? Muutus;

    public Navigaator(Control ala)
    {
        this.ala = ala;
        ala.Resize += (s, e) => { if (Praegune is Leht l && l.Dock == DockStyle.None) l.Bounds = ala.ClientRectangle; };
    }

    public Leht? Praegune => pinu.Count > 0 ? pinu[^1] : null;
    public IReadOnlyList<Leht> Lehed => pinu;
    public bool SaabTagasi => pinu.Count > 1;

    public void Ava(Leht leht)
    {
        var vana = Praegune;
        pinu.Add(leht);
        Naita(leht, vana, 1);
    }

    // Asendab praeguse lehe (nt otsingu tulemused muutuvad iga tähega)
    public void Asenda(Leht leht)
    {
        if (pinu.Count == 0) { Ava(leht); return; }
        var vana = pinu[^1];
        pinu[^1] = leht;
        Naita(leht, vana, 0);
        Eemalda(vana);
    }

    public bool Tagasi()
    {
        if (pinu.Count <= 1) return false;
        Mine(pinu.Count - 2);
        return true;
    }

    public void Avalehele()
    {
        if (pinu.Count == 0) return;
        Mine(0);
    }

    // "Leivapuru" klõps: tagasi selle tasemeni
    public void Mine(int indeks)
    {
        if (indeks < 0 || indeks >= pinu.Count) return;
        if (indeks == pinu.Count - 1) { Muutus?.Invoke(); Praegune!.Avatud(); return; }
        var vana = pinu[^1];
        var eemaldatud = pinu.Skip(indeks + 1).ToList();
        pinu.RemoveRange(indeks + 1, pinu.Count - indeks - 1);
        Naita(pinu[^1], vana, -1);
        foreach (var l in eemaldatud) Eemalda(l);
    }

    private void Eemalda(Leht l)
    {
        l.Suletud();
        ala.Controls.Remove(l);
        l.Dispose();
    }

    // suund: 1 = edasi (tuleb paremalt), -1 = tagasi (tuleb vasakult), 0 = ilma animatsioonita
    private void Naita(Leht uus, Leht? vana, int suund)
    {
        libisemine?.Stop();
        if (!ala.Controls.Contains(uus)) ala.Controls.Add(uus);

        uus.Dock = DockStyle.None;
        uus.Bounds = ala.ClientRectangle;
        uus.Visible = true;
        uus.BringToFront();
        if (vana != null && vana != uus) vana.Visible = false;
        Muutus?.Invoke();
        uus.Avatud();

        // Moodne: kohe, ilma libisemiseta (paljude elementidega leht "rebenes" liikudes)
        if (suund == 0 || Teema.OnMoodne)
        {
            uus.Dock = DockStyle.Fill;
            return;
        }
        int nihe = 70 * suund;
        libisemine = Animatsioon.Kaivita(uus, 450,
            t => uus.Left = (int)Math.Round(nihe * (1 - Animatsioon.Sujuv(t))),
            () => { uus.Left = 0; uus.Dock = DockStyle.Fill; });
    }
}

// Tume "pill" nupp:  ← Tagasi  (heledas teemas tume, tumedas hele)
public class TagasiNupp : Control
{
    private bool hover, all;
    private float nool;   // noole nihe hiire all (0 → 1)
    private System.Windows.Forms.Timer? anim;

    public TagasiNupp()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        Height = 48;
        AccessibleName = Keel.T("Tagasi");
        AccessibleRole = AccessibleRole.PushButton;
        Keel.Muutus += Sobita;
        Sobita();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) Keel.Muutus -= Sobita;
        base.Dispose(disposing);
    }

    private void Sobita()
    {
        Width = TextRenderer.MeasureText(Keel.T("Tagasi"), Font).Width + 58;
        AccessibleName = Keel.T("Tagasi");
        Invalidate();
    }

    private void Liiguta(bool sisse)
    {
        anim?.Stop();
        float algus = nool;
        anim = Animatsioon.Kaivita(this, 250, t => { nool = (float)(algus + ((sisse ? 1 : 0) - algus) * Animatsioon.Sujuv(t)); Invalidate(); });
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Liiguta(true); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = all = false; Liiguta(false); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { all = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { all = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Cursor = Enabled ? Cursors.Hand : Cursors.Default; Invalidate(); base.OnEnabledChanged(e); }

    public void Vajuta() { if (Enabled && Visible) OnClick(EventArgs.Empty); }

    protected override bool IsInputKey(Keys keyData) => keyData == Keys.Space || base.IsInputKey(keyData);
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Enter or Keys.Space) OnClick(EventArgs.Empty);
        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? BackColor);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Color taust = !Enabled ? Teema.Paneel2 : Teema.Tekst;
        Color esi = !Enabled ? Teema.TekstNork : Teema.Pind;
        if (Enabled && hover) taust = Ymardus.Segu(taust, Teema.Pind, 0.12f);

        float k = all ? 0.95f : 1f;   // vajutus: väikseks
        var r = new RectangleF(1, 1, Width - 3, Height - 3);
        r = RectangleF.Inflate(r, -(r.Width * (1 - k)) / 2, -(r.Height * (1 - k)) / 2);
        using (var tee = Ymardus.Tee(r, r.Height / 2))
        using (var b = new SolidBrush(taust))
            g.FillPath(b, tee);

        int x = (int)(r.X + 14 - nool * 4);
        Ikoonid.Joonista(g, Ikoonid.NoolVasakule, new Rectangle(x, (int)r.Y, 20, (int)r.Height), esi, 11f);
        TextRenderer.DrawText(g, Keel.T("Tagasi"), Font, new Rectangle((int)r.X + 38, (int)r.Y, (int)r.Width - 38, (int)r.Height), esi,
            TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        if (Focused && ShowFocusCues)
        {
            using var p = new Pen(Teema.Aktsent, 2);
            using var tee = Ymardus.Tee(RectangleF.Inflate(r, -2, -2), r.Height / 2 - 2);
            g.DrawPath(p, tee);
        }
    }
}

// Hiire külgnupp (XButton1 = "tagasi") igas aknas: aktiivne vorm läheb sammu tagasi
internal sealed class TagasiHiir : IMessageFilter
{
    private const int WM_XBUTTONUP = 0x020C;
    private const int XBUTTON1 = 1;

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg != WM_XBUTTONUP || ((long)m.WParam >> 16 & 0xFFFF) != XBUTTON1) return false;
        var vorm = Form.ActiveForm;
        if (vorm is ITagasi t) { t.MineTagasi(); return true; }
        if (vorm != null && UiStiil.AknaTagasiNupp(vorm) is TagasiNupp n) { n.Vajuta(); return true; }
        return false;
    }
}
