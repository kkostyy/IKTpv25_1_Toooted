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
        if (m.Msg != WM_KEYDOWN || (Keys)(int)m.WParam != Keys.F12 || Form.ActiveForm is not Form f) return false;
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
            foreach (Control l in c.Controls) Kirjuta(l, tase + 1);
        }
        Kirjuta(f, 0);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "akt_puu.txt"), sb.ToString());
        return true;
    }
}
