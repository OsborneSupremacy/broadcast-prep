using BroadCast.Prep.Models;
using JsonFlatFileDataStore;

namespace Broadcast.Prep.Data;

public class SermonData
{
    private readonly DataStore _store;

    private readonly IDocumentCollection<Sermon> _collection;

    public SermonData(string path)
    {
        _store = new DataStore(path);
        _collection =  _store.GetCollection<Sermon>();
    }

    public IEnumerable<Sermon> GetAllAsync() =>
        _collection.AsQueryable();

    public Sermon? SingleOrDefault(int id) =>
        _collection
            .AsQueryable()
            .SingleOrDefault(x => x.Id == id);

    public async Task InsertAsync(Sermon sermon) =>
        await _collection.InsertOneAsync(sermon);

    public IEnumerable<string> GetDistinctSpeakers() =>
        GetAllAsync()
            .GroupBy(s => s.Speaker!)
            .OrderByDescending(g => g.Count())
            .Select(x => x.Key);

    public IEnumerable<string> GetDistinctSeries() =>
        GetAllAsync()
            .GroupBy(s => s.Series!)
            .OrderByDescending(g => g.Max(s => s.Date))
            .Select(x => x.Key);

    public IEnumerable<string> GetRecentTitles(int count) =>
        GetAllAsync()
            .OrderByDescending(s => s.Date)
            .Take(count)
            .Select(s => s.Title!);

    public int GetSeasonBySeries(string series)
    {
        var season = GetAllAsync()
            .Where(s => s.Series == series)
            .OrderByDescending(s => s.Date)
            .FirstOrDefault()?
            .Season ?? 0;

        if (season != 0) return season;

        return GetAllAsync()
            .Max(s => s.Season);
    }

    public int GetLastEpisodeBySeries(string series)
    {
        var episode = GetAllAsync()
            .Where(s => s.Series == series)
            .OrderByDescending(s => s.Date)
            .FirstOrDefault()?
            .Episode ?? 0;

        if (episode != 0) return episode;

        return 1;
    }
}
