using Models;

[NotMapped]
public class MajstorDTO
{
    public required string Username { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
    public required string Tip { get; set; } //da li je majstor ili poslodavac, to mi ne treba ili trebaa?
    [MaxLength(30)]
    [MinLength(5)]
    public required string Naziv { get; set; }
    public string? Slika { get; set; }
    public required string Opis { get; set; }
    public required int GradID { get; set; }

    [RegularExpression(@"^[\w\.-]+@[a-zA-Z\d\.-]+\.[a-zA-Z]{2,}$")]
    public required string Email { get; set; }
    public required string TipMajstora { get; set; } //grupa ili obican, ili ovde ako se vec poziva ova funkcija podrazumevano je da je majstor a ne grupa??

    public required List<String> ListaVestina { get; set; }

    public required int Povezani { get; set; }
}