using Application.Common;
using Application.Extensions;
using Application.Repositories.Common;

namespace Application.Features.Journal.Queries.JournalVoucher;

public class GetJournalVoucherByIdQueryHandler(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<GetJournalVoucherByIdQueryResult>> Handle(GetJournalVoucherByIdQuery query, CancellationToken cancellationToken)
    {
        var validator = new GetJournalVoucherByIdQueryValidator();
        var validateResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validateResult.IsValid)
            return OperationResult<GetJournalVoucherByIdQueryResult>.FailureResult(validateResult.Errors.ConvertToKeyValuePair());

        var journalVoucher = await unitOfWork.JournalVoucherRepository.GetByIdAsync(query.Id, cancellationToken);

        if (journalVoucher is null)
            return OperationResult<GetJournalVoucherByIdQueryResult>.NotFoundResult(nameof(GetJournalVoucherByIdQuery.Id), "Journal voucher not found.");

        return OperationResult<GetJournalVoucherByIdQueryResult>.SuccessResult(new GetJournalVoucherByIdQueryResult
        {
            Id = journalVoucher.Id,
            JournalVoucherTitle = journalVoucher.JournalVoucherTitle,
            JournalVoucherNumber = journalVoucher.JournalVoucherNumber,
            ReferenceNumber = journalVoucher.ReferenceNumber,
            TransactionTime = journalVoucher.TransactionTime,
            JournalVoucherState = journalVoucher.JournalVoucherState,
            CreatedByUserId = journalVoucher.CreatedByUserId,
            FinancialPeriodId = journalVoucher.FinancialPeriodId,
            ApprovedByUserId = journalVoucher.ApprovedByUserId,
            ApprovedTime = journalVoucher.ApprovedTime,
            Description = journalVoucher.Description
        });
    }
}