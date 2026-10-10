using Application.Common;
using Xunit.Abstractions;

namespace Application.Tests.Extensions;

public static class ApplicationTestExtensions
{
    public static void WriteLineOperationResultErrors<TResult>(this ITestOutputHelper testOutputHelper,
        OperationResult<TResult> operationResult)
    {
        foreach (var error in operationResult.ErrorMessages)
        {
            testOutputHelper.WriteLine($"Property Name:{error.Key} Message:{error.Value}");
        }
    }
}