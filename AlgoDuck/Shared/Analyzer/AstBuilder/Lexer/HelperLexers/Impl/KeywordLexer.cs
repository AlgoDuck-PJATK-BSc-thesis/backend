using System.Text;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.CoreLexers;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers.Impl;

public class KeywordLexer(char[] fileChars, FilePosition filePosition, List<Token> tokens) : LexerCore(fileChars, filePosition, tokens), IKeywordLexer
{
    private readonly StringBuilder _keywordBuffer = new();
    private readonly FilePosition _filePosition = filePosition;
    
    public Token ConsumeKeyword(char triggerChar)
    {
        var startPos = _filePosition.GetFilePos() - 1;
        _keywordBuffer.Append(triggerChar);
        while (PeekChar() != null && (char.IsLetterOrDigit(PeekChar()!.Value) || PeekChar()!.Value == '_'))
        {
            _keywordBuffer.Append(ConsumeChar());
        }
        var result = _keywordBuffer.ToString();
        var token = result switch
        {
            "private" => CreateToken(TokenType.Private, startPos),
            "public" => CreateToken(TokenType.Public, startPos),
            "protected" => CreateToken(TokenType.Protected, startPos),
            "void" => CreateToken(TokenType.Void, startPos),
            "byte" => CreateToken(TokenType.Byte, startPos),
            "short" => CreateToken(TokenType.Short, startPos),
            "int" => CreateToken(TokenType.Int, startPos),
            "long" => CreateToken(TokenType.Long, startPos),
            "float" => CreateToken(TokenType.Float, startPos),
            "double" => CreateToken(TokenType.Double, startPos),
            "var" => CreateToken(TokenType.Var, startPos),
            "char" => CreateToken(TokenType.Char, startPos),
            "boolean" => CreateToken(TokenType.Boolean, startPos),
            "String" => CreateToken(TokenType.String, startPos),
            "static" => CreateToken(TokenType.Static, startPos),
            "final" => CreateToken(TokenType.Final, startPos),
            "abstract" => CreateToken(TokenType.Abstract, startPos),
            "strictfp" => CreateToken(TokenType.Strictfp, startPos),
            "default" => CreateToken(TokenType.Default, startPos),
            "transient" => CreateToken(TokenType.Transient, startPos),
            "synchronized" => CreateToken(TokenType.Synchronized, startPos),
            "volatile" => CreateToken(TokenType.Volatile, startPos),
            "native" => CreateToken(TokenType.Native, startPos),
            "return" => CreateToken(TokenType.Return, startPos),
            "class" => CreateToken(TokenType.Class, startPos),
            "interface" => CreateToken(TokenType.Interface, startPos),
            "record" => CreateToken(TokenType.Record, startPos),
            "enum" => CreateToken(TokenType.Enum, startPos),
            "extends" => CreateToken(TokenType.Extends, startPos),
            "implements" => CreateToken(TokenType.Implements, startPos),
            "import" => CreateToken(TokenType.Import, startPos),
            "package" => CreateToken(TokenType.Package, startPos),
            "throws" => CreateToken(TokenType.Throws, startPos),
            "throw" => CreateToken(TokenType.Throw, startPos),
            "try" => CreateToken(TokenType.Try, startPos),
            "catch" => CreateToken(TokenType.Catch, startPos),
            "finally" => CreateToken(TokenType.Finally, startPos),
            "new" => CreateToken(TokenType.New, startPos),
            "this" => CreateToken(TokenType.This, startPos),
            "super" => CreateToken(TokenType.Super, startPos),
            "instanceof" => CreateToken(TokenType.Instanceof, startPos),
            "true" => CreateToken(TokenType.BooleanLit, startPos, "true"),
            "null" => CreateToken(TokenType.NullLit, startPos, "null"),
            "assert" => CreateToken(TokenType.Assert, startPos),
            "false" => CreateToken(TokenType.BooleanLit, startPos, "false"),
            
            _ => CreateToken(TokenType.Ident, startPos, result),
        };
        _keywordBuffer.Clear();
        return token;
    }
}