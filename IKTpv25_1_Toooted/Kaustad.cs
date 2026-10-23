namespace IKTpv25_1_Toooted;

// Pildid ja arved hoitakse kaustas AppData\Local\ToodedApp
// (Program Files, kuhu installer rakenduse paneb, ei ole tavakasutajale kirjutatav)
public static class Kaustad
{
    public static readonly string Juur = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ToodedApp");
    public static readonly string Pildid = Path.Combine(Juur, "Images");
    public static readonly string Arved = Path.Combine(Juur, "Arved");
    public static readonly string AndmebaasiFail = Path.Combine(Juur, "tooded.db");

    // Loob kaustad ja kopeerib algtoodete pildid (projekti kaust Pildid), kui neid veel pole
    public static void Loo()
    {
        Directory.CreateDirectory(Pildid);
        Directory.CreateDirectory(Arved);

        string algPildid = Path.Combine(AppContext.BaseDirectory, "Pildid");
        if (!Directory.Exists(algPildid)) return;

        foreach (string fail in Directory.GetFiles(algPildid))
        {
            string siht = Path.Combine(Pildid, Path.GetFileName(fail));
            if (!File.Exists(siht)) File.Copy(fail, siht);
        }
    }

    // Täistee pildi failinimest (andmebaasis hoitakse ainult failinime)
    // Veebiaadressi puhul: kohalik koopia (laaditakse PildiVahemalu kaudu)
    public static string? PildiTee(string? failinimi)
    {
        if (string.IsNullOrEmpty(failinimi)) return null;
        if (OnVeebiPilt(failinimi)) return Path.Combine(Pildid, VeebiFailinimi(failinimi));
        return Path.Combine(Pildid, failinimi);
    }

    public static bool OnVeebiPilt(string? pilt) =>
        pilt != null && (pilt.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                         pilt.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

    // https://.../MAT_200280_PCE_EE  →  web_MAT_200280_PCE_EE.jpg
    public static string VeebiFailinimi(string url)
    {
        string nimi = url.TrimEnd('/');
        nimi = nimi.Substring(nimi.LastIndexOf('/') + 1);
        foreach (char c in Path.GetInvalidFileNameChars()) nimi = nimi.Replace(c, '_');
        return "web_" + nimi + ".jpg";
    }

    // Laeb pildi koopiana, et fail ei jääks lukku
    public static Image? LaePilt(string? tee)
    {
        if (string.IsNullOrEmpty(tee) || !File.Exists(tee)) return null;
        try
        {
            using var fs = new FileStream(tee, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var ajutine = Image.FromStream(fs);
            return new Bitmap(ajutine);
        }
        catch
        {
            return null; // vigane pildifail – jäta pilt tühjaks
        }
    }
}
