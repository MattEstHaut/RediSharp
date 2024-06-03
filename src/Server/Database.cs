using System.Collections.Concurrent;

public class Database
{
    private readonly ConcurrentDictionary<string, string> _data = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _ex = new();
    private readonly object _lock = new();

    public Database() { }

    public void Set(string key, string value)
    {
        lock (_lock)
        {
            _data[key] = value;
            _ex.Remove(key, out _);
        }
    }

    public void Set(string key, string value, long ex)
    {
        lock (_lock)
        {
            _data[key] = value;
            _ex[key] = DateTimeOffset.UtcNow.AddMilliseconds(ex);
        }
    }

    public void Set(string key, long ex)
    {
        lock (_lock)
        {
            if (_data.ContainsKey(key))
                _ex[key] = DateTimeOffset.UtcNow.AddMilliseconds(ex);
        }
    }

    public void Del(string key)
    {
        lock (_lock)
        {
            _data.Remove(key, out _);
            _ex.Remove(key, out _);
        }
    }

    public string? Get(string key)
    {
        lock (_lock)
        {
            if (_ex.TryGetValue(key, out var expire) && expire < DateTimeOffset.UtcNow)
                Del(key);

            return _data.TryGetValue(key, out var value) ? value : null;
        }
    }
}