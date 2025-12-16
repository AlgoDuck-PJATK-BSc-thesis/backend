namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

public enum TokenType
{
    Ident, 
    
    OpenCurly, CloseCurly, OpenParen, CloseParen, OpenBrace, CloseBrace, OpenChevron, CloseChevron,

    Assign, PlusAssign, MinusAssign, MulAssign, DivAssign, ModAssign, LBitShiftAssign, RBitShiftAssign, UrBitShiftAssign,
    BitOrAssign, BitAndAssign, BitXorAssign,

    Semi,
    Colon, 
    
    Import, Package,
    
    Class, Interface, Enum,
    
    Extends, Implements, Super,

    Public, Private, Protected,

    Byte, Short, Int, Long, Float, Double, Char, Boolean, Var, String /*No string in future*/, Void /*Special*/, 

    FloatLit, DoubleLit, CharLit, BooleanLit, IntLit, LongLit, StringLit,
    NullLit, 
    
    Static, Final, Abstract, Default, Transient, Synchronized, Volatile, Strictfp,
    
    Dot, Comma,
    
    Negation, Tilde,
    
    Plus, Minus, Mul, Div, Mod, Increment, Decrement, 
    
    LBitShift, RBitShift, UrBitShift,
    
    BitAnd, LogAnd, BitOr, LogOr, BitXor,
    
    Eq, Neq, Le, Ge,
    
    Throws,
    
    Wildcard, 
    
    Arrow, 
    DoubleColon, 
    
    New, 
    This, 
    Instanceof,
    
    If, Else, For, While, Do, Switch, Case, Break, Continue, Return, Try, Catch, Finally, Throw,
    
    Native, Assert,
    
    At
}