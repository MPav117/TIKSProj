using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebTemplate.Controllers;
using WebTemplate.Models;

namespace TIKSNUnit;

[TestFixture, Order(2)]
public class TestsKorisnik
{
    private ZanatstvoContext context;
    private OsnovniController osnovni;
    private ProfilController profil;
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




    [Test, Order(0)]
    public async Task TestGetZahteviPosaoLogin()
    {
        LogoutUser();

        var r = await korisnik.GetZahteviPosaoMajstorGrupa();
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Neispravan token?"));
    }

    [Test, Order(0)]
    public async Task TestGetZahteviPosao([Values(6, 17)]int id)
    {
        if (id == 6)
            await LoginUser("testPoslodavac6", "poslodavac");
        else
            await LoginUser("testMajstor2", "majstor");

        var r = await korisnik.GetZahteviPosaoMajstorGrupa();
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        string list = JsonConvert.SerializeObject(rez.Value, Formatting.None,
                                new JsonSerializerSettings()
                                {
                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                });
        var lista = JArray.Parse(list);
        //Assert.That(lista.First, Is.Not.Null);
        foreach (var item in lista)
        {
            if (!item.Value<bool>("zahtevGrupe"))
                Assert.That(item.Value<string>("Opis"), Does.Contain($"-{id}-"));
            else
                Assert.That(item.Value<string>("Opis"), Does.Contain($"-42-"));
        }

        LogoutUser();
    }

    [Test, Order(0)]
    public async Task TestGetUgovoriLogin()
    {
        LogoutUser();

        var r = await korisnik.GetUgovori();
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Neispravan token?"));
    }

    [Test, Order(0)]
    public async Task TestGetUgovori([Values(6, 7)] int id)
    {
        if (id == 6)
            await LoginUser("testPoslodavac6", "poslodavac");
        else
            await LoginUser("testMajstor7", "majstor");

        var r = await korisnik.GetUgovori();
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        var list = JArray.FromObject(rez.Value);
        //Assert.That(list.First, Is.Not.Null);
        if (id == 6)
            foreach (var item in list)
            {
                if (item.Value<string>("Status") == "potpisan" || item.Value<string>("Status") == "potpisaoPoslodavac")
                    Assert.That(item.Value<string>("ImePoslodavca"), Is.EqualTo("testPoslodavac6"));
            }
        else
            foreach (var item in list)
            {
                if (item.Value<string>("Status") == "potpisan" || item.Value<string>("Status") == "potpisaoMajstor")
                    Assert.That(item.Value<string>("ImeMajstora"), Is.EqualTo("testMajstor7"));
            }

        LogoutUser();
    }



    //NAPRAVI RECENZIJU

    [Test, Order(0)]
    public async Task TestNapraviRecenzijuLogin()
    {
        LogoutUser();

        RecenzijaDTO rev = new()
        {
            IdPrimalac = 17,
            IdUgovor = 8,
            Opis = "testRecenzija-2-17-",
            Ocena = 5
        };

        var r = await korisnik.NapraviRecenziju(rev);
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Korisnik nije pronađen."));
    }

    [Test, Order(0)]
    public async Task TestNapraviRecenzijuPrimalac()
    {
        await LoginUser("testPoslodavac2", "poslodavac");

        RecenzijaDTO rev = new()
        {
            IdPrimalac = 70,
            IdUgovor = 8,
            Opis = "testRecenzija-2-17-",
            Ocena = 5
        };

        var r = await korisnik.NapraviRecenziju(rev);
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Primalac nije pronađen."));

        LogoutUser();
    }

    [Test, Order(0)]
    public async Task TestNapraviRecenzijuUgovor()
    {
        await LoginUser("testPoslodavac2", "poslodavac");

        RecenzijaDTO rev = new()
        {
            IdPrimalac = 17,
            IdUgovor = 9,
            Opis = "testRecenzija-2-17-",
            Ocena = 5
        };

        var r = await korisnik.NapraviRecenziju(rev);
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Nije pronadjen ugovor izmedju vas i zadatog korisnika"));

        LogoutUser();
    }

    [Test, Order(0)]
    public async Task TestNapraviRecenzijuOcena([Values(0, 6)]int grade)
    {
        await LoginUser("testPoslodavac2", "poslodavac");

        RecenzijaDTO rev = new()
        {
            IdPrimalac = 17,
            IdUgovor = 8,
            Opis = "testRecenzija-2-17-",
            Ocena = grade
        };

        var r = await korisnik.NapraviRecenziju(rev);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Ocena mora da bude izmedju 1 i 5"));

        LogoutUser();
    }
    
    [Test, Order(1)]
    public async Task TestNapraviRecenziju1()
    {
        await LoginUser("testPoslodavac2", "poslodavac");

        RecenzijaDTO rev = new()
        {
            IdPrimalac = 17,
            IdUgovor = 8,
            Opis = "testRecenzija-2-17-",
            Ocena = 3
        };

        var r = await korisnik.NapraviRecenziju(rev);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Recenzija je napravljena"));
        OkObjectResult rr = (OkObjectResult)await osnovni.GetRecenzije(17);
        string list = JsonConvert.SerializeObject(rr.Value, Formatting.None,
                                new JsonSerializerSettings()
                                {
                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                });
        var lista = JArray.Parse(list);
        foreach (var item in lista)
        {
            if (item.Value<string>("Opis")!.Equals("testRecenzija-2-17-"))
            {
                Assert.That(item.Value<int>("Ocena"), Is.EqualTo(3));
            }
        }

        LogoutUser();
    }
    
    [Test, Order(2)]
    public async Task TestNapraviRecenziju2()
    {
        await LoginUser("testPoslodavac2", "poslodavac");

        RecenzijaDTO rev = new()
        {
            IdPrimalac = 17,
            IdUgovor = 8,
            Opis = "testRecenzija-2-17-2",
            Ocena = 5
        };

        var r = await korisnik.NapraviRecenziju(rev);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("uspesno izmenjena recenzija"));
        OkObjectResult rr = (OkObjectResult)await osnovni.GetRecenzije(17);
        string list = JsonConvert.SerializeObject(rr.Value, Formatting.None,
                                new JsonSerializerSettings()
                                {
                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                });
        var lista = JArray.Parse(list);
        foreach (var item in lista)
        {
            if (item.Value<string>("Opis")!.StartsWith("testRecenzija-2-17-"))
            {
                Assert.That(item.Value<int>("Ocena"), Is.EqualTo(5));
            }
        }

        LogoutUser();
    }



    //POTPISI UGOVOR

    [Test, Order(3)]
    public async Task TestPotpisiUgovorLogin()
    {
        LogoutUser();

        UgovorDTO u = new()
        {
            ID = 1,
            CenaPoSatu = 10,
            DatumPocetka = DateTime.Parse("3.3.2027."),
            DatumZavrsetka = DateTime.Parse("4.3.2027."),
            ImeMajstora = $"testMajstor4",
            ImePoslodavca = $"testPoslodavac6",
            MajstorID = 19,
            PoslodavacID = 6,
            Opis = "testUgovor-6-19-",
            PotpisMajstora = "testMajstor4",
            PotpisPoslodavca = $"testPoslodavac6",
            ZahtevZaPosaoID = 3
        };

        var r = await korisnik.PotpisiUgovor(u);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Doslo je do greske"));
    }

    [Test, Order(3)]
    public async Task TestPotpisiUgovorUgovor([Values(6, 19)]int id)
    {
        if (id == 6)
            await LoginUser("testPoslodavac6", "poslodavac");
        else
            await LoginUser("testMajstor4", "majstor");

        UgovorDTO u = new()
        {
            ID = 76,
            CenaPoSatu = 10,
            DatumPocetka = DateTime.Parse("3.3.2027."),
            DatumZavrsetka = DateTime.Parse("4.3.2027."),
            ImeMajstora = $"testMajstor4",
            ImePoslodavca = $"testPoslodavac6",
            MajstorID = 19,
            PoslodavacID = 6,
            Opis = "testUgovor-6-19-",
            PotpisMajstora = "testMajstor4",
            PotpisPoslodavca = "testPoslodavac6",
            ZahtevZaPosaoID = 3
        };

        var r = await korisnik.PotpisiUgovor(u);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Ugovor ne pripada vama!"));
        
        LogoutUser();
    }

    [Test, Order(3)]
    public async Task TestPotpisiUgovorDatum([Values(1, 2, 3, 4, 5)]int i)
    {
        string name;
        DateTime s, e;
        if (i < 4)
            name = "testMajstor6";
        else
            name = "testGrupa2";
        await LoginUser(name, "majstor");

        switch(i)
        {
            case 1:
            case 4:
                s = DateTime.Parse("7.6.2026");
                e = DateTime.Parse("8.7.2026");
                break;
            case 2:
            case 5:
                s = DateTime.Parse("2.10.2036");
                e = DateTime.Parse("3.10.2036");
                break;
            default:
                s = DateTime.Parse("2.7.2035");
                e = DateTime.Parse("3.7.2035");
                break;
        }

        UgovorDTO u = new()
        {
            ID = i < 4 ? 75 : 74,
            CenaPoSatu = 10,
            DatumPocetka = s,
            DatumZavrsetka = e,
            ImeMajstora = name,
            ImePoslodavca = "testPoslodavac6",
            MajstorID = i < 4 ? 21 : 42,
            PoslodavacID = 6,
            Opis = "testUgovor-6-19-",
            PotpisMajstora = name,
            PotpisPoslodavca = "testPoslodavac6",
            ZahtevZaPosaoID = i < 4 ? 123 : 122
        };

        var r = await korisnik.PotpisiUgovor(u);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Dani se preklapaju"));

        LogoutUser();
    }

    [Test, Order(3)]
    public async Task TestPotpisiUgovorPodaci([Values(18, 76)]int id)
    {
        if (id == 18)
            await LoginUser("testPoslodavac8", "poslodavac");
        else
            await LoginUser("testMajstor8", "majstor");

        UgovorDTO u = new()
        {
            ID = id,
            CenaPoSatu = 100,
            DatumPocetka = DateTime.Parse("10.10.2030."),
            DatumZavrsetka = DateTime.Parse("11.10.2030."),
            ImeMajstora = "testMajstor8",
            ImePoslodavca = "testPoslodavac8",
            MajstorID = 8,
            PoslodavacID = 8,
            Opis = "testUgovor-8-23-",
            PotpisMajstora = "testMajstor8",
            PotpisPoslodavca = "testPoslodavac8",
            ZahtevZaPosaoID = id == 18 ? 29 : 124
        };

        var r = await korisnik.PotpisiUgovor(u);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo(id == 18 ? "Podaci u ugovoru se ne poklapaju sa podacima koje je uneo majstor!" : "Podaci u ugovoru se ne poklapaju sa podacima koje je uneo poslodavac!"));

        LogoutUser();
    }
    
    [Test, Order(4)]
    public async Task TestPotpisiUgovor([Values()]bool p)
    {
        if (p)
            await LoginUser("testPoslodavac6", "poslodavac");
        else
            await LoginUser("testMajstor4", "majstor");

        UgovorDTO u = new()
        {
            ID = 1,
            CenaPoSatu = 100,
            DatumPocetka = DateTime.Parse("10.10.2030."),
            DatumZavrsetka = DateTime.Parse("11.10.2030."),
            ImeMajstora = "testMajstor4",
            ImePoslodavca = "testPoslodavac6",
            MajstorID = 4,
            PoslodavacID = 6,
            Opis = "testUgovor-6-19-",
            PotpisMajstora = "testMajstor4",
            PotpisPoslodavca = "testPoslodavac6",
            ZahtevZaPosaoID = 4
        };

        var r = await korisnik.PotpisiUgovor(u);
        Assert.That(r, Is.TypeOf<OkResult>());
        var ru = (OkObjectResult)await korisnik.GetUgovori();
        var list = JArray.FromObject(ru.Value!);
        foreach (var item in list)
        {
            if (item.Value<int>("ID") == 1)
                Assert.That(item.Value<string>("Opis"), Is.EqualTo("testUgovor-6-19-"));
        }

        LogoutUser();
    }



    //RASKINI UGOVOR

    [Test, Order(5)]
    public async Task TestRaskiniUgovorLogin()
    {
        LogoutUser();

        var r = await korisnik.RaskiniUgovor(1);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Doslo je do greske"));
    }

    [Test, Order(5)]
    public async Task TestRaskiniUgovorUgovor([Values(100, 1, 2)]int id)
    {
        if (id == 1)
            await LoginUser("testPoslodavac1", "poslodavac");
        else if (id == 2)
            await LoginUser("testMajstor2", "majstor");

        var r = await korisnik.RaskiniUgovor(id);
        if (id < 100)
        {
            Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
            BadRequestObjectResult rez = (BadRequestObjectResult)r;
            Assert.That(rez.Value, Is.EqualTo("Ugovor ne pripada vama!"));

            LogoutUser();
        }
        else
        {
            Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
            NotFoundObjectResult rez = (NotFoundObjectResult)r;
            Assert.That(rez.Value, Is.EqualTo("Ugovor nije pronađen."));
        }
    }

    [Test, Order(5)]
    public async Task TestRaskiniUgovorStatus([Values()]bool p)
    {
        if (p)
            await LoginUser("testPoslodavac7", "poslodavac");
        else
            await LoginUser("testMajstor4", "majstor");

        var r = await korisnik.RaskiniUgovor(9);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Ne mozete raskinuti ugovor!"));

        LogoutUser();
    }

    [Test, Order(6)]
    public async Task TestRaskiniUgovor1([Values()] bool p)
    {
        if (p)
            await LoginUser("testPoslodavac6", "poslodavac");
        else
            await LoginUser("testMajstor7", "majstor");

        var r = await korisnik.RaskiniUgovor(p ? 3 : 4);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        var ru = (OkObjectResult)await korisnik.GetUgovori();
        var list = JArray.FromObject(ru.Value!);
        foreach (var item in list)
        {
            if (item.Value<int>("ID") == (p ? 3 : 4))
                Assert.That(item.Value<string>("Status"), Is.EqualTo(p ? "raskidaPoslodavac" : "raskidaMajstor"));
        }

        LogoutUser();
    }

    [Test, Order(7)]
    public async Task TestRaskiniUgovor2([Values()] bool p)
    {
        if (p)
            await LoginUser("testPoslodavac6", "poslodavac");
        else
            await LoginUser("testMajstor6", "majstor");

        var r = await korisnik.RaskiniUgovor(p ? 4 : 3);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        var ru = (OkObjectResult)await korisnik.GetUgovori();
        var list = JArray.FromObject(ru.Value!);
        foreach (var item in list)
        {
            Assert.That(item.Value<int>("ID"), Is.Not.EqualTo(p ? 4 : 3));
        }

        LogoutUser();
    }





    [Test, Order(8)]
    public async Task TestGetOglasiLogin()
    {
        LogoutUser();

        var r = await korisnik.GetOglasi();
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Neispravan token?"));
    }

    [Test, Order(8)]
    public async Task TestGetOglasi([Values(3, 17)] int id)
    {
        if (id == 3)
            await LoginUser("testPoslodavac3", "poslodavac");
        else
            await LoginUser("testMajstor2", "majstor");

        var r = await korisnik.GetOglasi();
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        var lista = JArray.FromObject(rez.Value);
        foreach (var item in lista)
        {
            if (id == 3)
                Assert.That(item.Value<int>("PoslKorisnikID"), Is.EqualTo(id));
            else
                Assert.That(item["prijavljeni"]!.Values<int>(), Has.Member(id));
        }

        LogoutUser();
    }





    [OneTimeTearDown]
    public async Task TearDown()
    {
        await context.DisposeAsync();
    }
}