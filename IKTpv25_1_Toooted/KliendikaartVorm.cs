using System.Diagnostics;
using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    // ===================================================================
    //  Kliendikaart nagu iPhone'i rahakotis (Wallet):
    //  kaart libiseb alt üles, klõps pöörab kaardi ümber (taga vöötkood),
    //  all viimased ostud ja boonuse muutus. Esc / ✕ / klõps mujal sulgeb.
    // ===================================================================
    public class KliendikaartVorm : Form, ITagasi
    {
        // Avab kaardi; kui kaarti pole, pakub selle kohe luua
        public static void Ava(IWin32Window? omanik)
        {
            AktiivneKasutaja.Uuenda();
            if (AktiivneKasutaja.Kaardinumber == null)
            {
                if (Teade.Show("Sul pole veel kliendikaarti. Kas loon selle kohe?", "Kliendikaart",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                try
                {
                    using var db = new ToodedContext();
                    var k = db.Kasutajad.Find(AktiivneKasutaja.Id);
                    if (k == null) return;
                    k.Klient = Kliendikaart.Loo(db, k.Nimi ?? k.Kasutajanimi, k.Email);
                    db.SaveChanges();
                    AktiivneKasutaja.Uuenda();
                }
                catch (Exception ex) { Teade.Show("Viga:\n" + ex.Message); return; }
            }

            using var vorm = new KliendikaartVorm();
            if (omanik != null) vorm.ShowDialog(omanik);
            else { vorm.StartPosition = FormStartPosition.CenterScreen; vorm.ShowDialog(); }
        }

        private record Tehing(DateTime Aeg, decimal Summa, decimal Lisatud, decimal Kasutatud);

        private readonly string nimi, number, ean;
        private readonly decimal boonus;
        private readonly TaseInfo tase = Preemiad.TaseSummast(0);
        private double kaardiAeg;      // läige/sädemed (sekundid)
        private float holo = 0.5f;     // kuldkaardi hologramm hiire järgi
        private RectangleF preemiadKoht => new(28, kaardiKoht.Bottom + 12, 404, 40);
        private bool preemiadAll;
        private readonly List<Tehing> ajalugu = new();

        // Animatsioon
        private readonly System.Windows.Forms.Timer taimer = new() { Interval = 15 };
        private readonly Stopwatch kell = new();
        private double avatud;          // 0 → 1 (kaart tõuseb alt)
        private bool sulgeb;
        private double poore;           // 0 → 1 (kaardi pööramine)
        private bool poorab, tagusEes;  // kas praegu on tagakülg ees
        private long poordeAlgus;

        private const int Laius = 460, Korgus = 710;
        private readonly RectangleF kaardiKoht = new(28, 74, 404, 255);   // 1,586 : 1 nagu pangakaart
        private RectangleF sulgeKoht => new(Laius - 50, 18, 32, 32);
        private RectangleF tagasiKoht => new(22, 16, TextRenderer.MeasureText(Keel.T("Tagasi"), tagasiFont).Width + 50, 38);
        private static readonly Font tagasiFont = new("Segoe UI", 10.5F, FontStyle.Bold);
        private bool tagasiAll;

        public bool MineTagasi() { Sulge(); return true; }

        public KliendikaartVorm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            ClientSize = new Size(Laius, Korgus);
            BackColor = Color.FromArgb(20, 20, 24);
            DoubleBuffered = true;
            KeyPreview = true;
            Opacity = 0;
            Text = Keel.T("Kliendikaart");

            nimi = (AktiivneKasutaja.TaisNimi ?? AktiivneKasutaja.Nimi).ToUpper();
            number = AktiivneKasutaja.Kaardinumber ?? "";
            boonus = AktiivneKasutaja.Boonus;
            try
            {
                using var db0 = new ToodedContext();
                if (AktiivneKasutaja.KlientId is int kid) tase = Preemiad.LeiaTase(db0, kid);
            }
            catch { }
            ean = Triipkood.Ean13Number(number);

            try
            {
                using var db = new ToodedContext();
                ajalugu = db.Ostud.AsNoTracking()
                    .Where(o => o.KlientId == AktiivneKasutaja.KlientId && !o.Tagastatud)
                    .OrderByDescending(o => o.Aeg)
                    .Take(6)
                    .Select(o => new Tehing(o.Aeg, o.Summa, o.BoonusLisatud, o.Soodustus))
                    .ToList();
            }
            catch { }

            taimer.Tick += (s, e) => Samm();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            using var tee = Ymardus.Tee(new Rectangle(0, 0, Laius, Korgus), 14);
            Region = new Region(tee);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Hõbe- ja kuldkaardi läige ning sädemed liiguvad pidevalt
            if (tase.Tase != Tase.Pronks)
                Animatsioon.Tsykkel(this, s => { kaardiAeg = s; Invalidate(Rectangle.Ceiling(RectangleF.Inflate(kaardiKoht, 4, 4))); }, 40);
            kell.Start();
            taimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            taimer.Stop();
            taimer.Dispose();
            base.OnFormClosed(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || (e.Alt && e.KeyCode == Keys.Left)) Sulge();
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) Poora();
            base.OnKeyDown(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (sulgeKoht.Contains(e.Location) || tagasiKoht.Contains(e.Location)) Sulge();
            else if (preemiadKoht.Contains(e.Location))
            {
                // Preemiad: kaart sulgub, preemiate leht avaneb (poes lehena, muidu aknana)
                var omanik = Owner;
                Sulge();
                BeginInvoke(new Action(() => PreemiadLeht.Ava(omanik)));
            }
            else if (kaardiKoht.Contains(e.Location)) Poora();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (kaardiKoht.Contains(e.Location)) holo = (e.X - kaardiKoht.X) / kaardiKoht.Width;
            bool pAll = preemiadKoht.Contains(e.Location);
            if (pAll != preemiadAll) { preemiadAll = pAll; Invalidate(Rectangle.Ceiling(preemiadKoht)); }
            bool all = tagasiKoht.Contains(e.Location);
            if (all != tagasiAll) { tagasiAll = all; Invalidate(Rectangle.Ceiling(tagasiKoht)); }
            Cursor = kaardiKoht.Contains(e.Location) || sulgeKoht.Contains(e.Location) || all || pAll ? Cursors.Hand : Cursors.Default;
        }

        private void Poora()
        {
            if (poorab || avatud < 1) return;
            poorab = true;
            poordeAlgus = kell.ElapsedMilliseconds;
            taimer.Start();
        }

        private void Sulge()
        {
            if (sulgeb) return;
            sulgeb = true;
            kell.Restart();
            taimer.Start();
        }

        // Üks animatsiooni samm (~60 korda sekundis)
        private void Samm()
        {
            long ms = kell.ElapsedMilliseconds;
            bool kaib = false;

            if (sulgeb)
            {
                avatud = Math.Max(0, 1 - ms / 260.0);
                Opacity = avatud;
                if (avatud <= 0) { taimer.Stop(); Close(); return; }
                kaib = true;
            }
            else if (avatud < 1)
            {
                avatud = Math.Min(1, ms / 480.0);
                Opacity = Math.Min(1, avatud * 2.2);
                kaib = true;
            }

            if (poorab)
            {
                poore = Math.Min(1, (ms - poordeAlgus) / 420.0);
                if (poore >= 1) { poorab = false; poore = 0; tagusEes = !tagusEes; }
                else kaib = true;
            }

            Invalidate();
            if (!kaib) taimer.Stop();
        }

        // Pehme lõpp (ease-out), väike "vetrumine" lõpus
        private static double Pehme(double t)
        {
            double c1 = 1.2, c3 = c1 + 1;
            return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.Clear(BackColor);

            double k = Pehme(avatud);
            float nihe = (float)((1 - k) * 380);

            // ← Tagasi (hele "pill" tumedal taustal), pealkiri ja sulgemisnupp
            var tk = tagasiKoht;
            using (var tee = Ymardus.Tee(tk, tk.Height / 2))
            using (var b = new SolidBrush(tagasiAll ? Color.FromArgb(225, 228, 232) : Color.White))
                g.FillPath(b, tee);
            Ikoonid.Joonista(g, Ikoonid.NoolVasakule, new Rectangle((int)tk.X + 10 - (tagasiAll ? 3 : 0), (int)tk.Y, 20, (int)tk.Height), Color.FromArgb(27, 28, 31), 10f);
            TextRenderer.DrawText(g, Keel.T("Tagasi"), tagasiFont, new Rectangle((int)tk.X + 34, (int)tk.Y, (int)tk.Width - 34, (int)tk.Height),
                Color.FromArgb(27, 28, 31), TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
            using (var f = new Font("Segoe UI Semibold", 15F))
            using (var b = new SolidBrush(Color.White))
                g.DrawString(Keel.T("Kliendikaart"), f, b, tk.Right + 14, 21);
            using (var b = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                g.FillEllipse(b, sulgeKoht);
            using (var f = new Font("Segoe UI", 10F))
            using (var b = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("✕", f, b, sulgeKoht, sf);
            }

            // Vari kaardi all
            var kaart = kaardiKoht;
            kaart.Y += nihe;
            for (int i = 0; i < 6; i++)
            {
                using var vari = new SolidBrush(Color.FromArgb(14, 0, 0, 0));
                using var vt = Ymardus.Tee(RectangleF.Inflate(new RectangleF(kaart.X, kaart.Y + 10 + i, kaart.Width, kaart.Height), 6 - i, 4 - i * 0.5f), 18);
                g.FillPath(vari, vt);
            }

            // Kaart (pööramisel kitseneb ja laieneb)
            bool tagus = tagusEes ^ (poorab && poore >= 0.5);
            float sx = poorab ? (float)Math.Max(0.02, Math.Abs(Math.Cos(Math.PI * poore))) : 1f;
            float skaala = (float)(0.9 + 0.1 * k);
            var olek = g.Save();
            g.TranslateTransform(kaart.X + kaart.Width / 2, kaart.Y + kaart.Height / 2);
            g.ScaleTransform(skaala * sx, skaala);
            g.TranslateTransform(-kaart.Width / 2, -kaart.Height / 2);
            var r = new RectangleF(0, 0, kaart.Width, kaart.Height);
            if (tagus) Tagakulg(g, r); else Esikulg(g, r);
            g.Restore(olek);

            // Ostude ajalugu (ilmub pärast kaarti)
            int alfa = (int)(Math.Clamp((k - 0.4) / 0.6, 0, 1) * 255);
            if (alfa <= 0) return;
            // "Preemiad · Hõbe 1,5 %  ›" nupp kaardi all
            var pk = preemiadKoht;
            pk.Y += nihe * 0.5f;
            using (var tee = Ymardus.Tee(pk, pk.Height / 2))
            using (var b = new SolidBrush(Color.FromArgb(alfa * (preemiadAll ? 60 : 34) / 255, 255, 255, 255)))
                g.FillPath(b, tee);
            TaseKaart.Medal(g, new RectangleF(pk.X + 8, pk.Y + 7, 26, 26), tase.Tase);
            using (var f = new Font("Segoe UI Semibold", 10F))
                TextRenderer.DrawText(g, Keel.F("Preemiad · {0} {1} %", Keel.T(tase.Nimi), (tase.Protsent * 100).ToString("0.#", new System.Globalization.CultureInfo("et-EE"))) + "  ›",
                    f, Rectangle.Round(new RectangleF(pk.X + 44, pk.Y, pk.Width - 50, pk.Height)), Color.FromArgb(alfa, Color.White), TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            float y = kaardiKoht.Bottom + 34 + 52 + nihe * 0.5f;
            using (var f = new Font("Segoe UI Semibold", 11F))
            using (var b = new SolidBrush(Color.FromArgb(alfa, Color.White)))
                g.DrawString(Keel.T("Viimased ostud"), f, b, 28, y);
            y += 32;

            var hall = Color.FromArgb(alfa * 150 / 255, 255, 255, 255);
            using var tavaline = new Font("Segoe UI", 9.5F);
            using var paks = new Font("Segoe UI Semibold", 9.5F);
            if (ajalugu.Count == 0)
            {
                using var b = new SolidBrush(hall);
                g.DrawString(Keel.T("Ostusid veel pole – iga 1 € annab 1 sendi boonust."), tavaline, b, 28, y);
            }
            foreach (var t in ajalugu)
            {
                var rida = new RectangleF(28, y, 404, 36);
                using (var taust = new SolidBrush(Color.FromArgb(alfa * 18 / 255, 255, 255, 255)))
                using (var tee = Ymardus.Tee(rida, 8))
                    g.FillPath(taust, tee);
                using (var b = new SolidBrush(Color.FromArgb(alfa, Color.White)))
                    g.DrawString(t.Aeg.ToString("dd.MM.yyyy  HH:mm"), tavaline, b, 40, y + 9);
                using (var b = new SolidBrush(hall))
                    g.DrawString(Ostukorv.Eur(t.Summa), tavaline, b, 210, y + 9);
                string muutus = "+" + Ostukorv.Eur(t.Lisatud) + (t.Kasutatud > 0 ? "   −" + Ostukorv.Eur(t.Kasutatud) : "");
                var sf = new StringFormat { Alignment = StringAlignment.Far };
                using (var b = new SolidBrush(Color.FromArgb(alfa, 110, 220, 150)))
                    g.DrawString(muutus, paks, b, new RectangleF(250, y + 9, 170, 20), sf);
                y += 42;
            }

            using (var f = new Font("Segoe UI", 8.5F))
            using (var b = new SolidBrush(Color.FromArgb(alfa * 120 / 255, 255, 255, 255)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(Keel.T("Klõpsa kaardil – vöötkood kassas skannimiseks"), f, b, new RectangleF(0, Korgus - 34, Laius, 20), sf);
            }
        }

        // ---- Kaardi esikülg: värviline, logo, boonus, nimi, number ----
        private void Esikulg(Graphics g, RectangleF r)
        {
            // Taust taseme järgi: Pronks vask, Hõbe harjatud hõbe + läige, Kuld + hologramm ja sädemed
            TaseKaart.JoonistaTaust(g, r, tase.Tase, kaardiAeg, holo);
            using var tee = Ymardus.Tee(r, 9);

            var olek = g.Save();
            g.SetClip(tee);
            // Läige
            using (var laige = new LinearGradientBrush(new RectangleF(0, 0, r.Width, r.Height),
                       Color.FromArgb(50, Color.White), Color.FromArgb(0, Color.White), 70f))
                g.FillRectangle(laige, 0, 0, r.Width, r.Height / 2);
            g.Restore(olek);

            Logo.Joonista(g, new RectangleF(20, 18, 112, 42), valgeTaustal: false);

            using (var f = new Font("Segoe UI Semibold", 8F))
            using (var b = new SolidBrush(Color.FromArgb(215, Color.White)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Far };
                g.DrawString(Keel.T("KLIENDIKAART"), f, b, new RectangleF(r.Width - 200, 24, 180, 16), sf);
                // tase: väike medal + "Hõbe · 1,5 %"
                string taseTekst = $"{Keel.T(tase.Nimi)} · {(tase.Protsent * 100).ToString("0.#", new System.Globalization.CultureInfo("et-EE"))} %";
                g.DrawString(taseTekst, f, b, new RectangleF(r.Width - 200, 42, 180, 16), sf);
                var tw = g.MeasureString(taseTekst, f).Width;
                TaseKaart.Medal(g, new RectangleF(r.Width - 20 - tw - 20, 41, 16, 16), tase.Tase);
            }

            // Kiip
            var kiip = new RectangleF(r.Width - 72, 70, 46, 34);
            using (var kb = new LinearGradientBrush(kiip, Color.FromArgb(240, 214, 150), Color.FromArgb(190, 150, 80), 45f))
            using (var kt = Ymardus.Tee(kiip, 3))
                g.FillPath(kb, kt);
            using (var p = new Pen(Color.FromArgb(120, 120, 90, 40)))
            {
                g.DrawLine(p, kiip.X + 15, kiip.Y, kiip.X + 15, kiip.Bottom);
                g.DrawLine(p, kiip.X + 31, kiip.Y, kiip.X + 31, kiip.Bottom);
                g.DrawLine(p, kiip.X, kiip.Y + 17, kiip.Right, kiip.Y + 17);
            }

            using (var f = new Font("Segoe UI", 9F))
            using (var b = new SolidBrush(Color.FromArgb(210, Color.White)))
                g.DrawString(Keel.T("Boonus"), f, b, 22, 80);
            using (var f = new Font("Segoe UI Semibold", 28F))
            using (var b = new SolidBrush(Color.White))
                g.DrawString(Ostukorv.Eur(boonus), f, b, 16, 98);
            using (var f = new Font("Segoe UI", 8.5F))
            using (var b = new SolidBrush(Color.FromArgb(200, Color.White)))
                g.DrawString(Keel.T("1 € = 1 sent boonust · kasuta maksmisel"), f, b, 22, 150);

            using (var f = new Font("Segoe UI Semibold", 10.5F))
            using (var b = new SolidBrush(Color.White))
                g.DrawString(nimi, f, b, 22, r.Height - 64);
            using (var f = new Font("Consolas", 13F, FontStyle.Bold))
            using (var b = new SolidBrush(Color.FromArgb(235, Color.White)))
                g.DrawString(Ruumidega(number), f, b, 20, r.Height - 40);
        }

        // ---- Tagakülg: valge, vöötkood (EAN-13) ----
        private void Tagakulg(Graphics g, RectangleF r)
        {
            using var tee = Ymardus.Tee(r, 9);
            using (var taust = new SolidBrush(Color.White))
                g.FillPath(taust, tee);

            var olek = g.Save();
            g.SetClip(tee);
            using (var riba = new SolidBrush(Teema.Teine))
                g.FillRectangle(riba, 0, 0, r.Width, 44);
            g.Restore(olek);
            using (var f = new Font("Segoe UI Semibold", 10F))
            using (var b = new SolidBrush(Color.White))
                g.DrawString(Arve.PoeNimi + "  ·  " + Keel.T("Kliendikaart"), f, b, 18, 12);

            var kood = new RectangleF(r.Width / 2 - 140, 66, 280, 110);
            Triipkood.JoonistaEan13(g, ean, kood, Color.FromArgb(20, 20, 20));

            using (var f = new Font("Segoe UI", 8.5F))
            using (var b = new SolidBrush(Color.FromArgb(110, 110, 110)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(Keel.F("Näita kassas · kaart {0} · boonus {1}", number, Ostukorv.Eur(boonus)), f, b,
                    new RectangleF(0, r.Height - 40, r.Width, 18), sf);
            }
        }

        private static string Ruumidega(string n) =>
            string.Join(" ", Enumerable.Range(0, (n.Length + 2) / 3).Select(i => n.Substring(i * 3, Math.Min(3, n.Length - i * 3))));
    }

    // EAN-13 vöötkood (päris kodeering – telefoniga skannitav)
    public static class Triipkood
    {
        private static readonly string[] L = { "0001101", "0011001", "0010011", "0111101", "0100011", "0110001", "0101111", "0111011", "0110111", "0001011" };
        private static readonly string[] G = { "0100111", "0110011", "0011011", "0100001", "0011101", "0111001", "0000101", "0010001", "0001001", "0010111" };
        private static readonly string[] R = { "1110010", "1100110", "1101100", "1000010", "1011100", "1001110", "1010000", "1000100", "1001000", "1110100" };
        private static readonly string[] Paarsus = { "LLLLLL", "LLGLGG", "LLGGLG", "LLGGGL", "LGLLGG", "LGGLLG", "LGGGLL", "LGLGLG", "LGLGGL", "LGGLGL" };

        // Kaardinumbrist 13-kohaline number: 2 (poe sisekood) + number + kontrollnumber
        public static string Ean13Number(string kaart)
        {
            string numbrid = new string(kaart.Where(char.IsDigit).ToArray());
            string kaksteist = ("2" + numbrid.PadLeft(11, '0'));
            kaksteist = kaksteist.Substring(kaksteist.Length - 12);
            int summa = 0;
            for (int i = 0; i < 12; i++) summa += (kaksteist[i] - '0') * (i % 2 == 0 ? 1 : 3);
            return kaksteist + ((10 - summa % 10) % 10);
        }

        public static void JoonistaEan13(Graphics g, string ean, RectangleF koht, Color varv)
        {
            if (ean.Length != 13) return;
            int esimene = ean[0] - '0';
            var bitid = new System.Text.StringBuilder("101");
            for (int i = 1; i <= 6; i++)
            {
                int d = ean[i] - '0';
                bitid.Append(Paarsus[esimene][i - 1] == 'L' ? L[d] : G[d]);
            }
            bitid.Append("01010");
            for (int i = 7; i <= 12; i++) bitid.Append(R[ean[i] - '0']);
            bitid.Append("101");

            // 95 moodulit + vaikne ala mõlemal pool
            float moodul = koht.Width / 105f;
            float x0 = koht.X + moodul * 5;
            float kood = koht.Height - 18;
            var vanaOlek = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.None;
            using (var b = new SolidBrush(varv))
            {
                for (int i = 0; i < 95; i++)
                {
                    if (bitid[i] != '1') continue;
                    bool kaitse = i < 3 || (i >= 45 && i < 50) || i >= 92;   // pikemad kaitsejooned
                    g.FillRectangle(b, x0 + i * moodul, koht.Y, moodul, kood + (kaitse ? 8 : 0));
                }
            }
            g.SmoothingMode = vanaOlek;

            using var f = new Font("Consolas", 11F);
            using var tb = new SolidBrush(varv);
            float ty = koht.Y + kood + 2;
            g.DrawString(ean[0].ToString(), f, tb, koht.X, ty);
            var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(ean.Substring(1, 6), f, tb, new RectangleF(x0 + 3 * moodul, ty, 42 * moodul, 18), sf);
            g.DrawString(ean.Substring(7, 6), f, tb, new RectangleF(x0 + 50 * moodul, ty, 42 * moodul, 18), sf);
        }
    }
}
