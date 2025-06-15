   public class SpawnStatement : Statement
    {
        public Expression X { get; }
        public Expression Y { get; }
        public SpawnStatement(Expression x, Expression y)
        {
            X = x;
            Y = y;
        }
    }
