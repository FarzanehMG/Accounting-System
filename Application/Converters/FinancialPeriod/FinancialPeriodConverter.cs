using Application.Features.FinancialPeriod.Queries;
using Domain.Entities.FinancialPeriod;

namespace Application.Converters.FinancialPeriod;

public static class FinancialPeriodConverter
{
    public static GetFinancialPeriodByIdQueryResult ToDto(this FinancialPeriodEntity entity)
    {
        return new GetFinancialPeriodByIdQueryResult
        {
            Id = entity.Id,
            Title = entity.FinancialPeriodTitle,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsActive = entity.IsActive
        };
    }

    public static IReadOnlyList<GetFinancialPeriodsQueryResult> ToDto(this IReadOnlyList<FinancialPeriodEntity> entity)
    {
        return entity.Select(x => new GetFinancialPeriodsQueryResult
            {
                Title = x.FinancialPeriodTitle,
                IsActive = x.IsActive,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            }).ToList()
            .AsReadOnly();
    }
}