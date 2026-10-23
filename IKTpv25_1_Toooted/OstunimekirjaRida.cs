namespace IKTpv25_1_Toooted;

// Ostunimekirja rida: iga kasutaja oma nimekiri, linnuke = ostetud.
// Kui rida on seotud tootega (ToodeId), saab selle korvi lisada ja ost märgib selle ise ostetuks.
public class OstunimekirjaRida
{
    public int Id { get; set; }
    public int KasutajaId { get; set; }
    public string Tekst { get; set; } = "";
    public int Kogus { get; set; } = 1;
    public int? ToodeId { get; set; }
    public bool Ostetud { get; set; }
    public DateTime Lisatud { get; set; } = DateTime.Now;
    public DateTime? OstetudAeg { get; set; }
}
