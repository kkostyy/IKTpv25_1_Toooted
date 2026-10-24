namespace IKTpv25_1_Toooted;

// Arendaja abi: kui keskkonnamuutuja AKT_SILUMINE=1, kirjutab F12 aktiivse akna elementide puu
// (tüüp, asukoht, suurus, värvid) faili %TEMP%\akt_puu.txt. Tavakasutuses ei tee midagi.
internal sealed class ElementidePuu : IMessageFilter
{
    private const int WM_KEYDOWN = 0x0100;

    public static void Lisa()
    {
        if (Environment.GetEnvironmentVariable("AKT_SILUMINE") == "1") Application.AddMessageFilter(new ElementidePuu());
    }

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg != WM_KEYDOWN || Form.ActiveForm is not Form f) return false;
        var klahv = (Keys)(int)m.WParam;
        // Ctrl+F1…F11: avab vormid otse (testimiseks ilma hüpikmenüüta), Ctrl+F12 vahetab keelt
        if ((Control.ModifierKeys & Keys.Control) != 0 && klahv is >= Keys.F1 and <= Keys.F12)
        {
            Form? uus = klahv switch
            {
                Keys.F1 => new StatistikaVorm(), Keys.F2 => new DemoVorm(), Keys.F3 => new OhtuSeadedVorm(),
                Keys.F4 => new KasutajadVorm(), Keys.F5 => new EmailSeadedVorm(), Keys.F6 => new RegistreerimineVorm(lubaOmanik: true),
                Keys.F7 => new SeadedVorm(), Keys.F8 => new ParooliMuutmineVorm(), Keys.F9 => new KaardiTasemedVorm(),
                Keys.F10 => new KliendikaartMoodneVorm(), Keys.F11 => new KliendidVorm(), _ => null
            };
            if (uus != null) Aken.Ava(f, uus);
            else Keel.Sea(Keel.Praegu == KeeleKood.ET ? KeeleKood.RU : Keel.Praegu == KeeleKood.RU ? KeeleKood.EN : KeeleKood.ET);
            return true;
        }
        if (klahv != Keys.F12) return false;
        var sb = new System.Text.StringBuilder();
        void Kirjuta(Control c, int tase)
        {
            var p = c.Parent != null ? c.Parent.PointToClient(c.Parent.PointToScreen(c.Location)) : c.Location;
            var ekraan = c.Parent != null ? c.Parent.PointToScreen(c.Location) : c.Location;
            sb.Append(new string(' ', tase * 2)).Append(c.GetType().Name).Append(" '").Append(c.Name).Append("' ")
              .Append($"txt='{(c.Text.Length > 30 ? c.Text[..30] : c.Text)}' ")
              .Append($"ekraan={ekraan.X},{ekraan.Y} suurus={c.Width}x{c.Height} dock={c.Dock} vis={c.Visible} ")
              .Append($"bg={c.BackColor.R},{c.BackColor.G},{c.BackColor.B}")
              .AppendLine(c is RoundedPanel rp ? $" fill={rp.FillColor.R},{rp.FillColor.G},{rp.FillColor.B}" : "");
            if (c is Form fm) sb.AppendLine($"    FORM client={fm.ClientSize} disp={fm.DisplayRectangle} pad={fm.Padding} top={fm.TopLevel} min={fm.MinimumSize} max={fm.MaximumSize} as={fm.AutoScroll}");
            if (c is DataGridView d && d.Rows.Count > 0)
                sb.AppendLine($"    DGV def={d.DefaultCellStyle.BackColor} rows={d.RowsDefaultCellStyle.BackColor} alt={d.AlternatingRowsDefaultCellStyle.BackColor} r0={d.Rows[0].DefaultCellStyle.BackColor} r0inh={d.Rows[0].InheritedStyle.BackColor} c00={d.Rows[0].Cells[0].Style.BackColor} fore={d.Rows[0].InheritedStyle.ForeColor}");
            foreach (Control l in c.Controls) Kirjuta(l, tase + 1);
        }
        Kirjuta(f, 0);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "akt_puu.txt"), sb.ToString());
        return true;
    }
}
