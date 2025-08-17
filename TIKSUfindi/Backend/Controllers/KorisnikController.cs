using System;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Models;

namespace WebTemplate.Controllers;

[ApiController]
[Route("[controller]")]
public class KorisnikController : ControllerBase
{
    private readonly IUserService _userService;

    public ZanatstvoContext Context { get; set; }

    public KorisnikController(ZanatstvoContext context, IUserService userService)
    {
        Context = context;
        _userService = userService;
    }

    [HttpGet("getOglasi/{idKorisnika}")]
    public async Task<ActionResult> GetOglasi(int idKorisnika)
    {

        var korisnik = await Context.Korisnici
                            .Include(k => k.Identitet)
                            .Where(k => k.ID == idKorisnika)
                            .FirstOrDefaultAsync();

        if (korisnik != null && korisnik.Identitet != null)
        {
            if (korisnik.Identitet.Tip == "poslodavac")
            {
                //vrati oglase koje je postavio
                var poslodavac = await Context.Poslodavci
                                            .Include(p => p.Oglasi)
                                            .Where(p => p.Korisnik.ID == idKorisnika)
                                            .FirstOrDefaultAsync();
                if (poslodavac != null && poslodavac.Oglasi != null && poslodavac.Oglasi.Count > 0)
                {
                    //sme bese samo ovako??
                    return Ok(poslodavac.Oglasi);
                }
                return Ok("Korisnik sa prosledjenim ID-jem nema nijedan oglas!");
            }
            else if (korisnik.Identitet.Tip == "majstor")
            {
                //vrati oglase na koje je prijavljen
                //var majstor = await Context.Majstori
                //.Include(m => m.MajstoriOglas)
                //.ThenInclude(mo => mo.Oglas)
                //.Where(k => k.Korisnik.ID == idKorisnika)
                //.ToListAsync();
                var veze = Context.MajstoriOglasi
                                    .Include(v => v.Oglas)
                                    .Include(v => v.Majstor)
                                    .ThenInclude(v => v.Korisnik) //jel mi treba
                                    .Where(v => v.Majstor.Korisnik.ID.Equals(idKorisnika));

                var oglasi = await veze.Select(v => v.Oglas).ToListAsync();
                return Ok(oglasi);
            }
        }

        return NotFound("Ne postoji korisnik sa prosledjenim ID-jem");
    }

    [HttpGet("getZahteviPosao"), Authorize(Roles = "poslodavac, majstor")]
    public async Task<ActionResult> GetZahteviPosao() //ovde cu iz tokena da saznam pa recite sta je ispravnije
    {
        var usr = _userService.GetUser();
        var tip = _userService.GetRole();
        if (usr == null || tip == null)
        {
            return BadRequest("Neispravan token?");
        }

        if (tip == "poslodavac")
        {
            //vrati zahteve koje je poslao majstorima .Zahtevi
            var zahteviPoslati = await Context.Poslodavci
                                    .Include(p => p.Korisnik)
                                    .ThenInclude(k => k.Identitet)
                                    .Include(p => p.Zahtevi)
                                    .Where(p => p.Korisnik.Identitet.Username == usr)
                                    .Select(v => v.Zahtevi)
                                    .ToListAsync();

            return Ok(zahteviPoslati);
        }
        else if (tip == "majstor")
        {
            //vrati zahteve koje je primio .ZahteviPosao
            var zahteviPrimljeni = await Context.Majstori
                                    .Include(p => p.Korisnik)
                                    .ThenInclude(k => k.Identitet)
                                    .Include(p => p.ZahteviPosao)
                                    .Where(p => p.Korisnik.Identitet.Username == usr)
                                    .Select(v => v.ZahteviPosao)
                                    .ToListAsync();

            return Ok(zahteviPrimljeni);
        }
        return BadRequest("Doslo je do greske");
    }


    [HttpGet("getUgovori"), Authorize(Roles = "poslodavac, majstor")]
    public async Task<ActionResult> GetUgovori()
    {
        var usr = _userService.GetUser();
        var tip = _userService.GetRole();
        if (usr == null || tip == null)
        {
            return BadRequest("Neispravan token?");
        }

        if (tip == "poslodavac")
        {
            var ugovoriPoslodavac = await Context.Ugovori
                    .Include(u => u.Majstor)
                    .Include(u => u.Poslodavac)
                    .Include(u => u.ZahtevZaPosao)
                    .Where(u => u.Poslodavac.Korisnik.Identitet.Username == usr)
                    .Select(u => new
                    {
                        //PripadaGrupi = false,
                        u.ID,
                        u.ImePoslodavca,
                        u.ImeMajstora,
                        u.CenaPoSatu,
                        u.Opis,
                        u.DatumPocetka,
                        u.DatumZavrsetka,
                        u.Status,
                        MajstorID = u.Majstor.Korisnik.ID,
                        PoslodavacID = u.Poslodavac.Korisnik.ID,
                        ZahtevPosaoID = u.ZahtevZaPosao.ID,
                        imeZaPrikaz = u.Majstor.Korisnik.Naziv,
                        potpisPoslodavca = u.PotpisPoslodavca,  //DODATOOOOOOOOOOOO
                        potpisMajstor = u.PotpisMajstora //
                    })
            .ToListAsync();

            return Ok(ugovoriPoslodavac);
        }
        else if (tip == "majstor")
        {
            var majstor = await Context.Majstori
                                    .Include(m => m.Grupa)
                                    .FirstOrDefaultAsync(m => m.Korisnik.Identitet.Username == usr);
            if (majstor == null)
                return NotFound("Majstor nije pronadjen");

            var ugovoriMajstor = await Context.Ugovori
                                    .Include(u => u.Majstor)
                                    .Include(u => u.Poslodavac)
                                    .Include(u => u.ZahtevZaPosao)
                                    .Where(u => u.Majstor.ID == majstor.ID || (majstor.Grupa != null && u.Majstor.ID == majstor.Grupa.ID))
                                    .Select(u => new
                                    {
                                        PripadaGrupi = majstor.Grupa != null && u.Majstor.ID == majstor.Grupa.ID,
                                        u.ID,
                                        u.ImePoslodavca,
                                        u.ImeMajstora,
                                        u.CenaPoSatu,
                                        u.Opis,
                                        u.DatumPocetka,
                                        u.DatumZavrsetka,
                                        u.Status,
                                        MajstorID = u.Majstor.Korisnik.ID,
                                        PoslodavacID = u.Poslodavac.Korisnik.ID,
                                        ZahtevPosaoID = u.ZahtevZaPosao.ID,
                                        imeZaPrikaz = u.Poslodavac.Korisnik.Naziv,
                                        potpisMajstor = u.PotpisMajstora, //DODATOOOOOOOOOO
                                        potpisPoslodavca = u.PotpisPoslodavca //
                                    }).ToListAsync();


            return Ok(ugovoriMajstor);
        }
        return BadRequest("Doslo je do greske");
    }

    [HttpPost("NapraviRecenziju"), Authorize(Roles = "poslodavac, majstor")]
    public async Task<ActionResult> NapraviRecenziju([FromBody] RecenzijaDTO recenzija)
    {
        var username = _userService.GetUser();
        var korisnik = await Context.Korisnici.Include(p => p.PoslateRecenzije).FirstOrDefaultAsync(p => p.Identitet.Username == username);
        var primalac = await Context.Korisnici.Include(p => p.PrimljeneRecenzije).FirstOrDefaultAsync(p => p.ID == recenzija.IdPrimalac);

        if (korisnik == null)
        {
            return NotFound("Korisnik nije pronađen.");
        }

        if (primalac == null)
        {
            return NotFound("Primalac nije pronađen.");
        }


        var ugovor = await Context.Ugovori.Include(p => p.Majstor).ThenInclude(p => p.Korisnik)
                                        .Include(p => p.Poslodavac).ThenInclude(p => p.Korisnik)
                                .FirstOrDefaultAsync(p => p.ID == recenzija.IdUgovor && (p.Status == "potpisan " || p.Status == "raskidaMajstor" || p.Status == "raskidaPoslodavac"
                                                                                     || p.Status == "neuspesnoZavrsen" || p.Status == "uspesnoZavrsen") &&
                             ((p.Majstor.Korisnik.ID == primalac.ID && p.Poslodavac.Korisnik.ID == korisnik.ID) ||
                              (p.Majstor.Korisnik.ID == korisnik.ID && p.Poslodavac.Korisnik.ID == primalac.ID)));

        if (ugovor == null)
        {
            return NotFound("Nije pronadjen ugovor izmedju vas i zadatog korisnika");
        }

        if (recenzija.Ocena > 5 || recenzija.Ocena < 1)
        {
            return BadRequest("Ocena mora da bude izmedju 1 i 5");
        }

        var postojiRecenzija = await Context.Recenzije
       .FirstOrDefaultAsync(r => r.Davalac.ID == korisnik.ID &&
                                 r.Primalac.ID == primalac.ID &&
                                 r.Ugovor.ID == ugovor.ID);

        if (postojiRecenzija != null)
        {
            korisnik.PoslateRecenzije!.Remove(postojiRecenzija);
            primalac.PrimljeneRecenzije!.Remove(postojiRecenzija);

            postojiRecenzija.Opis = recenzija.Opis;
            postojiRecenzija.Ocena = recenzija.Ocena;
            postojiRecenzija.ListaSlika = recenzija.ListaSlika;
            postojiRecenzija.Ugovor = ugovor;
            postojiRecenzija.Davalac = korisnik;
            postojiRecenzija.Primalac = primalac;


            korisnik.PoslateRecenzije.Add(postojiRecenzija);
            primalac.PrimljeneRecenzije.Add(postojiRecenzija);

            double prosekk = primalac.PrimljeneRecenzije.Average(r => r.Ocena);

            primalac.ProsecnaOcena = (float)prosekk;

            await Context.SaveChangesAsync();
            return Ok("uspesno izmenjena recenzija");

        }

        Recenzija novaRecenzija = new Recenzija
        {
            Opis = recenzija.Opis,
            Ocena = recenzija.Ocena,
            ListaSlika = recenzija.ListaSlika,
            Ugovor = ugovor,
            Davalac = korisnik,
            Primalac = primalac,
        };
        if (korisnik.PoslateRecenzije == null)
        {
            korisnik.PoslateRecenzije = new List<Recenzija>();
        }
        korisnik.PoslateRecenzije.Add(novaRecenzija);


        if (primalac.PrimljeneRecenzije == null)
        {
            primalac.PrimljeneRecenzije = new List<Recenzija>();
        }
        primalac.PrimljeneRecenzije.Add(novaRecenzija);

        await Context.Recenzije.AddAsync(novaRecenzija);

        double prosek = primalac.PrimljeneRecenzije.Average(r => r.Ocena);

        primalac.ProsecnaOcena = (float)prosek;

        await Context.SaveChangesAsync();
        return Ok("Recenzija je napravljena");

    }


    [HttpPut("potpisiUgovor"), Authorize(Roles = "poslodavac, majstor")]
    public async Task<ActionResult> PotpisiUgovor([FromBody] UgovorDTO ugovor)
    {

        var username = _userService.GetUser();
        var uloga = _userService.GetRole();

        var ugovorBaza = await Context.Ugovori
                                    .Include(u => u.Poslodavac)
                                    .Include(u => u.Majstor)
                                    .Include(u => u.ZahtevZaPosao)
                                        .ThenInclude(z => z.Oglas)
                                    .FirstOrDefaultAsync(u => u.ID == ugovor.ID);
        if (ugovorBaza == null)
        {
            return NotFound("Ugovor nije pronađen.");
        }

        if (uloga == "majstor")
        {
            var majstor = await Context.Majstori
                            .Include(m => m.Grupa)
                            .Include(u => u.Majstori)
                            .Include(m => m.Korisnik)
                                .ThenInclude(k => k.Identitet)
                            .Include(m => m.Kalendar)
                            .FirstOrDefaultAsync(m => m.Korisnik.Identitet.Username == username);
            if (majstor == null)
            {
                return NotFound("Majstor nije pronadjen!");
            }
            if (ugovorBaza.Majstor.ID == majstor.ID)
            {
                DateTime noviPocetniDatumUgovora = ugovor.DatumPocetka;
                DateTime noviKrajnjiDatumUgovora = ugovor.DatumZavrsetka;

                if (majstor.Majstori != null) //grupa
                {
                    foreach (var clan in majstor.Majstori)
                    {
                        var clanGrupe = await Context.Majstori.Include(c => c.Kalendar).Where(c => c.ID == clan.ID).FirstOrDefaultAsync();
                        if (clanGrupe == null) //a nije
                        {
                            return BadRequest("Clan grupe nije pronadjen");
                        }
                        for (int i = 0; i < clanGrupe.Kalendar.PocetniDatumi.Count; i++)
                        {

                            DateTime postojeciPocetniDatum = clanGrupe.Kalendar.PocetniDatumi[i];
                            DateTime postojeciKrajnjiDatum = clanGrupe.Kalendar.KrajnjiDatumi[i];

                            if ((noviPocetniDatumUgovora <= postojeciKrajnjiDatum && noviKrajnjiDatumUgovora >= postojeciPocetniDatum) ||
                            (noviPocetniDatumUgovora <= postojeciPocetniDatum && noviKrajnjiDatumUgovora >= postojeciKrajnjiDatum) ||
                            (noviPocetniDatumUgovora >= postojeciPocetniDatum && noviKrajnjiDatumUgovora <= postojeciKrajnjiDatum))
                            {
                                return BadRequest("Dani se preklapaju");
                            }
                        }

                        for (int i = 0; i < clanGrupe.Kalendar.PocetniDatumiUgovora.Count; i++)
                        {

                            DateTime postojeciPocetniDatumUgovora = clanGrupe.Kalendar.PocetniDatumiUgovora[i];
                            DateTime postojeciKrajnjiDatumUgovora = clanGrupe.Kalendar.KrajnjiDatumiUgovora[i];

                            if ((noviPocetniDatumUgovora <= postojeciKrajnjiDatumUgovora && noviKrajnjiDatumUgovora >= postojeciPocetniDatumUgovora) ||
                            (noviPocetniDatumUgovora <= postojeciPocetniDatumUgovora && noviKrajnjiDatumUgovora >= postojeciKrajnjiDatumUgovora) ||
                            (noviPocetniDatumUgovora >= postojeciPocetniDatumUgovora && noviKrajnjiDatumUgovora <= postojeciKrajnjiDatumUgovora))
                            {
                                return BadRequest("Dani se preklapaju");
                            }

                        }
                    }

                }
                for (int i = 0; i < majstor.Kalendar.PocetniDatumi.Count; i++)
                {

                    DateTime postojeciPocetniDatum = majstor.Kalendar.PocetniDatumi[i];
                    DateTime postojeciKrajnjiDatum = majstor.Kalendar.KrajnjiDatumi[i];

                    if ((noviPocetniDatumUgovora <= postojeciKrajnjiDatum && noviKrajnjiDatumUgovora >= postojeciPocetniDatum) ||
                    (noviPocetniDatumUgovora <= postojeciPocetniDatum && noviKrajnjiDatumUgovora >= postojeciKrajnjiDatum) ||
                    (noviPocetniDatumUgovora >= postojeciPocetniDatum && noviKrajnjiDatumUgovora <= postojeciKrajnjiDatum))
                    {
                        return BadRequest("Dani se preklapaju");
                    }

                }

                for (int i = 0; i < majstor.Kalendar.PocetniDatumiUgovora.Count; i++)
                {

                    DateTime postojeciPocetniDatumUgovora = majstor.Kalendar.PocetniDatumiUgovora[i];
                    DateTime postojeciKrajnjiDatumUgovora = majstor.Kalendar.KrajnjiDatumiUgovora[i];

                    if ((noviPocetniDatumUgovora <= postojeciKrajnjiDatumUgovora && noviKrajnjiDatumUgovora >= postojeciPocetniDatumUgovora) ||
                    (noviPocetniDatumUgovora <= postojeciPocetniDatumUgovora && noviKrajnjiDatumUgovora >= postojeciKrajnjiDatumUgovora) ||
                    (noviPocetniDatumUgovora >= postojeciPocetniDatumUgovora && noviKrajnjiDatumUgovora <= postojeciKrajnjiDatumUgovora))
                    {
                        return BadRequest("Dani se preklapaju");
                    }

                }
                //proverio svoje, ali dodatno ako je clan grupe proverava i njene ugovorene liste:
                if (majstor.Grupa != null)
                {
                    var grupa = await Context.Majstori.Include(g => g.Kalendar).Where(g => g.ID == majstor.Grupa.ID).FirstOrDefaultAsync();
                    if (grupa == null)
                    {
                        return BadRequest("Clan je grupe, ali grupa nije pronadjena!");
                    }
                    for (int i = 0; i < grupa.Kalendar.PocetniDatumiUgovora.Count; i++)
                    {

                        DateTime postojeciPocetniDatumUgovora = grupa.Kalendar.PocetniDatumiUgovora[i];
                        DateTime postojeciKrajnjiDatumUgovora = grupa.Kalendar.KrajnjiDatumiUgovora[i];

                        if ((noviPocetniDatumUgovora <= postojeciKrajnjiDatumUgovora && noviKrajnjiDatumUgovora >= postojeciPocetniDatumUgovora) ||
                        (noviPocetniDatumUgovora <= postojeciPocetniDatumUgovora && noviKrajnjiDatumUgovora >= postojeciKrajnjiDatumUgovora) ||
                        (noviPocetniDatumUgovora >= postojeciPocetniDatumUgovora && noviKrajnjiDatumUgovora <= postojeciKrajnjiDatumUgovora))
                        {
                            return BadRequest("Dani se preklapaju");
                        }
                    }
                }

                if (ugovorBaza.Status == "nepotpisan")
                {
                    ugovorBaza.Status = "potpisaoMajstor";
                    ugovorBaza.ImeMajstora = ugovor.ImeMajstora;
                    //valjda majstor ne unosi ime poslodavca, ne potpisuje ga on?
                    ugovorBaza.CenaPoSatu = ugovor.CenaPoSatu;
                    ugovorBaza.Opis = ugovor.Opis;
                    ugovorBaza.DatumPocetka = ugovor.DatumPocetka;
                    ugovorBaza.DatumZavrsetka = ugovor.DatumZavrsetka;
                    ugovorBaza.PotpisMajstora = ugovor.PotpisMajstora; //slika potpisa
                }
                else if (ugovorBaza.Status == "potpisaoPoslodavac")
                {
                    //ako nije uneo iste informacije kao druga strana
                    if (ugovorBaza.CenaPoSatu != ugovor.CenaPoSatu || ugovorBaza.DatumPocetka != ugovor.DatumPocetka || ugovorBaza.DatumZavrsetka != ugovor.DatumZavrsetka || ugovorBaza.Opis != ugovor.Opis)
                    {
                        return BadRequest("Podaci u ugovoru se ne poklapaju sa podacima koje je uneo poslodavac!");
                    }
                    ugovorBaza.ImeMajstora = ugovor.ImeMajstora; //od njega samo potpis jer je sve ostalo vec uneto
                    ugovorBaza.Status = "potpisan"; //obe strane potpisale
                    ugovorBaza.PotpisMajstora = ugovor.PotpisMajstora;

                    if (ugovorBaza.ZahtevZaPosao.Oglas != null)
                    {
                        var oglass = await Context.Oglasi.FirstOrDefaultAsync(o => o.ID == ugovorBaza.ZahtevZaPosao.Oglas.ID);
                        if (oglass != null)
                        {
                            oglass.Prikazan = false;
                        }
                    }

                    //azuriranje kalendara
                    majstor.Kalendar.PocetniDatumiUgovora.Add(noviPocetniDatumUgovora);
                    majstor.Kalendar.KrajnjiDatumiUgovora.Add(noviKrajnjiDatumUgovora);
                }

            }
            else
            {
                return BadRequest("Ugovor ne pripada vama!");
            }
        }
        else if (uloga == "poslodavac")
        {
            var poslodavac = await Context.Poslodavci.Include(p => p.Korisnik).FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == username);
            if (poslodavac == null)
            {
                return NotFound("Poslodavac nije pronadjen");
            }
            if (ugovorBaza.Poslodavac.ID == poslodavac.ID)
            {
                if (ugovorBaza.Status == "nepotpisan")
                {
                    ugovorBaza.Status = "potpisaoPoslodavac";
                    ugovorBaza.ImePoslodavca = ugovor.ImePoslodavca;
                    ugovorBaza.CenaPoSatu = ugovor.CenaPoSatu;
                    ugovorBaza.Opis = ugovor.Opis;
                    ugovorBaza.DatumPocetka = ugovor.DatumPocetka;
                    ugovorBaza.DatumZavrsetka = ugovor.DatumZavrsetka;
                    ugovorBaza.PotpisPoslodavca = ugovor.PotpisPoslodavca;
                }
                else if (ugovorBaza.Status == "potpisaoMajstor")
                {
                    if (ugovorBaza.CenaPoSatu != ugovor.CenaPoSatu || ugovorBaza.DatumPocetka != ugovor.DatumPocetka || ugovorBaza.DatumZavrsetka != ugovor.DatumZavrsetka || ugovorBaza.Opis != ugovor.Opis)
                    {
                        return BadRequest("Podaci u ugovoru se ne poklapaju sa podacima koje je uneo majstor!");
                    }
                    ugovorBaza.ImePoslodavca = ugovor.ImePoslodavca; //od njega samo potpis jer je sve ostalo vec uneto
                    ugovorBaza.Status = "potpisan";
                    ugovorBaza.PotpisPoslodavca = ugovor.PotpisPoslodavca;
                    if (ugovorBaza.ZahtevZaPosao.Oglas != null)
                    {
                        var oglass = await Context.Oglasi.FirstOrDefaultAsync(o => o.ID == ugovorBaza.ZahtevZaPosao.Oglas.ID);
                        if (oglass != null)
                        {
                            oglass.Prikazan = false;
                        }
                    }

                    var majstor = await Context.Majstori.Include(m => m.Kalendar).FirstOrDefaultAsync(m => ugovorBaza.Majstor.ID == m.ID);
                    if (majstor != null)
                    {
                        majstor.Kalendar.PocetniDatumiUgovora.Add(ugovorBaza.DatumPocetka);
                        majstor.Kalendar.KrajnjiDatumiUgovora.Add(ugovorBaza.DatumZavrsetka);
                    }
                }
            }
            else
            {
                return BadRequest("Ugovor ne pripada vama!");
            }
        }
        else
        {
            return BadRequest("Doslo je do greske");
        }
        await Context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("raskiniUgovor"), Authorize(Roles = "poslodavac, majstor")]
    public async Task<ActionResult> RaskiniUgovor(int idUgovora)
    {
        // "Obe strane mogu da iniciraju raskidanje ugovora druga strana potvrdjuje i brise se ugovor iz baze"
        var username = _userService.GetUser();
        var uloga = _userService.GetRole();

        var ugovor = await Context.Ugovori
                                    .Include(u => u.Poslodavac)
                                    .Include(u => u.Majstor)
                                    .Include(u => u.ZahtevZaPosao)
                                        .ThenInclude(z => z.Oglas)
                                    .FirstOrDefaultAsync(u => u.ID == idUgovora);
        if (ugovor == null)
        {
            return NotFound("Ugovor nije pronađen.");
        }

        if (uloga == "majstor")
        {
            var majstor = await Context.Majstori.FirstOrDefaultAsync(m => m.Korisnik.Identitet.Username == username);
            if (majstor == null)
            {
                return NotFound("Majstor nije pronadjen!");
            }
            if (ugovor.Majstor.ID == majstor.ID)
            {
                if (ugovor.Status == "potpisan")
                    ugovor.Status = "raskidaMajstor";
                else if (ugovor.Status == "raskidaPoslodavac")
                {
                    //raskinule obe strane: brisanje iz baze
                    if (ugovor.ZahtevZaPosao.Oglas != null)
                    {
                        var oglass = await Context.Oglasi.FirstOrDefaultAsync(o => o.ID == ugovor.ZahtevZaPosao.Oglas.ID);
                        if (oglass != null)
                        {
                            oglass.Prikazan = true;
                            var veza = await Context.MajstoriOglasi.Where(o => o.OglasID == oglass.ID && o.MajstorId == majstor.ID).FirstOrDefaultAsync(); //dodala da ne bi ostao kao prijavljen 
                            if (veza != null)
                            {
                                Context.MajstoriOglasi.Remove(veza);
                            }
                        }
                    }
                    Context.Ugovori.Remove(ugovor);
                }
                else
                {
                    return BadRequest("Ne mozete raskinuti ugovor!");
                }
            }
            else
            {
                return BadRequest("Ugovor ne pripada vama!");
            }
        }
        else if (uloga == "poslodavac")
        {
            var poslodavac = await Context.Poslodavci.FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == username);
            if (poslodavac == null)
            {
                return NotFound("Poslodavac nije pronadjen");
            }
            if (ugovor.Poslodavac.ID == poslodavac.ID)
            {
                if (ugovor.Status == "potpisan")
                    ugovor.Status = "raskidaPoslodavac";
                else if (ugovor.Status == "raskidaMajstor")
                {
                    //raskinule obe strane: brisanje iz baze
                    if (ugovor.ZahtevZaPosao.Oglas != null)
                    {
                        var oglass = await Context.Oglasi.FirstOrDefaultAsync(o => o.ID == ugovor.ZahtevZaPosao.Oglas.ID);
                        if (oglass != null)
                        {
                            oglass.Prikazan = true;
                            var veza = await Context.MajstoriOglasi.Where(o => o.OglasID == oglass.ID && o.MajstorId == ugovor.Majstor.ID).FirstOrDefaultAsync(); //dodala da ne bi ostao kao prijavljen 
                            if (veza != null)
                            {
                                Context.MajstoriOglasi.Remove(veza);
                            }
                        }
                    }
                    Context.Ugovori.Remove(ugovor);
                }
                else
                {
                    return BadRequest("Ne mozete raskinuti ugovor!");
                }
            }
            else
            {
                return BadRequest("Ugovor ne pripada vama!");
            }
        }
        else
        {
            return BadRequest("Doslo je do greske");
        }
        await Context.SaveChangesAsync();
        return Ok("Uspesno");
    }


    [HttpGet("getZahteviPosaoMajstorGrupa"), Authorize(Roles = "poslodavac, majstor")]
    public async Task<ActionResult> GetZahteviPosaoMajstorGrupa()
    {
        var usr = _userService.GetUser();
        var tip = _userService.GetRole();
        if (usr == null || tip == null)
        {
            return BadRequest("Neispravan token?");
        }

        var korisnik = await Context.Korisnici.
                                        Include(k => k.Identitet)
                                        .FirstOrDefaultAsync(k => k.Identitet.Username == usr);
        if (korisnik == null)
        {
            return NotFound("Korisnik nije pronadjen!");
        }

        if (tip == "poslodavac")
        {
            //vrati zahteve koje je poslao majstorima .Zahtevi
            var zahteviPoslati = await Context.ZahteviZaPosao
                                                    .Include(z => z.Poslodavac)
                                                    .ThenInclude(p => p.Korisnik)
                                                    .Include(z => z.Majstor)
                                                    .Include(z => z.Oglas)
                                                    .Where(z => z.Poslodavac.Korisnik.ID == korisnik.ID)
                                                    .Select(z => new
                                                    {
                                                        z.ID,
                                                        z.Opis,
                                                        z.CenaPoSatu,
                                                        z.ListaSlika,
                                                        z.DatumZavrsetka,
                                                        DrugaStranaID = z.Majstor.Korisnik.ID,
                                                        DrugaStranaNaziv = z.Majstor.Korisnik.Naziv,
                                                        z.Prihvacen,
                                                        z.Oglas // sta vam treba?
                                                    }).ToListAsync();

            return Ok(zahteviPoslati);
        }
        else if (tip == "majstor")
        {
            //vrati zahteve koje je primio .ZahteviPosao
            //da li pripada grupi ili ne? ako pripada ubaci i zahteve grupe
            var majstor = await Context.Majstori
                                .Include(m => m.Grupa)
                                .Include(m => m.Korisnik)
                                .FirstOrDefaultAsync(m => m.Korisnik.ID == korisnik.ID);
            if (majstor == null)
            {
                return NotFound("Majstor nije pronadjen!");
            }
            var zahteviPrimljeni = await Context.ZahteviZaPosao
                                            .Include(z => z.Majstor)
                                            .Include(z => z.Poslodavac)
                                            .Where(z => z.Majstor.ID == majstor.ID || (majstor.Grupa != null && z.Majstor.ID == majstor.Grupa.ID))
                                            .Select(z => new
                                            {
                                                zahtevGrupe = majstor.Grupa != null ? z.Majstor.ID == majstor.Grupa.ID : false,
                                                z.ID,
                                                z.Opis,
                                                z.CenaPoSatu,
                                                z.ListaSlika,
                                                z.DatumZavrsetka,
                                                DrugaStranaID = z.Poslodavac.Korisnik.ID,
                                                DrugaStranaNaziv = z.Poslodavac.Korisnik.Naziv,
                                                z.Prihvacen,
                                                z.Oglas
                                            }).ToListAsync();

            return Ok(zahteviPrimljeni);

        }
        else
        {
            return BadRequest("Doslo je do greske!");
        }
    }


    // [HttpPost("GetChatHistory/{idKorisnik}"), Authorize(Roles = "poslodavac, majstor")] 
    // public async Task<ActionResult> GetChatHistory(int idKorisnik)
    // {

    //     var korisnik2 = await Context.Korisnici.FirstOrDefaultAsync(p => p.ID == idKorisnik);
    //      var username = _userService.GetUser();
    //     var korisnik1 = await Context.Korisnici
    //                                 .Include(p => p.ChatPoslate)
    //                                 .Include(p => p.ChatPrimljene)
    //                                 .FirstOrDefaultAsync(p => p.Identitet.Username == username);

    //     if(korisnik1==null || korisnik2==null)
    //     {
    //        return BadRequest("korisnik nije pronadjen");
    //     }
    //     if(korisnik1!.ChatPoslate==null || korisnik1.ChatPrimljene==null)
    //     {
    //         return BadRequest("korisnik jos nema chat");
    //     }

    //     bool imaChata = korisnik1.ChatPoslate.Any(c => c.KorisnikPrimalac?.ID == korisnik2.ID) ||
    //                 korisnik1.ChatPrimljene.Any(c => c.KorisnikPosiljalac?.ID == korisnik2.ID);

    // if (!imaChata)
    // {
    //     return NotFound("Nema chat istorije između korisnika.");
    // }

    //     var chatHistory = korisnik1.ChatPoslate
    //                     .Where(c => c.KorisnikPrimalac?.ID == korisnik2.ID)
    //                     .Concat(korisnik1.ChatPrimljene.Where(c => c.KorisnikPosiljalac?.ID == korisnik2.ID))
    //                     .Select(c=> new {
    //                          c.Sadrzaj,
    //                          c.VremeSlanja
    //                     }
    //                     )
    //                     .ToList();

    //     return Ok(chatHistory); 

    // }

    [HttpGet("getOglasi"), Authorize(Roles = "poslodavac, majstor")]
    public async Task<ActionResult> GetOglasi()
    {
        var usr = _userService.GetUser();
        var tip = _userService.GetRole();
        if (usr == null || tip == null)
        {
            return BadRequest("Neispravan token?");
        }

        var korisnik = await Context.Korisnici
                            .Include(k => k.Identitet)
                            .Where(k => k.Identitet.Username == usr)
                            .FirstOrDefaultAsync();

        if (korisnik != null)
        {
            if (tip == "poslodavac")
            {
                //vrati oglase koje je postavio
                var poslodavac = await Context.Poslodavci
                                            .Include(p => p.Oglasi)
                                            .Where(p => p.Korisnik.ID == korisnik.ID)
                                            .FirstOrDefaultAsync();

                if (poslodavac == null)
                    return NotFound("Poslodavac nije pronadjen!");
                var oglasi = await Context.Oglasi
                                    .Include(o => o.Poslodavac)
                                    .Where(o => o.Poslodavac.ID == poslodavac.ID)
                                    .Select(o => new
                                    {
                                        PripadaGrupi = false,
                                        o.ID,
                                        o.Naslov,
                                        o.Opis,
                                        o.CenaPoSatu,
                                        o.ListaSlika,
                                        o.DatumPostavljanja,
                                        o.DatumZavrsetka,
                                        PoslKorisnikID = o.Poslodavac.Korisnik.ID,
                                        o.ListaVestina,
                                        prijavljeni = o.OglasiMajstor != null ? o.OglasiMajstor.Select(o =>
                                            o.Majstor.Korisnik.ID
                                        ) : null
                                    }).ToListAsync();
                return Ok(oglasi);
            }
            else if (tip == "majstor")
            {
                var majstor = await Context.Majstori.Include(m => m.Grupa).Include(m => m.MajstoriOglas).Where(m => m.Korisnik.ID == korisnik.ID).FirstOrDefaultAsync();
                if (majstor == null)
                    return NotFound("Majstor nije pronadjen!");

                var oglasi = await Context.MajstoriOglasi
                                    .Include(v => v.Oglas)
                                    .ThenInclude(o => o.Poslodavac)
                                    .Where(v => v.MajstorId == majstor.ID || (majstor.Grupa != null && v.MajstorId == majstor.Grupa.ID))
                                    .Select(v => new
                                    {
                                        PripadaGrupi = majstor.Grupa != null && v.MajstorId == majstor.Grupa.ID,
                                        v.Oglas.ID,
                                        v.Oglas.Naslov,
                                        v.Oglas.Opis,
                                        v.Oglas.CenaPoSatu,
                                        v.Oglas.ListaSlika,
                                        v.Oglas.DatumPostavljanja,
                                        v.Oglas.DatumZavrsetka,
                                        PoslodavacID = v.Oglas.Poslodavac.Korisnik.ID,
                                        v.Oglas.ListaVestina,
                                        prijavljeni = v.Oglas.OglasiMajstor != null ? v.Oglas.OglasiMajstor.Select(o =>
                                            o.Majstor.Korisnik.ID
                                        ) : null
                                    }).ToListAsync();
                return Ok(oglasi);
            }
            else
            {
                return BadRequest("Greska u tipu");
            }
        }
        return NotFound("Korisnik nije pronadjen!");

    }


    [HttpGet("getUgovorTekst")]
    public async Task<string> GetUgovorTekst(string jezik, string stranica)
    {
        try
        {
            using (StreamReader reader = new StreamReader("ugovor.json"))
            {
                string jsonString = await reader.ReadToEndAsync();
                var ugovorTekstovi = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonString);

                if (ugovorTekstovi != null && ugovorTekstovi.ContainsKey(jezik) && ugovorTekstovi[jezik].ContainsKey(stranica))
                {
                    return ugovorTekstovi[jezik][stranica];
                }
                else
                {
                    return "Tekst nije pronađen.";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Greška pri učitavanju fajla: {ex.Message}";
        }
    }
}