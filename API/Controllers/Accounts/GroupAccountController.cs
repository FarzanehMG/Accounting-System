using API.Controllers.Common;
using Application.Features.Accounts.Commands.GroupAccount;
using Application.Services.Accounts;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Accounts.Queries.GroupAccount;
using API.Controllers.Models.Accounts;

namespace API.Controllers.Accounts;

[ApiController]
[Route("api/group_accounts")]
public class GroupAccountController(GroupAccountService service) : BaseController
{
    [HttpPost("Create")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CreateGroupAccountApiModel model, CancellationToken cancellationToken)
    {
        var command = new CreateGroupAccountCommand
        {
            Title = model.Title,
            Code = model.Code,
            JournalEntryType = model.JournalEntryType,
            IsActive = model.IsActive,
            Description = model.Description
        };

        return OperationResult(await service.CreateAsync(command, cancellationToken));
    }

    [HttpPut("Update")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Update(UpdateGroupAccountCommand model, CancellationToken cancellationToken)
    {
        return OperationResult(await service.UpdateAsync(model, cancellationToken));
    }

    [HttpGet("GroupAccount")]
    [ProducesResponseType(typeof(ApiResult<GetGroupAccountByIdQueryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetGroupAccountByIdQuery
        {
            Id = id 
        };

        return OperationResult(await service.GetByIdAsync(query, cancellationToken));
    }

    [HttpGet("GroupAccounts")]
    [ProducesResponseType(typeof(ApiResult<List<GetGroupAccountsQueryResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(FilterGroupAccountApiModel model, CancellationToken cancellationToken)
    {
        var query = new GetGroupAccountsQuery
        {
            Title = model.Title,
            Code = model.Code,
            JournalEntryType = model.JournalEntryType,
            IsActive = model.IsActive
        };

        return OperationResult(await service.GetAllAsync(query, cancellationToken));
    }
}