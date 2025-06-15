
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Introduce el código fuente (.pw). Termina con línea vacía:");
            var lines = new List<string>();
            string line;
            while ((line = Console.ReadLine()) != null && line != string.Empty)
                lines.Add(line);

            var source = string.Join("\n", lines);

            // 1. Lexical analysis
            var lexer = new Lexer(source);
            var tokens = lexer.Tokenize();
            Console.WriteLine("\nTokens:");
            foreach (var t in tokens)
                Console.WriteLine(t);
            if (lexer.HasErrors)
            {
                Console.WriteLine("\nErrores léxicos:");
                foreach (var err in lexer.Errors)
                    Console.WriteLine(err);
                return;
            }

            // 2. Parsing
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            Console.WriteLine($"\nParsed {ast.Statements.Count} statements:");
            foreach (var stmt in ast.Statements)
                Console.WriteLine($"  - {stmt.GetType().Name}");
            if (parser.HasErrors)
            {
                Console.WriteLine("\nErrores de sintaxis:");
                foreach (var err in parser.Errors)
                    Console.WriteLine(err);
                return;
            }

            // 3. Semantic analysis
            var semantic = new SemanticAnalyzer(ast);
            semantic.Analyze();
            if (semantic.Errors.Count > 0)
            {
                Console.WriteLine("Errores semánticos:");
                foreach (var err in semantic.Errors)
                    Console.WriteLine(err);
                return;
            }

            // 4. Interpretation
            Console.WriteLine("Ejecutando programa...");
            int canvasSize = 20; // ejemplo, puedes pedir al usuario
            var interpreter = new Interpreter(ast, canvasSize);
            try
            {
                interpreter.Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en tiempo de ejecución: {ex.Message}");
            }

            // 5. Mostrar canvas
            Console.WriteLine("Canvas resultante:");
            PrintCanvas(interpreter);
        }

        static void PrintCanvas(Interpreter interpreter)
        {
            var canvasField = typeof(Interpreter).GetField("_canvas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sizeField = typeof(Interpreter).GetField("_canvasSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int[,] canvas = (int[,])canvasField.GetValue(interpreter)!;
            int size = (int)sizeField.GetValue(interpreter)!;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Console.Write(canvas[x, y] == 0 ? "." : "#");
                }
                Console.WriteLine();
            }
        }
    }
    
    



