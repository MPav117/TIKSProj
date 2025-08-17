namespace Models;
public class Korisnik
{
    [Key]
    public int ID { get; set; }

    [MaxLength(30)]
    [MinLength(5)]
    public required string Naziv { get; set; }
    public string? Slika { get; set; }
    public required string Opis { get; set; }
    public required Grad Grad { get; set; }
    public float? ProsecnaOcena { get; set; }
    public required Identitet Identitet { get; set; }
    public List<Recenzija>? PrimljeneRecenzije { get; set; }
    public List<Recenzija>? PoslateRecenzije { get; set; }
    public List<ChatMessage>? ChatPrimljene { get; set; }
    public List<ChatMessage>? ChatPoslate { get; set; }
    public int Povezani { get; set; }
}