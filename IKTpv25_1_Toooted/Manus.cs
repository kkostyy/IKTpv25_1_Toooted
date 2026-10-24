namespace IKTpv25_1_Toooted;

// Aken, mis oskab teisi vorme enda sisse paigutada (üks aken kogu programmile, moodne disain)
public interface IVormiHost
{
    bool VoibManustada { get; }
    void Manusta(Form vorm, Action? suletud);
}

// Vormide avamine: moodsas disainis avaneb vorm peaakna sees (nagu leht), muidu dialoogina.
// "suletud" käivitatakse pärast sulgemist mõlemal juhul.
public static class Aken
{
    public static void Ava(IWin32Window? omanik, Form vorm, Action? suletud = null)
    {
        if (Teema.OnMoodne && Host(omanik) is IVormiHost h)
        {
            h.Manusta(vorm, suletud);
            return;
        }
        using (vorm)
        {
            if (omanik != null) vorm.ShowDialog(omanik);
            else { vorm.StartPosition = FormStartPosition.CenterScreen; vorm.ShowDialog(); }
        }
        suletud?.Invoke();
    }

    private static IVormiHost? Host(IWin32Window? omanik)
    {
        Control? c = omanik as Control ?? Form.ActiveForm;
        // lähim host vanemate ahelas (pood peaakna sees → pood ise)
        for (var x = c; x != null; x = x.Parent)
            if (x is IVormiHost h && h.VoibManustada && x.Visible && !x.IsDisposed) return h;
        if (c?.FindForm() is Form f && f.Owner is IVormiHost oh && oh.VoibManustada) return oh;
        return null;
    }

    // Paigutab vormi konteinerisse: ilma raamita, muudetava suurusega vorm täidab ala, teised on keskel
    public static void Paigalda(Form vorm, Control konteiner, Action valmis)
    {
        bool taida = vorm.FormBorderStyle is FormBorderStyle.Sizable or FormBorderStyle.SizableToolWindow;
        vorm.TopLevel = false;
        vorm.FormBorderStyle = FormBorderStyle.None;
        vorm.ShowInTaskbar = false;
        if (taida) vorm.Dock = DockStyle.Fill;
        else
        {
            void Keskele() => vorm.Location = new Point(Math.Max(0, (konteiner.ClientSize.Width - vorm.Width) / 2), 0);
            konteiner.Resize += (s, e) => { if (!vorm.IsDisposed) Keskele(); };
            Keskele();
        }

        // Dialoogi tulemus (OK / Cancel) sulgeb ka manustatud vormi
        void Jalgi(object? s, EventArgs e)
        {
            if (vorm.IsDisposed) { Application.Idle -= Jalgi; return; }
            if (vorm.DialogResult != DialogResult.None) vorm.Close();
        }
        Application.Idle += Jalgi;
        vorm.FormClosed += (s, e) =>
        {
            Application.Idle -= Jalgi;
            konteiner.Controls.Remove(vorm);
            valmis();
        };
        konteiner.Controls.Add(vorm);
        vorm.Show();
        vorm.BringToFront();
        vorm.Focus();
    }
}

// Poe leht, mille sees on vorm (seaded, kliendikaart, ajalugu ...)
public class VormiLeht : Leht
{
    private readonly Form vorm;
    private readonly Action? suletud;
    private readonly Action tagasi;
    private bool kinni;

    public override string Pealkiri { get; }

    public VormiLeht(Form vorm, Action? suletud, Action tagasi)
    {
        this.vorm = vorm;
        this.suletud = suletud;
        this.tagasi = tagasi;
        Pealkiri = vorm.Text;
        AutoScroll = true;
        Padding = new Padding(16, 0, 16, 16);
        Aken.Paigalda(vorm, this, () =>
        {
            bool iseSulges = !kinni;
            kinni = true;
            suletud?.Invoke();
            if (iseSulges) tagasi();   // vorm sulges end ise (Tagasi, Salvesta) → leht pinust maha
        });
    }

    // Leht eemaldati pinust (leivapuru, Tagasi): vorm suletakse
    public override void Suletud()
    {
        if (kinni || vorm.IsDisposed) return;
        kinni = true;
        vorm.Close();
    }
}
