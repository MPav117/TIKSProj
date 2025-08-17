using System.Text.Json.Serialization;

namespace Models;
public class Poslodavac
{
    [Key]
    public int ID { get; set; }
    public required string Adresa { get; set; }

    [RegularExpression(@"^[\w\.-]+@[a-zA-Z\d\.-]+\.[a-zA-Z]{2,}$")]
    public required string Email { get; set; }

    [JsonIgnore]
    public List<Oglas>? Oglasi { get; set; }
    //[JsonIgnore]

    public List<ZahtevZaPosao>? Zahtevi { get; set; }
    //[JsonIgnore]

    public List<Ugovor>? Ugovori { get; set; }
    //[JsonIgnore]

    public required Korisnik Korisnik { get; set; }


}