using System.Text;

namespace Shared.Resp;

public abstract class Item
{
  static public Item Decode(StreamReader reader)
  {
    throw new NotImplementedException();
  }

  public abstract string Encode();

  protected static int Size(StreamReader reader)
  {
    if (!int.TryParse(reader.ReadLine(), out int size))
      throw new Exception("Unable to parse size");

    return size;
  }
}

public class BulkString : Item
{
  public string Data { get; }

  public BulkString(string data) { Data = data; }

  public override string Encode()
  {
    int len = Encoding.UTF8.GetByteCount(Data);
    return $"${len}\r\n{Data}\r\n";
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
    throw new NotImplementedException();
  }
}