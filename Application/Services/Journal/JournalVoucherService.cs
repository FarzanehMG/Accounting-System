using Application.Common;
using Application.Features.Journal.Commands.JournalVoucher;
using Application.Features.Journal.Queries.JournalVoucher;

namespace Application.Services.Journal;

public sealed class JournalVoucherService(
    CreateJournalVoucherCommandHandler createHandler,
    UpdateJournalVoucherCommandHandler updateHandler,
    GetJournalVoucherByIdQueryHandler getByIdHandler,
    GetJournalVouchersQueryHandler getAllHandler)
{
    public Task<OperationResult<bool>> CreateAsync(CreateJournalVoucherCommand command, CancellationToken cancellationToken = default)
    {
        return createHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<bool>> UpdateAsync(UpdateJournalVoucherCommand command, CancellationToken cancellationToken = default)
    {
        return updateHandler.Handle(command, cancellationToken);
    }

    public Task<OperationResult<GetJournalVoucherByIdQueryResult>> GetByIdAsync(GetJournalVoucherByIdQuery query, CancellationToken cancellationToken = default)
    {
        return getByIdHandler.Handle(query, cancellationToken);
    }

    public Task<OperationResult<IReadOnlyList<GetJournalVouchersQueryResult>>> GetAllAsync(GetJournalVouchersQuery query, CancellationToken cancellationToken = default)
    {
        return getAllHandler.Handle(query, cancellationToken);
    }
}