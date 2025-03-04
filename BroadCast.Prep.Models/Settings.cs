using FluentValidation;

namespace BroadCast.Prep.Models;

public record Settings
{
    public string DataStorePath { get; init; } = string.Empty;

    public string NormalizedBulletinFolder { get; init; } = string.Empty;

    public string RecordingSourceFolder { get; init; } = string.Empty;

    public string PodcastArchiveFolder { get; init; } = string.Empty;

    public string PagesSourceFolder { get; init; } = string.Empty;

    public string PagesDestinationFolder { get; init; } = string.Empty;

    public List<PagesPng> PagesToConvertToPng { get; init; } = [];

    public string DateTxtPath { get; init; } = string.Empty;

    public string TitleAndDescriptionTxtPath { get; init; } = string.Empty;
}

public class SettingsValidator : AbstractValidator<Settings>
{
    public SettingsValidator()
    {
        RuleFor(x => x.DataStorePath)
            .NotEmpty()
            .Must(x => new FileInfo(x).Exists);

        RuleFor(x => x.NormalizedBulletinFolder)
            .NotEmpty()
            .Must(x => new DirectoryInfo(x).Exists);

        RuleFor(x => x.RecordingSourceFolder)
            .NotEmpty()
            .Must(x => new DirectoryInfo(x).Exists);

        RuleFor(x => x.PodcastArchiveFolder)
            .NotEmpty()
            .Must(x => new DirectoryInfo(x).Exists);

        RuleFor(x => x.PagesSourceFolder)
            .NotEmpty()
            .Must(x => new DirectoryInfo(x).Exists);

        RuleFor(x => x.PagesDestinationFolder)
            .NotEmpty()
            .Must(x => new DirectoryInfo(x).Exists);

        RuleForEach(x => x.PagesToConvertToPng)
            .SetValidator(new PagesPngValidator());
        
        RuleFor(x => x.DateTxtPath)
            .NotEmpty()
            .Must(x => new FileInfo(x).Directory?.Exists ?? false);

        RuleFor(x => x.TitleAndDescriptionTxtPath).NotEmpty();
    }
}