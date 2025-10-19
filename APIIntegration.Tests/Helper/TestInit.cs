using API;
using GenerateApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace APIIntegration.Tests.Helper;

public class TestInit
{
    public IServiceScope Scope { get; }
    public HttpClient HttpClient { get; set; }
    public WebApplicationFactory<Program> WebApp { get; }
    public AccountingDbContext AccountingDbContext => Scope.ServiceProvider.GetRequiredService<AccountingDbContext>();
    public GroupAccountClient GroupAccountClient => new(HttpClient);
    public GeneralLedgerAccountClient GeneralLedgerAccountClient => new(HttpClient);
    public SubLedgerAccountClient SubLedgerAccountClient => new(HttpClient);
    public DetailedAccountClient DetailedAccountClient => new(HttpClient);
    public FinancialPeriodClient FinancialPeriodClient => new(HttpClient);
    public JournalVoucherClient JournalVouchClient => new(HttpClient);
    public JournalEntryClient JournalEntryClient => new(HttpClient);


    private TestInit(Dictionary<string, string?> appSettings, string environment)
    {
        WebApp = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                foreach (var appSetting in appSettings)
                    builder.UseSetting(appSetting.Key, appSetting.Value);

                builder.UseEnvironment(environment);

            });

        HttpClient = WebApp.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        Scope = WebApp.Services.CreateScope();
    }

    private async Task Init()
    {

    }

    public static async Task<TestInit> Create(Dictionary<string, string?>? appSettings = null, string environment = "Development")
    {
        var testInit = new TestInit(appSettings ?? new Dictionary<string, string?>(), environment);
        await testInit.Init();
        return testInit;
    }
}