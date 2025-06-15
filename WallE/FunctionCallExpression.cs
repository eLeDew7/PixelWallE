public class FunctionCallExpression : Expression
    {
        public string Name { get; }
        public List<Expression> Arguments { get; }
        public FunctionCallExpression(string name, List<Expression> arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }