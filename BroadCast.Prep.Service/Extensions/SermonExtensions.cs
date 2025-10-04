namespace BroadCast.Prep.Service.Extensions;

public static class SermonExtensions
{
    public static string TitleOrPassage(this Sermon sermon) =>
        string.IsNullOrWhiteSpace(sermon.Title) ? sermon.Passage : sermon.Title;

    public static string PassageAndTitle(this Sermon sermon) =>
        !string.IsNullOrWhiteSpace(sermon.Title) ? $"{sermon.Passage}. {sermon.Title}" : sermon.Passage;

    public static string PassageAndQuotedTitle(this Sermon sermon) =>
        !string.IsNullOrWhiteSpace(sermon.Title) ? $"{sermon.Passage}. \"{sermon.Title}\"" : sermon.Passage;

    public static string BlogTitle(this Sermon sermon) =>
        !string.IsNullOrWhiteSpace(sermon.Title) ? $"{sermon.Title}, {sermon.Passage}" : sermon.Passage;

    public static string QuotedTitleAndPassage(this Sermon sermon) =>
        !string.IsNullOrWhiteSpace(sermon.Title) ? $"\"{sermon.Title}\" {sermon.Passage}" : sermon.Passage;
}