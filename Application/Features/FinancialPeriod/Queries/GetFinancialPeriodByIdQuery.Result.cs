namespace Application.Features.FinancialPeriod.Queries;

public record GetFinancialPeriodByIdQueryResult
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required bool IsActive { get; init; }
}