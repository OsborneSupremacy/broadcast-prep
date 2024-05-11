using Microsoft.Extensions.Configuration;
using BroadCast.Prep.Models;
using BroadCast.Prep.Service;
using OsborneSupremacy.Extensions.AspNet;
using Spectre.Console;

namespace BroadCast.Prep.Client;

public static class Program
{
    private static readonly Dictionary<string, Func<Settings, ProcessingResult>> ProcessingModes = new()
    {
        { "Execute entire workflow", ExecuteEntireWorkflow },
        { "Execute one step", ExecuteOneStep },
    };

    private static readonly Dictionary<string, OperationDelegate> Operations = new() {
        { "Prepare bulletin", InitialBulletinPrepService.Process },
        { "Make Copies of Current.pages", BulletinCopyService.Process },
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

    public static void Main()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var settings = configuration
            .GetAndValidateTypedSection("Settings", new SettingsValidator());

        var processingMode = ProcessingModes[AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select processing mode")
                .AddChoices(ProcessingModes.Keys)
        )];

        var keepProcessing = true;

        while (keepProcessing) {
            keepProcessing = processingMode(settings).Continue;
        }

        AnsiConsole.WriteLine("Goodbye.");
        Environment.Exit(0);
    }

    private static ProcessingResult ExecuteEntireWorkflow(Settings settings)
    {
        foreach(var operation in Operations)
        {
            while (!AnsiConsole.Confirm($"{operation.Key} - Press Y to proceed.")) { }
            var result = operation.Value(settings);
            if (!result.IsFaulted)
                continue;

            AnsiConsole.WriteLine($"""
                                   The following error was encountered:

                                   {result.Exception.Message}

                                   """);
            return new()
            {
                Continue = false
            };
        }
        AnsiConsole.WriteLine("All operations completed successfully.");
        return new()
        {
            Continue = false
        };
    }

    private static ProcessingResult ExecuteOneStep(Settings settings)
    {
        var operation = GetOperation();
        var result = operation(settings);

        if (!result.IsFaulted)
            return new()
            {
                Continue = true
            };

        AnsiConsole.WriteLine($"""
                               The following error was encountered:

                               {result.Exception.Message}

                               """);

        return new()
        {
            Continue = true
        };
    }

    private static OperationDelegate GetOperation() =>
        Operations[
            AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select operation")
                    .AddChoices(Operations.Keys)
            )
        ];

    private delegate Outcome<bool> OperationDelegate(Settings settings);

    private readonly struct ProcessingResult
    {
        public required bool Continue { get; init; }
    }
}