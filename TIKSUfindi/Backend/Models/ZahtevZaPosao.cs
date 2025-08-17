namespace Models;
public class ZahtevZaPosao
{
    [Key]
    public int ID { get; set; }
    public required string Opis { get; set; }
    public required int Prihvacen { get; set; } //moze samo '0' | '1'
    [Required]
    public float CenaPoSatu { get; set; }
    public List<String>? ListaSlika { get; set; }
    public DateTime DatumZavrsetka { get; set; }

    [InverseProperty("ZahteviPosao")]
    public required Majstor Majstor { get; set; }

    [InverseProperty("Zahtevi")]
    public required Poslodavac Poslodavac { get; set; }
    public Oglas? Oglas { get; set; }

}