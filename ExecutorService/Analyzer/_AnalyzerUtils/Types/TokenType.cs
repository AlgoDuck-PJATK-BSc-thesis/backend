namespace ExecutorService.Analyzer._AnalyzerUtils.Types;

public enum TokenType
{
    Ident, 
    
    OpenCurly, CloseCurly, OpenParen, CloseParen, OpenBrace, CloseBrace, OpenChevron, CloseChevron,

    Assign, PlusAssign, MinusAssign, MulAssign, DivAssign, ModAssign, LBitShiftAssign, RBitShiftAssign, UrBitShiftAssign,

    Semi,
    
    Import, Package,
    
    Class, Interface, Enum,
    
    Extends, Implements, Super,

    Public, Private, Protected,

    Byte, Short, Int, Long, Float, Double, Char, Boolean, String /*No string in future*/, Void /*Special*/, 

    FloatLit, DoubleLit, CharLit, BooleanLit, IntLit, LongLit, StringLit,
    
    Static, Final, Abstract, Default, Transient, Synchronized, Volatile, Strictfp,
    
    Dot, Comma,
    
    Negation, // TOOD put unaries here
    
    Plus, Minus, Mul, Div, Mod, Increment, Decrement, LBitShift, RBitShift, UrBitShift,
    
    BitAnd, LogAnd, BitOr, LogOr, BitXor, BitOrAssign, BitAndAssign, BitXorAssign,
    
    Eq, Neq, Le, Ge,
    
    Throws,
    
    Wildcard,
    
}
