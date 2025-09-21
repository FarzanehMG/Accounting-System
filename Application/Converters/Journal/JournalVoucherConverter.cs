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

    public static IReadOnlyList<GetJournalVouchersQueryResult> ToDto(this IReadOnlyList<JournalVoucherEntity> entity)
    {
        return entity.Select(x => new GetJournalVouchersQueryResult
            {
                Id = x.Id,
                JournalVoucherTitle = x.JournalVoucherTitle,
                ReferenceNumber = x.ReferenceNumber,
                TransactionTime = x.TransactionTime,
                JournalVoucherState = x.JournalVoucherState,
                CreatedByUserId = x.CreatedByUserId,
                FinancialPeriodId = x.FinancialPeriodId,
                Description = x.Description,
                JournalVoucherNumber = x.JournalVoucherNumber,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedTime = x.ApprovedTime
            }).ToList()
            .AsReadOnly();
    }
}