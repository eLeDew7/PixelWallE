public enum TokenKind
{
     // Instructions
        Spawn,
        Color,
        Size,
        DrawLine,
        DrawCircle,
        DrawRectangle,
        Fill,
        // Keywords and control
        GoTo,
        // Literals
        Number,
        String,
        Identifier,
        // Operators and punctuation
        Assign,       // "<-"
        Plus,         // "+"
        Minus,        // "-"
        Star,         // "*"
        Slash,        // "/"
        Percent,      // "%"
        Power,        // "**"
        Equal,        // "=="
        Greater,      // ">"
        Less,         // "<"
        GreaterEqual, // ">="
        LessEqual,    // "<="
        And,          // "&&"
        Or,           // "||"
        LeftParen,    // "("
        RightParen,   // ")"
        LeftBracket,  // "["
        RightBracket, // "]"
        Comma,        // ","
        NewLine,
        EOF,
    EqualEqual,
    BangEqual,
    Bang,
    GetActualX,
    GetActualY,
    GetCanvasSize,
    IsBrushColor,
    GetColorCount,
    IsBrushSize,
    IsCanvasSize,
    True,
    False
}
