public class Token
{
 public Token(TokenKind kind, string text, object value, int line){
    Kind = kind;
    Line = line;
    Text = text;
    Value = value;
    
 }

    public object Value { get; }
    public TokenKind Kind { get; }
    public int Line { get; }
    public string Text {get;}
    public override string ToString() => $"{Kind} '{Text}'";
}
