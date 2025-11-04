namespace Application.Features.FinancialPeriod.Commands;

public record CloseFinancialPeriodCommand
{
    public required Guid FinancialPeriodId { get; init; }
    public required string NewPeriodTitle { get; init; }
    public required DateTime NewStartDate { get; init; }
    public required DateTime NewEndDate { get; init; }
    public required Guid RequestedByUserId { get; init; }
}