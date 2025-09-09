namespace Application.Features.FinancialPeriod.Queries;

public record GetFinancialPeriodsQueryResult
{
    public string Title { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}