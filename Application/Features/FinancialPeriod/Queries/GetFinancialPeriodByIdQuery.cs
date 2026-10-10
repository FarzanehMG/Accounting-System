namespace Application.Features.FinancialPeriod.Queries;

public record GetFinancialPeriodByIdQuery
{
    public required Guid Id { get; set; }
}