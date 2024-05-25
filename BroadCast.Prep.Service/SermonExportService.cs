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
                        
                        Streaming Info:
                        
                        {streamingInfo}

                        Title:

                        {sermon.Title}

                        Speaker:

                        {sermon.Speaker}

                        Series:

                        {sermon.Series}

                        Post Title:

                        {sermon.Title}, {sermon.Passage}

                        Blog Post / YouTube Post Content:

                        {sermon.ToFormattedContent()}
                        
                        YouTube Post Title:
                        
                        {sermon.Passage}. "{sermon.Title}"

                        Port URL:

                        {sermon.ToUrl()}

                        Audio Title:

                        {sermon.Title}, {sermon.Passage}

                        Audio Artist:

                        {sermon.Speaker}

                        Podcast Title:

                        {sermon.Title}

                        Podcast Subtitle:

                        {sermon.Passage}

                        Podcast Season

                        {sermon.Season}

                        Podcast Episode:

                        {sermon.Episode}

                        Audio File:

                        {RecordingConversionService
                            .GetMp4FileName(archiveFolder, sermon.Title)}


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