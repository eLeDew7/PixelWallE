public class Interpreter
    {
        private readonly ProgramNode _program;
        private readonly Dictionary<string, int> _variables = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _labels = new Dictionary<string, int>();
        private int _ip = 0; // instruction pointer
        private int _x, _y;
        private int _brushSize = 1;
        private string _brushColor = "Transparent";
        private readonly int[,] _canvas;
        private readonly int _canvasSize;

        public Interpreter(ProgramNode program, int canvasSize)
        {
            _program = program;
            _canvasSize = canvasSize;
            _canvas = new int[canvasSize, canvasSize];

            // Initialize canvas to white (0)
            for (int i = 0; i < canvasSize; i++)
                for (int j = 0; j < canvasSize; j++)
                    _canvas[i, j] = 0;

            // Preprocess labels
            for (int i = 0; i < _program.Statements.Count; i++)
            {
                if (_program.Statements[i] is LabelStatement lbl)
                    _labels[lbl.Name] = i;
            }
        }

        public void Execute()
        {
            if (_program.Statements.Count == 0 || !(_program.Statements[0] is SpawnStatement))
                throw new Exception("Missing initial Spawn.");

            while (_ip < _program.Statements.Count)
            {
                var stmt = _program.Statements[_ip];
                Step(stmt);
                _ip++;
            }
        }

        private void Step(Statement stmt)
        {
            switch (stmt)
            {
                case SpawnStatement sp:
                    _x = EvaluateExpression(sp.X);
                    _y = EvaluateExpression(sp.Y);
                    if (_x < 0 || _x >= _canvasSize || _y < 0 || _y >= _canvasSize)
    throw new Exception($"Spawn coordinates ({_x}, {_y}) out of canvas bounds.");
                    break;

                case ColorStatement c:
                    _brushColor = c.ColorName;
                    break;

               case SizeStatement sz:
     int rawSize = EvaluateExpression(sz.Size);
                    if (rawSize <= 0)
                        throw new Exception("Brush size must be greater than 0.");
                    _brushSize = rawSize % 2 == 0 ? rawSize - 1 : rawSize;

    break;

                case DrawLineStatement dl:
                    int dx = EvaluateExpression(dl.DirX);
                    int dy = EvaluateExpression(dl.DirY);
                    int steps = EvaluateExpression(dl.Distance);
                    for (int i = 0; i < steps; i++)
                    {
                        Paint(_x, _y);
                        _x += dx;
                        _y += dy;
                    }
                    Paint(_x, _y);
                    break;

                case DrawCircleStatement dc:
                    int cdx = EvaluateExpression(dc.DirX);
                    int cdy = EvaluateExpression(dc.DirY);
                    int radius = EvaluateExpression(dc.Radius);
                    int ccx = _x + cdx * radius;
                    int ccy = _y + cdy * radius;
                    for (int angle = 0; angle < 360; angle++)
                    {
                        double rad = angle * Math.PI / 180;
                        int px = ccx + (int)(radius * Math.Cos(rad));
                        int py = ccy + (int)(radius * Math.Sin(rad));
                        Paint(px, py);
                    }
                    break;

                case DrawRectangleStatement dr:
                    int rdx = EvaluateExpression(dr.DirX);
                    int rdy = EvaluateExpression(dr.DirY);
                    int rdist = EvaluateExpression(dr.Distance);
                    int width = EvaluateExpression(dr.Width);
                    int height = EvaluateExpression(dr.Height);
                    int baseX = _x + rdx * rdist;
                    int baseY = _y + rdy * rdist;
                    for (int i = 0; i < width; i++)
                        for (int j = 0; j < height; j++)
                            Paint(baseX + i, baseY + j);
                    break;

                case FillStatement _:
                    int targetColor = _canvas[_x, _y];
                    int newColor = _brushColor.GetHashCode();
                    if (targetColor != newColor)
                        FloodFill(_x, _y, targetColor, newColor);
                    break;

                case AssignStatement asg:
                    int value = EvaluateExpression(asg.Value);
                    _variables[asg.Target] = value;
                    break;

                case ConditionalGotoStatement cg:
                    int cond = EvaluateExpression(cg.Condition);
                    if (cond != 0)
                    {
                        if (!_labels.ContainsKey(cg.Label))
                            throw new Exception($"Undefined label: {cg.Label}");
                        _ip = _labels[cg.Label] - 1;
                    }
                    break;

                case LabelStatement _:
                    break;

                default:
                    throw new Exception($"Unknown statement type: {stmt.GetType().Name}");
            }
        }

        private void Paint(int cx, int cy)
        {
            int r = _brushSize / 2;
            for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    int nx = cx + dx;
                    int ny = cy + dy;
                    if (nx >= 0 && nx < _canvasSize && ny >= 0 && ny < _canvasSize)
                        _canvas[nx, ny] = _brushColor.GetHashCode();
                }
        }

        private void FloodFill(int x, int y, int targetColor, int newColor)
        {
            var dirs = new (int dx, int dy)[] { (1,0),(-1,0),(0,1),(0,-1) };
            var queue = new Queue<(int x,int y)>();
            queue.Enqueue((x,y));
            _canvas[x, y] = newColor;
            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                foreach (var (dx, dy) in dirs)
                {
                    int nx = cx + dx;
                    int ny = cy + dy;
                    if (nx >= 0 && nx < _canvasSize && ny >= 0 && ny < _canvasSize)
                    {
                        if (_canvas[nx, ny] == targetColor)
                        {
                            _canvas[nx, ny] = newColor;
                            queue.Enqueue((nx, ny));
                        }
                    }
                }
            }
        }

        private int EvaluateExpression(Expression expr)
        {
            switch (expr)
            {
                case LiteralExpression lit:
                    if (lit.Value is bool b)
                    return b ? 1 : 0;
                    return Convert.ToInt32(lit.Value);    
                case VariableExpression var:
                    if (!_variables.TryGetValue(var.Name, out var v))
                        throw new Exception($"Undefined variable {var.Name}");
                    return v;
                case BinaryExpression bin:
                    int left = EvaluateExpression(bin.Left);
                    int right = EvaluateExpression(bin.Right);
                    return bin.Operator switch
                    {
                        TokenKind.Plus => left + right,
                        TokenKind.Minus => left - right,
                        TokenKind.Star => left * right,
                        TokenKind.Slash => left / right,
                        TokenKind.Percent => left % right,
                        TokenKind.Power => (int)Math.Pow(left, right),
                        TokenKind.EqualEqual => left == right ? 1 : 0,
                        TokenKind.Greater => left > right ? 1 : 0,
                        TokenKind.Less => left < right ? 1 : 0,
                        TokenKind.GreaterEqual => left >= right ? 1 : 0,
                        TokenKind.LessEqual => left <= right ? 1 : 0,
                        TokenKind.And => (left != 0 && right != 0) ? 1 : 0,
                        TokenKind.Or => (left != 0 || right != 0) ? 1 : 0,
                        _ => throw new Exception($"Unsupported binary operator {bin.Operator}")
                    };
                case GroupingExpression grp:
                    return EvaluateExpression(grp.Inner);
                case FunctionCallExpression fn:
                    return EvaluateFunctionCall(fn);
                default:
                    throw new Exception($"Unsupported expression type: {expr.GetType().Name}");
            }
        }

        private int EvaluateFunctionCall(FunctionCallExpression fn)
        {
            switch (fn.Name)
            {
                case "GetActualX":
                    return _x;
                case "GetActualY":
                    return _y;
                case "GetCanvasSize":
                    return _canvasSize;
                case "GetColorCount":
                    // args: string color, int x1, int y1, int x2, int y2
                    var colorArg = fn.Arguments[0] as LiteralExpression;
                    string colorName = colorArg?.Value as string ?? string.Empty;
                    int x1 = EvaluateExpression(fn.Arguments[1]);
                    int y1 = EvaluateExpression(fn.Arguments[2]);
                    int x2 = EvaluateExpression(fn.Arguments[3]);
                    int y2 = EvaluateExpression(fn.Arguments[4]);
                    int count = 0;
                    int minX = Math.Min(x1, x2), maxX = Math.Max(x1, x2);
                    int minY = Math.Min(y1, y2), maxY = Math.Max(y1, y2);
                    for (int ix = minX; ix <= maxX; ix++)
                        for (int iy = minY; iy <= maxY; iy++)
                        {
                            if (ix < 0 || ix >= _canvasSize || iy < 0 || iy >= _canvasSize) return 0;
                            if (_canvas[ix, iy] == colorName.GetHashCode()) count++;
                        }
                    return count;
                case "IsBrushColor":
                    // args: string color
                    var brushArg = fn.Arguments[0] as LiteralExpression;
                    string brushName = brushArg?.Value as string ?? string.Empty;
                    return _brushColor == brushName ? 1 : 0;
                case "IsBrushSize":
                    // args: int size
                    int sizeArg = EvaluateExpression(fn.Arguments[0]);
                    return _brushSize == sizeArg ? 1 : 0;
                case "IsCanvasColor":
                    // args: string color, int vertical, int horizontal
                    var canvasArg = fn.Arguments[0] as LiteralExpression;
                    string canvasColor = canvasArg?.Value as string ?? string.Empty;
                    int vert = EvaluateExpression(fn.Arguments[1]);
                    int horz = EvaluateExpression(fn.Arguments[2]);
                    int tx = _x + horz;
                    int ty = _y + vert;
                    if (tx < 0 || tx >= _canvasSize || ty < 0 || ty >= _canvasSize) return 0;
                    return _canvas[tx, ty] == canvasColor.GetHashCode() ? 1 : 0;
                default:
                    throw new Exception($"Unknown function {fn.Name}");
            }
        }
        }

