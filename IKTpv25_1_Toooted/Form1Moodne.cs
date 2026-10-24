using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    // ===================================================================
    //  Moodne disain (Ctrl+Shift+M) peaaknale: vasakul tume menüü,
    //  keskel pealkiri + 3 KPI plaati + tabel, paremal "Toote andmed" / ostukorv.
    //  Designeri elemendid tõstetakse ümber koodis (Form1.Designer.cs ei muutu).
    // ===================================================================
    public partial class Form1
    {
        private bool moodne;
        private Label? moodneKokku;

        private void UuendaMoodneKokku()
        {
            if (moodneKokku != null && !moodneKokku.IsDisposed) moodneKokku.Text = Ostukorv.Eur(Ostukorv.Kokku + Ostukorv.PantKokku);
        }
        private MoodneMenuu? moodneMenuu;
        private Label? moodnePealkiri;
        private MoodneKpi? kpiTooteid, kpiVahe, kpiMyyk;
        private bool taasAvada;

        // Program: kui disain vahetati moodsa ja tavalise vahel, avatakse peaaken uuesti
        public bool TaasAvada => taasAvada;

        private void RakendaMoodne()
        {
            moodne = true;
            BackColor = Teema.Taust;
            if (WindowState == FormWindowState.Normal)
            {
                var ala = Screen.FromControl(this).WorkingArea;
                ClientSize = new Size(Math.Min(1400, ala.Width - 40), Math.Min(860, ala.Height - 40));
            }

            // Vana päis, menüü, vahelehed, lindid ja külgmenüü peitu
            if (MainMenuStrip != null) MainMenuStrip.Visible = false;
            foreach (var ss in Controls.OfType<StatusStrip>()) ss.Visible = false;
            epoePais.Visible = epoeRiba.Visible = false;
            pnlTabid.Visible = pnlTop.Visible = pnlKassaRibbon.Visible = pnlJoon.Visible = false;
            kulg.PeidaMoodsas = true;

            // ---- Vasak tume menüü ----
            moodneMenuu = new MoodneMenuu { Dock = DockStyle.Left, Width = 252 };
            moodneMenuu.Lisa("tooted", "", "Tooted", () => ValiVaheleht(tabToode), () => AktiivneKasutaja.OnOmanik);
            moodneMenuu.Lisa("kassa", "", "Kassa", () => ValiVaheleht(tabKassa));
            moodneMenuu.Lisa("kampaaniad", "", "Kampaaniad", AvaKampaaniad, () => AktiivneKasutaja.OnOmanik);
            moodneMenuu.Lisa("myyk", "", "Müügipaneel", () => AvaVorm(new MyygipaneelVorm()), () => AktiivneKasutaja.OnOmanik);
            moodneMenuu.Lisa("partiid", "", "Partiid", AvaPartiid, () => AktiivneKasutaja.OnOmanik);
            moodneMenuu.Lisa("kliendid", "", "Kliendid", () => AvaVorm(new KliendidVorm()), () => AktiivneKasutaja.OnOmanik);
            moodneMenuu.Lisa("ajalugu", "", "Ostude ajalugu", AvaAjalugu);
            moodneMenuu.Lisa("nimekiri", "", "Ostunimekiri", AvaNimekiri);
            moodneMenuu.Lisa("rohkem", "", "Rohkem…", NaitaRohkem);
            moodneMenuu.Pood = AvaPood;
            moodneMenuu.Profiil = NaitaProfiil;
            Controls.Add(moodneMenuu);

            // ---- Keskosa: pealkiri, KPI-d, tabel ----
            var kesk = new Panel { Dock = DockStyle.Fill, BackColor = Teema.Taust };
            var pais = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Teema.Taust };
            moodnePealkiri = new Label { Text = "Tooted", AutoSize = true, Location = new Point(4, 8), Font = new Font("Segoe UI", 24F, FontStyle.Bold), ForeColor = Teema.Tekst, BackColor = Teema.Taust };
            pais.Controls.Add(moodnePealkiri);
            var csv = UiStiil.LooNupp("Ekspordi CSV", (s, e) => CsvEksport.Tooted(this));
            csv.Font = new Font("Segoe UI Semibold", 9.5F);
            pais.Controls.Add(csv);
            pais.Resize += (s, e) => csv.Location = new Point(pais.Width - csv.Width - 4, 16);

            var kpiRida = new TableLayoutPanel { Dock = DockStyle.Top, Height = 104, ColumnCount = 3, BackColor = Teema.Taust, Padding = new Padding(0, 0, 0, 12) };
            for (int i = 0; i < 3; i++) kpiRida.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            kpiTooteid = new MoodneKpi("Tooteid", MoodneKpi.Liik.Valge) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
            kpiVahe = new MoodneKpi("Väike laoseis (alla 10 tk)", MoodneKpi.Liik.Hoiatus) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
            kpiMyyk = new MoodneKpi("Müük täna", MoodneKpi.Liik.Tume) { Dock = DockStyle.Fill, Margin = new Padding(0) };
            kpiRida.Controls.Add(kpiTooteid, 0, 0);
            kpiRida.Controls.Add(kpiVahe, 1, 0);
            kpiRida.Controls.Add(kpiMyyk, 2, 0);

            pnlSisu.Controls.Remove(kaartTabel);
            kaartTabel.Dock = DockStyle.Fill;
            kaartTabel.Radius = 16;
            kesk.Controls.Add(kaartTabel);
            kesk.Controls.Add(kpiRida);
            kesk.Controls.Add(pais);

            // ---- Paremal: toote andmed (Tooted) või ostukorv (Kassa) ----
            pnlSisu.Padding = new Padding(16);
            pnlVahe.Dock = DockStyle.Right;
            pnlVahe.Width = 16;
            kaartSisestus.Dock = DockStyle.Right;
            kaartSisestus.Width = 380;
            kaartSisestus.Radius = 16;
            pnlPilt.Location = new Point(18, 50);
            pnlPilt.Size = new Size(110, 110);
            tblVali.Location = new Point(18, 172);
            tblVali.Size = new Size(344, 176);
            tblVali.ColumnStyles[0].Width = 96;
            lblSisestusPealkiri.Font = new Font("Segoe UI", 15F, FontStyle.Bold);

            // Nupud (Uuenda, Lisa, Kustuta, Tooteinfo, pilt, kategooriad) kaardi alla
            // valge paneel käsitsi "voolava" paigutusega (FlowLayoutPanel jättis tausta joonistamata)
            var nupud = new RoundedPanel { Dock = DockStyle.Bottom, Height = 150, Radius = 0, FillColor = Teema.Pind, BorderColor = Teema.Pind };
            foreach (var rida in new[] { ridaToode, ridaPilt, ridaKategooria })
                foreach (Control c in rida.Controls.Cast<Control>().ToList())
                {
                    rida.Controls.Remove(c);
                    c.MinimumSize = new Size(0, 38);
                    nupud.Controls.Add(c);
                }
            bool paigutan = false;
            nupud.Layout += (s, e) => { if (paigutan) return; paigutan = true; MoodneUi.Voog(nupud, 16, 8); paigutan = false; };
            btnLisa.Text = "+ Lisa uus";
            kaartSisestus.Controls.Add(nupud);

            kaartKorv.Dock = DockStyle.Right;
            kaartKorv.Width = 380;
            kaartKorv.Radius = 14;
            kaartKorv.BorderColor = kaartKorv.FillColor;
            foreach (var sisemine in kaartKorv.Controls.OfType<RoundedPanel>()) sisemine.BorderColor = sisemine.FillColor;
            // Kassa all: "Kokku" + summa, suur punane "Maksta", väiksemad nupud ühel real
            var kassaNupud = new RoundedPanel { Dock = DockStyle.Bottom, Height = 172, Radius = 0, FillColor = Teema.Pind, BorderColor = Teema.Pind };
            var kokkuSilt = new Label { Text = "Kokku", AutoSize = true, Location = new Point(18, 12), Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Teema.Tekst, BackColor = Teema.Pind };
            moodneKokku = new Label { AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Teema.Tekst, BackColor = Teema.Pind };
            foreach (var rida in new[] { ridaKokku, ridaOst, ridaArve, ridaPood }) rida.Visible = false;
            foreach (var b in new[] { btnValin, btnOstan, btnMaksta, btnSaadaArve, btnPood }) b.Parent?.Controls.Remove(b);
            btnMaksta.AutoSize = false;
            btnMaksta.Bounds = new Rectangle(18, 54, 344, 52);
            btnMaksta.Peamine = true;
            btnMaksta.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnMaksta.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            int bx = 18;
            foreach (var b in new[] { btnValin, btnSaadaArve, btnPood })
            {
                b.AutoSize = false;
                b.Peamine = false;
                b.Font = new Font("Segoe UI Semibold", 9.5F);
                b.Size = new Size(TextRenderer.MeasureText(Keel.T(b.Text), b.Font).Width + 30, 40);
                b.Location = new Point(bx, 116);
                bx += b.Width + 8;
            }
            kassaNupud.Controls.AddRange(new Control[] { kokkuSilt, moodneKokku, btnMaksta, btnValin, btnSaadaArve, btnPood });
            foreach (Control c in kassaNupud.Controls) { c.Anchor = AnchorStyles.Top | AnchorStyles.Left; if (c is not RoundedButton) c.BackColor = Teema.Pind; }
            void PaigutaKassa()
            {
                int w = kassaNupud.ClientSize.Width - 36;
                moodneKokku.Bounds = new Rectangle(kassaNupud.ClientSize.Width - 18 - 220, 4, 220, 44);
                btnMaksta.Bounds = new Rectangle(18, 56, Math.Max(100, w), 52);
            }
            kassaNupud.Resize += (s, e) => PaigutaKassa();
            PaigutaKassa();
            kaartKorv.Controls.Add(kassaNupud);
            UuendaMoodneKokku();
            Ostukorv.Muutus += UuendaMoodneKokku;
            FormClosed += (s, e) => Ostukorv.Muutus -= UuendaMoodneKokku;

            pnlSisu.Controls.Add(kesk);
            kesk.BringToFront();   // Fill dokitakse viimasena

            UuendaMoodneKpi();
            Load += (s, e) => UuendaMoodneKpi();
        }

        // KPI-d: tooteid, väike laoseis, tänane müük
        private void UuendaMoodneKpi()
        {
            if (!moodne || kpiTooteid == null || kpiVahe == null || kpiMyyk == null) return;
            kpiTooteid.Naita(koikTooted.Count.ToString());
            int vahe = koikTooted.Count(t => t.Kogus < VaikeLaoseis);
            kpiVahe.Naita(Keel.F("{0} toodet", vahe));
            try
            {
                using var db = new ToodedContext();
                var tana = DateTime.Today;
                decimal myyk = db.Ostud.AsNoTracking().Where(o => !o.Tagastatud && o.Aeg >= tana).Select(o => o.Summa).AsEnumerable().Sum();
                kpiMyyk.Naita(Ostukorv.Eur(myyk));
            }
            catch { kpiMyyk.Naita("—"); }
        }

        // ---- Üks aken: vormid avanevad peaakna sisualal (vasak menüü jääb alles) ----
        private Panel? manuseAla;
        private readonly List<Form> manused = new();
        private bool viimaneTooted = true, sulgen;

        public bool VoibManustada => moodne && Visible;

        public void Manusta(Form vorm, Action? suletud)
        {
            if (manuseAla == null)
            {
                manuseAla = new Panel { Dock = DockStyle.Fill, BackColor = Teema.Taust, AutoScroll = true, Padding = new Padding(16) };
                Controls.Add(manuseAla);
            }
            // sama vorm uuesti (nt Kampaaniad kaks korda): vana suletakse, ei kuhju
            sulgen = true;
            foreach (var f in manused.Where(f => f.GetType() == vorm.GetType()).ToList()) f.Close();
            sulgen = false;
            foreach (var f in manused) f.Visible = false;
            manused.Add(vorm);
            manuseAla.Visible = true;
            manuseAla.BringToFront();
            Aken.Paigalda(vorm, manuseAla, () =>
            {
                manused.Remove(vorm);
                if (manused.Count > 0) manused[^1].Visible = true;
                else if (manuseAla != null) { manuseAla.Visible = false; if (!sulgen) MoodneVaheleht(viimaneTooted); }
                suletud?.Invoke();
            });
        }

        private void SulgeManused()
        {
            sulgen = true;
            foreach (var f in manused.ToList()) if (!f.IsDisposed) f.Close();
            sulgen = false;
        }

        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; if (Teema.Komposiit) cp.ExStyle |= 0x02000000; return cp; }
        }

        private void MoodneVaheleht(bool tooted)
        {
            if (!moodne) return;
            viimaneTooted = tooted;
            if (!sulgen && manused.Count > 0) SulgeManused();
            pnlTop.Visible = pnlKassaRibbon.Visible = false;
            if (moodnePealkiri != null) moodnePealkiri.Text = Keel.T(tooted ? "Tooted" : "Kassa");
            moodneMenuu?.Vali(tooted ? "tooted" : "kassa");
            UuendaMoodneKpi();
        }

        // Teema muutus: kui moodne disain lülitati sisse/välja, ehitatakse peaaken uuesti (kui dialooge pole lahti)
        private void KontrolliMoodneVahetust()
        {
            if (Teema.OnMoodne == moodne || taasAvada) return;
            taasAvada = true;
            void Proovi()
            {
                if (OwnedForms.Any(f => f.Visible && f.Modal) || Application.OpenForms.Cast<Form>().Any(f => f != this && f.Modal)) return;
                Activated -= ProoviSyndmus;
                BeginInvoke(new Action(Close));
            }
            void ProoviSyndmus(object? s, EventArgs e) => Proovi();
            Activated += ProoviSyndmus;
            if (ContainsFocus) Proovi();
        }

        private void NaitaRohkem()
        {
            var m = new ContextMenuStrip { ShowImageMargin = false, Font = new Font("Segoe UI", 10F) };
            void L(string t, Action a, bool omanik = false) { if (!omanik || AktiivneKasutaja.OnOmanik) m.Items.Add(Keel.T(t), null, (s, e) => a()); }
            L("Statistika...", () => AvaVorm(new StatistikaVorm()), true);
            L("Demoandmed...", AvaDemo, true);
            L("Õhtune soodustus...", () => AvaVorm(new OhtuSeadedVorm()), true);
            L("Kasutajad...", () => AvaVorm(new KasutajadVorm()), true);
            L("E-posti seaded...", () => AvaVorm(new EmailSeadedVorm()), true);
            L("Registreeri kasutaja...", () => AvaVorm(new RegistreerimineVorm(lubaOmanik: true)), true);
            m.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem V(string t, Action a, bool valitud) { var i = new ToolStripMenuItem(Keel.T(t), null, (s, e) => a()) { Checked = valitud }; m.Items.Add(i); return i; }
            V("Hele teema", () => Teema.Sea(false, Teema.AktsendiNimi), !Teema.Tume);
            V("Tume teema", () => Teema.Sea(true, Teema.AktsendiNimi), Teema.Tume);
            V("Moodne disain", () => Teema.SeaDisain(Disain.Moodne), Teema.OnMoodne);
            V("E-poe disain (Rimi)", () => Teema.SeaDisain(Disain.Rimi), Teema.Stiil == Disain.Rimi);
            V("Klassikaline disain", () => Teema.SeaDisain(Disain.Klassikaline), Teema.Stiil == Disain.Klassikaline);
            m.Items.Add(new ToolStripSeparator());
            foreach (var k in Enum.GetValues<KeeleKood>())
                V(Keel.Nimi(k), () => Keel.Sea(k), Keel.Praegu == k).Tag = k;
            MoodneMenyyJoonistaja.Rakenda(m);
            m.Closed += (s, e) => BeginInvoke(new Action(m.Dispose));
            m.Show(Cursor.Position);
        }

        private void NaitaProfiil()
        {
            var m = new ContextMenuStrip { ShowImageMargin = false, Font = new Font("Segoe UI", 10F) };
            m.Items.Add(Keel.T("Seaded..."), null, (s, e) => SeadedVorm.Ava(this));
            m.Items.Add(Keel.T("Muuda parooli..."), null, (s, e) => AvaVorm(new ParooliMuutmineVorm()));
            m.Items.Add(Keel.T("Vaheta kasutajat..."), null, (s, e) => VahetaKasutajat());
            m.Items.Add(new ToolStripSeparator());
            m.Items.Add(Keel.T("Välju"), null, (s, e) => Close());
            MoodneMenyyJoonistaja.Rakenda(m);
            m.Closed += (s, e) => BeginInvoke(new Action(m.Dispose));
            m.Show(Cursor.Position);
        }
    }

    // Tume vasak menüü (raadius 32): logo, punktid (valitud = punane "pill"), "Ava e-pood ↗", kasutaja
    public class MoodneMenuu : Control
    {
        private record Punkt(string Votti, string Ikoon, string Tekst, Action Tegevus, Func<bool>? Nahtav);
        private readonly List<Punkt> punktid = new();
        private string? valitud = "tooted";
        private int hiir = -1;
        private double t = 1;
        public Action? Pood { get; set; }
        public Action? Profiil { get; set; }

        private static readonly Color Tume = Color.FromArgb(27, 28, 31);

        public MoodneMenuu()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Padding = new Padding(16, 16, 0, 16);
            Animatsioon.Kaivita(this, 600, x => { t = Animatsioon.Sujuv(x); Invalidate(); });
            Keel.Muutus += Invalidate;
            Teema.Muutus += Invalidate;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { Keel.Muutus -= Invalidate; Teema.Muutus -= Invalidate; }
            base.Dispose(disposing);
        }

        public void Lisa(string votti, string ikoon, string tekst, Action tegevus, Func<bool>? nahtav = null) =>
            punktid.Add(new Punkt(votti, ikoon, tekst, tegevus, nahtav));

        public void Vali(string votti) { valitud = votti; Invalidate(); }

        private List<Punkt> Nahtavad => punktid.Where(p => p.Nahtav?.Invoke() ?? true).ToList();
        private RectangleF Ala => new(16, 16, Width - 16, Height - 32);
        private RectangleF PunktiKoht(int i) => new(Ala.X + 12, Ala.Y + 90 + i * 46, Ala.Width - 24, 42);
        private RectangleF PoeKoht => new(Ala.X + 14, Ala.Bottom - 132, Ala.Width - 28, 52);
        private RectangleF ProfiiliKoht => new(Ala.X + 14, Ala.Bottom - 68, Ala.Width - 28, 52);

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var n = Nahtavad;
            int uus = Enumerable.Range(0, n.Count).FirstOrDefault(i => PunktiKoht(i).Contains(e.Location), -1);
            if (PoeKoht.Contains(e.Location)) uus = 100;
            if (ProfiiliKoht.Contains(e.Location)) uus = 101;
            Cursor = uus >= 0 ? Cursors.Hand : Cursors.Default;
            if (uus != hiir) { hiir = uus; Invalidate(); }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) { hiir = -1; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (PoeKoht.Contains(e.Location)) { Pood?.Invoke(); return; }
            if (ProfiiliKoht.Contains(e.Location)) { Profiil?.Invoke(); return; }
            var n = Nahtavad;
            for (int i = 0; i < n.Count; i++)
                if (PunktiKoht(i).Contains(e.Location))
                {
                    if (n[i].Votti != "rohkem") valitud = n[i].Votti;   // ka avatud vorm (Kampaaniad jne) on märgitud
                    Invalidate();
                    n[i].Tegevus();
                }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Taust);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var ala = Ala;
            using (var tee = Ymardus.Tee(ala, 32))
            using (var b = new SolidBrush(Tume))
                g.FillPath(b, tee);

            // logo
            var ring = new RectangleF(ala.X + 22, ala.Y + 24, 40, 40);
            using (var b = new SolidBrush(Color.FromArgb(226, 6, 19))) g.FillEllipse(b, ring);
            Ikoonid.Joonista(g, Ikoonid.Ostukorv, Rectangle.Round(ring), Color.White, 12f);
            using (var f = new Font("Segoe UI", 12.5F, FontStyle.Bold)) TextRenderer.DrawText(g, "AKT", f, Point.Round(new PointF(ring.Right + 12, ring.Y + 1)), Color.White);
            using (var f = new Font("Segoe UI", 8.5F)) TextRenderer.DrawText(g, Keel.T("Haldus"), f, Point.Round(new PointF(ring.Right + 12, ring.Y + 22)), Color.FromArgb(150, 155, 162));

            using var fp = new Font("Segoe UI Semibold", 10.5F);
            var n = Nahtavad;
            for (int i = 0; i < n.Count; i++)
            {
                double s = Math.Clamp(t * 1.4 - i * 0.06, 0, 1);
                var r = PunktiKoht(i);
                r.X -= (float)((1 - s) * 20);
                bool v = n[i].Votti == valitud;
                if (v || hiir == i)
                    using (var tee = Ymardus.Tee(r, r.Height / 2))
                    using (var b = new SolidBrush(v ? Color.FromArgb(226, 6, 19) : Color.FromArgb(20, 255, 255, 255)))
                        g.FillPath(b, tee);
                int a = (int)(255 * s);
                Ikoonid.Joonista(g, n[i].Ikoon, new Rectangle((int)r.X + 14, (int)r.Y, 22, (int)r.Height), Color.FromArgb(a, v ? Color.White : Color.FromArgb(200, 203, 208)), 11f);
                TextRenderer.DrawText(g, Keel.T(n[i].Tekst), fp, new Rectangle((int)r.X + 46, (int)r.Y, (int)r.Width - 50, (int)r.Height),
                    Color.FromArgb(a, v ? Color.White : Color.FromArgb(220, 223, 228)), TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }

            // Ava e-pood ↗ (punane pill)
            var pr = PoeKoht;
            using (var tee = Ymardus.Tee(pr, pr.Height / 2))
            using (var b = new SolidBrush(hiir == 100 ? Ymardus.Segu(Color.FromArgb(226, 6, 19), Color.White, 0.1f) : Color.FromArgb(226, 6, 19)))
                g.FillPath(b, tee);
            using (var f = new Font("Segoe UI", 11F, FontStyle.Bold))
                TextRenderer.DrawText(g, Keel.T("Ava e-pood"), f, new Rectangle((int)pr.X + 22, (int)pr.Y, (int)pr.Width - 70, (int)pr.Height), Color.White, TextFormatFlags.VerticalCenter);
            var nr = new RectangleF(pr.Right - 44, pr.Y + 8, 36, 36);
            using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, nr);
            Ikoonid.Joonista(g, "", Rectangle.Round(nr), Color.FromArgb(226, 6, 19), 10f);

            // kasutaja
            var kr = ProfiiliKoht;
            if (hiir == 101)
                using (var tee = Ymardus.Tee(kr, kr.Height / 2))
                using (var b = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                    g.FillPath(b, tee);
            var av = new RectangleF(kr.X + 6, kr.Y + 6, 40, 40);
            using (var b = new SolidBrush(Color.FromArgb(160, 35, 110))) g.FillEllipse(b, av);
            using (var f = new Font("Segoe UI Semibold", 9.5F))
                TextRenderer.DrawText(g, Pere.Initsiaalid(AktiivneKasutaja.Kuvanimi), f, Rectangle.Round(av), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            using (var f = new Font("Segoe UI Semibold", 10F))
                TextRenderer.DrawText(g, AktiivneKasutaja.Nimi, f, Point.Round(new PointF(av.Right + 10, kr.Y + 7)), Color.White);
            using (var f = new Font("Segoe UI", 8.5F))
                TextRenderer.DrawText(g, Keel.T(AktiivneKasutaja.Roll), f, Point.Round(new PointF(av.Right + 10, kr.Y + 27)), Color.FromArgb(150, 155, 162));
        }
    }

    // KPI plaat: valge / hoiatus (vilkuv punkt) / tume
    public class MoodneKpi : Control
    {
        public enum Liik { Valge, Hoiatus, Tume }
        private readonly string silt;
        private readonly Liik liik;
        private string vaartus = "";
        private double t = 1, aeg;

        public MoodneKpi(string silt, Liik liik)
        {
            this.silt = silt;
            this.liik = liik;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            if (liik == Liik.Hoiatus) Animatsioon.Tsykkel(this, s => { aeg = s; Invalidate(); }, 60);
        }

        public void Naita(string uus)
        {
            if (uus == vaartus) return;
            vaartus = uus;
            Animatsioon.Kaivita(this, 500, x => { t = Animatsioon.Sujuv(x); Invalidate(); });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Taust);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color taust = liik switch { Liik.Tume => Color.FromArgb(27, 28, 31), Liik.Hoiatus => Color.FromArgb(255, 241, 214), _ => Teema.Pind };
            Color tekst = liik == Liik.Tume ? Color.White : liik == Liik.Hoiatus ? Color.FromArgb(27, 28, 31) : Teema.Tekst;
            Color norg = liik == Liik.Tume ? Color.FromArgb(170, 175, 182) : liik == Liik.Hoiatus ? Color.FromArgb(140, 80, 0) : Teema.TekstNork;
            var r = new RectangleF(0.5f, 0.5f, Width - 2, Height - 2);
            using (var tee = Ymardus.Tee(r, 24))
            using (var b = new SolidBrush(taust))
                g.FillPath(b, tee);
            int x = 20;
            if (liik == Liik.Hoiatus)
            {
                int a = (int)(255 * (0.35 + 0.65 * (0.5 + 0.5 * Math.Cos(aeg * Math.PI * 2 / 1.8))));
                using var b = new SolidBrush(Color.FromArgb(a, 205, 115, 0));
                g.FillEllipse(b, 20, 22, 9, 9);
                x = 36;
            }
            using var fs = new Font("Segoe UI Semibold", 9.5F);
            using var fv = new Font("Segoe UI", 22F, FontStyle.Bold);
            TextRenderer.DrawText(g, Keel.T(silt), fs, new Point(x, 16), norg);
            TextRenderer.DrawText(g, vaartus, fv, new Point(16, 36 + (int)((1 - t) * 10)), Color.FromArgb((int)(255 * t), tekst));
        }
    }
}
