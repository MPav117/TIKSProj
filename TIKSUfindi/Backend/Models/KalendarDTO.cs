using Models;

[NotMapped]
public class KalendarDTO
{

    public required List<DateTime> PocetniDatumi { get; set; }
    public required List<DateTime> KrajnjiDatumi { get; set; }
}