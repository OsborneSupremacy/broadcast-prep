﻿using FluentValidation;

namespace BroadCast.Prep.Models;

public record Settings
{
    public string PagesSourceFolder { get; init; } = default!;

    public string PagesDestinationFolder { get; init; } = default!;

    public string DateTxtPath { get; init; } = default!;

    public string TitleAndDescriptionTxtPath { get; init; } = default!;

    public string TitleAndDescriptionTemplate { get; init; } = default!;
}

public class SettingsValidator : AbstractValidator<Settings>
{
    public SettingsValidator()
    {
        RuleFor(x => x.PagesSourceFolder).NotEmpty();
        RuleFor(x => x.PagesDestinationFolder).NotEmpty();
        RuleFor(x => x.DateTxtPath).NotEmpty();
        RuleFor(x => x.TitleAndDescriptionTxtPath).NotEmpty();
        RuleFor(x => x.TitleAndDescriptionTemplate).NotEmpty();

        RuleFor(x => x.PagesSourceFolder).Must(x => new DirectoryInfo(x).Exists);
        RuleFor(x => x.PagesDestinationFolder).Must(x => new DirectoryInfo(x).Exists);
        RuleFor(x => x.DateTxtPath).Must(x => new FileInfo(x)?.Directory?.Exists ?? false);
    }
}