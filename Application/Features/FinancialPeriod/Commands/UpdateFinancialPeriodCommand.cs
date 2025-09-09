using Application.Common;

namespace Application.Features.FinancialPeriod.Commands;

public record UpdateFinancialPeriodCommand
{
    public required Guid FinancialPeriodId { get; init; }
    public Patch<string>? FinancialPeriodTitle { get; init; }
    public Patch<DateTime>? StartDate { get; init; }
    public Patch<DateTime>? EndDate { get; init; }
}