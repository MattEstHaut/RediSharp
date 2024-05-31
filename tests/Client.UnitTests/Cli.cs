namespace Client.UnitTests
{
    public class CliTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("GET", "GET")]
        [InlineData("GET key", "GET", "key")]
        [InlineData("SET key value", "SET", "key", "value")]
        [InlineData("SET key \"value with spaces\"", "SET", "key", "value with spaces")]
        [InlineData("SET key 'value with spaces'", "SET", "key", "value with spaces")]
        [InlineData("SET key 'value with \\'quotes\\''", "SET", "key", "value with 'quotes'")]
        public void Tokenize(string command, params string[] expected)
        {
            var tokens = CLI.Tokenize(command);
            Assert.Equal(expected, tokens);
        }
    }
}