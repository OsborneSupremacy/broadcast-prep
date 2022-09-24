using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace BroadCast.Prep.Functions;

public static class ConfigurationRootExtensions
{
    public static T GetTypedSection<T>(
        this IConfigurationRoot input,
        string sectionName
    ) where T : new()
    {
        T output = new();
        input.Bind(sectionName, output);
        return output;
    }

    public static T GetAndValidateTypedSection<T>(
        this IConfigurationRoot input,
        string sectionName,
        AbstractValidator<T> validator
    ) where T : new()
    {
        var output = input.GetTypedSection<T>(sectionName);

        var validationResult = validator.Validate(output);

        if (!validationResult.IsValid)
            throw new Exception(string.Join(",", validationResult.Errors));

        return output;
    }
}