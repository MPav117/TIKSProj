using Models;

[NotMapped]
public class ZahtevPosaoDTO
{
    public required string Opis { get; set; }
    [Required]
    public float CenaPoSatu { get; set; }
    public List<String>? ListaSlika { get; set; }
    public DateTime DatumZavrsetka { get; set; }
    public required int KorisnikID { get; set; } //ali za majstora
    public int? OglasID { get; set; }
    //poslodavca cemo iz tokena
}