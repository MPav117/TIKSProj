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
using Models;

namespace TIKSNUnit;

[TestFixture, Order(1)]
public class TestsProfil
{
    private ZanatstvoContext context;
    private ProfilController profil;
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
        profil = new(context, config, service)
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
            Password = username == "marko" || username == "sanja" ? "password" : username
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
    public async Task TestGetGradovi([Values("nis", "empty")] string name)
    {
        var cities = await profil.GetGradovi(name);
        Assert.That(cities, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)cities;
        Assert.That(rez.Value, Is.Not.Null);
        var list = JArray.FromObject(rez.Value);
        if (name == "empty")
        {
            Assert.That(list, Is.Empty);
        }
        else
        {
            foreach (var item in list)
            {
                Assert.That(item.Value<string>("City_ascii")!.ToLower(), Does.Contain(name.ToLower()));
            }
        }
    }

    [Test, Order(0)]
    public async Task TestVratiKorisnika([Values(0, 1, 16)] int id)
    {
        var user = await profil.VratiKorisnika(id);
        if (id <= 0)
        {
            Assert.That(user, Is.TypeOf<NotFoundObjectResult>());
            NotFoundObjectResult rez = (NotFoundObjectResult)user;
            Assert.That(rez.Value, Is.EqualTo("Korisnik sa tim ID-jem nije pronadjen u bazi!"));
        }
        else if (id <= 15)
        {
            Assert.That(user, Is.TypeOf<OkObjectResult>());
            OkObjectResult rez = (OkObjectResult)user;
            Assert.That(rez.Value, Is.Not.Null);
            var rows = JObject.FromObject(rez.Value);
            Assert.That(rows.Value<string>("Naziv"), Is.EqualTo($"testPoslodavac{id}"));
        }
        else
        {
            Assert.That(user, Is.TypeOf<OkObjectResult>());
            OkObjectResult rez = (OkObjectResult)user;
            Assert.That(rez.Value, Is.Not.Null);
            var rows = JObject.FromObject(rez.Value);
            Assert.That(rows.Value<string>("Naziv"), Is.EqualTo($"testMajstor{id - 15}"));
        }
    }

    [Test, Order(0)]
    public async Task TestPodaciRegistracijeLogin()
    {
        LogoutUser();

        var me = await profil.PodaciRegistracije();
        Assert.That(me, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)me;
        Assert.That(rez.Value, Is.EqualTo("Nemate prava pristupa ovim podacima!"));
    }

    [Test, Order(0)]
    public async Task TestPodaciRegistracije([Values] bool p)
    {
        if (p)
            await LoginUser("testPoslodavac1", "poslodavac");
        else
            await LoginUser("testMajstor1", "majstor");

        var me = await profil.PodaciRegistracije();
        Assert.That(me, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)me;
        Assert.That(rez.Value, Is.Not.Null);
        var rows = JObject.FromObject(rez.Value);
        Assert.That(rows.Value<int>("ID"), Is.EqualTo(p ? 1 : 16));

        LogoutUser();
    }

    [Test, Order(0)]
    public async Task TestLogin([Values("testNone", "marko", "testPoslodavac1")] string username)
    {
        IdentitetDTO i = new()
        {
            Username = username,
            Password = username
        };
        var r = await profil.Login(i);
        if (!username.Equals("testPoslodavac1"))
        {
            Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
            BadRequestObjectResult rez = (BadRequestObjectResult)r;
            if (username.Equals("testNone"))
            {
                Assert.That(rez.Value, Is.EqualTo("User not found!"));
            }
            else
            {
                Assert.That(rez.Value, Is.EqualTo("Wrong password."));
            }
        }
        else
        {
            Assert.That(r, Is.TypeOf<OkObjectResult>());
            OkObjectResult rez = (OkObjectResult)r;
            Assert.That(rez.Value, Is.Not.Empty);
            string jwt = (string)rez.Value;
            JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
            List<Claim> claims = token.Claims.ToList();
            Assert.That(claims, Has.Count.EqualTo(2));
            Assert.That(claims[0].Value, Is.EqualTo(username));
        }
    }

    [Test, Order(1)]
    public async Task TestLogout()
    {
        IdentitetDTO i = new()
        {
            Username = "testPoslodavac1",
            Password = "testPoslodavac1"
        };
        await profil.Login(i);
        profil.Logout();
        Assert.That(service!._httpContextAccessor.HttpContext!.Response.Headers.Cookie, Is.Empty);
    }



    //REGISTER POSLODAVAC

    [Test, Order(2)]
    public async Task TestRegisterPoslodavacUsername()
    {
        PoslodavacDTO p = new()
        {
            Username = "testPoslodavac1",
            Password = "testPoslodavacT1",
            Tip = "poslodavac",
            Naziv = "testPoslodavacT1",
            Opis = "opisPoslodavacT1",
            GradID = 1,
            Adresa = "testPoslodavacT1",
            Email = "testPoslodavacT1@gmail.com",
            Povezani = 0
        };

        var r = await profil.RegisterPoslodavac(p);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Username koji ste uneli vec postoji, unesite drugi username!"));
    }

    [Test, Order(2)]
    public async Task TestRegisterPoslodavacGrad([Values(0, 50000)] int grad)
    {
        PoslodavacDTO p = new()
        {
            Username = "testPoslodavacT1",
            Password = "testPoslodavacT1",
            Tip = "poslodavac",
            Naziv = "testPoslodavacT1",
            Opis = "opisPoslodavacT1",
            GradID = grad,
            Adresa = "testPoslodavacT1",
            Email = "testPoslodavacT1@gmail.com",
            Povezani = 0
        };

        var r = await profil.RegisterPoslodavac(p);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Uneli ste nevazeci grad!"));
    }

    [Test, Order(2)]
    public async Task TestRegisterPoslodavacPovezani([Values(16, 60)] int id)
    {
        PoslodavacDTO p = new()
        {
            Username = "testPoslodavacT1",
            Password = "testPoslodavacT1",
            Tip = "poslodavac",
            Naziv = "testPoslodavacT1",
            Opis = "opisPoslodavacT1",
            GradID = 1,
            Adresa = "testPoslodavacT1",
            Email = "testPoslodavacT1@gmail.com",
            Povezani = id
        };

        var r = await profil.RegisterPoslodavac(p);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo(id == 60 ? "Ne postoji povezani korisnik" : "Korisnik je vec povezan"));
    }
    
    [Test, Order(3)]
    public async Task TestRegisterPoslodavac1()
    {
        PoslodavacDTO p = new()
        {
            Username = "testPoslodavacT1",
            Password = "testPoslodavacT1",
            Tip = "poslodavac",
            Naziv = "testPoslodavacT1",
            Opis = "testPoslodavacT1",
            GradID = 1,
            Adresa = "testPoslodavacT1",
            Email = $"testPoslodavacT1@gmail.com",
            Povezani = 0
        };

        var r = await profil.RegisterPoslodavac(p);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        var rezP = JObject.FromObject(rez.Value);
        Assert.That(rezP.Value<string>("Email"), Is.EqualTo("testPoslodavacT1@gmail.com"));
    }
    
    [Test, Order(4)]
    public async Task TestRegisterPoslodavac2()
    {
        PoslodavacDTO p = new()
        {
            Username = "testPoslodavacT2",
            Password = "testPoslodavacT2",
            Tip = "poslodavac",
            Naziv = "testPoslodavacT2",
            Opis = "testPoslodavacT2",
            GradID = 1,
            Adresa = "testPoslodavacT2",
            Email = $"testPoslodavacT2@gmail.com",
            Povezani = 30
        };

        var r = await profil.RegisterPoslodavac(p);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        var rezP = JObject.FromObject(rez.Value);
        OkObjectResult user = (OkObjectResult)await profil.VratiKorisnika(30);
        var usr = JObject.FromObject(user.Value!);
        Assert.Multiple(() =>
        {
            Assert.That(rezP.Value<string>("Email"), Is.EqualTo("testPoslodavacT2@gmail.com"));
            Assert.That(usr.Value<int>("Povezani"), Is.EqualTo(48));
        });
    }



    //REGISTER MAJSTOR

    [Test, Order(5)]
    public async Task TestRegisterMajstorUsername()
    {
        MajstorDTO m = new()
        {
            Username = "testMajstor1",
            Password = "testMajstorT1",
            Tip = "majstor",
            Naziv = "testMajstorT1",
            Opis = "testMajstorT1",
            GradID = 1,
            Email = "testMajstorT1@gmail.com",
            Povezani = 0,
            TipMajstora = "majstor",
            ListaVestina = ["stolar"]
        };

        var r = await profil.RegisterMajstor(m);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Username koji ste uneli vec postoji, unesite drugi username!"));
    }

    [Test, Order(5)]
    public async Task TestRegisterMajstorGrad([Values(0, 50000)] int grad)
    {
        MajstorDTO m = new()
        {
            Username = "testMajstorT1",
            Password = "testMajstorT1",
            Tip = "majstor",
            Naziv = "testMajstorT1",
            Opis = "testMajstorT1",
            GradID = grad,
            Email = "testMajstorT1@gmail.com",
            Povezani = 0,
            TipMajstora = "majstor",
            ListaVestina = ["stolar"]
        };

        var r = await profil.RegisterMajstor(m);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Uneli ste nevazeci grad!"));
    }

    [Test, Order(5)]
    public async Task TestRegisterMajstorPovezani([Values(16, 60)] int id)
    {
        MajstorDTO m = new()
        {
            Username = "testMajstorT1",
            Password = "testMajstorT1",
            Tip = "majstor",
            Naziv = "testMajstorT1",
            Opis = "testMajstorT1",
            GradID = 1,
            Email = "testMajstorT1@gmail.com",
            Povezani = id,
            TipMajstora = "majstor",
            ListaVestina = ["stolar"]
        };

        var r = await profil.RegisterMajstor(m);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo(id == 60 ? "Ne postoji povezani korisnik" : "Korisnik je vec povezan"));
    }
    
    [Test, Order(6)]
    public async Task TestRegisterMajstor1()
    {
        MajstorDTO m = new()
        {
            Username = "testMajstorT1",
            Password = "testMajstorT1",
            Tip = "majstor",
            Naziv = "testMajstorT1",
            Opis = "testMajstorT1",
            GradID = 1,
            Email = "testMajstorT1@gmail.com",
            Povezani = 0,
            TipMajstora = "majstor",
            ListaVestina = ["stolar"]
        };

        var r = await profil.RegisterMajstor(m);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        var rezP = JObject.FromObject(rez.Value);
        Assert.That(rezP.Value<string>("Email"), Is.EqualTo("testMajstorT1@gmail.com"));
    }
    
    [Test, Order(7)]
    public async Task TestRegisterMajstor2()
    {
        MajstorDTO m = new()
        {
            Username = "testMajstorT2",
            Password = "testMajstorT2",
            Tip = "majstor",
            Naziv = "testMajstorT2",
            Opis = "testMajstorT2",
            GradID = 1,
            Email = "testMajstorT2@gmail.com",
            Povezani = 15,
            TipMajstora = "majstor",
            ListaVestina = ["stolar"]
        };

        var r = await profil.RegisterMajstor(m);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        var rezP = JObject.FromObject(rez.Value);
        OkObjectResult user = (OkObjectResult)await profil.VratiKorisnika(15);
        var usr = JObject.FromObject(user.Value!);
        Assert.Multiple(() =>
        {
            Assert.That(rezP.Value<string>("Email"), Is.EqualTo("testMajstorT2@gmail.com"));
            Assert.That(usr.Value<int>("Povezani"), Is.EqualTo(50));
        });
    }



    //REGISTER GRUPA

    [Test, Order(8)]
    public async Task TestRegisterGrupaUsername()
    {
        await LoginUser("testMajstorP1", "majstor");

        MajstorDTO g = new()
        {
            Username = "testGrupa1",
            Password = "testGrupaT1",
            Tip = "majstor",
            Naziv = "testGrupaT1",
            Opis = "testGrupaT1",
            GradID = 1,
            Email = "testGrupaT1@gmail.com",
            Povezani = 0,
            TipMajstora = "grupa",
            ListaVestina = ["stolar"]
        };

        var r = await profil.RegisterGrupaMajstor(g);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Username koji ste uneli vec postoji, unesite drugi username!"));

        LogoutUser();
    }

    [Test, Order(8)]
    public async Task TestRegisterGrupaLogin()
    {
        MajstorDTO g = new()
        {
            Username = "testGrupaT1",
            Password = "testGrupaT1",
            Tip = "majstor",
            Naziv = "testGrupaT1",
            Opis = "testGrupaT1",
            GradID = 1,
            Email = "testGrupaT1@gmail.com",
            Povezani = 0,
            TipMajstora = "grupa",
            ListaVestina = ["stolar"]
        };

        var r = await profil.RegisterGrupaMajstor(g);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("ne mozes"));
    }

    [Test, Order(8)]
    public async Task TestRegisterGrupaZahtev([Values(4, 8)] int mId)
    {
        await LoginUser($"testMajstor{mId}", "majstor");

        MajstorDTO g = new()
        {
            Username = "testGrupaT1",
            Password = "testGrupaT1",
            Tip = "majstor",
            Naziv = "testGrupaT1",
            Opis = "testGrupaT1",
            GradID = 1,
            Email = "testGrupaT1@gmail.com",
            Povezani = 0,
            TipMajstora = "grupa",
            ListaVestina = ["stolar"]
        };

        var r = await profil.RegisterGrupaMajstor(g);
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Nisu pronadjeni zahtevi za grupu!"));

        LogoutUser();
    }

    [Test, Order(8)]
    public async Task TestRegisterGrupaGrad([Values(0, 50000)] int grad)
    {
        await LoginUser("testMajstor3", "majstor");

        MajstorDTO g = new()
        {
            Username = "testGrupaT1",
            Password = "testGrupaT1",
            Tip = "majstor",
            Naziv = "testGrupaT1",
            Opis = "testGrupaT1",
            GradID = grad,
            Email = "testGrupaT1@gmail.com",
            Povezani = 0,
            TipMajstora = "grupa",
            ListaVestina = ["stolar"]
        };

        var r = await profil.RegisterGrupaMajstor(g);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Uneli ste nevazeci grad!"));

        LogoutUser();
    }
    
    [Test, Order(9)]
    public async Task TestRegisterGrupa()
    {
        await LoginUser("testMajstor3", "majstor");

        MajstorDTO g = new()
        {
            Username = "testGrupaT1",
            Password = "testGrupaT1",
            Tip = "majstor",
            Naziv = "testGrupaT1",
            Opis = "testGrupaT1",
            GradID = 1,
            Email = "testGrupaT1@gmail.com",
            Povezani = 0,
            TipMajstora = "grupa",
            ListaVestina = ["stolar"]
        };

        var r = await profil.RegisterGrupaMajstor(g);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        OkObjectResult rez = (OkObjectResult)r;
        Assert.That(rez.Value, Is.Not.Null);
        Majstor rows = (Majstor)rez.Value;
        Assert.That(rows.Majstori, Is.Not.Empty);
        //var lista = JArray.FromObject(rows["Majstori"]);
        Assert.Multiple(() =>
        {
            Assert.That(rows.Tip, Is.EqualTo("grupa"));
            Assert.That(rows.Majstori, Has.Count.EqualTo(2));
            Assert.That(rows.Majstori.First(), Is.Not.Null);
            Assert.That(rows.Majstori.Last(), Is.Not.Null);
        });
        Majstor first = rows.Majstori.First();
        Majstor last = rows.Majstori.Last();
        Assert.Multiple(() =>
        {
            Assert.That(first.Grupa, Is.Not.Null);
            Assert.That(last.Grupa, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(first.VodjaGrupe, Is.EqualTo(1));
            Assert.That(first.Grupa.ID, Is.EqualTo(28));
            Assert.That(last.VodjaGrupe, Is.EqualTo(0));
            Assert.That(last.Grupa.ID, Is.EqualTo(28));
        });

        LogoutUser();
    }



    //IZBRISI PROFIL

    [Test, Order(10)]
    public async Task TestIzbrisiProfilLogin()
    {
        var r = await profil.IzbrisiProfil();
        Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Korisnik nije pronadjen!"));
    }

    [Test, Order(10)]
    public async Task TestIzbrisiProfilUgovor([Values("Poslodavac", "Majstor")] string role)
    {
        await LoginUser($"test{role}6", role.ToLower());

        var r = await profil.IzbrisiProfil();
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("Ne mozete obrisati profil jer postoji ugovor koji je trenutno aktivan!"));

        LogoutUser();
    }
    
    [Test, Order(11)]
    public async Task TestIzbrisiProfil1([Values("Poslodavac", "Majstor", "Grupa")] string role)
    {
        await LoginUser($"test{role}T1", role.Equals("Grupa") ? "majstor" : role.ToLower());

        var r = await profil.IzbrisiProfil();
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        var rez = await profil.PodaciRegistracije();
        Assert.That(rez, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rn = (NotFoundObjectResult)rez;
        string answer = role.Equals("Grupa") ? "Majstor" : role;
        Assert.That(rn.Value, Is.EqualTo($"{answer} nije pronadjen!"));

        LogoutUser();
    }
    
    [Test, Order(12)]
    public async Task TestIzbrisiProfil2()
    {
        await LoginUser("testMajstor1", "majstor");

        var r = await profil.IzbrisiProfil();
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        var rez1 = await profil.PodaciRegistracije();
        Assert.That(rez1, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rn = (NotFoundObjectResult)rez1;
        Assert.That(rn.Value, Is.EqualTo("Majstor nije pronadjen!"));

        var rez2 = await profil.VratiKorisnika(41);
        Assert.That(rez2, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rg = (NotFoundObjectResult)rez2;
        Assert.That(rg.Value, Is.EqualTo("Korisnik sa tim ID-jem nije pronadjen u bazi!"));

        LogoutUser();
    }



    //ADMIN IZBRISI PROFIL

    [Test, Order(13)]
    public async Task TestAdminIzbrisiProfilLogin([Values]bool l)
    {
        if (l)
            await LoginUser("testPoslodavac1", "poslodavac");

        var r = await profil.IzbrisiProfil(40);
        Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rez = (BadRequestObjectResult)r;
        Assert.That(rez.Value, Is.EqualTo("ne mozes"));

        if (l)
            LogoutUser();
    }

    [Test, Order(13)]
    public async Task TestAdminIzbrisiProfilKorisnik([Values(46, 70)]int id)
    {
        await LoginUser("marko", "poslodavac");

        var r = await profil.IzbrisiProfil(id);
        if (id == 46)
        {
            Assert.That(r, Is.TypeOf<BadRequestObjectResult>());
            BadRequestObjectResult rez = (BadRequestObjectResult)r;
            Assert.That(rez.Value, Is.EqualTo("Ne mozete obrisati drugog administratora!"));
        }
        else
        {
            Assert.That(r, Is.TypeOf<NotFoundObjectResult>());
            NotFoundObjectResult rez = (NotFoundObjectResult)r;
            Assert.That(rez.Value, Is.EqualTo("Korisnik nije pronadjen!"));
        }

        LogoutUser();
    }
    
    [Test, Order(14)]
    public async Task TestAdminIzbrisiProfil1([Values(44, 48, 50)] int id)
    {
        await LoginUser("marko", "poslodavac");

        var r = await profil.IzbrisiProfil(id);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        var rk = await profil.VratiKorisnika(id);
        Assert.That(rk, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rez = (NotFoundObjectResult)rk;
        Assert.That(rez.Value, Is.EqualTo("Korisnik sa tim ID-jem nije pronadjen u bazi!"));

        LogoutUser();
    }
    
    [Test, Order(15)]
    public async Task TestAdminIzbrisiProfil2()
    {
        await LoginUser("marko", "poslodavac");

        var r = await profil.IzbrisiProfil(39);
        Assert.That(r, Is.TypeOf<OkObjectResult>());
        var rez1 = await profil.VratiKorisnika(39);
        Assert.That(rez1, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rn = (NotFoundObjectResult)rez1;
        Assert.That(rn.Value, Is.EqualTo("Korisnik sa tim ID-jem nije pronadjen u bazi!"));

        var rez2 = await profil.VratiKorisnika(43);
        Assert.That(rez2, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult rg = (NotFoundObjectResult)rez2;
        Assert.That(rg.Value, Is.EqualTo("Korisnik sa tim ID-jem nije pronadjen u bazi!"));

        LogoutUser();
    }





    [OneTimeTearDown]
    public async Task TearDown()
    {
        await context.DisposeAsync();
    }
}