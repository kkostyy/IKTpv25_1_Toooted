namespace IKTpv25_1_Toooted;

public class Kategooria
{
    public int Id { get; set; }
    public string Kategooria_nimetus { get; set; } = "";
    public string? Kirjeldus { get; set; }

    public ICollection<Toode> Tooded { get; set; } = new List<Toode>();
}
