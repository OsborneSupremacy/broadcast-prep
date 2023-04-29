using BroadCast.Prep.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using OsborneSupremacy.Extensions.AspNet;
using Spectre.Console;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BroadCast.Prep.Service;

public static class InitialBulletinPrepService
{
    public static Outcome<bool> Process(Settings settings)
    {
        var sourceData = FindSourceDoc(settings);

        if (sourceData.IsFaulted)
            return new Outcome<bool>(sourceData.Exception);

        CopyToTargetAndCreateTxtFiles(settings, sourceData.Value);
        return true;
    }

    public static Outcome<SourceFileData> FindSourceDoc(Settings settings)
    {
        var files = new DirectoryInfo(settings.PagesSourceFolder)
            .GetFiles()
            .OrderByDescending(x => x.CreationTimeUtc)
            .ToDictionary(x => x.Name, x => new FileInfo(x.FullName));

        var fileName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select source file")
                .AddChoices(files.Keys)
            );

        AnsiConsole.WriteLine($"You selected {fileName}.");

        DateOnly serviceDate =
            ExtractDate(fileName)
            ?? GetDateFromUser();

        var targetFile = files[fileName];

        return new SourceFileData(targetFile, serviceDate);
    }

    private static DateOnly? ExtractDate(string input)
    {
        Match match = Regex.Match(input, @"\d{4}-\d{2}-\d{2}");

        if (match.Success && DateOnly.TryParse(match.Value, out DateOnly date))
        {
            AnsiConsole.WriteLine($"Service date extracted from file name: {date}");
            return date;
        }

        return null;
    }

    public static DateOnly GetDateFromUser()
    {
        DateOnly date;

        // Ask the user to enter a date in the format "yyyy-MM-dd".
        string prompt = "Enter a date (yyyy-MM-dd): ";
        string input = AnsiConsole.Prompt(new TextPrompt<string>(prompt));

        // Attempt to parse the user's input as a DateOnly value.
        while (!DateOnly.TryParse(input, out date))
        {
            // If the parsing fails, ask the user to enter a valid date.
            AnsiConsole.MarkupLine($"[red]Invalid date format[/]. Please enter a date in the format [yellow]yyyy-MM-dd[/].");
            input = AnsiConsole.Prompt(new TextPrompt<string>(prompt));
        }

        // Return the parsed date.
        return date;
    }

    private static void CopyToTargetAndCreateTxtFiles(Settings settings, SourceFileData sourceData)
    {
        var targetFile = Path.Combine(settings.PagesDestinationFolder, "Current.pages");

        sourceData.File.CopyTo(targetFile, true);
        AnsiConsole.WriteLine($"Copied file to {targetFile}");

        // write date to date txt file
        if (File.Exists(settings.DateTxtPath))
            File.Delete(settings.DateTxtPath);
        File.WriteAllText(settings.DateTxtPath, sourceData.Date.ToLongDateString());

        // write title and description file
        if (File.Exists(settings.TitleAndDescriptionTxtPath))
            File.Delete(settings.TitleAndDescriptionTxtPath);

        var titleAndDescription = settings.TitleAndDescriptionTemplate
            .Replace("{ServiceDate}", sourceData.Date.ToString("M/d/yyyy"));

        File.WriteAllText(settings.TitleAndDescriptionTxtPath, titleAndDescription);
        AnsiConsole.WriteLine($"Updated title and description files.");
    }
}
