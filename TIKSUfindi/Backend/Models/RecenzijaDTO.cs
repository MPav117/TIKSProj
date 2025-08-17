using Models;

[NotMapped]
public class RecenzijaDTO
{
    public required string Opis { get; set; }
    [Required]
    [Range(1, 5)]
    public int Ocena { get; set; }
    public List<String>? ListaSlika { get; set; }
    public required int IdUgovor { get; set; }
    public required int IdPrimalac { get; set; }
}