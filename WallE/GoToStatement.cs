public class GoToStatement : Statement
    {
        public Expression X { get; }
        public Expression Y { get; }
        public GoToStatement(Expression x, Expression y)
        {
            X = x; Y = y;
        }
    }



