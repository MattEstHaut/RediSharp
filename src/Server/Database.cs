using System.Collections.Concurrent;

public class Database
{
    private ConcurrentDictionary<string, string> _data = new();
    private ConcurrentDictionary<string, DateTimeOffset> _ex = new();

    public Database() { }

    public void Set(string key, string value)
    {
        _data[key] = value;
        _ex.Remove(key, out _);
    }

    public void Set(string key, string value, long ex)
    {
        _data[key] = value;
        _ex[key] = DateTimeOffset.UtcNow.AddMilliseconds(ex);
    }

    public void Set(string key, long ex)
    {
        if (_data.ContainsKey(key))
            _ex[key] = DateTimeOffset.UtcNow.AddMilliseconds(ex);
    }

    public void Del(string key)
    {
        _data.Remove(key, out _);
        _ex.Remove(key, out _);
    }

    public string? Get(string key)
    {
        if (_ex.TryGetValue(key, out var expire) && expire < DateTimeOffset.UtcNow)
            Del(key);

        return _data.TryGetValue(key, out var value) ? value : null;
    }
}