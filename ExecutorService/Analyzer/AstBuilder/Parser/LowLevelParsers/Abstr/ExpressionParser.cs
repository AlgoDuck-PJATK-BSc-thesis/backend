using ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Impl;
using OneOf;

namespace ExecutorService.Analyzer.AstBuilder.Parser.LowLevelParsers.Abstr;

public enum LiteralType
{
    
}
public class NodeTerm
{
    
}

public class NodeTermLit
{
    
}

public class NodeTermIdent
{
    
}

public class NodeTermParen
{
    
}

public class NodeBinExpr
{
    
}

public class NodeUnaryExpr
{
    
}

public class NodeExpr
{
    public OneOf<NodeTerm, NodeBinExpr, NodeUnaryExpr> Expr { get; set; }
}

public class ExpressionParser : IExpressionParser
{
    
}