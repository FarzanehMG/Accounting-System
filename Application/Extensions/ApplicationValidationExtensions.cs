using System.Diagnostics.CodeAnalysis;
using FluentValidation.Results;

namespace Application.Extensions;

public static class ApplicationValidationExtensions
{
    public static List<KeyValuePair<string, string>> ConvertToKeyValuePair(
        [NotNull] this List<ValidationFailure> failures)
    {
        return failures.Select(c => new KeyValuePair<string, string>(c.PropertyName, c.ErrorMessage)).ToList();
    }
}