using System.Text.Json.Serialization;

namespace Models;
public class Majstor
{
    [Key]
    public int ID { get; set; }

    [RegularExpression(@"^[\w\.-]+@[a-zA-Z\d\.-]+\.[a-zA-Z]{2,}$")]
    public required string Email { get; set; }

    public required List<String> ListaVestina { get; set; }
    public List<ZahtevZaPosao>? ZahteviPosao { get; set; }
    public required Kalendar Kalendar { get; set; }
    public required string Tip { get; set; }

    [JsonIgnore] // ovo sam dodala, ako se negde drugde javi greska da znam
    [InverseProperty("Majstori")]
    public Majstor? Grupa { get; set; } //Grupa kojoj pripada (MAJSTOR)
    public required int VodjaGrupe { get; set; } // '0' | '1'
    public List<Ugovor>? Ugovori { get; set; }
    public required Korisnik Korisnik { get; set; }

    [JsonIgnore]
    public List<ZahtevZaGrupu>? ZahteviGrupaPoslati { get; set; } //(MAJSTOR I GRUPA)
    [JsonIgnore]
    public List<ZahtevZaGrupu>? ZahteviGrupaPrimljeni { get; set; }

    [JsonIgnore]
    public List<Majstor>? Majstori { get; set; } //(GRUPA MAJSTORA), e sad da li tu odmah pri registraciji treba da dodam clana majstora
                                                 //koji je tu grupu kreirao i onog ko je prihvatio poziv, kako to??

    [JsonIgnore]
    public List<MajstorOglas>? MajstoriOglas { get; set; }



}