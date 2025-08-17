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
using Newtonsoft.Json;

namespace TIKSNUnit;

[TestFixture, Order(4)]
public class TestsMajstor
{
    private ZanatstvoContext context;
    private OsnovniController osnovni;
    private ProfilController profil;
    private MajstorController majstor;
    private KorisnikController korisnik;
    private PoslodavacController poslodavac;
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
        majstor = new(context, service)
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
        poslodavac = new(context, service)
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



    //AZURIRAJ MAJSTOR

    [Test, Order(0)]
    public async Task TestAzurirajMajstorLogin()
    {
        MajstorDTO m = new()
        {
            Username = "testMajstor14",
            Password = "testMajstor14",
            Tip = "majstor",
            Naziv = "testMajstor14",
            Opis = "testMajstor14",
            GradID = 1,
            Email = "testMajstor14@gmail.com",
            Povezani = 0,
            ListaVestina = ["keramicar"],
            TipMajstora = "majstor"
        };

        var r = await majstor.AzurirajMajstor2(m);
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Majstor nije pronadjen!"));
    }

    [Test, Order(0)]
    public async Task TestAzurirajMajstorGrad([Values(0, 50000)] int grad)
    {
        await LoginUser("testMajstor14", "majstor");

        MajstorDTO m = new()
        {
            Username = "testMajstor14",
            Password = "testMajstor14",
            Tip = "majstor",
            Naziv = "testMajstor14",
            Opis = "testMajstor14",
            GradID = grad,
            Email = "testMajstor14@gmail.com",
            Povezani = 0,
            ListaVestina = ["keramicar"],
            TipMajstora = "majstor"
        };

        var r = await majstor.AzurirajMajstor2(m);
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Grad nije pronadjen!"));

        LogoutUser();
    }

    
    [Test, Order(1)]
    public async Task TestAzurirajMajstor()
    {
        await LoginUser("testMajstor14", "majstor");

        MajstorDTO m = new()
        {
            Username = "testMajstor14",
            Password = "testMajstor14",
            Tip = "majstor",
            Naziv = "Novi Naziv",
            Opis = "Novi Opis",
            GradID = 100,
            Email = "testMajstor14@gmail.com",
            Povezani = 0,
            ListaVestina = ["keramicar"],
            TipMajstora = "majstor"
        };

        var r = await majstor.AzurirajMajstor2(m);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)await profil.PodaciRegistracije();
        var rows = JObject.FromObject(rez.Value!);

        Assert.Multiple(() =>
        {
            Assert.That(rows.Value<string>("Naziv"), Is.EqualTo("Novi Naziv"));
            Assert.That(rows.Value<string>("Opis"), Is.EqualTo("Novi Opis"));
            Assert.That(rows["ListaVestina"]!.Values<string>(), Is.EquivalentTo(new List<string> { "keramicar" }));
            Assert.That(rows.Value<int>("gradID"), Is.EqualTo(100));
        });

        LogoutUser();
    }



    //PRIJAVI NA OGLAS

    [Test, Order(2)]
    public async Task TestPrijaviNaOglasLogin()
    {
        LogoutUser();

        var r = await majstor.PrijaviNaOglas(8);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("majstor nije pronadjen"));
    }

    [Test, Order(2)]
    public async Task TestPrijaviNaOglasOglas()
    {
        await LoginUser("testMajstor4", "majstor");

        var r = await majstor.PrijaviNaOglas(50);
        Assert.That(r, Is.TypeOf<NotFoundResult>());
        
        LogoutUser();
    }

    [Test, Order(2)]
    public async Task TestPrijaviNaOglasPrijava()
    {
        await LoginUser("testMajstor4", "majstor");

        var r = await majstor.PrijaviNaOglas(4);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("majstor je već prijavljen na ovaj oglas"));

        LogoutUser();
    }

    [Test, Order(3)]
    public async Task TestPrijaviNaOglas()
    {
        await LoginUser("testMajstor4", "majstor");

        var r = await majstor.PrijaviNaOglas(8);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        var objs = JsonConvert.SerializeObject(rez.Value, Formatting.None,
                                new JsonSerializerSettings()
                                {
                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                });
        var obj = JObject.Parse(objs);
        Assert.Multiple(() =>
        {
            Assert.That(obj.Value<int>("MajstorId"), Is.EqualTo(4));
            Assert.That(obj.Value<int>("OglasID"), Is.EqualTo(8));
        });

        LogoutUser();
    }



    //ODJAVI SA OGLASA

    [Test, Order(4)]
    public async Task TestOdjaviSaOglasaLogin()
    {
        LogoutUser();

        var r = await majstor.OdjaviSaOglasa(4);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("majstor nije pronadjen"));
    }

    [Test, Order(4)]
    public async Task TestOdjaviSaOglasaPrijava([Values(0, 7, 100)]int id)
    {
        await LoginUser("testMajstor4", "majstor");

        var r = await majstor.OdjaviSaOglasa(id);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        if (id == 0)
            Assert.That(rez.Value, Is.EqualTo("Pogresan id"));
        else
            Assert.That(rez.Value, Is.EqualTo("Niste prijavljeni na taj oglas"));

        LogoutUser();
    }

    [Test, Order(5)]
    public async Task TestOdjaviSaOglasa()
    {
        await LoginUser("testMajstor4", "majstor");

        var r = await majstor.OdjaviSaOglasa(8);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)await osnovni.GetOglas(8);
        var obj = JObject.FromObject(rez.Value!);
        Assert.That(obj["prijavljeni"], Is.Empty);

        LogoutUser();
    }





    [Test, Order(6)]
    public async Task TestGetKalendarMajstor([Values(0, 100)]int id)
    {
        var r = await majstor.GetKalendar(id);
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Majstor nije pronadjen!"));
    }

    [Test, Order(6)]
    public async Task TestGetKalendar([Values(21, 22, 42)]int id)
    {
        var r = await majstor.GetKalendar(id);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        var obj = JObject.FromObject(rez.Value);
        if (id == 22)
            Assert.Multiple(() =>
            {
                Assert.That(obj["PocetniDatumi"]!.Values<DateTime>(), Is.Empty);
                Assert.That(obj["listaPocetnihDatumaUgovora"]!.Values<DateTime>(), Does.Contain(DateTime.Parse("1.11.2036.")));
            });
        else
            if (id == 21)
                Assert.Multiple(() =>
                {
                    Assert.That(obj["PocetniDatumi"]!.Values<DateTime>(), Does.Contain(DateTime.Parse("6.6.2026.")));
                    Assert.That(obj["listaPocetnihDatumaUgovora"]!.Values<DateTime>(), Does.Contain(DateTime.Parse("1.10.2036.")));
                    Assert.That(obj["listaPocetnihDatumaUgovora"]!.Values<DateTime>(), Does.Contain(DateTime.Parse("1.7.2035.")));
                });
            else
                Assert.Multiple(() =>
                {
                    Assert.That(obj["PocetniDatumi"]!.Values<DateTime>(), Is.Empty);
                    Assert.That(obj["listaPocetnihDatumaUgovora"]!.Values<DateTime>(), Does.Contain(DateTime.Parse("1.10.2036.")));
                    Assert.That(obj["listaPocetnihDatumaUgovora"]!.Values<DateTime>(), Does.Contain(DateTime.Parse("1.7.2035.")));
                    Assert.That(obj["listaPocetnihDatumaUgovora"]!.Values<DateTime>(), Does.Contain(DateTime.Parse("6.6.2026.")));
                });
    }

    [Test, Order(6)]
    public async Task TestGetZahteviGrupaLogin()
    {
        LogoutUser();

        var r = await majstor.GetZahteviGrupa();
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test, Order(6)]
    public async Task TestGetZahteviGrupa()
    {
        await LoginUser("testMajstorP1", "majstor");

        var r = await majstor.GetZahteviGrupa();
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        var obj = JObject.FromObject(rez.Value);
        Assert.Multiple(() =>
        {
            Assert.That(obj["PoslatiZahtevi"], Is.Not.Null);
            Assert.That(obj["PrimljeniZahtevi"], Is.Not.Null);
        });
        var list1 = JArray.FromObject(obj["PoslatiZahtevi"]);
        var list2 = JArray.FromObject(obj["PrimljeniZahtevi"]);
        Assert.Multiple(() =>
        {
            Assert.That(list1, Has.Count.EqualTo(1));
            Assert.That(list2, Has.Count.EqualTo(1));
        });
    }

    [Test, Order(6)]
    public async Task TestGetClanoviGrupa([Values(0, 1, 21, 100)]int id)
    {
        var r = await majstor.GetClanovi(id);
        if (id != 21)
            Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        else
        {
            Assert.That(r, Is.TypeOf<OkObjectResult>());
            OkObjectResult rez = (OkObjectResult)r;
            Assert.That(rez.Value, Is.Empty);
        }
    }

    [Test, Order(6)]
    public async Task TestGetClanovi()
    {
        var r = await majstor.GetClanovi(42);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        var list = JArray.FromObject(rez.Value);
        List<int> l = [17, 21, 25, 29];
        foreach (var item in list)
        {
            if(l.Contains(item.Value<int>("ID")))
                l.Remove(item.Value<int>("ID"));
        }
        Assert.That(l, Is.Empty);
    }



    //NAPRAVI ZAHTEV GRUPA

    [Test, Order(6)]
    public async Task TestNapraviZahtevGrupaLogin()
    {
        LogoutUser();

        var r = await majstor.NapraviZahtevGrupa("testZG", 7);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Greska prilikom provere identiteta majstora!"));
    }

    [Test, Order(6)]
    public async Task TestNapraviZahtevGrupaGrupa()
    {
        await LoginUser("testMajstor6", "majstor");

        var r = await majstor.NapraviZahtevGrupa("testZG", 7);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Mozete biti clan samo jedne grupe!"));

        LogoutUser();
    }

    [Test, Order(6)]
    public async Task TestNapraviZahtevGrupaPrimalac([Values(0, 100)]int id)
    {
        await LoginUser("testMajstor3", "majstor");

        var r = await majstor.NapraviZahtevGrupa("testZG", id);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Problem sa profilom majstora kome želite poslati zahtev!"));

        LogoutUser();
    }

    [Test, Order(7)]
    public async Task TestNapraviZahtevGrupa()
    {
        await LoginUser("testMajstor3", "majstor");

        var r = await majstor.NapraviZahtevGrupa("testZG", 22);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);

        OkObjectResult r1 = (OkObjectResult)await majstor.GetZahteviGrupa();
        var obj = JObject.FromObject(r1.Value!);
        var list = JArray.FromObject(obj["PoslatiZahtevi"]!);
        bool f = false;
        foreach (var item in list)
        {
            if (item.Value<int>("ID") == (int)rez.Value)
                f = true;
        }
        Assert.That(f);

        await LoginUser("testMajstor7", "majstor");

        r1 = (OkObjectResult)await majstor.GetZahteviGrupa();
        obj = JObject.FromObject(r1.Value!);
        list = JArray.FromObject(obj["PrimljeniZahtevi"]!);
        f = false;
        foreach (var item in list)
        {
            if (item.Value<int>("ID") == (int)rez.Value)
                f = true;
        }
        Assert.That(f);

        LogoutUser();
    }



    //POVUCI ZAHTEV GRUPA

    [Test, Order(8)]
    public async Task TestPovuciZahtevGrupaLogin()
    {
        LogoutUser();

        var r = await majstor.PovuciZahtevGrupa(14);
        Assert.That(r, Is.TypeOf<UnauthorizedResult>());
    }

    [Test, Order(8)]
    public async Task TestPovuciZahtevGrupaZahtev([Values(0, 5, 30)]int id)
    {
        await LoginUser("testMajstor3", "majstor");

        var r = await majstor.PovuciZahtevGrupa(id);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Zahtev za grupu nije pronađen ili ne pripada ovom majstoru."));

        LogoutUser();
    }

    [Test, Order(9)]
    public async Task TestPovuciZahtevGrupa()
    {
        await LoginUser("testMajstor3", "majstor");

        var r = await majstor.PovuciZahtevGrupa(19);
        Assert.That(r, Is.TypeOf<OkObjectResult>());

        OkObjectResult r1 = (OkObjectResult)await majstor.GetZahteviGrupa();
        var obj = JObject.FromObject(r1.Value!);
        var list = JArray.FromObject(obj["PoslatiZahtevi"]!);
        bool f = false;
        foreach (var item in list)
        {
            if (item.Value<int>("ID") == 19)
                f = true;
        }
        Assert.That(!f);

        await LoginUser("testMajstor7", "majstor");

        r1 = (OkObjectResult)await majstor.GetZahteviGrupa();
        obj = JObject.FromObject(r1.Value!);
        list = JArray.FromObject(obj["PrimljeniZahtevi"]!);
        f = false;
        foreach (var item in list)
        {
            if (item.Value<int>("ID") == 19)
                f = true;
        }
        Assert.That(!f);

        LogoutUser();
    }



    //ODGOVOR ZAHTEV GRUPA

    [Test, Order(10)]
    public async Task TestOdgovorZahtevGrupaLogin()
    {
        LogoutUser();

        var r = await majstor.OdgovorZahtevGrupa(5, 0);
        Assert.That(r, Is.TypeOf<UnauthorizedResult>());
    }

    [Test, Order(10)]
    public async Task TestOdgovorZahtevGrupaGrupa()
    {
        await LoginUser("testMajstor10", "majstor");

        var r = await majstor.OdgovorZahtevGrupa(7, 0);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Vec je clan neke druge grupe!"));
        
        LogoutUser();
    }

    [Test, Order(10)]
    public async Task TestOdgovorZahtevGrupaZahtev([Values(0, 1, 30)]int id)
    {
        await LoginUser("testMajstor13", "majstor");

        var r = await majstor.OdgovorZahtevGrupa(id, 0);
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        if (id == 1)
            Assert.That(rez.Value, Is.EqualTo("Zahtev ne pripada njemu!"));
        else
            Assert.That(rez.Value, Is.EqualTo("Nema tog zahteva"));

        LogoutUser();
    }

    [Test, Order(10)]
    public async Task TestOdgovorZahtevGrupaOdgovor([Values(-1, 2)] int odg)
    {
        await LoginUser("testMajstor13", "majstor");

        var r = await majstor.OdgovorZahtevGrupa(6, odg);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Uneli ste nevalidan podatak za odgovor: unesite ili '0' ili '1'."));

        LogoutUser();
    }

    [Test, Order(10)]
    public async Task TestOdgovorZahtevGrupaPosiljalac()
    {
        await LoginUser("testMajstor13", "majstor");

        var r = await majstor.OdgovorZahtevGrupa(2, 1);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Majstor posiljalac je postao clan druge grupe!"));

        LogoutUser();
    }

    [Test, Order(11)]
    public async Task TestOdgovorZahtevGrupa([Values(6, 10, 18)] int id)
    {
        await LoginUser("testMajstor13", "majstor");

        var r = await majstor.OdgovorZahtevGrupa(id, id == 6 ? 0 : 1);
        Assert.That(r, Is.TypeOf<OkObjectResult>());

        switch(id)
        {
            case 6:
            case 10:
                OkObjectResult r1 = (OkObjectResult)await majstor.GetZahteviGrupa();
                var obj = JObject.FromObject(r1.Value!);
                var list = JArray.FromObject(obj["PrimljeniZahtevi"]!);
                bool f = false;
                foreach (var item in list)
                {
                    if (item.Value<int>("ID") == id)
                    {
                        if (id == 6)
                            f = true;
                        else
                            Assert.That(item.Value<int>("Prihvacen"), Is.EqualTo(1));
                    }
                }
                if (id == 6)
                {
                    Assert.That(!f);
                    await LoginUser("testMajstorP1", "majstor");
                }
                else
                    await LoginUser("testMajstorP2", "majstor");


                r1 = (OkObjectResult)await majstor.GetZahteviGrupa();
                obj = JObject.FromObject(r1.Value!);
                list = JArray.FromObject(obj["PoslatiZahtevi"]!);
                f = false;
                foreach (var item in list)
                {
                    if (item.Value<int>("ID") == id)
                    {
                        if (id == 6)
                            f = true;
                        else
                            Assert.That(item.Value<int>("Prihvacen"), Is.EqualTo(1));
                    }
                }
                if (id == 6)
                    Assert.That(!f);
                break;
            case 18:
                OkObjectResult r2 = (OkObjectResult)await profil.PodaciRegistracije();
                var obj2 = JObject.FromObject(r2.Value!);
                Assert.That(obj2.Value<int>("GrupaID"), Is.EqualTo(42));

                r2 = (OkObjectResult)await majstor.GetClanovi(42);
                var list2 = JArray.FromObject(r2.Value!);
                bool f2 = false;
                foreach (var item in list2)
                {
                    if (item.Value<int>("ID") == 28)
                        f2 = true;
                }
                Assert.That(f2);
                break;
        }

        LogoutUser();
    }



    //IZBACI IZ GRUPE

    [Test, Order(12)]
    public async Task TestIzbaciIzGrupeLogin()
    {
        LogoutUser();

        var r = await majstor.IzbaciIzGrupe(28);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Nije pronađena grupa"));
    }

    [Test, Order(12)]
    public async Task TestIzbaciIzGrupeMajstor([Values(0, 1, 17, 26, 100)]int id)
    {
        await LoginUser("testGrupa2", "majstor");

        var r = await majstor.IzbaciIzGrupe(id);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        string msg = id switch
        {
            0 => "Pogrešan id",
            1 or 100 => "Nije nađen majstor",
            17 => "vođa ne može biti izbačen iz grupe",
            _ => "Majstor nije clan grupe",
        };
        Assert.That(rez.Value, Is.EqualTo(msg));

        LogoutUser();
    }

    [Test, Order(13)]
    public async Task TestIzbaciIzgrupe()
    {
        await LoginUser("testGrupa2", "majstor");

        var r = await majstor.IzbaciIzGrupe(28);
        Assert.That(r, Is.TypeOf<OkObjectResult>());

        OkObjectResult r1 = (OkObjectResult)await profil.VratiKorisnika(28);
        var obj = JObject.FromObject(r1.Value!);
        Assert.That(obj.Value<int>("GrupaID"), Is.EqualTo(0));

        r1 = (OkObjectResult)await majstor.GetClanovi(42);
        var list = JArray.FromObject(r1.Value!);
        bool f = false;
        foreach (var item in list)
        {
            if (item.Value<int>("ID") == 28)
                f = true;
        }
        Assert.That(!f);


        LogoutUser();
    }





    [Test, Order(14)]
    public async Task TestUpisiKalendarLogin()
    {
        LogoutUser();

        KalendarDTO k = new()
        {
            PocetniDatumi = [DateTime.Parse("1.1.2028.")],
            KrajnjiDatumi = [DateTime.Parse("3.1.2028.")]
        };

        var r = await majstor.UpisiKalendar(k);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("majstor nije pronadjen"));
    }

    [Test, Order(14)]
    public async Task TestUpisiKalendarDatumi()
    {
        await LoginUser("testMajstor3", "majstor");

        KalendarDTO k = new()
        {
            PocetniDatumi = [DateTime.Parse("1.1.2028."), DateTime.Parse("2.1.2028.")],
            KrajnjiDatumi = [DateTime.Parse("3.1.2028."), DateTime.Parse("4.1.2028.")]
        };

        var r = await majstor.UpisiKalendar(k);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("dani se preklapaju"));
        
        LogoutUser();
    }

    [Test, Order(15)]
    public async Task TestUpisiKalendar()
    {
        await LoginUser("testMajstor3", "majstor");

        KalendarDTO k = new()
        {
            PocetniDatumi = [DateTime.Parse("1.1.2028.")],
            KrajnjiDatumi = [DateTime.Parse("3.1.2028.")]
        };

        var r = await majstor.UpisiKalendar(k);
        Assert.That(r, Is.TypeOf<OkObjectResult>());

        OkObjectResult rez = (OkObjectResult)await majstor.GetKalendar(18);
        Assert.That(rez.Value, Is.Not.Null);
        var obj = JObject.FromObject(rez.Value);
        Assert.Multiple(() =>
        {
            Assert.That(obj["PocetniDatumi"]!.Values<DateTime>(), Does.Contain(DateTime.Parse("1.1.2028.")));
            Assert.That(obj["KrajnjiDatumi"]!.Values<DateTime>(), Does.Contain(DateTime.Parse("3.1.2028.")));
        });

        LogoutUser();
    }

    [Test, Order(15)]
    public async Task TestOdgovorZahtevPosaoZahtev([Values(0, 200, 300)]int id)
    {
        var r = await majstor.OdgovorZahtevPosao(id, id == 3 ? "da" : "ne");
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo(id == 0 ? "Pogrešan id" : "nije nadjen zahtev za posao"));
    }

    [Test, Order(15)]
    public async Task TestOdgovorZahtevPosaoOdgovor([Values("", "DA")] string odg)
    {
        var r = await majstor.OdgovorZahtevPosao(2, odg);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("odgovor mora biti da ili ne"));
    }

    [Test, Order(16)]
    public async Task TestOdgovorZahtevPosao([Values("ne", "da")] string odg)
    {
        await LoginUser("testMajstor2", "majstor");
        
        var r = await majstor.OdgovorZahtevPosao(odg.Equals("ne") ? 2 : 14, odg);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        if (odg.Equals("ne"))
        {
            OkObjectResult rez = (OkObjectResult)await korisnik.GetZahteviPosaoMajstorGrupa();
            var list = JArray.FromObject(rez.Value!);
            bool f = false;
            foreach (var item in list)
            {
                if (item.Value<int>("ID") == 2)
                    f = true;
            }
            Assert.That(!f);

            await LoginUser("testPoslodavac6", "poslodavac");
            rez = (OkObjectResult)await korisnik.GetZahteviPosaoMajstorGrupa();
            list = JArray.FromObject(rez.Value!);
            f = false;
            foreach (var item in list)
            {
                if (item.Value<int>("ID") == 2)
                    f = true;
            }
            Assert.That(!f);
        }
        else
        {
            OkObjectResult ru = (OkObjectResult)r;
            Assert.That(ru.Value, Is.Not.Null);

            OkObjectResult rez = (OkObjectResult)await korisnik.GetUgovori();
            var list = JArray.FromObject(rez.Value!);
            bool f = false;
            foreach (var item in list)
            {
                if (item.Value<int>("ID") == (int)ru.Value)
                    f = true;
            }
            Assert.That(f);

            await LoginUser("testPoslodavac7", "poslodavac");
            rez = (OkObjectResult)await korisnik.GetUgovori();
            list = JArray.FromObject(rez.Value!);
            f = false;
            foreach (var item in list)
            {
                if (item.Value<int>("ID") == (int)ru.Value)
                    f = true;
            }
            Assert.That(f);
        }

        LogoutUser();
    }



    //IZLAZ IZ GRUPE

    [Test, Order(17)]
    public async Task TestIzlazIzGrupeLogin([Values(2, 3, 4)]int id)
    {
        switch(id)
        {
            case 2:
                await LoginUser("testGrupa2", "majstor");
                break;
            case 3:
                await LoginUser("testMajstor3", "majstor");
                break;
            case 4:
                await LoginUser("testPoslodavac4", "poslodavac");
                break;
        }

        var r = await majstor.IzlazIzGrupe();
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Majstor ne postoji ili nije pripadnik nijedne grupe!"));

        LogoutUser();
    }

    [Test, Order(17)]
    public async Task TestIzlazIzGrupeUgovor()
    {
        await LoginUser("testMajstor14", "majstor");

        var r = await majstor.IzlazIzGrupe();
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Ne možete napustiti grupu jer imate aktivne ugovore!"));

        await LoginUser("testPoslodavac5", "poslodavac");
        await poslodavac.ZavrsiPosao(1, 73);

        LogoutUser();
    }

    [Test, Order(18)]
    public async Task TestIzlazIzGrupe1()
    {
        await LoginUser("testMajstor14", "majstor");

        var r = await majstor.IzlazIzGrupe();
        Assert.That(r, Is.TypeOf<OkObjectResult>());

        OkObjectResult r1 = (OkObjectResult)await profil.PodaciRegistracije();
        var obj = JObject.FromObject(r1.Value!);
        Assert.That(obj.Value<int>("GrupaID"), Is.EqualTo(0));

        r1 = (OkObjectResult)await majstor.GetClanovi(42);
        var list = JArray.FromObject(r1.Value!);
        bool f = false;
        foreach (var item in list)
        {
            if (item.Value<int>("ID") == 28)
                f = true;
        }
        Assert.That(!f);


        LogoutUser();
    }

    [Test, Order(19)]
    public async Task TestIzlazIzGrupe2()
    {
        await LoginUser("testMajstor2", "majstor");

        var r = await majstor.IzlazIzGrupe();
        Assert.That(r, Is.TypeOf<OkObjectResult>());

        for(int i = 17; i <= 29; i += 4)
        {
            OkObjectResult r1 = (OkObjectResult)await profil.VratiKorisnika(i);
            var obj = JObject.FromObject(r1.Value!);
            Assert.That(obj.Value<int>("GrupaID"), Is.EqualTo(0));
        }

        var user = await profil.VratiKorisnika(42);
        Assert.That(user, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)user;
        Assert.That(rez.Value, Is.EqualTo("Korisnik sa tim ID-jem nije pronadjen u bazi!"));

        LogoutUser();
	}





	[OneTimeTearDown]
	public async Task TearDown()
	{
		await context.DisposeAsync();
	}
}