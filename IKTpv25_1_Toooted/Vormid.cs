using System;
using System.Drawing;
using System.Windows.Forms;

namespace IKTpv25_1_Toooted
{
    // Ainult kujundus: need vormid avanevad, kuid nupud ei tee veel midagi.
    public abstract class KujundusVorm : Form
    {
        protected KujundusVorm(string pealkiri, int laius, int korgus)
        {
            Text = pealkiri;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Font = UiStiil.Põhifont;
            BackColor = Color.White;
            ClientSize = new Size(laius, korgus);
        }

        protected Label Silt(string tekst, int x, int y)
        {
            var l = new Label { Text = tekst, AutoSize = true, Location = new Point(x, y), ForeColor = Color.FromArgb(70, 75, 82) };
            Controls.Add(l);
            return l;
        }

        protected TextBox Sisend(int x, int y, int laius, bool parool = false)
        {
            var t = new TextBox { Location = new Point(x, y), Width = laius, UseSystemPasswordChar = parool };
            Controls.Add(t);
            return t;
        }

        protected Button Nupp(string tekst, int x, int y, bool peamine = false)
        {
            var b = UiStiil.LooNupp(tekst, null, peamine);
            b.Location = new Point(x, y);
            Controls.Add(b);
            return b;
        }

        protected Label Pealkiri(string tekst)
        {
            var l = new Label
            {
                Text = tekst,
                AutoSize = true,
                Location = new Point(20, 16),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = UiStiil.Sinine
            };
            Controls.Add(l);
            return l;
        }
    }

    public class LoginVorm : KujundusVorm
    {
        public LoginVorm() : base("Sisselogimine", 340, 260)
        {
            Pealkiri("Logi sisse");
            Silt("Kasutajanimi", 20, 64);
            Sisend(20, 84, 300);
            Silt("Parool", 20, 120);
            Sisend(20, 140, 300, true);
            Nupp("Logi sisse", 20, 190, true);
            Silt("Pole kontot? Registreeri", 150, 198);
        }
    }

    public class RegistreerimineVorm : KujundusVorm
    {
        public RegistreerimineVorm() : base("Registreerimine", 340, 380)
        {
            Pealkiri("Uus kasutaja");
            Silt("Kasutajanimi", 20, 64);
            Sisend(20, 84, 300);
            Silt("Parool", 20, 120);
            Sisend(20, 140, 300, true);
            Silt("Korda parooli", 20, 176);
            Sisend(20, 196, 300, true);
            Silt("Roll", 20, 232);
            var roll = new ComboBox { Location = new Point(20, 252), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            roll.Items.AddRange(new object[] { "Müüja", "Omanik" });
            roll.SelectedIndex = 0;
            Controls.Add(roll);
            Nupp("Registreeri", 20, 310, true);
            var tuhista = Nupp("Tühista", 130, 310);
            tuhista.DialogResult = DialogResult.Cancel;
            CancelButton = tuhista;
        }
    }

    public class KliendidVorm : KujundusVorm
    {
        public KliendidVorm() : base("Kliendid (administraatori paneel)", 640, 440)
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            Pealkiri("Kliendid");

            var vork = new DataGridView
            {
                Location = new Point(20, 56),
                Size = new Size(600, 200),
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            vork.Columns.Add("Nimi", "Nimi");
            vork.Columns.Add("Kliendikaart", "Kliendikaart");
            vork.Columns.Add("Boonus", "Boonus");
            Controls.Add(vork);

            Silt("Nimi", 20, 276);
            Sisend(20, 296, 230);
            Silt("Kliendikaart", 270, 276);
            Sisend(270, 296, 200);
            Silt("Boonus", 490, 276);
            Sisend(490, 296, 130);

            Nupp("Lisa", 20, 350, true);
            Nupp("Uuenda", 100, 350);
            Nupp("Kustuta", 190, 350);
        }
    }

    public class StatistikaVorm : KujundusVorm
    {
        public StatistikaVorm() : base("Statistika", 560, 400)
        {
            Pealkiri("Müügistatistika");
            Kaart("Müük täna", "0.00 €", 20);
            Kaart("Müük kokku", "0.00 €", 200);
            Kaart("Oste kokku", "0", 380);

            Silt("Väike laoseis (hoiatus)", 20, 150);
            var vork = new DataGridView
            {
                Location = new Point(20, 172),
                Size = new Size(520, 190),
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            vork.Columns.Add("Toode", "Toode");
            vork.Columns.Add("Kogus", "Kogus");
            Controls.Add(vork);
        }

        private void Kaart(string pealkiri, string vaartus, int x)
        {
            var kaart = new RoundedPanel { Location = new Point(x, 60), Size = new Size(160, 70) };
            kaart.Controls.Add(new Label { Text = pealkiri, AutoSize = true, Location = new Point(10, 8), ForeColor = Color.FromArgb(110, 116, 125), BackColor = Color.White });
            kaart.Controls.Add(new Label { Text = vaartus, AutoSize = true, Location = new Point(10, 30), Font = new Font("Segoe UI", 14F, FontStyle.Bold), BackColor = Color.White });
            Controls.Add(kaart);
        }
    }
}
