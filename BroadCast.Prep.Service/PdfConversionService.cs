using BroadCast.Prep.Models;
using OsborneSupremacy.Extensions.AspNet;
using PDFiumSharp;
using Spectre.Console;

namespace BroadCast.Prep.Service;

public static class PdfConversionService
{
    public static Outcome<bool> Process(Settings settings)
    {
        try
        {
            var pdfs = settings
                .PdfsToConvertToImages
                .Select(pdf => new FileInfo(pdf))
                .ToList();

            Table table = new();
            table.AddColumn("PDF");
            table.AddColumn("Last Modified");

            foreach (var pdf in pdfs)
                table.AddRow(
                    pdf.Name,
                    pdf.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                );

            AnsiConsole.Write(table);

            if (AnsiConsole.Confirm("Proceed with conversion?"))
                foreach (var pdf in pdfs)
                    Process(pdf.FullName);

            return true;
        }
        catch (Exception ex)
        {
            return new Outcome<bool>(ex);
        }
    }

    private static void Process(string pdfFilePath)
    {
        using var pdfDocument = new PdfDocument(pdfFilePath);

        var firstPage = pdfDocument.Pages.First();

        using var pageBitmap = new PDFiumBitmap(
            (int)firstPage.Width * 3,
            (int)firstPage.Height * 3,
            true
        );

        firstPage.Render(pageBitmap);
        using var pageImage = Image.Load(pageBitmap.AsBmpStream());

        pageImage.SaveAsPng(
            Path.ChangeExtension(pdfFilePath, "png"),
            new SixLabors.ImageSharp.Formats.Png.PngEncoder()
        );

        AnsiConsole.WriteLine($"{new FileInfo(pdfFilePath).Name} converted to a .png");
    }
}
