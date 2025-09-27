using Application.Repositories.AccountRepositories;
using Application.Repositories.Common;
using Application.Repositories.FinancialPeriod;
using Application.Repositories.JournalRepositories;
using Infrastructure.Repositories.Accounts;
using Infrastructure.Repositories.FinancialPeriod;
using Infrastructure.Repositories.Journal;

namespace Infrastructure.Repositories.Common;

public class UnitOfWork : IUnitOfWork
{
    private readonly AccountingDbContext _dbContext;


    public UnitOfWork(AccountingDbContext dbContext)
    {
        _dbContext = dbContext;
        FinancialPeriodRepository = new FinancialPeriodRepository(dbContext);
        JournalVoucherRepository = new JournalVoucherRepository(dbContext);
        JournalEntryRepository = new JournalEntryRepository(dbContext);
        GroupAccountRepository = new GroupAccountRepository(dbContext);
        GeneralLedgerAccountRepository = new GeneralLedgerAccountRepository(dbContext);
        SubLedgerAccountRepository = new SubLedgerAccountRepository(dbContext);
        DetailedAccountRepository = new DetailedAccountRepository(dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    public IGroupAccountRepository GroupAccountRepository { get; }
    public IGeneralLedgerAccountRepository GeneralLedgerAccountRepository { get; }
    public ISubLedgerAccountRepository SubLedgerAccountRepository { get; }
    public IDetailedAccountRepository DetailedAccountRepository { get; }

    public IFinancialPeriodRepository FinancialPeriodRepository { get; }

    public IJournalVoucherRepository JournalVoucherRepository { get; }
    public IJournalEntryRepository JournalEntryRepository { get; }


    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}