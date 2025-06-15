 public class DrawLineStatement : Statement
    {
        public Expression DirX { get; }
        public Expression DirY { get; }
        public Expression Distance { get; }
        public DrawLineStatement(Expression dirX, Expression dirY, Expression distance)
        {
            DirX = dirX;
            DirY = dirY;
            Distance = distance;
        }
    }