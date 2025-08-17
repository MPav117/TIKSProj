using Models;

[NotMapped]
public class PoslodavacDTO
{
    public required string Username { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
    public required string Tip { get; set; }

    [MaxLength(30)]
    [MinLength(5)]
    public required string Naziv { get; set; }
    public string? Slika { get; set; }
    public required string Opis { get; set; }
    public required int GradID { get; set; } //on mi prosledi string a ja mu iz baze dodelim objekat Grad?
    public required string Adresa { get; set; }

    [RegularExpression(@"^[\w\.-]+@[a-zA-Z\d\.-]+\.[a-zA-Z]{2,}$")]
    public required string Email { get; set; }

    public required int Povezani { get; set; }
}