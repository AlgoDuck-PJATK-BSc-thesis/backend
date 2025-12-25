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
            "String" => CreateToken(TokenType.String),
            
            "static" => CreateToken(TokenType.Static),
            "final" => CreateToken(TokenType.Final),
            "abstract" => CreateToken(TokenType.Abstract),
            "strictfp" => CreateToken(TokenType.Strictfp),
            "default" => CreateToken(TokenType.Default),
            "transient" => CreateToken(TokenType.Transient),
            "synchronized" => CreateToken(TokenType.Synchronized),
            "volatile" => CreateToken(TokenType.Volatile),
            "native" => CreateToken(TokenType.Native),
            "return" => CreateToken(TokenType.Return),
            
            "class" => CreateToken(TokenType.Class),
            "interface" => CreateToken(TokenType.Interface),
            "enum" => CreateToken(TokenType.Enum),
            "extends" => CreateToken(TokenType.Extends),
            "implements" => CreateToken(TokenType.Implements),
            
            "import" => CreateToken(TokenType.Import),
            "package" => CreateToken(TokenType.Package),
            
            "throws" => CreateToken(TokenType.Throws),
            "throw" => CreateToken(TokenType.Throw),
            "try" => CreateToken(TokenType.Try),
            "catch" => CreateToken(TokenType.Catch),
            "finally" => CreateToken(TokenType.Finally),
            
            "new" => CreateToken(TokenType.New),
            "this" => CreateToken(TokenType.This),
            "super" => CreateToken(TokenType.Super),
            "instanceof" => CreateToken(TokenType.Instanceof),
            
            "true" => CreateToken(TokenType.BooleanLit, "true"),
            "false" => CreateToken(TokenType.BooleanLit, "false"),
            "null" => CreateToken(TokenType.NullLit, "null"),
            
            "assert" => CreateToken(TokenType.Assert),
            
            _ => CreateToken(TokenType.Ident, result),
        };
        _keywordBuffer.Clear();
        return token;
    }
}