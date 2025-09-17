using Application.Features.Journal.Queries.JournalVoucher;
using Domain.Entities;

namespace Application.Converters.Journal;

public static class JournalVoucherConverter
{
    public static GetJournalVoucherByIdQueryResult ToDto(this JournalVoucherEntity entity)
    {
        return new GetJournalVoucherByIdQueryResult
        {
            Id = entity.Id,
            JournalVoucherTitle = entity.JournalVoucherTitle,
            JournalVoucherNumber = entity.JournalVoucherNumber,
            ReferenceNumber = entity.ReferenceNumber,
            TransactionTime = entity.TransactionTime,
            JournalVoucherState = entity.JournalVoucherState,
            CreatedByUserId = entity.CreatedByUserId,
            FinancialPeriodId = entity.FinancialPeriodId,
            ApprovedByUserId = entity.ApprovedByUserId,
            ApprovedTime = entity.ApprovedTime,
            Description = entity.Description
        };
    }
}