using System.Collections.Concurrent;
using Shared.Resp;

public class Database : IDisposable
{
    private readonly ConcurrentDictionary<string, string> _data = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _ex = new();
    private readonly object _lock = new();
    private readonly object _cleanLock = new();
    private CancellationTokenSource _cts = new();
    private Timer? _timer;

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

    public void SetValue(string key, string value)
    {
        _data[key] = value;
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

    public DateTimeOffset? TTL(string key)
    {
        lock (_lock)
        {
            if (_ex.TryGetValue(key, out var expire) && expire < DateTimeOffset.UtcNow)
            {
                Del(key);
                return null;
            }

            return _ex.TryGetValue(key, out var value) ? value : null;
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

    public Item Encode()
    {
        var db = new Dictionary<Item, Item>();
        var data = new Dictionary<Item, Item>();
        var ex = new Dictionary<Item, Item>();

        lock (_lock)
        {
            foreach (var (key, value) in _data)
                data[new BulkString(key)] = new BulkString(value);

            foreach (var (key, value) in _ex)
                ex[new BulkString(key)] = new Integer(value.ToUnixTimeMilliseconds());
        }

        db[new BulkString("data")] = new Map(data);
        db[new BulkString("ex")] = new Map(ex);
        return new Map(db);
    }

    public static Database Decode(Item item)
    {
        if (item is not Map map)
            throw new ArgumentException("Expected map");

        var dict = new Dictionary<string, Item>();
        foreach (var (key, value) in map.Items)
            dict[key.ToString() ?? ""] = value;

        if (!dict.TryGetValue("data", out var data) || data is not Map dataMap)
            throw new ArgumentException("Expected data map");

        if (!dict.TryGetValue("ex", out var ex) || ex is not Map exMap)
            throw new ArgumentException("Expected ex map");

        var db = new Database();
        db.Lock();

        foreach (var (key, value) in dataMap.Items)
            db._data[key.ToString() ?? ""] = value.ToString() ?? "";

        foreach (var (key, value) in exMap.Items)
        {
            if (value is not Integer i) throw new ArgumentException("Expected integer");
            db._ex[key.ToString() ?? ""] = DateTimeOffset.FromUnixTimeMilliseconds(i.Value);
        }

        db.Unlock();
        return db;
    }

    public void Save(string path)
    {
        File.WriteAllText(path + ".tmp", Encode().Encode());
        File.Create(path).Close();
        File.Replace(path + ".tmp", path, null);
    }

    public static Database Load(string path)
    {
        using var file = File.OpenRead(path);
        using var reader = new StreamReader(file);
        return Decode(Item.Decode(reader));
    }

    public static Database Link(string? path, int saveInterval)
    {
        if (path == null) return new Database();
        var db = File.Exists(path) ? Load(path) : new Database();
        db._timer = new Timer(_ => db.Save(path), null, 0, saveInterval);
        return db;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _data.Clear();
        _ex.Clear();
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}