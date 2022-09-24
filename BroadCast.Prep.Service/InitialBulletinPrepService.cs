using BroadCast.Prep.Models;
using LanguageExt.Common;

namespace BroadCast.Prep.Service;

public static class InitialBulletinPrepService
{
    public static Result<bool> Process(Settings settings)
    {
        return FindSourceDoc(settings).Match(
            sourceData =>
            {
                CopyToTargetAndCreateTxtFiles(settings, sourceData);
                return true;
            },
            error =>
            {
                return new Result<bool>(error);
            }
        );
    }

    public static Result<SourceFileData> FindSourceDoc(Settings settings)
    {
        var serviceDate = DateOnly.FromDateTime(DateTime.Today);
        while (serviceDate.DayOfWeek != DayOfWeek.Sunday)
            serviceDate = serviceDate.AddDays(1);

        var sourceFileName = $"{serviceDate:yyyy-MM-dd} FINAL.pages";
        var sourceFileFullPath = Path.Combine(settings.PagesSourceFolder, sourceFileName);

        Console.WriteLine($"Looking for {sourceFileName}");

        var targetFile = new FileInfo(sourceFileFullPath);

        if (!targetFile.Exists)
            return new Result<SourceFileData>(new FileNotFoundException($"{sourceFileFullPath} not found!"));

        Console.WriteLine($"{sourceFileFullPath} found.");

        return new SourceFileData(targetFile, serviceDate);
    }

    public static void CopyToTargetAndCreateTxtFiles(Settings settings, SourceFileData sourceData)
    {
        var targetFile = Path.Combine(settings.PagesDestinationFolder, "Current.pages");

        sourceData.File.CopyTo(targetFile, true);

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
    }
}
