using Microsoft.Playwright;
using NUnit.Framework.Internal;

namespace TIKSPlaywright;

[TestFixture, Order(0)]
public class TestsOsnovni : PageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
        };
    }

    [OneTimeSetUp]
    public void Init()
    {
        SetDefaultExpectTimeout(5000);
    }




    [Test]
    public async Task TestPromeniJezik()
    {
        await Page.GotoAsync("http://localhost:3000/");
        await Page.GetByRole(AriaRole.Button, new() { Name = "EN" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Srpski" }).ClickAsync();
        await Expect(Page.Locator("#root")).ToContainTextAsync("PRIJAVI SE");
        await Page.GetByRole(AriaRole.Button, new() { Name = "SR" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "English" }).ClickAsync();
        await Expect(Page.Locator("#root")).ToContainTextAsync("LOGIN");
    }



    //PREGLED MAJSTORA

    [Test]
    public async Task TestPregledMajstoraMinOcena1()
    {
        await Page.GotoAsync("http://localhost:3000/search_craftsmen");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByPlaceholder("Minimum Grade").ClickAsync();
        await Page.GetByPlaceholder("Minimum Grade").FillAsync("4.5");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page.GetByText("testMajstor6Skills: electricianGrade: ★★★★★From Jakarta, IndonesiaView Profile")).Not.ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Next" })).ToBeDisabledAsync();
    }

    [Test]
    public async Task TestPregledMajstoraMinOcena2()
    {
        await Page.GotoAsync("http://localhost:3000/search_craftsmen");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByPlaceholder("Minimum Grade").ClickAsync();
        await Page.GetByPlaceholder("Minimum Grade").FillAsync("1.5");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstor9Skills: carpenterGrade: ★★★★★From Tokyo, JapanView Profile$") }).Locator("span").Nth(2)).ToHaveClassAsync("text-yellow-500 text-3xl");
    }

    [Test]
    public async Task TestPregledMajstoraVestine([Values()]bool f)
    {
        await Page.GotoAsync("http://localhost:3000/search_craftsmen");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        if (f)
        {
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Other" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Other" }).FillAsync("none");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await Expect(Page.GetByText("testMajstor6Skills: electricianGrade: ★★★★★From Jakarta, IndonesiaView Profile")).Not.ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Next" })).ToBeDisabledAsync();
        }
        else
        {
            await Page.GetByRole(AriaRole.Checkbox).First.CheckAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await Expect(Page.GetByText("testMajstor9Skills: carpenterGrade: ★★★★★From Tokyo, JapanView Profile")).ToBeVisibleAsync();
            await Expect(Page.GetByText("testMajstor5Skills: carpenterGrade: ★★★★★From Tokyo, JapanView Profile")).ToBeVisibleAsync();
            await Expect(Page.GetByText("testMajstor6Skills: electricianGrade: ★★★★★From Jakarta, IndonesiaView Profile")).Not.ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task TestPregledMajstoraGrad()
    {
        await Page.GotoAsync("http://localhost:3000/search_craftsmen");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("guan");
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=guan");
        await Page.GetByRole(AriaRole.Combobox).SelectOptionAsync(new[] { "4" });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync(); 
        await Expect(Page.GetByText("From Guangzhou, China").First).ToContainTextAsync("From Guangzhou, China");
        await Expect(Page.GetByText("From Guangzhou, China").Nth(4)).ToContainTextAsync("From Guangzhou, China");
    }

    [Test]
    public async Task TestPregledMajstoraOcena()
    {
        await Page.GotoAsync("http://localhost:3000/search_craftsmen");
        await Expect(Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstor6Skills: electricianGrade: ★★★★★From Jakarta, IndonesiaView Profile$") }).Locator("span").Nth(2)).ToHaveClassAsync("text-yellow-500 text-3xl");
        await Expect(Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstor15Skills: plumberGrade: ★★★★★From Delhi, IndiaView Profile$") }).Locator("span").First).ToHaveClassAsync("text-gray-300 text-3xl");
    }

    [Test]
    public async Task TestPregledMajstoraBrRecenzija()
    {
        await Page.GotoAsync("http://localhost:3000/search_craftsmen");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Radio).Nth(1).CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstor6Skills: electricianGrade: ★★★★★From Jakarta, IndonesiaView Profile$") }).GetByRole(AriaRole.Button).ClickAsync();
        await Expect(Page.Locator("div:nth-child(10)")).ToBeVisibleAsync();
        await Page.GotoAsync("http://localhost:3000/search_craftsmen");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Radio).Nth(1).CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstor15Skills: plumberGrade: ★★★★★From Delhi, IndiaView Profile$") }).GetByRole(AriaRole.Button).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Reviews" })).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task TestPregledMajstoraNaziv([Values("empty", "testMajstor1", "tEsTmAjStOr1")]string name)
    {
        await Page.GotoAsync("http://localhost:3000/search_craftsmen");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Search..." }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Search..." }).FillAsync(name);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        if (name.Equals("empty"))
        {
            await Expect(Page.GetByText("testMajstor6Skills: electricianGrade: ★★★★★From Jakarta, IndonesiaView Profile")).Not.ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Next" })).ToBeDisabledAsync();
        }
        else
        {
            await Expect(Page.GetByText("testMajstor1Skills: carpenterGrade: ★★★★★From Tokyo, JapanView Profile")).ToBeVisibleAsync();
            await Expect(Page.GetByText("testMajstor13Skills: carpenterGrade: ★★★★★From Tokyo, JapanView Profile")).ToBeVisibleAsync();
            await Expect(Page.GetByText("testMajstor6Skills: electricianGrade: ★★★★★From Jakarta, IndonesiaView Profile")).Not.ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task TestPregledMajstoraStranica()
    {
        await Page.GotoAsync("http://localhost:3000/search_craftsmen");
        await Expect(Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstor6Skills: electricianGrade: ★★★★★From Jakarta, IndonesiaView Profile$") }).Locator("span").Nth(2)).ToHaveClassAsync("text-yellow-500 text-3xl");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
        await Expect(Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstor5Skills: carpenterGrade: ★★★★★From Tokyo, JapanView Profile$") }).Locator("span").First).ToHaveClassAsync("text-gray-300 text-3xl");
    }



    //PREGLED OGLASA

    [Test]
    public async Task TestPregledOglasaMinCena([Values(20, 40)]int rate)
    {
        await Page.GotoAsync("http://localhost:3000/search_job_postings");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByPlaceholder("Min. hourly pay").ClickAsync();
        await Page.GetByPlaceholder("Min. hourly pay").FillAsync($"{rate}");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        if (rate == 40)
        {
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac15testPoslodavac15Hourly Pay: 31 EURWanted Skills: plumber" }).Nth(3)).Not.ToBeVisibleAsync();
        }
        else
        {
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac15testPoslodavac15Hourly Pay: 31 EURWanted Skills: plumber" }).Nth(3)).ToBeVisibleAsync();
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac6testPoslodavac6Hourly Pay: 17.5 EURWanted Skills: electrician" }).Nth(3)).Not.ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task TestPregledOglasaMinOcena([Values(3, 4)]int grade)
    {
        await Page.GotoAsync("http://localhost:3000/search_job_postings");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByPlaceholder("Min. employer grade").ClickAsync();
        await Page.GetByPlaceholder("Min. employer grade").FillAsync($"{grade}");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        if (grade == 3)
        {
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac10testPoslodavac10Hourly Pay: 23.5 EURWanted Skills: electrician" }).Nth(3)).Not.ToBeVisibleAsync();
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac14testPoslodavac14Hourly Pay: 29.5 EURWanted Skills: electrician" }).Nth(3)).ToBeVisibleAsync();
        }
        else
        {
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac14testPoslodavac14Hourly Pay: 29.5 EURWanted Skills: electrician" }).Nth(3)).Not.ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task TestPregledOglasaGrad()
    {
        await Page.GotoAsync("http://localhost:3000/search_job_postings");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("guan");
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=guan");
        await Page.GetByRole(AriaRole.Combobox).SelectOptionAsync(new[] { "4" });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac4testPoslodavac4Hourly Pay: 14.5 EURWanted Skills: tilesetter" }).Nth(3)).ToBeVisibleAsync();
    }

    [Test]
    public async Task TestPregledOglasaOcena()
    {
        await Page.GotoAsync("http://localhost:3000/search_job_postings");
        await Page.Locator("div").Filter(new() { HasText = "testPoslodavac6testPoslodavac6Hourly Pay: 17.5 EURWanted Skills: electrician" }).Nth(3).ClickAsync();
        await Page.GetByText("testPoslodavac6").Nth(1).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Paragraph).Filter(new() { HasText = "Grade: ★★★★★" }).Locator("span").Nth(2)).ToHaveClassAsync("text-yellow-500 text-3xl");
        await Page.GotoAsync("http://localhost:3000/search_job_postings");
        await Page.Locator("div").Filter(new() { HasText = "testPoslodavac15testPoslodavac15Hourly Pay: 31 EURWanted Skills: plumber" }).Nth(3).ClickAsync();
        await Page.GetByText("testPoslodavac15").Nth(1).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Paragraph).Filter(new() { HasText = "Grade: ★★★★★" }).Locator("span").Nth(1)).ToHaveClassAsync("text-gray-300 text-3xl");
    }

    [Test]
    public async Task TestPregledOglasaCenaPoSatu()
    {
        await Page.GotoAsync("http://localhost:3000/search_job_postings");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Radio).Nth(1).CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac15testPoslodavac15Hourly Pay: 31 EURWanted Skills: plumber" }).Nth(3)).ToBeVisibleAsync();
        await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac5testPoslodavac5Hourly Pay: 16 EURWanted Skills: carpenter" }).Nth(3)).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task TestPregledOglasaReci([Values("empty", "testPoslodavac1", "tEsTpOsLoDaVaC1")]string words)
    {
        await Page.GotoAsync("http://localhost:3000/search_job_postings");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Search" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Search" }).FillAsync(words);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        if (words.Equals("empty"))
        {
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac6testPoslodavac6Hourly Pay: 17.5 EURWanted Skills: electrician" }).Nth(3)).Not.ToBeVisibleAsync();
        }
        else
        {
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac1testPoslodavac1Hourly Pay: 10 EURWanted Skills: carpenter" }).Nth(3)).ToBeVisibleAsync();
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac10testPoslodavac10Hourly Pay: 23.5 EURWanted Skills: electrician" }).Nth(3)).ToBeVisibleAsync();
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac6testPoslodavac6Hourly Pay: 17.5 EURWanted Skills: electrician" }).Nth(3)).Not.ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task TestPregledOglasaVestine([Values()]bool f)
    {
        
        await Page.GotoAsync("http://localhost:3000/search_job_postings");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        if (f)
        {
            await Page.GetByRole(AriaRole.Checkbox).First.CheckAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac9testPoslodavac9Hourly Pay: 22 EURWanted Skills: carpenter" }).Nth(3)).ToBeVisibleAsync();
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac5testPoslodavac5Hourly Pay: 16 EURWanted Skills: carpenter" }).Nth(3)).ToBeVisibleAsync();
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac6testPoslodavac6Hourly Pay: 17.5 EURWanted Skills: electrician" }).Nth(3)).Not.ToBeVisibleAsync();
        }
        else
        {
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Other" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Other" }).FillAsync("none");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac6testPoslodavac6Hourly Pay: 17.5 EURWanted Skills: electrician" }).Nth(3)).Not.ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Next" })).ToBeDisabledAsync();
        }
    }

    [Test]
    public async Task TestPregledOglasaStranica()
    {
        await Page.GotoAsync("http://localhost:3000/search_job_postings");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Radio).Nth(1).CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac15testPoslodavac15Hourly Pay: 31 EURWanted Skills: plumber" }).Nth(3)).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
        await Expect(Page.Locator("div").Filter(new() { HasText = "testPoslodavac1testPoslodavac1Hourly Pay: 10 EURWanted Skills: carpenter" }).Nth(3)).ToBeVisibleAsync();
    }





    [Test]
    public async Task TestGetRecenzije([Values()]bool f)
    {
        await Page.GotoAsync("http://localhost:3000/search_job_postings");
        if (f)
        {
            await Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
            await Page.Locator("div").Filter(new() { HasText = "testPoslodavac5testPoslodavac5Hourly Pay: 16 EURWanted Skills: carpenter" }).Nth(3).ClickAsync();
            await Page.GetByText("testPoslodavac5").Nth(1).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Reviews" })).Not.ToBeVisibleAsync();
        }
        else
        {
            await Page.Locator("div").Filter(new() { HasText = "testPoslodavac10testPoslodavac10Hourly Pay: 23.5 EURWanted Skills: electrician" }).Nth(3).ClickAsync();
            await Page.GetByText("testPoslodavac10").Nth(1).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Reviews" })).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task TestGetOglas()
    {
        await Page.GotoAsync("http://localhost:3000/search_job_postings");
        await Page.Locator("div").Filter(new() { HasText = "testPoslodavac6testPoslodavac6Hourly Pay: 17.5 EURWanted Skills: electrician" }).Nth(3).ClickAsync();
        await Expect(Page.GetByText("testPoslodavac6").Nth(2)).ToBeVisibleAsync();
    }
}
