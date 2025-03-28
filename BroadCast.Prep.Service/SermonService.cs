﻿namespace BroadCast.Prep.Service;

public static class SermonService
{
    public static Outcome<bool> Process(Settings settings)
    {
        try
        {
            var data = new SermonData(settings.DataStorePath);

            AnsiConsole.WriteLine("Provide Sermon Information");

            var series = PromptForListItemOrNew(data.GetDistinctSeries(), "Series");

            var sermon = new Sermon
            {
                Series = series,
                Speaker = PromptForListItemOrNew(data.GetDistinctSpeakers(), "Speaker"),
                Passage = PromptForText("Passage"),
                Title = PromptForText("Title"),
                Date = PromptForDateOnly("Date", GetDefaultDate()),
                Season = PromptForInt("Season", data.GetSeasonBySeries(series)),
                Episode = PromptForInt("Episode", data.GetLastEpisodeBySeries(series) + 1),
                PdfUrl = PromptForUrl("Shared URL of bulletin")
            };

            data.InsertAsync(sermon).GetAwaiter().GetResult();

            return true;
        }
        catch (Exception ex)
        {
            return new Outcome<bool>(ex);
        }
    }

    private static DateOnly GetDefaultDate()
    {
        var defaultDate = DateOnly.FromDateTime(DateTime.Now);
        while(defaultDate.DayOfWeek != DayOfWeek.Sunday)
            defaultDate = defaultDate.AddDays(1);

        return defaultDate;
    }

    private static DateOnly PromptForDateOnly(string prompt, DateOnly defaultValue) =>
        AnsiConsole.Prompt(
            new TextPrompt<DateOnly>($"{prompt}:")
                .DefaultValue(defaultValue)
                .Validate(value => value == default
                    ? ValidationResult.Error($"{prompt} must be a valid date.")
                    : ValidationResult.Success())
            );

    private static int PromptForInt(string prompt, int defaultValue) =>
        AnsiConsole.Prompt(
            new TextPrompt<int>($"{prompt}:")
                .DefaultValue(defaultValue)
                .Validate(value => value == default
                    ? ValidationResult.Error($"{prompt} must be a valid integer.")
                    : ValidationResult.Success())
            );

    private static string PromptForText(string prompt) =>
        AnsiConsole.Prompt(
            new TextPrompt<string>($"{prompt}:")
                .Validate(value => string.IsNullOrWhiteSpace(value)
                    ? ValidationResult.Error($"{prompt} must not be empty.")
                    : ValidationResult.Success())
        );

    private static string PromptForUrl(string prompt) =>
        AnsiConsole.Prompt(
            new TextPrompt<string>($"{prompt}:")
                .Validate(value => MustBeValidUrlOrEmpty(value)
                    ? ValidationResult.Error($"{prompt} must not be empty and a valid URL.")
                    : ValidationResult.Success())
        );

    private static bool MustBeValidUrlOrEmpty(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;
        return !Uri.TryCreate(value, UriKind.Absolute, out _);
    }
    
    private static string PromptForListItemOrNew(
        IEnumerable<string> items,
        string itemType
        )
    {
        const string newKeyword = "New";

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"{itemType}:")
                .AddChoices(newKeyword)
                .AddChoices(items)
            );

        return selection != newKeyword
            ? selection
            : AnsiConsole.Prompt(
                new TextPrompt<string>($"New {itemType}:")
                    .Validate(value => string.IsNullOrWhiteSpace(value)
                        ? ValidationResult.Error($"{itemType} must not be empty.")
                        : ValidationResult.Success())
                );
    }
}
