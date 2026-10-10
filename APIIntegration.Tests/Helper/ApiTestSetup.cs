namespace APIIntegration.Tests.Helper;

public class ApiTestSetup : IAsyncLifetime
{
    public TestInit TestInit1 { get; private protected set; } = null!;

    public async Task InitializeAsync()
    {
        TestInit1 = await TestInit.Create();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
} 
