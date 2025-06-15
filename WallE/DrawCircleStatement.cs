   public class DrawCircleStatement : Statement
    {
        public Expression DirX { get; }
        public Expression DirY { get; }
        public Expression Radius { get; }
        public DrawCircleStatement(Expression dirX, Expression dirY, Expression radius)
        {
            DirX = dirX;
            DirY = dirY;
            Radius = radius;
        }
    }

