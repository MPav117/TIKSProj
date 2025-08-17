using System;
using Microsoft.AspNetCore.Authorization;
using Models;

namespace WebTemplate.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "poslodavac")]
public class PoslodavacController : ControllerBase
{
    public IUserService _userService;

    public ZanatstvoContext Context { get; set; }

    public PoslodavacController(ZanatstvoContext context, IUserService userService)
    {
        Context = context;
        _userService = userService;
    }

    [HttpPost("postaviOglas"), Authorize(Roles = "poslodavac")]
    public async Task<ActionResult> PostaviOglas([FromBody] OglasDTO ogl)
    {
        var usernamePoslodavca = _userService.GetUser();
        var posl = await Context.Poslodavci.FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernamePoslodavca);

        if (posl == null)
        {
            return BadRequest("Problem pri pronalasku poslodavca");
        }

        Oglas noviOglas = new Oglas
        {
            Opis = ogl.Opis,
            CenaPoSatu = ogl.CenaPoSatu,
            ListaSlika = ogl.ListaSlika,
            DatumPostavljanja = DateTime.Now,
            DatumZavrsetka = ogl.DatumZavrsetka,
            Poslodavac = posl!,
            ListaVestina = ogl.ListaVestina,
            Naslov = ogl.Naslov,
            Prikazan = true

        };
        await Context.Oglasi.AddAsync(noviOglas);
        await Context.SaveChangesAsync();

        return Ok("Oglas je dodat");

    }


    [Route("IzbrisatiOglas/{id}")]
    [HttpDelete, Authorize(Roles = "poslodavac")]
    public async Task<ActionResult> IzbrisiOglas(int id) //KOPIRATI OD SANJE
    {
        if (id <= 0)
        {
            return BadRequest("Pogresan id");
        }

        var usernamePoslodavca = _userService.GetUser();
        var poslodavac = await Context.Poslodavci.FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernamePoslodavca);

        var oglas = await Context.Oglasi
                                .Include(o => o.OglasiMajstor)
                                .Include(o => o.Poslodavac)
                                 .FirstOrDefaultAsync(o => o.ID == id);

        if (poslodavac == null)
        {
            return Unauthorized();
        }

        if (oglas == null || oglas.Poslodavac.ID != poslodavac.ID)
        {
            return BadRequest("Oglas nije pronađen ili ne pripada ovom poslodavcu.");
        }

        try
        {
            //dodala --------------------------------------------------------------------
            var ugovori = await Context.Ugovori
                                        .Include(u => u.ZahtevZaPosao)
                                            .ThenInclude(z => z.Oglas)
                                        .Where(u => u.ZahtevZaPosao.Oglas != null && u.ZahtevZaPosao.Oglas.ID == id)
                                        .ToListAsync();

            var zahteviPosao = await Context.ZahteviZaPosao
                                                .Include(z => z.Oglas)
                                                .Where(z => z.Oglas != null && z.Oglas.ID == id)
                                                .ToListAsync();
            var lista = new List<ZahtevZaPosao>(zahteviPosao);
            foreach (var ugovor in ugovori)
            {
                if (ugovor.Status != "neuspesnoZavrsen")
                {
                    return BadRequest("Ne mozete obrisati oglas jer je preko njega poslat zahtev, a preko zahteva sklopljen ugovor!");
                }
                foreach (var z in zahteviPosao)
                {
                    if (ugovor.ZahtevZaPosao.ID == z.ID)
                    {
                        lista.Remove(z);
                    }

                }

            }

            if (oglas.OglasiMajstor != null)
            {
                Context.MajstoriOglasi.RemoveRange(oglas.OglasiMajstor);
            }

            // if (ugovor != null)
            //     return BadRequest("Ne mozete obrisati oglas jer je preko njega poslat zahtev, a preko zahteva sklopljen ugovor!");


            if (lista != null) //nadam se da ne pravi problem, dodala!!
            {
                Context.ZahteviZaPosao.RemoveRange(lista);
            }
            //--------------------------------------------------------------------------------



            Context.Oglasi.Remove(oglas);
            await Context.SaveChangesAsync();    //i majstor oglas dodaj

            return Ok("Uspesno izbrisan oglas");
        }


        catch (Exception e)
        {

            return BadRequest(e.Message);
        }

    }


    [HttpPost("napraviZahtevPosao"), Authorize(Roles = "poslodavac")]
    public async Task<ActionResult> NapraviZahtevPosao([FromBody] ZahtevPosaoDTO zahtev)
    {
        var usernamePoslodavca = _userService.GetUser();
        var posl = await Context.Poslodavci
                                .Include(p => p.Zahtevi)
                                .Include(p => p.Korisnik)
                                .ThenInclude(p => p.Identitet)
                                .FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernamePoslodavca);
        if (posl == null)
        {
            return BadRequest("Greska prilikom provere identiteta poslodavca!");
        }

        var majstor = await Context.Majstori
                                .Include(p => p.ZahteviPosao)
                                .Include(m => m.Korisnik)
                                .Where(m => m.Korisnik.ID == zahtev.KorisnikID)
                                .FirstOrDefaultAsync();
        if (majstor == null)
        {
            return BadRequest("Problem sa profilom majstora kome želite poslati zahtev!");
        }

        var oglas = (zahtev.OglasID == null) ? null : await Context.Oglasi.FindAsync(zahtev.OglasID);

        var noviZahtevPosao = new ZahtevZaPosao
        {
            Opis = zahtev.Opis,
            CenaPoSatu = zahtev.CenaPoSatu,
            ListaSlika = (zahtev.ListaSlika == null) ? null : new List<string>(zahtev.ListaSlika),
            DatumZavrsetka = zahtev.DatumZavrsetka,
            Majstor = majstor,
            Poslodavac = posl,
            Oglas = zahtev.OglasID == null ? null : oglas,
            Prihvacen = 0
        };

        if (majstor.ZahteviPosao == null)
        {
            majstor.ZahteviPosao = new List<ZahtevZaPosao>();
        }
        if (posl.Zahtevi == null)
        {
            posl.Zahtevi = new List<ZahtevZaPosao>();
        }
        majstor.ZahteviPosao.Add(noviZahtevPosao);
        posl.Zahtevi.Add(noviZahtevPosao);

        await Context.ZahteviZaPosao.AddAsync(noviZahtevPosao);
        await Context.SaveChangesAsync();
        //Context.Majstori.Update(majstor);
        //await Context.SaveChangesAsync();
        //return Ok("Napravljen je i poslat? zahtev za posao!"); //poslat u smislu zakacen u listu zahteva majstora/grupe
        return Ok(noviZahtevPosao.ID);
    }

    [Route("povuciZahtevPosao/{zahtevID}")]
    [HttpDelete, Authorize(Roles = "poslodavac")]
    public async Task<ActionResult> PovuciZahtevPosao(int zahtevID)
    {
        try
        {
            var usernamePoslodavca = _userService.GetUser();
            var poslodavac = await Context.Poslodavci.FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernamePoslodavca);
            if (poslodavac == null)
            {
                return Unauthorized();
            }

            var zahtev = await Context.ZahteviZaPosao.FindAsync(zahtevID);
            if (zahtev == null || zahtev.Poslodavac.ID != poslodavac.ID)
            {
                return BadRequest("Zahtev za posao nije pronađen ili ne pripada ovom poslodavcu.");
            }
            else if (zahtev != null)
            {
                Context.ZahteviZaPosao.Remove(zahtev);
                await Context.SaveChangesAsync();
                //jel ja radim ovo ili nee, jer to baza ne vidi, valjda ce automatski da mi ukloni??
                poslodavac.Zahtevi?.Remove(zahtev);
                var majstor = await Context.Majstori.Include(m => m.ZahteviPosao).Where(m => m.ZahteviPosao != null && m.ZahteviPosao.Contains(zahtev)).FirstOrDefaultAsync();
                majstor?.ZahteviPosao?.Remove(zahtev);
                return Ok($"Zahtev za posao sa ID: {zahtevID} je izbrisana iz baze podataka.");
            }
            else
            {
                return NotFound($"Nije pronađen zahtev sa ID: {zahtevID}");
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

    }

    [HttpPost("zavrsiPosao/{ugovorID}/{uspesno}"), Authorize(Roles = "poslodavac")]
    public async Task<ActionResult> ZavrsiPosao(int uspesno, int ugovorID) //'0' | '1' za neuspesno|uspesno
    {

        var usernamePoslodavca = _userService.GetUser();
        var poslodavac = await Context.Poslodavci.FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernamePoslodavca);
        if (poslodavac == null)
        {
            return Unauthorized();
        }

        var ugovor = await Context.Ugovori
                                .Include(u => u.Poslodavac)
                                .Include(u => u.Majstor)
                                .Include(U => U.ZahtevZaPosao)
                                    .ThenInclude(z => z.Oglas)
                                .FirstOrDefaultAsync(u => u.ID == ugovorID);

        if (ugovor == null)
        {
            return BadRequest("Ne postoji ugovor za posao koji zelite da zavrsite!");
        }

        if (ugovor.Poslodavac.ID != poslodavac.ID)
        {
            return BadRequest("Ugovor ne pripada Vama!");
        }

        var oglas = ugovor.ZahtevZaPosao.Oglas;

        if (uspesno == 0) //oglas se vraca iz arhive
        {
            ugovor.Status = "neuspesnoZavrsen";
            if (ugovor.ZahtevZaPosao.Oglas != null)
            {
                if (oglas != null)
                {
                    oglas.Prikazan = true;
                    var veza = await Context.MajstoriOglasi.Where(v => v.OglasID == oglas.ID && v.MajstorId == ugovor.Majstor.ID).FirstOrDefaultAsync(); //dodala da ne bi ostao kao prijavljen 
                    if (veza != null)
                    {
                        Context.MajstoriOglasi.Remove(veza);
                    }
                }
            }
        }
        else if (uspesno == 1)
        {
            ugovor.Status = "uspesnoZavrsen";
            if (oglas != null)
            {
                var veze = await Context.MajstoriOglasi.Where(v => v.OglasID == oglas.ID).ToListAsync(); //mora pre brisanja oglasa zbog FK
                if (veze != null)
                {
                    Context.MajstoriOglasi.RemoveRange(veze);
                }
                var ugovori = await Context.Ugovori
                                        .Include(u => u.ZahtevZaPosao)
                                            .ThenInclude(z => z.Oglas)
                                        .Where(u => u.ZahtevZaPosao.Oglas != null && u.ZahtevZaPosao.Oglas.ID == oglas.ID && u.ID != ugovor.ID)
                                        .ToListAsync();

                var zahteviPosao = await Context.ZahteviZaPosao
                                                    .Include(z => z.Oglas)
                                                    .Where(z => z.Oglas != null && z.Oglas.ID == oglas.ID && z.ID != ugovor.ZahtevZaPosao.ID)
                                                    .ToListAsync();
                var lista = new List<ZahtevZaPosao>(zahteviPosao);
                foreach (var u in ugovori)
                {
                    if (u.Status != "neuspesnoZavrsen")
                    {
                        return BadRequest("Ne mozete obrisati oglas jer je preko njega poslat zahtev, a preko zahteva sklopljen ugovor!");
                    }
                    foreach (var z in zahteviPosao)
                    {
                        if (u.ZahtevZaPosao.ID == z.ID)
                        {
                            lista.Remove(z);
                        }

                    }

                }

                // if (ugovor != null)
                //     return BadRequest("Ne mozete obrisati oglas jer je preko njega poslat zahtev, a preko zahteva sklopljen ugovor!");


                if (lista != null) //nadam se da ne pravi problem, dodala!!
                {
                    Context.ZahteviZaPosao.RemoveRange(lista);
                }
                Context.Oglasi.Remove(oglas);
            }

        }
        else
        {
            return BadRequest("Odgovor mora biti 0 - za neuspesno, ili 1 - uspesno zavrsen posao!");
        }
        Context.Ugovori.Update(ugovor);
        await Context.SaveChangesAsync();
        return Ok("Posao je zavrsen!");
    }

    [Route("AzurirajPoslodavac2")]
    [HttpPut, Authorize(Roles = "poslodavac")]
    public async Task<ActionResult> AzurirajPoslodavac2([FromBody] PoslodavacDTO noviPoslodavac)
    {

        try
        {
            var usernamePoslodavca = _userService.GetUser();
            var poslodavac = await Context.Poslodavci
                                    .Include(p => p.Korisnik)
                                        .ThenInclude(v => v.Identitet)
                                    .FirstOrDefaultAsync(p => p.Korisnik.Identitet.Username == usernamePoslodavca);
            if (poslodavac == null)
            {
                return NotFound("Poslodavac nije pronadjen!");
            }

            var korisnik = await Context.Korisnici
                                .Include(k => k.Identitet)
                                .Include(k => k.Grad)
                                .Where(k => k.Identitet.Username == usernamePoslodavca)
                                .FirstOrDefaultAsync();
            if (korisnik == null)
            {
                return NotFound("Korisnik nije pronadjen!");
            }

            korisnik.Naziv = noviPoslodavac.Naziv;
            korisnik.Slika = noviPoslodavac.Slika;
            korisnik.Opis = noviPoslodavac.Opis;
            if (korisnik.Grad.ID != noviPoslodavac.GradID)
            {
                var grad = await Context.Gradovi.FindAsync(noviPoslodavac.GradID);
                if (grad == null)
                    return NotFound("Grad nije pronadjen!");
                korisnik.Grad = grad;
            }

            poslodavac.Adresa = noviPoslodavac.Adresa;
            //Context.Poslodavci.Update(poslodavac);
            //Context.Korisnici.Update(korisnik);              
            await Context.SaveChangesAsync();
            return Ok("Poslodavac je uspesno izmenjen");

        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

    }
}
