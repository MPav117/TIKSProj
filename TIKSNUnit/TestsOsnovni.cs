using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebTemplate.Controllers;
using WebTemplate.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace TIKSNUnit;

[TestFixture, Order(0)]
public class TestsOsnovni
{
    private ZanatstvoContext context;
    private OsnovniController osnovni;
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
    }



    [Test]
    public async Task TestPromeniJezik()
    {
        string rezSr = await osnovni.PromeniJezik("sr");
        string rezEn = await osnovni.PromeniJezik("en");
        string rezErr = await osnovni.PromeniJezik("err");
        string rezNull = await osnovni.PromeniJezik("");

        Assert.That(rezSr.Contains("\"id\": \"sr\"") && rezEn.Contains("\"id\": \"en\"") 
            && rezErr.Equals("mora biti en ili sr") && rezNull.Equals("mora biti en ili sr"));
    }


    
    //PREGLED MAJSTORA
    
    [Test]
    public async Task TestPregledMajstoraMinOcena1()
    {
        var minO = await osnovni.PregledMajstora((float)4.5, null, -1, "ocena", "null", 1);
        Assert.That(minO, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezMinO = (OkObjectResult)minO;
        Assert.That(rezMinO.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezMinO.Value);
        Assert.Multiple(() =>
        {
            Assert.That(rows["lista"], Is.Empty);
            Assert.That(rows.Value<bool>("kraj"), Is.EqualTo(true));
        });
    }

    [Test]
    public async Task TestPregledMajstoraMinOcena2()
    {
        var minO = await osnovni.PregledMajstora((float)1.5, null, -1, "ocena", "null", 1);
        Assert.That(minO, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezMinO = (OkObjectResult)minO;
        Assert.That(rezMinO.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezMinO.Value);
        Assert.That(rows["lista"], Is.Not.Empty);
        var list = JArray.FromObject(rows["lista"]);
        foreach (var item in list)
        {
            Assert.That(item.Value<float>("ProsecnaOcena"), Is.GreaterThanOrEqualTo(2));
        }
    }

    [Test]
    public async Task TestPregledMajstoraVestine([Values("stolar", "none")]string skill)
    {
        var vestine = await osnovni.PregledMajstora(-1, [skill], -1, "ocena", "null", 1);
        Assert.That(vestine, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezVestine = (OkObjectResult)vestine;
        Assert.That(rezVestine.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezVestine.Value);
        if (skill.Equals("none"))
            Assert.That(rows["lista"], Is.Empty);
        else
        {
            Assert.That(rows["lista"], Is.Not.Empty);
            var list = JArray.FromObject(rows["lista"]);
            foreach (var item in list)
            {
                Assert.That(item["ListaVestina"], Is.Not.Empty);
                Assert.That(item["ListaVestina"].First, Is.Not.Null);
                Assert.That(item["ListaVestina"].First.Value<string>(), Is.EqualTo(skill));
            }
        }
    }

    [Test]
    public async Task TestPregledMajstoraGrad1()
    {
        var grad = await osnovni.PregledMajstora(-1, null, 1, "ocena", "null", 1);
        Assert.That(grad, Is.TypeOf<OkObjectResult>());
        ObjectResult rezGrad = (ObjectResult)grad;
        Assert.That(rezGrad.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezGrad.Value);
        Assert.That(rows["lista"], Is.Not.Empty);
        var list = JArray.FromObject(rows["lista"]);
        foreach (var item in list)
        {
            Assert.That(item["Grad"], Is.Not.Empty);
            Assert.That(item["Grad"].First, Is.Not.Empty);
            Assert.That(item["Grad"].First.First, Is.Not.Null);
            Assert.That(item["Grad"].First.First.Value<int>(), Is.EqualTo(1));
        }
    }

    [Test]
    public async Task TestPregledMajstoraGrad2()
    {
        var grad = await osnovni.PregledMajstora(-1, null, 50000, "ocena", "null", 1);
        Assert.That(grad, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezGrad = (OkObjectResult)grad;
        Assert.That(rezGrad.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezGrad.Value);
        Assert.Multiple(() =>
        {
            Assert.That(rows["lista"], Is.Empty);
            Assert.That(rows.Value<bool>("kraj"), Is.EqualTo(true));
        });
    }

    [Test]
    public async Task TestPregledMajstoraOcena()
    {
        var ocena = await osnovni.PregledMajstora(1, null, -1, "ocena", "null", 1);
        Assert.That(ocena, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezOcena = (OkObjectResult)ocena;
        Assert.That(rezOcena.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezOcena.Value);
        Assert.That(rows["lista"], Is.Not.Empty);
        var list = JArray.FromObject(rows["lista"]);
        Assert.Multiple(() =>
        {
            Assert.That(list.First, Is.Not.Null);
            Assert.That(list.Last, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(list.First.Value<float>("ProsecnaOcena"), Is.EqualTo(3));
            Assert.That(list.Last.Value<float>("ProsecnaOcena"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task TestPregledMajstoraSortError([Values("err", "")]string sort)
    {
        var err = await osnovni.PregledMajstora(1, null, -1, sort, "null", 1);
        Assert.That(err, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rezErr = (BadRequestObjectResult)err;
        Assert.That(rezErr.Value, Is.EqualTo("Neispravan parametar sortiranja!"));
    }

    [Test]
    public async Task TestPregledMajstoraNaziv1([Values("testMajstor1", "tEsTmAjStOrP", "")]string name)
    {
        var name1 = await osnovni.PregledMajstora(-1, null, -1, "ocena", name, 1);
        Assert.That(name1, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezName1 = (OkObjectResult)name1;
        Assert.That(rezName1.Value, Is.Not.Null);
        var rows1 = JObject.FromObject(rezName1.Value);
        Assert.That(rows1["lista"], Is.Not.Empty);
        var list1 = JArray.FromObject(rows1["lista"]);
        foreach (var item in list1)
        {
            Assert.That(item.Value<string>("Naziv"), Does.Contain(name).IgnoreCase);
        }
    }

    [Test]
    public async Task TestPregledMajstoraNazivEmpty()
    {
        var empty = await osnovni.PregledMajstora(-1, null, -1, "ocena", "empty", 1);
        Assert.That(empty, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezEmpty = (OkObjectResult)empty;
        Assert.That(rezEmpty.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezEmpty.Value);
        Assert.Multiple(() =>
        {
            Assert.That(rows["lista"], Is.Empty);
            Assert.That(rows.Value<bool>("kraj"), Is.EqualTo(true));
        });
    }

    [Test]
    public async Task TestPregledMajstoraStranica([Values(1, 3)]int page)
    {
        var page1 = await osnovni.PregledMajstora(-1, null, -1, "ocena", "null", page);
        Assert.That(page1, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezPage1 = (OkObjectResult)page1;
        Assert.That(rezPage1.Value, Is.Not.Null);
        var rows1 = JObject.FromObject(rezPage1.Value);
        Assert.Multiple(() =>
        {
            Assert.That(rows1["lista"], Is.Not.Empty);
            Assert.That(rows1.Value<bool>("kraj"), Is.EqualTo(page == 3));
        });
        var list1 = JArray.FromObject(rows1["lista"]);
        Assert.That(page == 1 ? list1.First : list1.Last, Is.Not.Null);
        if (page == 1)
        {
            Assert.That(list1.First!.Value<float>("ProsecnaOcena"), Is.EqualTo(3));
        }
        else
        {
            Assert.That(list1.Last!.Value<float?>("ProsecnaOcena"), Is.EqualTo(null));
        }
    }

    [Test]
    public async Task TestPregledMajstoraStranicaErr([Values(0, 50)] int page)
    {
        var err = await osnovni.PregledMajstora(-1, null, -1, "ocena", "null", page);
        Assert.That(err, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rezErr = (BadRequestObjectResult)err;
        Assert.That(rezErr.Value, Is.EqualTo(page == 0 ? "stranica mora da bude broj veci od 0"
            : "Ne postoji ta stranica!"));
    }
    


    //PREGLED OGLASA

    [Test]
    public async Task TestPregledOglasaMinCena1()
    {
        var minC = await osnovni.PregledOglasa(40, -1, -1, "Cena po satu", "null", null, 1);
        Assert.That(minC, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezMinC = (OkObjectResult)minC;
        Assert.That(rezMinC.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezMinC.Value);
        Assert.Multiple(() =>
        {
            Assert.That(rows["lista"], Is.Empty);
            Assert.That(rows.Value<bool>("kraj"), Is.EqualTo(true));
        });
    }

    [Test]
    public async Task TestPregledOglasaMinCena2()
    {
        var minC = await osnovni.PregledOglasa(20, -1, -1, "Cena po satu", "null", null, 1);
        Assert.That(minC, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezMinC = (OkObjectResult)minC;
        Assert.That(rezMinC.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezMinC.Value);
        Assert.That(rows["lista"], Is.Not.Empty);
        var list = JArray.FromObject(rows["lista"]);
        foreach (var item in list)
        {
            Assert.That(item.Value<float>("CenaPoSatu"), Is.GreaterThanOrEqualTo(20));
        }
    }

    [Test]
    public async Task TestPregledOglasaMinOcena1()
    {
        var minO = await osnovni.PregledOglasa(-1, 5, -1, "Cena po satu", "null", null, 1);
        Assert.That(minO, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezMinO = (OkObjectResult)minO;
        Assert.That(rezMinO.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezMinO.Value);
        Assert.Multiple(() =>
        {
            Assert.That(rows["lista"], Is.Empty);
            Assert.That(rows.Value<bool>("kraj"), Is.EqualTo(true));
        });
    }

    [Test]
    public async Task TestPregledOglasaMinOcena2([Values(3, 1)]float grade)
    {
        var minO = await osnovni.PregledOglasa(-1, grade, -1, "Cena po satu", "null", null, 1);
        Assert.That(minO, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezMinO = (OkObjectResult)minO;
        Assert.That(rezMinO.Value, Is.Not.Null);
        var rows1 = JObject.FromObject(rezMinO.Value);
        Assert.That(rows1["lista"], Is.Not.Empty);
        var list1 = JArray.FromObject(rows1["lista"]);
        Assert.That(list1, Has.Count.EqualTo(grade == 3 ? 8 : 10));
    }

    [Test]
    public async Task TestPregledOglasaGrad1()
    {
        var grad = await osnovni.PregledOglasa(-1, -1, 1, "Cena po satu", "null", null, 1);
        Assert.That(grad, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezGrad = (OkObjectResult)grad;
        Assert.That(rezGrad.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezGrad.Value);
        Assert.That(rows["lista"], Is.Not.Empty);
        var list = JArray.FromObject(rows["lista"]);
        Assert.That(list, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task TestPregledOglasaGrad2()
    {
        var grad = await osnovni.PregledOglasa(-1, -1, 50000, "Cena po satu", "null", null, 1);
        Assert.That(grad, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezGrad = (OkObjectResult)grad;
        Assert.That(rezGrad.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezGrad.Value);
        Assert.Multiple(() =>
        {
            Assert.That(rows["lista"], Is.Empty);
            Assert.That(rows.Value<bool>("kraj"), Is.EqualTo(true));
        });
    }

    [Test]
    public async Task TestPregledOglasaMinCenaPoSatu()
    {
        var cena = await osnovni.PregledOglasa(-1, -1, -1, "Cena po satu", "null", null, 1);
        Assert.That(cena, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezCena = (OkObjectResult)cena;
        Assert.That(rezCena.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezCena.Value);
        Assert.That(rows["lista"], Is.Not.Empty);
        var list = JArray.FromObject(rows["lista"]);
        Assert.Multiple(() =>
        {
            Assert.That(list.First, Is.Not.Null);
            Assert.That(list.Last, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(list.First.Value<float>("CenaPoSatu"), Is.EqualTo(31.0f));
            Assert.That(list.Last.Value<float>("CenaPoSatu"), Is.EqualTo(17.5f));
        });
    }

    [Test]
    public async Task TestPregledOglasaSortError([Values("err", "")]string sort)
    {
        var err = await osnovni.PregledOglasa(-1, -1, -1, sort, "null", null, 1);
        Assert.That(err, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rezErr = (BadRequestObjectResult)err;
        Assert.That(rezErr.Value, Is.EqualTo("Neispravan parametar sortiranja!"));
    }

    [Test]
    public async Task TestPregledOglasaReci1([Values("testPoslodavac1", "tEsTpOsLoDaVac1", "")]string keywords)
    {
        var words = await osnovni.PregledOglasa(-1, -1, -1, "Cena po satu", keywords, null, 1);
        Assert.That(words, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezWords = (OkObjectResult)words;
        Assert.That(rezWords.Value, Is.Not.Null);
        var rows1 = JObject.FromObject(rezWords.Value);
        Assert.That(rows1["lista"], Is.Not.Empty);
        var list1 = JArray.FromObject(rows1["lista"]);
        foreach (var item in list1)
        {
            string? naslov = item.Value<string>("Naslov");
            string? opis = item.Value<string>("Opis");
            Assert.Multiple(() =>
            {
                Assert.That(naslov, Is.Not.Null);
                Assert.That(opis, Is.Not.Null);
            });
            Assert.That(naslov.Contains(keywords, StringComparison.CurrentCultureIgnoreCase)
                || opis.Contains(keywords, StringComparison.CurrentCultureIgnoreCase));
        }
    }

    [Test]
    public async Task TestPregledOglasaReciEmpty()
    {
        var words = await osnovni.PregledOglasa(-1, -1, -1, "Cena po satu", "empty", null, 1);
        Assert.That(words, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezWords = (OkObjectResult)words;
        Assert.That(rezWords.Value, Is.Not.Null);
        var rows1 = JObject.FromObject(rezWords.Value);
        Assert.Multiple(() =>
        {
            Assert.That(rows1["lista"], Is.Empty);
            Assert.That(rows1.Value<bool>("kraj"), Is.EqualTo(true));
        });
    }

    [Test]
    public async Task TestPregledOglasaVestine([Values("stolar", "none")] string skill)
    {
        var vestine = await osnovni.PregledOglasa(-1, -1, -1, "Cena po satu", "null", [skill], 1);
        Assert.That(vestine, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezVestine = (OkObjectResult)vestine;
        Assert.That(rezVestine.Value, Is.Not.Null);
        var rows = JObject.FromObject(rezVestine.Value);
        if (skill.Equals("none"))
            Assert.That(rows["lista"], Is.Empty);
        else
        {
            Assert.That(rows["lista"], Is.Not.Empty);
            var list = JArray.FromObject(rows["lista"]);
            foreach (var item in list)
            {
                Assert.That(item["ListaVestina"], Is.Not.Empty);
                Assert.That(item["ListaVestina"].First, Is.Not.Null);
                Assert.That(item["ListaVestina"].First.Value<string>(), Is.EqualTo(skill));
            }
        }
    }

    [Test]
    public async Task TestPregledOglasaStranica([Values(1, 2)]int page)
    {
        var page1 = await osnovni.PregledOglasa(-1, -1, -1, "Cena po satu", "null", null, page);
        Assert.That(page1, Is.TypeOf<OkObjectResult>());
        OkObjectResult rezPage1 = (OkObjectResult)page1;
        Assert.That(rezPage1.Value, Is.Not.Null);
        var rows1 = JObject.FromObject(rezPage1.Value);
        Assert.Multiple(() =>
        {
            Assert.That(rows1["lista"], Is.Not.Empty);
            Assert.That(rows1.Value<bool>("kraj"), Is.EqualTo(page == 2));
        });
        var list1 = JArray.FromObject(rows1["lista"]);
        Assert.That(page == 1 ? list1.First : list1.Last, Is.Not.Null);
        if (page == 1)
        {
            Assert.That(list1.First!.Value<float>("CenaPoSatu"), Is.EqualTo(31));
        }
        else
        {
            Assert.That(list1.Last!.Value<float?>("CenaPoSatu"), Is.EqualTo(10));
        }
    }

    [Test]
    public async Task TestPregledOglasaStranicaErr([Values(0, 50)] int page)
    {
        var err = await osnovni.PregledMajstora(-1, null, -1, "ocena", "null", page);
        Assert.That(err, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult rezErr = (BadRequestObjectResult)err;
        Assert.That(rezErr.Value, Is.EqualTo(page == 0 ? "stranica mora da bude broj veci od 0"
            : "Ne postoji ta stranica!"));
    }





    [Test]
    public async Task TestGetRecenzije([Values(0, 60, 1, 10)]int id)
    {
        var reviews = await osnovni.GetRecenzije(id);
        if (id == 0 || id == 60)
        {
            Assert.That(reviews, Is.TypeOf<BadRequestObjectResult>());
            BadRequestObjectResult rez = (BadRequestObjectResult)reviews;
            Assert.That(rez.Value, Is.EqualTo(id == 0 ? "Pogresan id" : "Ne postoji takav korisnik"));
        }
        else
        {
            Assert.That(reviews, Is.TypeOf<OkObjectResult>());
            OkObjectResult rez = (OkObjectResult)reviews;
            Assert.That(rez.Value, Is.Not.Null);
            if (id == 1)
            {
                Assert.That(rez.Value, Is.Empty);
            }
            else
            {
                var list = JArray.FromObject(rez.Value);
                Assert.That(list, Has.Count.EqualTo(5));
            }
        }
    }

    [Test]
    public async Task TestGetOglas([Values(0, 1)]int id)
    {
        var ogl = await osnovni.GetOglas(id);
        if (id == 0)
        {
            Assert.That(ogl, Is.TypeOf<BadRequestObjectResult>());
            BadRequestObjectResult rez = (BadRequestObjectResult)ogl;
            Assert.That(rez.Value, Is.EqualTo("Nije nadjen oglas"));
        }
        else
        {
            Assert.That(ogl, Is.TypeOf<OkObjectResult>());
            OkObjectResult rez = (OkObjectResult)ogl;
            Assert.That(rez.Value, Is.Not.Null);
            var rows = JObject.FromObject(rez.Value);
            Assert.That(rows.Value<int>("ID"), Is.EqualTo(id));
        }
    }





    [OneTimeTearDown]
    public async Task TearDown()
    {
        await context.DisposeAsync();
    }
}