   public class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public TokenKind Operator { get; }
        public Expression Right { get; }
        public BinaryExpression(Expression left, TokenKind op, Expression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }
