using Models;

[NotMapped]
public class OglasDTO
{
    public required string Opis { get; set; }
    [Required]
    [Range(0, 9999999999)]
    public float CenaPoSatu { get; set; }
    public List<String>? ListaSlika { get; set; }
    //public DateTime DatumPostavljanja{get; set;}
    public DateTime DatumZavrsetka { get; set; }
    public required List<String> ListaVestina { get; set; }
    public required String Naslov { get; set; }

}