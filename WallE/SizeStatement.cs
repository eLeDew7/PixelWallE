public class SizeStatement : Statement
    {
        public Expression Size { get; }
        public SizeStatement(Expression size)
        {
            Size = size;
        }
    }