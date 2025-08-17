using Models;

[NotMapped]
public class Posl
{

    [MaxLength(30)]
    [MinLength(5)]
    public required string Naziv { get; set; }
    public string? Slika { get; set; }
    public required string Opis { get; set; }
    public required int Grad { get; set; }
    public required string Adresa { get; set; }

    [RegularExpression(@"^[\w\.-]+@[a-zA-Z\d\.-]+\.[a-zA-Z]{2,}$")]
    public required string Email { get; set; }

}