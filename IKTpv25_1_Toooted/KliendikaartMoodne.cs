using System.Drawing.Drawing2D;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    // Kasutaja valitud kaardi kujundus ja oma foto (fail kaart.json)
    public static class KaardiKujundus
    {
        public static readonly string[] Nimed = { "Tase", "Punane", "Ploom", "Valge ID", "Minu foto" };

        private sealed class Andmed { public Dictionary<int, int> Valik { get; set; } = new(); }
        private static string Fail => Path.Combine(Kaustad.Juur, "kaart.json");
        public static string FotoFail(int kasutajaId) => Path.Combine(Kaustad.Juur, $"kaardifoto_{kasutajaId}.png");

        private static Andmed Lae()
        {
            try { return File.Exists(Fail) ? JsonSerializer.Deserialize<Andmed>(File.ReadAllText(Fail)) ?? new() : new(); }
            catch { return new(); }
        }

        public static int Valitud(int kasutajaId) => Lae().Valik.TryGetValue(kasutajaId, out int v) ? v : 0;

        public static void Vali(int kasutajaId, int disain)
        {
            var a = Lae();
            a.Valik[kasutajaId] = disain;
            try { Directory.CreateDirectory(Kaustad.Juur); File.WriteAllText(Fail, JsonSerializer.Serialize(a)); } catch { }
        }

        public static Image? Foto(int kasutajaId)
        {
            try
            {
                string f = FotoFail(kasutajaId);
                if (!File.Exists(f)) return null;
                using var s = new MemoryStream(File.ReadAllBytes(f));   // fail ei jää lukku
                return Image.FromStream(s);
            }
            catch { return null; }
        }

        // Kaardi esikülg: taust kujunduse järgi, "joonistuvad" ümarad jooned, AKT POOD KLIENT,
        // kiip + kontaktivaba ikoon, nimi, number ja boonus
        public static void Joonista(Graphics g, RectangleF r, int disain, TaseInfo tase, Image? foto, string nimi, string number, decimal boonus, double aeg, double joon)
        {
            using var tee = Ymardus.Tee(r, r.Height * 0.07f);
            Color tint = Color.White, dot1 = Color.White, dot2 = Color.FromArgb(160, 35, 110);
            Color line1 = Color.FromArgb(190, 255, 255, 255), line2 = Color.FromArgb(150, 255, 200, 215);
            switch (disain)
            {
                case 0:
                    TaseKaart.JoonistaTaust(g, r, tase.Tase, aeg, 0.5f);
                    dot1 = Color.FromArgb(226, 6, 19);
                    break;
                case 1:
                    using (var b = new SolidBrush(Color.FromArgb(226, 6, 19))) g.FillPath(b, tee);
                    break;
                case 2:
                    using (var b = new LinearGradientBrush(r, Color.FromArgb(160, 35, 110), Color.FromArgb(110, 20, 80), 60f)) g.FillPath(b, tee);
                    line1 = Color.FromArgb(215, 246, 179, 214); line2 = Color.FromArgb(230, 226, 6, 19); dot1 = Color.FromArgb(226, 6, 19); dot2 = Color.FromArgb(246, 179, 214);
                    break;
                case 3:
                    using (var b = new SolidBrush(Color.White)) g.FillPath(b, tee);
                    using (var p = new Pen(Color.FromArgb(221, 226, 229))) g.DrawPath(p, tee);
                    tint = Color.FromArgb(27, 28, 31); line1 = Color.FromArgb(140, 226, 6, 19); line2 = Color.FromArgb(90, 160, 35, 110); dot1 = Color.FromArgb(226, 6, 19);
                    break;
                default:
                    using (var b = new SolidBrush(Color.FromArgb(91, 18, 64))) g.FillPath(b, tee);
                    if (foto != null)
                    {
                        var olek0 = g.Save();
                        g.SetClip(tee);
                        float k = Math.Max(r.Width / foto.Width, r.Height / foto.Height);
                        float w = foto.Width * k, h = foto.Height * k;
                        g.DrawImage(foto, r.X + (r.Width - w) / 2, r.Y + (r.Height - h) / 2, w, h);
                        using (var var = new LinearGradientBrush(r, Color.FromArgb(90, 27, 28, 31), Color.FromArgb(215, 91, 18, 64), 90f)) g.FillRectangle(var, r);
                        g.Restore(olek0);
                    }
                    line1 = Color.FromArgb(90, 255, 255, 255); line2 = Color.FromArgb(150, 226, 6, 19); dot1 = Color.FromArgb(226, 6, 19);
                    break;
            }

            var olek = g.Save();
            g.SetClip(tee);
            // ümarad "joonistuvad" ristkülikud paremas alanurgas
            float s = r.Width / 440f;
            foreach (var (x, y, w, h, rad, c) in new[] { (250f, 120f, 210f, 150f, 26f, line2), (280f, 146f, 150f, 104f, 20f, line1) })
            {
                var o = g.Save();
                g.TranslateTransform(r.X + 355 * s, r.Y + 195 * s);
                g.RotateTransform(-24);
                g.TranslateTransform(-(r.X + 355 * s), -(r.Y + 195 * s));
                using var rr = Ymardus.Tee(new RectangleF(r.X + x * s, r.Y + y * s, w * s, h * s), rad * s);
                using var p = new Pen(c, 1.4f) { DashStyle = DashStyle.Custom, DashPattern = new[] { (float)Math.Max(0.01, joon * 600), 600f } };
                g.DrawPath(p, rr);
                g.Restore(o);
            }
            g.Restore(olek);

            using var fh = new Font("Segoe UI", 9F * s, FontStyle.Bold);
            TextRenderer.DrawText(g, "A K T      P O O D      K L I E N T", fh, Point.Round(new PointF(r.X + 24 * s, r.Y + 22 * s)), tint);
            using (var b = new SolidBrush(dot1)) g.FillEllipse(b, r.Right - 58 * s, r.Y + 20 * s, 22 * s, 22 * s);
            using (var b = new SolidBrush(dot2)) g.FillEllipse(b, r.Right - 45 * s, r.Y + 20 * s, 22 * s, 22 * s);

            // kiip + kontaktivaba
            var kiip = new RectangleF(r.X + 24 * s, r.Y + 64 * s, 44 * s, 34 * s);
            using (var tp = Ymardus.Tee(kiip, 7 * s)) using (var p = new Pen(Color.FromArgb(140, tint))) g.DrawPath(p, tp);
            using (var b = new SolidBrush(tint))
                foreach (var (dx, dy) in new[] { (10f, 8f), (31f, 8f), (10f, 23f), (31f, 23f) }) g.FillEllipse(b, kiip.X + dx * s, kiip.Y + dy * s, 3 * s, 3 * s);
            using (var p = new Pen(tint, 1.8f * s) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                for (int i = 0; i < 3; i++) g.DrawArc(p, kiip.Right + 10 * s + i * 5 * s, kiip.Y + (8 - i * 4) * s, (8 + i * 8) * s, (18 + i * 8) * s, -50, 100);

            if (disain == 3)   // vertikaalne triipkood paremal
            {
                var rnd = new Random(number.GetHashCode());
                float yy = r.Y + 70 * s;
                using var b = new SolidBrush(Color.FromArgb(27, 28, 31));
                while (yy < r.Y + 170 * s) { float t = (1 + rnd.Next(3)) * s; g.FillRectangle(b, r.Right - 48 * s, yy, 26 * s, t); yy += t + (1 + rnd.Next(3)) * s; }
            }

            using var fn = new Font("Segoe UI", 9F * s, FontStyle.Bold);
            using var fnr = new Font("Segoe UI", 16F * s);
            using var fbs = new Font("Segoe UI", 7.5F * s, FontStyle.Bold);
            using var fb = new Font("Segoe UI", 20F * s, FontStyle.Bold);
            TextRenderer.DrawText(g, nimi.ToUpper(), fn, Point.Round(new PointF(r.X + 24 * s, r.Bottom - 70 * s)), tint);
            TextRenderer.DrawText(g, string.Join(" ", number.ToCharArray()), fnr, Point.Round(new PointF(r.X + 22 * s, r.Bottom - 50 * s)), tint);
            string bs = Ostukorv.Eur(boonus);
            int bw = TextRenderer.MeasureText(bs, fb).Width;
            TextRenderer.DrawText(g, Keel.T("BOONUS"), fbs, new Rectangle((int)(r.Right - 24 * s - 120), (int)(r.Bottom - 70 * s), 120, 16), Color.FromArgb(200, tint), TextFormatFlags.Right);
            TextRenderer.DrawText(g, bs, fb, new Point((int)(r.Right - 20 * s - bw), (int)(r.Bottom - 54 * s)), tint);
        }
    }

    // ===================================================================
    //  Kliendikaart moodsas disainis (lõuend 2.3): suur kaart, 5 kujundust
    //  horisontaalselt, oma foto üleslaadimine; paremal vöötkood, boonus,
    //  tase, viimased ostud ja nupud.
    // ===================================================================
    public class KliendikaartMoodneVorm : Form, ITagasi
    {
        private readonly SuurKaart kaart;
        private readonly List<KujunduseValik> pisipildid = new();
        private readonly Label lblValik;

        public KliendikaartMoodneVorm()
        {
            Text = Keel.T("Kliendikaart");
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            KeyPreview = true;
            Font = UiStiil.Põhifont;
            BackColor = Teema.Taust;
            ClientSize = new Size(1000, 720);
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            AktiivneKasutaja.Uuenda();
            var tase = Preemiad.TaseSummast(0);
            var ostud = new List<Ost>();
            try
            {
                using var db = new ToodedContext();
                if (AktiivneKasutaja.KlientId is int kid)
                {
                    tase = Preemiad.LeiaTase(db, kid);
                    ostud = db.Ostud.AsNoTracking().Where(o => o.KlientId == kid && !o.Tagastatud).OrderByDescending(o => o.Aeg).Take(3).ToList();
                }
            }
            catch { }
            int kasutaja = AktiivneKasutaja.Id;

            // ---- Vasak valge kaart ----
            var vasak = new RoundedPanel { Bounds = new Rectangle(16, 16, 688, 688), Radius = 16, FillColor = Teema.Pind, BorderColor = Teema.Pind, BackColor = Teema.Taust };
            var tagasi = new TagasiNupp { Location = new Point(28, 26) };
            tagasi.Click += (s, e) => Close();
            vasak.Controls.Add(tagasi);
            vasak.Controls.Add(new Label { Text = "Kliendikaart", Location = new Point(tagasi.Right + 14, 32), AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Teema.Tekst, BackColor = Teema.Pind });
            lblValik = new Label { AutoSize = true, Font = new Font("Segoe UI Semibold", 10F), ForeColor = Color.FromArgb(160, 35, 110), BackColor = Teema.Pind };
            vasak.Controls.Add(lblValik);

            kaart = new SuurKaart(tase, AktiivneKasutaja.Kuvanimi, AktiivneKasutaja.Kaardinumber ?? "", AktiivneKasutaja.Boonus, KaardiKujundus.Foto(kasutaja))
            {
                Bounds = new Rectangle(28, 92, 632, 380),
                Disain = KaardiKujundus.Valitud(kasutaja)
            };
            vasak.Controls.Add(kaart);

            for (int i = 0; i < KaardiKujundus.Nimed.Length; i++)
            {
                int n = i;
                var p = new KujunduseValik(n, tase, KaardiKujundus.Foto(kasutaja)) { Bounds = new Rectangle(28 + i * 128, 488, 120, 112) };
                p.Click += (s, e) => ValiKujundus(n);
                pisipildid.Add(p);
                vasak.Controls.Add(p);
            }

            var foto = UiStiil.LooNupp("Lae oma foto", (s, e) => LaeFoto(kasutaja));
            foto.Location = new Point(28, 622);
            foto.MinimumSize = new Size(150, 44);
            ((RoundedButton)foto).Peamine = false;
            vasak.Controls.Add(foto);
            vasak.Controls.Add(new Label { Text = "JPG või PNG · foto lõigatakse kaardi kujuliseks, all on tume toon, et tekst jääks loetav", Location = new Point(196, 628), Size = new Size(470, 36), ForeColor = Teema.TekstNork, BackColor = Teema.Pind, Font = new Font("Segoe UI", 8.5F) });
            Controls.Add(vasak);

            // ---- Paremal ----
            var kood = new VootkoodiKaart(AktiivneKasutaja.Kaardinumber ?? "") { Bounds = new Rectangle(720, 16, 264, 180) };
            Controls.Add(kood);
            Controls.Add(new VarviPlaat("Boonus", Ostukorv.Eur(AktiivneKasutaja.Boonus), Color.FromArgb(226, 6, 19)) { Bounds = new Rectangle(720, 208, 126, 90) });
            Controls.Add(new VarviPlaat("Tase", $"{Keel.T(tase.Nimi)} · {(tase.Protsent * 100).ToString("0.#", new System.Globalization.CultureInfo("et-EE"))} %", Color.FromArgb(160, 35, 110)) { Bounds = new Rectangle(858, 208, 126, 90) });

            var paremAll = new RoundedPanel { Bounds = new Rectangle(720, 310, 264, 394), Radius = 14, FillColor = Teema.Pind, BorderColor = Teema.Pind, BackColor = Teema.Taust };
            paremAll.Controls.Add(new Label { Text = "Viimased ostud", Location = new Point(18, 16), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Teema.Tekst, BackColor = Teema.Pind });
            paremAll.Controls.Add(new OstuRead(ostud) { Bounds = new Rectangle(12, 48, 240, 200) });
            var preemiad = UiStiil.LooSuurNupp("Preemiad ja tase  ›", (s, e) => { var o = Owner; Close(); BeginInvoke(new Action(() => PreemiadLeht.Ava(o))); });
            preemiad.Bounds = new Rectangle(18, 266, 228, 50);
            var boonus = UiStiil.LooSuurNupp("Kasuta boonust maksmisel", (s, e) => Teade.Toast("Maksmisel märgi „Kasuta boonust“ – boonus läheb allahindluseks.", TeateTyyp.Info, 4000));
            boonus.Bounds = new Rectangle(18, 326, 228, 50);
            boonus.Font = new Font("Segoe UI Semibold", 10F);
            paremAll.Controls.Add(preemiad);
            paremAll.Controls.Add(boonus);
            Controls.Add(paremAll);

            ValiKujundus(kaart.Disain, salvesta: false);
        }

        private void ValiKujundus(int n, bool salvesta = true)
        {
            kaart.Vaheta(n);
            foreach (var p in pisipildid) p.Valitud = p.Disain == n;
            lblValik.Text = Keel.T(KaardiKujundus.Nimed[n]);
            lblValik.Location = new Point(660 - lblValik.PreferredWidth, 40);
            if (salvesta) KaardiKujundus.Vali(AktiivneKasutaja.Id, n);
        }

        private void LaeFoto(int kasutaja)
        {
            using var dlg = new OpenFileDialog { Filter = "Pildid|*.jpg;*.jpeg;*.png", Title = Keel.T("Lae oma foto") };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                using (var alg = Image.FromFile(dlg.FileName))
                {
                    // vähendame kaardi suuruseks (max 900 px laiune)
                    float k = Math.Min(1f, 900f / alg.Width);
                    using var bmp = new Bitmap(alg, new Size((int)(alg.Width * k), (int)(alg.Height * k)));
                    Directory.CreateDirectory(Kaustad.Juur);
                    bmp.Save(KaardiKujundus.FotoFail(kasutaja), System.Drawing.Imaging.ImageFormat.Png);
                }
                var foto = KaardiKujundus.Foto(kasutaja);
                kaart.Foto = foto;
                foreach (var p in pisipildid) p.Foto = foto;
                ValiKujundus(4);
                Teade.Toast("Foto lisatud kaardile");
            }
            catch (Exception ex) { Teade.Show("Foto laadimine ebaõnnestus:\n" + ex.Message); }
        }

        public bool MineTagasi() { Close(); return true; }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || (e.Alt && e.KeyCode == Keys.Left)) { Close(); e.Handled = true; }
            base.OnKeyDown(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Teema.Rakenda(this);
        }
    }

    // Suur kaart roosal alusel: kujunduse vahetus = kaart "hüppab", jooned joonistuvad, kaart kõigub hiirega
    public class SuurKaart : Control
    {
        private readonly TaseInfo tase;
        private readonly string nimi, number;
        private readonly decimal boonus;
        private double aeg, joon = 1, hypp = 1;
        private PointF kalle;
        public Image? Foto { get; set; }
        public int Disain { get; set; }

        public SuurKaart(TaseInfo tase, string nimi, string number, decimal boonus, Image? foto)
        {
            this.tase = tase; this.nimi = nimi; this.number = number; this.boonus = boonus; Foto = foto;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Animatsioon.Tsykkel(this, s => { aeg = s; Invalidate(); }, 40);
        }

        public void Vaheta(int disain)
        {
            Disain = disain;
            Animatsioon.Kaivita(this, 2400, t => joon = Animatsioon.Sujuv(t));
            Animatsioon.Kaivita(this, 450, t => hypp = t);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            kalle = new PointF((e.X - Width / 2f) / Width, (e.Y - Height / 2f) / Height);
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) { kalle = PointF.Empty; base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Pind);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 26))
            using (var b = new SolidBrush(Color.FromArgb(246, 230, 239)))
                g.FillPath(b, tee);

            float k = (float)(0.92 + 0.08 * Animatsioon.Pop(hypp));
            float w = 440 * k, h = 278 * k;
            var r = new RectangleF((Width - w) / 2 + kalle.X * 10, (Height - h) / 2 + kalle.Y * 8 + (float)Math.Sin(aeg * 1.1) * 4, w, h);
            for (int i = 0; i < 8; i++)
                using (var vari = new SolidBrush(Color.FromArgb(12, 60, 10, 40)))
                using (var vt = Ymardus.Tee(RectangleF.Inflate(new RectangleF(r.X, r.Y + 18, r.Width, r.Height), 10 - i, 8 - i), 24))
                    g.FillPath(vari, vt);
            var olek = g.Save();
            g.TranslateTransform(r.X + r.Width / 2, r.Y + r.Height / 2);
            g.RotateTransform(kalle.X * 3);
            g.TranslateTransform(-(r.X + r.Width / 2), -(r.Y + r.Height / 2));
            KaardiKujundus.Joonista(g, r, Disain, tase, Foto, nimi, number, boonus, aeg, joon);
            g.Restore(olek);
        }
    }

    // Kujunduse pisipilt (valitud = punane raam)
    public class KujunduseValik : Control
    {
        public int Disain { get; }
        private readonly TaseInfo tase;
        private bool valitud, hover;
        public Image? Foto { get; set; }
        public bool Valitud { get => valitud; set { valitud = value; Invalidate(); } }

        public KujunduseValik(int disain, TaseInfo tase, Image? foto)
        {
            Disain = disain; this.tase = tase; Foto = foto;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Pind);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var raam = new RectangleF(1, 1, Width - 3, Height - 3);
            using (var tee = Ymardus.Tee(raam, 18))
            {
                using (var b = new SolidBrush(valitud ? Color.FromArgb(253, 236, 238) : hover ? Teema.Paneel2 : Teema.Pind)) g.FillPath(b, tee);
                using (var p = new Pen(valitud ? Color.FromArgb(226, 6, 19) : Teema.Joon, valitud ? 2 : 1)) g.DrawPath(p, tee);
            }
            var mini = new RectangleF(10, 10 - (hover ? 2 : 0), Width - 20, (Width - 20) / 1.58f);
            if (Disain == 4 && Foto == null)
            {
                using (var tee = Ymardus.Tee(mini, 8)) using (var b = new SolidBrush(Color.FromArgb(91, 18, 64))) g.FillPath(b, tee);
                Ikoonid.Joonista(g, "", Rectangle.Round(mini), Color.White, 14f);
            }
            else KaardiKujundus.Joonista(g, mini, Disain, tase, Foto, "", "", 0, 0, 1);
            using var f = new Font("Segoe UI Semibold", 9F);
            TextRenderer.DrawText(g, Keel.T(KaardiKujundus.Nimed[Disain]), f, new Rectangle(0, (int)mini.Bottom + 6, Width, 20), Teema.Tekst, TextFormatFlags.HorizontalCenter);
        }
    }

    // "KASSAS · SKANNI": EAN-13 vöötkood valgel kaardil
    public class VootkoodiKaart : Control
    {
        private readonly string number;
        public VootkoodiKaart(string number)
        {
            this.number = number;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Taust);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 28)) using (var b = new SolidBrush(Color.White)) g.FillPath(b, tee);
            using var f = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            TextRenderer.DrawText(g, Keel.T("KASSAS"), f, new Point(22, 18), Color.FromArgb(100, 106, 115));
            TextRenderer.DrawText(g, Keel.T("SKANNI"), f, new Rectangle(0, 18, Width - 22, 16), Color.FromArgb(100, 106, 115), TextFormatFlags.Right);
            if (number.Length > 0) Triipkood.JoonistaEan13(g, Triipkood.Ean13Number(number), new RectangleF(30, 42, Width - 60, 96), Color.FromArgb(27, 28, 31));
        }
    }

    // Värviline plaat (Boonus / Tase)
    public class VarviPlaat : Control
    {
        private readonly string silt, vaartus;
        private readonly Color varv;
        public VarviPlaat(string silt, string vaartus, Color varv)
        {
            this.silt = silt; this.vaartus = vaartus; this.varv = varv;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Taust);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var tee = Ymardus.Tee(new RectangleF(0, 0, Width - 1, Height - 1), 24)) using (var b = new SolidBrush(varv)) g.FillPath(b, tee);
            using var f = new Font("Segoe UI", 8.5F);
            using var fb = new Font("Segoe UI", vaartus.Length > 9 ? 11F : 16F, FontStyle.Bold);
            TextRenderer.DrawText(g, Keel.T(silt), f, new Point(16, 16), Color.FromArgb(230, Color.White));
            TextRenderer.DrawText(g, vaartus, fb, new Rectangle(14, 36, Width - 18, 40), Color.White, TextFormatFlags.WordBreak);
        }
    }

    // Viimased ostud roosadel ridadel (boonuse lisandumine rohelisega)
    public class OstuRead : Control
    {
        private readonly List<Ost> ostud;
        public OstuRead(List<Ost> ostud)
        {
            this.ostud = ostud;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Teema.Pind);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var fb = new Font("Segoe UI Semibold", 9.5F);
            using var f = new Font("Segoe UI", 8.5F);
            if (ostud.Count == 0) { TextRenderer.DrawText(g, Keel.T("Ostusid veel pole."), f, new Point(4, 4), Teema.TekstNork); return; }
            int y = 0;
            foreach (var o in ostud)
            {
                var r = new RectangleF(0, y, Width - 1, 60);
                using (var tee = Ymardus.Tee(r, 18)) using (var b = new SolidBrush(Color.FromArgb(246, 230, 239))) g.FillPath(b, tee);
                var ring = new RectangleF(10, y + 10, 40, 40);
                using (var b = new SolidBrush(Color.FromArgb(160, 35, 110))) g.FillEllipse(b, ring);
                Ikoonid.Joonista(g, Ikoonid.Ostukorv, Rectangle.Round(ring), Color.White, 11f);
                TextRenderer.DrawText(g, Keel.F("Ost nr {0}", o.Id), fb, new Point(60, y + 12), Color.FromArgb(27, 28, 31));
                TextRenderer.DrawText(g, o.Aeg.ToString("dd.MM.yyyy"), f, new Point(60, y + 32), Color.FromArgb(100, 106, 115));
                TextRenderer.DrawText(g, Ostukorv.Eur(o.Summa), fb, new Rectangle(0, y + 12, Width - 12, 18), Color.FromArgb(27, 28, 31), TextFormatFlags.Right);
                TextRenderer.DrawText(g, "+" + Ostukorv.Eur(o.BoonusLisatud), f, new Rectangle(0, y + 32, Width - 12, 18), Color.FromArgb(17, 104, 58), TextFormatFlags.Right);
                y += 68;
            }
        }
    }
}
