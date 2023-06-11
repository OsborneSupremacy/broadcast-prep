using Broadcast.Prep.Data;
using BroadCast.Prep.Models;
using Microsoft.AspNetCore.Components.Web;
using OsborneSupremacy.Extensions.AspNet;
using Spectre.Console;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BroadCast.Prep.Service;

public class RecordingConversionService
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

            var destinationPath = Path.Combine(
                settings.PodcastArchiveFolder,
                $"{GetSafeFileName(title)}.mp4"
            );

            var result = new CliWrap.Command(settings.FfMpegPath)
                .WithArguments($"-i \"{files[fileName].FullName}\" -vn -acodec copy \"{destinationPath}\"")
                .WithValidation(CliWrap.CommandResultValidation.ZeroExitCode)
                .ExecuteAsync()
                .GetAwaiter()
                .GetResult();

            return true;

        } catch (Exception ex)
        {
            return new Outcome<bool>(ex);
        }
    }

    private static string GetSafeFileName(string input) =>
        Path.GetInvalidFileNameChars()
            .Aggregate(input, (current, c) => current.Replace(c.ToString(), string.Empty));
}
