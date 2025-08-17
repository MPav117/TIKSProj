using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebTemplate.Controllers;
using WebTemplate.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TIKSNUnit;

[TestFixture, Order(3)]
public class TestsPoslodavac
{
    private ZanatstvoContext context;
    private OsnovniController osnovni;
    private ProfilController profil;
    private PoslodavacController poslodavac;
    private KorisnikController korisnik;
    private UserService service;

    [OneTimeSetUp]
    public void Init()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddJsonFile("appsettings.json");
        builder.Services.AddHttpContextAccessor();
        var config = builder.Configuration;

        var options = new DbContextOptionsBuilder<ZanatstvoContext>()
            .UseSqlServer("Server=(localdb)\\Ispit;Database=UfindiDB")
            .Options;
        context = new(options);
        var httpContext = new DefaultHttpContext();
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };
        service = new(httpContextAccessor);
        osnovni = new(context, service);
        profil = new(context, config, service)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            }
        };
        poslodavac = new(context, service)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            }
        };
        korisnik = new(context, service)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            }
        };
    }

    private async Task LoginUser(string username, string role)
    {
        IdentitetDTO i = new()
        {
            Username = username,
            Password = username
        };
        OkObjectResult rez = (OkObjectResult)await profil.Login(i);
        string p1jwt = (string)rez.Value!;

        var token = new JwtSecurityTokenHandler().ReadJwtToken(p1jwt);
        List<Claim> claims = (List<Claim>)token.Claims;
        claims.Add(new Claim(ClaimTypes.Role, role));
        var identity = new ClaimsPrincipal(new ClaimsIdentity(claims));
        service._httpContextAccessor.HttpContext!.User = identity;

        service._httpContextAccessor.HttpContext!.Request.Headers.Authorization = p1jwt;
    }

    private void LogoutUser()
    {
        profil.Logout();
        service._httpContextAccessor.HttpContext!.User = new ClaimsPrincipal();
        service._httpContextAccessor.HttpContext!.Request.Headers.Authorization = "";
    }




    //AZURIRAJ POSLODAVAC
    [Test, Order(0)]
    public async Task TestAzurirajPoslodavacLogin()
    {
        PoslodavacDTO p = new()
        {
            Username = "testPoslodavac14",
            Password = "testPoslodavac14",
            Tip = "poslodavac",
            Naziv = "testPoslodavac14",
            Opis = "testPoslodavac14",
            GradID = 1,
            Adresa = "testPoslodavac14",
            Email = $"testPoslodavac14@gmail.com",
            Povezani = 0
        };

        var r = await poslodavac.AzurirajPoslodavac2(p);
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Poslodavac nije pronadjen!"));
    }

    [Test, Order(0)]
    public async Task TestAzurirajPoslodavacGrad([Values(0, 50000)]int grad)
    {
        await LoginUser("testPoslodavac14", "poslodavac");

        PoslodavacDTO p = new()
        {
            Username = "testPoslodavac14",
            Password = "testPoslodavac14",
            Tip = "poslodavac",
            Naziv = "testPoslodavac14",
            Opis = "testPoslodavac14",
            GradID = grad,
            Adresa = "testPoslodavac14",
            Email = $"testPoslodavac14@gmail.com",
            Povezani = 0
        };

        var r = await poslodavac.AzurirajPoslodavac2(p);
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Grad nije pronadjen!"));

        LogoutUser();
    }
    
    [Test, Order(1)]
    public async Task TestAzurirajPoslodavac()
    {
        await LoginUser("testPoslodavac14", "poslodavac");

        PoslodavacDTO p = new()
        {
            Username = "testPoslodavac14",
            Password = "testPoslodavac14",
            Tip = "poslodavac",
            Naziv = "Novi naziv",
            Opis = "Novi opis",
            GradID = 100,
            Adresa = "Nova adresa",
            Email = "testPoslodavac14@gmail.com",
            Povezani = 0
        };

        var r = await poslodavac.AzurirajPoslodavac2(p);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)await profil.PodaciRegistracije();
        var rows = JObject.FromObject(rez.Value!);
        Assert.Multiple(() =>
        {
            Assert.That(rows.Value<string>("Naziv"), Is.EqualTo("Novi naziv"));
            Assert.That(rows.Value<string>("Opis"), Is.EqualTo("Novi opis"));
            Assert.That(rows.Value<string>("Adresa"), Is.EqualTo("Nova adresa"));
            Assert.That(rows.Value<int>("gradID"), Is.EqualTo(100));
        });

        LogoutUser();
    }



    //POSTAVI OGLAS

    [Test, Order(2)]
    public async Task TestPostaviOglasLogin()
    {
        OglasDTO o = new()
        {
            ListaVestina = ["stolar"],
            Naslov = "testOglasT1",
            Opis = "testOglasT1",
            CenaPoSatu = 10,
            DatumZavrsetka = DateTime.Parse("1.1.2030.")
        };

        var r = await poslodavac.PostaviOglas(o);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
    }
    
    [Test, Order(3)]
    public async Task TestPostaviOglas()
    {
        await LoginUser("testPoslodavac1", "poslodavac");

        OglasDTO o = new()
        {
            ListaVestina = ["stolar"],
            Naslov = "testOglasT1",
            Opis = "testOglasT1",
            CenaPoSatu = 10,
            DatumZavrsetka = DateTime.Parse("1.1.2030.")
        };

        var r = await poslodavac.PostaviOglas(o);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)await osnovni.GetOglas(16);
        var rows = JObject.FromObject(rez.Value!);
        Assert.That(rows.Value<string>("Naslov"), Is.EqualTo("testOglasT1"));

        LogoutUser();
    }



    //IZBRISI OGLAS

    [Test, Order(4)]
    public async Task TestIzbrisiOglasLogin()
    {
        var r = await poslodavac.IzbrisiOglas(16);
        Assert.That(r, Is.TypeOf<UnauthorizedResult>());
    }

    [Test, Order(4)]
    public async Task TestIzbrisiOglasID([Values(0, 2, 20)]int id)
    {
        await LoginUser("testPoslodavac1", "poslodavac");

        var r = await poslodavac.IzbrisiOglas(id);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo(id == 0 ? "Pogresan id" : "Oglas nije pronađen ili ne pripada ovom poslodavcu."));

        LogoutUser();
    }

    [Test, Order(4)]
    public async Task TestIzbrisiOglasUgovor()
    {
        await LoginUser("testPoslodavac2", "poslodavac");

        var r = await poslodavac.IzbrisiOglas(2);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Ne mozete obrisati oglas jer je preko njega poslat zahtev, a preko zahteva sklopljen ugovor!"));

        LogoutUser();
    }
    
    [Test, Order(5)]
    public async Task TestIzbrisiOglas()
    {
        await LoginUser("testPoslodavac3", "poslodavac");

        var r = await poslodavac.IzbrisiOglas(3);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        var ro = await osnovni.GetOglas(3);
        Assert.That(ro, Is.TypeOf<BadRequestObjectResult>());

        LogoutUser();
    }





    [Test, Order(6)]
    public async Task TestNapraviZahtevPosaoLogin()
    {
        ZahtevPosaoDTO z = new()
        {
            KorisnikID = 17,
            Opis = "testZahtevOpis",
            CenaPoSatu = 15,
            DatumZavrsetka = DateTime.Parse("1.2.3033.")
        };
        var r = await poslodavac.NapraviZahtevPosao(z);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Greska prilikom provere identiteta poslodavca!"));
    }

    [Test, Order(6)]
    public async Task TestNapraviZahtevPosaoMajstor([Values(2, 60)]int id)
    {
        await LoginUser("testPoslodavac1", "poslodavac");

        ZahtevPosaoDTO z = new()
        {
            KorisnikID = id,
            Opis = "testZahtevOpis",
            CenaPoSatu = 15,
            DatumZavrsetka = DateTime.Parse("1.2.3033.")
        };
        var r = await poslodavac.NapraviZahtevPosao(z);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Problem sa profilom majstora kome želite poslati zahtev!"));

        LogoutUser();
    }
    
    [Test, Order(7)]
    public async Task TestNapraviZahtevPosao()
    {
        await LoginUser("testPoslodavac1", "poslodavac");

        ZahtevPosaoDTO z = new()
        {
            KorisnikID = 27,
            Opis = "testZahtevOpis-1-27-",
            CenaPoSatu = 15,
            DatumZavrsetka = DateTime.Parse("1.2.2033.")
        };
        var r = await poslodavac.NapraviZahtevPosao(z);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rz = (OkObjectResult)await korisnik.GetZahteviPosaoMajstorGrupa();
        var list = JArray.FromObject(rz.Value!);
        Assert.That(list.First, Is.Not.Null);
        Assert.That(list.First.Value<string>("Opis"), Is.EqualTo("testZahtevOpis-1-27-"));

        await LoginUser("testMajstor12", "majstor");
        rz = (OkObjectResult)await korisnik.GetZahteviPosaoMajstorGrupa();
        list = JArray.FromObject(rz.Value!);
        Assert.That(list.First, Is.Not.Null);
        Assert.That(list.First.Value<string>("Opis"), Is.EqualTo("testZahtevOpis-1-27-"));

        LogoutUser();
    }





    [Test, Order(8)]
    public async Task TestPovuciZahtevPosaoLogin()
    {
        LogoutUser();

        var r = await poslodavac.PovuciZahtevPosao(124);
        Assert.That(r, Is.TypeOf<UnauthorizedResult>());
    }

    [Test, Order(8)]
    public async Task TestPovuciZahtevPosaoZahtev([Values(1, 0, 200)]int id)
    {
        await LoginUser("testPoslodavac1", "poslodavac");

        var r = await poslodavac.PovuciZahtevPosao(id);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Zahtev za posao nije pronađen ili ne pripada ovom poslodavcu."));

        LogoutUser();
    }
    
    [Test, Order(9)]
    public async Task TestPovuciZahtevPosao()
    {
        await LoginUser("testPoslodavac1", "poslodavac");

        var r = await poslodavac.PovuciZahtevPosao(125);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rz = (OkObjectResult)await korisnik.GetZahteviPosaoMajstorGrupa();
        Assert.That(rz.Value, Is.Empty);

        await LoginUser("testMajstor12", "majstor");
        rz = (OkObjectResult)await korisnik.GetZahteviPosaoMajstorGrupa();
        Assert.That(rz.Value, Is.Empty);

        LogoutUser();
    }



    //ZAVRSI POSAO

    [Test, Order(10)]
    public async Task TestZavrsiPosaoLogin()
    {
        LogoutUser();

        var r = await poslodavac.ZavrsiPosao(1, 11);
        Assert.That(r, Is.TypeOf<UnauthorizedResult>());
    }

    [Test, Order(10)]
    public async Task TestZavrsiPosaoUgovor([Values(0, 1, 100)]int id)
    {
        await LoginUser("testPoslodavac7", "poslodavac");

        var r = await poslodavac.ZavrsiPosao(1, id);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo(id == 1 ? "Ugovor ne pripada Vama!" : "Ne postoji ugovor za posao koji zelite da zavrsite!"));

        LogoutUser();
    }

    [Test, Order(10)]
    public async Task TestZavrsiPosaoVrednost([Values(-1, 2)]int v)
    {
        await LoginUser("testPoslodavac7", "poslodavac");

        var r = await poslodavac.ZavrsiPosao(v, 11);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Odgovor mora biti 0 - za neuspesno, ili 1 - uspesno zavrsen posao!"));

        LogoutUser();
    }

    [Test, Order(11)]
    public async Task TestZavrsiPosao1()
    {
        await LoginUser("testPoslodavac2", "poslodavac");

        var r = await poslodavac.ZavrsiPosao(0, 8);
        Assert.That(r, Is.TypeOf<OkObjectResult>());

        OkObjectResult ru = (OkObjectResult)await korisnik.GetUgovori();
        var list = JArray.FromObject(ru.Value!);
        foreach (var item in list)
        {
            if (item.Value<int>("ID")! == 8)
                Assert.That(item.Value<string>("Status")!, Is.EqualTo("neuspesnoZavrsen"));
        }

        await LoginUser("testMajstor2", "majstor");

        OkObjectResult ro = (OkObjectResult)await korisnik.GetOglasi();
        list = JArray.FromObject(ro.Value!);
        foreach (var item in list)
        {
            Assert.That(item.Value<int>("ID"), Is.Not.EqualTo(2));
        }

        LogoutUser();
    }

    [Test, Order(12)]
    public async Task TestZavrsiPosao2()
    {
        await LoginUser("testPoslodavac2", "poslodavac");

        var r = await poslodavac.ZavrsiPosao(1, 16);
        Assert.That(r, Is.TypeOf<OkObjectResult>());

        OkObjectResult ru = (OkObjectResult)await korisnik.GetUgovori();
        var list = JArray.FromObject(ru.Value!);
        foreach (var item in list)
        {
            if (item.Value<int>("ID")! == 16)
                Assert.That(item.Value<string>("Status")!, Is.EqualTo("uspesnoZavrsen"));
        }

        OkObjectResult ro = (OkObjectResult)await korisnik.GetOglasi();
        list = JArray.FromObject(ro.Value!);
        foreach (var item in list)
        {
            Assert.That(item.Value<int>("ID"), Is.Not.EqualTo(2));
        }

        LogoutUser();
	}





	[OneTimeTearDown]
	public async Task TearDown()
	{
		await context.DisposeAsync();
	}
}