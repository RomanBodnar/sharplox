
global using Newtonsoft.Json;
namespace sharplox;
// --------------------------------------
// VERSION 1 (no explicit precedence or associativity)
// expression     → literal     
//                | unary
//                | binary
//                | grouping ;
// literal        → NUMBER | STRING | "true" | "false" | "nil" ;
// grouping       → "(" expression ")" ;
// unary          → ( "-" | "!" ) expression ;
// binary         → expression operator expression ;
// operator       → "==" | "!=" | "<" | "<=" | ">" | ">="
//                | "+"  | "-"  | "*" | "/" ;
public class Lox
{
    private static readonly Interpreter interpreter = new Interpreter();
    public static bool HadError { get; private set; }
    public static bool HadRuntimeError {get; private set; }  

    static void Main(string[] args)
    {
        if(args.Length > 1) {
            Console.WriteLine("Usage: sharplox [script]");
            System.Environment.Exit(64);
        }
        else if (args.Length == 1) {
            RunFile(args[0]);
        }
        else {
            RunPropmt();
        }     
    }
    private static void RunFile(string v)
    {
        string source= File.ReadAllText(v);
        Run(source);

        // Indicate an error in the exit code
        if(HadError) {
            System.Environment.Exit(65);
        }
        if(HadRuntimeError) {
            System.Environment.Exit(70);
        }
    }

    private static void RunPropmt() {
        while(true) {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if(line is null) {
                break;
            }
            Run(line);
            HadError = false;
        }
    }

    private static void Run(string source) {
        var scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();
        Parser parser = new Parser(tokens);
        var statements = parser.Parse();

        // Stop if there was a syntax error.
        if (HadError) return;

        Resolver resolver = new Resolver(interpreter);
        resolver.Resolve(statements);
        
        if (HadError) return;
        interpreter.Interpret(statements);
        return;

    }

    public static void Error(int line, string message) {
        Report(line, "", message);
    }

    public static void Error(Token token, string message) {
        if(token.Type == TokenType.EOF) {
            Report(token.Line, " at end", message);
        }
        else {
            Report(token.Line, $" at '{token.Lexeme}'" , message);
        }
    }

    public static void RuntimeError(RuntimeErrorException exception){
        Console.Error.WriteLine(exception.Message + $"\n[line {exception.token.Line}]");
        HadRuntimeError = true;
    }

    public static void Report(int line, string where, string message) {
        Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
        HadError = true;
    }
}
