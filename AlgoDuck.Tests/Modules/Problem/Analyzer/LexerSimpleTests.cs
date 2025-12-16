using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer;

namespace AlgoDuck.Tests.Modules.Problem.Analyzer;

public class LexerSimpleTests
{
    [Fact]
    public void Tokenize_SimpleClass_ReturnsCorrectTokens()
    {
        var code = "public class Test {}";

        var tokens = LexerSimple.Tokenize(code);

        Assert.Equal(5, tokens.Count);
        Assert.Equal(TokenType.Public, tokens[0].Type);
        Assert.Equal(TokenType.Class, tokens[1].Type);
        Assert.Equal(TokenType.Ident, tokens[2].Type);
        Assert.Equal("Test", tokens[2].Value);
    }

    [Theory]
    [InlineData("123", TokenType.IntLit, "123")]
    [InlineData("123L", TokenType.LongLit, "123")]
    [InlineData("123.45f", TokenType.FloatLit, "123.45")]
    [InlineData("0x1A", TokenType.IntLit, "26")]
    public void Tokenize_NumericLiterals_ParsesCorrectly(string input, TokenType expectedType, string expectedValue)
    {
        var tokens = LexerSimple.Tokenize(input);
        Assert.Single(tokens);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Value);
    }
}