using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Common;

public class BaseController : ControllerBase
{
    protected IActionResult OperationResult<TModel>(OperationResult<TModel> result)
    {
        if (result.IsSuccess)
            return result.Result is bool ? Ok() : Ok(result.Result);

        if (result.IsNotFound)
        {
            AddErrors(result);

            var errors = new ValidationProblemDetails(ModelState);

            return NotFound(errors.Errors);
        }

        AddErrors(result);

        var badRequestErrors = new ValidationProblemDetails(ModelState);

        return BadRequest(badRequestErrors);
    }

    private void AddErrors<TModel>(OperationResult<TModel> result)
    {
        foreach (var errorMessage in result.ErrorMessages)
        {
            ModelState.AddModelError(errorMessage.Key, errorMessage.Value);
        }
    }
}

