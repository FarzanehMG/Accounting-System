using Application.Common;
using Application.Converters.Journal;
using Application.Extensions;
using Application.Features.Journal.Queries.JournalEntry.Dtos;
using Application.Repositories.Common;

namespace Application.Features.Journal.Queries.JournalEntry;

public class GetJournalEntriesQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<IReadOnlyList<GetJournalEntriesQueryResult>>> Handle(GetJournalEntriesQuery query, CancellationToken cancellationToken)
    {
        var validator = new GetJournalEntriesQueryValidator();
        var validateResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<IReadOnlyList<GetJournalEntriesQueryResult>>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var journalEntryFilter = new JournalEntryFilter
        {
            DetailedAccountId = query.DetailedAccountId,
            SubLedgerAccountId = query.SubLedgerAccountId,
            Amount = query.Amount,
            JournalEntryType = query.JournalEntryType,
            JournalVoucherId = query.JournalVoucherId,
            JournalEntryNumber = query.JournalEntryNumber
        };
        var journalEntry = await unitOfWork.JournalEntryRepository.GetAllAsync(journalEntryFilter, cancellationToken);

        if (!journalEntry.Any())
            return OperationResult<IReadOnlyList<GetJournalEntriesQueryResult>>.NotFoundResult(nameof(GetJournalEntriesQueryResult), "Journal entry not found.");

        return OperationResult<IReadOnlyList<GetJournalEntriesQueryResult>>
            .SuccessResult(journalEntry.ToDto());
    }
}