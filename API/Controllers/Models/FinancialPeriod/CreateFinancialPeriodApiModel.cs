namespace API.Controllers.Models.FinancialPeriod;

public record CreateFinancialPeriodApiModel
{
    public required string FinancialPeriodTitle { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
}