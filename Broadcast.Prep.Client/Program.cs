using Microsoft.Extensions.Configuration;
using BroadCast.Prep.Models;
using BroadCast.Prep.Service;
using OsborneSupremacy.Extensions.AspNet;

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

        var operation = GetOperation(args);

        if(operation.IsFaulted)
        {
            Console.WriteLine(operation.Exception.Message);
            Environment.Exit(1);
        };

        var outcome = operation.Value.Invoke(settings);

        if(outcome.IsFaulted)
        {
            Console.WriteLine(outcome.Exception.Message);
            Environment.Exit(1);
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
        Environment.Exit(0);
    }

    public static Outcome<Func<Settings, Outcome<bool>>> GetOperation(string[] args)
    {
        if (args.Length == 0)
            return new Outcome<Func<Settings, Outcome<bool>>>(new ArgumentException("No command line argument found"));

        if (!int.TryParse(args[0], out var op))
            return new Outcome<Func<Settings, Outcome<bool>>>(new ArgumentException("Command line argument must be an integer"));

        return op switch
        {
            0 => _initialBulletinPrepServiceDelegate,
            1 => _pdfConversionServiceDelegate,
            _ => new Outcome<Func<Settings, Outcome<bool>>>(new ArgumentException("Command line argument must correspond to defined process"))
        };
    }

    private static readonly Func<Settings, Outcome<bool>> _initialBulletinPrepServiceDelegate = InitialBulletinPrepService.Process;

    private static readonly Func<Settings, Outcome<bool>> _pdfConversionServiceDelegate = PdfConversionService.Process;

}