namespace Models;
public class Ugovor
{
    [Key]
    public int ID { get; set; }
    public required string Status { get; set; } // neuspesnoZavrsen | uspesnoZavrsen | potpisaoMajstor | potpisaoPoslodavac | potpisan | nepotpisan (inicijalno) | raskidaPoslodavac | raskidaMajstor
    [InverseProperty("Ugovori")]
    public required Majstor Majstor { get; set; }

    [InverseProperty("Ugovori")]
    public required Poslodavac Poslodavac { get; set; }
    public required ZahtevZaPosao ZahtevZaPosao { get; set; }
    public required string ImeMajstora { get; set; }
    public required string ImePoslodavca { get; set; }

    [Range(0, 9999999999)]
    public required float CenaPoSatu { get; set; }
    public required string Opis { get; set; }
    public required DateTime DatumPocetka { get; set; }
    public required DateTime DatumZavrsetka { get; set; }
    public required string PotpisMajstora { get; set; }
    public required string PotpisPoslodavca { get; set; }

}