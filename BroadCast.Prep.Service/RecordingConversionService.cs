using BroadCast.Prep.Models;
using OsborneSupremacy.Extensions.AspNet;
using Spectre.Console;

namespace BroadCast.Prep.Service;

public class RecordingConversionService
{
    public static Outcome<bool> Process(Settings settings)
    {
        try
        {
            var files = new DirectoryInfo(settings.RecordingSourceFolder)
                .GetFiles("*.mkv")
                .OrderByDescending(x => x.CreationTimeUtc)
                .ToDictionary(x => x.Name, x => new FileInfo(x.FullName));

            var fileName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select source file")
                    .AddChoices(files.Keys)
                );

            var destinationPath = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter the full destination path (including file name):")
                    .Validate(path =>
                    {
                        var destDirectory = new DirectoryInfo(Path.GetDirectoryName(path));
                        if (!destDirectory.Exists)
                            return ValidationResult.Error($"The directory, {destDirectory.FullName}, does not exist.");
                        return ValidationResult.Success();
                    })
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


}
