namespace BroadCast.Prep.Service;

public static class SermonExportService
{
    public static Outcome<bool> Process(Settings settings)
    {
        try
        {
            var data = new SermonData(settings.DataStorePath);

            var sermons = data
                .GetAllAsync()
                .OrderByDescending(s => s.Date)
                .Take(10)
                .ToDictionary(x => x.Id, x => x);

            var table = new Table();
            table.AddColumn("ID");
            table.AddColumn("Date");
            table.AddColumn("Title");
            table.AddColumn("Speaker");

            foreach (var sermon in sermons.Values)
            {
                table.AddRow(
                    sermon.Id.ToString(),
                    sermon.Date.ToString("yyyy-MM-dd"),
                    sermon.Title,
                    sermon.Speaker
                );
            }

            AnsiConsole.Write(table);

            var sermonId = AnsiConsole.Prompt(
                new TextPrompt<int>("Specify Sermon ID")
                    .DefaultValue(sermons.Max(x => x.Key))
                    .Validate(value => !sermons.ContainsKey(value)
                        ? ValidationResult.Error($"Sermon ID {value} does not exist.")
                        : ValidationResult.Success())
                );

            var selectedSermon = sermons[sermonId];

            CreatePodcastInfoFile(settings.PodcastArchiveFolder, selectedSermon);
            CreateServiceInfoFile(settings.PodcastArchiveFolder, selectedSermon);
            CreateSermonInfoFile(settings.PodcastArchiveFolder, selectedSermon);

            AnsiConsole.MarkupLine($"Sermon files exported.");

            return true;
        }
        catch (Exception ex)
        {
            return new Outcome<bool>(ex);
        }
    }

    private static readonly Action<string, Sermon> CreatePodcastInfoFile = (archiveFolder, sermon) =>
    {
        var streamingInfo = $"""
                             Service of Worship, {sermon.Date:MMMM dd, yyyy}

                             Worship Service of Grace & Peace Church, Oconomowoc, Wisconsin.

                             {sermon.Passage}. "{sermon.Title}".
                             """;

        var template = $"""
                         
                         # SERVICE & SERMON INFO
                         
                         ## Restream Info
                         
                         {streamingInfo}
                         
                         ## Facebook Post
                         
                         {sermon.ToFacebookFormattedContent()}
                         
                         ## YouTube Video

                         ### YouTube Title:

                         {sermon.Title}

                         ### YouTube Description
                         
                         {sermon.ToFormattedContent()}

                         ## SquareSpace Blog Post
                         
                         ### Blog Post Title
                         
                         {sermon.Title}, {sermon.Passage}
                         
                         ### Blog Post Excerpt
                         
                         {sermon.ToFormattedContent()}
                         
                         ### Blog Post Post URL
                         
                         {sermon.ToUrl()}
                         
                         ### Blog PostAuthor
                         
                         {sermon.Speaker}

                         ### Blog Post Categories
                         
                         {sermon.Series}
                         
                         ### Audio Track
                         
                         {RecordingConversionService
                            .GetMp4FileName(archiveFolder, sermon.Title)}

                         ### Audio Title

                         {sermon.Title}, {sermon.Passage}

                         ### Audio Author / Artist

                         {sermon.Speaker}

                         ### Podcast Title

                         {sermon.Title}
                         
                         ### Podcast Subtitle
                         
                         {sermon.Passage}

                         ### Podcast Summary
                         
                         {sermon.ToFormattedContent()}

                         ### Podcast Season Number

                         {sermon.Season}

                         ### Podcast Episode Number

                         {sermon.Episode}

                         """;

            var outputFile = Path.Combine(archiveFolder, $"sermon-{sermon.Id}.txt");
            if (File.Exists(outputFile))
                File.Delete(outputFile);
            File.WriteAllText(outputFile, template);
    };

    private static readonly Action<string, Sermon> CreateServiceInfoFile = (archiveFolder, sermon) =>
    {
        var outputFile = Path.Combine(archiveFolder, $"_service-info.txt");
        if (File.Exists(outputFile))
            File.Delete(outputFile);
        File.WriteAllText(outputFile, $"Service of Worship, {sermon.Date:MMMM dd, yyyy}");
    };

    private static readonly Action<string, Sermon> CreateSermonInfoFile = (archiveFolder, sermon) =>
    {
        var outputFile = Path.Combine(archiveFolder, $"_verse-title.txt");
        if (File.Exists(outputFile))
            File.Delete(outputFile);
        File.WriteAllText(outputFile, $"{sermon.Passage}. \"{sermon.Title}\"");
    };

    private static string ToFormattedContent(this Sermon sermon) =>
        $"""
         "{sermon.Title}" {sermon.Passage}
         {sermon.Speaker}, {sermon.Date:MMMM dd, yyyy}
         """;

    private static string ToFacebookFormattedContent(this Sermon sermon)
    {
        return $"""

               Our {sermon.Date:dddd}, {sermon.Date:M/d} worship service will be livestreamed on Youtube starting at 10 AM, at the link below.
               
               🔗 ______-LINK-_______
               
               {sermon.Passage}. "{sermon.Title}".
               
               The worship guide is available in PDF format at the link below.
               
               📖 ______-LINK-_______

               """;
    }

    private static string ToUrl(this Sermon sermon) =>
        @$"{(sermon.Title ?? string.Empty)
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(":", "")
            .Replace(" & ", " and ")
            .RemoveSpecialUrlChars()}";

    private static string RemoveSpecialUrlChars(this string uri) =>
        uri
            .Replace(":", "")
            .Replace("/", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("#", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("@", "");
}