namespace IKTpv25_1_Toooted
{
    // Peaakna kiirklahvid (F1 näitab kõiki)
    public partial class Form1
    {
        public IEnumerable<Kiirklahv> Kiirklahvid()
        {
            bool omanik() => AktiivneKasutaja.OnOmanik;
            void Vali(string votti, Action a) { moodneMenuu?.Vali(votti); a(); }
            const string R = "Peaaken";
            yield return new Kiirklahv(Keys.F2, R, "Kassa", () => Vali("kassa", () => ValiVaheleht(tabKassa)));
            yield return new Kiirklahv(Keys.F3, R, "Tooted", () => Vali("tooted", () => ValiVaheleht(tabToode)), omanik);
            yield return new Kiirklahv(Keys.F4, R, "Ava e-pood", AvaPood);
            yield return new Kiirklahv(Keys.F5, R, "Kampaaniad", () => Vali("kampaaniad", AvaKampaaniad), omanik);
            yield return new Kiirklahv(Keys.F6, R, "Müügipaneel", () => Vali("myyk", () => AvaVorm(new MyygipaneelVorm())), omanik);
            yield return new Kiirklahv(Keys.F7, R, "Partiid", () => Vali("partiid", AvaPartiid), omanik);
            yield return new Kiirklahv(Keys.F8, R, "Autotellimus", () => Vali("tellimus", () => AvaVorm(new AutotellimusVorm())), omanik);
            yield return new Kiirklahv(Keys.F9, R, "Ostude ajalugu", () => Vali("ajalugu", AvaAjalugu));
            yield return new Kiirklahv(Keys.Control | Keys.F, R, "Otsi toodet", () => { ValiVaheleht(tabToode); txtOtsi.Focus(); txtOtsi.SelectAll(); }, omanik);
            yield return new Kiirklahv(Keys.Control | Keys.L, R, "Ostunimekiri", () => Vali("nimekiri", AvaNimekiri));
            yield return new Kiirklahv(Keys.Control | Keys.K, R, "Kliendid", () => Vali("kliendid", () => AvaVorm(new KliendidVorm())), omanik);
            yield return new Kiirklahv(Keys.Control | Keys.Oemcomma, R, "Seaded", () => SeadedVorm.Ava(this));
            yield return new Kiirklahv(Keys.Control | Keys.Enter, "Kassa", "Maksa (vormista ost)", () => btnMaksta.PerformClick(), () => kaartKorv.Visible && Ostukorv.Ridad.Count > 0);
        }
    }
}
