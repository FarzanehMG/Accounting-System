namespace API.Controllers.Models.FinancialPeriod;

public record FilterFinancialPeriodApiModel
{
    public string? Title { get; init; }
    public bool? IsActive { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}