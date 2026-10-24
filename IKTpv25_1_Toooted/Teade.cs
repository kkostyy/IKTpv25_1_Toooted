using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

public enum TeateTyyp { Info, Edu, Hoiatus, Viga, Kysimus }

// Programmi stiilis teated:
//  Teade.Show(...)  – sama kasutus nagu MessageBox.Show, aga teema värvides
//  Teade.Toast(...) – väike teade paremal all, kaob ise (nt "Lisatud korvi")
public static class Teade
{
    // Küsib teksti (nt salvestatava ostukorvi nimi). Tagastab null, kui vajutati "Tühista".
    public static string? Kysi(string pealkiri, string kysimus, string algtekst = "")
    {
        using var dlg = new Form
        {
            Text = Keel.T(pealkiri),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            Font = UiStiil.Põhifont,
            BackColor = Teema.Pind,
            ClientSize = new Size(400, 150)
        };
        var silt = new Label { Text = Keel.TolgiTekst(kysimus), Location = new Point(18, 16), AutoSize = true, ForeColor = Teema.Tekst2 };
        var raam = UiStiil.LooSisend(out TextBox t, 364, false);
        raam.Location = new Point(18, 42);
        t.Text = algtekst;
        var ok = UiStiil.LooNupp("OK", null, true);
        ok.DialogResult = DialogResult.OK;
        ok.Location = new Point(220, 98);
        ok.MinimumSize = new Size(76, 34);
        var tyh = UiStiil.LooNupp("Tühista", null);
        tyh.DialogResult = DialogResult.Cancel;
        tyh.Location = new Point(302, 98);
        tyh.MinimumSize = new Size(80, 34);
        dlg.Controls.AddRange(new Control[] { silt, raam, ok, tyh });
        dlg.AcceptButton = ok;
        dlg.CancelButton = tyh;
        Teema.Rakenda(dlg);
        IWin32Window? omanik = Form.ActiveForm;
        var tulemus = omanik != null ? dlg.ShowDialog(omanik) : dlg.ShowDialog();
        return tulemus == DialogResult.OK ? t.Text : null;
    }

    public static DialogResult Show(string tekst) =>
        Show(tekst, "", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static DialogResult Show(string tekst, string pealkiri) =>
        Show(tekst, pealkiri, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static DialogResult Show(string tekst, string pealkiri, MessageBoxButtons nupud) =>
        Show(tekst, pealkiri, nupud, nupud == MessageBoxButtons.YesNo ? MessageBoxIcon.Question : MessageBoxIcon.Information);

    public static DialogResult Show(string tekst, string pealkiri, MessageBoxButtons nupud, MessageBoxIcon ikoon)
    {
        TeateTyyp tyyp = ikoon switch
        {
            MessageBoxIcon.Error => TeateTyyp.Viga,          // = Hand = Stop
            MessageBoxIcon.Warning => TeateTyyp.Hoiatus,     // = Exclamation
            MessageBoxIcon.Question => TeateTyyp.Kysimus,
            _ => TeateTyyp.Info
        };

        using var vorm = new TeateVorm(Keel.TolgiTekst(tekst), Keel.T(pealkiri), nupud, tyyp);
        IWin32Window? omanik = Form.ActiveForm;
        return omanik != null ? vorm.ShowDialog(omanik) : vorm.ShowDialog();
    }

    // ---------------------------------------------------------------
    //  Toast
    // ---------------------------------------------------------------
    private static readonly List<ToastVorm> avatud = new();

    public static void Toast(string tekst, TeateTyyp tyyp = TeateTyyp.Edu, int millisekundit = 2800)
    {
        Form? omanik = Form.ActiveForm;
        if (omanik == null || omanik is ToastVorm)
            omanik = Application.OpenForms.Cast<Form>().LastOrDefault(f => f.Visible && f is not ToastVorm);

        // Korraga kuni 3 teadet: vanim kaob
        while (avatud.Count >= 3) { var vana = avatud[0]; avatud.RemoveAt(0); vana.Close(); }

        var toast = new ToastVorm(Keel.TolgiTekst(tekst), tyyp, millisekundit);

        // Paremale alla; mitu teadet kuhjuvad üksteise kohale
        Rectangle ala = omanik != null && omanik.WindowState != FormWindowState.Minimized
            ? omanik.Bounds
            : Screen.PrimaryScreen!.WorkingArea;
        int y = ala.Bottom - 24;
        foreach (var t in avatud) y -= t.Height + 8;
        toast.Location = new Point(ala.Right - toast.Width - 24, y - toast.Height);

        avatud.Add(toast);
        toast.FormClosed += (s, e) => avatud.Remove(toast);

        if (omanik != null) toast.Show(omanik); else toast.Show();
    }

    public static Color TyybiVarv(TeateTyyp t) => t switch
    {
        TeateTyyp.Edu => Teema.Edu,
        TeateTyyp.Hoiatus => Teema.Hoiatus,
        TeateTyyp.Viga => Teema.Viga,
        _ => Teema.Aktsent
    };

    public static string TyybiMark(TeateTyyp t) => t switch
    {
        TeateTyyp.Edu => "✓",
        TeateTyyp.Hoiatus => "!",
        TeateTyyp.Viga => "✕",
        TeateTyyp.Kysimus => "?",
        _ => "i"
    };

    // Ümmargune ikoon (märk ringis)
    public static void JoonistaIkoon(Graphics g, Rectangle r, TeateTyyp tyyp)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var b = new SolidBrush(TyybiVarv(tyyp));
        g.FillEllipse(b, r);
        // ✓ ja ✕ vektorina (kõigis fontides neid märke pole)
        if (tyyp is TeateTyyp.Edu or TeateTyyp.Viga)
        {
            Ikoonid.Joonista(g, tyyp == TeateTyyp.Edu ? "\u2713" : "\uE711", r, Color.White, r.Height * 0.36f * 72f / g.DpiX);
            return;
        }
        using var f = new Font("Segoe UI", r.Height * 0.42f, FontStyle.Bold, GraphicsUnit.Pixel);
        TextRenderer.DrawText(g, TyybiMark(tyyp), f, r, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }
}

// Dialoogiaken teate jaoks
public class TeateVorm : Form
{
    private readonly TeateTyyp tyyp;

    public TeateVorm(string tekst, string pealkiri, MessageBoxButtons nupud, TeateTyyp tyyp)
    {
        this.tyyp = tyyp;
        if (string.IsNullOrWhiteSpace(pealkiri))
            pealkiri = Keel.T(tyyp switch
            {
                TeateTyyp.Viga => "Viga",
                TeateTyyp.Hoiatus => "Hoiatus",
                TeateTyyp.Kysimus => "Kinnitus",
                _ => "Teade"
            });

        Text = pealkiri;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Font = UiStiil.Põhifont;
        BackColor = Teema.Pind;
        ForeColor = Teema.Tekst;

        var ikoon = new Panel { Location = new Point(24, 24), Size = new Size(40, 40), BackColor = Teema.Pind };
        ikoon.Paint += (s, e) => Teade.JoonistaIkoon(e.Graphics, new Rectangle(0, 0, 39, 39), this.tyyp);

        var silt = new Label
        {
            Text = pealkiri,
            AutoSize = true,
            Location = new Point(80, 22),
            Font = new Font("Segoe UI Semibold", 12F),
            ForeColor = Teema.Tekst
        };

        var sisu = new Label
        {
            Text = tekst,
            AutoSize = true,
            MaximumSize = new Size(460, 0),
            Location = new Point(80, 52),
            ForeColor = Teema.Tekst2,
            Font = new Font("Segoe UI", 9.5F)
        };
        int sisuKorgus = sisu.GetPreferredSize(new Size(460, 0)).Height;
        int sisuLaius = sisu.GetPreferredSize(new Size(460, 0)).Width;

        // Nupuriba
        var riba = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Teema.Paneel };
        var nupuRida = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 13, 16, 0),
            BackColor = Teema.Paneel,
            WrapContents = false
        };

        Button Lisa(string t, DialogResult tulemus, bool peamine)
        {
            var n = UiStiil.LooNupp(t, null, peamine);
            n.MinimumSize = new Size(90, 34);
            n.DialogResult = tulemus;
            nupuRida.Controls.Add(n);
            return n;
        }

        switch (nupud)
        {
            case MessageBoxButtons.YesNo:
                var jah = Lisa("Jah", DialogResult.Yes, true);
                var ei = Lisa("Ei", DialogResult.No, false);
                AcceptButton = jah;
                CancelButton = ei;
                break;
            case MessageBoxButtons.OKCancel:
                var ok2 = Lisa("OK", DialogResult.OK, true);
                var tyh = Lisa("Tühista", DialogResult.Cancel, false);
                AcceptButton = ok2;
                CancelButton = tyh;
                break;
            case MessageBoxButtons.YesNoCancel:
                var j = Lisa("Jah", DialogResult.Yes, true);
                Lisa("Ei", DialogResult.No, false);
                var t3 = Lisa("Tühista", DialogResult.Cancel, false);
                AcceptButton = j;
                CancelButton = t3;
                break;
            default:
                var ok = Lisa("OK", DialogResult.OK, true);
                AcceptButton = ok;
                CancelButton = ok;
                break;
        }
        riba.Controls.Add(nupuRida);

        Controls.Add(ikoon);
        Controls.Add(silt);
        Controls.Add(sisu);
        Controls.Add(riba);

        int laius = Math.Max(380, Math.Max(sisuLaius, silt.GetPreferredSize(Size.Empty).Width) + 80 + 32);
        ClientSize = new Size(laius, Math.Max(52 + sisuKorgus, 64) + 28 + riba.Height);

        // Moodne (lõuend 3.2): ümar kaart, ikoon keskel üleval, tekst ja "pill" nupud keskel
        if (Teema.OnMoodne)
        {
            moodne = true;
            FormBorderStyle = FormBorderStyle.None;
            int w = Math.Max(400, sisuLaius + 64);
            ikoon.Bounds = new Rectangle((w - 56) / 2, 28, 56, 56);
            silt.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            silt.Location = new Point((w - silt.GetPreferredSize(Size.Empty).Width) / 2, 98);
            sisu.TextAlign = ContentAlignment.TopCenter;
            sisu.AutoSize = false;
            sisu.Bounds = new Rectangle(32, 132, w - 64, sisuKorgus + 4);
            riba.BackColor = nupuRida.BackColor = Teema.Pind;
            riba.Height = 76;
            nupuRida.Dock = DockStyle.None;
            nupuRida.Padding = new Padding(0);
            foreach (Control n in nupuRida.Controls) { n.MinimumSize = new Size(130, 44); n.Margin = new Padding(6, 0, 6, 0); }
            riba.Resize += (s, e) => nupuRida.Location = new Point((riba.Width - nupuRida.Width) / 2, 12);
            ClientSize = new Size(w, 132 + sisuKorgus + 20 + riba.Height);
        }
    }

    private readonly bool moodne;
    private double ilmub = 1;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (!moodne) return;
        using var tee = Ymardus.Tee(new Rectangle(0, 0, Width, Height), 28);
        Region = new Region(tee);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (!moodne) return;
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var p = new Pen(Teema.Joon, 2);
        using var tee = Ymardus.Tee(new RectangleF(1, 1, Width - 3, Height - 3), 27);
        e.Graphics.DrawPath(p, tee);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Teema.Rakenda(this);
        // Rakenda värvib ka ikoonipaneeli ja nupuriba õigesti (Pind / Paneel)
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (moodne)
        {
            // hüppab sisse: läbipaistvus 0 → 1, 8 px alt üles
            int y0 = Top;
            Animatsioon.Kaivita(this, 260, t => { ilmub = Animatsioon.Sujuv(t); Opacity = ilmub; Top = y0 + (int)((1 - ilmub) * 8); });
        }
        System.Media.SystemSounds.Asterisk.Play();
        if (AcceptButton is Control c) c.Focus();
    }
}

// Väike hüpikteade, kaob ise
public class ToastVorm : Form
{
    private readonly System.Windows.Forms.Timer taimer = new();
    private readonly TeateTyyp tyyp;
    private readonly string tekst;
    private bool kaob;

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x08000000;   // WS_EX_NOACTIVATE – ei võta fookust
            return cp;
        }
    }

    public ToastVorm(string tekst, TeateTyyp tyyp, int millisekundit)
    {
        this.tekst = tekst;
        this.tyyp = tyyp;

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        BackColor = Teema.Tume ? Color.FromArgb(58, 60, 66) : Color.FromArgb(38, 40, 44);
        Font = new Font("Segoe UI", 9.75F);
        DoubleBuffered = true;
        Cursor = Cursors.Hand;

        var mootmed = TextRenderer.MeasureText(tekst, Font, new Size(300, 0), TextFormatFlags.WordBreak);
        Size = new Size(380, Math.Max(56, mootmed.Height + 30));

        using (var path = Ymardus.Tee(new Rectangle(0, 0, Width, Height), 12))
            Region = new Region(path);

        Click += (s, e) => Kao();
        Opacity = 0;

        taimer.Interval = 15;
        int kulunud = 0;
        taimer.Tick += (s, e) =>
        {
            kulunud += taimer.Interval;
            if (!kaob && Opacity < 0.96) Opacity = Math.Min(0.96, Opacity + 0.12);   // ilmub
            else if (!kaob && kulunud >= millisekundit) kaob = true;
            else if (kaob)
            {
                Opacity -= 0.08;                                                    // kaob
                if (Opacity <= 0.05) { taimer.Stop(); Close(); }
            }
        };
        taimer.Start();
    }

    private void Kao() => kaob = true;

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Värviline riba vasakul + ikoon
        using (var b = new SolidBrush(Teade.TyybiVarv(tyyp)))
            g.FillRectangle(b, 0, 0, 5, Height);
        Teade.JoonistaIkoon(g, new Rectangle(18, (Height - 26) / 2, 26, 26), tyyp);

        TextRenderer.DrawText(g, tekst, Font, new Rectangle(56, 0, Width - 70, Height), Color.White,
            TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) taimer.Dispose();
        base.Dispose(disposing);
    }
}
