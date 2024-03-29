﻿using FluentValidation;

namespace BroadCast.Prep.Models;

public record Settings
{
    public string DataStorePath { get; set; } = default!;

    public string RecordingSourceFolder { get; init; } = default!;

    public string PodcastArchiveFolder { get; init; } = default!;

    public string PagesSourceFolder { get; init; } = default!;

    public string PagesDestinationFolder { get; init; } = default!;
    
    public List<PagesPng> PagesToConvertToPng { get; init;} = new();

    public string DateTxtPath { get; init; } = default!;

    public string TitleAndDescriptionTxtPath { get; init; } = default!;
}

public class SettingsValidator : AbstractValidator<Settings>
{
    public SettingsValidator()
    {
        RuleFor(x => x.DataStorePath)
            .NotEmpty()
            .Must(x => new FileInfo(x).Exists);

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