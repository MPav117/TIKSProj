using Microsoft.Playwright;
using NUnit.Framework.Internal;

namespace TIKSPlaywright;

[TestFixture, Order(1)]
public class TestsProfil : PageTest
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

    private async Task LoginUser(string username, string password)
    {
        await Page.GotoAsync("http://localhost:3000/");
        await Page.GetByRole(AriaRole.Link, new() { Name = "LOGIN" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync(username);
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync(password);
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
    public async Task TestGetGradovi([Values("nis", "empty")] string name)
    {
        await Page.GotoAsync("http://localhost:3000/");
        await Page.GetByRole(AriaRole.Button, new() { Name = "REGISTER" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Employer" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        if (name.Equals("nis"))
        {
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("nis");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Combobox)).ToContainTextAsync("Nishinomiya-hama, JapanNishitokyo, JapanNis, SerbiaNishio, JapanNisshin, JapanNishiwaki, JapanNishihara, JapanNisia Floresta, BrazilNiscemi, ItalyNiskayuna, United StatesNishigo, JapanNisang, IndiaNisko, PolandNishinoomote, JapanNiska Banja, SerbiaNishon Tumani, UzbekistanNishi, JapanNisarpur, IndiaNisporeni, Moldova");
        }
        else
        {
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("empty");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Combobox)).ToContainTextAsync("");
        }
    }

    [Test, Order(0)]
    public async Task TestVratiKorisnika()
    {
        await Page.GotoAsync("http://localhost:3000/");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Search Craftsmen" }).ClickAsync();
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstor6Skills: electricianGrade: ★★★★★From Jakarta, IndonesiaView Profile$") }).GetByRole(AriaRole.Button).ClickAsync();
        await Expect(Page.GetByText("testMajstor6").Nth(1)).ToBeVisibleAsync();
    }

    [Test, Order(0)]
    public async Task TestLoginErr()
    {
        await Page.GotoAsync("http://localhost:3000/");
        await Page.GetByRole(AriaRole.Link, new() { Name = "LOGIN" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync("testNone");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("testNone");
        void Page_Dialog_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            Page.Dialog -= Page_Dialog_EventHandler;
        }
        Page.Dialog += Page_Dialog_EventHandler;
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.Locator("div").Filter(new() { HasText = "Username*Password*Login" }).Nth(2)).ToBeVisibleAsync();
    }

    [Test, Order(1)]
    public async Task TestLogin()
    {
        await Page.GotoAsync("http://localhost:3000/");
        await Page.GetByRole(AriaRole.Link, new() { Name = "LOGIN" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync("testMajstor1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("testMajstor1");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.Locator("div").Filter(new() { HasText = "craftsmanSwitch ProfileLeave" }).Nth(1)).ToBeVisibleAsync();
    }

    [Test, Order(2)]
    public async Task TestLogout()
    {
        await LoginUser("testMajstor1", "testMajstor1");

        await Page.GetByRole(AriaRole.Button, new() { Name = "MENU" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log Out" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "REGISTER" })).ToBeVisibleAsync();
    }

    //[Ignore("")]
    [Test, Order(2)]
    public async Task TestPodaciRegistracije()
    {
        await LoginUser("testMajstor1", "testMajstor1");

        //await Page.GetByRole(AriaRole.Link).Filter(new() { HasTextRegex = new Regex("^$") }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Search Craftsmen" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "PROFILE" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(2)]
    public async Task TestRegisterPoslodavacErr()
    {
        await Page.GotoAsync("http://localhost:3000/");
        await Page.GetByRole(AriaRole.Button, new() { Name = "REGISTER" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Employer" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync("testPoslodavac1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("testPoslodavacT1");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).SetInputFilesAsync(new[] { "folder.jpg" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).FillAsync("testPoslodavacT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).FillAsync("testPoslodavacT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("testPoslodavacT1@gmail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("tok");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=tok");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Adress" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Adress" }).FillAsync("testPoslodavacT1");
        void Page_Dialog_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            Page.Dialog -= Page_Dialog_EventHandler;
        }
        Page.Dialog += Page_Dialog_EventHandler;
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true })).ToBeVisibleAsync();
    }

    //[Ignore("")]
    [Test, Order(3)]
    public async Task TestRegisterPoslodavac1()
    {
        await Page.GotoAsync("http://localhost:3000/");
        await Page.GetByRole(AriaRole.Button, new() { Name = "REGISTER" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Employer" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync("testPoslodavacT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("testPoslodavacT1");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).SetInputFilesAsync(new[] { "folder.jpg" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).FillAsync("testPoslodavacT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).FillAsync("testPoslodavacT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("testPoslodavacT1@gmail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("tok");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=tok");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Adress" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Adress" }).FillAsync("testPoslodavacT1");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Login" })).ToBeVisibleAsync();
    }

    [Test, Order(4)]
    public async Task TestRegisterPoslodavac2()
    {
        await LoginUser("testMajstor15", "testMajstor15");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Register as Employer" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync("testPoslodavacT2");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("testPoslodavacT2");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).SetInputFilesAsync(new[] { "folder.jpg" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).FillAsync("testPoslodavacT2");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).FillAsync("testPoslodavacT2");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("testPoslodavacT2@gmail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("tok");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=tok");
        await Page.GetByRole(AriaRole.Combobox).SelectOptionAsync(new[] { "1" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Adress" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Adress" }).FillAsync("testPoslodavacT2");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Login" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(4)]
    public async Task TestRegisterMajstorErr()
    {
        await Page.GotoAsync("http://localhost:3000/");
        await Page.GetByRole(AriaRole.Button, new() { Name = "REGISTER" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Craftsman" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync("testMajstor1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("testMajstorT1");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).SetInputFilesAsync(new[] { "folder.jpg" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).FillAsync("testMajstorT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).FillAsync("testMajstorT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("testMajstorT1@gmail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("tok");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=tok");
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Carpenter$") }).GetByRole(AriaRole.Checkbox).CheckAsync();
        void Page_Dialog1_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            Page.Dialog -= Page_Dialog1_EventHandler;
        }
        Page.Dialog += Page_Dialog1_EventHandler;
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true })).ToBeVisibleAsync();
    }

    //[Ignore("")]
    [Test, Order(5)]
    public async Task TestRegisterMajstor1()
    {
        await Page.GotoAsync("http://localhost:3000/");
        await Page.GetByRole(AriaRole.Button, new() { Name = "REGISTER" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Craftsman" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync("testMajstorT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("testMajstorT1");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).SetInputFilesAsync(new[] { "folder.jpg" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).FillAsync("testMajstorT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).FillAsync("testMajstorT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("testMajstorT1@gmail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("tok");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=tok");
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Carpenter$") }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Login" })).ToBeVisibleAsync();
    }

    [Test, Order(6)]
    public async Task TestRegisterMajstor2()
    {
        await LoginUser("testPoslodavac15", "testPoslodavac15");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Register as Craftsman" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync("testMajstorT2");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("testMajstorT2");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).SetInputFilesAsync(new[] { "folder.jpg" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).FillAsync("testMajstorT2");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).FillAsync("testMajstorT2");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("testMajstorT2@gmail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("tok");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=tok");
        await Page.GetByRole(AriaRole.Combobox).SelectOptionAsync(new[] { "1" });
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Carpenter$") }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Login" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(6)]
    public async Task TestRegisterGrupaErr()
    {
        await LoginUser("testMajstor3", "testMajstor3");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Group Requests" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Form Craftsman Group" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync("testGrupa1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("testGrupaT1");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).SetInputFilesAsync(new[] { "folder.jpg" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).FillAsync("testGrupaT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).FillAsync("testGrupaT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("testGrupaT1@gmail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("tok");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=tok");
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Carpenter$") }).GetByRole(AriaRole.Checkbox).CheckAsync();
        void Page_Dialog1_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            Page.Dialog -= Page_Dialog1_EventHandler;
        }
        Page.Dialog += Page_Dialog1_EventHandler;
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(7)]
    public async Task TestRegisterGrupa()
    {
        await LoginUser("testMajstor3", "testMajstor3");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Group Requests" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Form Craftsman Group" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Username" }).FillAsync("testGrupaT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("testGrupaT1");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Profile photo*" }).SetInputFilesAsync(new[] { "folder.jpg" });
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Public Name" }).FillAsync("testGrupaT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Description*" }).FillAsync("testGrupaT1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("testGrupaT1@gmail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "City" }).FillAsync("tok");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Profil/GetGradovi?start=tok");
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^Carpenter$") }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register", Exact = true }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Login" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(7)]
    public async Task TestIzbrisiProfilErr()
    {
        await LoginUser("testMajstor6", "testMajstor6");

        await Page.GetByRole(AriaRole.Button, new() { Name = "MENU" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete Profile" }).ClickAsync();
        void Page_Dialog2_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            Page.Dialog -= Page_Dialog2_EventHandler;
        }
        Page.Dialog += Page_Dialog2_EventHandler;
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Expect(Page.GetByText("Are you sure you want to delete your profile?YesNo")).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(8)]
    public async Task TestIzbrisiProfil()
    {
        await LoginUser("testPoslodavacT1", "testPoslodavacT1");

        await Page.GetByRole(AriaRole.Button, new() { Name = "MENU" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete Profile" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Expect(Page.GetByText("ufindiFind the right local")).ToBeVisibleAsync();
    }
    
    [Test, Order(8)]
    public async Task TestAdminIzbrisiProfilErr()
    {
        await LoginUser("marko", "password");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Search Craftsmen" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Osnovni/pregledMajstora/ocena/2?minOcenaf=-1&gradIDf=-1&nazivSearch=null");
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^sanjaSkills: carpenterGrade: ★★★★★From Nis, SerbiaView ProfileSend Job Request$") }).GetByRole(AriaRole.Button).First.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete User Profile" }).ClickAsync();
        void Page_Dialog_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            Page.Dialog -= Page_Dialog_EventHandler;
        }
        Page.Dialog += Page_Dialog_EventHandler;
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Delete User Profile" })).ToBeVisibleAsync();

        await LogoutUser();
    }

    [Test, Order(9)]
    public async Task TestAdminIzbrisiProfil()
    {
        await LoginUser("marko", "password");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Search Craftsmen" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
        await Page.WaitForResponseAsync("https://localhost:7080/Osnovni/pregledMajstora/ocena/2?minOcenaf=-1&gradIDf=-1&nazivSearch=null");
        if(!await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstorT1Skills: carpenterGrade: ★★★★★From Tokyo, JapanView ProfileSend Job Request$") }).IsVisibleAsync())
        {
            await Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
            await Page.WaitForResponseAsync("https://localhost:7080/Osnovni/pregledMajstora/ocena/3?minOcenaf=-1&gradIDf=-1&nazivSearch=null");
        }
        await Page.Locator("div").Filter(new() { HasTextRegex = new Regex("^testMajstorT1Skills: carpenterGrade: ★★★★★From Tokyo, JapanView ProfileSend Job Request$") }).GetByRole(AriaRole.Button).First.ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete User Profile" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "MENU" })).ToBeVisibleAsync();

        await LogoutUser();
    }
}
