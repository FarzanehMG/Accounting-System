using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Common;

public class BaseController : ControllerBase
{
    protected IActionResult OperationResult<TModel>(OperationResult<TModel> result)
    {
        if (result.IsSuccess)
        {
            var apiResult = new ApiResult<TModel>(
                IsSuccess: true,
                Message: "Success",
                StatusCode: ApiResultStatusCode.Ok,
                Data: result.Result!
            );

            return Ok(apiResult);
        }

        // For NotFound
        if (result.IsNotFound)
        {
            var message = result.ErrorMessages.FirstOrDefault().Value ?? "Not Found";

            var apiResult = new ApiResult<TModel>(
                IsSuccess: false,
                Message: message,
                StatusCode: ApiResultStatusCode.NotFound,
                Data: default!
            );

            return NotFound(apiResult);
        }

        // For validation or other errors
        var errorMessage = result.ErrorMessages.FirstOrDefault().Value ?? "Validation failed";

        var badRequestResult = new ApiResult<TModel>(
            IsSuccess: false,
            Message: errorMessage,
            StatusCode: ApiResultStatusCode.BadRequest,
            Data: default!
        );

        return BadRequest(badRequestResult);
    }

    private void AddErrors<TModel>(OperationResult<TModel> result)
    {
        foreach (var errorMessage in result.ErrorMessages)
        {
            ModelState.AddModelError(errorMessage.Key, errorMessage.Value);
        }
    }
}

