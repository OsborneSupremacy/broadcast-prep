using Broadcast.Prep.Data;
using BroadCast.Prep.Models;
using OsborneSupremacy.Extensions.AspNet;
using Spectre.Console;

namespace BroadCast.Prep.Service;

public static class SermonService
{
    public static Outcome<bool> Process(Settings settings)
    {
        try
        {
            var data = new SermonData(settings.DataStorePath);

            AnsiConsole.WriteLine("Provide Sermon Information");

            var series = AnsiConsole.Confirm("Is this a new sermon series?", false) switch {

                true => AnsiConsole.Prompt(
                    new TextPrompt<string>("Series Name:")
                        .Validate(value =>
                        {
                            if (string.IsNullOrWhiteSpace(value))
                                return ValidationResult.Error($"Series Name must not be empty.");
                            return ValidationResult.Success();
                        })
                    ),

                false => AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Series")
                        .AddChoices(data.GetDistinctSeries())
                    )
            };

            var speaker = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Speaker")
                    .AddChoices(data.GetDistinctSpeakers())
                );

            var passage = AnsiConsole.Prompt(
                new TextPrompt<string>("Passage:")
                    .Validate(value =>
                    {
                        if (string.IsNullOrWhiteSpace(value))
                            return ValidationResult.Error($"Passage must not be empty.");
                        return ValidationResult.Success();
                    })
                );

            var title = AnsiConsole.Prompt(
                new TextPrompt<string>("Title:")
                    .Validate(value =>
                    {
                        if (string.IsNullOrWhiteSpace(value))
                            return ValidationResult.Error($"Title must not be empty.");
                        return ValidationResult.Success();
                    })
                );

            var defaultDate = DateOnly.FromDateTime(DateTime.Now);
            while(defaultDate.DayOfWeek != DayOfWeek.Sunday)
                defaultDate = defaultDate.AddDays(1);

            var date = AnsiConsole.Prompt(
                new TextPrompt<DateOnly>("Date")
                    .DefaultValue(defaultDate)
                    .Validate(value =>
                    {
                        if (value == default)
                            return ValidationResult.Error($"Date must not be empty.");
                        return ValidationResult.Success();
                    })
                );

            var season = AnsiConsole.Prompt(
                new TextPrompt<int>("Season:")
                    .DefaultValue(data.GetSeasonBySeries(series))
                    .Validate(value =>
                    {
                        if (value == default)
                            return ValidationResult.Error($"Season must be a valid integer.");
                        return ValidationResult.Success();
                    })
                );

            var episode = AnsiConsole.Prompt(
                new TextPrompt<int>("Episode:")
                    .DefaultValue(data.GetLastEpisodeBySeries(series) + 1)
                    .Validate(value =>
                    {
                        if (value == default)
                            return ValidationResult.Error($"Episode must be a valid integer.");
                        return ValidationResult.Success();
                    })
                );

            var sermon = new Sermon
            {
                Series = series,
                Speaker = speaker,
                Passage = passage,
                Title = title,
                Date = date,
                Season = season,
                Episode = episode
            };

            data.InsertAsync(sermon).GetAwaiter().GetResult();

            return true;
        }
        catch (Exception ex)
        {
            return new Outcome<bool>(ex);
        };
    }
}
