namespace IKTpv25_1_Toooted
{
    // E-poe kiirklahvid (F1 näitab kõiki)
    public partial class PoodVorm
    {
        public IEnumerable<Kiirklahv> Kiirklahvid()
        {
            const string R = "E-pood";
            bool korvis() => Ostukorv.Ridad.Count > 0;
            yield return new Kiirklahv(Keys.Control | Keys.F, R, "Otsi toodet", () => { txtOtsi.Focus(); txtOtsi.SelectAll(); });
            yield return new Kiirklahv(Keys.Control | Keys.H, R, "Avaleht", NaitaAvaleht);
            yield return new Kiirklahv(Keys.Control | Keys.D, R, "Minu lemmikud", () => NaitaRuudustik(Vaade.Lemmikud));
            yield return new Kiirklahv(Keys.Control | Keys.E, R, "Kampaaniad", () => NaitaRuudustik(Vaade.Kampaaniad));
            yield return new Kiirklahv(Keys.Control | Keys.K, R, "Kliendikaart", () => KliendikaartVorm.Ava(this));
            yield return new Kiirklahv(Keys.Control | Keys.P, R, "Preemiad", () => { if (lehed.Praegune is not PreemiadLeht) AvaLeht(new PreemiadLeht()); });
            yield return new Kiirklahv(Keys.Control | Keys.L, R, "Ostunimekiri", AvaNimekiri);
            yield return new Kiirklahv(Keys.Control | Keys.M, R, "Minu kulutused", AvaKulutused);
            yield return new Kiirklahv(Keys.Control | Keys.T, "Ostukorv", "Toiteväärtus", AvaToitevaartus, korvis);
            yield return new Kiirklahv(Keys.Control | Keys.S, "Ostukorv", "Salvesta ostukorv", SalvestaKorv, korvis);
            yield return new Kiirklahv(Keys.Control | Keys.Enter, "Ostukorv", "Maksma", Vormista, korvis);
            yield return new Kiirklahv(Keys.F2, "Ostukorv", "Maksma", Vormista, korvis);
        }
    }
}
