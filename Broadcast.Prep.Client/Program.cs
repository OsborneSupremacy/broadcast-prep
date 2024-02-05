using Microsoft.Extensions.Configuration;
using BroadCast.Prep.Models;
using BroadCast.Prep.Service;
using OsborneSupremacy.Extensions.AspNet;
using Spectre.Console;

namespace BroadCast.Prep.Client;

public static class Program 
{
    public static void Main()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var settings = configuration
            .GetAndValidateTypedSection("Settings", new SettingsValidator());

        var keepProcessing = true;

        while (keepProcessing) {
            keepProcessing = Process(settings).Continue;
        }

        AnsiConsole.WriteLine("Goodbye.");
        Environment.Exit(0);
    }

    private static ProcessingResult Process(Settings settings)
    {
        var operation = GetOperation();
        var result = operation(settings);

        if(result.IsFaulted)
        {
            AnsiConsole.WriteLine("The following error was encountered:");
            AnsiConsole.WriteLine(result.Exception.Message);
        }

        return new()
        {
            Continue = true
        };
    }

    private static OperationDelegate GetOperation()
    {
        Dictionary<string, OperationDelegate> operations = new() {
            { "Prepare bulletin", InitialBulletinPrepService.Process },
            { "Convert Pages to PNG", PagesToPngService.Process },
            { "Add Sermon", SermonService.Process },
            { "Export Sermon", SermonExportService.Process },
            { "Convert recording MKV to MP3", RecordingConversionService.Process },
            { "Exit", _ =>
                {
                    Environment.Exit(0);
                    return new Outcome<bool>(true);
                }
            },
        };

        var opKey = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select operation")
                .AddChoices(operations.Keys)
            );
        return operations[opKey];
    }

    private delegate Outcome<bool> OperationDelegate(Settings settings);

    private readonly struct ProcessingResult
    {
        public required bool Continue { get; init; }
    }

}