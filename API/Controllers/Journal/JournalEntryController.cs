using API.Controllers.Common;
using API.Controllers.Models.Journal;
using Application.Features.Journal.Commands.JournalEntry;
using Application.Features.Journal.Queries.JournalEntry;
using Application.Services.Journal;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Journal;

[ApiController]
[Route("api/journal_entries")]
public class JournalEntryController(JournalEntryService service) : BaseController
{
    [HttpPost("Create")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CreateJournalEntryApiModel model, CancellationToken cancellationToken)
    {
        var commands = model.Entries.Select(e => new CreateJournalEntryCommand
        {
            JournalEntryType = e.JournalEntryType,
            Amount = e.Amount,
            SubLedgerAccountId = e.SubLedgerAccountId,
            JournalVoucherId = e.JournalVoucherId,
            DetailedAccountId = e.DetailedAccountId,
            Description = e.Description,
            Attachments = e.Attachments
        }).ToList();

        return OperationResult(await service.CreateAsync(commands, cancellationToken));
    }

    [HttpPut("Update")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Update(UpdateJournalEntryCommand model, CancellationToken cancellationToken)
    {
        return OperationResult(await service.UpdateAsync(model, cancellationToken));
    }

    [HttpGet("JournalEntry")]
    [ProducesResponseType(typeof(ApiResult<GetJournalEntryByIdQueryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetJournalEntryByIdQuery
        {
            Id = id
        };

        return OperationResult(await service.GetByIdAsync(query, cancellationToken));
    }

    [HttpGet("journalEntries")]
    [ProducesResponseType(typeof(ApiResult<List<GetJournalEntriesQueryResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FilterJournalEntriesApiModel model, CancellationToken cancellationToken)
    {
        var query = new GetJournalEntriesQuery
        {
            DetailedAccountId = model.DetailedAccountId,
            SubLedgerAccountId = model.SubLedgerAccountId,
            Amount = model.Amount,
            JournalEntryType = model.JournalEntryType,
            JournalEntryNumber = model.JournalEntryNumber,
            JournalVoucherId = model.JournalVoucherId
        };

        return OperationResult(await service.GetAllAsync(query, cancellationToken));
    }
}