namespace BroadCast.Prep.Service;

public static class BulletinCopyService
{
    public static Outcome<bool> Process(Settings settings)
    {
        var targetFile = Path.Combine(settings.PagesDestinationFolder, "Current.pages");

        var makeCopies = false;
        AnsiConsole.WriteLine("Once you've made the desired edits to Current.pages, press Y to make copies of the file.");
        while(!makeCopies)
            makeCopies = AnsiConsole.Confirm("Ready?");

        var pageDestinations = settings
            .PagesToConvertToPng
            .Select(p => Path.Combine(settings.PagesDestinationFolder, $"{p.PageName}.pages"));

        foreach (var destination in pageDestinations)
        {
            if(targetFile.Equals(destination, StringComparison.CurrentCultureIgnoreCase))
                continue;
            File.Copy(targetFile, destination, true);
        }

        AnsiConsole.WriteLine("Current.pages has been copied.");

        return true;
    }
}