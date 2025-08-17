using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Models;

namespace WebTemplate.Controllers;

[ApiController]
[Route("[controller]")]
public class MajstorController : ControllerBase
{
    private readonly IUserService _userService;

    public ZanatstvoContext Context { get; set; }

    public MajstorController(ZanatstvoContext context, IUserService userService)
    {
        Context = context;
        _userService = userService;
    }

    [HttpPost("prijaviNaOglas"), Authorize(Roles = "majstor")]
    public async Task<ActionResult> PrijaviNaOglas(int id)
    {
        var oglas = await Context.Oglasi.FindAsync(id);
        if (oglas == null)
        {
            return NotFound();
        }

        var usernameMajstora = _userService.GetUser();
        var majstor = await Context.Majstori.FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernameMajstora);

        if (majstor == null)
        {
            return BadRequest("majstor nije pronadjen");
        }

        var vezapostoji = await Context.MajstoriOglasi
        .FirstOrDefaultAsync(mo => mo.MajstorId == majstor.ID && mo.OglasID == id);

        if (vezapostoji != null)
        {
            return BadRequest("majstor je već prijavljen na ovaj oglas");
        }

        var veza = new MajstorOglas
        {
            Majstor = majstor!,
            MajstorId = majstor!.ID,
            OglasID = id,
            Oglas = oglas!
        };
        await Context.MajstoriOglasi.AddAsync(veza);
        await Context.SaveChangesAsync();

        return Ok(veza);

    }

    [Route("OdjaviSaOglasa/{idOglas}")]
    [HttpDelete, Authorize(Roles = "majstor")]
    public async Task<ActionResult> OdjaviSaOglasa(int idOglas)
    {
        if (idOglas <= 0)
        {
            return BadRequest("Pogresan id");
        }

        var usernameMajstora = _userService.GetUser();
        var majstor = await Context.Majstori.FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernameMajstora);

        if (majstor == null)
        {
            return BadRequest("majstor nije pronadjen");
        }

        var veza = Context.MajstoriOglasi.Where(p => p.OglasID == idOglas && p.MajstorId == majstor!.ID).FirstOrDefault();

        if (veza == null)
        {
            return BadRequest("Niste prijavljeni na taj oglas");
        }

        try
        {
            Context.MajstoriOglasi.Remove(veza);
            await Context.SaveChangesAsync();
            return Ok("Uspesna odjava sa oglasa");

        }

        catch (Exception e)
        {

            return BadRequest(e.Message);
        }

    }


    [HttpGet("getKalendar/{korisnikID}")]
    public async Task<ActionResult> GetKalendar(int korisnikID)
    {

        var majstor = await Context.Majstori
                                .Include(m => m.Korisnik)
                                .Include(m => m.Kalendar)
                                .Include(m => m.Grupa)
                                .Include(m => m.Majstori)
                                .Where(m => m.Korisnik.ID == korisnikID)
                                .FirstOrDefaultAsync();

        if (majstor == null)
            return NotFound("Majstor nije pronadjen!");

        if (majstor.Grupa == null)
        {
            if (majstor.Majstori == null) //obican majstor
            {
                Console.WriteLine("obican majstor");
                var kalendar = await Context.Kalendari.Where(k => k.ID == majstor.Kalendar.ID)
                                        .Select(k => new
                                        {
                                            k.ID,
                                            k.KrajnjiDatumi,
                                            k.PocetniDatumi,
                                            listaPocetnihDatumaUgovora = k.PocetniDatumiUgovora,
                                            listaKrajnjihDatumaUgovora = k.KrajnjiDatumiUgovora
                                        }).FirstOrDefaultAsync();

                return Ok(kalendar);

            }
            else //grupa: nadovezi na ugovorene dane i ugovorene i rucno postavljene dane svih clanova 
            {
                Console.WriteLine("grupa");
                var listaPocetnihDatumaUgovora = majstor.Kalendar.PocetniDatumiUgovora;
                var listaKrajnjihDatumaUgovora = majstor.Kalendar.KrajnjiDatumiUgovora;

                foreach (var clan in majstor.Majstori)
                {
                    var clanGrupe = await Context.Majstori.Include(m => m.Kalendar).Where(m => m.ID == clan.ID).FirstOrDefaultAsync();
                    if (clanGrupe == null)
                        return NotFound("Clan grupe nije pronadjen!");

                    listaPocetnihDatumaUgovora.AddRange(clanGrupe.Kalendar.PocetniDatumiUgovora);
                    listaKrajnjihDatumaUgovora.AddRange(clanGrupe.Kalendar.KrajnjiDatumiUgovora);

                    listaPocetnihDatumaUgovora.AddRange(clanGrupe.Kalendar.PocetniDatumi);
                    listaKrajnjihDatumaUgovora.AddRange(clanGrupe.Kalendar.KrajnjiDatumi);
                }
                var kalendar = await Context.Kalendari
                                        .Where(k => k.ID == majstor.Kalendar.ID)
                                        .Select(k => new
                                        {
                                            k.ID,
                                            k.PocetniDatumi,
                                            k.KrajnjiDatumi,
                                            listaPocetnihDatumaUgovora,
                                            listaKrajnjihDatumaUgovora
                                        }).FirstOrDefaultAsync();
                return Ok(kalendar);

            }
        }
        else //clan grupe: na liste ugovorenih dana nadovezi samo ugovorene dane grupe, OCE
        {
            Console.WriteLine("clan grupe");

            var grupa = await Context.Majstori.Include(m => m.Kalendar).Where(m => majstor.Grupa.ID == m.ID).FirstOrDefaultAsync();
            if (grupa == null)
                return NotFound("Grupa nije pronadjena!");

            var listaPocetnihDatumaUgovora = majstor.Kalendar.PocetniDatumiUgovora;
            var listaKrajnjihDatumaUgovora = majstor.Kalendar.KrajnjiDatumiUgovora;

            listaPocetnihDatumaUgovora.AddRange(grupa.Kalendar.PocetniDatumiUgovora);
            listaKrajnjihDatumaUgovora.AddRange(grupa.Kalendar.KrajnjiDatumiUgovora);

            var kalendari = await Context.Kalendari
                                        .Where(k => k.ID == majstor.Kalendar.ID)
                                        .Select(k => new
                                        {
                                            k.ID,
                                            k.KrajnjiDatumi,
                                            k.PocetniDatumi,
                                            listaPocetnihDatumaUgovora,
                                            listaKrajnjihDatumaUgovora
                                        }).FirstOrDefaultAsync();
            return Ok(kalendari);
        }

    }


    [HttpPost("napraviZahtevGrupa/{primalacKorisnikID}"), Authorize(Roles = "majstor")]
    public async Task<ActionResult> NapraviZahtevGrupa([FromBody] string opisZahteva, int primalacKorisnikID)
    {
        var usr = _userService.GetUser(); //iz tokena onog ko taj zahtev salje/trenutno prijavljeni se izvlaci?
        var posiljalac = await Context.Majstori
                                .Include(m => m.Grupa)
                                .Include(m => m.ZahteviGrupaPoslati)
                                .Include(m => m.ZahteviGrupaPrimljeni)
                                .Include(p => p.Korisnik)
                                .ThenInclude(p => p.Identitet)
                                .FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usr);
        if (posiljalac == null)
        {
            return BadRequest("Greska prilikom provere identiteta majstora!");
        }
        if (posiljalac.Grupa != null) //kao da ne vidi da je ovaj vec clan grupe, zato?
        {
            return BadRequest("Mozete biti clan samo jedne grupe!");
        }
        Console.WriteLine(posiljalac.Grupa);
        var primalac = await Context.Majstori
                                .Include(m => m.ZahteviGrupaPoslati)
                                .Include(m => m.ZahteviGrupaPrimljeni)
                                .Include(m => m.Korisnik)
                                .Where(m => m.Korisnik.ID == primalacKorisnikID)
                                .FirstOrDefaultAsync();
        if (primalac == null)
        {
            return BadRequest("Problem sa profilom majstora kome želite poslati zahtev!");
        }

        var noviZahtevGrupa = new ZahtevZaGrupu
        {
            Opis = opisZahteva,
            Prihvacen = 0, //inicijalno 
            MajstorPosiljalac = posiljalac,
            MajstorPrimalac = primalac
        };

        if (posiljalac.ZahteviGrupaPoslati == null)
            posiljalac.ZahteviGrupaPoslati = new List<ZahtevZaGrupu>();

        if (primalac.ZahteviGrupaPrimljeni == null)
            primalac.ZahteviGrupaPrimljeni = new List<ZahtevZaGrupu>();

        posiljalac.ZahteviGrupaPoslati.Add(noviZahtevGrupa);
        primalac.ZahteviGrupaPrimljeni.Add(noviZahtevGrupa);

        await Context.ZahteviZaGrupu.AddAsync(noviZahtevGrupa);
        await Context.SaveChangesAsync();

        //ovo mi je samo za proveru, obrisite ako smeta

        Console.WriteLine("Primljeni zahtevi za primaoca:");
        foreach (var zahtev in primalac.ZahteviGrupaPrimljeni)
        {
            Console.WriteLine($"Opis: {zahtev.Opis}, Posiljalac: {zahtev.MajstorPosiljalac}");
        }

        Console.WriteLine("Poslati zahtevi za pošiljaoca:");
        foreach (var zahtev in posiljalac.ZahteviGrupaPoslati)
        {
            Console.WriteLine($"Opis: {zahtev.Opis}, Primalac: {zahtev.MajstorPrimalac}");
        }

        //return Ok($"Napravljen je i poslat zahtev za grupu sa ID: {noviZahtevGrupa.ID}!"); //poslat u smislu zakacen u listu zahteva majstora/grupe
        return Ok(noviZahtevGrupa.ID);
    }

    [Route("povuciZahtevGrupa/{zahtevID}")]
    [HttpDelete, Authorize(Roles = "majstor")]
    public async Task<ActionResult> PovuciZahtevGrupa(int zahtevID)
    {
        try
        {
            var usr = _userService.GetUser();
            var majstor = await Context.Majstori
                                    .Include(m => m.ZahteviGrupaPoslati)
                                    .FirstOrDefaultAsync(m => m.Korisnik.Identitet.Username == usr);

            if (majstor == null)
            {
                return Unauthorized();
            }

            var zahtev = await Context.ZahteviZaGrupu
                                        .Include(zahtev => zahtev.MajstorPosiljalac)
                                        .Include(zahtev => zahtev.MajstorPrimalac)
                                        .Where(zahtev => zahtev.ID == zahtevID)
                                        .FirstOrDefaultAsync();

            if (zahtev == null || zahtev.MajstorPosiljalac.ID != majstor.ID)
            {
                return BadRequest("Zahtev za grupu nije pronađen ili ne pripada ovom majstoru.");
            }
            else if (zahtev != null)
            {
                if (zahtev.MajstorPrimalac.ZahteviGrupaPrimljeni != null)
                    zahtev.MajstorPrimalac.ZahteviGrupaPrimljeni.Remove(zahtev);
                if (zahtev.MajstorPosiljalac.ZahteviGrupaPoslati != null)
                    zahtev.MajstorPosiljalac.ZahteviGrupaPoslati.Remove(zahtev);

                Context.ZahteviZaGrupu.Remove(zahtev);
                await Context.SaveChangesAsync();

                return Ok($"Zahtev za grupu sa ID: {zahtevID} je izbrisan iz baze podataka.");
            }
            else
            {
                return NotFound($"Nije pronađen zahtev za grupu sa ID: {zahtevID}");
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

    }

    [HttpPut("odgovorZahtevGrupa/{zahtevID}/{odgovor}"), Authorize(Roles = "majstor")]
    public async Task<ActionResult> OdgovorZahtevGrupa(int zahtevID, int odgovor) //odgovor: '0' ili '1'
    {
        var username = _userService.GetUser();
        var majstorPrimalac = await Context.Majstori
                                //.Include(m => m.Korisnik)
                                //.ThenInclude(m => m.Identitet)
                                .Where(m => m.Korisnik.Identitet.Username == username)
                                .FirstOrDefaultAsync();
        if (majstorPrimalac == null)
        {
            return Unauthorized();
        }
        if (majstorPrimalac.Grupa != null)
        {
            return BadRequest("Vec je clan neke druge grupe!");
        }

        var zahtevGrupa = await Context.ZahteviZaGrupu
        //proveri dal ti trebaju ovi Include?
                                        .Include(z => z.MajstorPrimalac)
                                        .Include(z => z.MajstorPosiljalac)
                                        .ThenInclude(m => m.Grupa)
                                        .Where(z => z.ID == zahtevID)
                                        .FirstOrDefaultAsync();
        if (zahtevGrupa == null)
            return NotFound("Nema tog zahteva");
        if (zahtevGrupa.MajstorPrimalac.ID != majstorPrimalac.ID) //i da li stvarno njemu pripada
            return NotFound("Zahtev ne pripada njemu!");

        if (odgovor == 0) //nije prihvacen i samo obrisi taj zahtev
        {
            if (zahtevGrupa.MajstorPrimalac.ZahteviGrupaPrimljeni != null)
                zahtevGrupa.MajstorPrimalac.ZahteviGrupaPrimljeni.Remove(zahtevGrupa);
            if (zahtevGrupa.MajstorPosiljalac.ZahteviGrupaPoslati != null)
                zahtevGrupa.MajstorPosiljalac.ZahteviGrupaPoslati.Remove(zahtevGrupa);

            Context.ZahteviZaGrupu.Remove(zahtevGrupa);
            await Context.SaveChangesAsync();
            return Ok("Uspesno ste odbili zahtev za posao!");

        }
        else if (odgovor == 1)
        {
            zahtevGrupa.Prihvacen = 1;
            Context.ZahteviZaGrupu.Update(zahtevGrupa);
            await Context.SaveChangesAsync();
        }
        else
        {
            return BadRequest("Uneli ste nevalidan podatak za odgovor: unesite ili '0' ili '1'.");
        }

        //zahtev je prihvacen:
        //vi ispitajte da li je zahtevGrupa.MajstorPosiljalac.Tip == "majstor" | "grupa" i ako je majstor -> registerGrupaMajstora ili na osnovu mog komentara
        string tipPosiljaoca = zahtevGrupa.MajstorPosiljalac.Tip;
        if (tipPosiljaoca == "majstor" && zahtevGrupa.MajstorPosiljalac.Grupa == null)
        {
            return Ok("registerGrupaMajstora");
        }
        else if (tipPosiljaoca == "grupa")
        {
            //onda dodaj ovog u listu clanova grupe
            if (zahtevGrupa.MajstorPosiljalac.Majstori == null) //mada ne bi trebalo da je to moguce jer je vec on sam u toj grupi
                zahtevGrupa.MajstorPosiljalac.Majstori = new List<Majstor>();

            zahtevGrupa.MajstorPosiljalac.Majstori.Add(majstorPrimalac);
            majstorPrimalac.Grupa = zahtevGrupa.MajstorPosiljalac; //daj mu referencu na tu grupu
        }
        /*else if (zahtevGrupa.MajstorPosiljalac.Grupa != null) //u medjuvremenu je postao grupa, mislim da moze jer da je ovaj imao grupu ne bi ni mogao da mu posalje zahtev, to sam dodala u napravizahtevgrupa
        {
            Console.WriteLine("u medjuvremenu postao grupa, a zahtev mu je poslao dok je bio majstor!");
            zahtevGrupa.MajstorPosiljalac.Grupa.Majstori?.Add(majstorPrimalac);
            majstorPrimalac.Grupa = zahtevGrupa.MajstorPosiljalac.Grupa;
            // Console.WriteLine(majstorPrimalac.Grupa.Majstori.ToList());
        }
        else
        {
            return Ok("Ni ne moze da bude nekog treceg tipa!!"); //ogranici to negde zapravo...
        }*/
        else
        {
            return BadRequest("Majstor posiljalac je postao clan druge grupe!");
        }

        await Context.SaveChangesAsync();
        return Ok("Uspesno ste odgovorili na zahtev za grupu!");
    }


    [Route("IzbaciIzGrupe/{idMajstor}")]
    [HttpDelete, Authorize(Roles = "majstor")]
    public async Task<ActionResult> IzbaciIzGrupe(int idMajstor)
    {
        if (idMajstor <= 0)
        {
            return BadRequest("Pogrešan id");
        }

        var usernameMajstora = _userService.GetUser();
        var grupa = await Context.Majstori.FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernameMajstora);

        if (grupa == null)
        {
            return BadRequest("Nije pronađena grupa");
        }

        var majstor = await Context.Majstori.Include(m => m.Korisnik).FirstOrDefaultAsync(m => m.Korisnik.ID == idMajstor);
        if (majstor == null)
        {
            return BadRequest("Nije nađen majstor");
        }

        if (majstor.Grupa != grupa)
        {
            return BadRequest("Majstor nije clan grupe");
        }

        if (majstor.VodjaGrupe == 1)
        {
            return BadRequest("vođa ne može biti izbačen iz grupe");
        }

        var clanovi = grupa.Majstori;
        if (clanovi != null && clanovi.Contains(majstor))
        {
            majstor.Grupa = null;
            clanovi.Remove(majstor);
            await Context.SaveChangesAsync();
            return Ok("Uklonjen majstor iz grupe");
        }
        else
        {
            return BadRequest("Nije moguće ukloniti majstora");
        }
    }

    [Route("GetClanovi/{idKorisnika}")]
    [HttpGet]
    public async Task<ActionResult> GetClanovi(int idKorisnika)
    {
        var grupa = await Context.Majstori
                                  .Include(p => p.Majstori)
                                  .Include(p => p.Korisnik)
                                  .FirstOrDefaultAsync(p => p.Korisnik.ID == idKorisnika);

        if (grupa == null)
        {
            return NotFound("Grupa nije pronađena.");
        }

        if (grupa.Majstori == null || !grupa.Majstori.Any())
        {
            return Ok(Array.Empty<string>());
        }

        var clanovi = await Context.Majstori
                                     .Include(m => m.Majstori)
                                     .Include(m => m.Korisnik)
                                     .ThenInclude(m => m.Grad)
                                     .Include(m => m.Korisnik)
                                     .ThenInclude(m => m.Identitet)
                                     .Where(p => p.Korisnik.ID == idKorisnika)
                                     .SelectMany(m => m.Majstori!.Select(subMajstor => new
                                     {
                                         subMajstor.Korisnik.ID,
                                         subMajstor.Korisnik.Naziv,
                                         Slika = subMajstor.Korisnik.Slika != null ? subMajstor.Korisnik.Slika : "Nema slike",
                                         subMajstor.Korisnik.Grad,
                                         ProsecnaOcena = subMajstor.Korisnik.ProsecnaOcena != null ? subMajstor.Korisnik.ProsecnaOcena : 0,
                                         subMajstor.ListaVestina
                                     })).ToListAsync();


        return Ok(clanovi);
    }


    [Route("UpisiKalendar")]
    [HttpPut, Authorize(Roles = "majstor")]
    public async Task<ActionResult> UpisiKalendar([FromBody] KalendarDTO kalendar)
    {
        var noviPocetniDatumi = kalendar.PocetniDatumi;
        var noviKrajnjiDatumi = kalendar.KrajnjiDatumi;

        var usernameMajstora = _userService.GetUser();
        var majstor = await Context.Majstori.Include(m => m.Kalendar)
                                            .FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernameMajstora);

        if (majstor == null)
        {
            return BadRequest("majstor nije pronadjen");
        }

        for (int i = 0; i < noviPocetniDatumi.Count; i++)
        {
            DateTime pocetniDatum1 = noviPocetniDatumi[i];
            DateTime krajnjiDatum1 = noviKrajnjiDatumi[i];

            for (int j = 0; j < noviPocetniDatumi.Count; j++)
            {
                if (i == j) continue;

                DateTime pocetniDatum2 = noviPocetniDatumi[j];
                DateTime krajnjiDatum2 = noviKrajnjiDatumi[j];


                if ((pocetniDatum1 <= krajnjiDatum2 && krajnjiDatum1 >= pocetniDatum2) ||
                    (pocetniDatum1 <= pocetniDatum2 && krajnjiDatum1 >= krajnjiDatum2))
                {
                    return BadRequest("dani se preklapaju");
                }
            }
        }

        majstor.Kalendar.PocetniDatumi = noviPocetniDatumi;
        majstor.Kalendar.KrajnjiDatumi = noviKrajnjiDatumi;
        await Context.SaveChangesAsync();

        return Ok("kalendar upisan u bazu");

    }

    [Route("OdgovorZahtevPosao/{idZahtevPosao}/{odgovor}")]
    [HttpGet, Authorize(Roles = "majstor")]
    public async Task<ActionResult> OdgovorZahtevPosao(int idZahtevPosao, string odgovor)
    {
        if (idZahtevPosao <= 0)
        {
            return BadRequest("Pogrešan id");
        }

        if (odgovor == "ne")
        {
            var zahtevPosao = await Context.ZahteviZaPosao.FindAsync(idZahtevPosao);

            if (zahtevPosao == null)
            {
                return BadRequest("nije nadjen zahtev za posao");
            }

            Context.ZahteviZaPosao.Remove(zahtevPosao);
            await Context.SaveChangesAsync();

            return Ok("Zahtev za posao je odbijen");

        }

        else if (odgovor == "da")
        {
            var zahtevPosao = Context.ZahteviZaPosao.Include(p => p.Majstor).Include(p => p.Poslodavac).FirstOrDefault(p => p.ID == idZahtevPosao);

            if (zahtevPosao == null)
            {
                return BadRequest("nije nađen zahtev za posao");
            }

            Ugovor ugovor = new Ugovor
            {
                Status = "nepotpisan",
                Majstor = zahtevPosao.Majstor,
                Poslodavac = zahtevPosao.Poslodavac,
                ZahtevZaPosao = zahtevPosao,
                ImeMajstora = "null",
                ImePoslodavca = "null",
                CenaPoSatu = 0,
                Opis = "null",
                DatumPocetka = DateTime.Now,
                DatumZavrsetka = DateTime.Now,
                PotpisMajstora = "null",
                PotpisPoslodavca = "null"
            };

            zahtevPosao.Prihvacen = 1;
            Context.ZahteviZaPosao.Update(zahtevPosao);
            await Context.Ugovori.AddAsync(ugovor);
            await Context.SaveChangesAsync();

            //return Ok("Zahtev je uspešno prihvaćen");
            return Ok(ugovor.ID);
        }
        else
        {
            return BadRequest("odgovor mora biti da ili ne");
        }


    }

    [Route("izlazIzGrupe")]
    [HttpDelete, Authorize(Roles = "majstor")]
    public async Task<ActionResult> IzlazIzGrupe()
    {

        var usr = _userService.GetUser();
        var majstor = await Context.Majstori
                                .Include(m => m.Grupa)
                                .Include(m => m.Korisnik)
                                    .ThenInclude(k => k.Identitet)
                                .Include(m => m.ZahteviGrupaPrimljeni)
                                .FirstOrDefaultAsync(m => m.Korisnik.Identitet.Username == usr);

        if (majstor == null || majstor.Grupa == null)
        {
            return NotFound("Majstor ne postoji ili nije pripadnik nijedne grupe!");
        }

        var ugovor = await Context.Ugovori
                                .Include(u => u.Majstor)
                                .Where(u => u.Majstor.ID == majstor.Grupa.ID && u.DatumZavrsetka > DateTime.Now
                                && (u.Status.Equals("potpisan") || u.Status.Equals("raskidaMajstor") || u.Status.Equals("raskidaPoslodavac")))
                                .FirstOrDefaultAsync();
        if (ugovor != null)
        {
            return BadRequest("Ne možete napustiti grupu jer imate aktivne ugovore!");
        }

        var grupa = await Context.Majstori.Include(g => g.Majstori)
                                    .Include(g => g.ZahteviGrupaPoslati)
                                    .Include(g => g.Kalendar)
                                    .Include(g => g.ZahteviPosao)
                                    .Include(g => g.Ugovori)
                                    .Include(g => g.Korisnik)
                                    .Include(g => g.MajstoriOglas)
                                    .Where(g => g.ID == majstor.Grupa.ID)
                                    .FirstOrDefaultAsync();
        if (grupa == null || grupa.Majstori == null)
            return NotFound("Grupa nije pronadjena!");


        var vodjaGrupe = await Context.Majstori.Include(m => m.Grupa).FirstOrDefaultAsync(m => m.Grupa != null && m.Grupa.ID == grupa.ID && m.VodjaGrupe == 1);
        if (vodjaGrupe == null)
            return NotFound("Vodja nije pronadjen!");
        Console.WriteLine($"{vodjaGrupe.ID} == {majstor.ID}");

        if (vodjaGrupe.ID == majstor.ID) //brisanje grupe
        {

            foreach (var clan in grupa.Majstori)
            {
                var clanGrupe = await Context.Majstori.Include(m => m.Grupa).FirstOrDefaultAsync(c => c.ID == clan.ID);
                if (clanGrupe == null)
                    return NotFound("Clan nije pronadjen!");
                clanGrupe.Grupa = null;
            }

            // Obriši sve zahteve za grupu koje je poslao vođa ili sama grupa
            var zahtevi = await Context.ZahteviZaGrupu
                                            .Include(z => z.MajstorPosiljalac)
                                            .Where(z => z.MajstorPosiljalac.ID == vodjaGrupe.ID || z.MajstorPosiljalac.ID == grupa.ID).ToListAsync();
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
                var kalendar = await Context.Kalendari.FirstOrDefaultAsync(k => k.ID == grupa.Kalendar.ID);
                if (kalendar == null)
                {
                    return NotFound("Kalendar nije pronadjen!");
                }
                int id = kalendar.ID;
                Context.Kalendari.Remove(kalendar);
                Console.WriteLine($"Brisanje kalendara sa: {id}");
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

            Context.Majstori.Remove(grupa);

            var korisnik = await Context.Korisnici.Include(k => k.Identitet).FirstOrDefaultAsync(k => k.ID == grupa.Korisnik.ID);
            if (korisnik == null)
                return NotFound("Korisnik koji odgovara grupi nije pronadjen!");

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

            var identitet = korisnik.Identitet;
            if (korisnik.Identitet == null)
                return NotFound("Identitet grupe nije pronadjen!");
            Context.Korisnici.Remove(korisnik);
            Console.WriteLine($"Brisanje korisnika");

            Context.Identiteti.Remove(identitet);
            Console.WriteLine($"Brisanje identiteta");
            vodjaGrupe.VodjaGrupe = 0;
            await Context.SaveChangesAsync();
            return Ok("Grupa vise ne postoji!");
        }
        else
        {
            Console.WriteLine("nije vodja, obican clan izlazi");

            grupa.Majstori.Remove(majstor);
            majstor.Grupa = null;

            var zahtev = Context.ZahteviZaGrupu
                                .Include(z => z.MajstorPrimalac)
                                .Include(z => z.MajstorPosiljalac)
                                .FirstOrDefault(z => z.MajstorPrimalac.ID == majstor.ID && (z.MajstorPosiljalac.ID == vodjaGrupe.ID || z.MajstorPosiljalac.ID == grupa.ID));
            if (zahtev != null)
            {
                Context.ZahteviZaGrupu.Remove(zahtev);
            }

            await Context.SaveChangesAsync();
            return Ok("Uspesan izlaz iz grupe!");
        }

    }

    [Route("GetZahteviGrupa")]
    [HttpGet, Authorize(Roles = "majstor")]
    public async Task<ActionResult> GetZahteviGrupa()
    {

        var usernameMajstora = _userService.GetUser();

        if (usernameMajstora == null)
        {
            return BadRequest("Greska prilikom provere identiteta majstora!");
        }

        var zahteviGrupa = await Context.ZahteviZaGrupu
                                        .Include(z => z.MajstorPosiljalac)
                                        .ThenInclude(mp => mp.Korisnik)
                                        .ThenInclude(k => k.Identitet)
                                        .Include(z => z.MajstorPrimalac)
                                        .ThenInclude(mp => mp.Korisnik)
                                        .ThenInclude(k => k.Identitet)
                                        .Where(z => z.MajstorPosiljalac.Korisnik.Identitet.Username == usernameMajstora ||
                                              z.MajstorPrimalac.Korisnik.Identitet.Username == usernameMajstora)
                                        .ToListAsync();

        var poslati = zahteviGrupa
            .Where(z => z.MajstorPosiljalac.Korisnik.Identitet.Username == usernameMajstora)
            .Select(z => new
            {
                z.ID,
                Opis = z.Opis,
                Tip = "Poslati",
                Korisnik = z.MajstorPrimalac.Korisnik.ID,
                naziv = z.MajstorPrimalac.Korisnik.Naziv,
                z.Prihvacen
            }).ToList();

        var primljeni = zahteviGrupa
            .Where(z => z.MajstorPrimalac.Korisnik.Identitet.Username == usernameMajstora)
            .Select(z => new
            {
                z.ID,
                Opis = z.Opis,
                Tip = "Primljeni",
                Korisnik = z.MajstorPosiljalac.Korisnik.ID,
                naziv = z.MajstorPosiljalac.Korisnik.Naziv,
                z.Prihvacen
            }).ToList();

        var sviZahtevi = new
        {
            PoslatiZahtevi = poslati,
            PrimljeniZahtevi = primljeni
        };

        return Ok(sviZahtevi);
    }

    [Route("azurirajMajstor2")]
    [HttpPut, Authorize(Roles = "majstor")]
    public async Task<ActionResult> AzurirajMajstor2([FromBody] MajstorDTO noviMajstor)
    {
        try
        {
            var usernameMajstora = _userService.GetUser();
            var majstor = await Context.Majstori
                                    .Include(p => p.Korisnik)
                                        .ThenInclude(v => v.Identitet)
                                    .FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernameMajstora);
            if (majstor == null)
            {
                return NotFound("Majstor nije pronadjen!");
            }

            var korisnik = await Context.Korisnici
                                .Include(k => k.Grad)
                                .Where(k => k.ID == majstor.Korisnik.ID)
                                .FirstOrDefaultAsync();
            if (korisnik == null)
            {
                return NotFound("Korisnik nije pronadjen!");
            }

            korisnik.Naziv = noviMajstor.Naziv;
            korisnik.Slika = noviMajstor.Slika;
            korisnik.Opis = noviMajstor.Opis;
            if (korisnik.Grad.ID != noviMajstor.GradID)
            {
                var grad = await Context.Gradovi.FindAsync(noviMajstor.GradID);
                if (grad == null)
                    return NotFound("Grad nije pronadjen!");
                korisnik.Grad = grad;
            }

            majstor.ListaVestina = noviMajstor.ListaVestina;
            //Context.Majstori.Update(majstor);
            //Context.Korisnici.Update(korisnik);              
            await Context.SaveChangesAsync();
            return Ok("Majstor je uspesno izmenjen");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}
