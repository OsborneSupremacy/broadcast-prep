namespace BroadCast.Prep.Models;

public record Sermon
{
    public int Id { get; set; }

    public string? Series { get; set; }

    public string? Speaker { get; set; }

    public string? Passage { get; set; }

    public string? Title { get; set; }

    public DateOnly Date { get; set; }

    public int Season { get; set; }

    public int Episode { get; set; }
}
