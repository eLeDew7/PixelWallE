  public class DrawRectangleStatement : Statement
    {
        public Expression DirX { get; }
        public Expression DirY { get; }
        public Expression Distance { get; }
        public Expression Width { get; }
        public Expression Height { get; }
        public DrawRectangleStatement(Expression dirX, Expression dirY, Expression distance, Expression width, Expression height)
        {
            DirX = dirX;
            DirY = dirY;
            Distance = distance;
            Width = width;
            Height = height;
        }
    }

