using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted
{
    // Seaded moodsas disainis (lõuend 3.3): vasakul jaotised, valikud segmentidena
    // (teema, disain, keel) ja poe värv värvitäppidena.
    public partial class SeadedVorm
    {
        private void RakendaMoodne()
        {
            const int nihe = 230;
            BackColor = Teema.Pind;
            foreach (Control c in Controls) c.Left += nihe;
            ClientSize = new Size(ClientSize.Width + nihe, ClientSize.Height);

            var jaotised = new SeadeteJaotised { Bounds = new Rectangle(16, 16, 200, ClientSize.Height - 32), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left };
            jaotised.Lisa("Profiil", "", 58);
            jaotised.Lisa("Turvalisus", "", 368);
            jaotised.Lisa("Välimus", "", 446);
            Controls.Add(jaotised);

            // Teema: Hele / Tume
            var teema = new Segmendid(new[] { "Hele", "Tume" }, () => Teema.Tume ? 1 : 0, i => cmbTeema.SelectedIndex = i) { Location = cmbTeema.Location, Width = 250 };
            // Disain: Rimi / Klassikaline / Moodne
            var disain = new Segmendid(new[] { "Rimi", "Klassik", "Moodne" }, () => cmbDisain.SelectedIndex, i => cmbDisain.SelectedIndex = i) { Location = cmbDisain.Location, Width = 250 };
            var keel = new KeelePill { Location = cmbKeel.Location };
            var varvid = new VarviTapid(cmbVarv) { Location = cmbVarv.Location, Width = 250 };
            foreach (var c in new[] { cmbTeema, cmbDisain, cmbKeel, cmbVarv }) c.Visible = false;
            Controls.AddRange(new Control[] { teema, disain, keel, varvid });

            // Jaotised päris vahelehtedena: igaüks oma paneelil, nähtav ainult valitud
            var piirid = new[] { 58, 368, 446, 620 };
            var lehed = new List<Panel>();
            for (int i = 0; i < 3; i++)
            {
                int algus = piirid[i], lopp = piirid[i + 1];
                var leht = new Panel { Location = new Point(nihe - 6, 50), Size = new Size(ClientSize.Width - nihe + 6, 560), BackColor = Teema.Pind, Visible = i == 0 };
                foreach (var c in Controls.Cast<Control>().Where(c => c != jaotised && c.Top >= algus && c.Top < lopp && c.Left >= nihe - 6).ToList())
                {
                    Controls.Remove(c);
                    c.Location = new Point(c.Left - (nihe - 6), c.Top - algus + 8);
                    leht.Controls.Add(c);
                }
                lehed.Add(leht);
                Controls.Add(leht);
            }
            jaotised.ValitudNr += i =>
            {
                for (int k = 0; k < lehed.Count; k++) lehed[k].Visible = k == i;
                var l = lehed[i];
                int y0 = l.Top;
                Animatsioon.Kaivita(l, 220, t => l.Top = y0 + (int)(12 * (1 - Animatsioon.Sujuv(t))));   // libiseb kergelt üles
            };

            // Salvesta / Sulge: suured "pillid"
            var alumised = Controls.OfType<RoundedButton>().Where(b => b.Top >= 690).OrderBy(b => b.Left).ToList();
            foreach (var b in alumised)
            {
                b.AutoSize = false;
                b.Size = new Size(170, 46);
                b.Font = new Font("Segoe UI Semibold", 10.5F);
            }
            if (alumised.Count == 2) alumised[1].Left = alumised[0].Right + 12;
            Teema.Muutus += teema.Invalidate;
            Teema.Muutus += varvid.Invalidate;
            FormClosed += (s, e) => { Teema.Muutus -= teema.Invalidate; Teema.Muutus -= varvid.Invalidate; };
        }

        // Jaotise pealkiri "hüppab" (et näha, kus see on)
        private void VilgutaPlokki(int y)
        {
            var silt = Controls.OfType<Label>().FirstOrDefault(l => l.Top == y && l.Font == UiStiil.AlapealkiriFont);
            if (silt == null) return;
            var alg = silt.ForeColor;
            Animatsioon.Kaivita(silt, 700, t => silt.ForeColor = Ymardus.Segu(Teema.Aktsent, alg, (float)t));
            if (AutoScroll) ScrollControlIntoView(silt);
        }
    }

    // Vasak jaotiste loend (valge, valitud = tume "pill")
    public class SeadeteJaotised : Control
    {
        private readonly List<(string Nimi, string Ikoon, int Y)> jaotised = new();
        private int valitud, hiir = -1;
        public event Action<int>? Valitud;
        public event Action<int>? ValitudNr;   // jaotise järjekorranumber

        public SeadeteJaotised()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
        }

        public void Lisa(string nimi, string ikoon, int y) => jaotised.Add((nimi, ikoon, y));

        private RectangleF Koht(int i) => new(8, 64 + i * 50, Width - 16, 44);

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int uus = Enumerable.Range(0, jaotised.Count).FirstOrDefault(i => Koht(i).Contains(e.Location), -1);
            if (uus != hiir) { hiir = uus; Invalidate(); }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) { hiir = -1; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            for (int i = 0; i < jaotised.Count; i++)
                if (Koht(i).Contains(e.Location)) { valitud = i; Invalidate(); Valitud?.Invoke(jaotised[i].Y); ValitudNr?.Invoke(i); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Pind);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 24))
            using (var b = new SolidBrush(Teema.Paneel2))
                g.FillPath(b, tee);
            var av = new RectangleF(14, 14, 36, 36);
            using (var b = new SolidBrush(Color.FromArgb(160, 35, 110))) g.FillEllipse(b, av);
            using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
                TextRenderer.DrawText(g, Pere.Initsiaalid(AktiivneKasutaja.Kuvanimi), f, Rectangle.Round(av), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            using (var f = new Font("Segoe UI Semibold", 9.5F))
                TextRenderer.DrawText(g, AktiivneKasutaja.Kuvanimi, f, new Rectangle(58, 22, Width - 64, 20), Teema.Tekst, TextFormatFlags.EndEllipsis);
            using var fp = new Font("Segoe UI Semibold", 10F);
            for (int i = 0; i < jaotised.Count; i++)
            {
                var r = Koht(i);
                bool v = i == valitud;
                if (v || hiir == i)
                    using (var tee = Ymardus.Tee(r, r.Height / 2))
                    using (var b = new SolidBrush(v ? Teema.Tekst : Teema.Joon))
                        g.FillPath(b, tee);
                Ikoonid.Joonista(g, jaotised[i].Ikoon, new Rectangle((int)r.X + 12, (int)r.Y, 22, (int)r.Height), v ? Teema.Pind : Teema.Tekst, 11f);
                TextRenderer.DrawText(g, Keel.T(jaotised[i].Nimi), fp, new Rectangle((int)r.X + 44, (int)r.Y, (int)r.Width - 48, (int)r.Height), v ? Teema.Pind : Teema.Tekst, TextFormatFlags.VerticalCenter);
            }
        }
    }

    // Segmendid (hall "pill", valitud valge varjuga)
    public class Segmendid : Control
    {
        private readonly string[] valikud;
        private readonly Func<int> praegu;
        private readonly Action<int> vali;

        public Segmendid(string[] valikud, Func<int> praegu, Action<int> vali)
        {
            this.valikud = valikud;
            this.praegu = praegu;
            this.vali = vali;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 38;
            Font = new Font("Segoe UI Semibold", 9.5F);
            Cursor = Cursors.Hand;
        }

        private RectangleF Osa(int i) { float w = (Width - 8) / (float)valikud.Length; return new RectangleF(4 + i * w, 4, w, Height - 8); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            for (int i = 0; i < valikud.Length; i++) if (Osa(i).Contains(e.Location)) { vali(i); Invalidate(); }
            base.OnMouseClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Pind);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), (Height - 1) / 2f))
            using (var b = new SolidBrush(Teema.Paneel2))
                g.FillPath(b, tee);
            int v = praegu();
            for (int i = 0; i < valikud.Length; i++)
            {
                var r = Osa(i);
                if (i == v)
                    using (var tee = Ymardus.Tee(r, r.Height / 2))
                    using (var b = new SolidBrush(Teema.Tekst))
                        g.FillPath(b, tee);
                TextRenderer.DrawText(g, Keel.T(valikud[i]), Font, Rectangle.Round(r), i == v ? Teema.Pind : Teema.Tekst, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }

    // Poe värv: värvitäpid (valitud = rõngas ümber)
    public class VarviTapid : Control
    {
        private readonly ComboBox cmb;

        public VarviTapid(ComboBox cmb)
        {
            this.cmb = cmb;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Height = 38;
            Cursor = Cursors.Hand;
        }

        private RectangleF Tapp(int i) => new(4 + i * 42, 4, 30, 30);

        protected override void OnMouseClick(MouseEventArgs e)
        {
            for (int i = 0; i < Teema.Aktsendid.Count; i++) if (Tapp(i).Contains(e.Location)) cmb.SelectedIndex = i;
            Invalidate();
            base.OnMouseClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Pind);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int i = 0;
            foreach (var (nimi, v) in Teema.Aktsendid)
            {
                var r = Tapp(i++);
                using (var b = new SolidBrush(Teema.Tume ? v.Tume : v.Hele)) g.FillEllipse(b, RectangleF.Inflate(r, -4, -4));
                if (nimi == Teema.AktsendiNimi)
                    using (var p = new Pen(Teema.Tekst, 2)) g.DrawEllipse(p, r);
            }
        }
    }
}
