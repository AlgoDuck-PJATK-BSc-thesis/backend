using System.Text;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.CoreLexers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers.Impl;

public class KeywordLexer(char[] fileChars, FilePosition filePosition, List<Token> tokens) : LexerCore(fileChars, filePosition, tokens), IKeywordLexer
{
    private readonly StringBuilder _keywordBuffer = new();
    public Token ConsumeKeyword(char triggerChar)
    {
        _keywordBuffer.Append(triggerChar);
        while (PeekChar() != null && (char.IsLetterOrDigit(PeekChar()!.Value) || PeekChar()!.Value == '_'))
        {
            _keywordBuffer.Append(ConsumeChar());
        }
        var result = _keywordBuffer.ToString();
        var token = result switch
        {
            "private" => CreateToken(TokenType.Private),
            "public" => CreateToken(TokenType.Public),
            "protected" => CreateToken(TokenType.Protected),
            "void" => CreateToken(TokenType.Void),
            "byte" => CreateToken(TokenType.Byte),
            "short" => CreateToken(TokenType.Short),
            "int" => CreateToken(TokenType.Int),
            "long" => CreateToken(TokenType.Long),
            "float" => CreateToken(TokenType.Float),
            "double" => CreateToken(TokenType.Double),
            "var" => CreateToken(TokenType.Var),
            "char" => CreateToken(TokenType.Char),
            "boolean" => CreateToken(TokenType.Boolean),
            "static" => CreateToken(TokenType.Static),
            "final" => CreateToken(TokenType.Final),
            "class" => CreateToken(TokenType.Class),
            "String" => CreateToken(TokenType.String),
            "import" => CreateToken(TokenType.Import),
            "interface" => CreateToken(TokenType.Interface),
            "throws" => CreateToken(TokenType.Throws),
            "abstract" => CreateToken(TokenType.Abstract),
            "enum" => CreateToken(TokenType.Enum),
            "implements" => CreateToken(TokenType.Implements),
            "extends" => CreateToken(TokenType.Extends),
            "super" => CreateToken(TokenType.Super),
            "strictfp" => CreateToken(TokenType.Strictfp),
            "default" => CreateToken(TokenType.Default),
            _ => CreateToken(TokenType.Ident, result),
        };
        _keywordBuffer.Clear();
        return token;
    }
}