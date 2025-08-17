using Models;

public class MajstorOglas
{
    [Key]
    public int ID { get; set; }
    public int MajstorId { get; set; }
    public required Majstor Majstor { get; set; }

    public int OglasID { get; set; }
    public required Oglas Oglas { get; set; }
}