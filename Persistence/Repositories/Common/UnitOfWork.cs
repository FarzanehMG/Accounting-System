using Application.Repositories.AccountRepositories;
using Application.Repositories.Attachment;
using Application.Repositories.Common;
using Application.Repositories.FinancialPeriod;
using Application.Repositories.JournalRepositories;
using Persistence.Repositories.Accounts;
using Persistence.Repositories.Attachment;
using Persistence.Repositories.FinancialPeriod;
using Persistence.Repositories.Journal;

namespace Persistence.Repositories.Common;

public class UnitOfWork(AccountingDbContext dbContext) : IUnitOfWork
{
    public void Dispose()
    {
        dbContext.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await dbContext.DisposeAsync();
    }

    public IGroupAccountRepository GroupAccountRepository { get; } = new GroupAccountRepository(dbContext);
    public IGeneralLedgerAccountRepository GeneralLedgerAccountRepository { get; } = new GeneralLedgerAccountRepository(dbContext);
    public ISubLedgerAccountRepository SubLedgerAccountRepository { get; } = new SubLedgerAccountRepository(dbContext);
    public IDetailedAccountRepository DetailedAccountRepository { get; } = new DetailedAccountRepository(dbContext);

    public IFinancialPeriodRepository FinancialPeriodRepository { get; } = new FinancialPeriodRepository(dbContext);

    public IJournalVoucherRepository JournalVoucherRepository { get; } = new JournalVoucherRepository(dbContext);
    public IJournalEntryRepository JournalEntryRepository { get; } = new JournalEntryRepository(dbContext);
    public IAttachmentRepository AttachmentRepository { get; } = new AttachmentRepository(dbContext);


    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}