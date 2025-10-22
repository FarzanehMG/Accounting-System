using API;
using Domain.Entities.Accounts;
using Domain.Entities.FinancialPeriod;
using Domain.Entities.Journal;
using FluentAssertions;
using GenerateApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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

    public Guid FinancialPeriodId { get; set; }

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
        // Get or Create Financial period
        var financialPeriod = await GetOrCreateFinancialPeriod();
        FinancialPeriodId = financialPeriod.Id;
    }

    public static async Task<TestInit> Create(Dictionary<string, string?>? appSettings = null, string environment = "Development")
    {
        var testInit = new TestInit(appSettings ?? new Dictionary<string, string?>(), environment);
        await testInit.Init();
        return testInit;
    }

    internal async Task CreateJournalEntry(Guid groupAccountId)
    {
        // Create GeneralLedger account
        var generalLedgerAccount = await CreateGeneralLedgerAccount(groupAccountId, JournalEntryType.Credit);
        var generalLedgerAccount2 = await CreateGeneralLedgerAccount(groupAccountId, JournalEntryType.Debit);

        // Create SubLedger account
        var subLedgerAccount = await CreateSubLedgerAccount(generalLedgerAccount.Id, JournalEntryType.Credit);
        var subLedgerAccount2 = await CreateSubLedgerAccount(generalLedgerAccount2.Id, JournalEntryType.Debit);

        // Create JournalVoucher
        var journalVoucher = await CreateJournalVoucher();

        // Create JournalEntry
        var createJournalEntry = new CreateJournalEntryApiModel
        {
            Entries = new List<JournalEntryApiModel>
            {
                new()
                {
                    Amount = 2,
                    JournalEntryType = JournalEntryType.Credit,
                    JournalVoucherId = journalVoucher.Id,
                    SubLedgerAccountId = subLedgerAccount.Id,
                    Attachments = []
                },
                new()
                {

                    Amount = 2,
                    JournalEntryType = JournalEntryType.Debit,
                    JournalVoucherId = journalVoucher.Id,
                    SubLedgerAccountId = subLedgerAccount2.Id,
                    Attachments = []
                }
            }
        };
        await JournalEntryClient.CreateAsync(createJournalEntry);

        // Assert JournalEntries exist
        var journalEntries = await AccountingDbContext.Set<JournalEntryEntity>()
            .Where(x => x.JournalVoucherId == journalVoucher.Id)
            .ToListAsync();

        journalEntries.Should().HaveCount(2);
        journalEntries.Should().ContainSingle(x => x.SubLedgerAccountId == subLedgerAccount.Id && x.JournalEntryType == Domain.Enums.JournalEntryType.Credit);
        journalEntries.Should().ContainSingle(x => x.SubLedgerAccountId == subLedgerAccount2.Id && x.JournalEntryType == Domain.Enums.JournalEntryType.Debit);
    }

    internal async Task<GroupAccountEntity> CreateGroupAccount(JournalEntryType journalEntryType, bool isActive = true)
    {
        var createGroupAccount = new CreateGroupAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = isActive,
            JournalEntryType = journalEntryType,
        };
        await GroupAccountClient.CreateAsync(createGroupAccount);

        // Assert
        var groupAccount = await AccountingDbContext.Set<GroupAccountEntity>()
            .SingleAsync(x => x.AccountTitle == createGroupAccount.Title && x.AccountCode == createGroupAccount.Code);

        groupAccount.Should().NotBeNull();
        groupAccount.AccountTitle.Should().Be(createGroupAccount.Title);
        groupAccount.AccountCode.Should().Be(createGroupAccount.Code);

        // Return Group account
        return groupAccount;
    }

    private async Task<GeneralLedgerAccountEntity> CreateGeneralLedgerAccount(Guid groupAccountId, JournalEntryType journalEntryType)
    {
        var createGeneralLedgerAccount = new CreateGeneralLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = journalEntryType,
            GroupAccountId = groupAccountId
        };
        await GeneralLedgerAccountClient.CreateAsync(createGeneralLedgerAccount);

        // Assert
        var generalLedgerAccount = await AccountingDbContext.Set<GeneralLedgerAccountEntity>()
            .SingleAsync(x => x.AccountTitle == createGeneralLedgerAccount.Title && x.AccountCode == createGeneralLedgerAccount.Code);

        generalLedgerAccount.Should().NotBeNull();
        generalLedgerAccount.AccountTitle.Should().Be(createGeneralLedgerAccount.Title);
        generalLedgerAccount.AccountCode.Should().Be(createGeneralLedgerAccount.Code);
        generalLedgerAccount.IsActive.Should().BeTrue();
        generalLedgerAccount.GroupAccountId.Should().Be(groupAccountId);

        // Return GeneralLedger account
        return generalLedgerAccount;
    }

    private async Task<SubLedgerAccountEntity> CreateSubLedgerAccount(Guid generalLedgerAccountId, JournalEntryType journalEntryType)
    {
        var createSubLedgerAccount = new CreateSubLedgerAccountApiModel
        {
            Title = Guid.NewGuid().ToString(),
            Code = Guid.NewGuid().ToString(),
            IsActive = true,
            JournalEntryType = journalEntryType,
            GeneralLedgerAccountId = generalLedgerAccountId
        };
        await SubLedgerAccountClient.CreateAsync(createSubLedgerAccount);

        // Assert
        var subLedgerAccount = await AccountingDbContext.Set<SubLedgerAccountEntity>()
            .SingleAsync(x => x.AccountTitle == createSubLedgerAccount.Title && x.AccountCode == createSubLedgerAccount.Code);

        subLedgerAccount.Should().NotBeNull();
        subLedgerAccount.AccountTitle.Should().Be(createSubLedgerAccount.Title);
        subLedgerAccount.AccountCode.Should().Be(createSubLedgerAccount.Code);
        subLedgerAccount.IsActive.Should().BeTrue();
        subLedgerAccount.GeneralLedgerAccountId.Should().Be(generalLedgerAccountId);

        // Return SubLedger account
        return subLedgerAccount;
    }

    private async Task<FinancialPeriodEntity> GetOrCreateFinancialPeriod()
    {
        var financialPeriod = await AccountingDbContext.Set<FinancialPeriodEntity>()
            .SingleOrDefaultAsync(x=>x.IsActive);

        if (financialPeriod is null)
        {

            var createFinancialPeriod = new CreateFinancialPeriodApiModel
            {
                FinancialPeriodTitle = Guid.NewGuid().ToString(),
                StartDate = DateTime.UtcNow.AddDays(-100),
                EndDate = DateTime.UtcNow.AddDays(100)
            };
            await FinancialPeriodClient.CreateAsync(createFinancialPeriod);

            financialPeriod = await AccountingDbContext.Set<FinancialPeriodEntity>()
                .SingleAsync(x => x.FinancialPeriodTitle == createFinancialPeriod.FinancialPeriodTitle && x.StartDate == createFinancialPeriod.StartDate);

            financialPeriod.FinancialPeriodTitle.Should().Be(createFinancialPeriod.FinancialPeriodTitle);
            financialPeriod.StartDate.Should().Be(createFinancialPeriod.StartDate);
            financialPeriod.EndDate.Should().Be(createFinancialPeriod.EndDate);
        }

        financialPeriod.Should().NotBeNull();

        // Return Financial period
        return financialPeriod;
    }

    private async Task<JournalVoucherEntity> CreateJournalVoucher()
    {
        var createJournalVoucher = new CreateJournalVoucherApiModel
        {
            JournalVoucherTitle = Guid.NewGuid().ToString(),
            ReferenceNumber = Guid.NewGuid().ToString(),
            TransactionTime = DateTime.UtcNow,
            CreatedByUserId = Guid.NewGuid(),
            FinancialPeriodId = FinancialPeriodId
        };
        await JournalVouchClient.CreateAsync(createJournalVoucher);

        var journalVoucher = await AccountingDbContext.Set<JournalVoucherEntity>()
            .SingleAsync(x => x.JournalVoucherTitle == createJournalVoucher.JournalVoucherTitle && x.ReferenceNumber == createJournalVoucher.ReferenceNumber);

        journalVoucher.Should().NotBeNull();
        journalVoucher.JournalVoucherTitle.Should().Be(createJournalVoucher.JournalVoucherTitle);
        journalVoucher.ReferenceNumber.Should().Be(createJournalVoucher.ReferenceNumber);
        journalVoucher.TransactionTime.Should().Be(createJournalVoucher.TransactionTime);
        journalVoucher.CreatedByUserId.Should().Be(createJournalVoucher.CreatedByUserId);
        journalVoucher.FinancialPeriodId.Should().Be(createJournalVoucher.FinancialPeriodId);

        // Return Journal voucher
        return journalVoucher;
    }
}