using FluentValidation;

namespace BroadCast.Prep.Models;

public record PagesPng
{
    public required string PageName { get; init; }
    
    public required int Width { get; init; }
    
    public required int Height { get; init; }
}

public class PagesPngValidator : AbstractValidator<PagesPng>
{
    public PagesPngValidator()
    {
        RuleFor(x => x.PageName)
            .NotEmpty();
        
        RuleFor(x => x.Width)
            .GreaterThan(0);
        
        RuleFor(x => x.Height)
            .GreaterThan(0);
    }
}