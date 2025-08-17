namespace TIKSPlaywright;

[SetUpFixture]
public class SetupTests
{
    private readonly TIKSNUnit.SetupTests setup = new();

    [OneTimeSetUp]
    public async Task Init()
    {
        await setup.Init();
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await setup.CleanUp();
    }
}
