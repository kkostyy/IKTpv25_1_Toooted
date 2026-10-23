namespace IKTpv25_1_Toooted;

// Toote partii säilivusajaga (FEFO: enne müüakse varem aeguv)
public class Partii
{
    public int Id { get; set; }
    public int ToodeId { get; set; }
    public Toode Toode { get; set; } = null!;
    public int Kogus { get; set; }
    public DateTime Aegub { get; set; }
    public DateTime Lisatud { get; set; } = DateTime.Now;
    public int MahaKantud { get; set; }              // aegunud ja maha kantud kogus
    public DateTime? MahaKantudAeg { get; set; }
}
