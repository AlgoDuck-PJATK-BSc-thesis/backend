using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

namespace AlgoDuck.Shared.Analyzer.AstBuilder.Lexer.HelperLexers.Abstr;

public interface ILiteralLexer
{
    public static readonly int[][] BinaryLiteralRange = new int[][] { [48, 49] };
    public static readonly int[][] OctalLiteralRange = new int[][] { [48, 55] };
    public static readonly int[][] DecimalLiteralRange = new int[][] { [48, 57] };
    public static readonly int[][] HexadecimalLiteralRange = new int[][] { [48, 57], [65, 70], [97, 102] };

    public Token ConsumeStringLit();
    public Token ConsumeCharLit();
    public Token ConsumeNumericLit(char consumed);


}