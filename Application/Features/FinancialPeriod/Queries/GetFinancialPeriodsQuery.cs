namespace Application.Features.FinancialPeriod.Queries;

public record GetFinancialPeriodsQuery
{
    public string? Title { get; init; }
    public bool? IsActive { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}