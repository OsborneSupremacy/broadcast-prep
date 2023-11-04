namespace BroadCast.Prep.Models;

public record SourceFileData(FileInfo File, DateOnly Date)
{
    public FileInfo File { get; set; } = File ?? throw new ArgumentNullException(nameof(File));

    public DateOnly Date { get; set; } = Date;
}

