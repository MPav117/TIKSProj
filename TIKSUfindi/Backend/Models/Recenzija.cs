namespace Models;
public class Recenzija
{
    [Key]
    public int ID { get; set; }
    public required string Opis { get; set; }
    [Required]
    [Range(1, 5)]
    public int Ocena { get; set; }
    public List<String>? ListaSlika { get; set; }
    public required Ugovor Ugovor { get; set; }

    [InverseProperty("PrimljeneRecenzije")]
    public required Korisnik Primalac { get; set; }

    [InverseProperty("PoslateRecenzije")]
    public required Korisnik Davalac { get; set; }
}