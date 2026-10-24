namespace IKTpv25_1_Toooted;

// Pere ühise ostukorvi rida (kes, mida, mitu, millal) – vt PereKorv
public class PereKorviRida
{
    public int Id { get; set; }
    public string PereKood { get; set; } = "";
    public int ToodeId { get; set; }
    public Toode Toode { get; set; } = null!;
    public int Kogus { get; set; }
    public int KasutajaId { get; set; }
    public string Lisaja { get; set; } = "";       // eesnimi (tšekil/kaardil näitamiseks)
    public DateTime Aeg { get; set; } = DateTime.Now;
}
