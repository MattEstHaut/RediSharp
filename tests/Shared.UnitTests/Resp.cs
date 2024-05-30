using System.Text;
using Shared.Resp;

namespace Shared.unitTests;

public class RespTests
{
  [Theory]
  [InlineData("+OK\r\n", "OK")]
  [InlineData("-NOT OK\r\n", "NOT OK")]
  [InlineData("$5\r\nHELLO\r\n", "HELLO")]
  [InlineData("$6\r\n\nWORLD\r\n", "\nWORLD")]
  [InlineData("$0\r\n\r\n", "")]
  public void StringDecode(string input, string expected)
  {
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
    using var reader = new StreamReader(stream);

    var result = Item.Decode(reader);

    Assert.Equal(expected, result.ToString());
  }
}