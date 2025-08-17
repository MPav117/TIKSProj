using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Models;

namespace WebTemplate.Controllers;

[ApiController]
[Route("[controller]")]
public class OsnovniController : ControllerBase
{
    private readonly IUserService _userService;

    public ZanatstvoContext Context { get; set; }

    public OsnovniController(ZanatstvoContext context, IUserService userService)
    {
        Context = context;
        _userService = userService;
    }

    [HttpPost("pregledMajstora/{ocenaIliBrRecenzija}/{stranica}")]  //izmenjeno je 
    public async Task<ActionResult> PregledMajstora(float minOcenaf, List<String>? vestinef, int gradIDf, string ocenaIliBrRecenzija, string nazivSearch, int stranica)
    {
        if (stranica < 1)
        {
            return BadRequest("stranica mora da bude broj veci od 0");
        }

        IQueryable<Majstor> majstoriQuery = Context.Majstori
                                .Include(m => m.Korisnik)
                                .ThenInclude(m => m.Grad);

        if (minOcenaf != -1)
        {
            majstoriQuery = majstoriQuery.Where(m => m.Korisnik.ProsecnaOcena != null && m.Korisnik.ProsecnaOcena >= minOcenaf);
        }
        if (vestinef != null)
        {
            majstoriQuery = majstoriQuery.Where(m => m.ListaVestina.Count(v => vestinef.Contains(v)) == vestinef.Count);
        }
        if (gradIDf != -1)
        {
            majstoriQuery = majstoriQuery.Where(m => m.Korisnik.Grad.ID == gradIDf);
        }

        if (nazivSearch != "null")
        {
            majstoriQuery = majstoriQuery.Where(m => m.Korisnik.Naziv.Contains(nazivSearch) ||
                                                     m.Korisnik.Naziv.Equals(nazivSearch) ||
                                                      m.Korisnik.Naziv.ToLower().Equals(nazivSearch.ToLower()));
        }

        if (ocenaIliBrRecenzija == "ocena")
        {
            majstoriQuery = majstoriQuery.OrderByDescending(m => m.Korisnik.ProsecnaOcena);
        }
        else if (ocenaIliBrRecenzija == "brojRecenzija")
        {
            majstoriQuery = majstoriQuery.OrderByDescending(m => m.Korisnik.PrimljeneRecenzije!.Count);
        }
        else
        {
            return BadRequest("Neispravan parametar sortiranja!");
        }

        var majstori = await majstoriQuery.Select(m => new
        {
            m.Korisnik.ID,
            m.Korisnik.Naziv,
            m.Korisnik.Slika,
            m.Korisnik.Grad,
            m.Korisnik.ProsecnaOcena,
            m.ListaVestina
        }).ToListAsync();

        if (stranica > (majstori.Count / 10) + 1)
        {
            return BadRequest("Ne postoji ta stranica!");
        }

        int pocetniIndeks = (stranica - 1) * 10;
        int brojElemenata = 10;
        if (majstori.Count - (stranica - 1) * 10 < 10)
        {
            brojElemenata = majstori.Count - (stranica - 1) * 10;
        }

        var majstoriStranica = majstori.GetRange(pocetniIndeks, brojElemenata);

        var zaPrikaz = new
        {
            lista = majstoriStranica,
            kraj = brojElemenata < 10
        };
        return Ok(zaPrikaz);
    }

    [Route("GetRecenzije/{id}")]
    [HttpGet]
    public async Task<ActionResult> GetRecenzije(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Pogresan id");
        }

        var korisnik = await Context.Korisnici
                                    .Include(p => p.PrimljeneRecenzije)
                                    .Where(p => p.ID == id)
                                    .FirstOrDefaultAsync();

        if (korisnik == null)
        {
            return BadRequest("Ne postoji takav korisnik");
        }

        if (korisnik.PrimljeneRecenzije == null)
        {
            return Ok(Array.Empty<RecenzijaDTO>());
        }

        var korisnik2 = await Context.Korisnici
                                        .Include(p => p.PrimljeneRecenzije!)
                                        .ThenInclude(p => p.Davalac)
                                        .Include(p => p.PrimljeneRecenzije!)
                                        .ThenInclude(p => p.Ugovor)
                                        .Where(p => p.ID == id)
                                        .FirstOrDefaultAsync();

        var recenzijeDTO = korisnik2!.PrimljeneRecenzije!.Select(p => new //RecenzijaDTO
        {
            Opis = p.Opis,
            Ocena = p.Ocena,
            ListaSlika = p.ListaSlika,
            Ugovor = p.Ugovor,
            ImeDavalac = p.Davalac.Naziv,
            SlikaPoslodavca = p.Davalac.Slika
        }).ToList();

        return Ok(recenzijeDTO);
    }

    [Route("PregledOglasa/{sort}/{stranica}")]
    [HttpPost]
    public async Task<ActionResult> PregledOglasa(float minCenaPoSatu, float minOcenaPoslodavca, int idGrad, string sort, string reci, [FromBody] List<String>? vestine, int stranica)
    {
        if (stranica < 1)
        {
            return BadRequest("stranica mora da bude broj veci od 0");
        }

        var oglasi = Context.Oglasi.AsQueryable();

        if (minOcenaPoslodavca != -1)
        {
            oglasi = oglasi.Include(p => p.Poslodavac)
             .ThenInclude(p => p.Korisnik)
             .Where(o => o.Poslodavac.Korisnik.ProsecnaOcena != null && o.Poslodavac.Korisnik.ProsecnaOcena >= minOcenaPoslodavca);

        }

        if (minCenaPoSatu != -1)
        {
            oglasi = oglasi.Where(o => o.CenaPoSatu >= minCenaPoSatu);
        }

        if (idGrad != -1)
        {
            oglasi = oglasi
            .Include(p => p.Poslodavac)
            .ThenInclude(p => p.Korisnik)
            .ThenInclude(p => p.Grad)
            .Where(o => o.Poslodavac.Korisnik.Grad != null && o.Poslodavac.Korisnik.Grad.ID == idGrad);
        }

        if (reci != "null")
        {
            var reciNiz = reci.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            oglasi = oglasi.Where(o => reciNiz.All(rec => o.Naslov.ToLower().Contains(rec.ToLower()) || o.Opis.ToLower().Contains(rec.ToLower())));
        }

        if (vestine != null && vestine.Count > 0)
        {
            oglasi = oglasi.Where(o => o.ListaVestina.Any(v => vestine.Contains(v)));
        }
        oglasi = oglasi.Where(o => o.Prikazan == true);
        if (sort == "Cena po satu")
        {
            oglasi = oglasi
             .OrderByDescending(o => o.CenaPoSatu);
        }
        else if (sort == "Ocena")
        {
            oglasi = oglasi
            .OrderByDescending(o => o.Poslodavac.Korisnik.ProsecnaOcena.HasValue)
            .ThenByDescending(o => o.Poslodavac.Korisnik.ProsecnaOcena);
        }
        else
        {
            return BadRequest("Neispravan parametar sortiranja!");
        }

        var ogl = await oglasi.Select(o => new
        {
            IdOglas = o.ID,
            Slika = o.ListaSlika != null ? o.ListaSlika.ElementAt(0) : null,
            IdPoslodavca = o.Poslodavac.ID,
            o.DatumZavrsetka,
            o.CenaPoSatu,
            o.Opis,
            o.ListaVestina,
            o.Naslov,
            prijavljeni = o.OglasiMajstor != null ? o.OglasiMajstor.Select(o =>
                o.Majstor.Korisnik.ID
            ) : null
        })
            .ToListAsync();

        if (stranica > (ogl.Count / 10) + 1)
        {
            return BadRequest("Ne postoji ta stranica!");
        }

        int pocetniIndeks = (stranica - 1) * 10;
        int brojElemenata = 10;
        if (ogl.Count - (stranica - 1) * 10 < 10)
        {
            brojElemenata = ogl.Count - (stranica - 1) * 10;
        }
        var oglasiStranica = ogl.GetRange(pocetniIndeks, brojElemenata);
        var zaPrikaz = new
        {
            lista = oglasiStranica,
            kraj = brojElemenata < 10
        };

        return Ok(zaPrikaz);

    }

    [Route("GetOglas/{idOglas}")]
    [HttpGet]
    public async Task<ActionResult> GetOglas(int idOglas)
    {
        var oglas = await Context.Oglasi.Select(o => new
        {
            o.ID,
            o.Naslov,
            slike = o.ListaSlika,
            idKorisnik = o.Poslodavac.Korisnik.ID,
            o.Poslodavac.Korisnik.Naziv,
            o.DatumZavrsetka,
            o.CenaPoSatu,
            o.Opis,
            o.ListaVestina,
            prijavljeni = o.OglasiMajstor != null ? o.OglasiMajstor.Select(o =>
                o.Majstor.Korisnik.ID
            ) : null
        }).Where(o => o.ID == idOglas).FirstOrDefaultAsync();
        if (oglas == null)
        {
            return BadRequest("Nije nadjen oglas");
        }
        return Ok(oglas);
    }

    [Route("PromeniJezik/{culture}")]
    [HttpGet]
    public async Task<string> PromeniJezik(string culture)
    {

        try
        {
            if (culture == "en")
            {
                if (System.IO.File.Exists("../../../../TIKSUfindi/Frontend/src/assets/en.json"))
                {
                    using StreamReader reader = new("../../../../TIKSUfindi/Frontend/src/assets/en.json");
                    string jsonString = await reader.ReadToEndAsync();
                    return jsonString;
                }
                else
                {
                    using StreamReader reader = new("../Frontend/src/assets/en.json");
                    string jsonString = await reader.ReadToEndAsync();
                    return jsonString;
                }
            }
            else if (culture == "sr")
            {
                if (System.IO.File.Exists("../../../../TIKSUfindi/Frontend/src/assets/sr.json"))
                {
                    using StreamReader reader2 = new("../../../../TIKSUfindi/Frontend/src/assets/sr.json");
                    string jsonString2 = await reader2.ReadToEndAsync();
                    return jsonString2;
                }
                else
                {
                    using StreamReader reader2 = new("../Frontend/src/assets/sr.json");
                    string jsonString2 = await reader2.ReadToEndAsync();
                    return jsonString2;
                }
            }
            else
            {
                return "mora biti en ili sr";
            }
        }
        catch (Exception ex)
        {
            return $"Greška pri učitavanju fajla: {ex.Message}";
        }
    }
}