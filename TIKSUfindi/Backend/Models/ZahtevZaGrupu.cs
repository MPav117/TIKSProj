namespace Models;
public class ZahtevZaGrupu
{
    [Key]
    public int ID { get; set; }
    public required string Opis { get; set; }
    public required int Prihvacen { get; set; } //moze samo '0' | '1'

    [InverseProperty("ZahteviGrupaPrimljeni")]
    public required Majstor MajstorPrimalac { get; set; }

    [InverseProperty("ZahteviGrupaPoslati")]
    public required Majstor MajstorPosiljalac { get; set; }
}