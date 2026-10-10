using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace API.Controllers.Common;

public record ApiResult(bool IsSuccess, string Message, ApiResultStatusCode StatusCode)
{
    public string RequestId => Activity.Current?.TraceId.ToHexString() ?? string.Empty;
}

public record ApiResult<TResult>(
    bool IsSuccess,
    string Message,
    ApiResultStatusCode StatusCode,
    TResult Data
) : ApiResult(IsSuccess, Message, StatusCode)
{
    public Dictionary<string, string>? Errors { get; init; }
}


public enum ApiResultStatusCode
{
    [Display(Name = "Success")]
    Ok = 200,

    [Display(Name = "NotFound")]
    NotFound = 404,

    [Display(Name = "BadRequest")]
    BadRequest = 400,

    [Display(Name = "ServerError")]
    ServerError = 500,

    [Display(Name = "Forbidden")]
    Forbidden = 403,

    [Display(Name = "Unauthorized")]
    Unauthorized = 401,
}