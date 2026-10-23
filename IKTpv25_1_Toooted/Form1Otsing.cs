namespace IKTpv25_1_Toooted
{
    // Üks rida toodete tabelis (DataGridView näitab neid omadusi veergudena)
    public class TooteVaade
    {
        public int Id { get; set; }
        public string Toodenimetus { get; set; } = "";
        public int Kogus { get; set; }
        public decimal Hind { get; set; }
        public string? Pilt { get; set; }
        public int KategooriaId { get; set; }
        public string Kategooria { get; set; } = "";
    }

    // Toodete otsing: nime järgi ja kategooria järgi (filtreerimine mälus)
    public partial class Form1
    {
        private const string KoikKategooriad = "Kõik kategooriad";

        private readonly TextBox txtOtsi = new();
        private readonly ComboBox cmbOtsiKategooria = new();
        private readonly Label lblLeitud = new();
        private List<TooteVaade> koikTooted = new();

        private void LooOtsing()
        {
            var riba = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(0, 2, 0, 4)
            };

            txtOtsi.Width = 260;
            txtOtsi.PlaceholderText = "Otsi toodet nime järgi...";
            txtOtsi.Margin = new Padding(0, 6, 10, 0);
            txtOtsi.TextChanged += (s, e) => NaitaTooted();

            cmbOtsiKategooria.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOtsiKategooria.Width = 180;
            cmbOtsiKategooria.Margin = new Padding(0, 5, 10, 0);
            cmbOtsiKategooria.Items.Add(Keel.T(KoikKategooriad));
            cmbOtsiKategooria.SelectedIndex = 0;
            cmbOtsiKategooria.SelectedIndexChanged += (s, e) => NaitaTooted();

            var tyhjenda = UiStiil.LooNupp("Tühjenda otsing", (s, e) =>
            {
                txtOtsi.Clear();
                cmbOtsiKategooria.SelectedIndex = 0;
            });

            lblLeitud.AutoSize = true;
            lblLeitud.Margin = new Padding(10, 10, 0, 0);
            lblLeitud.ForeColor = Color.FromArgb(110, 116, 125);
            lblLeitud.BackColor = Color.White;

            riba.Controls.AddRange(new Control[] { txtOtsi, cmbOtsiKategooria, tyhjenda, lblLeitud });

            // Pealkirja alla, tabeli kohale (dokkimise järjekord: pealkiri, otsing, tabel)
            kaartTabel.Controls.Add(riba);
            kaartTabel.Controls.SetChildIndex(riba, 1);
        }

        // Kategooriate valik otsingus vastavalt olemasolevatele toodetele
        private void UuendaOtsinguKategooriad()
        {
            string? valitud = cmbOtsiKategooria.SelectedItem as string;

            cmbOtsiKategooria.BeginUpdate();
            cmbOtsiKategooria.Items.Clear();
            cmbOtsiKategooria.Items.Add(Keel.T(KoikKategooriad));
            foreach (string k in koikTooted.Select(t => t.Kategooria).Distinct().OrderBy(k => k))
                cmbOtsiKategooria.Items.Add(k);
            cmbOtsiKategooria.EndUpdate();

            int i = valitud != null ? cmbOtsiKategooria.Items.IndexOf(valitud) : -1;
            cmbOtsiKategooria.SelectedIndex = i >= 0 ? i : 0;
        }

        // Näitab tabelis ainult otsingule vastavaid tooteid
        private void NaitaTooted()
        {
            IEnumerable<TooteVaade> tulemus = koikTooted;

            string otsi = txtOtsi.Text.Trim();
            if (otsi.Length > 0)
                tulemus = tulemus.Where(t => t.Toodenimetus.Contains(otsi, StringComparison.CurrentCultureIgnoreCase));

            if (cmbOtsiKategooria.SelectedIndex > 0 && cmbOtsiKategooria.SelectedItem is string kategooria)
                tulemus = tulemus.Where(t => t.Kategooria == kategooria);

            var nimekiri = tulemus.ToList();
            dataGridTooted.DataSource = nimekiri;

            if (dataGridTooted.Columns["Id"] is DataGridViewColumn id) id.Visible = false;
            if (dataGridTooted.Columns["KategooriaId"] is DataGridViewColumn kid) kid.Visible = false;
            if (dataGridTooted.Columns["Toodenimetus"] is DataGridViewColumn nimi) nimi.HeaderText = Keel.T("Toode");
            if (dataGridTooted.Columns["Hind"] is DataGridViewColumn hind) hind.DefaultCellStyle.Format = "0.00";
            dataGridTooted.ClearSelection();

            lblLeitud.Text = Keel.F("Leitud: {0} / {1}", nimekiri.Count, koikTooted.Count);
        }
    }
}
