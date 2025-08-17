using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using WebTemplate.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Newtonsoft.Json.Linq;

namespace TIKSNUnit;

[SetUpFixture]
public class SetupTests
{
    private readonly List<string> skillList = ["stolar", "elektricar", "vodoinstalater", "keramicar"];

    private ZanatstvoContext context;
    private KorisnikController korisnik;
    private MajstorController majstor;
    private PoslodavacController poslodavac;
    private ProfilController profil;
    private UserService service;

    [OneTimeSetUp]
    public async Task Init()
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
        korisnik = new(context, service)
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
        poslodavac = new(context, service)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            }
        };
        profil = new(context, config, service)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            }
        };

        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(""" 
            BULK INSERT Gradovi
            FROM 'C:\Users\marko\Desktop\prob.csv'
            WITH (
                FIELDTERMINATOR = ';',
                ROWTERMINATOR = '\n',
                FIRSTROW = 2 -- Preskoči prvu red liniju ako sadrži zaglavlje
            );       
            """);

        await InitEmployersAndJobPostings();
        await InitCraftsmenAndApplications();
        await InitConnectedAccounts();
        await InitCraftsmanGroups();
        await InitJobs();
        await AddVacation();
        await CreateAdmin();
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

    private async Task InitEmployersAndJobPostings()
    {
        for (int i = 0; i < 15; i++)
        {
            await CreateEmployerAndJobPosting($"testPoslodavac{i + 1}", skillList[i % 4], 10 + i * 1.5, i + 1);
        }
    }

    private async Task CreateEmployerAndJobPosting(string name, string skill, double rate, int grad)
    {
        PoslodavacDTO p = new()
        {
            Username = name,
            Password = name,
            Tip = "poslodavac",
            Naziv = name,
            Opis = name,
            GradID = grad,
            Adresa = name,
            Email = $"{name}@gmail.com",
            Povezani = 0
        };
        await profil.RegisterPoslodavac(p);

        await LoginUser(name, "poslodavac");

        OglasDTO o = new()
        {
            Naslov = name,
            Opis = name,
            ListaVestina = [skill],
            CenaPoSatu = (float)rate,
            DatumZavrsetka = DateTime.Parse("1.1.2030.")
        };
        await poslodavac.PostaviOglas(o);
    }

    private async Task InitCraftsmenAndApplications()
    {
        for (int i = 0; i < 15; i++)
        {
            await CreateCraftsmanAndApply($"testMajstor{i + 1}", skillList[i % 4], (i % 4) + 1, (i % 4) + 1);
        }
    }

    private async Task CreateCraftsmanAndApply(string name, string skill, int grad, int oglas)
    {
        MajstorDTO m = new()
        {
            Username = name,
            Password = name,
            Tip = "majstor",
            Naziv = name,
            Opis = name,
            GradID = grad,
            Email = $"{name}@gmail.com",
            Povezani = 0,
            TipMajstora = "majstor",
            ListaVestina = [skill]
        };
        await profil.RegisterMajstor(m);

        await LoginUser(name, name);

        await majstor.PrijaviNaOglas(oglas);
    }

    private async Task InitConnectedAccounts()
    {
        for (int i = 0; i < 10; i++)
        {
            await CreateConnectedAccount(i < 5, i < 5 ? i + 1 : i - 4, skillList[Math.Abs(i - 1) % 4]);
        }
    }

    private async Task CreateConnectedAccount(bool C_E, int id, string skill)
    {
        if (C_E)
        {
            PoslodavacDTO p = new()
            {
                Username = $"testPoslodavacM{id}",
                Password = $"testPoslodavacM{id}",
                Tip = "poslodavac",
                Naziv = $"testPoslodavacM{id}",
                Opis = $"testPoslodavacM{id}",
                GradID = ((id - 1) % 4) + 1,
                Adresa = $"testPoslodavacM{id}",
                Email = $"testPoslodavacM{id}@gmail.com",
                Povezani = id + 15
            };
            await profil.RegisterPoslodavac(p);
        }
        else
        {
            MajstorDTO m = new()
            {
                Username = $"testMajstorP{id}",
                Password = $"testMajstorP{id}",
                Tip = "majstor",
                Naziv = $"testMajstorP{id}",
                Opis = $"testMajstorP{id}",
                GradID = id,
                Email = $"testMajstorP{id}@gmail.com",
                Povezani = id,
                TipMajstora = "majstor",
                ListaVestina = [skill]
            };
            await profil.RegisterMajstor(m);
        }
    }

    private async Task InitCraftsmanGroups()
    {
        await SendGroupRequest(24, 36, false, false);
        await SendGroupRequest(21, 28, false, false);
        for (int i = 0; i < 4; i++)
        {
            int j = 4;
            while (i + j < 15)
            {
                if (j == 4 || i == 3)
                    await SendGroupRequest(16 + i, 16 + i + j, i < 3, i < 2);
                else if (i < 2)
                    await SendGroupRequest(41 + i, 16 + i + j, i < 3, false);
                j += 4;
            }
            await SendGroupRequest(36 + i, 28, false, false);
        }
        await SendGroupRequest(39, 36, true, true);
        await SendGroupRequest(40, 37, true, true);
        await SendGroupRequest(42, 28, false, false);
    }

    private async Task SendGroupRequest(int snd, int rcv, bool acc, bool formGroup)
    {
        int sndNum;
        if (snd > 40)
        {
            sndNum = snd - 40;
            await LoginUser($"testGrupa{sndNum}", "majstor");
        }
        else if (snd > 35)
        {
            sndNum = snd - 35;
            await LoginUser($"testMajstorP{sndNum}", "majstor");
        }
        else
        {
            sndNum = snd - 15;
            await LoginUser($"testMajstor{sndNum}", "majstor");
        }
        OkObjectResult rez = (OkObjectResult)await majstor.NapraviZahtevGrupa($"testGrupa{snd}{rcv}", rcv);
        
        if (acc)
        {
            if (rcv > 35)
                await LoginUser($"testMajstorP{rcv - 35}", "majstor");
            else
                await LoginUser($"testMajstor{rcv - 15}", "majstor");
            await majstor.OdgovorZahtevGrupa((int)rez.Value!, 1);

            if (formGroup)
            {
                if (snd > 35)
                    await LoginUser($"testMajstorP{sndNum}", "majstor");
                else
                    await LoginUser($"testMajstor{sndNum}", "majstor");
                MajstorDTO m = new()
                {
                    Username = $"testGrupa{sndNum}",
                    Password = $"testGrupa{sndNum}",
                    Tip = "majstor",
                    Naziv = $"testGrupa{sndNum}",
                    Opis = $"testGrupa{sndNum}",
                    GradID = (snd % 4) + 1,
                    Email = $"{$"testGrupa{sndNum}"}@gmail.com",
                    Povezani = 0,
                    TipMajstora = "grupa",
                    ListaVestina = [skillList[snd % 4]]
                };
                await profil.RegisterGrupaMajstor(m);
            }
        }
    }

    private async Task InitJobs()
    {
        for (int i = 6; i < 16; i++)
        {
            for (int j = 1; j < 11; j++)
            {
                await SendJobRequest(i, 15 + j, 10 + j * 1.5, ((i * i * j) % 5) + 1, j > 3, j > 7, true, j > 4, j > 5);
            }
            await SendJobRequest(2, 11 + i, 15, 4, i < 8, false, false, true, true);
            await SendJobRequest(3, 11 + i, 15, 4, false, false, false, false, false);
        }
        await SendJobRequest(5, 42, 20, 4, true, false, false, true, true);
        await SendJobRequest(6, 42, 20, 4, true, false, false, false, false);
        await SendJobRequest(6, 21, 20, 4, true, false, false, false, false);
        await SendJobRequest(4, 23, 20, 4, true, false, false, false, true);
    }

    private async Task SendJobRequest(int snd, int rcv, double rate, int grade, bool acc, bool done, bool review, bool signm, bool signp)
    {
        ZahtevPosaoDTO z = new()
        {
            KorisnikID = rcv,
            Opis = $"testZahtev-{snd}-{rcv}-",
            CenaPoSatu = (float)rate,
            DatumZavrsetka = DateTime.Parse("1.1.2030."),
            OglasID = snd > 3 ? null : snd
        };
        await LoginUser($"testPoslodavac{snd}", "poslodavac");
        OkObjectResult rez = (OkObjectResult)await poslodavac.NapraviZahtevPosao(z);

        if (acc)
        {
            string name;
            if (rcv < 40)
                name = $"testMajstor{rcv - 15}";
            else
                name = "testGrupa2";
            await LoginUser(name, "majstor");
            OkObjectResult uId = (OkObjectResult)await majstor.OdgovorZahtevPosao((int)rez.Value!, "da");

            UgovorDTO u = new()
            {
                ID = (int)uId.Value!,
                CenaPoSatu = (float)rate,
                DatumPocetka = DateTime.Parse($"1.{(rcv % 12) + 1}.{2030 + snd}."),
                DatumZavrsetka = DateTime.Parse($"2.{(rcv % 12) + 1}.{2030 + snd}."),
                ImeMajstora = name,
                ImePoslodavca = $"testPoslodavac{snd}",
                MajstorID = rcv,
                PoslodavacID = snd,
                Opis = $"testUgovor{snd}{rcv}",
                PotpisMajstora = name,
                PotpisPoslodavca = $"testPoslodavac{snd}",
                ZahtevZaPosaoID = (int)rez.Value!
            };

            if (signm)
            {
                await korisnik.PotpisiUgovor(u);
            }
            if (signp)
            {
                await LoginUser($"testPoslodavac{snd}", "poslodavac");
                await korisnik.PotpisiUgovor(u);
            }
            if (signm && signp)
            {
                if (review)
                {
                    RecenzijaDTO r1 = new()
                    {
                        IdPrimalac = rcv,
                        IdUgovor = (int)uId.Value!,
                        Opis = $"testRecenzija{snd}{rcv}",
                        Ocena = grade
                    };
                    await korisnik.NapraviRecenziju(r1);

                    await LoginUser($"testMajstor{rcv - 15}", "majstor");
                    RecenzijaDTO r2 = new()
                    {
                        IdPrimalac = snd,
                        IdUgovor = (int)uId.Value!,
                        Opis = $"testRecenzija{rcv}{snd}",
                        Ocena = grade
                    };
                    await korisnik.NapraviRecenziju(r2);
                }

                if (done)
                {
                    await LoginUser($"testPoslodavac{snd}", "poslodavac");
                    await poslodavac.ZavrsiPosao(1, (int)uId.Value!);
                }
            }
        }
    }

    private async Task AddVacation()
    {
        await LoginUser("testMajstor6", "majstor");

        KalendarDTO k = new()
        {
            PocetniDatumi = [DateTime.Parse("6.6.2026.")],
            KrajnjiDatumi = [DateTime.Parse("7.7.2026.")]
        };
        await majstor.UpisiKalendar(k);
    }

    private async Task CreateAdmin()
    {
        PoslodavacDTO a1 = new()
        {
            Username = "marko",
            Password = "password",
            Tip = "poslodavac",
            Naziv = "marko",
            Opis = "moji opis",
            GradID = 3553,
            Adresa = "moja adresa",
            Email = $"markopvl@gmail.com",
            Povezani = 0
        };
        await profil.RegisterPoslodavac(a1);

        MajstorDTO a2 = new()
        {
            Username = "sanja",
            Password = "password",
            Tip = "majstor",
            Naziv = "sanja",
            Opis = "moji opis",
            GradID = 3553,
            Email = $"sanja@gmail.com",
            Povezani = 0,
            ListaVestina = ["stolar"],
            TipMajstora = "majstor"
        };
        await profil.RegisterMajstor(a2);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await context.DisposeAsync();
    }
}
