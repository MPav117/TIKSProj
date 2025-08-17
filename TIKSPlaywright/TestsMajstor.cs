using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using Models;
using NUnit.Framework.Internal;

namespace TIKSPlaywright;

[TestFixture, Order(4)]
public class TestsMajstor : PageTest
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

    private async Task LoginUser(string username)
    {
        await Page.GotoAsync("http://localhost:3000/");
        await Page.GetByRole(AriaRole.Link, new() { Name = "LOGIN" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync(username);
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync(username);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
    }

    private async Task LogoutUser()
    {
        await Page.GotoAsync("http://localhost:3000/profile");
        await Page.GetByRole(AriaRole.Button, new() { Name = "MENU" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log Out" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
    }





    [Test, Order(0)]
    public async Task TestAzurirajMajstor()
    {
        await LoginUser("testMajstor14");

        await Page.GetByRole(AriaRole.Button, new() { Name = "MENU" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Edit Profile" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo" }).SetInputFilesAsync(new[] { "folder.jpg" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).FillAsync("novi naziv");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description" }).FillAsync("novi opis");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("kua");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=kua");
        await Page.GetByRole(AriaRole.Combobox).SelectOptionAsync(new[] { "52" });
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Tilesetter$") }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Electrician$") }).GetByRole(AriaRole.Checkbox).UncheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();
        await Expect(Page.Locator("#root div").Filter(new() { HasText = "novi nazivnovi opisGrade" }).Nth(3)).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(1)]
    public async Task TestPrijaviNaOglas()
    {
        await LoginUser("testMajstor4");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Search Job Postings" }).ClickAsync();
        await Page.Locator("div").Filter(new() { HasText = "testPoslodavac8testPoslodavac8Hourly Pay: 20.5 EURWanted Skills: tilesetter" }).Nth(3).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Sign off" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(2)]
    public async Task TestOdjaviSaOglasa()
    {
        await LoginUser("testMajstor4");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Search Job Postings" }).ClickAsync();
        await Page.Locator("div").Filter(new() { HasText = "testPoslodavac8testPoslodavac8Hourly Pay: 20.5 EURWanted Skills: tilesetter" }).Nth(3).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign off" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(3)]
    public async Task TestGetKalendar()
    {
        await LoginUser("testMajstor6");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Calendar" }).ClickAsync();
        await Expect(Page.Locator(".react-calendar")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(3)]
    public async Task TestGetZahteviGrupa()
    {
        await LoginUser("testMajstorP1");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Group Requests" }).ClickAsync();
        await Expect(Page.GetByText("Sent Group RequestsRecipient")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(3)]
    public async Task TestGetClanovi()
    {
        await LoginUser("testGrupa2");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Members" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Kick From Group" }).First).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(3)]
    public async Task TestNapraviZahtevGrupa()
    {
        await LoginUser("testMajstor4");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Search Craftsmen" }).ClickAsync();
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstor7Skills: plumberGrade: ★★★★★From Delhi, IndiaView Profile$") }).GetByRole(AriaRole.Button).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Send Group Request" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Request Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Request Description*" }).FillAsync("testGZT1");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Send Group Request" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "MENU" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(3)]
    public async Task TestOdgovorZahtevGrupaErr()
    {
        await LoginUser("testMajstor13");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Group Requests" }).ClickAsync();
        void Page_Dialog2_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            Page.Dialog -= Page_Dialog2_EventHandler;
        }
        Page.Dialog += Page_Dialog2_EventHandler;
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Sender: testMajstor6testGrupa2128AcceptRefuse$") }).GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Page.GetByText("Sender: testMajstor6testGrupa2128AcceptRefuse")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(4)]
    public async Task TestOdgovorZahtevGrupa1()
    {
        await LoginUser("testMajstor13");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Group Requests" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Refuse" }).First.ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Majstor/GetZahteviGrupa");
        await Expect(Page.GetByText("Sender: testMajstorP1testGrupa3628AcceptRefuse")).Not.ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(5)]
    public async Task TestOdgovorZahtevGrupa2()
    {
        await LoginUser("testMajstor13");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Group Requests" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Accept" }).Nth(3).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Majstor/GetZahteviGrupa");
        await Expect(Page.GetByText("Sender: testGrupa2testGrupa4228AcceptRefuse")).Not.ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(6)]
    public async Task TestIzbaciIzGrupeErr()
    {
        await LoginUser("testGrupa2");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Members" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Kick From Group" }).First.ClickAsync();
        void Page_Dialog1_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            Page.Dialog -= Page_Dialog1_EventHandler;
        }
        Page.Dialog += Page_Dialog1_EventHandler;
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Expect(Page.GetByText("testMajstor2Skills: electricianGrade: ★★★★★From Jakarta, IndonesiaView")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(7)]
    public async Task TestIzbaciIzGrupe()
    {
        await LoginUser("testGrupa2");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Members" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Kick From Group" }).Nth(3).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Members" }).ClickAsync();
        await Expect(Page.GetByText("testMajstor13Skills: carpenterGrade: ★★★★★From Tokyo, JapanView ProfileKick")).Not.ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(8)]
    public async Task TestUpisiKalendar()
    {
        await LoginUser("testMajstor3");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Calendar" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "›" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "›" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "October 30," }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Calendar" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Group Requests" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Calendar" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "›" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "›" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "October 30," })).ToHaveClassAsync("react-calendar__tile react-calendar__month-view__days__day bg-gray-300");

        await LogoutUser();
    }

    [Test, Order(9)]
    public async Task TestOdgovorZahtevPosao1()
    {
        await LoginUser("testMajstor2");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Job Requests" }).ClickAsync();
        await Page.Locator(".flex.space-x-2 > .bg-orange-700").First.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Job Requests" }).ClickAsync();
        await Expect(Page.GetByText("Sender: testPoslodavac6testZahtev-6-17-Hourly Pay: 13 EURDate of Ending: 1/1/")).Not.ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(10)]
    public async Task TestOdgovorZahtevPosao2()
    {
        await LoginUser("testMajstor2");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Job Requests" }).ClickAsync();
        await Page.Locator("div:nth-child(4) > .flex.flex-wrap > .flex > .bg-green-700").ClickAsync();
        await Expect(Page.GetByText("testMajstor2 - testPoslodavac7Status: UnsignedSign Contract")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(11)]
    public async Task TestIzlazIzGrupeErr()
    {
        await LoginUser("testMajstor14");

        void Page_Dialog_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            Page.Dialog -= Page_Dialog_EventHandler;
        }
        Page.Dialog += Page_Dialog_EventHandler;
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Leave Group$") }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Leave Group" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(12)]
    public async Task TestIzlazIzGrupe()
    {
        await LoginUser("testMajstor1");

        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Leave Group$") }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Leave Group" })).Not.ToBeVisibleAsync();

        await LogoutUser();
    }
}
