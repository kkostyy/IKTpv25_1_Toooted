using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    // Klassikalise disaini pood (esimene kujundus): otsing, kategooriate nupud, tootekaardid "+" nupuga
    // ja ostukorv paremal. Ctrl+Shift+D vahetab e-poe (Rimi) disaini vastu.
    public class PoodKlassikVorm : Form
    {
        private readonly Form1 kassa;
        private readonly FlowLayoutPanel vork;
        private readonly FlowLayoutPanel kategooriad;
        private readonly TextBox txtOtsi;
        private readonly Label lblTulemus, lblKokku, lblKorvis;
        private readonly RoundedButton btnVormista;

        private List<Toode> koikTooted = new();
        private int? valitudKategooria;

        public PoodKlassikVorm(Form1 kassa)
        {
            this.kassa = kassa;

            Text = Keel.F("Pood – {0}", Arve.PoeNimi);
            StartPosition = FormStartPosition.CenterParent;
            Font = UiStiil.Põhifont;
            BackColor = Teema.Taust;
            ClientSize = new Size(1200, 740);
            MinimumSize = new Size(960, 600);
            KeyPreview = true;
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            // ---- Paremal: ostukorv ----
            var korv = new Panel { Dock = DockStyle.Right, Width = 370, BackColor = Teema.Pind, Padding = new Padding(18, 16, 18, 16) };
            var pealkiri = new Label { Text = "Ostukorv", Dock = DockStyle.Top, Height = 34, Font = new Font("Segoe UI Semibold", 15F), ForeColor = Teema.Tekst, BackColor = Teema.Pind };
            lblKorvis = new Label { Dock = DockStyle.Top, Height = 26, ForeColor = Teema.TekstNork, BackColor = Teema.Pind };
            var korvVaade = new KorvVaade { Dock = DockStyle.Fill };

            var alumine = new Panel { Dock = DockStyle.Bottom, Height = 150, BackColor = Teema.Pind };
            var joon = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Teema.Joon };
            var kokkuRida = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = Teema.Pind };
            var kokkuSilt = new Label { Text = "Kokku", Dock = DockStyle.Left, Width = 100, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 11F), ForeColor = Teema.Tekst2, BackColor = Teema.Pind };
            lblKokku = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI Semibold", 20F), ForeColor = Teema.Tekst, BackColor = Teema.Pind };
            kokkuRida.Controls.Add(lblKokku);
            kokkuRida.Controls.Add(kokkuSilt);

            btnVormista = UiStiil.LooSuurNupp("Vormista ost  →", (s, e) => Vormista());
            btnVormista.Dock = DockStyle.Top;

            var vahe = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Teema.Pind };
            var lisaNupud = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, BackColor = Teema.Pind, WrapContents = false };
            var tyhjenda = (RoundedButton)UiStiil.LooNupp("Tühjenda korv", (s, e) =>
            {
                if (Ostukorv.Ridad.Count > 0 &&
                    Teade.Show("Kas tühjendan ostukorvi?", "Ostukorv", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    Ostukorv.Tyhjenda();
            });
            tyhjenda.Ohtlik = true;
            lisaNupud.Controls.Add(tyhjenda);
            lisaNupud.Controls.Add(UiStiil.LooNupp("Viimane tšekk e-postile", (s, e) => kassa.SaadaArve()));

            alumine.Controls.Add(lisaNupud);
            alumine.Controls.Add(vahe);
            alumine.Controls.Add(btnVormista);
            alumine.Controls.Add(kokkuRida);
            alumine.Controls.Add(joon);

            korv.Controls.Add(korvVaade);
            korv.Controls.Add(alumine);
            korv.Controls.Add(lblKorvis);
            korv.Controls.Add(pealkiri);

            // ---- Ülal: otsing + kategooriad ----
            var ylaRiba = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(16, 16, 16, 4),
                BackColor = Teema.Taust,
                WrapContents = true
            };
            var otsing = UiStiil.LooSisend(out txtOtsi, 300, false, "Otsi toodet...  (Ctrl+F)");
            otsing.Margin = new Padding(0, 0, 16, 6);
            txtOtsi.TextChanged += (s, e) => NaitaTooted();
            txtOtsi.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { txtOtsi.Clear(); e.SuppressKeyPress = true; } };

            kategooriad = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = Teema.Taust, WrapContents = true, Margin = new Padding(0) };
            ylaRiba.Controls.Add(otsing);
            var nimekiriNupp = UiStiil.LooNupp("Ostunimekiri", (s, e) => { using var v = new OstunimekiriVorm(); v.ShowDialog(this); });
            nimekiriNupp.Margin = new Padding(0, 0, 8, 6);
            var seadedNupp = UiStiil.LooNupp("Seaded", (s, e) => SeadedVorm.Ava(this));
            seadedNupp.Margin = new Padding(0, 0, 16, 6);
            ylaRiba.Controls.Add(nimekiriNupp);
            ylaRiba.Controls.Add(seadedNupp);
            ylaRiba.Controls.Add(kategooriad);

            lblTulemus = new Label { Dock = DockStyle.Top, Height = 26, Padding = new Padding(22, 4, 0, 0), ForeColor = Teema.TekstNork, BackColor = Teema.Taust };

            // ---- Tootekaardid ----
            vork = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12, 2, 12, 12), BackColor = Teema.Taust };

            Controls.Add(vork);
            Controls.Add(lblTulemus);
            Controls.Add(ylaRiba);
            Controls.Add(korv);
            vork.BringToFront();

            Ostukorv.Muutus += KorvMuutus;
            PildiVahemalu.Laetud += PildidLaetud;
            Teema.Muutus += DisainMuutus;
            Load += (s, e) => { LaadiTooted(); UuendaKokku(); txtOtsi.Focus(); };
        }

        protected override void OnLoad(EventArgs e)
        {
            Teema.Rakenda(this);
            base.OnLoad(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F) { txtOtsi.Focus(); txtOtsi.SelectAll(); e.Handled = true; }
            base.OnKeyDown(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Ostukorv.Muutus -= KorvMuutus;
            PildiVahemalu.Laetud -= PildidLaetud;
            Teema.Muutus -= DisainMuutus;
            base.OnFormClosed(e);
        }

        // Ctrl+Shift+D: pood suletakse ja Form1 avab teise disaini poe
        private void DisainMuutus()
        {
            if (Teema.OnRimi && IsHandleCreated)
            {
                Tag = "disain";
                BeginInvoke(new Action(Close));
            }
            else
            {
                Text = Keel.F("Pood – {0}", Arve.PoeNimi);
                LaadiTooted();
                UuendaKokku();
                Teema.Rakenda(this);
            }
        }

        private void PildidLaetud()
        {
            if (IsHandleCreated && !IsDisposed)
                BeginInvoke(new Action(() => { foreach (Control c in vork.Controls) c.Invalidate(); }));
        }

        private void KorvMuutus()
        {
            UuendaKokku();
            foreach (Control c in vork.Controls) c.Invalidate();
        }

        private void UuendaKokku()
        {
            lblKokku.Text = Ostukorv.Eur(Ostukorv.Kokku);
            int tk = Ostukorv.Kogus;
            lblKorvis.Text = tk == 0 ? Keel.T("Korv on tühi") : Keel.F("{0} toodet, {1} rida", tk, Ostukorv.Ridad.Count);
            btnVormista.Enabled = Ostukorv.Ridad.Count > 0;
        }

        private void LaadiTooted()
        {
            using var db = new ToodedContext();
            koikTooted = db.Toodetabel.Include(t => t.Kategooria).AsNoTracking().ToList()
                .OrderBy(t => t.KategooriaId).ThenBy(t => t.Toodenimetus).ToList();
            var kat = db.Kategooriatabel.AsNoTracking().ToList().OrderBy(k => k.Id).ToList();

            foreach (Control c in kategooriad.Controls.Cast<Control>().ToList()) c.Dispose();
            kategooriad.Controls.Add(KategooriaNupp("Kõik", null, koikTooted.Count));
            foreach (var k in kat)
                kategooriad.Controls.Add(KategooriaNupp(k.Kategooria_nimetus, k.Id, koikTooted.Count(t => t.KategooriaId == k.Id)));
            NaitaTooted();
        }

        private RoundedButton KategooriaNupp(string nimi, int? id, int arv)
        {
            var nupp = (RoundedButton)UiStiil.LooNupp($"{Keel.T(nimi)}  {arv}", null, valitudKategooria == id);
            nupp.Radius = 16;
            nupp.MinimumSize = new Size(0, 34);
            nupp.Tag = id;
            nupp.Click += (s, e) =>
            {
                valitudKategooria = id;
                foreach (RoundedButton b in kategooriad.Controls.OfType<RoundedButton>())
                {
                    b.Peamine = Equals(b.Tag, id);
                    b.Invalidate();
                }
                NaitaTooted();
            };
            return nupp;
        }

        private void NaitaTooted()
        {
            string otsi = txtOtsi.Text.Trim();
            var nahtavad = koikTooted
                .Where(t => valitudKategooria == null || t.KategooriaId == valitudKategooria)
                .Where(t => otsi.Length == 0 || t.Toodenimetus.Contains(otsi, StringComparison.CurrentCultureIgnoreCase)
                            || (t.Kaubamark ?? "").Contains(otsi, StringComparison.CurrentCultureIgnoreCase))
                .ToList();

            vork.SuspendLayout();
            foreach (Control c in vork.Controls.Cast<Control>().ToList()) c.Dispose();
            foreach (var t in nahtavad)
            {
                var kaart = new KlassikKaart(t);
                kaart.Lisa += LisaKorvi;
                kaart.Ava += AvaToode;
                vork.Controls.Add(kaart);
            }
            if (nahtavad.Count == 0)
                vork.Controls.Add(new Label
                {
                    Text = otsi.Length > 0 ? Keel.F("„{0}“ – midagi ei leitud.", otsi) : Keel.T("Selles kategoorias pole tooteid."),
                    AutoSize = true,
                    Margin = new Padding(12),
                    ForeColor = Teema.TekstNork,
                    Font = new Font("Segoe UI", 11F)
                });
            vork.ResumeLayout();
            lblTulemus.Text = Keel.F("{0} toodet", nahtavad.Count);
        }

        private static void LisaKorvi(Toode t)
        {
            if (t.Kogus <= 0) { Teade.Toast(Keel.F("„{0}“ on otsas.", t.Toodenimetus), TeateTyyp.Hoiatus); return; }
            string? viga = Ostukorv.Lisa(t.Id, t.Toodenimetus, t.Hind, t.Kogus, 1, t.Pilt);
            if (viga != null) Teade.Toast(viga, TeateTyyp.Hoiatus);
            else Teade.Toast(Keel.F("{0} lisati korvi", t.Toodenimetus), TeateTyyp.Edu, 1400);
        }

        private void AvaToode(Toode t)
        {
            using var vorm = new ToodeVorm(t, koikTooted);
            vorm.ShowDialog(this);
            if (vorm.OtsiKaubamarki != null) txtOtsi.Text = vorm.OtsiKaubamarki;
        }

        private void Vormista()
        {
            if (kassa.Vormista()) LaadiTooted();
        }
    }

    // Klassikaline tootekaart: vari, pilt, märgid, hind ja "+" nupp (klõps "+" lisab, mujal avab toote)
    public class KlassikKaart : Control
    {
        public Toode Toode { get; }
        public event Action<Toode>? Lisa;
        public event Action<Toode>? Ava;
        private bool hover;

        public KlassikKaart(Toode toode)
        {
            Toode = toode;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(196, 276);
            Margin = new Padding(4);
            Cursor = Cursors.Hand;
        }

        private RectangleF Kaart => new(6, hover && Toode.Kogus > 0 ? 3 : 5, Width - 13, Height - 16);
        private RectangleF Pluss => new(Kaart.Right - 50, Kaart.Bottom - 50, 38, 38);

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (e.Button != MouseButtons.Left) return;
            if (Pluss.Contains(e.Location)) Lisa?.Invoke(Toode);
            else Ava?.Invoke(Toode);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent?.BackColor ?? Teema.Taust);

            bool laos = Toode.Kogus > 0;
            bool aktiivne = hover && laos;
            var kaart = Kaart;

            int varjuAlfa = Teema.Tume ? 34 : 12;
            for (int i = 1; i <= 5; i++)
            {
                var v = kaart;
                v.Offset(0, i * (aktiivne ? 1.2f : 0.8f));
                using var vp = Ymardus.Tee(v, 14);
                using var vb = new SolidBrush(Color.FromArgb(varjuAlfa, Color.Black));
                g.FillPath(vb, vp);
            }

            using (var path = Ymardus.Tee(kaart, 14))
            using (var b = new SolidBrush(Teema.Pind))
            using (var p = new Pen(aktiivne ? Teema.Aktsent : Teema.Joon, aktiivne ? 1.6f : 1f))
            {
                g.FillPath(b, path);
                g.DrawPath(p, path);
            }

            var pildiKast = new RectangleF(kaart.X + 10, kaart.Y + 10, kaart.Width - 20, 132);
            using (var pp = Ymardus.Tee(pildiKast, 10))
            using (var pb = new SolidBrush(Teema.PildiTaust))
                g.FillPath(pb, pp);
            if (PildiVahemalu.Too(Toode.Pilt) is Image pilt)
                PildiVahemalu.Joonista(g, pilt, RectangleF.Inflate(pildiKast, -10, -10));
            else
                Ikoonid.JoonistaEmoji(g, Ikoonid.KategooriaIkoon(Toode.Kategooria?.Kategooria_nimetus), Rectangle.Round(pildiKast), Teema.Joon, 34f);

            int korvis = Ostukorv.KorvisKogus(Toode.Id);
            if (korvis > 0) Mark(g, Keel.F("Korvis: {0}", korvis), Teema.Aktsent, pildiKast.X + 6, pildiKast.Y + 6, false, 0);
            if (!laos) Mark(g, Keel.T("Otsas"), Teema.Viga, 0, pildiKast.Y + 6, true, pildiKast.Right - 6);
            else if (Toode.SoodusProtsent is int pr) Mark(g, $"-{pr}%", Teema.Viga, 0, pildiKast.Y + 6, true, pildiKast.Right - 6);
            else if (Toode.Kogus < Form1.VaikeLaoseis) Mark(g, Keel.F("Vähe: {0}", Toode.Kogus), Teema.Hoiatus, 0, pildiKast.Y + 6, true, pildiKast.Right - 6);

            int x = (int)kaart.X + 12, w = (int)kaart.Width - 24;
            int y = (int)pildiKast.Bottom + 8;
            using (var vaike = new Font("Segoe UI", 8F))
                TextRenderer.DrawText(g, Keel.T(Toode.Kategooria?.Kategooria_nimetus ?? "").ToUpper(), vaike, new Rectangle(x, y, w, 16), Teema.TekstNork,
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            using (var nimi = new Font("Segoe UI Semibold", 10F))
                TextRenderer.DrawText(g, Toode.Toodenimetus, nimi, new Rectangle(x, y + 16, w, 40), Teema.Tekst,
                    TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            int all = (int)kaart.Bottom;
            using (var hind = new Font("Segoe UI Semibold", 15F))
                TextRenderer.DrawText(g, Ostukorv.Eur(Toode.Hind), hind, new Rectangle(x, all - 54, w - 44, 28),
                    laos ? Teema.Tekst : Teema.TekstNork, TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
            using (var v2 = new Font("Segoe UI", 8.25F))
                TextRenderer.DrawText(g, laos ? Keel.F("Laos {0} tk", Toode.Kogus) : Keel.T("Pole laos"), v2, new Rectangle(x, all - 26, w - 44, 18),
                    laos && Toode.Kogus < Form1.VaikeLaoseis ? Teema.Hoiatus : Teema.TekstNork, TextFormatFlags.NoPrefix);

            var pluss = Pluss;
            using (var pb2 = new SolidBrush(!laos ? Teema.Paneel2 : aktiivne ? Teema.AktsentTume : Teema.Aktsent))
                g.FillEllipse(pb2, pluss);
            using (var pf = new Font("Segoe UI", 16F, FontStyle.Bold))
                TextRenderer.DrawText(g, "+", pf, Rectangle.Round(pluss), laos ? Color.White : Teema.TekstNork,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            if (!laos)
            {
                using var tuhm = new SolidBrush(Color.FromArgb(120, Teema.Pind));
                using var tp = Ymardus.Tee(kaart, 14);
                g.FillPath(tuhm, tp);
            }
        }

        private static void Mark(Graphics g, string tekst, Color varv, float x, float y, bool paremale, float parem)
        {
            using var f = new Font("Segoe UI Semibold", 8F);
            var suurus = TextRenderer.MeasureText(tekst, f);
            float w = suurus.Width + 10;
            if (paremale) x = parem - w;
            var r = new RectangleF(x, y, w, 20);
            using (var path = Ymardus.Tee(r, 10))
            using (var b = new SolidBrush(varv))
                g.FillPath(b, path);
            TextRenderer.DrawText(g, tekst, f, Rectangle.Round(r), Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
