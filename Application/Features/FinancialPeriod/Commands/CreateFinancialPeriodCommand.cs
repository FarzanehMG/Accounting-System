namespace Application.Features.FinancialPeriod.Commands;

public record CreateFinancialPeriodCommand
{
    public required string FinancialPeriodTitle { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
}