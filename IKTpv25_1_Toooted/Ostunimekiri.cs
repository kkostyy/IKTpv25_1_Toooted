using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore;

namespace IKTpv25_1_Toooted
{
    // Ostunimekirja abifunktsioonid (andmed tabelis Ostunimekiri, iga kasutaja oma)
    public static class Ostunimekiri
    {
        public static event Action? Muutus;

        public static void TeavitaMuutusest() => Muutus?.Invoke();

        // Lisab toote (või vaba teksti) nimekirja; sama ostmata toode → kogus suureneb
        public static void Lisa(string tekst, int kogus = 1, int? toodeId = null)
        {
            if (AktiivneKasutaja.Id == 0 || string.IsNullOrWhiteSpace(tekst)) return;
            tekst = tekst.Trim();
            using var db = new ToodedContext();

            // Vaba tekst, mis on täpselt toote nimi → seome tootega
            if (toodeId == null)
            {
                var toode = db.Toodetabel.AsNoTracking().Where(t => t.Toodenimetus == tekst).Select(t => (int?)t.Id).FirstOrDefault();
                toodeId = toode;
            }

            var pere = Pere.NahtavadKasutajad(db);   // pere nimekirjas sama toode ainult üks kord
            var ostmata = db.Ostunimekiri.Where(r => pere.Contains(r.KasutajaId) && !r.Ostetud);
            var olemas = toodeId is int tid
                ? ostmata.FirstOrDefault(r => r.ToodeId == tid)
                : ostmata.FirstOrDefault(r => r.Tekst == tekst);
            if (olemas != null) olemas.Kogus += Math.Max(1, kogus);
            else db.Ostunimekiri.Add(new OstunimekirjaRida
            {
                KasutajaId = AktiivneKasutaja.Id,
                Tekst = tekst,
                Kogus = Math.Max(1, kogus),
                ToodeId = toodeId
            });
            db.SaveChanges();
            Muutus?.Invoke();
        }

        // Pärast ostu: ostetud tooted saavad nimekirjas linnukese
        public static int MargiOstetuks(Ost ost)
        {
            if (AktiivneKasutaja.Id == 0) return 0;
            try
            {
                var ids = ost.Read.Where(r => r.ToodeId != null).Select(r => r.ToodeId).ToList();
                var nimed = ost.Read.Select(r => r.Toodenimetus).ToList();
                using var db = new ToodedContext();
                var pere = Pere.NahtavadKasutajad(db);
                var read = db.Ostunimekiri
                    .Where(r => pere.Contains(r.KasutajaId) && !r.Ostetud &&
                                ((r.ToodeId != null && ids.Contains(r.ToodeId)) || nimed.Contains(r.Tekst)))
                    .ToList();
                foreach (var r in read) { r.Ostetud = true; r.OstetudAeg = DateTime.Now; r.OstisKasutajaId = AktiivneKasutaja.Id; }
                db.SaveChanges();
                if (read.Count > 0) Muutus?.Invoke();
                return read.Count;
            }
            catch { return 0; }
        }

        public static int OstmataArv()
        {
            if (AktiivneKasutaja.Id == 0) return 0;
            try
            {
                using var db = new ToodedContext();
                var pere = Pere.NahtavadKasutajad(db);
                return db.Ostunimekiri.Count(r => pere.Contains(r.KasutajaId) && !r.Ostetud);
            }
            catch { return 0; }
        }
    }

    public class NimekirjaVaade
    {
        public int Id { get; set; }
        public bool Ostetud { get; set; }
        public string Toode { get; set; } = "";
        public int Kogus { get; set; }
        public string Hind { get; set; } = "";
        public string Laos { get; set; } = "";
        public DateTime Lisatud { get; set; }
        public string Lisas { get; set; } = "";     // pere nimekirjas: kes lisas
        public string Ostis { get; set; } = "";     // "ostis Jaan"
        internal int? ToodeId;
        internal int KasutajaId;
    }

    // ===================================================================
    //  Ostunimekiri (shopping list): mis on ostetud ✓, mis veel mitte
    // ===================================================================
    public class OstunimekiriVorm : KujundusVorm
    {
        private readonly DataGridView vork;
        private readonly TextBox txtToode;
        private readonly NumericUpDown numKogus;
        private readonly RadioButton rbKoik, rbOstmata, rbOstetud;
        private readonly Label lblArv;
        private readonly Edenemine edenemine = new();
        private readonly Font labi = new("Segoe UI", 9F, FontStyle.Strikeout);
        private List<NimekirjaVaade> koik = new();

        // Pere nimekiri
        private record Tegevus(DateTime Aeg, int KasutajaId, string Nimi, string Tekst);
        private List<Kasutaja> liikmed = new();
        private List<Tegevus> tegevused = new();
        private string? pereKood;
        private readonly Label lblPealkiri;
        private readonly PereAvatarid avatarid = new();
        private readonly Panel perePaneel;
        private readonly KoodiPlaadid koodiPlaadid = new();
        private readonly TegevusteLoend tegevusteLoend = new();
        private readonly System.Windows.Forms.Timer elav = new() { Interval = 5000 };

        // Moodne rida: kogu rida joonistatakse ise (ümar hall taust, ploom ring, nimi, kogus, hind, avatar)
        private void MoodneRida(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || vork.Rows[e.RowIndex].DataBoundItem is not NimekirjaVaade r) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var rb = e.RowBounds;
            using (var b = new SolidBrush(vork.BackgroundColor)) g.FillRectangle(b, rb);
            var kast = new RectangleF(rb.X + 2, rb.Y + 3, vork.ClientSize.Width - 6, rb.Height - 6);
            bool valitud = vork.Rows[e.RowIndex].Selected;
            using (var tee = Ymardus.Tee(kast, 22))
            {
                using (var b = new SolidBrush(r.Ostetud ? Color.FromArgb(246, 248, 249) : Teema.Paneel2)) g.FillPath(b, tee);
                if (valitud) using (var p = new Pen(Teema.Teine, 2)) g.DrawPath(p, tee);
            }
            var ring = new RectangleF(kast.X + 16, kast.Y + (kast.Height - 28) / 2, 28, 28);
            if (r.Ostetud)
            {
                using (var b = new SolidBrush(Teema.Teine)) g.FillEllipse(b, ring);
                using var p = new Pen(Color.White, 2.2f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
                g.DrawLines(p, new[] { new PointF(ring.X + 8, ring.Y + 14), new PointF(ring.X + 12, ring.Y + 18), new PointF(ring.X + 20, ring.Y + 10) });
            }
            else
            {
                using (var b = new SolidBrush(Color.White)) g.FillEllipse(b, ring);
                using var p = new Pen(Color.FromArgb(205, 211, 215), 2);
                g.DrawEllipse(p, ring);
            }
            int parem = (int)kast.Right - 14;
            using var fn = new Font("Segoe UI Semibold", 11F, r.Ostetud ? FontStyle.Strikeout : FontStyle.Regular);
            using var f = new Font("Segoe UI", 8.5F);
            using var fb = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            const TextFormatFlags V = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;
            var rida = new Rectangle((int)kast.X, (int)kast.Y, (int)kast.Width, (int)kast.Height);
            if (r.Ostis.Length > 0)
            {
                int w = TextRenderer.MeasureText(r.Ostis, fb).Width + 4;
                TextRenderer.DrawText(g, r.Ostis, fb, new Rectangle(parem - w, rida.Y, w, rida.Height), Color.FromArgb(17, 104, 58), V);
                parem -= w + 10;
            }
            if (r.Lisas.Length > 0)
            {
                var av = new RectangleF(parem - 30, kast.Y + (kast.Height - 30) / 2, 30, 30);
                using (var b = new SolidBrush(Pere.AvatariVarv(r.KasutajaId))) g.FillEllipse(b, av);
                string ini = string.Concat(r.Lisas.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x => char.ToUpper(x[0])));
                using var fa = new Font("Segoe UI", 7.5F, FontStyle.Bold);
                TextRenderer.DrawText(g, ini, fa, Rectangle.Round(av), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                parem -= 42;
            }
            string info = $"{r.Kogus} {Keel.T("tk")}" + (r.Hind.Length > 0 ? "  ·  " + r.Hind : "");
            int iw = TextRenderer.MeasureText(info, f).Width + 4;
            TextRenderer.DrawText(g, info, f, new Rectangle(parem - iw, rida.Y, iw, rida.Height), Teema.TekstNork, V);
            parem -= iw + 12;
            TextRenderer.DrawText(g, r.Toode, fn, new Rectangle((int)ring.Right + 14, rida.Y, Math.Max(40, parem - (int)ring.Right - 14), rida.Height), r.Ostetud ? Teema.TekstNork : Teema.Tekst, V);
            e.Handled = true;
        }

        public OstunimekiriVorm() : base("Ostunimekiri", 1040, 660)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimumSize = new Size(900, 540);
            BackColor = Teema.Taust;

            // ---- Pealkiri + edenemine + pere ----
            var yla = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Teema.Taust };
            lblPealkiri = new Label { Text = "Ostunimekiri", Location = new Point(18, 12), AutoSize = true, Font = UiStiil.PealkirjaFont, ForeColor = Teema.Tekst, BackColor = Teema.Taust };
            yla.Controls.Add(lblPealkiri);
            var kutsu = UiStiil.LooNupp("+ Kutsu / liitu", (s, e) => AvaPereDialoog(), true);
            ((RoundedButton)kutsu).Radius = 16;
            yla.Controls.Add(kutsu);
            yla.Controls.Add(avatarid);
            yla.Resize += (s, e) =>
            {
                kutsu.Location = new Point(yla.Width - kutsu.Width - 18, 12);
                avatarid.Location = new Point(kutsu.Left - avatarid.Width - 12, 10);
            };
            lblArv = new Label { Location = new Point(20, 50), AutoSize = true, ForeColor = Teema.Tekst2, BackColor = Teema.Taust, Font = new Font("Segoe UI Semibold", 9.5F) };
            yla.Controls.Add(lblArv);
            edenemine.Location = new Point(20, 74);
            edenemine.Size = new Size(300, 8);
            yla.Controls.Add(edenemine);
            yla.Resize += (s, e) => edenemine.Width = Math.Max(200, yla.Width - 40);

            // ---- Paremal: pere kood ja viimased tegevused (tume paneel) ----
            perePaneel = new Panel { Dock = DockStyle.Right, Width = 290, Padding = new Padding(0, 0, 16, 8), BackColor = Teema.Taust, Visible = false };
            var tume = new RoundedPanel { Dock = DockStyle.Fill, Radius = 24, FillColor = Color.FromArgb(27, 28, 31), BorderColor = Color.FromArgb(27, 28, 31), BackColor = Teema.Taust, Padding = new Padding(18, 16, 18, 16) };
            var kopeeri = (RoundedButton)UiStiil.LooNupp("Kopeeri kood", (s, e) =>
            {
                if (pereKood == null) return;
                try { Clipboard.SetText(pereKood); Teade.Toast("Pere kood kopeeritud"); } catch { }
            });
            kopeeri.Dock = DockStyle.Top;
            kopeeri.AutoSize = false;
            kopeeri.Height = 40;
            kopeeri.Radius = 20;
            koodiPlaadid.Dock = DockStyle.Top;
            tegevusteLoend.Dock = DockStyle.Fill;
            tume.Controls.Add(tegevusteLoend);
            tume.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 18, BackColor = Color.FromArgb(27, 28, 31) });
            tume.Controls.Add(kopeeri);
            tume.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Color.FromArgb(27, 28, 31) });
            tume.Controls.Add(koodiPlaadid);
            perePaneel.Controls.Add(tume);

            // ---- Lisamise rida ----
            var lisa = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Teema.Taust, Padding = new Padding(16, 6, 16, 6) };
            var raam = UiStiil.LooSisend(out txtToode, 380, false, "Mida osta? (nt Piim või toote nimi)");
            raam.Location = new Point(18, 10);
            numKogus = new NumericUpDown { Location = new Point(410, 14), Width = 64, Minimum = 1, Maximum = 99, Value = 1, Font = new Font("Segoe UI", 10F) };
            var lisaNupp = UiStiil.LooSuurNupp("Lisa", (s, e) => LisaRida());
            lisaNupp.Bounds = new Rectangle(488, 10, 110, 38);
            lisa.Controls.AddRange(new Control[] { raam, numKogus, lisaNupp });
            lisa.Resize += (s, e) =>
            {
                lisaNupp.Left = lisa.Width - lisaNupp.Width - 18;
                numKogus.Left = lisaNupp.Left - numKogus.Width - 12;
                raam.Width = Math.Max(200, numKogus.Left - 12 - raam.Left);
            };
            txtToode.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { LisaRida(); e.SuppressKeyPress = true; } };

            // Toodete nimed automaatseks täitmiseks
            try
            {
                using var db = new ToodedContext();
                var nimed = new AutoCompleteStringCollection();
                nimed.AddRange(db.Toodetabel.AsNoTracking().Select(t => t.Toodenimetus).ToArray());
                txtToode.AutoCompleteCustomSource = nimed;
                txtToode.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txtToode.AutoCompleteSource = AutoCompleteSource.CustomSource;
            }
            catch { }

            // ---- Filter ----
            var filter = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 34, BackColor = Teema.Taust, Padding = new Padding(16, 4, 0, 0) };
            rbKoik = new RadioButton { Text = "Kõik", AutoSize = true, Checked = true, ForeColor = Teema.Tekst, Margin = new Padding(0, 0, 18, 0) };
            rbOstmata = new RadioButton { Text = "Ostmata", AutoSize = true, ForeColor = Teema.Tekst, Margin = new Padding(0, 0, 18, 0) };
            rbOstetud = new RadioButton { Text = "Ostetud", AutoSize = true, ForeColor = Teema.Tekst };
            foreach (var rb in new[] { rbKoik, rbOstmata, rbOstetud })
            {
                rb.CheckedChanged += (s, e) => { if (((RadioButton)s!).Checked) Naita(); };
                filter.Controls.Add(rb);
            }

            // ---- Tabel ----
            vork = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            vork.CellClick += (s, e) =>
            {
                if (!Teema.OnMoodne && e.RowIndex >= 0 && e.ColumnIndex >= 0 && vork.Columns[e.ColumnIndex].Name == "Ostetud")
                    Vaheta(e.RowIndex);
            };
            vork.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0 && vork.Columns[e.ColumnIndex].Name != "Ostetud") Vaheta(e.RowIndex); };
            vork.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Space && vork.CurrentRow != null) { Vaheta(vork.CurrentRow.Index); e.Handled = true; }
                if (e.KeyCode == Keys.Delete) { KustutaValitud(); e.Handled = true; }
            };
            // Ümar linnuke "Ostetud" veerus
            vork.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || vork.Columns[e.ColumnIndex].Name != "Ostetud" || e.Graphics == null) return;
                e.PaintBackground(e.CellBounds, true);
                bool on = e.Value is true;
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = new RectangleF(e.CellBounds.X + (e.CellBounds.Width - 20) / 2f, e.CellBounds.Y + (e.CellBounds.Height - 20) / 2f, 20, 20);
                if (on)
                {
                    using var b = new SolidBrush(Teema.Edu);
                    g.FillEllipse(b, r);
                    using var p = new Pen(Color.White, 2f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round };
                    g.DrawLines(p, new[] { new PointF(r.X + 5, r.Y + 10), new PointF(r.X + 8.5f, r.Y + 13.5f), new PointF(r.X + 15, r.Y + 6.5f) });
                }
                else
                {
                    using var p = new Pen(Teema.Raam, 1.6f);
                    g.DrawEllipse(p, r);
                }
                e.Handled = true;
            };
            vork.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || vork.Rows[e.RowIndex].DataBoundItem is not NimekirjaVaade r) return;
                if (r.Ostetud && e.CellStyle != null)
                {
                    e.CellStyle.Font = labi;
                    e.CellStyle.ForeColor = Teema.TekstNork;
                }
                if (e.CellStyle != null && vork.Columns[e.ColumnIndex].Name == "Ostis")
                {
                    e.CellStyle.ForeColor = Teema.Edu;
                    e.CellStyle.Font = vork.Font;
                }
            };
            if (Teema.OnMoodne)
            {
                // lõuend N5: iga rida on hall ümar "nupp" – ring vasakul (klõps = ostetud), lisaja avatar, "ostis …"
                vork.ColumnHeadersVisible = false;
                vork.CellBorderStyle = DataGridViewCellBorderStyle.None;
                vork.RowTemplate.Height = 64;
                vork.RowPrePaint += MoodneRida;
                vork.RowsAdded += (s, e) => { for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++) vork.Rows[i].Height = 64; };
                vork.MouseClick += (s, e) =>
                {
                    var h = vork.HitTest(e.X, e.Y);
                    if (h.RowIndex >= 0 && e.X < 64) Vaheta(h.RowIndex);
                };
            }
            var tabeliKaart = UiStiil.LooKaardiPaneel("Nimekiri  (klõps ruudul või tühik = ostetud / ostmata)", vork);
            var keskel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 8), BackColor = Teema.Taust };
            tabeliKaart.Dock = DockStyle.Fill;
            keskel.Controls.Add(tabeliKaart);

            // ---- Nupud ----
            var nupud = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 56, BackColor = Teema.Taust, Padding = new Padding(16, 6, 16, 8) };
            nupud.Controls.Add(UiStiil.LooNupp("Lisa ostmata korvi", (s, e) => LisaKorvi(), true));
            nupud.Controls.Add(UiStiil.LooNupp("Märgi valitud ostetuks", (s, e) => MargiValitud(true)));
            nupud.Controls.Add(UiStiil.LooNupp("Ostmata", (s, e) => MargiValitud(false)));
            var kustuta = (RoundedButton)UiStiil.LooNupp("Kustuta valitud", (s, e) => KustutaValitud());
            kustuta.Ohtlik = true;
            nupud.Controls.Add(kustuta);
            nupud.Controls.Add(UiStiil.LooNupp("Kustuta ostetud", (s, e) => KustutaOstetud()));

            Controls.Add(keskel);
            Controls.Add(perePaneel);
            Controls.Add(nupud);
            Controls.Add(filter);
            Controls.Add(lisa);
            Controls.Add(yla);
            keskel.BringToFront();

            // Moodne (lõuend 2.4): nimekiri valgel ümaral kaardil, pere paneel paremal
            if (Teema.OnMoodne)
            {
                filter.Height = 50;   // "pill" valikud on kõrgemad
                MoodneStiil.KaardiSisse(this, yla, lisa, filter, keskel, nupud);
                lblPealkiri.Font = new Font("Segoe UI", 19F, FontStyle.Bold);
                tabeliKaart.BorderColor = tabeliKaart.FillColor;
                if (nupud.Controls.Count > 0 && nupud.Controls[0] is RoundedButton peamine)
                {
                    peamine.AutoSize = false;
                    peamine.Size = new Size(230, 46);
                    peamine.TextAlign = ContentAlignment.MiddleLeft;
                    MoodneUi.NoolStiil(peamine);
                }
            }

            Load += (s, e) => Laadi();
            Shown += (s, e) => txtToode.Focus();
            FormClosed += (s, e) => { labi.Dispose(); elav.Stop(); elav.Dispose(); };

            // "Elav" pere nimekiri: teiste muudatused tulevad iga 5 s järel nähtavale
            elav.Tick += (s, e) => { if (pereKood != null && !vork.IsCurrentCellInEditMode && ContainsFocus) Laadi(); };
            elav.Start();
        }

        private void Laadi()
        {
            try
            {
                using var db = new ToodedContext();
                var pere = Pere.NahtavadKasutajad(db);
                liikmed = Pere.Liikmed(db);
                pereKood = Pere.MinuKood(db);
                var nimed = db.Kasutajad.AsNoTracking().Where(k => pere.Contains(k.Id)).ToDictionary(k => k.Id, Pere.Nimi);
                var read = db.Ostunimekiri.AsNoTracking()
                    .Where(r => pere.Contains(r.KasutajaId))
                    .ToList();
                tegevused = read.Select(r => new Tegevus(r.Lisatud, r.KasutajaId, nimed.GetValueOrDefault(r.KasutajaId, "?"), Keel.F("lisas „{0}“", r.Tekst)))
                    .Concat(read.Where(r => r.Ostetud && r.OstetudAeg != null).Select(r => new Tegevus(r.OstetudAeg!.Value, r.OstisKasutajaId ?? r.KasutajaId,
                        nimed.GetValueOrDefault(r.OstisKasutajaId ?? r.KasutajaId, "?"), Keel.F("ostis „{0}“", r.Tekst))))
                    .OrderByDescending(t => t.Aeg).Take(8).ToList();
                var ids = read.Where(r => r.ToodeId != null).Select(r => r.ToodeId!.Value).ToList();
                var tooted = db.Toodetabel.AsNoTracking().Where(t => ids.Contains(t.Id)).ToDictionary(t => t.Id);

                koik = read
                    .OrderBy(r => r.Ostetud).ThenByDescending(r => r.Lisatud)
                    .Select(r =>
                    {
                        Toode? t = r.ToodeId is int id && tooted.TryGetValue(id, out var x) ? x : null;
                        return new NimekirjaVaade
                        {
                            Id = r.Id,
                            Ostetud = r.Ostetud,
                            Toode = r.Tekst,
                            Kogus = r.Kogus,
                            Hind = t != null ? Ostukorv.Eur(t.Hind * r.Kogus) : "",
                            Laos = t == null ? "" : t.Kogus > 0 ? Keel.F("{0} tk", t.Kogus) : Keel.T("Pole laos"),
                            Lisatud = r.Lisatud,
                            Lisas = nimed.GetValueOrDefault(r.KasutajaId, ""),
                            Ostis = r.Ostetud && r.OstisKasutajaId is int o ? Keel.F("ostis {0}", nimed.GetValueOrDefault(o, "?")) : "",
                            ToodeId = r.ToodeId,
                            KasutajaId = r.KasutajaId
                        };
                    })
                    .ToList();
            }
            catch (Exception ex) { Teade.Show("Andmete laadimine ebaõnnestus:\n" + ex.Message); koik = new(); }
            Naita();
        }

        private void Naita()
        {
            var nimekiri = koik.Where(r => rbKoik.Checked || (rbOstetud.Checked ? r.Ostetud : !r.Ostetud)).ToList();
            int rida = vork.CurrentRow?.Index ?? -1;
            vork.DataSource = nimekiri;
            if (vork.Columns["Id"] is DataGridViewColumn id) id.Visible = false;
            if (vork.Columns["Ostetud"] is DataGridViewColumn o) { o.HeaderText = "✓"; o.FillWeight = 22; o.MinimumWidth = 40; }
            if (vork.Columns["Toode"] is DataGridViewColumn t) t.FillWeight = 220;
            if (vork.Columns["Kogus"] is DataGridViewColumn k) k.FillWeight = 40;
            if (vork.Columns["Hind"] is DataGridViewColumn h) { h.FillWeight = 55; h.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; }
            if (vork.Columns["Laos"] is DataGridViewColumn l) l.FillWeight = 55;
            if (vork.Columns["Lisatud"] is DataGridViewColumn d) { d.DefaultCellStyle.Format = "dd.MM HH:mm"; d.FillWeight = 60; }
            vork.ClearSelection();
            if (rida >= 0 && rida < vork.Rows.Count) vork.CurrentCell = vork.Rows[rida].Cells["Toode"];

            int ostetud = koik.Count(r => r.Ostetud);
            lblArv.Text = koik.Count == 0
                ? Keel.T("Nimekiri on tühi – lisa ülal, mida on vaja osta.")
                : Keel.F("Ostetud {0} / {1}   ·   ostmata {2}", ostetud, koik.Count, koik.Count - ostetud);
            edenemine.Osa = koik.Count == 0 ? 0 : (float)ostetud / koik.Count;

            // Pere: pealkiri, liikmed, kood, viimased tegevused
            bool peres = pereKood != null;
            lblPealkiri.Text = Keel.T(peres ? "Pere ostunimekiri" : "Ostunimekiri");
            if (peres && koik.Count > 0) lblArv.Text = Keel.F("Ostetud {0} / {1} · kõik näevad sama nimekirja", ostetud, koik.Count);
            perePaneel.Visible = peres;
            avatarid.Naita(liikmed);
            koodiPlaadid.Naita(pereKood);
            tegevusteLoend.Naita(tegevused.Select(t => (t.KasutajaId, t.Nimi, t.Tekst, t.Aeg)).ToList());
            if (vork.Columns["Lisas"] is DataGridViewColumn ls) { ls.Visible = peres; ls.FillWeight = 50; }
            if (vork.Columns["Ostis"] is DataGridViewColumn os) { os.Visible = peres; os.FillWeight = 60; }
        }

        // Pere loomine / liitumine / lahkumine
        private void AvaPereDialoog()
        {
            var d = new PereDialoog(pereKood != null);
            Aken.Ava(this, d, () => { if (d.Muudetud) Laadi(); });
        }

        private void LisaRida()
        {
            string tekst = txtToode.Text.Trim();
            if (tekst.Length == 0) { txtToode.Focus(); return; }
            try
            {
                Ostunimekiri.Lisa(tekst, (int)numKogus.Value);
                txtToode.Clear();
                numKogus.Value = 1;
                txtToode.Focus();
                Laadi();
            }
            catch (Exception ex) { Teade.Show("Viga lisamisel:\n" + ex.Message); }
        }

        private void Vaheta(int rida)
        {
            if (vork.Rows[rida].DataBoundItem is not NimekirjaVaade v) return;
            Muuda(new[] { v.Id }, r => { r.Ostetud = !r.Ostetud; r.OstetudAeg = r.Ostetud ? DateTime.Now : null; r.OstisKasutajaId = r.Ostetud ? AktiivneKasutaja.Id : null; });
        }

        private List<int> ValitudIdd()
        {
            var ids = vork.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem).OfType<NimekirjaVaade>().Select(v => v.Id).ToList();
            if (ids.Count == 0 && vork.CurrentRow?.DataBoundItem is NimekirjaVaade c) ids.Add(c.Id);
            return ids;
        }

        private void MargiValitud(bool ostetud)
        {
            var ids = ValitudIdd();
            if (ids.Count == 0) { Teade.Show("Vali nimekirjast rida."); return; }
            Muuda(ids, r => { r.Ostetud = ostetud; r.OstetudAeg = ostetud ? DateTime.Now : null; r.OstisKasutajaId = ostetud ? AktiivneKasutaja.Id : null; });
        }

        private void Muuda(IEnumerable<int> ids, Action<OstunimekirjaRida> muutus)
        {
            try
            {
                var list = ids.ToList();
                using var db = new ToodedContext();
                var pere = Pere.NahtavadKasutajad(db);
                foreach (var r in db.Ostunimekiri.Where(r => list.Contains(r.Id) && pere.Contains(r.KasutajaId)))
                    muutus(r);
                db.SaveChanges();
                Ostunimekiri.TeavitaMuutusest();
                Laadi();
            }
            catch (Exception ex) { Teade.Show("Salvestamine ebaõnnestus:\n" + ex.Message); }
        }

        private void KustutaValitud()
        {
            var ids = ValitudIdd();
            if (ids.Count == 0) { Teade.Show("Vali nimekirjast rida."); return; }
            Kustuta(db => { var pere = Pere.NahtavadKasutajad(db); return db.Ostunimekiri.Where(r => ids.Contains(r.Id) && pere.Contains(r.KasutajaId)); });
        }

        private void KustutaOstetud()
        {
            if (!koik.Any(r => r.Ostetud)) { Teade.Toast("Ostetud ridu pole", TeateTyyp.Info); return; }
            Kustuta(db => { var pere = Pere.NahtavadKasutajad(db); return db.Ostunimekiri.Where(r => r.Ostetud && pere.Contains(r.KasutajaId)); });
        }

        private void Kustuta(Func<ToodedContext, IQueryable<OstunimekirjaRida>> paring)
        {
            try
            {
                using var db = new ToodedContext();
                db.Ostunimekiri.RemoveRange(paring(db).ToList());
                db.SaveChanges();
                Ostunimekiri.TeavitaMuutusest();
                Laadi();
            }
            catch (Exception ex) { Teade.Show("Salvestamine ebaõnnestus:\n" + ex.Message); }
        }

        // Ostmata tooted (mis on poes olemas) lähevad ostukorvi
        private void LisaKorvi()
        {
            var ostmata = koik.Where(r => !r.Ostetud && r.ToodeId != null).ToList();
            if (ostmata.Count == 0)
            {
                Teade.Show("Nimekirjas pole ostmata tooteid, mis on poes olemas.\nVali toode nimekirja lisamisel soovitustest.");
                return;
            }
            try
            {
                using var db = new ToodedContext();
                var ids = ostmata.Select(r => r.ToodeId!.Value).ToList();
                var tooted = db.Toodetabel.AsNoTracking().Where(t => ids.Contains(t.Id)).ToDictionary(t => t.Id);
                int lisatud = 0;
                var vead = new List<string>();
                foreach (var r in ostmata)
                {
                    if (!tooted.TryGetValue(r.ToodeId!.Value, out var t)) continue;
                    string? viga = t.Kogus <= 0 ? Keel.F("„{0}“ on otsas.", t.Toodenimetus)
                        : Ostukorv.LisaToode(t, r.Kogus);
                    if (viga == null) lisatud++; else vead.Add(viga);
                }
                if (lisatud > 0) Teade.Toast(Keel.F("Korvi lisati {0} toodet", lisatud), TeateTyyp.Edu);
                if (vead.Count > 0) Teade.Toast(string.Join("\n", vead.Take(4)), TeateTyyp.Hoiatus, 5000);
            }
            catch (Exception ex) { Teade.Show("Viga:\n" + ex.Message); }
        }
    }

    // Õhuke ümardatud edenemisriba (mitu protsenti nimekirjast on ostetud)
    public class Edenemine : Control
    {
        private float osa;
        public float Osa { get => osa; set { osa = Math.Clamp(value, 0, 1); Invalidate(); } }

        public Edenemine()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            if (r.Width <= 0 || r.Height <= 0) return;
            using (var taust = new SolidBrush(Teema.Paneel2))
            using (var tee = Ymardus.Tee(r, Height / 2))
                g.FillPath(taust, tee);
            int w = (int)(r.Width * osa);
            if (w >= Height)
            {
                using var taidis = new SolidBrush(Teema.Edu);
                using var tee = Ymardus.Tee(new Rectangle(0, 0, w, r.Height), Height / 2);
                g.FillPath(taidis, tee);
            }
        }
    }
}
