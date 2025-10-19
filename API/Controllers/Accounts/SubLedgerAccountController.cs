using API.Controllers.Common;
using API.Controllers.Models.Accounts;
using Application.Features.Accounts.Commands.SubLedgerAccount;
using Application.Features.Accounts.Queries.SubLedgerAccount;
using Application.Services.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounts;

[ApiController]
[Route("api/sub-ledger-accounts")]
public class SubLedgerAccountController(SubLedgerAccountService service) : BaseController
{
    [HttpPost("Create")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CreateSubLedgerAccountApiModel model, CancellationToken cancellationToken)
    {
        var command = new CreateSubLedgerAccountCommand
        {
            Title = model.Title,
            Code = model.Code,
            JournalEntryType = model.JournalEntryType,
            IsActive = model.IsActive,
            Description = model.Description,
            GeneralLedgerAccountId = model.GeneralLedgerAccountId
        };

        return OperationResult(await service.CreateAsync(command, cancellationToken));
    }

    [HttpPut("Update")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Update(UpdateSubLedgerAccountCommand model, CancellationToken cancellationToken)
    {
        return OperationResult(await service.UpdateAsync(model, cancellationToken));
    }

    [HttpGet("SubLedgerAccount")]
    [ProducesResponseType(typeof(ApiResult<GetSubLedgerAccountByIdQueryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSubLedgerAccountByIdQuery
        {
            Id = id
        };

        return OperationResult(await service.GetByIdAsync(query, cancellationToken));
    }

    [HttpGet("SubLedgerAccounts")]
    [ProducesResponseType(typeof(ApiResult<List<GetSubLedgerAccountsQueryResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FilterSubLedgerAccountApiModel model, CancellationToken cancellationToken)
    {
        var query = new GetSubLedgerAccountsQuery
        {
            Title = model.Title,
            Code = model.Code,
            JournalEntryType = model.JournalEntryType,
            IsActive = model.IsActive,
            GeneralLedgerAccountId = model.GeneralLedgerAccountId
        };

        return OperationResult(await service.GetAllAsync(query, cancellationToken));
    }
}