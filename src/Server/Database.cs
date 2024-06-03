using System.Collections.Concurrent;

public class Database : IDisposable
{
    private readonly ConcurrentDictionary<string, string> _data = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _ex = new();
    private readonly object _lock = new();
    private readonly object _cleanLock = new();
    private CancellationTokenSource _cts = new();

    public Database()
    {
        _ = Task.Run(Clean);
    }

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

    public void Lock() => Monitor.Enter(_cleanLock);

    public void Unlock() => Monitor.Exit(_cleanLock);

    private void Clean()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            foreach (var (key, expire) in _ex)
            {
                if (expire < DateTimeOffset.UtcNow)
                    lock (_cleanLock) Del(key);
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _data.Clear();
        _ex.Clear();
        GC.SuppressFinalize(this);
    }
}