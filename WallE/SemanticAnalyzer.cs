 public enum SymbolType { Integer, Boolean }
public class SemanticAnalyzer
    {
        private readonly ProgramNode _program;
        private readonly List<string> _errors = new List<string>();
        private readonly Dictionary<string, SymbolType> _variableTypes = new Dictionary<string, SymbolType>();
        private readonly HashSet<string> _labelNames = new HashSet<string>();
        private bool _spawnSeen = false;

        public SemanticAnalyzer(ProgramNode program)
        {
            _program = program;
        }

        public IReadOnlyList<string> Errors => _errors;

        public void Analyze()
        {
            // 1. Check spawn first
            if (_program.Statements.Count == 0 || !(_program.Statements[0] is SpawnStatement))
                _errors.Add("Semantic Error: 'Spawn' must be the first statement.");
            else
                _spawnSeen = true;

            // Collect labels
            foreach (var stmt in _program.Statements)
            {
                if (stmt is LabelStatement lbl)
                {
                    if (_labelNames.Contains(lbl.Name))
                        _errors.Add($"Semantic Error: Duplicate label '{lbl.Name}'.");
                    else
                        _labelNames.Add(lbl.Name);
                }
            }

            // Visit statements
            foreach (var stmt in _program.Statements)
                CheckStatement(stmt);

            // Check that at least one spawn was present
            if (!_spawnSeen)
                _errors.Add("Semantic Error: Missing 'Spawn' statement.");
        }

        private void CheckStatement(Statement stmt)
        {
            switch (stmt)
            {
                case SpawnStatement sp:
                    // spawn args are literal ints
                   if (CheckExpressionType(sp.X) != SymbolType.Integer)
    _errors.Add("Semantic Error: Spawn X must evaluate to an integer.");

if (CheckExpressionType(sp.Y) != SymbolType.Integer)
    _errors.Add("Semantic Error: Spawn Y must evaluate to an integer.");
                    break;
                case ColorStatement _:
                    // OK
                    break;
                case SizeStatement sz:
                  if (CheckExpressionType(sz.Size) != SymbolType.Integer)
    {
        _errors.Add("Semantic Error: Size must be an integer expression.");
    }
    break;
                case DrawLineStatement dl:
                    CheckDirection(dl.DirX, "DrawLine dirX");
                    CheckDirection(dl.DirY, "DrawLine dirY");
                    break;
                case DrawCircleStatement dc:
                    CheckDirection(dc.DirX, "DrawCircle dirX");
                    CheckDirection(dc.DirY, "DrawCircle dirY");
                    break;
                case DrawRectangleStatement dr:
                    CheckDirection(dr.DirX, "DrawRect dirX");
                    CheckDirection(dr.DirY, "DrawRect dirY");
                    break;
                case FillStatement _:
                    break;
                case AssignStatement asg:
                    var type = CheckExpressionType(asg.Value);
                    _variableTypes[asg.Target] = type;
                    Console.WriteLine($"[DEBUG SEM] Variable '{asg.Target}' inferida como {type}");
                    break;
                case ConditionalGotoStatement cg:
                    if (!_labelNames.Contains(cg.Label))
                        _errors.Add($"Semantic Error: Undefined label '{cg.Label}' in GoTo.");
                    var condType = CheckExpressionType(cg.Condition);
                    if (condType != SymbolType.Boolean)
                        _errors.Add("Semantic Error: GoTo condition must be boolean.");
                    break;
                default:
                    break;
            }
        }

        private SymbolType CheckExpressionType(Expression expr)
        {
            switch (expr)
            {
                case LiteralExpression lit:
                    if (lit.Value is int) return SymbolType.Integer;
                    if (lit.Value is bool) return SymbolType.Boolean;
                    break;
                case VariableExpression var:
                    if (_variableTypes.TryGetValue(var.Name, out var t)) return t;
                    _errors.Add($"Semantic Error: Undefined variable '{var.Name}'.");
                    return SymbolType.Integer;
                case BinaryExpression bin:
                    var left = CheckExpressionType(bin.Left);
                    var right = CheckExpressionType(bin.Right);
                    switch (bin.Operator)
                    {
                        case TokenKind.Plus:
                        case TokenKind.Minus:
                        case TokenKind.Star:
                        case TokenKind.Slash:
                        case TokenKind.Percent:
                        case TokenKind.Power:
                            if (left == SymbolType.Integer && right == SymbolType.Integer)
                                return SymbolType.Integer;
                            break;
                        case TokenKind.Greater:
                        case TokenKind.Less:
                        case TokenKind.GreaterEqual:
                        case TokenKind.LessEqual:
                         if (left == SymbolType.Integer && right == SymbolType.Integer)
                                return SymbolType.Boolean;
                                break;
                        case TokenKind.EqualEqual:
                        case TokenKind.BangEqual:
                            if (left == SymbolType.Integer && right == SymbolType.Integer)
                                return SymbolType.Boolean;
                            if (left == SymbolType.Boolean && right == SymbolType.Boolean)
                            return SymbolType.Boolean;
                            if (left == SymbolType.Boolean
                                && bin.Right is LiteralExpression rightLit
                                && rightLit.Value is int rv
                                && (rv == 0 || rv == 1))
                                {
                                return SymbolType.Boolean;
                                }
                                 if (right == SymbolType.Boolean
                                        && bin.Left is LiteralExpression leftLit
                                        && leftLit.Value is int lv
                                        && (lv == 0 || lv == 1))
                                            {
                                        return SymbolType.Boolean;
                                            }    
                            break;
                        case TokenKind.And:
                        case TokenKind.Or:
                            if (left == SymbolType.Boolean && right == SymbolType.Boolean)
                                return SymbolType.Boolean;
                            break;
                    }
                    _errors.Add($"Semantic Error: Type mismatch in binary expression '{bin.Operator}'.");
                    return left;
                case FunctionCallExpression fn:
                    // All built-in functions return integer, except those returning boolean encoded as int
                    // Validate arity
                    // (Could add more checks)
                    return SymbolType.Integer;
                case GroupingExpression grp:
                    return CheckExpressionType(grp.Inner);
            }

            // Default fallback
            return SymbolType.Integer;
        }

        private void ValidateLiteralInt(Expression expr, string context)
        {
            if (expr is LiteralExpression lit && lit.Value is int)
                return;
            _errors.Add($"Semantic Error: {context} must be integer literal.");
        }

        private void CheckDirection(Expression expr, string context)
        {
            if (CheckExpressionType(expr) != SymbolType.Integer)
    {
        _errors.Add($"Semantic Error: {context} must be an integer expression.");
        return;
    }

    // 2) Si es literal, verificar rango -1..1
    if (expr is LiteralExpression lit && lit.Value is int v)
    {
        if (v < -1 || v > 1)
            _errors.Add($"Semantic Error: {context} literal must be -1, 0 or 1.");
    }
        }
    }

