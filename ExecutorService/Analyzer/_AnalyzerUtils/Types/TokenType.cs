namespace ExecutorService.Analyzer._AnalyzerUtils.Types;

public enum TokenType
{
    Ident, 
    
    OpenCurly, CloseCurly, OpenParen, CloseParen, OpenBrace, CloseBrace, OpenChevron, CloseChevron,

    Assign, 

    Semi,
    
    Import, Package,
    
    Class, Interface, Enum,
    
    Extends, Implements, Super,

    Public, Private, Protected,

    Byte, Short, Int, Long, Float, Double, Char, Boolean, String /*No string in future*/, Void /*Special*/, 

    FloatLit, DoubleLit, CharLit, BooleanLit, IntLit, LongLit, StringLit,
    
    Static, Final, Abstract, Default, Transient, Synchronized, Volatile, Strictfp,
    
    Dot, Comma,
    
    Plus, Minus, Mul, Div, Mod, Increment, Decrement,
    
    And, LAnd, Or, LOr, Xor, LXor, Not, LNot,
    
    Eq, Neq, Le, Ge,
    
    Throws,
    
    Wildcard,
    
}
