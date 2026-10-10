using FluentValidation.Results;

namespace Application.Extensions;

public static class ApplicationValidationExtensions
{
    public static List<KeyValuePair<string, string>> ConvertToKeyValuePair(this List<ValidationFailure> failures)
    {
        return failures.Select(c => new KeyValuePair<string, string>(c.PropertyName, c.ErrorMessage)).ToList();
    }
}