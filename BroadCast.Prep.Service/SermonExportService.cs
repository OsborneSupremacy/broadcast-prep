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
            
            var streamingInfo = $"""
                                 Service of Worship, {selectedSermon.Date:MMMM dd, yyyy}

                                 Worship Service of Grace & Peace Church, Oconomowoc, Wisconsin.

                                 {selectedSermon.Passage}. "{selectedSermon.Title}".
                                 """;            

            var template = $"""
                            
                            Streaming Info:
                            
                            {streamingInfo}

                            Title:

                            {selectedSermon.Title}

                            Speaker:

                            {selectedSermon.Speaker}

                            Series:

                            {selectedSermon.Series}

                            Post Title:

                            {selectedSermon.Title}, {selectedSermon.Passage}

                            Post Content:

                            {selectedSermon.ToFormattedContent()}

                            Port URL:

                            {selectedSermon.ToUrl()}

                            Audio Title:

                            {selectedSermon.Title}, {selectedSermon.Passage}

                            Audio Artist:

                            {selectedSermon.Speaker}

                            Podcast Title:

                            {selectedSermon.Title}

                            Podcast Subtitle:

                            {selectedSermon.Passage}

                            Podcast Season

                            {selectedSermon.Season}

                            Podcast Episode:

                            {selectedSermon.Episode}

                            Audio File:

                            {RecordingConversionService
                                .GetMp4FileName(settings.PodcastArchiveFolder, selectedSermon.Title ?? "untitled")}


                            """;

            var outputFile = Path.Combine(settings.PodcastArchiveFolder, $"sermon-{selectedSermon.Id}.txt");
            if (File.Exists(outputFile))
                File.Delete(outputFile);
            File.WriteAllText(outputFile, template);

            AnsiConsole.MarkupLine($"Sermon exported to {outputFile}");

            return true;
        }
        catch (Exception ex)
        {
            return new Outcome<bool>(ex);
        }
    }
    
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
