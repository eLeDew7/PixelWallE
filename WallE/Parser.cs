
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;
        private readonly List<string> _errors = new();
        private bool _hasSpawn = false;
        private readonly Dictionary<string, int> _labelDefinitions = new();
        private readonly List<(string label, Token token)> _labelReferences = new();


        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public bool HasErrors => _errors.Count > 0;
        public IReadOnlyList<string> Errors => _errors;

        public ProgramNode Parse()
        {
            var program = new ProgramNode();
            bool spawnFound = false;
            while (!IsAtEnd())
            {
                var stmt = ParseStatement();
                if (stmt != null){

                    program.Statements.Add(stmt);

                    if (!spawnFound)
            {
                if (stmt is SpawnStatement)
                {
                    spawnFound = true;
                    _hasSpawn = true;
                }
                else
                {
                    _errors.Add($"[Line {1}] Error: 'Spawn' must be the first executable statement");
                }
            }
                }
            }

            if (!_hasSpawn)
                _errors.Add("Missing 'Spawn(...)' instruction at the beginning.");

            // Validación de etiquetas y GoTo:
            var labels = new HashSet<string>();
            foreach (var s in program.Statements)
            {
                if (s is LabelStatement label)
                    labels.Add(label.Name);
            }
            foreach (var s in program.Statements)
            {
                if (s is ConditionalGotoStatement goTo)
                {
                    if (!labels.Contains(goTo.Label))
                        _errors.Add($"[Line ???] Unknown label '{goTo.Label}' in GoTo.");
                }
            }
            foreach (var (label, token) in _labelReferences)
            {
            if (!_labelDefinitions.ContainsKey(label))
            {
            _errors.Add($"[Line {token.Line}] Error: Undefined label '{label}'");
            }
            }

            return program;
        }

        private Statement? ParseStatement()
        {
            try
            {
                if (Match(TokenKind.Spawn))
                {
                    if (_hasSpawn)
                        throw new Exception("Only one 'Spawn' allowed.");
                    _hasSpawn = true;
                    return ParseSpawn();
                }

                if (Match(TokenKind.Color))
                    return ParseColor();

                if (Match(TokenKind.Size))
                    return ParseSize();

                if (Match(TokenKind.DrawLine))
                    return ParseDrawLine();

                if (Match(TokenKind.DrawCircle))
                    return ParseDrawCircle();

                if (Match(TokenKind.DrawRectangle))
                    return ParseDrawRectangle();

                if (Match(TokenKind.Fill))
                    return ParseFill();
                
                 if (Check(TokenKind.Identifier) && (PeekNext().Kind == TokenKind.NewLine || PeekNext().Kind == TokenKind.EOF))
                    return ParseLabel();

                if (Check(TokenKind.Identifier) && PeekNext().Kind == TokenKind.Assign)
                    return ParseAssign();

                if (Match(TokenKind.GoTo))
                    return ParseConditionalGoto();

                if (Match(TokenKind.NewLine))
                    return null;

                AddError(Peek(), "Expected a statement, assignment or label.");
                Advance();
            }
            catch (Exception ex)
            {
                AddError(Peek(), ex.Message);
                Synchronize();
            }
            return null;
        }

        private SpawnStatement ParseSpawn()
        {
             Consume(TokenKind.LeftParen, "Expected '(' after Spawn.");

            // Parsear dos argumentos
            var xExpr = ParseExpression();
            Consume(TokenKind.Comma, "Expected ',' between x and y in Spawn.");
            var yExpr = ParseExpression();

            // No permitir argumentos extras
            if (Check(TokenKind.Comma))
                AddError(Peek(), "Spawn takes exactly two arguments.");

            Consume(TokenKind.RightParen, "Expected ')' after Spawn arguments.");

            // Aceptar un salto de línea o EOF
            if (Match(TokenKind.NewLine))
            {
                // consumido salto de línea
            }
            else if (Peek().Kind == TokenKind.EOF)
            {
                // fin de archivo, válido
            }
            else
            {
                AddError(Peek(), "Expected end of line after Spawn.");
            }

            // Validar que los argumentos sean literales enteras
            //if (!(xExpr is LiteralExpression xLit && xLit.Value is int))
             //   AddError(Peek(), "Spawn first argument must be an integer literal.");
            //if (!(yExpr is LiteralExpression yLit && yLit.Value is int))
              //  AddError(Peek(), "Spawn second argument must be an integer literal.");

            return new SpawnStatement(xExpr, yExpr);
        }

        private ColorStatement ParseColor()
        {
             Consume(TokenKind.LeftParen, "Expected '(' after Color.");

            // Aceptar string literal o identificador
            Token colorToken;
            if (Match(TokenKind.String))
            {
            colorToken = Previous();
            }
            else
            {
            colorToken = Consume(TokenKind.Identifier, "Expected color name as string or identifier for Color.");
            }
            // Obtener nombre de color (sin las comillas si venían)
            var colorName = colorToken.Kind == TokenKind.String
            ? colorToken.Text.Trim('\"')
            : colorToken.Text;
        
    
            Consume(TokenKind.RightParen, "Expected ')' after Color argument.");
    
            // Aceptar EOF o Newline
            if (!IsAtEnd() && !Check(TokenKind.NewLine) && !Check(TokenKind.EOF))
                {
                AddError(Peek(), "Expected end of line after Color.");
                 }
                else if (Match(TokenKind.NewLine))
                {
                    // Consumir el newline
                }

                return new ColorStatement(colorName);
                }

        private SizeStatement ParseSize()
        {
            Consume(TokenKind.LeftParen, "Expected '(' after Size.");
            var sizeExpr = ParseExpression();
            //var sizeToken = Consume(TokenKind.Number, "Expected integer literal for size.");
            Consume(TokenKind.RightParen, "Expected ')' after Size argument.");
              if (!IsAtEnd() && !Check(TokenKind.NewLine) && !Check(TokenKind.EOF))
        {
        AddError(Peek(), "Expected end of line after Size.");
        }
        else if (Match(TokenKind.NewLine))
        {
        // Consumir el newline
        }

            //int valor = (int)sizeToken.Value!;
            //if (valor <= 0)
              //  throw new Exception("Size must be > 0.");
            //if (valor % 2 == 0)
              //  valor -= 1;
            return new SizeStatement(sizeExpr);
        }
           private AssignStatement ParseAssign()
        {
            var idToken = Consume(TokenKind.Identifier, "Expected variable name.");
            if (char.IsDigit(idToken.Text[0]) || idToken.Text[0] == '_')
                throw new Exception("Variable names cannot start with a digit or underscore.");

            Consume(TokenKind.Assign, "Expected '<-' after variable name.");
            var expr = ParseExpression();
             if (!IsAtEnd() && !Check(TokenKind.NewLine) && !Check(TokenKind.EOF))
        {
        AddError(Peek(), "Expected end of line after <-.");
        }
        else if (Match(TokenKind.NewLine))
        {
        // Consumir el newline
        }
            return new AssignStatement(idToken.Text, expr);
        }

        private DrawLineStatement ParseDrawLine()
        {
            Consume(TokenKind.LeftParen, "Expected '(' after DrawLine.");
            var dirXExpr = ParseExpression();
            RestrictToDirections(dirXExpr, "dirX");
            Consume(TokenKind.Comma, "Expected ',' after dirX.");

            var dirYExpr = ParseExpression();
            RestrictToDirections(dirYExpr, "dirY");
            Consume(TokenKind.Comma, "Expected ',' after dirY.");

            var distanceExpr = ParseExpression();
            Consume(TokenKind.RightParen, "Expected ')' after DrawLine arguments.");
              if (!IsAtEnd() && !Check(TokenKind.NewLine) && !Check(TokenKind.EOF))
        {
        AddError(Peek(), "Expected end of line after DrawLine.");
        }
        else if (Match(TokenKind.NewLine))
        {
        // Consumir el newline
        }

            return new DrawLineStatement(dirXExpr, dirYExpr, distanceExpr);
        }

        private DrawCircleStatement ParseDrawCircle()
        {
            Consume(TokenKind.LeftParen, "Expected '(' after DrawCircle.");
            var dirXExpr = ParseExpression();
            RestrictToDirections(dirXExpr, "dirX");
            Consume(TokenKind.Comma, "Expected ',' after dirX.");

            var dirYExpr = ParseExpression();
            RestrictToDirections(dirYExpr, "dirY");
            Consume(TokenKind.Comma, "Expected ',' after dirY.");

            var radiusExpr = ParseExpression();
            Consume(TokenKind.RightParen, "Expected ')' after DrawCircle arguments.");
              if (!IsAtEnd() && !Check(TokenKind.NewLine) && !Check(TokenKind.EOF))
        {
        AddError(Peek(), "Expected end of line after DrawCircle.");
        }
        else if (Match(TokenKind.NewLine))
        {
        // Consumir el newline
        }

            return new DrawCircleStatement(dirXExpr, dirYExpr, radiusExpr);
        }

        private DrawRectangleStatement ParseDrawRectangle()
        {
            Consume(TokenKind.LeftParen, "Expected '(' after DrawRectangle.");
            var dirXExpr = ParseExpression();
            RestrictToDirections(dirXExpr, "dirX");
            Consume(TokenKind.Comma, "Expected ',' after dirX.");

            var dirYExpr = ParseExpression();
            RestrictToDirections(dirYExpr, "dirY");
            Consume(TokenKind.Comma, "Expected ',' after dirY.");

            var distanceExpr = ParseExpression();
            Consume(TokenKind.Comma, "Expected ',' after distance.");

            var widthExpr = ParseExpression();
            Consume(TokenKind.Comma, "Expected ',' after width.");

            var heightExpr = ParseExpression();
            Consume(TokenKind.RightParen, "Expected ')' after DrawRectangle arguments.");
              if (!IsAtEnd() && !Check(TokenKind.NewLine) && !Check(TokenKind.EOF))
        {
        AddError(Peek(), "Expected end of line after DrawRectangle.");
        }
        else if (Match(TokenKind.NewLine))
        {
        // Consumir el newline
        }

            return new DrawRectangleStatement(dirXExpr, dirYExpr, distanceExpr, widthExpr, heightExpr);
        }

        private FillStatement ParseFill()
        {
            Consume(TokenKind.LeftParen, "Expected '(' after Fill.");
            Consume(TokenKind.RightParen, "Expected ')' after Fill.");
              if (!IsAtEnd() && !Check(TokenKind.NewLine) && !Check(TokenKind.EOF))
        {
        AddError(Peek(), "Expected end of line after Fill.");
        }
        else if (Match(TokenKind.NewLine))
        {
        // Consumir el newline
        }
            return new FillStatement();
        }

        private LabelStatement ParseLabel()
        {
             var labelToken = Consume(TokenKind.Identifier, "Expected label identifier.");


           //var labelToken = Previous(); // Guardar el token de la etiqueta
            //Console.WriteLine($"[DEBUG] Entered ParseLabel for '{labelToken.Text}' at line {labelToken.Line}");
           // Consume(TokenKind.NewLine, "Expected end of line after label.");
            var name = labelToken.Text;
    
            if (_labelDefinitions.ContainsKey(name))
            {
            AddError(labelToken, $"Duplicate label '{name}'");
            }
            else
            {
            _labelDefinitions.Add(name, _current);
            }
    
            if (Match(TokenKind.NewLine))
    {
        // OK, etiqueta consumida
    }
    else if (!IsAtEnd())
    {
        // Si no es EOF, error
        AddError(Peek(), "Expected end of line after label.");
    }
            return new LabelStatement(name);
        }

        private ConditionalGotoStatement ParseConditionalGoto()
        {
            Consume(TokenKind.LeftBracket, "Expected '[' after GoTo.");
            var labelToken = Consume(TokenKind.Identifier, "Expected label inside GoTo brackets.");
            Consume(TokenKind.RightBracket, "Expected ']' after label.");
             _labelReferences.Add((labelToken.Text, labelToken));
            Consume(TokenKind.LeftParen, "Expected '(' after GoTo condition.");
            var condition = ParseExpression();
            Consume(TokenKind.RightParen, "Expected ')' after GoTo condition.");
              if (!IsAtEnd() && !Check(TokenKind.NewLine) && !Check(TokenKind.EOF))
        {
        AddError(Peek(), "Expected end of line after GoTo.");
        }
        else if (Match(TokenKind.NewLine))
        {
        // Consumir el newline
        }

            return new ConditionalGotoStatement(labelToken.Text, condition);
        }
        

        private Expression ParseExpression() => ParseOr();

        private Expression ParseOr()
        {
            var expr = ParseAnd();
            while (Match(TokenKind.Or))
            {
                var op = Previous();
                var right = ParseAnd();
                expr = new BinaryExpression(expr, op.Kind, right);
            }
            return expr;
        }

        private Expression ParseAnd()
        {
            var expr = ParseEquality();
            while (Match(TokenKind.And))
            {
                var op = Previous();
                var right = ParseEquality();
                expr = new BinaryExpression(expr, op.Kind, right);
            }
            return expr;
        }

        private Expression ParseEquality()
        {
            var expr = ParseComparison();
            while (Match(TokenKind.EqualEqual, TokenKind.BangEqual))
            {
                var op = Previous();
                var right = ParseComparison();
                expr = new BinaryExpression(expr, op.Kind, right);
            }
            return expr;
        }

        private Expression ParseComparison()
        {
            var expr = ParseTerm();
            while (Match(TokenKind.Greater, TokenKind.Less, TokenKind.GreaterEqual, TokenKind.LessEqual))
            {
                var op = Previous();
                var right = ParseTerm();
                expr = new BinaryExpression(expr, op.Kind, right);
            }
            return expr;
        }

        private Expression ParseTerm()
        {
            var expr = ParseFactor();
            while (Match(TokenKind.Plus, TokenKind.Minus))
            {
                var op = Previous();
                var right = ParseFactor();
                expr = new BinaryExpression(expr, op.Kind, right);
            }
            return expr;
        }

        private Expression ParseFactor()
        {
            var expr = ParseUnary();
            while (Match(TokenKind.Star, TokenKind.Slash, TokenKind.Percent, TokenKind.Power))
            {
                var op = Previous();
                var right = ParseUnary();
                expr = new BinaryExpression(expr, op.Kind, right);
            }
            return expr;
        }

        private Expression ParseUnary()
        {
            if (Match(TokenKind.Minus))
            {
                var op = Previous();
                var right = ParseUnary();
                return new BinaryExpression(new LiteralExpression(0), op.Kind, right);
            }
            return ParseFunctionOrPrimary();
        }

        private Expression ParseFunctionOrPrimary()
        {
            if (Match(TokenKind.Identifier, TokenKind.GetActualX, TokenKind.GetActualY, TokenKind.GetCanvasSize, TokenKind.GetColorCount, TokenKind.IsBrushColor, TokenKind.IsBrushSize, TokenKind.IsCanvasSize))
            {
                var name = Previous().Text;
                if (Match(TokenKind.LeftParen))
                {
                    var args = new List<Expression>();
                    if (!Check(TokenKind.RightParen))
                    {
                        do { args.Add(ParseExpression()); } while (Match(TokenKind.Comma));
                    }
                    Consume(TokenKind.RightParen, "Expected ')' after function arguments.");

                if (name == "GetColorCount" && args.Count != 5)
            {
                AddError(Previous(), $"Function '{name}' requires 5 arguments");
            }
            else if ((name == "IsBrushColor" || name == "IsBrushSize") && args.Count != 1)
            {
                AddError(Previous(), $"Function '{name}' requires 1 argument");
            }
            else if (name == "IsCanvasColor" && args.Count != 3)
            {
                AddError(Previous(), $"Function '{name}' requires 3 arguments");
            }
            else if (args.Count > 0 && (name == "GetActualX" || name == "GetActualY" || name == "GetCanvasSize"))
            {
                AddError(Previous(), $"Function '{name}' does not take arguments");
            }

                    return new FunctionCallExpression(name, args);
                }
                return new VariableExpression(name);
            }
            if (Match(TokenKind.True))
                return new LiteralExpression(true);
            if (Match(TokenKind.False))
                return new LiteralExpression(false);
            if (Match(TokenKind.Number))
                return new LiteralExpression(Previous().Value!);
            if (Match(TokenKind.String))
                return new LiteralExpression(Previous().Value!);
            if (Match(TokenKind.LeftParen))
            {
                var expr = ParseExpression();
                Consume(TokenKind.RightParen, "Expected ')' after expression.");
                return new GroupingExpression(expr);
            }
            throw new Exception($"Unexpected token '{Peek().Text}'.");
        }

        private void RestrictToDirections(Expression expr, string fieldName)
        {
            if (expr is LiteralExpression lit && lit.Value is int v)
            {
                if (!(v == -1 || v == 0 || v == 1))
                    throw new Exception($"{fieldName} must be -1, 0 or 1.");
            }
        }

        // Helpers
        private bool Match(params TokenKind[] types)
        {
            foreach (var t in types)
            {
                if (Check(t))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private Token Consume(TokenKind type, string message)
        {
            if (Check(type)) return Advance();
            throw new Exception(message);
        }

        private Token PeekNext()
        {
            return _tokens.Count > _current + 1
                ? _tokens[_current + 1]
                : _tokens[_current];
        }

        private void AddError(Token token, string message)
        {
            _errors.Add($"[Line {token.Line}] Error at '{token.Text}': {message}");
        }

        private bool Check(TokenKind type) => !IsAtEnd() && Peek().Kind == type;
        private Token Advance() { if (!IsAtEnd()) _current++; return Previous(); }
        private bool IsAtEnd() => Peek().Kind == TokenKind.EOF;
        private Token Peek() => _tokens[_current];
        private Token Previous() => _tokens[_current - 1];

        private void Synchronize()
        {
            Advance();
            while (!IsAtEnd())
            {
                if (Previous().Kind == TokenKind.NewLine)
                    return;
                switch (Peek().Kind)
                {
                    case TokenKind.Spawn:
                    case TokenKind.Color:
                    case TokenKind.Size:
                    case TokenKind.DrawLine:
                    case TokenKind.DrawCircle:
                    case TokenKind.DrawRectangle:
                    case TokenKind.Fill:
                    case TokenKind.GoTo:
                    case TokenKind.Identifier:
                        return;
                }
                Advance();
            }
        }
    }

