using API.Controllers.Common;
using API.Controllers.Models.FinancialPeriod;
using Application.Features.FinancialPeriod.Commands;
using Application.Features.FinancialPeriod.Queries;
using Application.Services.FinancialPeriod;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.FinancialPeriod;

[ApiController]
[Route("api/financial_periods")]
public class FinancialPeriodController(FinancialPeriodService service) : BaseController
{
    [HttpPost("Create")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CreateFinancialPeriodApiModel model, CancellationToken cancellationToken)
    {
        var command = new CreateFinancialPeriodCommand
        {
            FinancialPeriodTitle = model.FinancialPeriodTitle,
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };

        return OperationResult(await service.CreateAsync(command, cancellationToken));
    }

    [HttpPut("Update")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Update(UpdateFinancialPeriodCommand model, CancellationToken cancellationToken)
    {
        return OperationResult(await service.UpdateAsync(model, cancellationToken));
    }

    [HttpGet("FinancialPeriod")]
    [ProducesResponseType(typeof(ApiResult<GetFinancialPeriodByIdQueryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetFinancialPeriodByIdQuery
        {
            Id = id
        };

        return OperationResult(await service.GetByIdAsync(query, cancellationToken));
    }

    [HttpGet("FinancialPeriods")]
    [ProducesResponseType(typeof(ApiResult<List<GetFinancialPeriodsQueryResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FilterFinancialPeriodApiModel model, CancellationToken cancellationToken)
    {
        var query = new GetFinancialPeriodsQuery
        {
            Title = model.Title,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            IsActive = model.IsActive
        };

        return OperationResult(await service.GetAllAsync(query, cancellationToken));
    }
}