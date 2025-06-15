 public class AssignStatement : Statement
    {
        public string Target { get; }
        public Expression Value { get; }
        public AssignStatement(string target, Expression value)
        {
            Target = target;
            Value = value;
        }
    }
