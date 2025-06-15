public class ConditionalGotoStatement : Statement
    {
        public string Label { get; }
        public Expression Condition { get; }
        public ConditionalGotoStatement(string label, Expression condition)
        {
            Label = label;
            Condition = condition;
        }
    }