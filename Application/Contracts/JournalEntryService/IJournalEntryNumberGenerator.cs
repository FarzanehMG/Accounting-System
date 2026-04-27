namespace Application.Contracts.JournalEntryService;

public interface IJournalEntryNumberGenerator
{
    Task<int> GenerateAsync(Guid voucherId, CancellationToken cancellationToken);
}