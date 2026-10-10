using API.Controllers.Common;
using API.Controllers.Models.Accounts;
using Application.Features.Accounts.Commands.DetailedAccount;
using Application.Features.Accounts.Queries.DetailedAccount;
using Application.Services.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounts;

[ApiController]
[Route("api/detailed-accounts")]
public class DetailedAccountController(DetailedAccountService service) : BaseController
{
    [HttpPost("Create")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CreateDetailedAccountApiModel model, CancellationToken cancellationToken)
    {
        var command = new CreateDetailedAccountCommand
        {
            Title = model.Title,
            Code = model.Code,
            JournalEntryType = model.JournalEntryType,
            IsActive = model.IsActive,
            Description = model.Description,
            SubLedgerAccountId = model.SubLedgerAccountId
        };

        return OperationResult(await service.CreateAsync(command, cancellationToken));
    }

    [HttpPut("Update")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Update(UpdateDetailedAccountCommand model, CancellationToken cancellationToken)
    {
        return OperationResult(await service.UpdateAsync(model, cancellationToken));
    }

    [HttpGet("DetailedAccount")]
    [ProducesResponseType(typeof(ApiResult<GetDetailedAccountByIdQueryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetDetailedAccountByIdQuery
        {
            Id = id
        };

        return OperationResult(await service.GetByIdAsync(query, cancellationToken));
    }

    [HttpGet("DetailedAccounts")]
    [ProducesResponseType(typeof(ApiResult<List<GetDetailedAccountsQueryResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FilterDetailedAccountApiModel model, CancellationToken cancellationToken)
    {
        var query = new GetDetailedAccountsQuery
        {
            Title = model.Title,
            Code = model.Code,
            JournalEntryType = model.JournalEntryType,
            IsActive = model.IsActive,
            SubLedgerAccountId = model.SubLedgerAccountId
        };

        return OperationResult(await service.GetAllAsync(query, cancellationToken));
    }
}