using System.ComponentModel.DataAnnotations;

namespace BroadCast.Prep.Models;

public record Settings
{
    [Required(AllowEmptyStrings = false)]
    public string PagesSourceFolder { get; init; } = default!;

    [Required(AllowEmptyStrings = false)]
    public string PagesDestinationFolder { get; init; } = default!;

    [Required(AllowEmptyStrings = false)]
    public string VideoBackgroundSourceFolder { get; init; } = default!;

    [Required(AllowEmptyStrings = false)]
    public string VideoBackgroundDestinationPath { get; init; } = default!;

    [Required(AllowEmptyStrings = false)]
    public string DateTxtPath { get; init; } = default!;

    [Required(AllowEmptyStrings = false)]
    public string TitleAndDescriptionTxtPath { get; init; } = default!;

    [Required(AllowEmptyStrings = false)]
    public string RawTxtPath { get; init; } = default!;

    [Required(AllowEmptyStrings = false)]
    public string TitleAndDescriptionTemplate { get; init; } = default!;
}
