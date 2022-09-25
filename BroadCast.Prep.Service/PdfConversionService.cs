using BroadCast.Prep.Models;
using LanguageExt.Common;
using PDFiumSharp;
using SixLabors.ImageSharp;

namespace BroadCast.Prep.Service;

public static class PdfConversionService
{
    public static Result<bool> Process(Settings settings)
    {
        try
        {
            foreach (var pdf in settings.PdfsToConvertToImages)
                Process(pdf);
            return true;
        }
        catch (Exception ex)
        {
            return new Result<bool>(ex);
        };
    }

    public static void Process(string pdfFilePath)
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

        Console.WriteLine($"{new FileInfo(pdfFilePath).Name} converted to a .png");
    }
}
