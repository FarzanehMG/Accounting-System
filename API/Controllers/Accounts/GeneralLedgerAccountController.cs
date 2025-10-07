using API.Controllers.Common;
using API.Controllers.Models.Accounts;
using Application.Features.Accounts.Commands.GeneralLedgerAccount;
using Application.Features.Accounts.Queries.GeneralLedgerAccount;
using Application.Services.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounts;

[ApiController]
[Route("api/generalLedger_account")]
public class GeneralLedgerAccountController(GeneralLedgerAccountService service) : BaseController
{
    [HttpPost("Create")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CreateGeneralLedgerAccountApiModel model, CancellationToken cancellationToken)
    {
        var command = new CreateGeneralLedgerAccountCommand
        {
            Title = model.Title,
            Code = model.Code,
            JournalEntryType = model.JournalEntryType,
            IsActive = model.IsActive,
            Description = model.Description,
            GroupAccountId = model.GroupAccountId
        };

        return OperationResult(await service.CreateAsync(command, cancellationToken));
    }

    [HttpPut("Update")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Update(UpdateGeneralLedgerAccountCommand model, CancellationToken cancellationToken)
    {
        return OperationResult(await service.UpdateAsync(model, cancellationToken));
    }

    [HttpGet("GeneralLedgerAccount")]
    [ProducesResponseType(typeof(ApiResult<GetGeneralLedgerAccountByIdQueryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetGeneralLedgerAccountByIdQuery
        {
            Id = id
        };

        return OperationResult(await service.GetByIdAsync(query, cancellationToken));
    }

    [HttpGet("GeneralLedgerAccounts")]
    [ProducesResponseType(typeof(ApiResult<List<GetGeneralLedgerAccountsQueryResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(FilterGeneralLedgerAccountApiModel model, CancellationToken cancellationToken)
    {
        var query = new GetGeneralLedgerAccountsQuery
        {
            Title = model.Title,
            Code = model.Code,
            JournalEntryType = model.JournalEntryType,
            IsActive = model.IsActive,
            GroupAccountId = model.GroupAccountId
        };

        return OperationResult(await service.GetAllAsync(query, cancellationToken));
    }
}