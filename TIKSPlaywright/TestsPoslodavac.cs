using Microsoft.Playwright;
using NUnit.Framework.Internal;

namespace TIKSPlaywright;

[TestFixture, Order(3)]
public class TestsPoslodavac : PageTest
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
    public async Task TestAzurirajPoslodavac()
    {
        await LoginUser("testPoslodavac14");

        await Page.GetByRole(AriaRole.Button, new() { Name = "MENU" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Edit Profile" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo" }).SetInputFilesAsync(new[] { "folder.jpg" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).FillAsync("novi naziv");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description" }).FillAsync("novi opis");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("chon");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=chon");
        await Page.GetByRole(AriaRole.Combobox).SelectOptionAsync(new[] { "33" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Adress" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Adress" }).FillAsync("nova adresa");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();
        await Expect(Page.Locator("#root div").Filter(new() { HasText = "novi nazivnovi opisGrade" }).Nth(3)).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(1)]
    public async Task TestPostaviOglas()
    {
        await LoginUser("testPoslodavac1");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Create Job Posting" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Title" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Title" }).FillAsync("testOglasT1");
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Carpenter$") }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).FillAsync("testOglasT1");
        await Page.GetByRole(AriaRole.Spinbutton, new() { Name = "Hourly Pay*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Spinbutton, new() { Name = "Hourly Pay*" }).FillAsync("10");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Deadline*" }).FillAsync("2030-01-01");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();
        await Expect(Page.Locator("#root div").Filter(new() { HasText = "testPoslodavac1testPoslodavac1Grade: ★★★★★Email: testPoslodavac1@gmail." }).Nth(3)).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(2)]
    public async Task TestIzbrisiOglasErr()
    {
        await LoginUser("testPoslodavac2");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Job Postings" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete Job Posting" }).ClickAsync();
        void Page_Dialog4_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            Page.Dialog -= Page_Dialog4_EventHandler;
        }
        Page.Dialog += Page_Dialog4_EventHandler;
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Delete Job Posting" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(3)]
    public async Task TestIzbrisiOglas()
    {
        await LoginUser("testPoslodavac4");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Job Postings" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete Job Posting" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "No Job Postings" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(4)]
    public async Task TestNapraviZahtevPosao()
    {
        await LoginUser("testPoslodavac1");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Search Craftsmen" }).ClickAsync();
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstor12Skills: tilesetterGrade: ★★★★★From Guangzhou, ChinaView ProfileSend Job Request$") }).GetByRole(AriaRole.Button).Nth(1).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).FillAsync("testZahtev-1-27-");
        await Page.GetByRole(AriaRole.Spinbutton, new() { Name = "Hourly Pay*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Spinbutton, new() { Name = "Hourly Pay*" }).FillAsync("15");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Deadline*" }).FillAsync("2033-02-01");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Job Requests" }).ClickAsync();
        await Expect(Page.GetByText("Recipient: testMajstor12testZahtev-1-27-Hourly Pay: 15 EURDate of Ending: 2/1/")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(5)]
    public async Task TestPovuciZahtevPosao()
    {
        await LoginUser("testPoslodavac1");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Job Requests" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Retract" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "No Job Requests" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(6)]
    public async Task TestPovuciZahtevPosao1()
    {
        await LoginUser("testPoslodavac2");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Job Failed" }).First.ClickAsync();
        await Expect(Page.GetByText("Status: Unsuccessfuly Finished")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(7)]
    public async Task TestPovuciZahtevPosao2()
    {
        await LoginUser("testPoslodavac2");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Job Successful" }).First.ClickAsync();
        await Expect(Page.GetByText("Status: Successfuly Finished")).ToBeVisibleAsync();

        await LogoutUser();
    }
}
