using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace IKTpv25_1_Toooted;

// ===================================================================
//  Autotellimus (omanik): mida tarnijatelt juurde tellida müügitempo ja
//  laoseisu järgi. Kogust saab tabelis muuta; eksport CSV ja PDF tarnijale.
// ===================================================================
public class AutotellimusVorm : KujundusVorm
{
    private static readonly CultureInfo Eesti = new("et-EE");

    private readonly DataGridView tabel;
    private readonly ComboBox cmbTarnija;
    private readonly Label lblKokku;
    private readonly MoodneFilter kate;
    private List<Autotellimus.Rida> read = new();
    private int katePaevi = 14;

    public AutotellimusVorm() : base("Autotellimus", 1100, 720)
    {
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimumSize = new Size(860, 520);
        BackColor = Teema.Taust;

        var yla = new Panel { Dock = DockStyle.Top, Height = 140, BackColor = Teema.Taust };
        yla.Controls.Add(new Label { Text = "Autotellimus", Location = new Point(18, 14), AutoSize = true, Font = UiStiil.PealkirjaFont, ForeColor = Teema.Tekst, BackColor = Teema.Taust });
        yla.Controls.Add(new Label
        {
            Text = Keel.F("Müügitempo viimase {0} päeva järgi + {1} päeva varu. Kogust saab tabelis muuta.", Autotellimus.Ajalugu, Autotellimus.Varu),
            Location = new Point(20, 52), AutoSize = true, ForeColor = Teema.TekstNork, BackColor = Teema.Taust
        });

        var kateSilt = new Label { Text = "Kaup peab jätkuma", Location = new Point(20, 98), AutoSize = true, ForeColor = Teema.Tekst2, BackColor = Teema.Taust, Font = new Font("Segoe UI Semibold", 9.5F) };
        yla.Controls.Add(kateSilt);
        kate = new MoodneFilter { Location = new Point(160, 84) };
        // Tagasi-nupp nihutab päise elemente – filter jääb alati sildi järele
        kateSilt.LocationChanged += (s, e) => kate.Location = new Point(kateSilt.Right + 12, kateSilt.Top - 12);
        kateSilt.SizeChanged += (s, e) => kate.Location = new Point(kateSilt.Right + 12, kateSilt.Top - 12);
        kate.Sea(new[] { (Keel.F("{0} päeva", 7), -1), (Keel.F("{0} päeva", 14), -1), (Keel.F("{0} päeva", 30), -1) }, 1);
        kate.Valitud += i => { katePaevi = new[] { 7, 14, 30 }[i]; Laadi(); };
        yla.Controls.Add(kate);

        cmbTarnija = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220, Font = new Font("Segoe UI", 10F), Tag = "ei-jalgi" };
        cmbTarnija.SelectedIndexChanged += (s, e) => NaitaTabel();
        yla.Controls.Add(cmbTarnija);
        var csv = UiStiil.LooNupp("Ekspordi CSV", (s, e) => EkspordiCsv());
        var pdf = UiStiil.LooNupp("Ekspordi PDF", (s, e) => EkspordiPdf(), true);
        yla.Controls.Add(csv);
        yla.Controls.Add(pdf);
        yla.Resize += (s, e) =>
        {
            pdf.Location = new Point(yla.Width - pdf.Width - 18, 90);
            csv.Location = new Point(pdf.Left - csv.Width - 10, 90);
            cmbTarnija.Location = new Point(csv.Left - cmbTarnija.Width - 14, 94);
        };

        var kaart = new RoundedPanel { Dock = DockStyle.Fill, Radius = 16, FillColor = Teema.Pind, BorderColor = Teema.Pind, BackColor = Teema.Taust, Padding = new Padding(14) };
        tabel = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Teema.Pind,
            BorderStyle = BorderStyle.None,
            EditMode = DataGridViewEditMode.EditOnEnter
        };
        foreach (var (nimi, pais, kaal, ainultLugeda) in new[]
        {
            ("Tarnija", "Tarnija", 120, true), ("Toode", "Toode", 260, true), ("Laos", "Laos", 60, true),
            ("Paevas", "Müük päevas", 80, true), ("Jatkub", "Jätkub (päeva)", 90, true), ("Telli", "Telli (tk)", 70, false),
            ("Ostuhind", "Ostuhind", 80, true), ("Summa", "Summa", 80, true)
        })
            tabel.Columns.Add(new DataGridViewTextBoxColumn { Name = nimi, HeaderText = pais, FillWeight = kaal, ReadOnly = ainultLugeda });
        tabel.CellValidating += (s, e) =>
        {
            if (tabel.Columns[e.ColumnIndex].Name == "Telli" && (!int.TryParse(e.FormattedValue?.ToString(), out int v) || v < 0))
            {
                e.Cancel = true;
                Teade.Toast("Kogus peab olema täisarv (0 või rohkem).", TeateTyyp.Hoiatus);
            }
        };
        tabel.CellEndEdit += (s, e) =>
        {
            if (tabel.Rows[e.RowIndex].Tag is Autotellimus.Rida r && int.TryParse(tabel.Rows[e.RowIndex].Cells["Telli"].Value?.ToString(), out int v))
            {
                r.Telli = v;
                tabel.Rows[e.RowIndex].Cells["Summa"].Value = Ostukorv.Eur(r.Summa);
                UuendaKokku();
            }
        };
        tabel.CellFormatting += (s, e) =>
        {
            if (e.RowIndex >= 0 && tabel.Columns[e.ColumnIndex].Name == "Jatkub" && tabel.Rows[e.RowIndex].Tag is Autotellimus.Rida r)
                e.CellStyle!.ForeColor = r.PaeviJatkub < 3 ? Teema.Viga : r.PaeviJatkub < 7 ? Teema.Hoiatus : Teema.Tekst2;
        };
        kaart.Controls.Add(tabel);

        var all = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Teema.Taust };
        lblKokku = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 22, 0), Font = new Font("Segoe UI Semibold", 13F), ForeColor = Teema.Tekst, BackColor = Teema.Taust };
        all.Controls.Add(lblKokku);

        var keskel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 0), BackColor = Teema.Taust };
        keskel.Controls.Add(kaart);
        Controls.Add(keskel);
        Controls.Add(all);
        Controls.Add(yla);
        keskel.BringToFront();

        Load += (s, e) => { Teema.StiliseeriTabel(tabel); Laadi(); };
    }

    private void Laadi()
    {
        try
        {
            using var db = new ToodedContext();
            var ostud = Myygianalyys.Ostud(db, DateTime.Today.AddDays(-Autotellimus.Ajalugu), DateTime.Now.AddMinutes(1));
            var tooted = db.Toodetabel.AsNoTracking().ToList();
            read = Autotellimus.Arvuta(ostud, tooted, katePaevi);
        }
        catch (Exception ex) { Teade.Show("Andmete laadimine ebaõnnestus:\n" + ex.Message); read = new(); }

        string? valitud = cmbTarnija.SelectedIndex > 0 ? cmbTarnija.SelectedItem as string : null;
        cmbTarnija.Items.Clear();
        cmbTarnija.Items.Add(Keel.T("Kõik tarnijad"));
        foreach (var t in read.Select(r => r.Tarnija).Distinct()) cmbTarnija.Items.Add(t);   // tarnija nimi tõlkimata (filtri võti)
        int i = valitud != null ? cmbTarnija.Items.IndexOf(valitud) : 0;
        cmbTarnija.SelectedIndex = Math.Max(0, i);   // käivitab NaitaTabel
    }

    private IEnumerable<Autotellimus.Rida> Valitud =>
        cmbTarnija.SelectedIndex > 0 ? read.Where(r => r.Tarnija == (string)cmbTarnija.SelectedItem!) : read;

    private void NaitaTabel()
    {
        tabel.Rows.Clear();
        foreach (var r in Valitud)
        {
            int n = tabel.Rows.Add(Keel.T(r.Tarnija), r.Toode.Toodenimetus, r.Toode.Kogus, r.PaevasTk.ToString("0.0", Eesti),
                r.Toode.Kogus <= 0 ? Keel.T("otsas") : r.PaeviJatkub.ToString("0.0", Eesti), r.Telli, Ostukorv.Eur(r.Ostuhind), Ostukorv.Eur(r.Summa));
            tabel.Rows[n].Tag = r;
        }
        UuendaKokku();
    }

    private void UuendaKokku()
    {
        var v = Valitud.Where(r => r.Telli > 0).ToList();
        lblKokku.Text = v.Count == 0
            ? Keel.T("Praegu pole midagi vaja tellida – laost jätkub.")
            : Keel.F("{0} toodet · {1} tk · tellimuse summa {2}", v.Count, v.Sum(r => r.Telli), Ostukorv.Eur(v.Sum(r => r.Summa)));
    }

    private List<Autotellimus.Rida> Tellida()
    {
        tabel.EndEdit();
        var v = Valitud.Where(r => r.Telli > 0).ToList();
        if (v.Count == 0) Teade.Show("Tellimuses pole ühtegi toodet.");
        return v;
    }

    private string? KysiFail(string laiend, string filter)
    {
        string nimi = cmbTarnija.SelectedIndex > 0 ? Failinimi((string)cmbTarnija.SelectedItem!) : "koik";
        using var dlg = new SaveFileDialog
        {
            FileName = $"Tellimus_{nimi}_{DateTime.Now:yyyyMMdd}.{laiend}",
            Filter = filter,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };
        return dlg.ShowDialog(this) == DialogResult.OK ? dlg.FileName : null;
    }

    private static string Failinimi(string s) => new string(s.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());

    private void Valmis(string fail)
    {
        if (Teade.Show("Fail salvestatud:\n" + fail + "\n\nKas avan?", "Eksport", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            Arve.Ava(fail);
    }

    private void EkspordiCsv()
    {
        var v = Tellida();
        if (v.Count == 0 || KysiFail("csv", "CSV (Excel)|*.csv") is not string fail) return;
        var sb = new StringBuilder();
        sb.AppendLine("Tarnija;Toode;Telli (tk);Ostuhind (€);Summa (€);Laos;Müük päevas");
        foreach (var r in v)
            sb.AppendLine(string.Join(";", new[]
            {
                r.Tarnija, r.Toode.Toodenimetus, r.Telli.ToString(), r.Ostuhind.ToString("0.00", Eesti), r.Summa.ToString("0.00", Eesti),
                r.Toode.Kogus.ToString(), r.PaevasTk.ToString("0.0", Eesti)
            }.Select(x => x.Contains(';') || x.Contains('"') ? "\"" + x.Replace("\"", "\"\"") + "\"" : x)));
        try
        {
            File.WriteAllText(fail, sb.ToString(), new UTF8Encoding(true));
            Valmis(fail);
        }
        catch (Exception ex) { Teade.Show("Salvestamine ebaõnnestus:\n" + ex.Message); }
    }

    // PDF: iga tarnija oma lehel (A4), tabel + summa
    private void EkspordiPdf()
    {
        var v = Tellida();
        if (v.Count == 0 || KysiFail("pdf", "PDF|*.pdf") is not string fail) return;
        try
        {
            TellimusPdf.Loo(fail, v);
            Valmis(fail);
        }
        catch (Exception ex) { Teade.Show(Keel.T("PDF-i loomine ebaõnnestus:") + "\n" + ex.Message); }
    }
}

public static class TellimusPdf
{
    private static readonly XPdfFontOptions Unicode = new(PdfFontEncoding.Unicode);
    private static readonly CultureInfo Eesti = new("et-EE");

    public static void Loo(string fail, List<Autotellimus.Rida> read)
    {
        using var dok = new PdfDocument();
        dok.Info.Title = Keel.T("Tellimus tarnijale");
        var f = new XFont("Arial", 10, XFontStyle.Regular, Unicode);
        var fb = new XFont("Arial", 10, XFontStyle.Bold, Unicode);
        var fp = new XFont("Arial", 18, XFontStyle.Bold, Unicode);
        var fs = new XFont("Arial", 8, XFontStyle.Regular, Unicode);
        var hall = new XSolidBrush(XColor.FromArgb(110, 116, 125));
        var punane = new XSolidBrush(XColor.FromArgb(226, 6, 19));

        foreach (var tarnija in read.GroupBy(r => r.Tarnija))
        {
            PdfPage leht = dok.AddPage();
            leht.Size = PdfSharpCore.PageSize.A4;
            var g = XGraphics.FromPdfPage(leht);
            double x0 = 50, x1 = leht.Width.Point - 50, y = 50;

            g.DrawRectangle(punane, x0, y, 44, 44);
            g.DrawString("AKT", new XFont("Arial", 14, XFontStyle.Bold, Unicode), XBrushes.White, new XRect(x0, y, 44, 44), XStringFormats.Center);
            g.DrawString(Keel.T("Tellimus tarnijale"), fp, XBrushes.Black, x0 + 60, y + 20);
            g.DrawString(tarnija.Key, new XFont("Arial", 12, XFontStyle.Regular, Unicode), hall, x0 + 60, y + 40);
            g.DrawString(Keel.F("Kuupäev: {0}", DateTime.Now.ToString("dd.MM.yyyy")), f, XBrushes.Black, new XRect(x0, y, x1 - x0, 20), XStringFormats.TopRight);
            g.DrawString(Keel.T("AKT pood · IKTpv25 · Tallinn"), fs, hall, new XRect(x0, y + 18, x1 - x0, 20), XStringFormats.TopRight);
            y += 80;

            double[] veerud = { x0, x0 + 30, x1 - 190, x1 - 120, x1 };   // nr | toode | kogus | hind | summa
            void Pais()
            {
                g.DrawRectangle(new XSolidBrush(XColor.FromArgb(241, 243, 244)), x0, y - 14, x1 - x0, 22);
                g.DrawString("Nr", fb, XBrushes.Black, veerud[0] + 4, y);
                g.DrawString(Keel.T("Toode"), fb, XBrushes.Black, veerud[1], y);
                g.DrawString(Keel.T("Kogus"), fb, XBrushes.Black, new XRect(veerud[2], y - 12, 60, 16), XStringFormats.TopRight);
                g.DrawString(Keel.T("Hind"), fb, XBrushes.Black, new XRect(veerud[3], y - 12, 60, 16), XStringFormats.TopRight);
                g.DrawString(Keel.T("Summa"), fb, XBrushes.Black, new XRect(veerud[3], y - 12, veerud[4] - veerud[3], 16), XStringFormats.TopRight);
                y += 22;
            }
            Pais();
            int nr = 1;
            foreach (var r in tarnija)
            {
                if (y > leht.Height.Point - 90)
                {
                    g.Dispose();
                    leht = dok.AddPage();
                    leht.Size = PdfSharpCore.PageSize.A4;
                    g = XGraphics.FromPdfPage(leht);
                    y = 60;
                    Pais();
                }
                string nimi = r.Toode.Toodenimetus;
                while (nimi.Length > 4 && g.MeasureString(nimi, f).Width > veerud[2] - veerud[1] - 10) nimi = nimi[..^2];
                if (nimi != r.Toode.Toodenimetus) nimi += "…";
                g.DrawString(nr++.ToString(), f, XBrushes.Black, veerud[0] + 4, y);
                g.DrawString(nimi, f, XBrushes.Black, veerud[1], y);
                g.DrawString(Keel.F("{0} tk", r.Telli), f, XBrushes.Black, new XRect(veerud[2], y - 12, 60, 16), XStringFormats.TopRight);
                g.DrawString(r.Ostuhind.ToString("0.00", Eesti), f, XBrushes.Black, new XRect(veerud[3], y - 12, 60, 16), XStringFormats.TopRight);
                g.DrawString(r.Summa.ToString("0.00", Eesti), f, XBrushes.Black, new XRect(veerud[3], y - 12, veerud[4] - veerud[3], 16), XStringFormats.TopRight);
                g.DrawLine(new XPen(XColor.FromArgb(226, 230, 232), 0.5), x0, y + 6, x1, y + 6);
                y += 20;
            }
            y += 10;
            g.DrawString(Keel.F("KOKKU: {0} tk · {1} €", tarnija.Sum(r => r.Telli), tarnija.Sum(r => r.Summa).ToString("0.00", Eesti)),
                new XFont("Arial", 12, XFontStyle.Bold, Unicode), XBrushes.Black, new XRect(x0, y, x1 - x0, 20), XStringFormats.TopRight);
            y += 40;
            g.DrawString(Keel.T("Palume kinnitada tellimus ja tarneaeg e-postiga."), f, hall, x0, y);
            g.Dispose();
        }
        dok.Save(fail);
    }
}
