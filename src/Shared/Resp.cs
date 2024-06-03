using System.Text;

namespace Shared.Resp;

public abstract class Item
{
    static public Item Decode(StreamReader reader)
    {
        char type = (char)reader.Peek();

        return type switch
        {
            '+' => SimpleString.Decode(reader),
            '-' => SimpleError.Decode(reader),
            '_' => Null.Decode(reader),
            '*' => ItemArray.Decode(reader),
            '$' => BulkString.Decode(reader),
            _ => throw new Exception("Invalid item type")
        };
    }

    public abstract string Encode();

    protected static int Size(StreamReader reader)
    {
        if (!int.TryParse(reader.ReadLine(), out int size))
            throw new Exception("Unable to parse size");

        return size;
    }

    public static implicit operator Item(string data) => new BulkString(data);

    public static implicit operator Item(Item[] items) => new ItemArray(items);

    public static implicit operator Item(string[] data)
    {
        return new ItemArray(data.Select(d => new BulkString(d)).ToArray());
    }
}

public class BulkString : Item
{
    public string Data { get; }

    public BulkString(string data) { Data = data; }

    public override string Encode()
    {
        return $"${Data.Length}\r\n{Data}\r\n";
    }

    public override string ToString() => Data;

    public new static BulkString Decode(StreamReader reader)
    {
        if ((char)reader.Read() != '$')
            throw new Exception("Invalid bulk string header");

        int size = Size(reader);
        if (size < 0) throw new Exception("Size less than zero");

        char[] buffer = new char[size];
        if (reader.Read(buffer) < size) throw new EndOfStreamException();
        _ = reader.ReadLine();

        return new BulkString(new string(buffer));
    }
}

public class SimpleString : BulkString
{
    public SimpleString(string data) : base(data) { }

    public override string Encode() => $"+{Data}\r\n";

    public new static SimpleString Decode(StreamReader reader)
    {
        if ((char)reader.Read() != '+')
            throw new Exception("Invalid simple string header");

        if (reader.ReadLine() is not string data)
            throw new Exception("Unable to read simple string");

        return new SimpleString(data);
    }
}

public class SimpleError : BulkString
{
    public SimpleError(string data) : base(data) { }

    public override string Encode() => $"-{Data}\r\n";

    public new static SimpleError Decode(StreamReader reader)
    {
        if ((char)reader.Read() != '-')
            throw new Exception("Invalid simple error header");

        if (reader.ReadLine() is not string data)
            throw new Exception("Unable to read simple error");

        return new SimpleError(data);
    }
}

public class Null : Item
{
    public override string Encode() => "_\r\n";

    public override string ToString() => "null";

    public new static Null Decode(StreamReader reader)
    {
        if ((char)reader.Read() != '_')
            throw new Exception("Invalid null header");
        _ = reader.ReadLine();
        return new Null();
    }
}

public class ItemArray : Item
{
    public List<Item> Items { get; } = new List<Item>();

    public ItemArray() { }

    public ItemArray(params Item[] items)
    {
        Items.AddRange(items);
    }

    public override string Encode()
    {
        var sb = new StringBuilder();
        sb.Append($"*{Items.Count}\r\n");

        foreach (var item in Items)
            sb.Append(item.Encode());

        return sb.ToString();
    }

    public override string ToString() => $"[{string.Join(", ", Items)}]";

    public new static ItemArray Decode(StreamReader reader)
    {
        if ((char)reader.Read() != '*')
            throw new Exception("Invalid item array header");

        int size = Size(reader);
        if (size < 0) throw new Exception("Size less than zero");
        var items = new List<Item>();

        for (int i = 0; i < size; i++)
            items.Add(Item.Decode(reader));

        return new ItemArray(items.ToArray());
    }
}

public class Integer : Item
{
    public long Value { get; }

    public Integer(long value) { Value = value; }

    public override string Encode() => $":{Value}\r\n";

    public override string ToString() => Value.ToString();

    public new static Integer Decode(StreamReader reader)
    {
        if ((char)reader.Read() != ':')
            throw new Exception("Invalid integer header");

        if (!long.TryParse(reader.ReadLine(), out long value))
            throw new Exception("Unable to parse integer");

        return new Integer(value);
    }
}

public class Boolean : Item {
    public bool Value { get; }

    public Boolean(bool value) { Value = value; }

    public override string Encode() => $"#{(Value ? 't' : 'f')}\r\n";

    public override string ToString() => Value.ToString();

    public new static Boolean Decode(StreamReader reader)
    {
        if ((char)reader.Read() != '#')
            throw new Exception("Invalid boolean header");

        char value = (char)reader.Read();
        if (value != 't' && value != 'f')
            throw new Exception("Invalid boolean value");

        _ = reader.ReadLine();
        return new Boolean(value == 't');
    }
}

public class Map : Item {
  public Dictionary<Item, Item> Items { get; } = new Dictionary<Item, Item>();

  public Map() { }

  public Map(Dictionary<Item, Item> items)
  {
      Items = items.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
  }

  public override string ToString()
  {
      var sb = new StringBuilder();
      sb.Append("{");

      foreach (var (key, value) in Items)
          sb.Append($"{key}: {value}, ");

      if (Items.Count > 0)
          sb.Length -= 2;

      sb.Append("}");
      return sb.ToString();
  }

  public override string Encode()
  {
      var sb = new StringBuilder();
      sb.Append($"%{Items.Count}\r\n");

      foreach (var (key, value) in Items)
      {
          sb.Append(key.Encode());
          sb.Append(value.Encode());
      }

      return sb.ToString();
  }

  public new static Map Decode(StreamReader reader)
  {
      if ((char)reader.Read() != '%')
          throw new Exception("Invalid map header");

      int size = Size(reader);
      if (size < 0) throw new Exception("Size less than zero");
      var items = new Dictionary<Item, Item>();

      for (int i = 0; i < size; i++)
      {
          var key = Item.Decode(reader);
          var value = Item.Decode(reader);
          items.Add(key, value);
      }

      return new Map(items);
  }
}