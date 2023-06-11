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
}
