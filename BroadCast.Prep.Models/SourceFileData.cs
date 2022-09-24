namespace BroadCast.Prep.Models;

public record SourceFileData
{
    public SourceFileData(FileInfo file, DateOnly date)
    {
        File = file ?? throw new ArgumentNullException(nameof(file));
        Date = date;
    }

    public FileInfo File { get; set; }

    public DateOnly Date { get; set; }
}

