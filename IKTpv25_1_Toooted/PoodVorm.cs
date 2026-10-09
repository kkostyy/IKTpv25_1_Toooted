using System;
using System.Drawing;
using System.Windows.Forms;

namespace IKTpv25_1_Toooted
{
    // Visuaalne kassa (piltide võrk). Praegu ainult kujundus - ei tööta veel.
    public class PoodVorm : Form
    {
        public PoodVorm()
        {
            Text = "Pood";
            StartPosition = FormStartPosition.CenterParent;
            Font = UiStiil.Põhifont;
            BackColor = UiStiil.ToaTaust;
            ClientSize = new Size(900, 560);
            MinimumSize = new Size(700, 420);

            // Parem paneel: ostukorv
            var korv = new Panel { Dock = DockStyle.Right, Width = 240, BackColor = Color.White, Padding = new Padding(12) };
            var korvPealkiri = new Label { Text = "OSTUKORV", Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(110, 116, 125) };
            var korvList = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None };
            var alumine = new Panel { Dock = DockStyle.Bottom, Height = 90 };
            var kokku = new Label { Text = "Kokku: 0.00 €", Dock = DockStyle.Top, Height = 32, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            var ostan = UiStiil.LooNupp("Ostan", null, true);
            ostan.Location = new Point(0, 44);
            alumine.Controls.Add(ostan);
            alumine.Controls.Add(kokku);
            korv.Controls.Add(korvList);
            korv.Controls.Add(alumine);
            korv.Controls.Add(korvPealkiri);

            // Toodete võrk
            var vork = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10), BackColor = UiStiil.ToaTaust };
            for (int i = 1; i <= 8; i++)
                vork.Controls.Add(LooPlaat("Toode " + i, (i * 1.5m).ToString("0.00") + " €"));

            Controls.Add(vork);
            Controls.Add(korv);
        }

        private static Control LooPlaat(string nimi, string hind)
        {
            var plaat = new RoundedPanel { Size = new Size(160, 190), Margin = new Padding(8), Cursor = Cursors.Hand };
            var pilt = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(138, 110),
                BackColor = Color.FromArgb(240, 242, 245),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            var silt = new Label { Text = nimi, Location = new Point(10, 128), Size = new Size(138, 22), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), BackColor = Color.White };
            var h = new Label { Text = hind, Location = new Point(10, 152), Size = new Size(138, 22), ForeColor = UiStiil.Sinine, BackColor = Color.White };
            plaat.Controls.Add(pilt);
            plaat.Controls.Add(silt);
            plaat.Controls.Add(h);
            return plaat;
        }
    }
}
