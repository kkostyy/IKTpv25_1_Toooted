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

            int kasutajaId = AktiivneKasutaja.Id;
            var ostmata = db.Ostunimekiri.Where(r => r.KasutajaId == kasutajaId && !r.Ostetud);
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
                var read = db.Ostunimekiri
                    .Where(r => r.KasutajaId == AktiivneKasutaja.Id && !r.Ostetud &&
                                ((r.ToodeId != null && ids.Contains(r.ToodeId)) || nimed.Contains(r.Tekst)))
                    .ToList();
                foreach (var r in read) { r.Ostetud = true; r.OstetudAeg = DateTime.Now; }
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
                return db.Ostunimekiri.Count(r => r.KasutajaId == AktiivneKasutaja.Id && !r.Ostetud);
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
        internal int? ToodeId;
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

        public OstunimekiriVorm() : base("Ostunimekiri", 760, 640)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimumSize = new Size(640, 520);
            BackColor = Teema.Taust;

            // ---- Pealkiri + edenemine ----
            var yla = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Teema.Taust };
            yla.Controls.Add(new Label { Text = "Ostunimekiri", Location = new Point(18, 12), AutoSize = true, Font = UiStiil.PealkirjaFont, ForeColor = Teema.Tekst, BackColor = Teema.Taust });
            lblArv = new Label { Location = new Point(20, 50), AutoSize = true, ForeColor = Teema.Tekst2, BackColor = Teema.Taust, Font = new Font("Segoe UI Semibold", 9.5F) };
            yla.Controls.Add(lblArv);
            edenemine.Location = new Point(20, 74);
            edenemine.Size = new Size(300, 8);
            yla.Controls.Add(edenemine);
            yla.Resize += (s, e) => edenemine.Width = Math.Max(200, yla.Width - 40);

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
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && vork.Columns[e.ColumnIndex].Name == "Ostetud")
                    Vaheta(e.RowIndex);
            };
            vork.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0 && vork.Columns[e.ColumnIndex].Name != "Ostetud") Vaheta(e.RowIndex); };
            vork.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Space && vork.CurrentRow != null) { Vaheta(vork.CurrentRow.Index); e.Handled = true; }
                if (e.KeyCode == Keys.Delete) { KustutaValitud(); e.Handled = true; }
            };
            vork.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || vork.Rows[e.RowIndex].DataBoundItem is not NimekirjaVaade r) return;
                if (r.Ostetud && e.CellStyle != null)
                {
                    e.CellStyle.Font = labi;
                    e.CellStyle.ForeColor = Teema.TekstNork;
                }
            };
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
            Controls.Add(nupud);
            Controls.Add(filter);
            Controls.Add(lisa);
            Controls.Add(yla);
            keskel.BringToFront();

            Load += (s, e) => Laadi();
            Shown += (s, e) => txtToode.Focus();
            FormClosed += (s, e) => labi.Dispose();
        }

        private void Laadi()
        {
            try
            {
                using var db = new ToodedContext();
                var read = db.Ostunimekiri.AsNoTracking()
                    .Where(r => r.KasutajaId == AktiivneKasutaja.Id)
                    .ToList();
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
                            ToodeId = r.ToodeId
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
            Muuda(new[] { v.Id }, r => { r.Ostetud = !r.Ostetud; r.OstetudAeg = r.Ostetud ? DateTime.Now : null; });
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
            Muuda(ids, r => { r.Ostetud = ostetud; r.OstetudAeg = ostetud ? DateTime.Now : null; });
        }

        private void Muuda(IEnumerable<int> ids, Action<OstunimekirjaRida> muutus)
        {
            try
            {
                var list = ids.ToList();
                using var db = new ToodedContext();
                foreach (var r in db.Ostunimekiri.Where(r => list.Contains(r.Id) && r.KasutajaId == AktiivneKasutaja.Id))
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
            Kustuta(db => db.Ostunimekiri.Where(r => ids.Contains(r.Id) && r.KasutajaId == AktiivneKasutaja.Id));
        }

        private void KustutaOstetud()
        {
            if (!koik.Any(r => r.Ostetud)) { Teade.Toast("Ostetud ridu pole", TeateTyyp.Info); return; }
            Kustuta(db => db.Ostunimekiri.Where(r => r.Ostetud && r.KasutajaId == AktiivneKasutaja.Id));
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
                        : Ostukorv.Lisa(t.Id, t.Toodenimetus, t.Hind, t.Kogus, r.Kogus, t.Pilt);
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
