using Application.Common;
using Application.Converters.Journal;
using Application.Extensions;
using Application.Features.Journal.Queries.JournalVoucher;
using Application.Repositories.Common;

namespace Application.Features.Journal.Queries.JournalEntry;

public class GetJournalEntryByIdQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<GetJournalEntryByIdQueryResult>> Handle(GetJournalEntryByIdQuery query, CancellationToken cancellationToken)
    {
        var validator = new GetJournalEntryByIdQueryValidator();
        var validateResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<GetJournalEntryByIdQueryResult>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var journalEntry = await unitOfWork.JournalEntryRepository.GetByIdAsync(query.Id, cancellationToken);

        if (journalEntry is null)
            return OperationResult<GetJournalEntryByIdQueryResult>.NotFoundResult(nameof(GetJournalVoucherByIdQuery.Id), "Journal entry not found.");

        return OperationResult<GetJournalEntryByIdQueryResult>.SuccessResult(journalEntry.ToDto());
    }
}