namespace IKTpv25_1_Toooted;

// Demoandmed (omanik): kaks loendurit (tooted, ostud), edenemisriba, lisa / eemalda
public class DemoVorm : KujundusVorm
{
    private readonly Loendur tooteid = new("toodet"), oste = new("ostu");
    private readonly Edenemine riba = new() { Visible = false };
    private readonly Label lblOlek;
    private readonly RoundedButton btnLisa, btnEemalda;

    public DemoVorm() : base("Demoandmed", 520, 380)
    {
        Pealkiri("Demoandmed");
        Controls.Add(new Label
        {
            Text = "Üks nupp lisab 50 toodet, 3 klienti ja 20 ostu viimase 30 päeva jooksul, et programm ei näeks esitlusel tühi välja. Demoandmed on märgitud ja neid saab hiljem korraga eemaldada.",
            Location = new Point(20, 56), Size = new Size(480, 56), ForeColor = Teema.Tekst2
        });
        tooteid.Location = new Point(20, 120);
        oste.Location = new Point(270, 120);
        Controls.Add(tooteid);
        Controls.Add(oste);
        riba.Bounds = new Rectangle(20, 236, 480, 8);
        Controls.Add(riba);
        lblOlek = new Label { Location = new Point(20, 252), Size = new Size(480, 22), ForeColor = Teema.TekstNork };
        Controls.Add(lblOlek);

        btnLisa = (RoundedButton)Nupp("Lisa demoandmed", 20, 300, true);
        btnLisa.Click += (s, e) => Lisa();
        btnEemalda = (RoundedButton)Nupp("Eemalda demoandmed", 200, 300);
        btnEemalda.Ohtlik = true;
        btnEemalda.Click += (s, e) => Eemalda();

        Load += (s, e) => Uuenda(false);
    }

    private void Uuenda(bool animeeri)
    {
        using var db = new ToodedContext();
        var (t, o) = DemoAndmed.Arvud(db);
        tooteid.Naita(t, animeeri);
        oste.Naita(o, animeeri);
        btnLisa.Enabled = t == 0;
        btnEemalda.Enabled = t > 0 || o > 0;
    }

    private void Lisa()
    {
        btnLisa.Enabled = false;
        riba.Visible = true;
        riba.Osa = 0;
        lblOlek.Text = Keel.T("Loon demoandmeid...");
        Application.DoEvents();
        try
        {
            using var db = new ToodedContext();
            string tulemus = DemoAndmed.Lisa(db, osa => { riba.Osa = (float)osa; riba.Refresh(); });
            lblOlek.Text = tulemus;
            Teade.Toast("Demoandmed lisatud");
        }
        catch (Exception ex) { lblOlek.Text = Keel.T("Viga:") + " " + ex.Message; }
        Uuenda(true);
    }

    private void Eemalda()
    {
        if (Teade.Show("Kas eemaldan kõik demoandmed (tooted, kliendid, ostud)?", "Demoandmed", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            using var db = new ToodedContext();
            lblOlek.Text = DemoAndmed.Eemalda(db);
            riba.Visible = false;
            Teade.Toast("Demoandmed eemaldatud", TeateTyyp.Info);
        }
        catch (Exception ex) { lblOlek.Text = Keel.T("Viga:") + " " + ex.Message; }
        Uuenda(true);
    }
}

// Suur number, mis loeb animatsiooniga 0 → N
public class Loendur : Control
{
    private readonly string silt;
    private int siht, nahtav;

    public Loendur(string silt)
    {
        this.silt = silt;
        Size = new Size(230, 100);
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public void Naita(int arv, bool animeeri)
    {
        int algus = animeeri ? nahtav : arv;
        siht = arv;
        Animatsioon.Kaivita(this, animeeri ? 900 : 0, t => { nahtav = (int)Math.Round(algus + (siht - algus) * Animatsioon.Sujuv(t)); Invalidate(); });
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Pind);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var r = new RectangleF(0.5f, 0.5f, Width - 2, Height - 2);
        using (var tee = Ymardus.Tee(r, 20))
        using (var b = new SolidBrush(Teema.Paneel2))
            g.FillPath(b, tee);
        using var f = new Font("Segoe UI", 30F, FontStyle.Bold);
        using var fs = new Font("Segoe UI", 10F);
        TextRenderer.DrawText(g, nahtav.ToString(), f, new Point(16, 8), Teema.Aktsent);
        TextRenderer.DrawText(g, Keel.T(silt), fs, new Point(20, 66), Teema.Tekst2);
    }
}
