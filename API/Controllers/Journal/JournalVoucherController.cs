using API.Controllers.Common;
using API.Controllers.Models.Journal;
using Application.Features.Journal.Commands.JournalVoucher;
using Application.Features.Journal.Queries.JournalVoucher;
using Application.Services.Journal;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Journal;

[ApiController]
[Route("api/journal_vouchers")]
public class JournalVoucherController(JournalVoucherService service) : BaseController
{
    [HttpPost("Create")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CreateJournalVoucherApiModel model, CancellationToken cancellationToken)
    {
        var command = new CreateJournalVoucherCommand
        {
            JournalVoucherTitle = model.JournalVoucherTitle,
            ReferenceNumber = model.ReferenceNumber,
            TransactionTime = model.TransactionTime,
            CreatedByUserId = model.CreatedByUserId,
            FinancialPeriodId = model.FinancialPeriodId,
            Description = model.Description
        };

        return OperationResult(await service.CreateAsync(command, cancellationToken));
    }

    [HttpPut("Update")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Update(UpdateJournalVoucherCommand model, CancellationToken cancellationToken)
    {
        return OperationResult(await service.UpdateAsync(model, cancellationToken));
    }

    [HttpGet("JournalVoucher")]
    [ProducesResponseType(typeof(ApiResult<GetJournalVoucherByIdQueryResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetJournalVoucherByIdQuery
        {
            Id = id
        };

        return OperationResult(await service.GetByIdAsync(query, cancellationToken));
    }

    [HttpGet("journalVouchers")]
    [ProducesResponseType(typeof(ApiResult<List<GetJournalVouchersQueryResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(FilterJournalVoucherApiModel model, CancellationToken cancellationToken)
    {
        var query = new GetJournalVouchersQuery
        {
            FinancialPeriodId = model.FinancialPeriodId,
            JournalVoucherState = model.JournalVoucherState,
            ApprovedByUserId = model.ApprovedByUserId,
            ApprovedTime = model.ApprovedTime,
            CreatedByUserId = model.CreatedByUserId,
            JournalVoucherNumber = model.JournalVoucherNumber,
            JournalVoucherTitle = model.JournalVoucherTitle,
            TransactionTime = model.TransactionTime,
            ReferenceNumber = model.ReferenceNumber
        };

        return OperationResult(await service.GetAllAsync(query, cancellationToken));
    }
}