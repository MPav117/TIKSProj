using System.Text.Json.Serialization;

namespace Models;
public class Oglas
{
    [Key]
    public int ID { get; set; }
    public required string Opis { get; set; }
    [Required]
    [Range(0, 9999999999)]
    public float CenaPoSatu { get; set; }
    public List<String>? ListaSlika { get; set; }//=new List<string>();
    public required List<String> ListaVestina { get; set; }
    public required String Naslov { get; set; }
    public DateTime DatumPostavljanja { get; set; }
    public DateTime DatumZavrsetka { get; set; }

    [JsonIgnore]
    public List<MajstorOglas>? OglasiMajstor { get; set; }

    [InverseProperty("Oglasi")]
    //[JsonIgnore]
    public required Poslodavac Poslodavac { get; set; }

    public required bool Prikazan { get; set; }

}