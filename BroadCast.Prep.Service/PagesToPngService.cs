using ImageMagick;
using SixLabors.ImageSharp.Formats.Tga;

namespace BroadCast.Prep.Service;

public static class PagesToPngService
{
    public static Outcome<bool> Process(Settings settings)
    {
        try
        {
            var pages = settings.PagesToConvertToPng;

            AnsiConsole.WriteLine("Convert the following pages to PNG? Current files will be overwritten.");
            foreach (var page in pages.Select(p => p.PageName))
                AnsiConsole.WriteLine($"* {page}");

            if (!AnsiConsole.Confirm("Continue?"))
                return new Outcome<bool>(false);

            ConvertToPdf(settings, pages);

            foreach (var page in pages)
                ConvertToPng(settings, page);

            DeleteTempFiles(settings, pages);

            return true;
        }
        catch (Exception ex)
        {
            return new Outcome<bool>(ex);
        }
    }

    /// <summary>
    /// Apple does not make it easy to automate Pages via the command line (and definitely not via .NET).
    /// To work around this, create a simple AppleScript to open the Pages document and export it to PDF.
    /// We can't use the AppleScript directly from .NET, so we'll ask the user to run the script manually.
    /// This makes me want to cry, but it's not worth spending more time to fully automate it.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="pagesPngs"></param>
    private static void ConvertToPdf(Settings settings, List<PagesPng> pagesPngs)
    {
        var pdfsVerified = 0;

        while (pdfsVerified < pagesPngs.Count)
        {
            var pdfsReady = AnsiConsole.Confirm(
                "You need to manually run the AppleScript to convert the Pages document to PDF now (/Scripts/pages-to-pdf.app). Enter Y when finished.");

            if (!pdfsReady)
                continue;

            pdfsVerified = 0;

            foreach (var page in pagesPngs.Select(p => p.PageName))
            {
                var pdfName = Path.Combine(settings.PagesDestinationFolder, $"{page}.pdf");
                var fileInfo = new FileInfo(pdfName);
                if (!fileInfo.Exists)
                {
                    AnsiConsole.MarkupLine(
                        $"[red]Error:[/] {pdfName} does not exist. Please run the AppleScript to convert the Pages documents to PDFs.");
                    break;
                }

                pdfsVerified++;
            }
        }
    }

    private static void ConvertToPng(Settings settings, PagesPng page)
    {
        var inputFileName = Path.Combine(settings.PagesDestinationFolder, $"{page.PageName}.pdf");
        var outputFileName = Path.Combine(settings.PagesDestinationFolder, $"{page.PageName}.png");

        _ = new CliWrap.Command("sips")
            .WithArguments(
                $"-s format png -z {page.Height} {page.Width} \"{inputFileName}\" --out \"{outputFileName}\"")
            .WithValidation(CliWrap.CommandResultValidation.ZeroExitCode)
            .ExecuteAsync()
            .GetAwaiter()
            .GetResult();
        AnsiConsole.MarkupLine($"Page [green]{page.PageName}[/] converted to {outputFileName}");
    }

    private static void DeleteTempFiles(Settings settings, List<PagesPng> pagesPngs)
    {
        foreach(var png in pagesPngs)
        {
            File.Delete(Path.Combine(settings.PagesDestinationFolder, $"{png.PageName}.pdf"));
            File.Delete(Path.Combine(settings.PagesDestinationFolder, $"{png.PageName}.pages"));
        }
    }

    public static void InvertImage(string pdfPath)
    {
        var outputFileName = Path.ChangeExtension(pdfPath, ".converted.png");

        using var image = new MagickImage();
        image.Read(pdfPath, MagickFormat.Pdf);
        image.Format = MagickFormat.Png;
        image.Density = new Density(300);
        image.Quality = 100;
        //image.Negate(Channels.RGB);
        image.Write(outputFileName);
    }

}