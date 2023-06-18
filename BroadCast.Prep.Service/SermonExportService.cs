﻿using Broadcast.Prep.Data;
using BroadCast.Prep.Models;
using OsborneSupremacy.Extensions.AspNet;
using Spectre.Console;

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
                    sermon.Title ?? "Missing Title",
                    sermon.Speaker ?? "Missing Speaker"
                );
            }

            AnsiConsole.Write(table);

            var sermonId = AnsiConsole.Prompt(
                new TextPrompt<int>("Specify Sermon ID")
                    .DefaultValue(sermons.Max(x => x.Key))
                    .Validate(value =>
                    {
                        if (!sermons.ContainsKey(value))
                        return ValidationResult.Error($"Sermon ID {value} does not exist.");
                        return ValidationResult.Success();
                    })
                );

            var selectedSermon = sermons[sermonId];

            var template = @$"

Title:

{selectedSermon.Title}

Speaker:

{selectedSermon.Speaker}

Series:

{selectedSermon.Series}

Post Title:

{selectedSermon.Title}, {selectedSermon.Passage}

Post Content:

{FormatSermonContent(selectedSermon)}

Port URL:

{GetSermonUrl(selectedSermon)}

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

";

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

    private static string FormatSermonContent(Sermon sermon) =>
        @$"{sermon.Title} {sermon.Passage}
{sermon.Speaker}, {sermon.Date:MMMM dd, yyyy}";

    private static string GetSermonUrl(Sermon sermon)
    {
        return @$"{(sermon.Title ?? string.Empty)
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(":", "").RemoveSpecialUrlChars()}";
    }

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