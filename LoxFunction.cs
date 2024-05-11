namespace sharplox;
#nullable disable
public class LoxFunction : ILoxCallable
{
    private readonly FunctionStmt declaration;
    private readonly Environment closure;
    private readonly bool isInitializer;

    public LoxFunction(FunctionStmt declaration, Environment closure, bool isInitializer = false) {
        this.isInitializer = isInitializer;
        this.declaration = declaration;
        this.closure = closure;
    }

    public int Arity => declaration.Params.Count;

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var environment = new Environment(closure);
        for(int i = 0; i < declaration.Params.Count; i++) {
            environment.Define(declaration.Params[i].Lexeme, arguments[i]);
        }
        try {
            interpreter.ExecuteBlock(declaration.Body, environment);
        }
        catch(ReturnException returnValue) {
            if (isInitializer) return closure.GetAt(0, "this");
            return returnValue.Value;
        }
        if(isInitializer) {
            return closure.GetAt(0, "this");
        }
        return null;
    }

    public LoxFunction Bind(LoxInstance instance) {
        Environment environment = new Environment(closure);
        environment.Define("this", instance);
        return new LoxFunction(declaration, environment, isInitializer);
    }

    public override string ToString()
    {
        return $"<fn {declaration.Name.Lexeme}>";
    }
}
