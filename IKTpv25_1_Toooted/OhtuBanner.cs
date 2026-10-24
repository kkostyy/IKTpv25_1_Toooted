using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted;

// Tume-ploomikarva bänner kuuga: "Õhtune küpsetiste tund".
// Enne 20:00 loeb alla ("Algab 12 min pärast"), pärast seda "Kehtib kuni 00:00". Tähed vilguvad, kuu hõljub.
public class OhtuBanner : Control
{
    private double aeg;

    public OhtuBanner()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = 150;
        Margin = new Padding(0, 0, 0, 18);
        Tag = "laius";
        Animatsioon.Tsykkel(this, s => { aeg = s; Invalidate(); }, 50);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? Teema.Taust);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var nyyd = Hinnad.Kell();
        bool kaib = OhtuSoodustus.Kaib(nyyd);
        var s = OhtuSoodustus.S;
        Color taust = kaib ? Color.FromArgb(43, 19, 64) : Color.FromArgb(58, 31, 79);
        var r = new RectangleF(0.5f, 0.5f, Width - 2, Height - 2);
        using (var tee = Ymardus.Tee(r, 28))
        using (var b = new SolidBrush(taust))
            g.FillPath(b, tee);

        // tähed ja kuu
        var olek = g.Save();
        using (var klipp = Ymardus.Tee(r, 28)) g.SetClip(klipp);
        float[][] tahed = { new[] { 0.42f, 0.2f }, new[] { 0.5f, 0.6f }, new[] { 0.58f, 0.25f }, new[] { 0.66f, 0.75f }, new[] { 0.36f, 0.8f } };
        for (int i = 0; i < tahed.Length; i++)
        {
            float a = (float)(0.2 + 0.8 * (0.5 + 0.5 * Math.Sin(aeg * 2.6 + i * 1.3)));
            using var b = new SolidBrush(Color.FromArgb((int)(a * 255), Color.White));
            float sz = 3 + 2 * a;
            g.FillEllipse(b, r.Width * tahed[i][0], r.Height * tahed[i][1], sz, sz);
        }
        float ky = 22 + (float)Math.Sin(aeg * 1.25) * 6;
        var kuu = new RectangleF(r.Right - 140, ky, 84, 84);
        using (var b = new SolidBrush(Color.FromArgb(245, 212, 106))) g.FillEllipse(b, kuu);
        using (var b = new SolidBrush(taust)) g.FillEllipse(b, kuu.X - 22, kuu.Y - 8, 84, 84);
        g.Restore(olek);

        using var fp = new Font("Segoe UI", 7.5F, FontStyle.Bold);
        using var ft = new Font("Segoe UI", 20F, FontStyle.Bold);
        using var f = new Font("Segoe UI", 10F);
        using var fk = new Font("Consolas", 22F, FontStyle.Bold);
        using var fs = new Font("Segoe UI Semibold", 9.5F);

        // märk "KÄIB PRAEGU" / "TÄNA ÕHTUL"
        string marg = Keel.T(kaib ? "KÄIB PRAEGU" : "TÄNA ÕHTUL");
        int mw = TextRenderer.MeasureText(marg, fp).Width + 30;
        var mr = new RectangleF(24, 18, mw, 24);
        using (var tee = Ymardus.Tee(mr, 12)) using (var b = new SolidBrush(Color.FromArgb(36, Color.White))) g.FillPath(b, tee);
        bool vilgub = (int)(aeg * 2) % 2 == 0;
        using (var b = new SolidBrush(kaib ? Color.FromArgb(vilgub ? 255 : 110, 90, 200, 130) : Color.FromArgb(245, 212, 106))) g.FillEllipse(b, mr.X + 10, mr.Y + 8, 8, 8);
        TextRenderer.DrawText(g, marg, fp, Rectangle.Round(new RectangleF(mr.X + 22, mr.Y, mr.Width - 22, mr.Height)), Color.White, TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(g, Keel.T("Õhtune küpsetiste tund"), ft, new Point(22, 46), Color.White);
        TextRenderer.DrawText(g, Keel.F("Pärast kella {0:hh\\:mm} on kõik leivad, saiad ja saiakesed {1} % odavamad.", s.Algus, s.Protsent), f, new Point(26, 86), Color.FromArgb(232, 225, 240));

        // kell + olek
        TextRenderer.DrawText(g, nyyd.ToString("HH:mm"), fk, new Point(24, 108), Color.White);
        string olekTekst = kaib ? Keel.T("Kehtib kuni 00:00")
            : OhtuSoodustus.Alguseni(nyyd) is TimeSpan j
                ? Keel.F("Algab {0} pärast", j.TotalMinutes >= 60 ? $"{(int)j.TotalHours} h {j.Minutes} min" : $"{Math.Max(1, (int)Math.Ceiling(j.TotalMinutes))} min")
                : "";
        int ow = TextRenderer.MeasureText(olekTekst, fs).Width + 26;
        var or = new RectangleF(120, 116, ow, 26);
        using (var tee = Ymardus.Tee(or, 13))
        using (var b = new SolidBrush(kaib ? Color.FromArgb(245, 212, 106) : Color.FromArgb(!vilgub ? 36 : 60, Color.White)))
            g.FillPath(b, tee);
        TextRenderer.DrawText(g, olekTekst, fs, Rectangle.Round(or), kaib ? Color.FromArgb(43, 19, 64) : Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

// Haldus: õhtuse soodustuse seaded (algus, protsent, kategooria, sees/väljas)
public class OhtuSeadedVorm : KujundusVorm
{
    private readonly CheckBox chkSees;
    private readonly DateTimePicker dtAlgus;
    private readonly NumericUpDown numProtsent;
    private readonly ComboBox cmbKat;

    public OhtuSeadedVorm() : base("Õhtune soodustus", 440, 330)
    {
        Pealkiri("Õhtune soodustus");
        var s = OhtuSoodustus.S;
        chkSees = new CheckBox { Text = "Sees", Location = new Point(20, 60), AutoSize = true, Checked = s.Sees };
        Controls.Add(chkSees);
        Silt("Algab", 20, 96);
        dtAlgus = new DateTimePicker { Location = new Point(20, 116), Width = 120, Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Value = DateTime.Today + s.Algus };
        Controls.Add(dtAlgus);
        Silt("Soodustus %", 170, 96);
        numProtsent = new NumericUpDown { Location = new Point(170, 116), Width = 90, Minimum = 5, Maximum = 90, Value = s.Protsent };
        Controls.Add(numProtsent);
        Silt("Kategooria", 20, 156);
        cmbKat = new ComboBox { Location = new Point(20, 176), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
        Controls.Add(cmbKat);
        var selgitus = new Label { Text = "Ei liitu kampaania ega säilivusaja soodustusega – kehtib suurim. Hind arvutatakse maksmise hetkel.", Location = new Point(20, 210), Size = new Size(400, 36), ForeColor = Teema.TekstNork };
        Controls.Add(selgitus);
        var salvesta = Nupp("Salvesta", 20, 270, true);
        if (Teema.OnMoodne)
        {
            // lõuend N12: reeglid hallil kastil ja näidis, kuidas rida tšekil välja näeb
            ClientSize = new Size(440, 560);
            selgitus.Visible = false;
            var kast = new RoundedPanel { Bounds = new Rectangle(20, 214, 400, 118), Radius = 10, FillColor = Color.FromArgb(246, 248, 249), BorderColor = Color.FromArgb(246, 248, 249), Padding = new Padding(14, 10, 14, 10) };
            kast.Controls.Add(new Label
            {
                Text = "• " + Keel.F("Kehtib iga päev {0:hh\\:mm} kuni poe sulgemiseni (00:00).", s.Algus) + "\n• " + Keel.T("Ei liitu kampaania ega säilivusaja soodustusega – rakendub suurim.") + "\n• " + Keel.T("Hind arvutatakse kassas ostu hetkel, mitte korvi lisamisel."),
                Dock = DockStyle.Fill, ForeColor = Teema.Tekst2, BackColor = kast.FillColor, Font = new Font("Segoe UI", 9F)
            });
            Controls.Add(kast);
            Controls.Add(new Label { Text = Keel.T("TŠEKIL"), Location = new Point(20, 346), AutoSize = true, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Teema.TekstNork });
            var tume = Color.FromArgb(27, 28, 31);
            var tsekk = new RoundedPanel { Bounds = new Rectangle(20, 368, 400, 70), Radius = 8, FillColor = tume, BorderColor = tume, Padding = new Padding(14, 10, 14, 8) };
            decimal naideHind = 1.69m, pool = Math.Round(naideHind * s.Protsent / 100m, 2);
            var tsekiFont = new Font("Consolas", 9.5F);
            tsekk.Controls.Add(new Label { Text = $"{Keel.F("Õhtune soodustus −{0} %", s.Protsent)}", Dock = DockStyle.Top, Height = 22, Font = tsekiFont, ForeColor = Color.FromArgb(245, 212, 106), BackColor = tume, Padding = new Padding(12, 0, 0, 0) });
            var r1 = new Label { Text = "1 x Rukkileib Rehe", Dock = DockStyle.Top, Height = 22, Font = tsekiFont, ForeColor = Color.White, BackColor = tume };
            tsekk.Controls.Add(r1);
            var summad = new Label { Text = $"{naideHind:0.00}\n-{pool:0.00}", Dock = DockStyle.Right, Width = 70, TextAlign = ContentAlignment.TopRight, Font = tsekiFont, ForeColor = Color.White, BackColor = tume };
            tsekk.Controls.Add(summad);
            summad.SendToBack();   // dokitakse esimesena: kogu kõrgus paremal
            Controls.Add(tsekk);
            salvesta.Location = new Point(20, 470);
            salvesta.Size = new Size(400, 52);
        }
        salvesta.Click += (sd, e) => Salvesta();
        Load += (sd, e) =>
        {
            using var db = new ToodedContext();
            foreach (var k in db.Kategooriatabel.OrderBy(k => k.Id).ToList())
            {
                int i = cmbKat.Items.Add(new KeyValuePair<int, string>(k.Id, Keel.T(k.Kategooria_nimetus)));
                if (k.Id == s.KategooriaId) cmbKat.SelectedIndex = i;
            }
            cmbKat.DisplayMember = "Value";
        };
    }

    private void Salvesta()
    {
        OhtuSoodustus.Salvesta(new OhtuSoodustus.Seaded
        {
            Sees = chkSees.Checked,
            Algus = new TimeSpan(dtAlgus.Value.Hour, dtAlgus.Value.Minute, 0),
            Protsent = (int)numProtsent.Value,
            KategooriaId = cmbKat.SelectedItem is KeyValuePair<int, string> kv ? kv.Key : 2
        });
        Teade.Toast("Seaded salvestatud");
        DialogResult = DialogResult.OK;
    }
}
