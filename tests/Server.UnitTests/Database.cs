using System.Collections.Concurrent;
using System.Reflection;

namespace Server.UnitTests;

public class DatabaseTests
{
    [Theory]
    [InlineData("key", "value")]
    [InlineData("", "value")]
    [InlineData("key", "")]
    [InlineData("", "")]
    public void SetGet(string key, string value)
    {
        using var db = new Database();
        db.Set(key, value);
        var result = db.Get(key);
        Assert.Equal(value, result);
    }

    [Theory]
    [InlineData("key1", "value", 50)]
    [InlineData("key2", "value", 10)]
    public void SetGetEx(string key, string value, long ex)
    {
        using var db = new Database();
        db.Set(key, value, ex);
        var result = db.Get(key);
        Assert.Equal(value, result);

        Thread.Sleep((int)ex + 10);

        result = db.Get(key);
        Assert.Null(result);
    }

    [Fact]
    public void AutoClean()
    {
        using var db = new Database();
        db.Set("key1", "value");
        db.Set("key2", "value", 50);
        db.Set("key3", "value", 20);

        Thread.Sleep(100);

        var data = db.GetType()
          .GetField("_data", BindingFlags.NonPublic | BindingFlags.Instance)?
          .GetValue(db) as ConcurrentDictionary<string, string>;

        Assert.Equal(1, data?.Count);
    }
}