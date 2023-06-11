﻿using BroadCast.Prep.Models;
using OsborneSupremacy.Extensions.AspNet;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace BroadCast.Prep.Service;

public static partial class InitialBulletinPrepService
{
    public static Outcome<bool> Process(Settings settings)
    {
        var sourceData = FindSourceDoc(settings);

        if (sourceData.IsFaulted)
            return new Outcome<bool>(sourceData.Exception);

        CopyToTargetAndCreateTxtFiles(settings, sourceData.Value);
        return true;
    }

    private static Outcome<SourceFileData> FindSourceDoc(Settings settings)
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
        Match match = DateRegex().Match(input);

        if (match.Success && DateOnly.TryParse(match.Value, out DateOnly date))
        {
            AnsiConsole.WriteLine($"Service date extracted from file name: {date}");
            return date;
        }

        return null;
    }

    private static DateOnly GetDateFromUser()
    {
        static DateOnly? TryGetDate()
        {
            var isValid = DateOnly.TryParse(
                AnsiConsole.Prompt(new TextPrompt<string>("Enter a date (yyyy-MM-dd): ")),
                out var date);

            if (!isValid)
                AnsiConsole.MarkupLine($"[red]Invalid date format[/]. Please enter a date in the format [yellow]yyyy-MM-dd[/].");

            return isValid ? date : null;
        }

        DateOnly? date = null;

        while (date is null)
            date = TryGetDate();

        return date.Value;
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

    [GeneratedRegex("\\d{4}-\\d{2}-\\d{2}")]
    private static partial Regex DateRegex();
}