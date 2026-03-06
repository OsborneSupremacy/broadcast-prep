namespace BroadCast.Prep.Service.Extensions;

public static class SermonExtensions
{
    extension(Sermon sermon)
    {
        public string TitleOrPassage() =>
            string.IsNullOrWhiteSpace(sermon.Title) ? sermon.Passage : sermon.Title;

        public string PassageAndTitle() =>
            !string.IsNullOrWhiteSpace(sermon.Title) ? $"{sermon.Passage}. {sermon.Title}" : sermon.Passage;

        public string PassageAndQuotedTitle() =>
            !string.IsNullOrWhiteSpace(sermon.Title) ? $"{sermon.Passage}. \"{sermon.Title}\"" : sermon.Passage;

        public string BlogTitle() =>
            !string.IsNullOrWhiteSpace(sermon.Title) ? $"{sermon.Title}, {sermon.Passage}" : sermon.Passage;

        public string QuotedTitleAndPassage() =>
            !string.IsNullOrWhiteSpace(sermon.Title) ? $"\"{sermon.Title}\" {sermon.Passage}" : sermon.Passage;
    }
}