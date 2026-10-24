using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted
{
    // ===================================================================
    //  E-pood moodsas disainis (Ctrl+Shift+M), nagu lõuendil "1.2 Pood":
    //  vasakul valge ikoonimenüü, ülal otsingu "pill" + tumedad vahelehed + keel,
    //  avalehel "bento" (nädala pakkumine, tasuta tarne rõngas, kategooriad),
    //  paremal ümar ostukorv.
    // ===================================================================
    public partial class PoodVorm
    {
        private readonly bool moodne = Teema.OnMoodne;
        private MoodneKylgriba? kylgriba;
        private MoodneTabid? tabid;

        private void RakendaMoodne(Panel lilla, Panel pais, FlowLayoutPanel nav, Panel keskmine, Panel korv, Panel korviPais)
        {
            BackColor = Teema.Taust;
            Padding = new Padding(16);
            lilla.Visible = pais.Visible = nav.Visible = rail.Visible = false;
            kulg.PeidaMoodsas = true;

            // ---- Vasak valge ikoonimenüü ----
            kylgriba = new MoodneKylgriba { Dock = DockStyle.Left, Width = 72 };
            kylgriba.Lisa("avaleht", "", "Avaleht", NaitaAvaleht);
            kylgriba.Lisa("kampaaniad", "", "Kampaaniad", () => NaitaRuudustik(Vaade.Kampaaniad));
            kylgriba.Lisa("lemmikud", "", "Minu lemmikud", () => NaitaRuudustik(Vaade.Lemmikud));
            kylgriba.Lisa("kaart", "", "Kliendikaart", () => KliendikaartVorm.Ava(this));
            kylgriba.Lisa("preemiad", "", "Preemiad", () => { if (lehed.Praegune is not PreemiadLeht) AvaLeht(new PreemiadLeht()); });
            kylgriba.Lisa("nimekiri", "", "Ostunimekiri", AvaNimekiri, Ostunimekiri.OstmataArv);
            kylgriba.Lisa("seaded", "", "Seaded", () => SeadedVorm.Ava(this));
            kylgriba.Valju = Close;
            kylgriba.Profiil = () => SeadedVorm.Ava(this);
            Controls.Add(new Panel { Dock = DockStyle.Left, Width = 16, BackColor = Teema.Taust });
            Controls.Add(kylgriba);

            // ---- Ülariba: otsing, vahelehed, keel ----
            var yla = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Teema.Taust, Padding = new Padding(0, 0, 0, 16) };
            var otsing = new RoundedPanel { Location = new Point(0, 0), Size = new Size(330, 56), Radius = 14, FillColor = Teema.Pind, BorderColor = Teema.Pind, BackColor = Teema.Taust, Padding = new Padding(58, 17, 16, 0) };
            var luup = new Label { Text = Ikoonid.Otsi, Font = new Font("Segoe MDL2 Assets", 12F), Size = new Size(40, 40), Location = new Point(8, 8), TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, BackColor = Teema.Pind, Cursor = Cursors.Hand };
            luup.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.Clear(Teema.Pind);
                using var b = new SolidBrush(Teema.Aktsent);
                e.Graphics.FillEllipse(b, 0, 0, 39, 39);
                Ikoonid.Joonista(e.Graphics, Ikoonid.Otsi, new Rectangle(0, 0, 40, 40), Color.White, 12f);
            };
            luup.Click += (s, e) => Otsi();
            txtOtsi.Parent?.Controls.Remove(txtOtsi);
            txtOtsi.BackColor = Teema.Pind;
            txtOtsi.Font = new Font("Segoe UI", 11.5F);
            txtOtsi.PlaceholderText = Keel.T("Otsi toodet…");
            otsing.Controls.Add(txtOtsi);
            otsing.Controls.Add(luup);
            luup.BringToFront();

            tabid = new MoodneTabid { Location = new Point(346, 0) };
            tabid.Lisa("Tooted", Vaade.Avaleht, NaitaAvaleht);
            tabid.Lisa("Kampaaniad", Vaade.Kampaaniad, () => NaitaRuudustik(Vaade.Kampaaniad));
            tabid.Lisa("Tõesti odav!", Vaade.Odav, () => NaitaRuudustik(Vaade.Odav));
            tabid.Lisa("Lemmikud", Vaade.Lemmikud, () => NaitaRuudustik(Vaade.Lemmikud));
            var keel = new KeelePill { BackColor = Teema.Taust };
            yla.Controls.AddRange(new Control[] { otsing, tabid, keel });
            yla.Resize += (s, e) => keel.Location = new Point(yla.Width - keel.Width, 9);
            keskmine.Controls.Add(yla);
            yla.SendToBack();   // kõige üles

            // ---- Ostukorv: ümar valge kaart, vahe keskosast ----
            korv.Resize += (s, e) =>
            {
                using var tee = Ymardus.Tee(new Rectangle(0, 0, korv.Width, korv.Height), 32);
                korv.Region = new Region(tee);
            };
            Controls.Add(new Panel { Dock = DockStyle.Right, Width = 16, BackColor = Teema.Taust });
            foreach (var l in korviPais.Controls.OfType<Label>())
            {
                l.Text = "Ostukorv";
                l.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
                l.Location = new Point(22, 14);
            }
            korv.Width = 360;
        }

        private void MoodneValik()
        {
            if (!moodne) return;
            kylgriba?.Vali(vaade switch { Vaade.Avaleht => "avaleht", Vaade.Kampaaniad => "kampaaniad", Vaade.Lemmikud => "lemmikud", _ => null });
            tabid?.Vali(vaade);
        }

        // Avalehe "bento": nädala pakkumine + tasuta tarne + kategooriad
        private Control LooBento()
        {
            var bento = new TableLayoutPanel { Height = 372, ColumnCount = 2, RowCount = 2, BackColor = Teema.Taust, Margin = new Padding(0, 0, 0, 16), Tag = "laius" };
            bento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            bento.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            bento.RowStyles.Add(new RowStyle(SizeType.Absolute, 166));
            bento.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var parim = koikTooted.Where(t => t.Kogus > 0 && t.Pilt != null).OrderByDescending(t => Hinnad.Lopphind(t).Protsent).FirstOrDefault();
            if (parim != null)
            {
                var hero = new NadalaPakkumine(parim, () => LisaKorvi(parim), () => AvaToode(parim)) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
                bento.Controls.Add(hero, 0, 0);
                bento.SetRowSpan(hero, 2);
            }
            bento.Controls.Add(new TarneRong { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 16) }, 1, 0);

            var kat = new RoundedPanel { Dock = DockStyle.Fill, Radius = 14, FillColor = Color.FromArgb(160, 35, 110), BorderColor = Color.FromArgb(160, 35, 110), BackColor = Teema.Taust, Padding = new Padding(22, 18, 22, 14), Margin = new Padding(0) };
            var chips = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(160, 35, 110), WrapContents = true };
            foreach (var k in kategooriad)
            {
                int id = k.Id;
                var c = new KiipNupp(k.Kategooria_nimetus) { Margin = new Padding(0, 0, 8, 8) };
                c.Click += (s, e) => NaitaKategooria(id);
                chips.Controls.Add(c);
            }
            kat.Controls.Add(chips);
            kat.Controls.Add(new Label { Text = "Kategooriad", Dock = DockStyle.Top, Height = 38, Font = new Font("Segoe UI", 13.5F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.FromArgb(160, 35, 110) });
            bento.Controls.Add(kat, 1, 1);
            return bento;
        }
    }

    // Valge ümar ikoonimenüü (72 px): punane logo, ikoonid (valitud = tume ring), märgid, välju, profiil
    public class MoodneKylgriba : Control
    {
        private record Punkt(string Votti, string Ikoon, string Tekst, Action Tegevus, Func<int>? Mark);
        private readonly List<Punkt> punktid = new();
        private readonly ToolTip vihje = new();
        private string? valitud = "avaleht";
        private int hiir = -1;
        private double aeg;
        public Action? Valju { get; set; }
        public Action? Profiil { get; set; }

        public MoodneKylgriba()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Animatsioon.Tsykkel(this, s => { aeg = s; Invalidate(new Rectangle(0, 0, Width, 80)); }, 50);
            Ostukorv.Muutus += Invalidate;
            Ostunimekiri.Muutus += Invalidate;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { Ostukorv.Muutus -= Invalidate; Ostunimekiri.Muutus -= Invalidate; vihje.Dispose(); }
            base.Dispose(disposing);
        }

        public void Lisa(string votti, string ikoon, string tekst, Action tegevus, Func<int>? mark = null) => punktid.Add(new Punkt(votti, ikoon, tekst, tegevus, mark));
        public void Vali(string? votti) { valitud = votti; Invalidate(); }

        private RectangleF Koht(int i) => new((Width - 44) / 2f, 82 + i * 54, 44, 44);
        private RectangleF ValjuKoht => new((Width - 44) / 2f, Height - 124, 44, 44);
        private RectangleF ProfiilKoht => new((Width - 44) / 2f, Height - 66, 44, 44);

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int uus = Enumerable.Range(0, punktid.Count).FirstOrDefault(i => Koht(i).Contains(e.Location), -1);
            if (ValjuKoht.Contains(e.Location)) uus = 100;
            if (ProfiilKoht.Contains(e.Location)) uus = 101;
            if (uus != hiir)
            {
                hiir = uus;
                vihje.SetToolTip(this, uus >= 0 && uus < punktid.Count ? Keel.T(punktid[uus].Tekst) : uus == 100 ? Keel.T("Sulge pood") : uus == 101 ? AktiivneKasutaja.Kuvanimi : "");
                Cursor = uus >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) { hiir = -1; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (ValjuKoht.Contains(e.Location)) { Valju?.Invoke(); return; }
            if (ProfiilKoht.Contains(e.Location)) { Profiil?.Invoke(); return; }
            for (int i = 0; i < punktid.Count; i++)
                if (Koht(i).Contains(e.Location)) punktid[i].Tegevus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Taust);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var ala = new RectangleF(0, 0, Width - 1, Height - 1);
            using (var tee = Ymardus.Tee(ala, 36))
            using (var b = new SolidBrush(Teema.Pind))
                g.FillPath(b, tee);

            // logo pulseerib
            float s = (float)(1 + 0.04 * Math.Sin(aeg * 2.4));
            var logo = new RectangleF(Width / 2f - 24 * s, 14 + 24 - 24 * s, 48 * s, 48 * s);
            using (var b = new SolidBrush(Color.FromArgb(226, 6, 19))) g.FillEllipse(b, logo);
            Ikoonid.Joonista(g, Ikoonid.Ostukorv, Rectangle.Round(logo), Color.White, 12f);

            using var fm = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            for (int i = 0; i < punktid.Count; i++)
            {
                var r = Koht(i);
                bool v = punktid[i].Votti == valitud;
                if (v || hiir == i)
                    using (var b = new SolidBrush(v ? Teema.Tekst : Teema.Paneel2)) g.FillEllipse(b, r);
                Ikoonid.Joonista(g, punktid[i].Ikoon, Rectangle.Round(r), v ? Teema.Pind : Teema.Tekst, 12f);
                int mark = punktid[i].Mark?.Invoke() ?? 0;
                if (mark > 0)
                {
                    var mr = new RectangleF(r.Right - 14, r.Y - 2, 20, 18);
                    using (var tee = Ymardus.Tee(mr, 9)) using (var b = new SolidBrush(Teema.Aktsent)) g.FillPath(b, tee);
                    using (var p = new Pen(Teema.Pind, 2)) using (var tee = Ymardus.Tee(mr, 9)) g.DrawPath(p, tee);
                    TextRenderer.DrawText(g, mark > 9 ? "9+" : mark.ToString(), fm, Rectangle.Round(mr), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }

            using (var p = new Pen(Teema.Joon, 2)) g.DrawLine(p, Width / 2f, Height - 186, Width / 2f, Height - 140);
            var vr = ValjuKoht;
            using (var b = new SolidBrush(hiir == 100 ? Teema.Joon : Teema.Paneel2)) g.FillEllipse(b, vr);
            Ikoonid.Joonista(g, "", Rectangle.Round(vr), Teema.Tekst, 11f);
            var pr = ProfiilKoht;
            using (var b = new SolidBrush(Color.FromArgb(160, 35, 110))) g.FillEllipse(b, pr);
            using var fi = new Font("Segoe UI", 10F, FontStyle.Bold);
            TextRenderer.DrawText(g, Pere.Initsiaalid(AktiivneKasutaja.Kuvanimi), fi, Rectangle.Round(pr), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    // Tume vahelehtede "pill": valitud vaheleht = valge
    public class MoodneTabid : Control
    {
        private readonly List<(string Tekst, object Votti, Action Tegevus, Rectangle Koht)> tabid = new();
        private object? valitud;
        private int hiir = -1;

        public MoodneTabid()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Height = 56;
            Cursor = Cursors.Hand;
            Keel.Muutus += Paiguta;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Keel.Muutus -= Paiguta;
            base.Dispose(disposing);
        }

        public void Lisa(string tekst, object votti, Action tegevus)
        {
            tabid.Add((tekst, votti, tegevus, Rectangle.Empty));
            valitud ??= votti;
            Paiguta();
        }

        private void Paiguta()
        {
            int x = 6;
            for (int i = 0; i < tabid.Count; i++)
            {
                int w = TextRenderer.MeasureText(Keel.T(tabid[i].Tekst), Font).Width + 32;
                tabid[i] = (tabid[i].Tekst, tabid[i].Votti, tabid[i].Tegevus, new Rectangle(x, 8, w, 40));
                x += w + 4;
            }
            Width = x + 2;
            Invalidate();
        }

        public void Vali(object? votti) { valitud = votti; Invalidate(); }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int uus = tabid.FindIndex(t => t.Koht.Contains(e.Location));
            if (uus != hiir) { hiir = uus; Invalidate(); }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) { hiir = -1; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            var t = tabid.FirstOrDefault(x => x.Koht.Contains(e.Location));
            if (t.Tegevus != null) { valitud = t.Votti; Invalidate(); t.Tegevus(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Taust);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 28))
            using (var b = new SolidBrush(Color.FromArgb(27, 28, 31)))
                g.FillPath(b, tee);
            for (int i = 0; i < tabid.Count; i++)
            {
                var (tekst, votti, _, r) = tabid[i];
                bool v = Equals(votti, valitud);
                if (v || hiir == i)
                    using (var tee = Ymardus.Tee(r, 20))
                    using (var b = new SolidBrush(v ? Color.White : Color.FromArgb(40, 255, 255, 255)))
                        g.FillPath(b, tee);
                TextRenderer.DrawText(g, Keel.T(tekst), Font, r, v ? Color.FromArgb(27, 28, 31) : Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }

    // Läbipaistev kiip ploomikarva taustal (kategooriad)
    public class KiipNupp : Control
    {
        private bool hover;
        public KiipNupp(string tekst)
        {
            Text = tekst;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Font = new Font("Segoe UI Semibold", 9.5F);
            Cursor = Cursors.Hand;
            Size = new Size(TextRenderer.MeasureText(Keel.T(tekst), Font).Width + 28, 34);
        }
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Color.FromArgb(160, 35, 110));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 17))
            using (var b = new SolidBrush(Color.FromArgb(hover ? 60 : 30, 255, 255, 255)))
                g.FillPath(b, tee);
            TextRenderer.DrawText(g, Keel.T(Text), Font, ClientRectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }

    // Nädala pakkumine: tume kaart, suur nimi ja hind, −% kiip, toode valges ringis (hõljub)
    public class NadalaPakkumine : Control
    {
        private readonly Toode toode;
        private double aeg;

        public NadalaPakkumine(Toode toode, Action lisa, Action info)
        {
            this.toode = toode;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            var lisaNupp = MoodneUi.NoolNupp("Lisa ostukorvi", (s, e) => lisa());
            lisaNupp.Size = new Size(210, 52);
            var infoNupp = new RoundedButton { Text = Keel.T("Tooteinfo"), Size = new Size(130, 52), Font = new Font("Segoe UI Semibold", 10.5F), Cursor = Cursors.Hand, Peamine = false };
            infoNupp.Click += (s, e) => info();
            infoNupp.Paint += (s, e) =>
            {
                // läbipaistev raamiga nupp tumedal taustal
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.FromArgb(27, 28, 31));
                using var p = new Pen(Color.FromArgb(70, 255, 255, 255), 1.5f);
                using var tee = Ymardus.Tee(new RectangleF(1, 1, infoNupp.Width - 3, infoNupp.Height - 3), 25);
                g.DrawPath(p, tee);
                TextRenderer.DrawText(g, infoNupp.Text, infoNupp.Font, infoNupp.ClientRectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            Controls.Add(lisaNupp);
            Controls.Add(infoNupp);
            Resize += (s, e) =>
            {
                lisaNupp.Location = new Point(32, Height - 84);
                infoNupp.Location = new Point(lisaNupp.Right + 10, Height - 84);
            };
            Animatsioon.Tsykkel(this, s => { aeg = s; Invalidate(new Rectangle(Width - 300, 0, 300, Height)); }, 50);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Taust);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(0, 0, Width - 1, Height - 1);
            using var tee = Ymardus.Tee(r, 32);
            using (var b = new SolidBrush(Color.FromArgb(27, 28, 31))) g.FillPath(b, tee);
            var olek = g.Save();
            g.SetClip(tee);
            using (var b = new SolidBrush(Color.FromArgb(40, 226, 6, 19))) g.FillEllipse(b, r.Right - 280, -80, 360, 360);
            using (var b = new SolidBrush(Color.FromArgb(56, 160, 35, 110))) g.FillEllipse(b, r.Right - 320, r.Bottom - 120, 260, 260);
            g.Restore(olek);

            var h = Hinnad.Lopphind(toode);
            using var fm = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            var mr = new RectangleF(32, 30, TextRenderer.MeasureText(Keel.T("NÄDALA PAKKUMINE"), fm).Width + 34, 30);
            using (var tp = Ymardus.Tee(mr, 15)) using (var b = new SolidBrush(Color.FromArgb(26, 255, 255, 255))) g.FillPath(b, tp);
            using (var b = new SolidBrush(Color.FromArgb(240, 80, 90))) g.FillEllipse(b, mr.X + 13, mr.Y + 11, 8, 8);
            TextRenderer.DrawText(g, Keel.T("NÄDALA PAKKUMINE"), fm, Rectangle.Round(new RectangleF(mr.X + 26, mr.Y, mr.Width - 26, mr.Height)), Color.White, TextFormatFlags.VerticalCenter);

            int tekstiLaius = Width - 330;
            using (var f = new Font("Segoe UI", 26F, FontStyle.Bold))
                TextRenderer.DrawText(g, toode.Toodenimetus, f, new Rectangle(28, 70, tekstiLaius, 100), Color.White, TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
            using (var f = new Font("Segoe UI", 10.5F))
                TextRenderer.DrawText(g, string.Join(" · ", new[] { toode.Kaubamark, toode.Pakend }.Where(x => !string.IsNullOrEmpty(x))), f, new Point(32, 172), Color.FromArgb(185, 189, 196));
            using (var f = new Font("Segoe UI", 34F, FontStyle.Bold))
            {
                string hind = Ostukorv.Eur(h.Hind);
                TextRenderer.DrawText(g, hind, f, new Point(26, 196), Color.White);
                int hw = TextRenderer.MeasureText(hind, f).Width;
                if (h.Soodus)
                {
                    var kr = new RectangleF(26 + hw + 8, 208, 62, 26);
                    using (var tp = Ymardus.Tee(kr, 13)) using (var b = new SolidBrush(Color.FromArgb(226, 6, 19))) g.FillPath(b, tp);
                    using var fk = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                    TextRenderer.DrawText(g, $"−{h.Protsent}%", fk, Rectangle.Round(kr), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    using var fv = new Font("Segoe UI", 10.5F, FontStyle.Strikeout);
                    TextRenderer.DrawText(g, Ostukorv.Eur(h.TavaHind), fv, new Point((int)kr.X + 2, 238), Color.FromArgb(185, 189, 196));
                }
            }

            // toode valges ringis, hõljub
            float d = Math.Min(230, Height - 80), cx = Width - 160, cy = Height / 2f - 10 + (float)Math.Sin(aeg * 0.9) * 8;
            var ring = new RectangleF(cx - d / 2, cy - d / 2, d, d);
            for (int i = 0; i < 6; i++)
                using (var vari = new SolidBrush(Color.FromArgb(16, 0, 0, 0)))
                    g.FillEllipse(vari, RectangleF.Inflate(new RectangleF(ring.X, ring.Y + 18, ring.Width, ring.Height), 6 - i, 6 - i));
            using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, ring);
            if (PildiVahemalu.Too(toode.Pilt) is Image pilt)
                PildiVahemalu.Joonista(g, pilt, RectangleF.Inflate(ring, -d * 0.16f, -d * 0.16f));
        }
    }

    // Tasuta kojuveo rõngas: mitu % on täis, "veel 16,44 €"
    public class TarneRong : Control
    {
        private double t = 1;

        public TarneRong()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Ostukorv.Muutus += Muutus;
            Animatsioon.Kaivita(this, 1200, x => { t = Animatsioon.Sujuv(x); Invalidate(); });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Ostukorv.Muutus -= Muutus;
            base.Dispose(disposing);
        }

        private void Muutus() { if (!IsDisposed) Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Taust);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 28))
            using (var b = new SolidBrush(Teema.Pind))
                g.FillPath(b, tee);
            float osa = (float)Math.Min(1m, Ostukorv.Kokku / Tarne.TasutaAlates);
            var ring = new RectangleF(24, (Height - 84) / 2f, 84, 84);
            using (var p = new Pen(Teema.Paneel2, 10)) g.DrawEllipse(p, RectangleF.Inflate(ring, -5, -5));
            if (osa * t > 0.005)
                using (var p = new Pen(Teema.Edu, 10) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawArc(p, RectangleF.Inflate(ring, -5, -5), -90, 360 * osa * (float)t);
            using (var f = new Font("Segoe UI", 12.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, $"{(int)(osa * 100)}%", f, Rectangle.Round(ring), Teema.Tekst, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            decimal puudu = Tarne.Puudu(Ostukorv.Kokku);
            int x = (int)ring.Right + 18, y = Height / 2 - 34;
            using (var f = new Font("Segoe UI Semibold", 9.5F)) TextRenderer.DrawText(g, Keel.T(puudu > 0 ? "Tasuta kojuveoni" : "Kojuvedu on tasuta"), f, new Point(x, y), Teema.Edu);
            using (var f = new Font("Segoe UI", 16F, FontStyle.Bold)) TextRenderer.DrawText(g, puudu > 0 ? Keel.F("veel {0}", Ostukorv.Eur(puudu)) : "✓", f, new Point(x - 2, y + 18), Teema.Tekst);
            using (var f = new Font("Segoe UI", 9.5F)) TextRenderer.DrawText(g, Keel.F("Tasuta alates {0}", Ostukorv.Eur(Tarne.TasutaAlates)), f, new Point(x, y + 52), Teema.TekstNork);
        }
    }
}
