namespace Application.Features.FinancialPeriod.Queries.Dtos;

public record FinancialPeriodFilter
{
    public string? Title { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}