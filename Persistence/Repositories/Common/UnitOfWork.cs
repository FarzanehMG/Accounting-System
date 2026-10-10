using Application.Repositories.AccountRepositories;
using Application.Repositories.Attachment;
using Application.Repositories.Balance;
using Application.Repositories.Common;
using Application.Repositories.FinancialPeriod;
using Application.Repositories.JournalRepositories;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Repositories.Accounts;
using Persistence.Repositories.Attachment;
using Persistence.Repositories.Balance;
using Persistence.Repositories.FinancialPeriod;
using Persistence.Repositories.Journal;

namespace Persistence.Repositories.Common;

public class UnitOfWork(AccountingDbContext dbContext) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

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

    public IAccountBalanceRepository AccountBalanceRepository { get; } = new AccountBalanceRepository(dbContext);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null) 
            return;

        _transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            if (_transaction is not null) 
                await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await ReleaseTransaction();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null) 
            await _transaction.RollbackAsync(cancellationToken);

        await ReleaseTransaction();
    }

    private async Task ReleaseTransaction()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<AccountingDbContext, T>(
            dbContext,
            async (db, _, execCancellationToken) =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync(execCancellationToken);

                try
                {
                    var result = await operation();

                    // Ensure changes are persisted within the same transaction
                    await db.SaveChangesAsync(execCancellationToken);

                    await transaction.CommitAsync(execCancellationToken);

                    return result;
                }
                catch
                {
                    try
                    {
                        await transaction.RollbackAsync(execCancellationToken);
                    }
                    catch
                    {
                        // Swallow rollback exceptions - higher level should log
                    }

                    throw;
                }
            },
            null,
            cancellationToken);
    }
}