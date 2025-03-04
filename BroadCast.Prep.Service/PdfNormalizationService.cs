using Amazon.Runtime.CredentialManagement;
using Amazon.Textract;
using Amazon.Textract.Model;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace BroadCast.Prep.Service;

public static class PdfNormalizationService
{
    public static Outcome<bool> Process(Settings settings)
    {
        var sourceDoc = GetSourceDoc(settings);

        var normalizedDoc = Path.Combine(
            settings.NormalizedBulletinFolder,
            Path.ChangeExtension(sourceDoc.Value.Name, ".json")
        );

        // check if the file already exists
        if (File.Exists(normalizedDoc))
        {
            AnsiConsole.MarkupLine($"File {normalizedDoc} already exists.");
            return true;
        }

        AnsiConsole.MarkupLine($"File {normalizedDoc} does not exist. Will require normalization.");

        if(!new CredentialProfileStoreChain().TryGetAWSCredentials("personal", out var credentials))
        {
            AnsiConsole.WriteLine("Could not find AWS credentials.");
            return false;
        }

        var textractClient = new AmazonTextractClient(credentials);

        var pages = GetPages(sourceDoc.Value.FullName);

        foreach (var page in pages)
        {
            var request = new AnalyzeDocumentRequest
            {
                FeatureTypes = [FeatureType.LAYOUT],
                Document = new Document
                {
                    Bytes = new MemoryStream(page)
                }
            };
            var analyzeResponse = textractClient
                .AnalyzeDocumentAsync(request).GetAwaiter().GetResult();
        }

        return true;
    }

    private static List<byte[]> GetPages(string filePath)
    {
        var pagesOut = new List<byte[]>();

        using var inputDocument = PdfReader
            .Open(
                filePath,
                PdfDocumentOpenMode.Import
            );

        for (var i = 0; i < inputDocument.PageCount; i++)
        {
            var pageOut = new PdfDocument();
            pageOut.AddPage(inputDocument.Pages[i]);
            var stream = new MemoryStream();
            pageOut.Save(stream, false);
            pagesOut.Add(stream.ToArray());
        }

        return pagesOut;
    }

    private static Outcome<FileInfo> GetSourceDoc(Settings settings)
    {
        var files = new DirectoryInfo(settings.PagesSourceFolder)
            .GetFiles("*.pdf", SearchOption.AllDirectories)
            .OrderByDescending(x => x.CreationTimeUtc)
            .ToDictionary(x => x.Name, x => new FileInfo(x.FullName));

        var fileName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select source file")
                .AddChoices(files.Keys)
        );

        AnsiConsole.WriteLine($"You selected {fileName}.");

        return files[fileName];
    }
}