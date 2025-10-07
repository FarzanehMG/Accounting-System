using Application.Common;
using Application.Features.Journal.Commands.JournalEntry;
using Application.Features.Journal.Queries.JournalEntry;

namespace Application.Services.Journal;

public sealed class JournalEntryService(
    CreateJournalEntryCommandHandler createHandler,
    UpdateJournalEntryCommandHandler updateHandler,
    GetJournalEntryByIdQueryHandler getByIdHandler,
    GetJournalEntriesQueryHandler getAllHandler)
{
    public Task<OperationResult<bool>> CreateAsync(List<CreateJournalEntryCommand> command, CancellationToken cancellationToken = default)
    {
        return createHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<bool>> UpdateAsync(UpdateJournalEntryCommand command, CancellationToken cancellationToken = default)
    {
        return updateHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<GetJournalEntryByIdQueryResult>> GetByIdAsync(GetJournalEntryByIdQuery query, CancellationToken cancellationToken = default)
    {
        return getByIdHandler.Handle(query, cancellationToken);
    }

    public Task<OperationResult<IReadOnlyList<GetJournalEntriesQueryResult>>> GetAllAsync(GetJournalEntriesQuery query, CancellationToken cancellationToken = default)
    {
        return getAllHandler.Handle(query, cancellationToken);
    }
}