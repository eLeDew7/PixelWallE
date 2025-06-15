class Lexer
{
    private readonly string _source;
    private readonly List<Token> _tokens = new();
    private readonly List<string> _errors = new();
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;
    public bool HasErrors => _errors.Count > 0;
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

        public Lexer(string source)
        {
            _source = source;
        }
        public List<Token> Tokenize()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                try
                {
                    ScanToken();
                }
                catch (Exception ex)
                {
                    AddError(ex.Message);
                }
            }

            _tokens.Add(new Token(TokenKind.EOF, string.Empty, null, _line));
            return _tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '<':
                    if (Match('-')) AddToken(TokenKind.Assign);
                    else if(Match('=')) AddToken(TokenKind.LessEqual);
                    else AddToken(TokenKind.Less);
                    break;
                case '+': AddToken(TokenKind.Plus); break;
                case '-': AddToken(TokenKind.Minus); break;
                case '*':
                    if (Match('*')) AddToken(TokenKind.Power);
                    else AddToken(TokenKind.Star);
                    break;
                case '%': AddToken(TokenKind.Percent); break;
                case '/': AddToken(TokenKind.Slash); break;
                case '(' : AddToken(TokenKind.LeftParen); break;
                case ')' : AddToken(TokenKind.RightParen); break;
                case '[' : AddToken(TokenKind.LeftBracket); break;
                case ']' : AddToken(TokenKind.RightBracket); break;
                case ',' : AddToken(TokenKind.Comma); break;
                case '=':
                if (Match('=')) AddToken(TokenKind.EqualEqual); // ==
                else AddToken(TokenKind.Equal);
                break;
                case '!':
                if (Match('=')) AddToken(TokenKind.BangEqual); // !=
                else AddToken(TokenKind.Bang);
                break;
                case '&':
                if (Match('&')) AddToken(TokenKind.And); // &&
                break;
                case '|':
                if (Match('|')) AddToken(TokenKind.Or); // ||
                break;
                case '>':
                if (Match('=')) AddToken(TokenKind.GreaterEqual);
                else AddToken(TokenKind.Greater);
                break;
                case '\r': break;
                case '\n': _line++; AddToken(TokenKind.NewLine); break;
                case ' ':
                case '\t': break;
                case '"': ReadString(); break;
                default:
                    if (IsDigit(c)) ReadNumber();
                    else if (IsAlpha(c)) ReadIdentifierOrKeyword();
                    else AddError($"Unexpected character '{c}'.");
                    break;
            }
        }

        private void ReadString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsAtEnd())
            {
                AddError("Unterminated string.");
                return;
            }

            // Consume closing quote
            Advance();
            string value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenKind.String, value);
        }

        private void ReadNumber()
        {
            while (IsDigit(Peek())) Advance();
            string text = _source.Substring(_start, _current - _start);
            if (int.TryParse(text, out var number))
            {
                AddToken(TokenKind.Number, number);
            }
            else
            {
                AddError($"Invalid number '{text}'.");
            }
        }

        private void ReadIdentifierOrKeyword()
        {
            while (IsAlphaNumeric(Peek()) || Peek() == '_') Advance();
            string text = _source.Substring(_start, _current - _start);
            TokenKind type = text switch
            {
                "Spawn" => TokenKind.Spawn,
                "Color" => TokenKind.Color,
                "Size"  => TokenKind.Size,
                "DrawLine" => TokenKind.DrawLine,
                "DrawCircle" => TokenKind.DrawCircle,
                "DrawRectangle" => TokenKind.DrawRectangle,
                "Fill" => TokenKind.Fill,
                "GoTo" => TokenKind.GoTo,
                "GetActualX" => TokenKind.GetActualX,
                "GetActualY" => TokenKind.GetActualY,
                "GetColorCount" => TokenKind.GetColorCount,
                "IsBrushSize" => TokenKind.IsBrushSize,
                "GetCanvasSize" => TokenKind.GetCanvasSize,
                "IsBrushColor" => TokenKind.IsBrushColor,
                "IsCanvasSize" => TokenKind.IsCanvasSize,
                "true" => TokenKind.True,
                "false" => TokenKind.False,
                _ => TokenKind.Identifier

            };
            AddToken(type, type == TokenKind.Identifier ? text : null);
        }

        private bool Match(char expected)
        {
            if (IsAtEnd() || _source[_current] != expected) return false;
            _current++;
            return true;
        }

        private char Peek() => IsAtEnd() ? '\0' : _source[_current];
        private char Advance() => _source[_current++];
        private bool IsAtEnd() => _current >= _source.Length;
        private bool IsDigit(char c) => c >= '0' && c <= '9';
        private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

        private void AddToken(TokenKind type, object? literal = null)
        {
            string lexeme = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, lexeme, literal, _line));
        }

        private void AddError(string message)
        {
            _errors.Add($"Line {_line}: {message}");
        }
    }