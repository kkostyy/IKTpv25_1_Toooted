namespace IKTpv25_1_Toooted
{
    // Menüü ja rollid: Omanik näeb vahelehti Tooted ja Kassa, Müüja ainult Kassa
    public partial class Form1
    {
        private readonly ToolStripMenuItem menuRegistreeri = new("Registreeri kasutaja...");
        private readonly ToolStripMenuItem menuKliendid = new("Kliendid...");
        private readonly ToolStripMenuItem menuStatistika = new("Statistika...");
        private readonly ToolStripMenuItem menuEksport = new("Ekspordi tooted (CSV)...");
        private readonly ToolStripMenuItem menuEmail = new("E-posti seaded...");
        private readonly ToolStripMenuItem menuMakse = new("Makse seaded...");
        private readonly ToolStripMenuItem menuKasutajad = new("Kasutajad...");
        private readonly ToolStripMenuItem menuKampaaniad = new("Kampaaniad...");
        private readonly ToolStripMenuItem menuDemo = new("Demoandmed...");
        private readonly ToolStripMenuItem menuMyygipaneel = new("Müügipaneel...");
        private readonly ToolStripMenuItem menuPartiid = new("Partiid ja säilivusaeg...");
        private readonly ToolStripMenuItem menuOhtu = new("Õhtune soodustus...");
        private readonly ToolStripStatusLabel lblKasutaja = new();
        private string? algneTiitel;
        private Point tabKassaAlgne;

        private void LooMenuu()
        {
            var menuu = new MenuStrip { BackColor = UiStiil.PaneeliTaust };

            var kasutaja = new ToolStripMenuItem("Kasutaja");
            kasutaja.DropDownItems.Add("Seaded...", null, (s, e) => SeadedVorm.Ava(this));
            kasutaja.DropDownItems.Add(new ToolStripMenuItem("Ostunimekiri...", null, (s, e) => AvaNimekiri()) { ShortcutKeys = Keys.Control | Keys.L });
            kasutaja.DropDownItems.Add(new ToolStripSeparator());
            kasutaja.DropDownItems.Add("Vaheta kasutajat...", null, (s, e) => VahetaKasutajat());
            kasutaja.DropDownItems.Add("Muuda parooli...", null, (s, e) => AvaVorm(new ParooliMuutmineVorm()));
            kasutaja.DropDownItems.Add(menuRegistreeri);
            kasutaja.DropDownItems.Add(new ToolStripSeparator());
            kasutaja.DropDownItems.Add("Välju", null, (s, e) => Close());
            menuRegistreeri.Click += (s, e) => AvaVorm(new RegistreerimineVorm(lubaOmanik: true));

            var haldus = new ToolStripMenuItem("Haldus");
            haldus.DropDownItems.Add("Ostude ajalugu...", null, (s, e) => AvaAjalugu());
            haldus.DropDownItems.Add(menuKasutajad);
            haldus.DropDownItems.Add(menuKliendid);
            menuKasutajad.Click += (s, e) => AvaVorm(new KasutajadVorm());
            haldus.DropDownItems.Add(menuKampaaniad);
            menuKampaaniad.Click += (s, e) => AvaKampaaniad();
            haldus.DropDownItems.Add(menuMyygipaneel);
            haldus.DropDownItems.Add(menuPartiid);
            haldus.DropDownItems.Add(menuOhtu);
            menuOhtu.Click += (s, e) => AvaVorm(new OhtuSeadedVorm());
            menuPartiid.Click += (s, e) => AvaPartiid();
            menuMyygipaneel.Click += (s, e) => AvaVorm(new MyygipaneelVorm());
            haldus.DropDownItems.Add(menuDemo);
            menuDemo.Click += (s, e) => AvaDemo();
            haldus.DropDownItems.Add(menuStatistika);
            haldus.DropDownItems.Add(menuEksport);
            haldus.DropDownItems.Add(menuEmail);
            haldus.DropDownItems.Add(menuMakse);
            menuMakse.Click += (s, e) => AvaVorm(new MakseSeadedVorm());
            menuEksport.Click += (s, e) => CsvEksport.Tooted(this);
            menuEmail.Click += (s, e) => AvaVorm(new EmailSeadedVorm());
            menuKliendid.Click += (s, e) => AvaVorm(new KliendidVorm());
            menuStatistika.Click += (s, e) => AvaVorm(new StatistikaVorm());

            menuu.Items.Add(kasutaja);
            menuu.Items.Add(haldus);
            menuu.Items.Add(LooVaadeMenuu());
            Controls.Add(menuu);
            MainMenuStrip = menuu;
            menuu.SendToBack();   // menüü kõige üles

            LooEpoePais();

            // Olekuriba all: kes on sisse logitud
            var olek = new StatusStrip { BackColor = UiStiil.PaneeliTaust };
            olek.Items.Add(lblKasutaja);
            Controls.Add(olek);

            // Külgmenüü paremal: menüü, päise ja olekuriba vahel (dokkimise järjekord)
            LooKulgMenuu();
            Controls.Add(kulg);
            int koht = new[] { Controls.GetChildIndex(menuu), Controls.GetChildIndex(olek),
                               Controls.GetChildIndex(epoePais), Controls.GetChildIndex(epoeRiba) }.Min();
            Controls.SetChildIndex(kulg, koht);
            ClientSize = new Size(ClientSize.Width + KulgMenuu.MenuuLaius, ClientSize.Height);

            Shown += (s, e) => RakendaRoll();
        }

        // Peidab/näitab vahelehed ja menüüpunktid vastavalt rollile
        private void RakendaRoll()
        {
            algneTiitel ??= Text;
            if (tabKassaAlgne == Point.Empty) tabKassaAlgne = tabKassa.Location;

            bool omanik = AktiivneKasutaja.OnOmanik;

            tabToode.Visible = omanik;
            tabKassa.Location = omanik ? tabKassaAlgne : tabToode.Location;   // ei jää auku
            menuRegistreeri.Visible = omanik;
            menuKliendid.Visible = omanik;
            menuStatistika.Visible = omanik;
            menuEksport.Visible = omanik;
            menuEmail.Visible = omanik;
            menuMakse.Visible = omanik;
            menuKasutajad.Visible = omanik;
            menuKampaaniad.Visible = omanik;
            menuDemo.Visible = omanik;
            menuMyygipaneel.Visible = omanik;
            menuPartiid.Visible = omanik;
            menuOhtu.Visible = omanik;

            // Ostja (klient) ei näe kassat ega ladu – talle avaneb kohe e-pood
            if (AktiivneKasutaja.OnOstja)
            {
                Opacity = 0;
                BeginInvoke(new Action(OstjaReziim));
                return;
            }
            Opacity = 1;

            ValiVaheleht(omanik ? tabToode : tabKassa);

            UuendaKeeleTekstid();
        }

        // Kasutaja nimega tekstid valitud keeles (ka keele vahetusel)
        private void UuendaKeeleTekstid()
        {
            if (algneTiitel == null || AktiivneKasutaja.Nimi.Length == 0) return;
            string roll = Keel.T(AktiivneKasutaja.Roll);
            Text = $"{Keel.T(algneTiitel)} – {AktiivneKasutaja.Nimi} ({roll})";
            lblKasutaja.Text = Keel.F("Sisse logitud: {0}   |   Roll: {1}", AktiivneKasutaja.Nimi, roll);
            lblTere.Text = Keel.F("Tere, {0}!   ·   {1}   ·   {2}", AktiivneKasutaja.Kuvanimi, roll, DateTime.Now.ToString("dd.MM.yyyy"));
            UuendaOtsinguKategooriad();
            NaitaTooted();
        }

        // E-poe stiilis päis: lilla riba + valge päis logoga (luuakse koodis, Designer-faili ei muudeta)
        private readonly Label lblTere = new();

        private Panel epoePais = null!, epoeRiba = null!;
        private Label epoePealkiri = null!;

        private void LooEpoePais()
        {
            var pais = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Teema.Pind };
            epoePais = pais;
            var logo = new Logo { Location = new Point(18, 11), Size = new Size(116, 46) };
            var pealkiri = epoePealkiri = new Label
            {
                Text = "Kassa ja ladu",
                Location = new Point(156, 22),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 13F),
                ForeColor = Teema.Teine,
                BackColor = Teema.Pind
            };
            var pood = UiStiil.LooSuurNupp("Ava e-pood  ›", (s, e) => AvaPood());
            pood.Size = new Size(170, 42);
            pood.Radius = 6;
            pood.Font = new Font("Segoe UI Semibold", 10F);
            var seaded = UiStiil.LooSuurNupp("⚙  Seaded", (s, e) => SeadedVorm.Ava(this), peamine: false);
            seaded.Size = new Size(120, 42);
            seaded.Radius = 6;
            var nimekiri = UiStiil.LooSuurNupp("☑  Ostunimekiri", (s, e) => AvaNimekiri(), peamine: false);
            nimekiri.Size = new Size(160, 42);
            nimekiri.Radius = 6;
            pais.Controls.AddRange(new Control[] { logo, pealkiri, nimekiri, seaded, pood });
            pais.Resize += (s, e) =>
            {
                pood.Location = new Point(pais.Width - pood.Width - 18, 13);
                seaded.Location = new Point(pood.Left - seaded.Width - 10, 13);
                nimekiri.Location = new Point(seaded.Left - nimekiri.Width - 10, 13);
            };
            pais.Paint += (s, e) =>
            {
                using var p = new Pen(Teema.Joon);
                e.Graphics.DrawLine(p, 0, pais.Height - 1, pais.Width, pais.Height - 1);
            };

            var lilla = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Teema.Teine };
            epoeRiba = lilla;
            lblTere.AutoSize = true;
            lblTere.Location = new Point(18, 9);
            lblTere.ForeColor = Color.White;
            lblTere.BackColor = Teema.Teine;
            lblTere.Font = new Font("Segoe UI Semibold", 9F);
            lilla.Controls.Add(lblTere);
            var keeled = new KeeleLingid(Color.White) { BackColor = Teema.Teine };
            lilla.Controls.Add(keeled);
            lilla.Resize += (s, e) => keeled.Location = new Point(lilla.Width - keeled.Width - 20, (lilla.Height - keeled.Height) / 2);

            // Menüü on juba kõige üleval; päis ja lilla riba tulevad selle kohale
            Controls.Add(pais);
            pais.SendToBack();
            Controls.Add(lilla);
            lilla.SendToBack();

            ClientSize = new Size(ClientSize.Width, ClientSize.Height + lilla.Height + pais.Height);
            NaitaDisain();
        }

        // E-poe päis on nähtav ainult Rimi disainis; klassikalises on esimene (Wordi-laadne) vaade
        private void NaitaDisain()
        {
            epoePais.Visible = epoeRiba.Visible = Teema.OnRimi && !moodne;
            epoeRiba.BackColor = lblTere.BackColor = Teema.Teine;
            epoePealkiri.ForeColor = Teema.Teine;
            epoePais.Invalidate(true);
        }

        // Vaade: hele/tume teema ja poe värv
        private readonly ToolStripMenuItem menuHele = new("Hele teema");
        private readonly ToolStripMenuItem menuTume = new("Tume teema");
        private readonly List<ToolStripMenuItem> menuVarvid = new();
        private readonly ToolStripMenuItem menuDisainRimi = new("E-poe disain (Rimi)");
        private readonly List<ToolStripMenuItem> menuKeeled = new();
        private readonly ToolStripMenuItem menuDisainKlassik = new("Klassikaline disain");
        private readonly ToolStripMenuItem menuDisainMoodne = new("Moodne disain") { ShortcutKeyDisplayString = "Ctrl+Shift+M" };

        private ToolStripMenuItem LooVaadeMenuu()
        {
            var vaade = new ToolStripMenuItem("Vaade");
            menuHele.Click += (s, e) => Teema.Sea(false, Teema.AktsendiNimi);
            menuTume.Click += (s, e) => Teema.Sea(true, Teema.AktsendiNimi);
            vaade.DropDownItems.Add(menuHele);
            vaade.DropDownItems.Add(menuTume);
            vaade.DropDownItems.Add(new ToolStripSeparator());

            var varv = new ToolStripMenuItem("Poe värv");
            foreach (string nimi in Teema.Aktsendid.Keys)
            {
                var m = new ToolStripMenuItem(nimi) { Tag = nimi };
                m.Click += (s, e) => Teema.Sea(Teema.Tume, nimi);
                // Väike värviruut menüüs
                var ruut = new Bitmap(16, 16);
                using (var g = Graphics.FromImage(ruut))
                using (var b = new SolidBrush(Teema.Aktsendid[nimi].Hele))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.FillEllipse(b, 1, 1, 14, 14);
                }
                m.Image = ruut;
                menuVarvid.Add(m);
                varv.DropDownItems.Add(m);
            }
            vaade.DropDownItems.Add(varv);
            vaade.DropDownItems.Add(new ToolStripSeparator());
            menuDisainRimi.ShortcutKeyDisplayString = "Ctrl+Shift+D";
            menuDisainKlassik.ShortcutKeyDisplayString = "Ctrl+Shift+D";
            menuDisainRimi.Click += (s, e) => Teema.SeaDisain(Disain.Rimi);
            menuDisainKlassik.Click += (s, e) => Teema.SeaDisain(Disain.Klassikaline);
            menuDisainMoodne.Click += (s, e) => Teema.SeaDisain(Disain.Moodne);
            vaade.DropDownItems.Add(menuDisainRimi);
            vaade.DropDownItems.Add(menuDisainKlassik);
            vaade.DropDownItems.Add(menuDisainMoodne);
            vaade.DropDownItems.Add(new ToolStripSeparator());

            // Keel: nimed alati oma keeles (Eesti / Русский / English)
            var keel = new ToolStripMenuItem("Keel");
            foreach (KeeleKood k in Enum.GetValues<KeeleKood>())
            {
                var m = new ToolStripMenuItem(Keel.Nimi(k)) { Tag = k };
                m.Click += (s, e) => Keel.Sea(k);
                menuKeeled.Add(m);
                keel.DropDownItems.Add(m);
            }
            vaade.DropDownItems.Add(keel);

            Teema.Muutus += MargiVaade;
            FormClosed += (s, e) => Teema.Muutus -= MargiVaade;
            MargiVaade();
            return vaade;
        }

        private void MargiVaade()
        {
            menuHele.Checked = !Teema.Tume;
            menuTume.Checked = Teema.Tume;
            foreach (var m in menuVarvid) m.Checked = Equals(m.Tag, Teema.AktsendiNimi);
            menuDisainRimi.Checked = Teema.Stiil == Disain.Rimi;
            menuDisainKlassik.Checked = !Teema.OnRimi;
            menuDisainMoodne.Checked = Teema.OnMoodne;
            foreach (var m in menuKeeled) m.Checked = Equals(m.Tag, Keel.Praegu);
        }

        // Ostja: ainult e-pood. Poe sulgemisel küsitakse uuesti sisselogimist.
        private bool ostjaReziimis;

        private void OstjaReziim()
        {
            if (ostjaReziimis) return;
            ostjaReziimis = true;
            Hide();
            try
            {
                while (AktiivneKasutaja.OnOstja)
                {
                    AvaPood();
                    using var login = new LoginVorm();
                    login.StartPosition = FormStartPosition.CenterScreen;
                    login.ShowInTaskbar = true;
                    if (login.ShowDialog() != DialogResult.OK) { Close(); return; }
                }
            }
            finally { ostjaReziimis = false; }

            Show();
            RakendaRoll();   // töötaja logis sisse
        }

        // Ümar külgmenüü (Vaade → seadetes saab välja lülitada)
        private KulgMenuu? kulg;

        // Kampaaniate haldus; pärast sulgemist on hinnad muutunud
        private void AvaKampaaniad()
        {
            AvaVorm(new KampaaniadVorm(), LaadiTooted);
        }

        private void AvaPartiid()
        {
            AvaVorm(new AegumineVorm(), LaadiTooted);
        }

        private void AvaDemo()
        {
            AvaVorm(new DemoVorm(), LaadiTooted);
        }

        private void LooKulgMenuu()
        {
            kulg = new KulgMenuu();
            kulg.Lisa("tooted", "\uE80F", "Tooted", () => ValiVaheleht(tabToode), nahtav: () => AktiivneKasutaja.OnOmanik);
            kulg.Lisa("kassa", "\uE7BF", "Kassa", () => ValiVaheleht(tabKassa), () => Ostukorv.Ridad.Sum(r => r.Kogus));
            kulg.Lisa("pood", "\uE719", "Ava e-pood", AvaPood);
            kulg.Lisa("kaart", "\uE8C7", "Kliendikaart", () => KliendikaartVorm.Ava(this));
            kulg.Lisa("nimekiri", "\uE8FD", "Ostunimekiri", AvaNimekiri, Ostunimekiri.OstmataArv);
            kulg.Lisa("ajalugu", "\uE81C", "Ostude ajalugu", AvaAjalugu);
            kulg.Lisa("kliendid", "\uE716", "Kliendid", () => AvaVorm(new KliendidVorm()), nahtav: () => AktiivneKasutaja.OnOmanik);
            kulg.Lisa("kasutajad", "\uE77B", "Kasutajad", () => AvaVorm(new KasutajadVorm()), nahtav: () => AktiivneKasutaja.OnOmanik);
            kulg.Lisa("kampaaniad", "\uE8EC", "Kampaaniad", AvaKampaaniad, nahtav: () => AktiivneKasutaja.OnOmanik);
            kulg.Lisa("myyk", "\uE9F9", "Müügipaneel", () => AvaVorm(new MyygipaneelVorm()), nahtav: () => AktiivneKasutaja.OnOmanik);
            kulg.Lisa("partiid", "\uE823", "Partiid", AvaPartiid, nahtav: () => AktiivneKasutaja.OnOmanik);
            kulg.Lisa("tellimus", "\uE7B8", "Autotellimus", () => AvaVorm(new AutotellimusVorm()), nahtav: () => AktiivneKasutaja.OnOmanik);
            kulg.Lisa("koos", "\uE8EC", "Ostetakse koos", () => AvaVorm(new OstukorviAnalyysVorm()), nahtav: () => AktiivneKasutaja.OnOmanik);
            kulg.Lisa("demo", "\uE7B8", "Demoandmed", AvaDemo, nahtav: () => AktiivneKasutaja.OnOmanik);
            kulg.Lisa("statistika", "\uE9D2", "Statistika", () => AvaVorm(new StatistikaVorm()), nahtav: () => AktiivneKasutaja.OnOmanik);
            kulg.Lisa("seaded", "\uE713", "Seaded", () => SeadedVorm.Ava(this));
            kulg.Valju = VahetaKasutajat;
            kulg.ValjuTekst = "Vaheta kasutajat";
            kulg.Profiil = () => SeadedVorm.Ava(this);
            kulg.LogoKlikk = AvaPood;
        }

        // Aktiivne vaheleht külgmenüüs
        private void MargiKulg(string votti)
        {
            if (kulg == null) return;
            kulg.Aktiivne = votti;
            kulg.UuendaMargid();
        }

        private void AvaNimekiri() => AvaVorm(new OstunimekiriVorm());

        private void VahetaKasutajat()
        {
            using var login = new LoginVorm();
            if (login.ShowDialog(this) == DialogResult.OK)
                RakendaRoll();
        }

        // Ostude ajalugu: kõik näevad ja saavad tšeki printida, tagastada saab ainult omanik
        private void AvaAjalugu()
        {
            var vorm = new OstudVorm(lubaTagastus: AktiivneKasutaja.OnOmanik);
            AvaVorm(vorm, () =>
            {
                foreach (int id in vorm.TagastatudOstud) OstTagastati(id);
                if (vorm.TagastatudOstud.Count > 0) LaadiTooted();
            });
        }

        // Moodsas disainis avaneb vorm peaakna sees, muidu dialoogina; "parast" pärast sulgemist
        private void AvaVorm(Form vorm, Action? parast = null) => Aken.Ava(this, vorm, parast);
    }
}
