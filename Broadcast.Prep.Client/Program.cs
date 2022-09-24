using Microsoft.Extensions.Configuration;
using BroadCast.Prep.Models;
using BroadCast.Prep.Service;
using BroadCast.Prep.Functions;
using LanguageExt.Common;

namespace BroadCast.Prep.Client;

public class Program 
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting");

        IConfigurationRoot? configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var settings = configuration
            .GetAndValidateTypedSection("Settings", new SettingsValidator());

        var exitCode = GetOperation(args).Match(
            op =>
            {
                return op.Invoke(settings).Match(
                    success =>
                    {
                        Console.WriteLine("Done");
                        return 0;
                    },
                    error =>
                    {
                        Console.WriteLine(error.Message);
                        return 1;
                    }
                );
            },
            error =>
            {
                Console.WriteLine(error.Message);
                return 1;
            }
        );

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
        Environment.Exit(0);
    }

    public static Result<Func<Settings, Result<bool>>> GetOperation(string[] args)
    {
        if (args.Length == 0)
            return new Result<Func<Settings, Result<bool>>>(new ArgumentException("No command line argument found"));

        if (!int.TryParse(args[0], out var op))
            return new Result<Func<Settings, Result<bool>>>(new ArgumentException("Command line argument must be an integer"));

        return op switch
        {
            0 => _initialBulletinPrepServiceDelegate,
            1 => _pdfConversionServiceDelegate,
            _ => new Result<Func<Settings, Result<bool>>>(new ArgumentException("Command line argument must correspond to defined process"))
        };
    }

    private static Func<Settings, Result<bool>> _initialBulletinPrepServiceDelegate = (Settings settings) => {
        return InitialBulletinPrepService.Process(settings);
    };

    private static Func<Settings, Result<bool>> _pdfConversionServiceDelegate = (Settings settings) => {
        return PdfConversionService.Process(settings);
    };

}