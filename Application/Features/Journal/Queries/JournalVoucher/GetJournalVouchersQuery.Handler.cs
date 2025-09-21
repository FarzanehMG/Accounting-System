using Application.Common;
using Application.Converters.Journal;
using Application.Extensions;
using Application.Features.Journal.Queries.JournalVoucher.Dtos;
using Application.Repositories.Common;

namespace Application.Features.Journal.Queries.JournalVoucher;

public class GetJournalVouchersQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<IReadOnlyList<GetJournalVouchersQueryResult>>> Handle(GetJournalVouchersQuery query, CancellationToken cancellationToken)
    {
        var validator = new GetJournalVouchersQueryValidator();
        var validateResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<IReadOnlyList<GetJournalVouchersQueryResult>>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var journalVoucherFilter = new JournalVoucherFilter
        {
            JournalVoucherTitle = query.JournalVoucherTitle,
            JournalVoucherState = query.JournalVoucherState,
            TransactionTime = query.TransactionTime,
            ReferenceNumber = query.ReferenceNumber,
            FinancialPeriodId = query.FinancialPeriodId,
            ApprovedByUserId = query.ApprovedByUserId,
            ApprovedTime = query.ApprovedTime,
            CreatedByUserId = query.CreatedByUserId,
            JournalVoucherNumber = query.JournalVoucherNumber
        };
        var journalVoucher = await unitOfWork.JournalVoucherRepository.GetAllAsync(journalVoucherFilter, cancellationToken);

        if (!journalVoucher.Any())
            return OperationResult<IReadOnlyList<GetJournalVouchersQueryResult>>.NotFoundResult(nameof(GetJournalVouchersQueryResult), "Journal voucher not found.");

        return OperationResult<IReadOnlyList<GetJournalVouchersQueryResult>>
            .SuccessResult(journalVoucher.ToDto());
    }
}