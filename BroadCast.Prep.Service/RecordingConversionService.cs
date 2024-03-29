﻿namespace BroadCast.Prep.Service;

public static class RecordingConversionService
{
    public static Outcome<bool> Process(Settings settings)
    {
        try
        {
            var data = new SermonData(settings.DataStorePath);

            var files = new DirectoryInfo(settings.RecordingSourceFolder)
                .GetFiles("*.mkv")
                .OrderByDescending(x => x.CreationTimeUtc)
                .ToDictionary(x => x.Name, x => new FileInfo(x.FullName));

            var fileName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select source file")
                    .AddChoices(files.Keys)
                );

            var title = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select title")
                    .AddChoices(data.GetRecentTitles(5))
            );

            var destinationPath = GetMp4FileName(settings.PodcastArchiveFolder, title);

            _ = new CliWrap.Command("ffmpeg")
                .WithArguments($"-i \"{files[fileName].FullName}\" -vn -acodec copy \"{destinationPath}\"")
                .WithValidation(CliWrap.CommandResultValidation.ZeroExitCode)
                .ExecuteAsync()
                .GetAwaiter()
                .GetResult();

            AnsiConsole.MarkupLine($"Recording converted to to {destinationPath}");

            return true;

        } catch (Exception ex)
        {
            return new Outcome<bool>(ex);
        }
    }

    public static string GetMp4FileName(string directory, string title) =>
        Path.Combine(
            directory,
            $"{GetSafeFileName(title)}.mp4"
        );

    private static string GetSafeFileName(string input) =>
        Path.GetInvalidFileNameChars()
            .Aggregate(input, (current, c) => current.Replace(c.ToString(), string.Empty));
}
