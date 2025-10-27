using Google.Apis.Json;
using Google.Apis.Util.Store;

namespace ContactManager.data;

public class InMemoryDataStore : IDataStore
{
    private readonly Dictionary<string, string> _dict = new();
    
    public Task StoreAsync<T>(string key, T value)
    {
        _dict[key] = NewtonsoftJsonSerializer.Instance.Serialize(value); return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(string key)
    {
        _dict.Remove(key); return Task.CompletedTask;
    }

    public Task<T> GetAsync<T>(string key)
    {
        return Task.FromResult(_dict.TryGetValue(key, out var v) ? NewtonsoftJsonSerializer.Instance.Deserialize<T>(v) : default!);
    }

    public Task ClearAsync()
    {
        _dict.Clear(); return Task.CompletedTask;
    }
}