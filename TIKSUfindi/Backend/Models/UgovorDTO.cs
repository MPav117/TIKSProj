using Models;

[NotMapped]
public class UgovorDTO
{
    public int ID { get; set; }
    //public required string Status { get; set; } // neuspesnoZavrsen | uspesnoZavrsen | potpisaoMajstor | potpisaoPoslodavac | potpisan | nepotpisan (inicijalno) | raskidaPoslodavac | raskidaMajstor
    public required int MajstorID { get; set; }
    public required int PoslodavacID { get; set; }
    public required int ZahtevZaPosaoID { get; set; }
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