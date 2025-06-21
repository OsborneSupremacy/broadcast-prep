using System.Text.RegularExpressions;

namespace BroadCast.Prep.Service;

public static partial class SermonExportService
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
                table.AddRow(
                    sermon.Id.ToString(),
                    sermon.Date.ToString("yyyy-MM-dd"),
                    sermon.Title,
                    sermon.Speaker
                );

            AnsiConsole.Write(table);

            var sermonId = AnsiConsole.Prompt(
                new TextPrompt<int>("Specify Sermon ID")
                    .DefaultValue(sermons.Max(x => x.Key))
                    .Validate(value => !sermons.ContainsKey(value)
                        ? ValidationResult.Error($"Sermon ID {value} does not exist.")
                        : ValidationResult.Success())
                );

            var selectedSermon = sermons[sermonId];

            CreatePodcastInfoFile(settings.AssetPath, selectedSermon);
            CreateServiceInfoFile(settings.AssetPath, selectedSermon);
            CreateSermonInfoFile(settings.AssetPath, selectedSermon);

            AnsiConsole.MarkupLine("Sermon files exported.");

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
                             Service of Worship, {sermon.Date:MMMM d, yyyy}

                             Worship Service of Grace & Peace Church, Oconomowoc, Wisconsin.

                             {sermon.Passage}. "{sermon.Title}".
                             
                             The worship guide is available at the link below.
                             
                             📖 {sermon.PdfUrl}

                             """;

        var template = $"""
                         
                         # SERVICE & SERMON INFO
                         
                         ## Restream Info
                         
                         {streamingInfo}
                         
                         ## Facebook Post
                         
                         {sermon.ToFacebookFormattedContent()}
                         
                         ## YouTube Video

                         ### YouTube Title:

                         {sermon.Passage}. "{sermon.Title}"

                         ### YouTube Description
                         
                         {sermon.ToFormattedContent()}
                         
                         ### YouTube Tags
                         
                         PCA, Christianity, Grace&Peace, Oconomowoc

                         ## SquareSpace Blog Post
                         
                         ### Blog Post Title
                         
                         {sermon.Title}, {sermon.Passage}
                         
                         ### Blog Post Excerpt
                         
                         {sermon.ToFormattedContent()}
                         
                         ### Blog Post Post URL
                         
                         {sermon.ToUrl()}
                         
                         ### Blog Post Author
                         
                         {sermon.Speaker}

                         ### Blog Post Categories
                         
                         {sermon.Series}

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
        var outputFile = Path.Combine(archiveFolder, "01-service-info.txt");
        if (File.Exists(outputFile))
            File.Delete(outputFile);
        File.WriteAllText(outputFile, $"Service of Worship, {sermon.Date:MMMM d, yyyy}");
    };

    private static readonly Action<string, Sermon> CreateSermonInfoFile = (archiveFolder, sermon) =>
    {
        var outputFile = Path.Combine(archiveFolder, "02-verse-title.txt");
        if (File.Exists(outputFile))
            File.Delete(outputFile);
        File.WriteAllText(outputFile, $"{sermon.Passage}. \"{sermon.Title}\"");
    };

    private static string ToFormattedContent(this Sermon sermon) =>
        $"""
         "{sermon.Title}" {sermon.Passage}
         {sermon.Speaker}, {sermon.Date:MMMM d, yyyy}
         """;

    private static string ToFacebookFormattedContent(this Sermon sermon)
    {
        return $"""

               Our {sermon.Date:dddd}, {sermon.Date:M/d} worship service will be livestreamed on Youtube starting at 10 AM, at the link below.
               
               🔗 ______-LINK-_______
               
               {sermon.Passage}. "{sermon.Title}".
               
               The worship guide is available at the link below.
               
               📖 {sermon.PdfUrl}

               """;
    }

    private static string ToUrl(this Sermon sermon) =>
        $"{sermon.Title
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(":", string.Empty)
            .Replace(" & ", " and ")
            .RemoveSpecialUrlChars()}";

    private static string RemoveSpecialUrlChars(this string uri) =>
        SpecialUrlCharsRegex().Replace(uri, string.Empty);

    [GeneratedRegex(@"[:\/!?#\[\]@]")]
    private static partial Regex SpecialUrlCharsRegex();
}