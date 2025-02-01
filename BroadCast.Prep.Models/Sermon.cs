namespace BroadCast.Prep.Models;

public record Sermon
{
    public int Id { get; init; }

    public required string Series { get; init; }

    public required string Speaker { get; init; }

    public required string Passage { get; init; }

    public required string Title { get; init; }

    public required DateOnly Date { get; init; }

    public required int Season { get; init; }

    public required int Episode { get; init; }

    public required string PdfUrl { get; init; }
}
