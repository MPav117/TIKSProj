using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace WebTemplate.Controllers;

[ApiController]
[Route("[controller]")]
public class ProfilController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private static List<string> administratori = ["anastasija", "darija", "marko", "sanja"];

    public ZanatstvoContext Context { get; set; }

    public ProfilController(ZanatstvoContext context, IConfiguration configuration, IUserService userService)
    {
        Context = context;
        this._configuration = configuration;
        this._userService = userService;
    }

    //da iscupa username
    /*[HttpGet("username"), Authorize]
    public ActionResult<string> GetMyName()
    {
        return Ok(_userService.GetUser());
    }*/

    //da iscupa ulogu
    /*[HttpGet("role"), Authorize]
    public ActionResult<string> GetMyRole()
    {
        return Ok(_userService.GetRole());
    }*/

    //Pribavljanje korisnika po ID
    /*[HttpGet("user/{id}")]
    public async Task<ActionResult> GetUser(int id)
    {
        try
        {
            var korisnik = await Context.Korisnici.FindAsync(id);
            return Ok(korisnik);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }*/

    [HttpPost("register-poslodavac")]
    public async Task<ActionResult> RegisterPoslodavac([FromBody] PoslodavacDTO poslodavac)
    {
        var usr = await Context.Identiteti.Where(i => i.Username.Equals(poslodavac.Username)).FirstOrDefaultAsync();
        if (usr != null)
        {
            return BadRequest("Username koji ste uneli vec postoji, unesite drugi username!");
        }

        var gradIzBaze = await Context.Gradovi.FindAsync(poslodavac.GradID);
        if (gradIzBaze == null)
        {
            return BadRequest("Uneli ste nevazeci grad!");
        }

        Korisnik? povezaniKorisnik = null;
        if (poslodavac.Povezani != 0)
        {
            povezaniKorisnik = await Context.Korisnici.FindAsync(poslodavac.Povezani);
            if (povezaniKorisnik == null)
            {
                return BadRequest("Ne postoji povezani korisnik");
            }
            if (povezaniKorisnik.Povezani != 0)
            {
                return BadRequest("Korisnik je vec povezan");
            }
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(poslodavac.Password);
        Identitet identitet = new Identitet
        {
            Username = poslodavac.Username,
            PasswordHash = passwordHash,
            Tip = poslodavac.Tip
        };
        await Context.Identiteti.AddAsync(identitet);

        Korisnik korisnik = new Korisnik
        {
            Naziv = poslodavac.Naziv,
            Slika = poslodavac.Slika,
            Grad = gradIzBaze,
            Opis = poslodavac.Opis,
            Identitet = identitet,
            PrimljeneRecenzije = null,
            PoslateRecenzije = null,
            Povezani = poslodavac.Povezani
        };

        await Context.Korisnici.AddAsync(korisnik);
        await Context.SaveChangesAsync();

        if (povezaniKorisnik != null)
        {
            povezaniKorisnik.Povezani = korisnik.ID;
        }

        Poslodavac noviPoslodavac = new Poslodavac
        {
            Adresa = poslodavac.Adresa,
            Email = poslodavac.Email,
            Oglasi = null,
            Ugovori = null,
            Korisnik = korisnik
        };

        await Context.Poslodavci.AddAsync(noviPoslodavac);
        await Context.SaveChangesAsync();

        return Ok(noviPoslodavac);
    }
    [HttpPost("register-grupaMajstora"), Authorize(Roles = "majstor")]
    public async Task<ActionResult> RegisterGrupaMajstor([FromBody] MajstorDTO majstorDTO)
    {
        var usr = await Context.Identiteti.Where(i => i.Username == majstorDTO.Username).FirstOrDefaultAsync();
        if (usr != null)
        {
            return BadRequest("Username koji ste uneli vec postoji, unesite drugi username!");
        }

        var usernameMajstora = _userService.GetUser();
        if (usernameMajstora == null)
        {
            return BadRequest("ne mozes");
        }
        var majstor = await Context.Majstori.FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernameMajstora);
        if (majstor == null)
        {
            return NotFound("Majstor nije pronadjen!");
        }

        var zahtevi = await Context.ZahteviZaGrupu
                                    .Include(z => z.MajstorPosiljalac)
                                    .Include(z => z.MajstorPrimalac)
                                    .Where(z => z.MajstorPosiljalac.ID == majstor.ID && z.Prihvacen == 1)
                                    .ToListAsync();
        if (zahtevi.Count == 0)
            return NotFound("Nisu pronadjeni zahtevi za grupu!");

        var gradIzBaze = await Context.Gradovi.FindAsync(majstorDTO.GradID);
        if (gradIzBaze == null)
        {
            return BadRequest("Uneli ste nevazeci grad!");
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(majstorDTO.Password);
        Identitet identitet = new Identitet
        {
            Username = majstorDTO.Username,
            PasswordHash = passwordHash,
            Tip = majstorDTO.Tip //to ono osnovno da je majstor, a ne poslodavac
        };
        await Context.Identiteti.AddAsync(identitet);
        //await Context.SaveChangesAsync();
        Korisnik korisnik = new Korisnik
        {
            Naziv = majstorDTO.Naziv,
            Slika = majstorDTO.Slika,
            Grad = gradIzBaze,
            Opis = majstorDTO.Opis,
            Identitet = identitet,
            Povezani = 0
        };

        await Context.Korisnici.AddAsync(korisnik);
        //await Context.SaveChangesAsync();

        Kalendar kalendar = new Kalendar
        {
            PocetniDatumi = new List<DateTime>(),
            KrajnjiDatumi = new List<DateTime>(),
            PocetniDatumiUgovora = new List<DateTime>(),
            KrajnjiDatumiUgovora = new List<DateTime>()
        };

        var listaMajstora = new List<Majstor>
        {
            majstor
        };

        /*foreach (var z in zahtevi)
        {
            var primalac = await Context.Majstori
                                   .Where(m => m.ID == z.MajstorPrimalac.ID)
                                   .FirstOrDefaultAsync();

            if (primalac != null)
            {
                listaMajstora.Add(primalac);
            }
        }*/


        Majstor grupa = new Majstor
        {
            Email = majstorDTO.Email,
            ListaVestina = majstorDTO.ListaVestina,
            ZahteviPosao = null,
            Kalendar = kalendar,
            Tip = majstorDTO.TipMajstora, //"grupa" mora 
            Grupa = null,
            VodjaGrupe = 0,
            Ugovori = null,
            Korisnik = korisnik,
            ZahteviGrupaPoslati = null,
            ZahteviGrupaPrimljeni = null,
            Majstori = listaMajstora, //new List<Majstor>(listaMajstora), //da li?
            MajstoriOglas = null
        };
        await Context.Majstori.AddAsync(grupa);
        await Context.SaveChangesAsync();

        foreach (var z in zahtevi)
        {
            var primalac = await Context.Majstori
                                   .Where(m => m.ID == z.MajstorPrimalac.ID)
                                   .FirstOrDefaultAsync();

            if (primalac != null)
            {
                listaMajstora.Add(primalac);
                primalac.Grupa = grupa;
            }
            Context.ZahteviZaGrupu.Remove(z);
        }

        grupa.Majstori = listaMajstora;
        majstor.Grupa = grupa;
        majstor.VodjaGrupe = 1; //POSTAJE VODJA


        await Context.SaveChangesAsync();
        return Ok(grupa);
    }

    [HttpPost("register-majstor")]
    public async Task<ActionResult> RegisterMajstor([FromBody] MajstorDTO majstorDTO)
    {
        var usr = await Context.Identiteti.Where(i => i.Username == majstorDTO.Username).FirstOrDefaultAsync();
        if (usr != null)
        {
            return BadRequest("Username koji ste uneli vec postoji, unesite drugi username!");
        }

        var gradIzBaze = await Context.Gradovi.FindAsync(majstorDTO.GradID);
        if (gradIzBaze == null)
        {
            return BadRequest("Uneli ste nevazeci grad!");
        }

        Korisnik? povezaniKorisnik = null;
        if (majstorDTO.Povezani != 0)
        {
            povezaniKorisnik = await Context.Korisnici.FindAsync(majstorDTO.Povezani);
            if (povezaniKorisnik == null)
            {
                return BadRequest("Ne postoji povezani korisnik");
            }
            if (povezaniKorisnik.Povezani != 0)
            {
                return BadRequest("Korisnik je vec povezan");
            }
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(majstorDTO.Password);
        Identitet identitet = new Identitet
        {
            Username = majstorDTO.Username,
            PasswordHash = passwordHash,
            Tip = majstorDTO.Tip
        };
        await Context.Identiteti.AddAsync(identitet);
        //await Context.SaveChangesAsync();
        Korisnik korisnik = new Korisnik
        {
            Naziv = majstorDTO.Naziv,
            Slika = majstorDTO.Slika,
            Grad = gradIzBaze!,
            Opis = majstorDTO.Opis,
            Identitet = identitet,
            PrimljeneRecenzije = null, //jel se ovo bese podrazumeva da je null ako ne stavim?
            PoslateRecenzije = null,
            ChatPrimljene = null,
            ChatPoslate = null,
            Povezani = majstorDTO.Povezani
        };

        await Context.Korisnici.AddAsync(korisnik);
        await Context.SaveChangesAsync();

        if (povezaniKorisnik != null)
        {
            povezaniKorisnik.Povezani = korisnik.ID;
        }

        Kalendar kalendar = new Kalendar
        {
            PocetniDatumi = new List<DateTime>(),
            KrajnjiDatumi = new List<DateTime>(),
            PocetniDatumiUgovora = new List<DateTime>(),
            KrajnjiDatumiUgovora = new List<DateTime>()
        };

        Majstor majstor = new Majstor
        {
            Email = majstorDTO.Email,
            ListaVestina = majstorDTO.ListaVestina,
            ZahteviPosao = null,
            Kalendar = kalendar,
            Tip = majstorDTO.TipMajstora,
            Grupa = null,
            VodjaGrupe = 0,
            Ugovori = null,
            Korisnik = korisnik,
            ZahteviGrupaPoslati = null,
            ZahteviGrupaPrimljeni = null,
            Majstori = null,
            MajstoriOglas = null
        };

        await Context.Majstori.AddAsync(majstor);
        await Context.SaveChangesAsync();
        return Ok(majstor);
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(IdentitetDTO request)
    {

        //trazim da li imam onog sa takvim username-om vec u bazi 
        var postojeci = await Context.Identiteti.FirstOrDefaultAsync(i => i.Username == request.Username);
        if (postojeci == null)
        {
            return BadRequest("User not found!");
        }
        //ako ima onda Verify
        if (!BCrypt.Net.BCrypt.Verify(request.Password, postojeci.PasswordHash))
        {
            return BadRequest("Wrong password."); //losa praksa, bolje da ne zna sta je pogresno, veca sigurnost
        }
        string jwt = CreateToken(postojeci);
        Response.ContentType = "text/plain";
        //zanemariti
        Response.Cookies.Append("jwt", jwt, new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddHours(1) // Postavi vreme isteka kolačića
        });
        return Ok(jwt);
    }

    private string CreateToken(Identitet korisnik)
    {

        List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.Name, korisnik.Username),
                new Claim(ClaimTypes.Role, korisnik.Tip),
            };

        if (administratori.Contains(korisnik.Username))
        {
            claims.Add(new Claim(ClaimTypes.Role, "admin"));
            Console.WriteLine("admin");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("AppSettings:Token").Value!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    /*[HttpGet("expose")]
    public ActionResult KoJe()
    {
        try
        {
            var jwt = HttpContext.Request.Cookies["jwt"];
            var token = Verify(jwt!); //msm da bi koriscenje cookie-ja uvek ovako funkcionisalo, ja u funkciji probam da pristupim cookie-ju
                                      //uzmem taj token i probam da ga verifikujem, ako valja nastavi dalji tok funkcije, ako ne Unauthorized ili tako nesto??
            string usr = token.Issuer; //ne ide meni to, zato i ne valja 

            var korisnikDTO = Context.Identiteti.FirstOrDefaultAsync(i => i.Username == usr);
            return Ok(korisnikDTO);
        }
        catch (Exception)
        {
            return Unauthorized();
        }
    }*/

    /*private JwtSecurityToken Verify(string jwt)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration.GetSection("AppSettings:Token").Value!);
        tokenHandler.ValidateToken(jwt, new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false
        }, out SecurityToken validatedToken);

        return (JwtSecurityToken)validatedToken;
    }*/

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt"); //izbaci taj token iz cookie-ja

        return Ok(new
        {
            message = "success"
        });
    }


    [HttpGet("podaciRegistracije"), Authorize(Roles = "poslodavac, majstor")]
    public async Task<ActionResult> PodaciRegistracije()
    {
        try
        {
            var username = _userService.GetUser();
            var uloga = _userService.GetRole();
            if (uloga == "majstor")
            {
                var majstor = await Context.Majstori
                                    .Include(m => m.Korisnik)
                                    .ThenInclude(k => k.Grad)
                                    .Include(m => m.Korisnik)
                                    .ThenInclude(k => k.Identitet)
                                    .Where(m => m.Korisnik.Identitet.Username == username)
                                    .Select(m => new
                                    {
                                        jeAdmin = administratori.Contains(username),
                                        m.Korisnik.Identitet.Tip,
                                        m.Korisnik.Identitet.Username, //zbog chata
                                        m.Korisnik.ID,
                                        m.Korisnik.Naziv,
                                        m.Korisnik.Slika,
                                        m.Korisnik.Opis,
                                        gradID = m.Korisnik.Grad.ID,
                                        m.Korisnik.Grad.City_ascii,
                                        m.Korisnik.Grad.Country,
                                        m.Email,
                                        TipMajstora = m.Tip, //majstor|grupa
                                        m.Korisnik.ProsecnaOcena,
                                        m.ListaVestina,
                                        m.Korisnik.Povezani,
                                        grupa = m.Grupa == null ? 0 : 1,
                                        GrupaID = m.Grupa == null ? 0 : m.Grupa.Korisnik.ID
                                    }).FirstOrDefaultAsync();
                if (majstor != null)
                    return Ok(majstor);
                else
                    return NotFound("Majstor nije pronadjen!");
            }
            else if (uloga == "poslodavac")
            {
                var poslodavac = await Context.Poslodavci
                                   .Include(p => p.Korisnik)
                                       .ThenInclude(k => k.Grad)
                                   .Include(p => p.Korisnik)
                                       .ThenInclude(k => k.Identitet)
                                   .Where(p => p.Korisnik.Identitet.Username == username)
                                   .Select(p => new
                                   {
                                       jeAdmin = administratori.Contains(username),
                                       p.Korisnik.Identitet.Tip,
                                       p.Korisnik.Identitet.Username, //zbog chata
                                       p.Korisnik.ID,
                                       p.Korisnik.Naziv,
                                       p.Korisnik.Slika,
                                       p.Korisnik.Opis,
                                       gradID = p.Korisnik.Grad.ID,
                                       p.Korisnik.Grad.City_ascii,
                                       p.Korisnik.Grad.Country,
                                       p.Adresa,
                                       p.Email,
                                       p.Korisnik.ProsecnaOcena,
                                       p.Korisnik.Povezani
                                   }).FirstOrDefaultAsync();
                if (poslodavac != null)
                    return Ok(poslodavac);
                else
                    return NotFound("Poslodavac nije pronadjen!");
            }
            else
            {
                return BadRequest("Nemate prava pristupa ovim podacima!");
            }
        }
        catch (Exception)
        {
            return Unauthorized();
        }
    }

    [HttpGet("GetGradovi")]
    public async Task<ActionResult> GetGradovi(string start)
    {
        // || p.City.Equals(start, StringComparison.CurrentCultureIgnoreCase)
        var gradovi = await Context.Gradovi
        .Where(p => p.City_ascii.ToLower().StartsWith(start))
        .Select(p =>
            new
            {
                p.ID,
                p.City_ascii,
                p.Country
            }).ToListAsync();

        return Ok(gradovi);
    }

    [HttpGet("vratiKorisnika/{idKorisnika}")]
    public async Task<ActionResult> VratiKorisnika(int idKorisnika)
    {
        try
        {
            var korisnik = await Context.Korisnici
                                .Include(k => k.Identitet)
                                .Where(k => k.ID == idKorisnika)
                                .FirstOrDefaultAsync();
            if (korisnik == null)
            {
                return NotFound("Korisnik sa tim ID-jem nije pronadjen u bazi!");
            }
            if (korisnik.Identitet.Tip == "majstor")
            {
                var majstor = await Context.Majstori
                                        .Include(m => m.Korisnik)
                                        .ThenInclude(k => k.Identitet)
                                        .Include(m => m.Korisnik)
                                        .ThenInclude(k => k.Grad)
                                        .Where(m => m.Korisnik.ID == idKorisnika)
                                        .Select(m => new
                                        {
                                            Tip = m.Korisnik.Identitet.Tip,
                                            Naziv = m.Korisnik.Naziv,
                                            Slika = m.Korisnik.Slika,
                                            Opis = m.Korisnik.Opis,
                                            Grad = m.Korisnik.Grad,
                                            m.Korisnik.ProsecnaOcena,
                                            Email = m.Email,
                                            TipMajstora = m.Tip,
                                            ListaVestina = m.ListaVestina,
                                            m.Korisnik.Povezani,
                                            GrupaID = m.Grupa == null ? 0 : m.Grupa.Korisnik.ID
                                        }).FirstOrDefaultAsync();

                if (majstor == null)
                    return NotFound("Majstor koji odgovara korisniku nije pronadjen");

                return Ok(majstor);
            }
            else if (korisnik.Identitet.Tip == "poslodavac")
            {
                var poslodavac = await Context.Poslodavci
                                        .Include(p => p.Korisnik)
                                        .ThenInclude(k => k.Identitet)
                                        .Include(p => p.Korisnik)
                                        .ThenInclude(k => k.Grad)
                                        .Where(p => p.Korisnik.ID == idKorisnika)
                                        .Select(p => new
                                        {
                                            Tip = p.Korisnik.Identitet.Tip,
                                            Naziv = p.Korisnik.Naziv,
                                            Slika = p.Korisnik.Slika,
                                            Opis = p.Korisnik.Opis,
                                            Grad = p.Korisnik.Grad,
                                            p.Korisnik.ProsecnaOcena,
                                            Email = p.Email,
                                            Adresa = p.Adresa,
                                            p.Korisnik.Povezani
                                        }).FirstOrDefaultAsync();

                if (poslodavac == null)
                    return NotFound("Poslodavac koji odgovara korisniku nije pronadjen");

                return Ok(poslodavac);
            }

            return BadRequest("Los tip, al to ne moze da se desi");
        }
        catch (Exception)
        {
            return Unauthorized();
        }
    }

    /*[Route("AzurirajPoslodavac")]
    [HttpPut, Authorize(Roles = "poslodavac")]
    public async Task<ActionResult> AzurirajPoslodavac(string adresa, string email, string naziv, string slika, string opis, int idGrad)
    {

        try
        {
            var usernamePoslodavca = _userService.GetUser();
            var poslodavac = await Context.Poslodavci
            .Include(p => p.Korisnik)
            .ThenInclude(k => k.Grad)
            .Include(l => l.Korisnik)
            .ThenInclude(v => v.Identitet)
            .FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernamePoslodavca);

            if (poslodavac == null)
            {
                return BadRequest("Nije pronadjen poslodavac");
            }
            if (adresa != "null")
            {
                poslodavac.Adresa = adresa;
            }
            if (email != "null")
            {
                poslodavac.Email = email;
            }
            if (naziv != "null")
            {
                poslodavac.Korisnik.Naziv = naziv;
            }
            if (slika != "null")
            {
                poslodavac.Korisnik.Slika = slika;
            }
            if (opis != "null")
            {
                poslodavac.Korisnik.Opis = opis;
            }

            if (idGrad != -1)
            {

                var gradIzBaze = await Context.Gradovi.FindAsync(idGrad);

                if (gradIzBaze == null)
                {
                    return BadRequest("Nije pronadjen grad");
                }

                poslodavac.Korisnik.Grad = gradIzBaze;
            }

            await Context.SaveChangesAsync();
            return Ok("Poslodavac je uspesno izmenjen");

        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

    }*/

    /*[Route("AzurirajMajstor")]
    [HttpPut, Authorize(Roles = "majstor")]
    public async Task<ActionResult> AzurirajMajstor(string naziv, string slika, string opis, int idGrad, string email, List<string>? vestine)
    {

        try
        {
            var usernameMajstor = _userService.GetUser();
            var majstor = await Context.Majstori
            .Include(k => k.Kalendar)
            .Include(p => p.Korisnik)
            .ThenInclude(g => g.Grad)
            .Include(l => l.Korisnik)
            .ThenInclude(v => v.Identitet)
            .FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernameMajstor);

            if (majstor == null)
            {
                return BadRequest("Nije pronadjen majstor");
            }

            if (naziv != "null")
            {
                majstor.Korisnik.Naziv = naziv;
            }

            if (slika != "null")
            {
                majstor.Korisnik.Slika = slika;
            }

            if (opis != "null")
            {
                majstor.Korisnik.Opis = opis;
            }

            if (idGrad != -1)
            {

                var gradIzBaze = await Context.Gradovi.FindAsync(idGrad);

                if (gradIzBaze == null)
                {
                    return BadRequest("Nije pronadjen grad");
                }

                majstor.Korisnik.Grad = gradIzBaze;
            }

            if (email != "null")
            {
                majstor.Email = email;
            }

            if (vestine != null)
            {
                majstor.ListaVestina = vestine;
            }

            await Context.SaveChangesAsync();

            return Ok("Majstor je uspesno izmenjen");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

    }*/

    [HttpDelete("izbrisiProfil"), Authorize(Roles = "poslodavac, majstor")]
    public async Task<ActionResult> IzbrisiProfil()
    {
        try
        {

            var username = _userService.GetUser();
            var uloga = _userService.GetRole();

            var korisnik = await Context.Korisnici
                                    .Include(k => k.PoslateRecenzije)
                                    .Include(k => k.PrimljeneRecenzije)
                                    .Include(k => k.ChatPoslate)
                                    .Include(k => k.ChatPrimljene)
                                    .Include(k => k.Identitet)
                                    .FirstOrDefaultAsync(k => k.Identitet.Username == username);
            if (korisnik == null)
            {
                return NotFound("Korisnik nije pronadjen!");
            }

            if (uloga == "majstor")
            {
                var aktivanUgovor = await Context.Ugovori
                    .Include(u => u.Majstor)
                        .ThenInclude(m => m.Korisnik)
                    .Where(u => (u.Majstor.Korisnik.ID == korisnik.ID) && u.DatumZavrsetka > DateTime.Now && u.Status == "potpisan")
                    .FirstOrDefaultAsync();

                if (aktivanUgovor != null)
                {
                    return BadRequest("Ne mozete obrisati profil jer postoji ugovor koji je trenutno aktivan!");
                }
                var majstor = await Context.Majstori
                                .Include(m => m.Majstori)
                                .Include(m => m.Grupa)
                                .Include(m => m.ZahteviGrupaPoslati)
                                .Include(m => m.ZahteviGrupaPrimljeni)
                                .Include(m => m.Ugovori)
                                .Include(m => m.ZahteviPosao)
                                .Include(m => m.MajstoriOglas)
                                .Include(m => m.Kalendar)
                                .Where(m => m.Korisnik.ID == korisnik.ID).FirstOrDefaultAsync();
                if (majstor == null)
                {
                    return NotFound("Majstor nije pronadjen!");
                }

                if (majstor.Ugovori != null)
                {
                    Console.WriteLine($"Brisanje {majstor.Ugovori.Count} ugovora");
                    Context.Ugovori.RemoveRange(majstor.Ugovori);
                    majstor.Ugovori = null;
                }

                if (majstor.ZahteviGrupaPoslati != null)
                {
                    Console.WriteLine($"Brisanje {majstor.ZahteviGrupaPoslati.Count} zahteva");
                    Context.ZahteviZaGrupu.RemoveRange(majstor.ZahteviGrupaPoslati);
                    majstor.ZahteviGrupaPoslati = null;
                }
                if (majstor.ZahteviGrupaPrimljeni != null)
                {
                    Console.WriteLine($"Brisanje {majstor.ZahteviGrupaPrimljeni.Count} zahteva");
                    Context.ZahteviZaGrupu.RemoveRange(majstor.ZahteviGrupaPrimljeni);
                    majstor.ZahteviGrupaPrimljeni = null;
                }

                //svi primljeni zahtevi za posao
                if (majstor.ZahteviPosao != null)
                {
                    Console.WriteLine($"Brisanje {majstor.ZahteviPosao.Count} zahteva za posao");
                    Context.ZahteviZaPosao.RemoveRange(majstor.ZahteviPosao);
                    majstor.ZahteviPosao = null;
                }


                //sve veze na prijavljene oglase
                if (majstor.MajstoriOglas != null)
                {
                    Console.WriteLine($"Brisanje {majstor.MajstoriOglas.Count} veza na prijavljene oglase");
                    Context.MajstoriOglasi.RemoveRange(majstor.MajstoriOglas);
                    majstor.MajstoriOglas = null;
                }
                //-------------------------kalendar-------------------------------------------------------------------------
                var kalendar = await Context.Kalendari.FirstOrDefaultAsync(k => k.ID == majstor.Kalendar.ID);
                if (kalendar == null)
                {
                    return NotFound("Kalendar nije pronadjen!");
                }
                int id = kalendar.ID;
                Context.Kalendari.Remove(kalendar);
                Console.WriteLine($"Brisanje kalendara: {id}");
                //--------------------------------vezano za grupu----------------------------------------------------------------------------
                //ISPITATI!!!!

                if (majstor.Grupa != null) //clan grupe, ali mozda majstor koji je vodja grupe
                {
                    var grupa = await Context.Majstori
                                        .Include(g => g.Korisnik)
                                        .Include(g => g.Kalendar)
                                        .Include(g => g.Majstori).Where(g => g.ID == majstor.Grupa.ID).FirstOrDefaultAsync();
                    if (grupa != null)
                    {
                        if (grupa.Majstori != null && (grupa.Majstori.ElementAt(0).ID == majstor.ID))
                        {
                            Console.WriteLine($"{grupa.Majstori.ElementAt(0).ID} == {majstor.ID}");
                            foreach (var m in grupa.Majstori)
                            {
                                Console.WriteLine("Skidanje reference clanu grupe");
                                m.Grupa = null; //svakom majstoru pre brisanja grupe skini referencu na grupu
                            }
                            var zahtevi = await Context.ZahteviZaGrupu
                                            .Include(z => z.MajstorPosiljalac)
                                            .Where(z => z.MajstorPosiljalac.ID == grupa.Majstori.ElementAt(0).ID || z.MajstorPosiljalac.ID == grupa.ID).ToListAsync();
                            if (zahtevi != null)
                            {
                                Context.ZahteviZaGrupu.RemoveRange(zahtevi);
                                Console.WriteLine($"{zahtevi.Count} zahteva za posao obrisano.");
                            }

                            if (grupa.Ugovori != null)
                            {
                                Context.Ugovori.RemoveRange(grupa.Ugovori);
                                Console.WriteLine($"Brisanje {grupa.Ugovori.Count} ugovora");
                            }

                            if (grupa.Kalendar != null)
                            {
                                var klndr = await Context.Kalendari.FirstOrDefaultAsync(k => k.ID == grupa.Kalendar.ID);
                                if (klndr == null)
                                {
                                    return NotFound("Kalendar nije pronadjen!");
                                }
                                Context.Kalendari.Remove(klndr);
                            }

                            if (grupa.ZahteviPosao != null)
                            {
                                Context.ZahteviZaPosao.RemoveRange(grupa.ZahteviPosao);
                                Console.WriteLine($"Brisanje {grupa.ZahteviPosao.Count} zahteva za posao");
                            }

                            if (grupa.MajstoriOglas != null)
                            {
                                Console.WriteLine($"Brisanje {grupa.MajstoriOglas.Count} veza na prijavljene oglase");
                                Context.MajstoriOglasi.RemoveRange(grupa.MajstoriOglas);
                                majstor.MajstoriOglas = null;
                            }
                            Console.WriteLine("Brisanje grupe ciji je ovaj majstor vodja!");
                            Context.Majstori.Remove(grupa);

                            var krsnk = await Context.Korisnici.Include(k => k.Identitet).FirstOrDefaultAsync(k => k.ID == grupa.Korisnik.ID);
                            if (krsnk == null)
                                return NotFound("Korisnik koji odgovara grupi nije pronadjen!");

                            if (krsnk.PrimljeneRecenzije != null)
                            {
                                Console.WriteLine($"Brisanje {krsnk.PrimljeneRecenzije.Count} primljenih recenzija");
                                Context.Recenzije.RemoveRange(krsnk.PrimljeneRecenzije);
                            }
                            if (krsnk.PoslateRecenzije != null)
                            {
                                Console.WriteLine($"Brisanje {krsnk.PoslateRecenzije.Count} poslatih recenzija");
                                Context.Recenzije.RemoveRange(krsnk.PoslateRecenzije);
                            }
                            if (krsnk.ChatPoslate != null)
                            {
                                Console.WriteLine($"Brisanje {krsnk.ChatPoslate.Count} chats");
                                Context.ChatMessages.RemoveRange(krsnk.ChatPoslate);
                            }
                            if (krsnk.ChatPrimljene != null)
                            {
                                Console.WriteLine($"Brisanje {krsnk.ChatPrimljene.Count} chats");
                                Context.ChatMessages.RemoveRange(krsnk.ChatPrimljene);
                            }

                            var idnt = krsnk.Identitet;
                            if (krsnk.Identitet == null)
                                return NotFound("Identitet grupe nije pronadjen!");
                            Context.Korisnici.Remove(krsnk);
                            Console.WriteLine($"Brisanje korisnika");

                            Context.Identiteti.Remove(idnt);
                            Console.WriteLine($"Brisanje identiteta");
                            grupa.Majstori.ElementAt(0).VodjaGrupe = 0;
                        }
                        else
                        {
                            grupa.Majstori?.Remove(majstor);
                        }
                    }

                }
                if (majstor.Tip == "grupa")
                {
                    if (majstor.Majstori != null && majstor.Majstori.ElementAt(0).ID == majstor.ID)
                    {
                        foreach (var m in majstor.Majstori)
                        {
                            Console.Write("Skidanje reference clanu grupe");
                            m.Grupa = null;
                        }
                        Console.WriteLine("Sledi brisanje grupe");

                    }
                }
                //-------------------------------------------------------------------------------------------------------------------------------                
                //brisem majstora
                Context.Majstori.Remove(majstor);
                Console.WriteLine($"Brisanje majstora/grupe");

            }
            else if (uloga == "poslodavac")
            {
                var aktivanUgovor = await Context.Ugovori
                                            .Include(u => u.Poslodavac)
                                            .ThenInclude(p => p.Korisnik)
                                            .Where(u => (u.Poslodavac.Korisnik.ID == korisnik.ID) && u.DatumZavrsetka > DateTime.Now && u.Status == "potpisan")
                                            .FirstOrDefaultAsync();
                if (aktivanUgovor != null)
                {
                    return BadRequest("Ne mozete obrisati profil jer postoji ugovor koji je trenutno aktivan!");
                }

                var poslodavac = await Context.Poslodavci
                                                .Include(p => p.Oglasi)
                                                .Include(p => p.Zahtevi)
                                                .Include(p => p.Ugovori)
                                                .Include(p => p.Korisnik)
                                                .Where(p => p.Korisnik.ID == korisnik.ID)
                                                .FirstOrDefaultAsync();
                if (poslodavac == null)
                    return NotFound("Poslodavac nije pronadjen!");

                if (poslodavac.Ugovori != null)
                {
                    Console.WriteLine($"Brisanje {poslodavac.Ugovori.Count} ugovora");
                    Context.Ugovori.RemoveRange(poslodavac.Ugovori);
                    poslodavac.Ugovori = null;
                }
                if (poslodavac.Zahtevi != null)
                {
                    Console.WriteLine($"Brisanje {poslodavac.Zahtevi.Count} zahteva za posao");
                    Context.ZahteviZaPosao.RemoveRange(poslodavac.Zahtevi);
                    poslodavac.Zahtevi = null;
                }

                if (poslodavac.Oglasi != null)
                {
                    foreach (var oglas in poslodavac.Oglasi)
                    {
                        if (oglas.OglasiMajstor != null)
                        {
                            Console.WriteLine($"Brisanje veze ka oglasu iz tabele spoja");
                            Context.MajstoriOglasi.RemoveRange(oglas.OglasiMajstor);
                            oglas.OglasiMajstor = null;
                        }
                    }
                    Console.WriteLine($"Brisanje {poslodavac.Oglasi.Count} oglasa");
                    Context.Oglasi.RemoveRange(poslodavac.Oglasi);
                    poslodavac.Oglasi = null;
                }

                Context.Poslodavci.Remove(poslodavac);
            }


            if (korisnik.PrimljeneRecenzije != null)
            {
                Console.WriteLine($"Brisanje {korisnik.PrimljeneRecenzije.Count} primljenih recenzija");
                Context.Recenzije.RemoveRange(korisnik.PrimljeneRecenzije);
                //korisnik.PrimljeneRecenzije = null;
            }
            if (korisnik.PoslateRecenzije != null)
            {
                Console.WriteLine($"Brisanje {korisnik.PoslateRecenzije.Count} poslatih recenzija");
                Context.Recenzije.RemoveRange(korisnik.PoslateRecenzije);
                //korisnik.PoslateRecenzije = null;
            }
            if (korisnik.ChatPoslate != null)
            {
                Console.WriteLine($"Brisanje {korisnik.ChatPoslate.Count} chats");
                Context.ChatMessages.RemoveRange(korisnik.ChatPoslate);
                //korisnik.ChatPoslate = null;
            }
            if (korisnik.ChatPrimljene != null)
            {
                Console.WriteLine($"Brisanje {korisnik.ChatPrimljene.Count} chats");
                Context.ChatMessages.RemoveRange(korisnik.ChatPrimljene);
                //korisnik.ChatPrimljene = null;
            }
            if (korisnik.Povezani != 0)
            {
                var povezaniKorisnik = await Context.Korisnici.FindAsync(korisnik.Povezani);

                if (povezaniKorisnik != null)
                {
                    povezaniKorisnik.Povezani = 0;
                }
            }

            var identitet = korisnik.Identitet;
            Context.Korisnici.Remove(korisnik);
            Console.WriteLine($"Brisanje korisnika");
            Context.Identiteti.Remove(identitet);
            Console.WriteLine($"Brisanje identiteta");

            // if(administratori.Contains(username))
            // {               
            //     administratori.Remove(username);
            //     Console.WriteLine("administrator brise samog sebe, uklanjam ga iz liste navodno");
            // }

            await Context.SaveChangesAsync();
            return Ok($"Uspešno ste obrisali svoj profil.");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }


    [HttpDelete("izbrisiProfil/{id}"), Authorize(Roles = "admin")]
    public async Task<ActionResult> IzbrisiProfil(int id)
    {
        try
        {
            var usernameAdministratora = _userService.GetUser();

            if (!administratori.Contains(usernameAdministratora))
            {
                return BadRequest("ne mozes");
            }

            var korisnik = await Context.Korisnici
                                    .Include(k => k.PoslateRecenzije)
                                    .Include(k => k.PrimljeneRecenzije)
                                    .Include(k => k.ChatPoslate)
                                    .Include(k => k.ChatPrimljene)
                                    .Include(k => k.Identitet)
                                    .FirstOrDefaultAsync(k => k.ID == id);
            if (korisnik == null)
            {
                return NotFound("Korisnik nije pronadjen!");
            }

            if (usernameAdministratora != korisnik.Identitet.Username && administratori.Contains(korisnik.Identitet.Username))
            {
                return BadRequest("Ne mozete obrisati drugog administratora!");
            }

            if (korisnik.Identitet.Tip == "majstor")
            {
                /*var aktivanUgovor = await Context.Ugovori
                    .Include(u => u.Majstor)
                        .ThenInclude(m => m.Korisnik)
                    .Where(u => (u.Majstor.Korisnik.ID == korisnik.ID) && u.DatumZavrsetka > DateTime.Now
                                && (u.Status.Equals("potpisan") || u.Status.Equals("raskidaMajstor") || u.Status.Equals("raskidaPoslodavac")))
                    .FirstOrDefaultAsync();

                if (aktivanUgovor != null)
                {
                    return BadRequest("Ne mozete obrisati profil jer postoji ugovor koji je trenutno aktivan!");
                }*/
                var majstor = await Context.Majstori
                                .Include(m => m.Majstori)
                                .Include(m => m.Grupa)
                                .Include(m => m.ZahteviGrupaPoslati)
                                .Include(m => m.ZahteviGrupaPrimljeni)
                                .Include(m => m.Ugovori)
                                .Include(m => m.ZahteviPosao)
                                .Include(m => m.MajstoriOglas)
                                .Include(m => m.Kalendar)
                                .Where(m => m.Korisnik.ID == korisnik.ID).FirstOrDefaultAsync();
                if (majstor == null)
                {
                    return NotFound("Majstor nije pronadjen!");
                }

                if (majstor.Ugovori != null)
                {
                    Console.WriteLine($"Brisanje {majstor.Ugovori.Count} ugovora");
                    Context.Ugovori.RemoveRange(majstor.Ugovori);
                    majstor.Ugovori = null;
                }

                if (majstor.ZahteviGrupaPoslati != null)
                {
                    Console.WriteLine($"Brisanje {majstor.ZahteviGrupaPoslati.Count} zahteva");
                    Context.ZahteviZaGrupu.RemoveRange(majstor.ZahteviGrupaPoslati);
                    majstor.ZahteviGrupaPoslati = null;
                }
                if (majstor.ZahteviGrupaPrimljeni != null)
                {
                    Console.WriteLine($"Brisanje {majstor.ZahteviGrupaPrimljeni.Count} zahteva");
                    Context.ZahteviZaGrupu.RemoveRange(majstor.ZahteviGrupaPrimljeni);
                    majstor.ZahteviGrupaPrimljeni = null;
                }

                //svi primljeni zahtevi za posao
                if (majstor.ZahteviPosao != null)
                {
                    Console.WriteLine($"Brisanje {majstor.ZahteviPosao.Count} zahteva za posao");
                    Context.ZahteviZaPosao.RemoveRange(majstor.ZahteviPosao);
                    majstor.ZahteviPosao = null;
                }


                //sve veze na prijavljene oglase
                if (majstor.MajstoriOglas != null)
                {
                    Console.WriteLine($"Brisanje {majstor.MajstoriOglas.Count} veza na prijavljene oglase");
                    Context.MajstoriOglasi.RemoveRange(majstor.MajstoriOglas);
                    majstor.MajstoriOglas = null;
                }
                //-------------------------kalendar-------------------------------------------------------------------------
                var kalendar = await Context.Kalendari.FirstOrDefaultAsync(k => k.ID == majstor.Kalendar.ID);
                if (kalendar == null)
                {
                    return NotFound("Kalendar nije pronadjen!");
                }
                Context.Kalendari.Remove(kalendar);

                if (majstor.Grupa != null) //clan grupe, ali mozda majstor koji je vodja grupe
                {
                    var grupa = await Context.Majstori
                                        .Include(g => g.Korisnik)
                                        .Include(g => g.Kalendar)
                                        .Include(g => g.Majstori).Where(g => g.ID == majstor.Grupa.ID).FirstOrDefaultAsync();
                    if (grupa != null)
                    {
                        if (grupa.Majstori != null && (grupa.Majstori.ElementAt(0).ID == majstor.ID))
                        {
                            Console.WriteLine($"{grupa.Majstori.ElementAt(0).ID} == {majstor.ID}");
                            foreach (var m in grupa.Majstori)
                            {
                                Console.WriteLine("Skidanje reference clanu grupe");
                                m.Grupa = null; //svakom majstoru pre brisanja grupe skini referencu na grupu
                            }
                            var zahtevi = await Context.ZahteviZaGrupu
                                            .Include(z => z.MajstorPosiljalac)
                                            .Where(z => z.MajstorPosiljalac.ID == grupa.Majstori.ElementAt(0).ID || z.MajstorPosiljalac.ID == grupa.ID).ToListAsync();
                            if (zahtevi != null)
                            {
                                Context.ZahteviZaGrupu.RemoveRange(zahtevi);
                                Console.WriteLine($"{zahtevi.Count} zahteva za posao obrisano.");
                            }

                            if (grupa.Ugovori != null)
                            {
                                Context.Ugovori.RemoveRange(grupa.Ugovori);
                                Console.WriteLine($"Brisanje {grupa.Ugovori.Count} ugovora");
                            }

                            if (grupa.Kalendar != null)
                            {
                                var klndr = await Context.Kalendari.FirstOrDefaultAsync(k => k.ID == grupa.Kalendar.ID);
                                if (klndr == null)
                                {
                                    return NotFound("Kalendar nije pronadjen!");
                                }
                                Context.Kalendari.Remove(klndr);
                            }

                            if (grupa.ZahteviPosao != null)
                            {
                                Context.ZahteviZaPosao.RemoveRange(grupa.ZahteviPosao);
                                Console.WriteLine($"Brisanje {grupa.ZahteviPosao.Count} zahteva za posao");
                            }

                            if (grupa.MajstoriOglas != null)
                            {
                                Console.WriteLine($"Brisanje {grupa.MajstoriOglas.Count} veza na prijavljene oglase");
                                Context.MajstoriOglasi.RemoveRange(grupa.MajstoriOglas);
                                majstor.MajstoriOglas = null;
                            }
                            Console.WriteLine("Brisanje grupe ciji je ovaj majstor vodja!");
                            Context.Majstori.Remove(grupa);

                            var krsnk = await Context.Korisnici.Include(k => k.Identitet).FirstOrDefaultAsync(k => k.ID == grupa.Korisnik.ID);
                            if (krsnk == null)
                                return NotFound("Korisnik koji odgovara grupi nije pronadjen!");

                            if (krsnk.PrimljeneRecenzije != null)
                            {
                                Console.WriteLine($"Brisanje {krsnk.PrimljeneRecenzije.Count} primljenih recenzija");
                                Context.Recenzije.RemoveRange(krsnk.PrimljeneRecenzije);
                            }
                            if (krsnk.PoslateRecenzije != null)
                            {
                                Console.WriteLine($"Brisanje {krsnk.PoslateRecenzije.Count} poslatih recenzija");
                                Context.Recenzije.RemoveRange(krsnk.PoslateRecenzije);
                            }
                            if (krsnk.ChatPoslate != null)
                            {
                                Console.WriteLine($"Brisanje {krsnk.ChatPoslate.Count} chats");
                                Context.ChatMessages.RemoveRange(krsnk.ChatPoslate);
                            }
                            if (krsnk.ChatPrimljene != null)
                            {
                                Console.WriteLine($"Brisanje {krsnk.ChatPrimljene.Count} chats");
                                Context.ChatMessages.RemoveRange(krsnk.ChatPrimljene);
                            }

                            var idnt = krsnk.Identitet;
                            if (krsnk.Identitet == null)
                                return NotFound("Identitet grupe nije pronadjen!");
                            Context.Korisnici.Remove(krsnk);
                            Console.WriteLine($"Brisanje korisnika");

                            Context.Identiteti.Remove(idnt);
                            Console.WriteLine($"Brisanje identiteta");
                            grupa.Majstori.ElementAt(0).VodjaGrupe = 0;
                        }
                        else
                        {
                            grupa.Majstori?.Remove(majstor);
                        }
                    }

                }
                if (majstor.Tip == "grupa")
                {
                    if (majstor.Majstori != null && majstor.Majstori.ElementAt(0).ID == majstor.ID)
                    {
                        foreach (var m in majstor.Majstori)
                        {
                            Console.Write("Skidanje reference clanu grupe");
                            m.Grupa = null;
                        }
                        Console.WriteLine("Sledi brisanje grupe");

                    }
                }
                Context.Majstori.Remove(majstor);
                Console.WriteLine($"Brisanje majstora/grupe");

            }
            else if (korisnik.Identitet.Tip == "poslodavac")
            {
                /*var aktivanUgovor = await Context.Ugovori
                                            .Include(u => u.Poslodavac)
                                            .ThenInclude(p => p.Korisnik)
                                            .Where(u => (u.Poslodavac.Korisnik.ID == korisnik.ID) && u.DatumZavrsetka > DateTime.Now)
                                            .FirstOrDefaultAsync();
                if (aktivanUgovor != null)
                {
                    return BadRequest("Ne mozete obrisati profil jer postoji ugovor koji je trenutno aktivan!");
                }*/

                var poslodavac = await Context.Poslodavci
                                                .Include(p => p.Zahtevi)
                                                .Include(p => p.Ugovori)
                                                .Include(p => p.Korisnik)
                                                .Include(p => p.Oglasi)
                                                .Where(p => p.Korisnik.ID == korisnik.ID)
                                                .FirstOrDefaultAsync();
                if (poslodavac == null)
                    return NotFound("Poslodavac nije pronadjen!");

                if (poslodavac.Oglasi != null)
                {
                    var oglasi = await Context.Oglasi.Include(o => o.OglasiMajstor).Where(o => o.Poslodavac.ID == poslodavac.ID).ToListAsync();
                    foreach (var oglas in oglasi)
                    {
                        if (oglas.OglasiMajstor != null)
                        {
                            Console.WriteLine($"Brisanje veze ka oglasu iz tabele spoja");
                            Context.MajstoriOglasi.RemoveRange(oglas.OglasiMajstor);
                            oglas.OglasiMajstor = null;
                        }
                    }
                    Console.WriteLine($"Brisanje {poslodavac.Oglasi.Count} oglasa");
                    Context.Oglasi.RemoveRange(poslodavac.Oglasi);
                    poslodavac.Oglasi = null;
                }

                if (poslodavac.Ugovori != null)
                {
                    Console.WriteLine($"Brisanje {poslodavac.Ugovori.Count} ugovora");
                    Context.Ugovori.RemoveRange(poslodavac.Ugovori);
                    poslodavac.Ugovori = null;
                }
                if (poslodavac.Zahtevi != null)
                {
                    Console.WriteLine($"Brisanje {poslodavac.Zahtevi.Count} zahteva za posao");
                    Context.ZahteviZaPosao.RemoveRange(poslodavac.Zahtevi);
                    poslodavac.Zahtevi = null;
                }

                Context.Poslodavci.Remove(poslodavac);
            }

            if (korisnik.PrimljeneRecenzije != null)
            {
                Console.WriteLine($"Brisanje {korisnik.PrimljeneRecenzije.Count} primljenih recenzija");
                Context.Recenzije.RemoveRange(korisnik.PrimljeneRecenzije);
            }
            if (korisnik.PoslateRecenzije != null)
            {
                Console.WriteLine($"Brisanje {korisnik.PoslateRecenzije.Count} poslatih recenzija");
                Context.Recenzije.RemoveRange(korisnik.PoslateRecenzije);
            }
            if (korisnik.ChatPoslate != null)
            {
                Console.WriteLine($"Brisanje {korisnik.ChatPoslate.Count} chats");
                Context.ChatMessages.RemoveRange(korisnik.ChatPoslate);
            }
            if (korisnik.ChatPrimljene != null)
            {
                Console.WriteLine($"Brisanje {korisnik.ChatPrimljene.Count} chats");
                Context.ChatMessages.RemoveRange(korisnik.ChatPrimljene);
            }
            if (korisnik.Povezani != 0)
            {
                var povezaniKorisnik = await Context.Korisnici.FindAsync(korisnik.Povezani);

                if (povezaniKorisnik != null)
                {
                    povezaniKorisnik.Povezani = 0;
                }
            }

            if (korisnik.Identitet.Username == usernameAdministratora)
            {
                administratori.Remove(usernameAdministratora);
                Console.WriteLine("administrator brise samog sebe, uklanjam ga iz liste navodno");
                Console.WriteLine(administratori);
            }

            var identitet = korisnik.Identitet;
            Context.Korisnici.Remove(korisnik);
            Console.WriteLine($"Brisanje korisnika");
            Context.Identiteti.Remove(identitet);
            Console.WriteLine($"Brisanje identiteta");

            await Context.SaveChangesAsync(); //kad se izbrise majstor1 u objektu oglasa 1 idalje ostaje kao da je prijavljen za njega iako je majstoroglas izbrisan
            return Ok($"Uspešno ste obrisal profil.");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

}
