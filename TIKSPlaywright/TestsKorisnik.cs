using Microsoft.Playwright;
using NUnit.Framework.Internal;

namespace TIKSPlaywright;

[TestFixture, Order(2)]
public class TestsKorisnik : PageTest
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
    public async Task TestGetOglasi()
    {
        await LoginUser("testPoslodavac3");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Job Postings" }).ClickAsync();
        await Expect(Page.GetByText("Active Job PostingstestPoslodavac3testPoslodavac3Wanted Skills: plumberPosting")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(0)]
    public async Task TestGetZahteviPosao()
    {
        await LoginUser("testPoslodavac6");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Job Requests" }).ClickAsync();
        await Expect(Page.GetByText("Job RequestsRecipient:")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(0)]
    public async Task TestGetUgovori()
    {
        await LoginUser("testPoslodavac6");

        await Expect(Page.GetByText("Active ContractstestPoslodavac6 - testMajstor4Status: UnsignedSign")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(1)]
    public async Task TestNapraviRecenziju()
    {
        await LoginUser("testPoslodavac2");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Add Review" }).First.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "★" }).Nth(2).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Review Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Review Description*" }).FillAsync("testRecenzija-2-17-");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add Review" }).ClickAsync();
        await Page.GetByRole(AriaRole.Heading, new() { Name = "testPoslodavac2 - testMajstor2" }).ClickAsync();
        await Expect(Page.GetByText("testPoslodavac2Grade:★★★★★")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(1)]
    public async Task TestPotpisiUgovorErr()
    {
        await LoginUser("testMajstor6");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign Contract" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Name of Craftsman*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Name of Craftsman*" }).FillAsync("testMajstor6");
        await Page.GetByRole(AriaRole.Spinbutton, new() { Name = "Hourly Pay*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Spinbutton, new() { Name = "Hourly Pay*" }).FillAsync("10");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Job Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Job Description*" }).FillAsync("testUgovor-6-19-");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Date of Beginning*" }).FillAsync("2026-06-07");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Date of Ending*" }).FillAsync("2026-07-08");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Signature*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Signature*" }).SetInputFilesAsync(new[] { "folder.jpg" });
        void Page_Dialog_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            Page.Dialog -= Page_Dialog_EventHandler;
        }
        Page.Dialog += Page_Dialog_EventHandler;
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign Contract" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Sign Contract" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(2)]
    public async Task TestPotpisiUgovor()
    {
        await LoginUser("testMajstor4");

        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstor4 - testPoslodavac6Status: UnsignedSign Contract$") }).GetByRole(AriaRole.Button).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Name of Craftsman*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Name of Craftsman*" }).FillAsync("testMajstor4");
        await Page.GetByRole(AriaRole.Spinbutton, new() { Name = "Hourly Pay*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Spinbutton, new() { Name = "Hourly Pay*" }).FillAsync("100");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Job Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Job Description*" }).FillAsync("testUgovor-6-19");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Date of Beginning*" }).FillAsync("2030-10-10");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Date of Ending*" }).FillAsync("2030-10-11");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Signature*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Signature*" }).SetInputFilesAsync(new[] { "folder.jpg" });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign Contract" }).ClickAsync();
        await Expect(Page.GetByText("testMajstor4 - testPoslodavac6testUgovor-6-19Date of Beginning: 10/10/2030Date")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(3)]
    public async Task TestRaskiniUgovor()
    {
        await LoginUser("testMajstor7");

        await Page.Locator(".flex.flex-wrap > .bg-orange-700").First.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Expect(Page.GetByText("Status: Terminated by")).ToBeVisibleAsync();

        await LogoutUser();
    }


}
