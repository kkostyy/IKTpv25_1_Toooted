namespace IKTpv25_1_Toooted;

// Autotellimus: mida tarnijalt juurde tellida, et kaup jätkuks "kate" päevaks (+ varu päevad).
// Müügitempo = viimase 28 päeva müük / 28 (sama mis müügipaneeli "Lõpevad peagi").
public static class Autotellimus
{
    public const int Ajalugu = 28, Varu = 3;
    public const string MuuTarnija = "Muu tarnija";

    public record Rida(Toode Toode, string Tarnija, decimal PaevasTk, decimal PaeviJatkub, int Soovitus)
    {
        public int Telli { get; set; } = Soovitus;
        public decimal Ostuhind => Toode.Ostuhind ?? Math.Round(Toode.Hind * (1 - Myygianalyys.HinnangulineMarginaal), 2);
        public decimal Summa => Ostuhind * Telli;
    }

    public static string Tarnija(Toode t) =>
        !string.IsNullOrWhiteSpace(t.Tootja) ? t.Tootja!.Trim() : !string.IsNullOrWhiteSpace(t.Kaubamark) ? t.Kaubamark!.Trim() : MuuTarnija;

    public static List<Rida> Arvuta(IEnumerable<Ost> ostud, IEnumerable<Toode> tooted, int kate = 14)
    {
        var myydud = ostud.SelectMany(o => o.Read).Where(r => r.ToodeId != null)
            .GroupBy(r => r.ToodeId!.Value).ToDictionary(g => g.Key, g => g.Sum(r => r.Kogus));
        var tulem = new List<Rida>();
        foreach (var t in tooted)
        {
            if (!myydud.TryGetValue(t.Id, out int tk) || tk <= 0) continue;
            decimal paevas = (decimal)tk / Ajalugu;
            int laos = Math.Max(0, t.Kogus);
            decimal jatkub = Math.Round(laos / paevas, 1);
            if (jatkub >= kate) continue;
            int vaja = (int)Math.Ceiling(paevas * (kate + Varu)) - laos;
            if (vaja > 0) tulem.Add(new Rida(t, Tarnija(t), Math.Round(paevas, 1), jatkub, vaja));
        }
        return tulem.OrderBy(r => r.Tarnija).ThenBy(r => r.PaeviJatkub).ToList();
    }
}
