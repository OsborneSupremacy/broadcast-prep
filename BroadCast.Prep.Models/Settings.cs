using FluentValidation;

namespace BroadCast.Prep.Models;

public record Settings
{
    public string DataStorePath { get; init; } = string.Empty;

    public string AssetPath { get; init; } = string.Empty;

    public string DateTxtPath { get; init; } = string.Empty;
}

public class SettingsValidator : AbstractValidator<Settings>
{
    public SettingsValidator()
    {
        RuleFor(x => x.DataStorePath)
            .NotEmpty()
            .Must(x => new FileInfo(x).Exists);

        RuleFor(x => x.AssetPath)
            .NotEmpty()
            .Must(x => new DirectoryInfo(x).Exists);
        
        RuleFor(x => x.DateTxtPath)
            .NotEmpty()
            .Must(x => new FileInfo(x).Directory?.Exists ?? false);
    }
}