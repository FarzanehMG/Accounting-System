using Application.Common;
using Application.Extensions;
using Application.Features.Journal.Commands.JournalEntry;
using Application.Features.Journal.Commands.JournalVoucher;
using Application.Repositories.Common;
using Domain.Entities.Balance;
using Domain.Enums;

namespace Application.Features.FinancialPeriod.Commands;

public class CloseFinancialPeriodCommandHandler(IUnitOfWork unitOfWork, 
    CreateFinancialPeriodCommandHandler createHandler, 
    CreateJournalVoucherCommandHandler createJournalVoucherHandler, 
    CreateJournalEntryCommandHandler createJournalEntryHandler)
{
    public async Task<OperationResult<bool>> Handle(CloseFinancialPeriodCommand command, CancellationToken cancellationToken)
    {
        var validator = new CloseFinancialPeriodCommandValidator();
        var validateResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<bool>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        // Retrieve current financial period
        var financialPeriod = await unitOfWork.FinancialPeriodRepository.GetByIdAsync(command.FinancialPeriodId, cancellationToken);
        if (financialPeriod is null)
            return OperationResult<bool>.NotFoundResult("FinancialPeriod", "Financial period Not Found.");

        if (!financialPeriod.IsActive)
            return OperationResult<bool>.FailureResult("FinancialPeriod", "Only an active, non-closed financial period can be closed.");

        // Business Rule: ensure no unPosted(draft) transactions exist
        var hasUnPosted = await unitOfWork.JournalVoucherRepository.HasUnPostedTransactionsAsync(financialPeriod.Id, cancellationToken);
        if (hasUnPosted)
            return OperationResult<bool>.FailureResult("Transactions", "There are unPosted(draft) journal entries in this period.");

        // Business Rule: Compute closing balances per account
        var journalVouchers = await unitOfWork.JournalVoucherRepository.GetByFinancialPeriodAsync(financialPeriod.Id, JournalVoucherState.Approved, cancellationToken);

        var accountBalances = journalVouchers
            .SelectMany(v => v.JournalEntries) // flatten all entries
            .GroupBy(e => e.SubLedgerAccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                TotalDebit = g.Where(x => x.JournalEntryType == JournalEntryType.Debit).Sum(x => x.Amount),
                TotalCredit = g.Where(x => x.JournalEntryType == JournalEntryType.Credit).Sum(x => x.Amount),
                ClosingBalance = g.Where(x => x.JournalEntryType == JournalEntryType.Debit).Sum(x => x.Amount)
                                 - g.Where(x => x.JournalEntryType == JournalEntryType.Credit).Sum(x => x.Amount)
            }).ToList();

        // Persist balances
        foreach (var accountBalance in accountBalances)
        {
            var balanceEntity = new AccountBalanceEntity
            {
                AccountId = accountBalance.AccountId,
                FinancialPeriodId = financialPeriod.Id,
                TotalDebit = accountBalance.TotalDebit,
                TotalCredit = accountBalance.TotalCredit,
                ClosingBalance = accountBalance.ClosingBalance,
                CreatedDate = DateTime.UtcNow
            };
            await unitOfWork.AccountBalanceRepository.CreateAsync(balanceEntity, cancellationToken);
        }

        // Close the current period
        financialPeriod.IsActive = false;
        financialPeriod.ClosedBy = command.RequestedByUserId;
        financialPeriod.ClosedDate = DateTime.UtcNow;

        await unitOfWork.FinancialPeriodRepository.UpdateAsync(financialPeriod, cancellationToken);

        var createFinancialPeriodCommand = new CreateFinancialPeriodCommand
        {
            FinancialPeriodTitle = command.NewPeriodTitle,
            StartDate = command.NewStartDate,
            EndDate = command.NewEndDate
        };

        await createHandler.Handle(createFinancialPeriodCommand, cancellationToken);

        // Create opening journal voucher and entries in the new period based on closing balances
        var createJournalVoucherCommand = new CreateJournalVoucherCommand
        {
            JournalVoucherTitle = $"Opening Balances for {createFinancialPeriodCommand.FinancialPeriodTitle}",
            ReferenceNumber = $"OPEN-{createFinancialPeriodCommand.FinancialPeriodTitle}",
            TransactionTime = createFinancialPeriodCommand.StartDate,
            FinancialPeriodId = newPeriod.Id,
            CreatedByUserId = systemUserId, // system or admin ID
            JournalVoucherState = JournalVoucherState.Approved, // approved since it’s system-generated
            Description = $"Opening entries transferred from {financialPeriod.FinancialPeriodTitle}"
        };
        var voucherResult = await createJournalVoucherHandler.Handle(createJournalVoucherCommand, cancellationToken);

        if (!voucherResult.IsSuccess)
            return OperationResult<bool>.FailureResult("JournalVoucher", "Failed to create opening journal voucher.");

        var openingEntries = new List<CreateJournalEntryCommand>();

        foreach (var accountBalance in accountBalances)
        {
            if (accountBalance.ClosingBalance == 0)
                continue;

            openingEntries.Add(new CreateJournalEntryCommand
            {
                JournalEntryType = accountBalance.ClosingBalance > 0 ? JournalEntryType.Debit : JournalEntryType.Credit,
                Amount = Math.Abs(accountBalance.ClosingBalance),
                SubLedgerAccountId = accountBalance.AccountId,
                JournalVoucherId = voucherResult.JournalVoucherId, // from the created voucher
                Description = $"Opening balance carry forward from {financialPeriod.FinancialPeriodTitle}",
                Attachments = null
            });
        }

        if (openingEntries.Any())
        {
            var entriesResult = await createJournalEntryHandler.Handle(openingEntries, cancellationToken);

            if (!entriesResult.IsSuccess)
                return OperationResult<bool>.FailureResult("JournalEntries", "Failed to create opening entries.");
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<bool>.SuccessResult(true);
    }
}