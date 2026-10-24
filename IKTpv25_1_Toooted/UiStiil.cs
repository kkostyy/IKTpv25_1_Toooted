namespace IKTpv25_1_Toooted
{
    // Ühtne kasutajaliidese stiil. Värvid tulevad teemast (Teema.cs), seega
    // hele/tume teema ja poe värv kehtivad kõikjal.
    public static class UiStiil
    {
        public static Color Sinine => Teema.Aktsent;          // aktsentvärv (nimi jäi ajaloolistel põhjustel)
        public static Color PaneeliTaust => Teema.Paneel;
        public static Color ToaTaust => Teema.Taust;
        public static Color Joon => Teema.Joon;

        public static readonly Font Põhifont = new Font("Segoe UI", 9F);
        public static readonly Font PealkirjaFont = new Font("Segoe UI Semibold", 15F);
        public static readonly Font AlapealkiriFont = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        public static FlowLayoutPanel LooTopPaneel()
        {
            return new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                Padding = new Padding(6),
                BackColor = PaneeliTaust
            };
        }

        // Paneeli nupud on alati ühel real: aken laiendatakse nii, et kõik rühmad mahuksid ära
        public static void NupudUhelReal(Form vorm, FlowLayoutPanel paneel)
        {
            paneel.WrapContents = false;
            int raam = vorm.Width - vorm.ClientSize.Width;
            int vajalik = Math.Min(paneel.GetPreferredSize(Size.Empty).Width,
                Screen.PrimaryScreen!.WorkingArea.Width - raam);

            vorm.MinimumSize = new Size(vajalik + raam, vorm.MinimumSize.Height);
            if (vorm.ClientSize.Width < vajalik)
            {
                vorm.ClientSize = new Size(vajalik, vorm.ClientSize.Height);
            }
        }

        public static Panel LooEraldusjoon()
        {
            return new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Joon };
        }

        public static Button LooNupp(string tekst, EventHandler? kasitleja, bool rohutatud = false)
        {
            var nupp = new RoundedButton
            {
                Text = tekst,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(0, 32),
                Margin = new Padding(3),
                Padding = new Padding(8, 0, 8, 0),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Peamine = rohutatud,
                Cursor = Cursors.Hand
            };
            if (kasitleja != null) nupp.Click += kasitleja;
            return nupp;
        }

        // Suur nupp (nt "Vormista ost")
        public static RoundedButton LooSuurNupp(string tekst, EventHandler? kasitleja, bool peamine = true)
        {
            var nupp = (RoundedButton)LooNupp(tekst, kasitleja, peamine);
            nupp.AutoSize = false;
            nupp.Height = 44;
            nupp.Radius = 10;
            nupp.Font = new Font("Segoe UI Semibold", 11F);
            return nupp;
        }

        // Ümardatud raamiga sisestusväli (nagu peavormil): raam läheb fookuses aktsentvärvi
        public static RoundedPanel LooSisend(out TextBox tekst, int laius, bool parool = false, string vihje = "")
        {
            var raam = new RoundedPanel
            {
                Size = new Size(laius, 34),
                Radius = 8,
                Padding = new Padding(10, 8, 10, 0),
                BorderColor = Teema.Raam,
                FillColor = Teema.Pind,
                Margin = new Padding(0, 0, 0, 4)
            };
            var t = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                UseSystemPasswordChar = parool,
                PlaceholderText = vihje,
                BackColor = Teema.Pind,
                ForeColor = Teema.Tekst,
                Font = new Font("Segoe UI", 10F)
            };
            raam.Controls.Add(t);
            LisaFookus(raam, t);
            tekst = t;
            return raam;
        }

        public static void LisaFookus(RoundedPanel raam, Control sisu)
        {
            sisu.Enter += (s, e) => { raam.BorderColor = Teema.Aktsent; raam.Invalidate(); };
            sisu.Leave += (s, e) => { raam.BorderColor = Teema.Raam; raam.Invalidate(); };
            raam.Click += (s, e) => sisu.Focus();
        }

        // Kaart: ümardatud valge paneel, ülal väike pealkiri, sees sisu (täidab ülejäänud ruumi)
        public static RoundedPanel LooKaardiPaneel(string pealkiri, Control sisu)
        {
            var kaart = new RoundedPanel { Padding = new Padding(14, 10, 14, 14), Radius = 12 };
            var silt = new Label
            {
                Text = pealkiri.ToUpper(),
                Dock = DockStyle.Top,
                Height = 26,
                Font = AlapealkiriFont,
                ForeColor = Teema.TekstNork,
                BackColor = Teema.Pind
            };
            sisu.Dock = DockStyle.Fill;
            kaart.Controls.Add(sisu);
            kaart.Controls.Add(silt);
            return kaart;
        }

        // Ümardatud "kaart": väike pealkiri + nupud (asendab GroupBox-i)
        public static RoundedPanel LooKaart(string pealkiri, params Control[] elemendid)
        {
            var rida = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0),
                BackColor = Teema.Pind
            };
            rida.Controls.AddRange(elemendid);

            var silt = new Label
            {
                Text = pealkiri,
                AutoSize = true,
                ForeColor = Teema.TekstNork,
                BackColor = Teema.Pind,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Margin = new Padding(4, 0, 0, 2)
            };

            var sisu = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown,
                Margin = new Padding(0),
                BackColor = Teema.Pind
            };
            sisu.Controls.Add(silt);
            sisu.Controls.Add(rida);

            var kaart = new RoundedPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Radius = 12,
                Margin = new Padding(4, 2, 4, 2),
                Padding = new Padding(8, 6, 8, 8)
            };
            kaart.Controls.Add(sisu);
            return kaart;
        }

        public static Label LooSilt(string tekst)
        {
            return new Label { Text = tekst, AutoSize = true, Margin = new Padding(3, 8, 0, 3) };
        }

        public static string FormatAeg(int sekundid)
        {
            sekundid = Math.Max(0, sekundid);
            return $"{sekundid / 60:00}:{sekundid % 60:00}";
        }

        public static void SeadistaSkaleerimine(Form vorm)
        {
            vorm.AutoScaleDimensions = new SizeF(96F, 96F);
            vorm.AutoScaleMode = AutoScaleMode.Dpi;
        }

        public static void SeadistaNupuOlek(Button nupp, bool lubatud)
        {
            nupp.Enabled = lubatud;
            nupp.Cursor = lubatud ? Cursors.Hand : Cursors.Default;
        }

        // ---------------------------------------------------------------
        //  Nupp "← Tagasi" akna vasakus ülanurgas: sulgeb akna (dialoogil Cancel).
        //  Kui akna ülaosas on päisepaneel, läheb nupp sinna; muidu tekib uus päis
        //  ja ülejäänud elemendid nihkuvad allapoole. Esc ja Alt+← sulgevad samuti.
        // ---------------------------------------------------------------
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Form, TagasiNupp> tagasiNupud = new();

        public static TagasiNupp? AknaTagasiNupp(Form vorm) => tagasiNupud.TryGetValue(vorm, out var n) ? n : null;

        public static TagasiNupp LisaTagasiNupp(Form vorm, Label? pealkiri = null, Action? tagasi = null)
        {
            if (AknaTagasiNupp(vorm) is TagasiNupp olemas) return olemas;

            var nupp = new TagasiNupp { TabIndex = 0, Name = "tagasiNupp" };
            nupp.Click += (s, e) => (tagasi ?? (() => SulgeTagasi(vorm)))();
            tagasiNupud.AddOrUpdate(vorm, nupp);

            vorm.SuspendLayout();
            var ylemine = vorm.Controls.Cast<Control>()
                .Where(c => c.Dock == DockStyle.Top && c.Visible)
                .OrderByDescending(c => vorm.Controls.GetChildIndex(c))   // kõige ülemine dokitud
                .FirstOrDefault();

            if (pealkiri == null && ylemine is Panel p && p is not FlowLayoutPanel && p.Height >= 56)
            {
                // Olemasolev päis: vasakul olevad elemendid nihkuvad nupu võrra paremale
                int nihe = nupp.Width + 16;
                foreach (Control c in p.Controls)
                    if (c.Dock == DockStyle.None && c.Left < nupp.Width + 32 && (c.Anchor & AnchorStyles.Right) == 0) c.Left += nihe;
                nupp.Location = new Point(16, (p.Height - nupp.Height) / 2);
                p.Controls.Add(nupp);
            }
            else
            {
                const int korgus = 68;
                var pais = new Panel { Dock = DockStyle.Top, Height = korgus, BackColor = vorm.BackColor, Name = "tagasiPais" };
                nupp.Location = new Point(16, 12);
                pais.Controls.Add(nupp);

                var vabad = vorm.Controls.Cast<Control>().Where(c => c.Dock == DockStyle.None && c != pealkiri).ToList();
                bool dokitud = vorm.Controls.Cast<Control>().Any(c => c.Dock != DockStyle.None);
                if (pealkiri != null)
                {
                    vorm.Controls.Remove(pealkiri);
                    pealkiri.Location = new Point(nupp.Right + 16, (korgus - pealkiri.Height) / 2);
                    pais.Controls.Add(pealkiri);
                }
                int nihe = vabad.Count == 0 ? 0 : pealkiri != null ? Math.Max(0, korgus + 8 - vabad.Min(c => c.Top)) : korgus;
                foreach (var c in vabad)
                    if ((c.Anchor & AnchorStyles.Top) != 0 || (c.Anchor & AnchorStyles.Bottom) == 0) c.Top += nihe;

                int kasv = Math.Max(nihe, dokitud ? korgus : 0);
                vorm.Controls.Add(pais);
                pais.SendToBack();   // dokitakse esimesena → kõige üles
                if (vorm.WindowState == FormWindowState.Normal)
                {
                    if (vorm.MinimumSize.Height > 0) vorm.MinimumSize = new Size(vorm.MinimumSize.Width, vorm.MinimumSize.Height + kasv);
                    vorm.ClientSize = new Size(vorm.ClientSize.Width, vorm.ClientSize.Height + kasv);
                }
            }
            vorm.ResumeLayout(true);

            if (vorm is not ITagasi)
            {
                vorm.KeyPreview = true;
                vorm.KeyDown += (s, e) =>
                {
                    bool esc = e.KeyCode == Keys.Escape && vorm.CancelButton == null && !(vorm.ActiveControl is TextBox t && t.TextLength > 0);
                    if (esc || (e.Alt && e.KeyCode == Keys.Left)) { e.Handled = e.SuppressKeyPress = true; nupp.Vajuta(); }
                };
            }
            return nupp;
        }

        // Sulgemine nupu "Tagasi" järgi (dialoogil Cancel)
        public static void SulgeTagasi(Form vorm)
        {
            if (vorm is ITagasi t) { t.MineTagasi(); return; }
            if (vorm.Modal) vorm.DialogResult = DialogResult.Cancel;
            vorm.Close();
        }
    }
}
