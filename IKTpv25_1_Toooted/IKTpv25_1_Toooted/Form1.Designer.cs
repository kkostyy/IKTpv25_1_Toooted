namespace IKTpv25_1_Toooted
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlTop = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlTabid = new System.Windows.Forms.Panel();
            this.tabToode = new RibbonTab();
            this.tabKassa = new RibbonTab();
            this.pnlKassaRibbon = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPuhasta = new RoundedButton();
            this.kaartOst = new RoundedPanel();
            this.sisuOst = new System.Windows.Forms.FlowLayoutPanel();
            this.lblOst = new System.Windows.Forms.Label();
            this.ridaOst = new System.Windows.Forms.FlowLayoutPanel();
            this.btnValin = new RoundedButton();
            this.btnOstan = new RoundedButton();
            this.kaartArve = new RoundedPanel();
            this.sisuArve = new System.Windows.Forms.FlowLayoutPanel();
            this.lblArve = new System.Windows.Forms.Label();
            this.ridaArve = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSaadaArve = new RoundedButton();
            this.btnMaksta = new RoundedButton();
            this.kaartPood = new RoundedPanel();
            this.sisuPood = new System.Windows.Forms.FlowLayoutPanel();
            this.lblPood = new System.Windows.Forms.Label();
            this.ridaPood = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPood = new RoundedButton();
            this.kaartKokku = new RoundedPanel();
            this.sisuKokku = new System.Windows.Forms.FlowLayoutPanel();
            this.lblKokku = new System.Windows.Forms.Label();
            this.ridaKokku = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlKokku = new RoundedPanel();
            this.txtKokku = new System.Windows.Forms.TextBox();
            this.kaartToode = new RoundedPanel();
            this.sisuToode = new System.Windows.Forms.FlowLayoutPanel();
            this.lblToode = new System.Windows.Forms.Label();
            this.ridaToode = new System.Windows.Forms.FlowLayoutPanel();
            this.btnLisa = new RoundedButton();
            this.btnUuenda = new RoundedButton();
            this.btnKustuta = new RoundedButton();
            this.kaartKategooria = new RoundedPanel();
            this.sisuKategooria = new System.Windows.Forms.FlowLayoutPanel();
            this.lblKategooria = new System.Windows.Forms.Label();
            this.ridaKategooria = new System.Windows.Forms.FlowLayoutPanel();
            this.btnLisaKategooria = new RoundedButton();
            this.btnKustutaKategooria = new RoundedButton();
            this.kaartPilt = new RoundedPanel();
            this.sisuPilt = new System.Windows.Forms.FlowLayoutPanel();
            this.lblPilt = new System.Windows.Forms.Label();
            this.ridaPilt = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOtsiFail = new RoundedButton();
            this.pnlJoon = new System.Windows.Forms.Panel();
            this.pnlSisu = new System.Windows.Forms.Panel();
            this.pnlVahe = new System.Windows.Forms.Panel();
            this.kaartSisestus = new RoundedPanel();
            this.lblSisestusPealkiri = new System.Windows.Forms.Label();
            this.tblVali = new System.Windows.Forms.TableLayoutPanel();
            this.lblNimetus = new System.Windows.Forms.Label();
            this.pnlNimetus = new RoundedPanel();
            this.txtNimetus = new System.Windows.Forms.TextBox();
            this.lblKogus = new System.Windows.Forms.Label();
            this.pnlKogus = new RoundedPanel();
            this.txtKogus = new System.Windows.Forms.TextBox();
            this.lblHind = new System.Windows.Forms.Label();
            this.pnlHind = new RoundedPanel();
            this.txtHind = new System.Windows.Forms.TextBox();
            this.lblKat = new System.Windows.Forms.Label();
            this.pnlKategooria = new RoundedPanel();
            this.cmbKategooria = new System.Windows.Forms.ComboBox();
            this.pnlPilt = new RoundedPanel();
            this.picPilt = new System.Windows.Forms.PictureBox();
            this.kaartTabel = new RoundedPanel();
            this.lblTabeliPealkiri = new System.Windows.Forms.Label();
            this.dataGridTooted = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.picPilt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTooted)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.AutoSize = true;
            this.pnlTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlTop.BackColor = System.Drawing.Color.FromArgb(245, 246, 248);
            this.pnlTop.Controls.Add(this.kaartToode);
            this.pnlTop.Controls.Add(this.kaartKategooria);
            this.pnlTop.Controls.Add(this.kaartPilt);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Padding = new System.Windows.Forms.Padding(8);
            this.pnlTop.Size = new System.Drawing.Size(1000, 88);
            this.pnlTop.TabIndex = 1;
            // 
            // kaartToode
            // 
            this.kaartToode.AutoSize = true;
            this.kaartToode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.kaartToode.BackColor = System.Drawing.Color.White;
            this.kaartToode.BorderColor = System.Drawing.Color.FromArgb(210, 214, 220);
            this.kaartToode.Controls.Add(this.sisuToode);
            this.kaartToode.FillColor = System.Drawing.Color.White;
            this.kaartToode.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.kaartToode.Name = "kaartToode";
            this.kaartToode.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.kaartToode.Radius = 12;
            this.kaartToode.Size = new System.Drawing.Size(200, 70);
            this.kaartToode.TabIndex = 2;
            // 
            // sisuToode
            // 
            this.sisuToode.AutoSize = true;
            this.sisuToode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sisuToode.BackColor = System.Drawing.Color.White;
            this.sisuToode.Controls.Add(this.lblToode);
            this.sisuToode.Controls.Add(this.ridaToode);
            this.sisuToode.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.sisuToode.Location = new System.Drawing.Point(8, 6);
            this.sisuToode.Margin = new System.Windows.Forms.Padding(0);
            this.sisuToode.Name = "sisuToode";
            this.sisuToode.Size = new System.Drawing.Size(180, 55);
            this.sisuToode.TabIndex = 3;
            this.sisuToode.WrapContents = false;
            // 
            // lblToode
            // 
            this.lblToode.AutoSize = true;
            this.lblToode.BackColor = System.Drawing.Color.White;
            this.lblToode.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblToode.ForeColor = System.Drawing.Color.FromArgb(110, 116, 125);
            this.lblToode.Margin = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblToode.Name = "lblToode";
            this.lblToode.Size = new System.Drawing.Size(50, 13);
            this.lblToode.TabIndex = 4;
            this.lblToode.Text = "TOODE";
            // 
            // ridaToode
            // 
            this.ridaToode.AutoSize = true;
            this.ridaToode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ridaToode.BackColor = System.Drawing.Color.White;
            this.ridaToode.Controls.Add(this.btnLisa);
            this.ridaToode.Controls.Add(this.btnUuenda);
            this.ridaToode.Controls.Add(this.btnKustuta);
            this.ridaToode.Controls.Add(this.btnPuhasta);
            this.ridaToode.Location = new System.Drawing.Point(0, 15);
            this.ridaToode.Margin = new System.Windows.Forms.Padding(0);
            this.ridaToode.Name = "ridaToode";
            this.ridaToode.Size = new System.Drawing.Size(180, 40);
            this.ridaToode.TabIndex = 5;
            this.ridaToode.WrapContents = false;
            // 
            // btnLisa
            // 
            this.btnLisa.AutoSize = true;
            this.btnLisa.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnLisa.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnLisa.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLisa.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnLisa.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLisa.ForeColor = System.Drawing.Color.White;
            this.btnLisa.Margin = new System.Windows.Forms.Padding(3);
            this.btnLisa.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnLisa.Name = "btnLisa";
            this.btnLisa.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnLisa.Radius = 8;
            this.btnLisa.Size = new System.Drawing.Size(60, 30);
            this.btnLisa.TabIndex = 6;
            this.btnLisa.Text = "Lisa";
            this.btnLisa.UseVisualStyleBackColor = false;
            // 
            // btnUuenda
            // 
            this.btnUuenda.AutoSize = true;
            this.btnUuenda.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnUuenda.BackColor = System.Drawing.Color.White;
            this.btnUuenda.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUuenda.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.btnUuenda.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUuenda.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.btnUuenda.Margin = new System.Windows.Forms.Padding(3);
            this.btnUuenda.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnUuenda.Name = "btnUuenda";
            this.btnUuenda.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnUuenda.Radius = 8;
            this.btnUuenda.Size = new System.Drawing.Size(60, 30);
            this.btnUuenda.TabIndex = 7;
            this.btnUuenda.Text = "Uuenda";
            this.btnUuenda.UseVisualStyleBackColor = false;
            // 
            // btnKustuta
            // 
            this.btnKustuta.AutoSize = true;
            this.btnKustuta.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnKustuta.BackColor = System.Drawing.Color.White;
            this.btnKustuta.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnKustuta.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.btnKustuta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKustuta.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.btnKustuta.Margin = new System.Windows.Forms.Padding(3);
            this.btnKustuta.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnKustuta.Name = "btnKustuta";
            this.btnKustuta.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnKustuta.Radius = 8;
            this.btnKustuta.Size = new System.Drawing.Size(60, 30);
            this.btnKustuta.TabIndex = 8;
            this.btnKustuta.Text = "Kustuta";
            this.btnKustuta.UseVisualStyleBackColor = false;
            // 
            // kaartKategooria
            // 
            this.kaartKategooria.AutoSize = true;
            this.kaartKategooria.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.kaartKategooria.BackColor = System.Drawing.Color.White;
            this.kaartKategooria.BorderColor = System.Drawing.Color.FromArgb(210, 214, 220);
            this.kaartKategooria.Controls.Add(this.sisuKategooria);
            this.kaartKategooria.FillColor = System.Drawing.Color.White;
            this.kaartKategooria.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.kaartKategooria.Name = "kaartKategooria";
            this.kaartKategooria.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.kaartKategooria.Radius = 12;
            this.kaartKategooria.Size = new System.Drawing.Size(200, 70);
            this.kaartKategooria.TabIndex = 9;
            // 
            // sisuKategooria
            // 
            this.sisuKategooria.AutoSize = true;
            this.sisuKategooria.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sisuKategooria.BackColor = System.Drawing.Color.White;
            this.sisuKategooria.Controls.Add(this.lblKategooria);
            this.sisuKategooria.Controls.Add(this.ridaKategooria);
            this.sisuKategooria.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.sisuKategooria.Location = new System.Drawing.Point(8, 6);
            this.sisuKategooria.Margin = new System.Windows.Forms.Padding(0);
            this.sisuKategooria.Name = "sisuKategooria";
            this.sisuKategooria.Size = new System.Drawing.Size(180, 55);
            this.sisuKategooria.TabIndex = 10;
            this.sisuKategooria.WrapContents = false;
            // 
            // lblKategooria
            // 
            this.lblKategooria.AutoSize = true;
            this.lblKategooria.BackColor = System.Drawing.Color.White;
            this.lblKategooria.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblKategooria.ForeColor = System.Drawing.Color.FromArgb(110, 116, 125);
            this.lblKategooria.Margin = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblKategooria.Name = "lblKategooria";
            this.lblKategooria.Size = new System.Drawing.Size(50, 13);
            this.lblKategooria.TabIndex = 11;
            this.lblKategooria.Text = "KATEGOORIA";
            // 
            // ridaKategooria
            // 
            this.ridaKategooria.AutoSize = true;
            this.ridaKategooria.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ridaKategooria.BackColor = System.Drawing.Color.White;
            this.ridaKategooria.Controls.Add(this.btnLisaKategooria);
            this.ridaKategooria.Controls.Add(this.btnKustutaKategooria);
            this.ridaKategooria.Location = new System.Drawing.Point(0, 15);
            this.ridaKategooria.Margin = new System.Windows.Forms.Padding(0);
            this.ridaKategooria.Name = "ridaKategooria";
            this.ridaKategooria.Size = new System.Drawing.Size(180, 40);
            this.ridaKategooria.TabIndex = 12;
            this.ridaKategooria.WrapContents = false;
            // 
            // btnLisaKategooria
            // 
            this.btnLisaKategooria.AutoSize = true;
            this.btnLisaKategooria.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnLisaKategooria.BackColor = System.Drawing.Color.White;
            this.btnLisaKategooria.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLisaKategooria.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.btnLisaKategooria.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLisaKategooria.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.btnLisaKategooria.Margin = new System.Windows.Forms.Padding(3);
            this.btnLisaKategooria.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnLisaKategooria.Name = "btnLisaKategooria";
            this.btnLisaKategooria.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnLisaKategooria.Radius = 8;
            this.btnLisaKategooria.Size = new System.Drawing.Size(60, 30);
            this.btnLisaKategooria.TabIndex = 13;
            this.btnLisaKategooria.Text = "Lisa kategooria";
            this.btnLisaKategooria.UseVisualStyleBackColor = false;
            // 
            // btnKustutaKategooria
            // 
            this.btnKustutaKategooria.AutoSize = true;
            this.btnKustutaKategooria.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnKustutaKategooria.BackColor = System.Drawing.Color.White;
            this.btnKustutaKategooria.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnKustutaKategooria.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.btnKustutaKategooria.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKustutaKategooria.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.btnKustutaKategooria.Margin = new System.Windows.Forms.Padding(3);
            this.btnKustutaKategooria.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnKustutaKategooria.Name = "btnKustutaKategooria";
            this.btnKustutaKategooria.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnKustutaKategooria.Radius = 8;
            this.btnKustutaKategooria.Size = new System.Drawing.Size(60, 30);
            this.btnKustutaKategooria.TabIndex = 14;
            this.btnKustutaKategooria.Text = "Kustuta kategooria";
            this.btnKustutaKategooria.UseVisualStyleBackColor = false;
            // 
            // kaartPilt
            // 
            this.kaartPilt.AutoSize = true;
            this.kaartPilt.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.kaartPilt.BackColor = System.Drawing.Color.White;
            this.kaartPilt.BorderColor = System.Drawing.Color.FromArgb(210, 214, 220);
            this.kaartPilt.Controls.Add(this.sisuPilt);
            this.kaartPilt.FillColor = System.Drawing.Color.White;
            this.kaartPilt.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.kaartPilt.Name = "kaartPilt";
            this.kaartPilt.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.kaartPilt.Radius = 12;
            this.kaartPilt.Size = new System.Drawing.Size(200, 70);
            this.kaartPilt.TabIndex = 15;
            // 
            // sisuPilt
            // 
            this.sisuPilt.AutoSize = true;
            this.sisuPilt.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sisuPilt.BackColor = System.Drawing.Color.White;
            this.sisuPilt.Controls.Add(this.lblPilt);
            this.sisuPilt.Controls.Add(this.ridaPilt);
            this.sisuPilt.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.sisuPilt.Location = new System.Drawing.Point(8, 6);
            this.sisuPilt.Margin = new System.Windows.Forms.Padding(0);
            this.sisuPilt.Name = "sisuPilt";
            this.sisuPilt.Size = new System.Drawing.Size(180, 55);
            this.sisuPilt.TabIndex = 16;
            this.sisuPilt.WrapContents = false;
            // 
            // lblPilt
            // 
            this.lblPilt.AutoSize = true;
            this.lblPilt.BackColor = System.Drawing.Color.White;
            this.lblPilt.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblPilt.ForeColor = System.Drawing.Color.FromArgb(110, 116, 125);
            this.lblPilt.Margin = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblPilt.Name = "lblPilt";
            this.lblPilt.Size = new System.Drawing.Size(50, 13);
            this.lblPilt.TabIndex = 17;
            this.lblPilt.Text = "PILT";
            // 
            // ridaPilt
            // 
            this.ridaPilt.AutoSize = true;
            this.ridaPilt.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ridaPilt.BackColor = System.Drawing.Color.White;
            this.ridaPilt.Controls.Add(this.btnOtsiFail);
            this.ridaPilt.Location = new System.Drawing.Point(0, 15);
            this.ridaPilt.Margin = new System.Windows.Forms.Padding(0);
            this.ridaPilt.Name = "ridaPilt";
            this.ridaPilt.Size = new System.Drawing.Size(180, 40);
            this.ridaPilt.TabIndex = 18;
            this.ridaPilt.WrapContents = false;
            // 
            // btnOtsiFail
            // 
            this.btnOtsiFail.AutoSize = true;
            this.btnOtsiFail.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOtsiFail.BackColor = System.Drawing.Color.White;
            this.btnOtsiFail.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOtsiFail.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.btnOtsiFail.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOtsiFail.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.btnOtsiFail.Margin = new System.Windows.Forms.Padding(3);
            this.btnOtsiFail.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnOtsiFail.Name = "btnOtsiFail";
            this.btnOtsiFail.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnOtsiFail.Radius = 8;
            this.btnOtsiFail.Size = new System.Drawing.Size(60, 30);
            this.btnOtsiFail.TabIndex = 19;
            this.btnOtsiFail.Text = "Otsi fail...";
            this.btnOtsiFail.UseVisualStyleBackColor = false;
            // 
            // pnlJoon
            // 
            this.pnlJoon.BackColor = System.Drawing.Color.FromArgb(210, 214, 220);
            this.pnlJoon.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlJoon.Location = new System.Drawing.Point(0, 88);
            this.pnlJoon.Name = "pnlJoon";
            this.pnlJoon.Size = new System.Drawing.Size(1000, 1);
            this.pnlJoon.TabIndex = 20;
            // 
            // pnlSisu
            // 
            this.pnlSisu.BackColor = System.Drawing.Color.FromArgb(235, 237, 240);
            this.pnlSisu.Controls.Add(this.kaartTabel);
            this.pnlSisu.Controls.Add(this.pnlVahe);
            this.pnlSisu.Controls.Add(this.kaartSisestus);
            this.pnlSisu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSisu.Location = new System.Drawing.Point(0, 89);
            this.pnlSisu.Name = "pnlSisu";
            this.pnlSisu.Padding = new System.Windows.Forms.Padding(12);
            this.pnlSisu.Size = new System.Drawing.Size(1000, 531);
            this.pnlSisu.TabIndex = 21;
            // 
            // pnlVahe
            // 
            this.pnlVahe.BackColor = System.Drawing.Color.FromArgb(235, 237, 240);
            this.pnlVahe.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlVahe.Location = new System.Drawing.Point(12, 258);
            this.pnlVahe.Name = "pnlVahe";
            this.pnlVahe.Size = new System.Drawing.Size(976, 12);
            this.pnlVahe.TabIndex = 22;
            // 
            // kaartSisestus
            // 
            this.kaartSisestus.BackColor = System.Drawing.Color.White;
            this.kaartSisestus.BorderColor = System.Drawing.Color.FromArgb(210, 214, 220);
            this.kaartSisestus.Controls.Add(this.lblSisestusPealkiri);
            this.kaartSisestus.Controls.Add(this.tblVali);
            this.kaartSisestus.Controls.Add(this.pnlPilt);
            this.kaartSisestus.Dock = System.Windows.Forms.DockStyle.Top;
            this.kaartSisestus.FillColor = System.Drawing.Color.White;
            this.kaartSisestus.Location = new System.Drawing.Point(12, 12);
            this.kaartSisestus.Name = "kaartSisestus";
            this.kaartSisestus.Radius = 14;
            this.kaartSisestus.Size = new System.Drawing.Size(976, 246);
            this.kaartSisestus.TabIndex = 23;
            // 
            // lblSisestusPealkiri
            // 
            this.lblSisestusPealkiri.AutoSize = true;
            this.lblSisestusPealkiri.BackColor = System.Drawing.Color.White;
            this.lblSisestusPealkiri.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblSisestusPealkiri.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.lblSisestusPealkiri.Location = new System.Drawing.Point(18, 14);
            this.lblSisestusPealkiri.Name = "lblSisestusPealkiri";
            this.lblSisestusPealkiri.Size = new System.Drawing.Size(113, 21);
            this.lblSisestusPealkiri.TabIndex = 24;
            this.lblSisestusPealkiri.Text = "Toote andmed";
            // 
            // tblVali
            // 
            this.tblVali.BackColor = System.Drawing.Color.White;
            this.tblVali.ColumnCount = 2;
            this.tblVali.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tblVali.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblVali.Controls.Add(this.lblNimetus, 0, 0);
            this.tblVali.Controls.Add(this.pnlNimetus, 1, 0);
            this.tblVali.Controls.Add(this.lblKogus, 0, 1);
            this.tblVali.Controls.Add(this.pnlKogus, 1, 1);
            this.tblVali.Controls.Add(this.lblHind, 0, 2);
            this.tblVali.Controls.Add(this.pnlHind, 1, 2);
            this.tblVali.Controls.Add(this.lblKat, 0, 3);
            this.tblVali.Controls.Add(this.pnlKategooria, 1, 3);
            this.tblVali.Location = new System.Drawing.Point(18, 50);
            this.tblVali.Name = "tblVali";
            this.tblVali.RowCount = 4;
            this.tblVali.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tblVali.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tblVali.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tblVali.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tblVali.Size = new System.Drawing.Size(470, 176);
            this.tblVali.TabIndex = 25;
            // 
            // lblNimetus
            // 
            this.lblNimetus.BackColor = System.Drawing.Color.White;
            this.lblNimetus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNimetus.ForeColor = System.Drawing.Color.FromArgb(70, 75, 82);
            this.lblNimetus.Location = new System.Drawing.Point(3, 0);
            this.lblNimetus.Name = "lblNimetus";
            this.lblNimetus.Size = new System.Drawing.Size(114, 44);
            this.lblNimetus.TabIndex = 26;
            this.lblNimetus.Text = "Toodenimetus";
            this.lblNimetus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlNimetus
            // 
            this.pnlNimetus.BackColor = System.Drawing.Color.White;
            this.pnlNimetus.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.pnlNimetus.Controls.Add(this.txtNimetus);
            this.pnlNimetus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNimetus.FillColor = System.Drawing.Color.White;
            this.pnlNimetus.Location = new System.Drawing.Point(123, 4);
            this.pnlNimetus.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlNimetus.Name = "pnlNimetus";
            this.pnlNimetus.Radius = 9;
            this.pnlNimetus.Size = new System.Drawing.Size(344, 36);
            this.pnlNimetus.TabIndex = 27;
            // 
            // txtNimetus
            // 
            this.txtNimetus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNimetus.BackColor = System.Drawing.Color.White;
            this.txtNimetus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtNimetus.Location = new System.Drawing.Point(10, 10);
            this.txtNimetus.Name = "txtNimetus";
            this.txtNimetus.Size = new System.Drawing.Size(324, 16);
            this.txtNimetus.TabIndex = 0;
            // 
            // lblKogus
            // 
            this.lblKogus.BackColor = System.Drawing.Color.White;
            this.lblKogus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKogus.ForeColor = System.Drawing.Color.FromArgb(70, 75, 82);
            this.lblKogus.Location = new System.Drawing.Point(3, 0);
            this.lblKogus.Name = "lblKogus";
            this.lblKogus.Size = new System.Drawing.Size(114, 44);
            this.lblKogus.TabIndex = 28;
            this.lblKogus.Text = "Kogus";
            this.lblKogus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlKogus
            // 
            this.pnlKogus.BackColor = System.Drawing.Color.White;
            this.pnlKogus.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.pnlKogus.Controls.Add(this.txtKogus);
            this.pnlKogus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlKogus.FillColor = System.Drawing.Color.White;
            this.pnlKogus.Location = new System.Drawing.Point(123, 4);
            this.pnlKogus.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlKogus.Name = "pnlKogus";
            this.pnlKogus.Radius = 9;
            this.pnlKogus.Size = new System.Drawing.Size(344, 36);
            this.pnlKogus.TabIndex = 29;
            // 
            // txtKogus
            // 
            this.txtKogus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKogus.BackColor = System.Drawing.Color.White;
            this.txtKogus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtKogus.Location = new System.Drawing.Point(10, 10);
            this.txtKogus.Name = "txtKogus";
            this.txtKogus.Size = new System.Drawing.Size(324, 16);
            this.txtKogus.TabIndex = 0;
            // 
            // lblHind
            // 
            this.lblHind.BackColor = System.Drawing.Color.White;
            this.lblHind.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHind.ForeColor = System.Drawing.Color.FromArgb(70, 75, 82);
            this.lblHind.Location = new System.Drawing.Point(3, 0);
            this.lblHind.Name = "lblHind";
            this.lblHind.Size = new System.Drawing.Size(114, 44);
            this.lblHind.TabIndex = 30;
            this.lblHind.Text = "Hind";
            this.lblHind.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlHind
            // 
            this.pnlHind.BackColor = System.Drawing.Color.White;
            this.pnlHind.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.pnlHind.Controls.Add(this.txtHind);
            this.pnlHind.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHind.FillColor = System.Drawing.Color.White;
            this.pnlHind.Location = new System.Drawing.Point(123, 4);
            this.pnlHind.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlHind.Name = "pnlHind";
            this.pnlHind.Radius = 9;
            this.pnlHind.Size = new System.Drawing.Size(344, 36);
            this.pnlHind.TabIndex = 31;
            // 
            // txtHind
            // 
            this.txtHind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHind.BackColor = System.Drawing.Color.White;
            this.txtHind.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtHind.Location = new System.Drawing.Point(10, 10);
            this.txtHind.Name = "txtHind";
            this.txtHind.Size = new System.Drawing.Size(324, 16);
            this.txtHind.TabIndex = 0;
            // 
            // lblKat
            // 
            this.lblKat.BackColor = System.Drawing.Color.White;
            this.lblKat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKat.ForeColor = System.Drawing.Color.FromArgb(70, 75, 82);
            this.lblKat.Location = new System.Drawing.Point(3, 0);
            this.lblKat.Name = "lblKat";
            this.lblKat.Size = new System.Drawing.Size(114, 44);
            this.lblKat.TabIndex = 32;
            this.lblKat.Text = "Kategooria";
            this.lblKat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlKategooria
            // 
            this.pnlKategooria.BackColor = System.Drawing.Color.White;
            this.pnlKategooria.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.pnlKategooria.Controls.Add(this.cmbKategooria);
            this.pnlKategooria.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlKategooria.FillColor = System.Drawing.Color.White;
            this.pnlKategooria.Location = new System.Drawing.Point(123, 4);
            this.pnlKategooria.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlKategooria.Name = "pnlKategooria";
            this.pnlKategooria.Radius = 9;
            this.pnlKategooria.Size = new System.Drawing.Size(344, 36);
            this.pnlKategooria.TabIndex = 33;
            // 
            // cmbKategooria
            // 
            this.cmbKategooria.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbKategooria.BackColor = System.Drawing.Color.White;
            this.cmbKategooria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKategooria.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbKategooria.FormattingEnabled = true;
            this.cmbKategooria.Location = new System.Drawing.Point(6, 6);
            this.cmbKategooria.Name = "cmbKategooria";
            this.cmbKategooria.Size = new System.Drawing.Size(332, 23);
            this.cmbKategooria.TabIndex = 0;
            // 
            // pnlPilt
            // 
            this.pnlPilt.BackColor = System.Drawing.Color.FromArgb(245, 246, 248);
            this.pnlPilt.BorderColor = System.Drawing.Color.FromArgb(210, 214, 220);
            this.pnlPilt.Controls.Add(this.picPilt);
            this.pnlPilt.FillColor = System.Drawing.Color.FromArgb(245, 246, 248);
            this.pnlPilt.Location = new System.Drawing.Point(530, 50);
            this.pnlPilt.Name = "pnlPilt";
            this.pnlPilt.Padding = new System.Windows.Forms.Padding(8);
            this.pnlPilt.Radius = 14;
            this.pnlPilt.Size = new System.Drawing.Size(176, 176);
            this.pnlPilt.TabIndex = 34;
            // 
            // picPilt
            // 
            this.picPilt.BackColor = System.Drawing.Color.FromArgb(245, 246, 248);
            this.picPilt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPilt.Location = new System.Drawing.Point(8, 8);
            this.picPilt.Name = "picPilt";
            this.picPilt.Size = new System.Drawing.Size(160, 160);
            this.picPilt.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPilt.TabIndex = 0;
            this.picPilt.TabStop = false;
            // 
            // kaartTabel
            // 
            this.kaartTabel.BackColor = System.Drawing.Color.White;
            this.kaartTabel.BorderColor = System.Drawing.Color.FromArgb(210, 214, 220);
            this.kaartTabel.Controls.Add(this.dataGridTooted);
            this.kaartTabel.Controls.Add(this.lblTabeliPealkiri);
            this.kaartTabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kaartTabel.FillColor = System.Drawing.Color.White;
            this.kaartTabel.Location = new System.Drawing.Point(12, 270);
            this.kaartTabel.Name = "kaartTabel";
            this.kaartTabel.Padding = new System.Windows.Forms.Padding(16, 10, 16, 16);
            this.kaartTabel.Radius = 14;
            this.kaartTabel.Size = new System.Drawing.Size(976, 249);
            this.kaartTabel.TabIndex = 35;
            // 
            // lblTabeliPealkiri
            // 
            this.lblTabeliPealkiri.BackColor = System.Drawing.Color.White;
            this.lblTabeliPealkiri.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTabeliPealkiri.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTabeliPealkiri.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.lblTabeliPealkiri.Location = new System.Drawing.Point(16, 10);
            this.lblTabeliPealkiri.Name = "lblTabeliPealkiri";
            this.lblTabeliPealkiri.Size = new System.Drawing.Size(944, 34);
            this.lblTabeliPealkiri.TabIndex = 36;
            this.lblTabeliPealkiri.Text = "Tooted";
            this.lblTabeliPealkiri.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dataGridTooted
            // 
            this.dataGridTooted.AllowUserToAddRows = false;
            this.dataGridTooted.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(248, 249, 251);
            this.dataGridTooted.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridTooted.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridTooted.BackgroundColor = System.Drawing.Color.White;
            this.dataGridTooted.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridTooted.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.dataGridTooted.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridTooted.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridTooted.ColumnHeadersHeight = 34;
            this.dataGridTooted.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(214, 232, 250);
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.dataGridTooted.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridTooted.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridTooted.EnableHeadersVisualStyles = false;
            this.dataGridTooted.GridColor = System.Drawing.Color.FromArgb(232, 234, 238);
            this.dataGridTooted.Location = new System.Drawing.Point(16, 44);
            this.dataGridTooted.Name = "dataGridTooted";
            this.dataGridTooted.ReadOnly = true;
            this.dataGridTooted.RowHeadersVisible = false;
            this.dataGridTooted.RowTemplate.Height = 30;
            this.dataGridTooted.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridTooted.Size = new System.Drawing.Size(944, 189);
            this.dataGridTooted.TabIndex = 0;
            // 
            // pnlTabid
            // 
            this.pnlTabid.BackColor = System.Drawing.Color.FromArgb(235, 237, 240);
            this.pnlTabid.Controls.Add(this.tabToode);
            this.pnlTabid.Controls.Add(this.tabKassa);
            this.pnlTabid.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTabid.Location = new System.Drawing.Point(0, 0);
            this.pnlTabid.Name = "pnlTabid";
            this.pnlTabid.Size = new System.Drawing.Size(1000, 34);
            this.pnlTabid.TabIndex = 201;
            // 
            // tabToode
            // 
            this.tabToode.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tabToode.Location = new System.Drawing.Point(10, 4);
            this.tabToode.Name = "tabToode";
            this.tabToode.Size = new System.Drawing.Size(90, 30);
            this.tabToode.TabIndex = 202;
            this.tabToode.Text = "Tooted";
            this.tabToode.Valitud = true;
            // 
            // tabKassa
            // 
            this.tabKassa.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tabKassa.Location = new System.Drawing.Point(104, 4);
            this.tabKassa.Name = "tabKassa";
            this.tabKassa.Size = new System.Drawing.Size(90, 30);
            this.tabKassa.TabIndex = 203;
            this.tabKassa.Text = "Kassa";
            // 
            // pnlKassaRibbon
            // 
            this.pnlKassaRibbon.AutoSize = true;
            this.pnlKassaRibbon.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlKassaRibbon.BackColor = System.Drawing.Color.FromArgb(245, 246, 248);
            this.pnlKassaRibbon.Controls.Add(this.kaartOst);
            this.pnlKassaRibbon.Controls.Add(this.kaartArve);
            this.pnlKassaRibbon.Controls.Add(this.kaartPood);
            this.pnlKassaRibbon.Controls.Add(this.kaartKokku);
            this.pnlKassaRibbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlKassaRibbon.Location = new System.Drawing.Point(0, 34);
            this.pnlKassaRibbon.Name = "pnlKassaRibbon";
            this.pnlKassaRibbon.Padding = new System.Windows.Forms.Padding(8);
            this.pnlKassaRibbon.Size = new System.Drawing.Size(1000, 88);
            this.pnlKassaRibbon.TabIndex = 204;
            this.pnlKassaRibbon.Visible = false;
            // 
            // btnPuhasta
            // 
            this.btnPuhasta.AutoSize = true;
            this.btnPuhasta.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPuhasta.BackColor = System.Drawing.Color.White;
            this.btnPuhasta.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPuhasta.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.btnPuhasta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPuhasta.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.btnPuhasta.Margin = new System.Windows.Forms.Padding(3);
            this.btnPuhasta.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnPuhasta.Name = "btnPuhasta";
            this.btnPuhasta.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnPuhasta.Radius = 8;
            this.btnPuhasta.Size = new System.Drawing.Size(60, 30);
            this.btnPuhasta.TabIndex = 205;
            this.btnPuhasta.Text = "Puhasta";
            this.btnPuhasta.UseVisualStyleBackColor = false;
            // 
            // kaartOst
            // 
            this.kaartOst.AutoSize = true;
            this.kaartOst.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.kaartOst.BackColor = System.Drawing.Color.White;
            this.kaartOst.BorderColor = System.Drawing.Color.FromArgb(210, 214, 220);
            this.kaartOst.Controls.Add(this.sisuOst);
            this.kaartOst.FillColor = System.Drawing.Color.White;
            this.kaartOst.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.kaartOst.Name = "kaartOst";
            this.kaartOst.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.kaartOst.Radius = 12;
            this.kaartOst.Size = new System.Drawing.Size(200, 70);
            this.kaartOst.TabIndex = 206;
            // 
            // sisuOst
            // 
            this.sisuOst.AutoSize = true;
            this.sisuOst.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sisuOst.BackColor = System.Drawing.Color.White;
            this.sisuOst.Controls.Add(this.lblOst);
            this.sisuOst.Controls.Add(this.ridaOst);
            this.sisuOst.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.sisuOst.Location = new System.Drawing.Point(8, 6);
            this.sisuOst.Margin = new System.Windows.Forms.Padding(0);
            this.sisuOst.Name = "sisuOst";
            this.sisuOst.Size = new System.Drawing.Size(180, 55);
            this.sisuOst.TabIndex = 207;
            this.sisuOst.WrapContents = false;
            // 
            // lblOst
            // 
            this.lblOst.AutoSize = true;
            this.lblOst.BackColor = System.Drawing.Color.White;
            this.lblOst.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblOst.ForeColor = System.Drawing.Color.FromArgb(110, 116, 125);
            this.lblOst.Margin = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblOst.Name = "lblOst";
            this.lblOst.Size = new System.Drawing.Size(50, 13);
            this.lblOst.TabIndex = 208;
            this.lblOst.Text = "OST";
            // 
            // ridaOst
            // 
            this.ridaOst.AutoSize = true;
            this.ridaOst.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ridaOst.BackColor = System.Drawing.Color.White;
            this.ridaOst.Controls.Add(this.btnValin);
            this.ridaOst.Controls.Add(this.btnOstan);
            this.ridaOst.Location = new System.Drawing.Point(0, 15);
            this.ridaOst.Margin = new System.Windows.Forms.Padding(0);
            this.ridaOst.Name = "ridaOst";
            this.ridaOst.Size = new System.Drawing.Size(180, 40);
            this.ridaOst.TabIndex = 209;
            this.ridaOst.WrapContents = false;
            // 
            // btnValin
            // 
            this.btnValin.AutoSize = true;
            this.btnValin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnValin.BackColor = System.Drawing.Color.White;
            this.btnValin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnValin.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.btnValin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnValin.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.btnValin.Margin = new System.Windows.Forms.Padding(3);
            this.btnValin.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnValin.Name = "btnValin";
            this.btnValin.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnValin.Radius = 8;
            this.btnValin.Size = new System.Drawing.Size(60, 30);
            this.btnValin.TabIndex = 210;
            this.btnValin.Text = "Valin";
            this.btnValin.UseVisualStyleBackColor = false;
            // 
            // btnOstan
            // 
            this.btnOstan.AutoSize = true;
            this.btnOstan.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOstan.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnOstan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOstan.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnOstan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOstan.ForeColor = System.Drawing.Color.White;
            this.btnOstan.Margin = new System.Windows.Forms.Padding(3);
            this.btnOstan.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnOstan.Name = "btnOstan";
            this.btnOstan.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnOstan.Radius = 8;
            this.btnOstan.Size = new System.Drawing.Size(60, 30);
            this.btnOstan.TabIndex = 211;
            this.btnOstan.Text = "Ostan";
            this.btnOstan.UseVisualStyleBackColor = false;
            // 
            // kaartArve
            // 
            this.kaartArve.AutoSize = true;
            this.kaartArve.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.kaartArve.BackColor = System.Drawing.Color.White;
            this.kaartArve.BorderColor = System.Drawing.Color.FromArgb(210, 214, 220);
            this.kaartArve.Controls.Add(this.sisuArve);
            this.kaartArve.FillColor = System.Drawing.Color.White;
            this.kaartArve.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.kaartArve.Name = "kaartArve";
            this.kaartArve.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.kaartArve.Radius = 12;
            this.kaartArve.Size = new System.Drawing.Size(200, 70);
            this.kaartArve.TabIndex = 212;
            // 
            // sisuArve
            // 
            this.sisuArve.AutoSize = true;
            this.sisuArve.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sisuArve.BackColor = System.Drawing.Color.White;
            this.sisuArve.Controls.Add(this.lblArve);
            this.sisuArve.Controls.Add(this.ridaArve);
            this.sisuArve.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.sisuArve.Location = new System.Drawing.Point(8, 6);
            this.sisuArve.Margin = new System.Windows.Forms.Padding(0);
            this.sisuArve.Name = "sisuArve";
            this.sisuArve.Size = new System.Drawing.Size(180, 55);
            this.sisuArve.TabIndex = 213;
            this.sisuArve.WrapContents = false;
            // 
            // lblArve
            // 
            this.lblArve.AutoSize = true;
            this.lblArve.BackColor = System.Drawing.Color.White;
            this.lblArve.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblArve.ForeColor = System.Drawing.Color.FromArgb(110, 116, 125);
            this.lblArve.Margin = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblArve.Name = "lblArve";
            this.lblArve.Size = new System.Drawing.Size(50, 13);
            this.lblArve.TabIndex = 214;
            this.lblArve.Text = "ARVE";
            // 
            // ridaArve
            // 
            this.ridaArve.AutoSize = true;
            this.ridaArve.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ridaArve.BackColor = System.Drawing.Color.White;
            this.ridaArve.Controls.Add(this.btnSaadaArve);
            this.ridaArve.Controls.Add(this.btnMaksta);
            this.ridaArve.Location = new System.Drawing.Point(0, 15);
            this.ridaArve.Margin = new System.Windows.Forms.Padding(0);
            this.ridaArve.Name = "ridaArve";
            this.ridaArve.Size = new System.Drawing.Size(180, 40);
            this.ridaArve.TabIndex = 215;
            this.ridaArve.WrapContents = false;
            // 
            // btnSaadaArve
            // 
            this.btnSaadaArve.AutoSize = true;
            this.btnSaadaArve.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSaadaArve.BackColor = System.Drawing.Color.White;
            this.btnSaadaArve.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaadaArve.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.btnSaadaArve.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaadaArve.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.btnSaadaArve.Margin = new System.Windows.Forms.Padding(3);
            this.btnSaadaArve.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnSaadaArve.Name = "btnSaadaArve";
            this.btnSaadaArve.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnSaadaArve.Radius = 8;
            this.btnSaadaArve.Size = new System.Drawing.Size(60, 30);
            this.btnSaadaArve.TabIndex = 216;
            this.btnSaadaArve.Text = "Saada arve";
            this.btnSaadaArve.UseVisualStyleBackColor = false;
            // 
            // btnMaksta
            // 
            this.btnMaksta.AutoSize = true;
            this.btnMaksta.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnMaksta.BackColor = System.Drawing.Color.White;
            this.btnMaksta.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMaksta.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.btnMaksta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaksta.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.btnMaksta.Margin = new System.Windows.Forms.Padding(3);
            this.btnMaksta.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnMaksta.Name = "btnMaksta";
            this.btnMaksta.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnMaksta.Radius = 8;
            this.btnMaksta.Size = new System.Drawing.Size(60, 30);
            this.btnMaksta.TabIndex = 217;
            this.btnMaksta.Text = "Maksta";
            this.btnMaksta.UseVisualStyleBackColor = false;
            // 
            // kaartPood
            // 
            this.kaartPood.AutoSize = true;
            this.kaartPood.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.kaartPood.BackColor = System.Drawing.Color.White;
            this.kaartPood.BorderColor = System.Drawing.Color.FromArgb(210, 214, 220);
            this.kaartPood.Controls.Add(this.sisuPood);
            this.kaartPood.FillColor = System.Drawing.Color.White;
            this.kaartPood.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.kaartPood.Name = "kaartPood";
            this.kaartPood.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.kaartPood.Radius = 12;
            this.kaartPood.Size = new System.Drawing.Size(200, 70);
            this.kaartPood.TabIndex = 218;
            // 
            // sisuPood
            // 
            this.sisuPood.AutoSize = true;
            this.sisuPood.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sisuPood.BackColor = System.Drawing.Color.White;
            this.sisuPood.Controls.Add(this.lblPood);
            this.sisuPood.Controls.Add(this.ridaPood);
            this.sisuPood.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.sisuPood.Location = new System.Drawing.Point(8, 6);
            this.sisuPood.Margin = new System.Windows.Forms.Padding(0);
            this.sisuPood.Name = "sisuPood";
            this.sisuPood.Size = new System.Drawing.Size(180, 55);
            this.sisuPood.TabIndex = 219;
            this.sisuPood.WrapContents = false;
            // 
            // lblPood
            // 
            this.lblPood.AutoSize = true;
            this.lblPood.BackColor = System.Drawing.Color.White;
            this.lblPood.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblPood.ForeColor = System.Drawing.Color.FromArgb(110, 116, 125);
            this.lblPood.Margin = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblPood.Name = "lblPood";
            this.lblPood.Size = new System.Drawing.Size(50, 13);
            this.lblPood.TabIndex = 220;
            this.lblPood.Text = "POOD";
            // 
            // ridaPood
            // 
            this.ridaPood.AutoSize = true;
            this.ridaPood.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ridaPood.BackColor = System.Drawing.Color.White;
            this.ridaPood.Controls.Add(this.btnPood);
            this.ridaPood.Location = new System.Drawing.Point(0, 15);
            this.ridaPood.Margin = new System.Windows.Forms.Padding(0);
            this.ridaPood.Name = "ridaPood";
            this.ridaPood.Size = new System.Drawing.Size(180, 40);
            this.ridaPood.TabIndex = 221;
            this.ridaPood.WrapContents = false;
            // 
            // btnPood
            // 
            this.btnPood.AutoSize = true;
            this.btnPood.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPood.BackColor = System.Drawing.Color.White;
            this.btnPood.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPood.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.btnPood.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPood.ForeColor = System.Drawing.Color.FromArgb(40, 40, 40);
            this.btnPood.Margin = new System.Windows.Forms.Padding(3);
            this.btnPood.MinimumSize = new System.Drawing.Size(0, 30);
            this.btnPood.Name = "btnPood";
            this.btnPood.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.btnPood.Radius = 8;
            this.btnPood.Size = new System.Drawing.Size(60, 30);
            this.btnPood.TabIndex = 222;
            this.btnPood.Text = "Pood";
            this.btnPood.UseVisualStyleBackColor = false;
            // 
            // kaartKokku
            // 
            this.kaartKokku.AutoSize = true;
            this.kaartKokku.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.kaartKokku.BackColor = System.Drawing.Color.White;
            this.kaartKokku.BorderColor = System.Drawing.Color.FromArgb(210, 214, 220);
            this.kaartKokku.Controls.Add(this.sisuKokku);
            this.kaartKokku.FillColor = System.Drawing.Color.White;
            this.kaartKokku.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.kaartKokku.Name = "kaartKokku";
            this.kaartKokku.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            this.kaartKokku.Radius = 12;
            this.kaartKokku.Size = new System.Drawing.Size(200, 70);
            this.kaartKokku.TabIndex = 223;
            // 
            // sisuKokku
            // 
            this.sisuKokku.AutoSize = true;
            this.sisuKokku.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.sisuKokku.BackColor = System.Drawing.Color.White;
            this.sisuKokku.Controls.Add(this.lblKokku);
            this.sisuKokku.Controls.Add(this.ridaKokku);
            this.sisuKokku.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.sisuKokku.Location = new System.Drawing.Point(8, 6);
            this.sisuKokku.Margin = new System.Windows.Forms.Padding(0);
            this.sisuKokku.Name = "sisuKokku";
            this.sisuKokku.Size = new System.Drawing.Size(180, 55);
            this.sisuKokku.TabIndex = 224;
            this.sisuKokku.WrapContents = false;
            // 
            // lblKokku
            // 
            this.lblKokku.AutoSize = true;
            this.lblKokku.BackColor = System.Drawing.Color.White;
            this.lblKokku.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblKokku.ForeColor = System.Drawing.Color.FromArgb(110, 116, 125);
            this.lblKokku.Margin = new System.Windows.Forms.Padding(4, 0, 0, 2);
            this.lblKokku.Name = "lblKokku";
            this.lblKokku.Size = new System.Drawing.Size(50, 13);
            this.lblKokku.TabIndex = 225;
            this.lblKokku.Text = "KOKKU";
            // 
            // ridaKokku
            // 
            this.ridaKokku.AutoSize = true;
            this.ridaKokku.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ridaKokku.BackColor = System.Drawing.Color.White;
            this.ridaKokku.Controls.Add(this.pnlKokku);
            this.ridaKokku.Location = new System.Drawing.Point(0, 15);
            this.ridaKokku.Margin = new System.Windows.Forms.Padding(0);
            this.ridaKokku.Name = "ridaKokku";
            this.ridaKokku.Size = new System.Drawing.Size(180, 40);
            this.ridaKokku.TabIndex = 226;
            this.ridaKokku.WrapContents = false;
            // 
            // pnlKokku
            // 
            this.pnlKokku.BackColor = System.Drawing.Color.White;
            this.pnlKokku.BorderColor = System.Drawing.Color.FromArgb(200, 204, 210);
            this.pnlKokku.Controls.Add(this.txtKokku);
            this.pnlKokku.FillColor = System.Drawing.Color.White;
            this.pnlKokku.Margin = new System.Windows.Forms.Padding(3);
            this.pnlKokku.Name = "pnlKokku";
            this.pnlKokku.Radius = 8;
            this.pnlKokku.Size = new System.Drawing.Size(150, 30);
            this.pnlKokku.TabIndex = 227;
            // 
            // txtKokku
            // 
            this.txtKokku.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKokku.BackColor = System.Drawing.Color.White;
            this.txtKokku.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtKokku.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.txtKokku.Location = new System.Drawing.Point(10, 7);
            this.txtKokku.Name = "txtKokku";
            this.txtKokku.ReadOnly = true;
            this.txtKokku.Size = new System.Drawing.Size(130, 17);
            this.txtKokku.TabIndex = 0;
            this.txtKokku.TabStop = false;
            this.txtKokku.Text = "0.00 €";
            this.txtKokku.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(235, 237, 240);
            this.ClientSize = new System.Drawing.Size(1000, 620);
            this.Controls.Add(this.pnlSisu);
            this.Controls.Add(this.pnlJoon);
            this.Controls.Add(this.pnlKassaRibbon);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlTabid);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Minu Epood";
            ((System.ComponentModel.ISupportInitialize)(this.picPilt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridTooted)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel pnlTop;
        private RoundedPanel kaartToode;
        private System.Windows.Forms.FlowLayoutPanel sisuToode;
        private System.Windows.Forms.Label lblToode;
        private System.Windows.Forms.FlowLayoutPanel ridaToode;
        private RoundedButton btnLisa;
        private RoundedButton btnUuenda;
        private RoundedButton btnKustuta;
        private RoundedPanel kaartKategooria;
        private System.Windows.Forms.FlowLayoutPanel sisuKategooria;
        private System.Windows.Forms.Label lblKategooria;
        private System.Windows.Forms.FlowLayoutPanel ridaKategooria;
        private RoundedButton btnLisaKategooria;
        private RoundedButton btnKustutaKategooria;
        private RoundedPanel kaartPilt;
        private System.Windows.Forms.FlowLayoutPanel sisuPilt;
        private System.Windows.Forms.Label lblPilt;
        private System.Windows.Forms.FlowLayoutPanel ridaPilt;
        private RoundedButton btnOtsiFail;
        private System.Windows.Forms.Panel pnlJoon;
        private System.Windows.Forms.Panel pnlSisu;
        private System.Windows.Forms.Panel pnlVahe;
        private RoundedPanel kaartSisestus;
        private System.Windows.Forms.Label lblSisestusPealkiri;
        private System.Windows.Forms.TableLayoutPanel tblVali;
        private System.Windows.Forms.Label lblNimetus;
        private RoundedPanel pnlNimetus;
        private System.Windows.Forms.TextBox txtNimetus;
        private System.Windows.Forms.Label lblKogus;
        private RoundedPanel pnlKogus;
        private System.Windows.Forms.TextBox txtKogus;
        private System.Windows.Forms.Label lblHind;
        private RoundedPanel pnlHind;
        private System.Windows.Forms.TextBox txtHind;
        private System.Windows.Forms.Label lblKat;
        private RoundedPanel pnlKategooria;
        private System.Windows.Forms.ComboBox cmbKategooria;
        private RoundedPanel pnlPilt;
        private System.Windows.Forms.PictureBox picPilt;
        private RoundedPanel kaartTabel;
        private System.Windows.Forms.Label lblTabeliPealkiri;
        private System.Windows.Forms.DataGridView dataGridTooted;
        private System.Windows.Forms.Panel pnlTabid;
        private RibbonTab tabToode;
        private RibbonTab tabKassa;
        private System.Windows.Forms.FlowLayoutPanel pnlKassaRibbon;
        private RoundedButton btnPuhasta;
        private RoundedPanel kaartOst;
        private System.Windows.Forms.FlowLayoutPanel sisuOst;
        private System.Windows.Forms.Label lblOst;
        private System.Windows.Forms.FlowLayoutPanel ridaOst;
        private RoundedButton btnValin;
        private RoundedButton btnOstan;
        private RoundedPanel kaartArve;
        private System.Windows.Forms.FlowLayoutPanel sisuArve;
        private System.Windows.Forms.Label lblArve;
        private System.Windows.Forms.FlowLayoutPanel ridaArve;
        private RoundedButton btnSaadaArve;
        private RoundedButton btnMaksta;
        private RoundedPanel kaartPood;
        private System.Windows.Forms.FlowLayoutPanel sisuPood;
        private System.Windows.Forms.Label lblPood;
        private System.Windows.Forms.FlowLayoutPanel ridaPood;
        private RoundedButton btnPood;
        private RoundedPanel kaartKokku;
        private System.Windows.Forms.FlowLayoutPanel sisuKokku;
        private System.Windows.Forms.Label lblKokku;
        private System.Windows.Forms.FlowLayoutPanel ridaKokku;
        private RoundedPanel pnlKokku;
        private System.Windows.Forms.TextBox txtKokku;
    }
}
