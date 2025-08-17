namespace Models;
public class Kalendar
{
    [Key]
    public int ID { get; set; }

    public required List<DateTime> PocetniDatumi { get; set; }
    public required List<DateTime> KrajnjiDatumi { get; set; }
    public required List<DateTime> PocetniDatumiUgovora { get; set; }
    public required List<DateTime> KrajnjiDatumiUgovora { get; set; }
}