using System.Drawing.Drawing2D;

namespace IKTpv25_1_Toooted
{
    // ===================================================================
    //  Külgmenüü: ümar "pill" ikoonidega (nagu kaasaegsetes e-poodides).
    //  Üleval poe ring, keskel ikoonid (aktiivne = tume ruut), märgid (korvis 3),
    //  all väljumine ja kasutaja ring initsiaalidega. Vihje hiire all.
    // ===================================================================
    public class KulgMenuu : Control
    {
        public class Punkt
        {
            public string Votti = "", Glyph = "", Tekst = "";
            public Action Tegevus = () => { };
            public Func<int>? Mark;
            public Func<bool>? Nahtav;
            public bool OnNahtav => Nahtav?.Invoke() ?? true;
            public int MargiVaartus;
            public RectangleF Koht;
        }

        private readonly List<Punkt> punktid = new();
        private readonly ToolTip vihje = new() { InitialDelay = 250 };
        private int hiir = -1;                 // mitmes punkt on hiire all (-2 = väljumine, -3 = kasutaja, -4 = logo)
        private RectangleF valjuKoht, kasutajaKoht, logoKoht;

        public string? Aktiivne { get; set; }
        public Action? Valju { get; set; }
        public Action? Profiil { get; set; }
        public Action? LogoKlikk { get; set; }
        public string ValjuTekst { get; set; } = "Logi välja";

        public const int MenuuLaius = 88;

        public KulgMenuu()
        {
            Width = MenuuLaius;
            Dock = DockStyle.Right;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            Ostukorv.Muutus += UuendaMargid;
            Ostunimekiri.Muutus += UuendaMargid;
            Teema.Muutus += TeemaMuutus;
            Visible = Teema.Umarad;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Ostukorv.Muutus -= UuendaMargid;
                Ostunimekiri.Muutus -= UuendaMargid;
                Teema.Muutus -= TeemaMuutus;
                vihje.Dispose();
            }
            base.Dispose(disposing);
        }

        private void TeemaMuutus()
        {
            Visible = Teema.Umarad;   // seadetes "Ümarad nurgad ja külgmenüü"
            Invalidate();
        }

        public void Lisa(string votti, string glyph, string tekst, Action tegevus, Func<int>? mark = null, Func<bool>? nahtav = null)
        {
            punktid.Add(new Punkt { Votti = votti, Glyph = glyph, Tekst = tekst, Tegevus = tegevus, Mark = mark, Nahtav = nahtav });
            UuendaMargid();
        }

        public void UuendaMargid()
        {
            foreach (var p in punktid)
                try { p.MargiVaartus = p.Mark?.Invoke() ?? 0; } catch { p.MargiVaartus = 0; }
            if (IsHandleCreated) BeginInvoke(new Action(Invalidate));
        }

        // ---- Hiir ----
        private int Leia(Point p)
        {
            if (valjuKoht.Contains(p) && Valju != null) return -2;
            if (kasutajaKoht.Contains(p) && Profiil != null) return -3;
            if (logoKoht.Contains(p) && LogoKlikk != null) return -4;
            for (int i = 0; i < punktid.Count; i++)
                if (punktid[i].OnNahtav && punktid[i].Koht.Contains(p)) return i;
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int uus = Leia(e.Location);
            if (uus == hiir) return;
            hiir = uus;
            Cursor = uus == -1 ? Cursors.Default : Cursors.Hand;
            string tekst = uus switch
            {
                >= 0 => Keel.T(punktid[uus].Tekst) + (punktid[uus].MargiVaartus > 0 ? $" ({punktid[uus].MargiVaartus})" : ""),
                -2 => Keel.T(ValjuTekst),
                -3 => AktiivneKasutaja.Kuvanimi + " · " + Keel.T("Seaded"),
                -4 => Arve.PoeNimi,
                _ => ""
            };
            vihje.SetToolTip(this, tekst);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hiir = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int i = Leia(e.Location);
            if (i >= 0) punktid[i].Tegevus();
            else if (i == -2) Valju?.Invoke();
            else if (i == -3) Profiil?.Invoke();
            else if (i == -4) LogoKlikk?.Invoke();
        }

        // ---- Joonistamine ----
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.Clear(Parent?.BackColor ?? Teema.Taust);
            if (Width < 40 || Height < 200) return;

            float w = Width - 22, x = 11;

            // Üleval poe ring (aktsentvärv)
            logoKoht = new RectangleF(x + (w - 58) / 2, 12, 58, 58);
            Vari(g, logoKoht, true);
            using (var b = new SolidBrush(Teema.Aktsent))
                g.FillEllipse(b, logoKoht);
            using (var f = new Font("Segoe UI Black", 13F, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(Arve.PoeNimi, f, b, RectangleF.Inflate(logoKoht, 0, -2), sf);
            }

            // Pill
            var pill = new RectangleF(x, logoKoht.Bottom + 14, w, Height - logoKoht.Bottom - 26);
            Vari(g, pill, false);
            using (var b = new SolidBrush(Teema.Pind))
            using (var tee = PillTee(pill))
                g.FillPath(b, tee);

            // Ikoonid (kui kõrgust napib, lähevad väiksemaks)
            float y = pill.Y + 16;
            int nahtavaid = Math.Max(1, punktid.Count(p => p.OnNahtav));
            float ruum = pill.Height - 16 - 130;
            float samm = Math.Clamp(ruum / nahtavaid, 30, 58);
            float ruut = Math.Min(Math.Min(48, w - 14), samm - 6);
            for (int i = 0; i < punktid.Count; i++)
            {
                var p = punktid[i];
                if (!p.OnNahtav) { p.Koht = RectangleF.Empty; continue; }
                p.Koht = new RectangleF(x + (w - ruut) / 2, y, ruut, ruut);
                bool aktiivne = p.Votti == Aktiivne;
                if (aktiivne || hiir == i)
                {
                    using var b = new SolidBrush(aktiivne ? (Teema.Tume ? Color.FromArgb(235, 236, 240) : Color.FromArgb(24, 24, 28)) : Teema.Paneel2);
                    using var t = PehmeRuut(p.Koht, 16);
                    g.FillPath(b, t);
                }
                var ikooniVarv = aktiivne ? (Teema.Tume ? Color.FromArgb(24, 24, 28) : Color.White) : Teema.Tekst2;
                Ikoonid.Joonista(g, p.Glyph, Rectangle.Round(p.Koht), ikooniVarv, 14f);

                if (p.MargiVaartus > 0)
                {
                    string m = p.MargiVaartus > 99 ? "99+" : p.MargiVaartus.ToString();
                    using var mf = new Font("Segoe UI", 7F, FontStyle.Bold);
                    var sz = g.MeasureString(m, mf);
                    var mr = new RectangleF(p.Koht.Right - 14, p.Koht.Y + 2, Math.Max(16, sz.Width + 4), 16);
                    using (var mb = new SolidBrush(Teema.Aktsent))
                    using (var mt = PillTee(mr))
                        g.FillPath(mb, mt);
                    using var tb = new SolidBrush(Color.White);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(m, mf, tb, mr, sf);
                }
                y += samm;
            }

            // All: kasutaja ring ja väljumine
            float ring = Math.Min(42, w - 18);
            kasutajaKoht = new RectangleF(x + (w - ring) / 2, pill.Bottom - ring - 16, ring, ring);
            valjuKoht = Valju != null ? new RectangleF(x + (w - 36) / 2, kasutajaKoht.Y - 50, 36, 36) : RectangleF.Empty;

            // Õhuke joon ikoonide ja alumise osa vahel
            float joonAlgus = y + 4, joonLopp = (Valju != null ? valjuKoht.Y : kasutajaKoht.Y) - 14;
            if (joonLopp - joonAlgus > 20)
                using (var pen = new Pen(Teema.Joon, 2) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawLine(pen, x + w / 2, joonAlgus, x + w / 2, joonLopp);

            if (Valju != null)
            {
                using (var b = new SolidBrush(hiir == -2 ? Teema.Viga : (Teema.Tume ? Color.FromArgb(70, 72, 78) : Color.FromArgb(28, 28, 32))))
                    g.FillEllipse(b, valjuKoht);
                Ikoonid.Joonista(g, "", Rectangle.Round(valjuKoht), Color.White, 10.5f);
            }

            using (var b = new SolidBrush(Teema.Teine))
                g.FillEllipse(b, kasutajaKoht);
            if (hiir == -3)
                using (var pen = new Pen(Teema.Aktsent, 2))
                    g.DrawEllipse(pen, RectangleF.Inflate(kasutajaKoht, 2, 2));
            using (var f = new Font("Segoe UI Semibold", 11F))
            using (var tb = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(Initsiaalid(), f, tb, kasutajaKoht, sf);
            }
        }

        private static string Initsiaalid()
        {
            var osad = AktiivneKasutaja.Kuvanimi.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (osad.Length == 0) return "?";
            string s = osad.Length == 1 ? osad[0][..Math.Min(2, osad[0].Length)] : $"{osad[0][0]}{osad[^1][0]}";
            return s.ToUpper();
        }

        private void Vari(Graphics g, RectangleF r, bool ring)
        {
            for (int i = 1; i <= 4; i++)
            {
                using var b = new SolidBrush(Color.FromArgb(Teema.Tume ? 26 : 9, Color.Black));
                var v = new RectangleF(r.X - i * 0.6f, r.Y + i * 1.2f, r.Width + i * 1.2f, r.Height + i * 0.8f);
                if (ring) g.FillEllipse(b, v);
                else { using var t = PillTee(v); g.FillPath(b, t); }
            }
        }

        // Täiesti ümarad otsad (raadius = pool laiust), sõltumata Ymardus seadest
        private static GraphicsPath PillTee(RectangleF r)
        {
            var p = new GraphicsPath();
            float d = Math.Min(r.Width, r.Height);
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        private static GraphicsPath PehmeRuut(RectangleF r, float raadius)
        {
            var p = new GraphicsPath();
            float d = Math.Min(raadius * 2, Math.Min(r.Width, r.Height));
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
