public class LabelStatement : Statement
    {
        public string Name { get; }
        public LabelStatement(string name)
        {
            Name = name;
        }
    }