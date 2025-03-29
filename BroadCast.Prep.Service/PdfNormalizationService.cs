using Amazon.Runtime.CredentialManagement;
using Amazon.Textract;
using Amazon.Textract.Model;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace BroadCast.Prep.Service;

public static class PdfNormalizationService
{
    private static readonly BlockType[] TextBlockTypes =
    [
        BlockType.LINE,
        BlockType.WORD
    ];

    public static Outcome<bool> Process(Settings settings)
    {
        var sourceDoc = GetSourceDoc(settings);

        var plainTextDoc = Path.Combine(
            settings.NormalizedBulletinFolder,
            Path.ChangeExtension(sourceDoc.Value.Name, ".text")
        );

        // check if the file already exists
        if (File.Exists(plainTextDoc))
        {
            AnsiConsole.MarkupLine($"File {plainTextDoc} already exists.");
            return true;
        }

        AnsiConsole.MarkupLine($"File {plainTextDoc} does not exist. Will require normalization.");

        if(!new CredentialProfileStoreChain().TryGetAWSCredentials("personal", out var credentials))
        {
            AnsiConsole.WriteLine("Could not find AWS credentials.");
            return false;
        }

        var textractClient = new AmazonTextractClient(credentials);

        // stream output to plain text file
        using var writer = new StreamWriter(plainTextDoc);

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

            var lastBlockType = BlockType.LINE;

            foreach (var block in analyzeResponse
                         .Blocks.Where(b => TextBlockTypes.Contains(b.BlockType)))
            {
                AnsiConsole.WriteLine($"Content: {block.Text}");

                if(lastBlockType != block.BlockType)
                    writer.WriteLine();

                if (block.BlockType == BlockType.WORD)
                    writer.Write($"{block.Text} ");

                if (block.BlockType == BlockType.LINE)
                {
                    writer.WriteLine(block.Text);
                    writer.WriteLine();
                }

                lastBlockType = block.BlockType;
            }
        }

        // close the writer
        writer.Flush();
        writer.Close();

        AnsiConsole.WriteLine("Text extraction complete. File saved to {plainTextDoc}");

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