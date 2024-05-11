using static sharplox.TokenType;

namespace sharplox;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object?>
{
    public readonly Environment Globals = new();
    private readonly Dictionary<Expr, int> locals = [];
    private Environment environment;
    
    public Interpreter() {
        environment = Globals;
        Globals.Define("clock", new ClockFunction());
    }

    public void Interpret(IEnumerable<Stmt> statements) {
        try {
            foreach (var stmt in statements) {
                Execute(stmt);
            }
        } 
        catch(RuntimeErrorException ex) {
            Lox.RuntimeError(ex);
        }
    }

#region Statement visitors
    public object? VisitExpressionStmt(ExpressionStmt stmt) {
        Evaluate(stmt.Expression);
        return null;
    }
    public object? VisitPrint(Print stmt) {
       object value = Evaluate(stmt.Expression);
       Console.WriteLine(Stringify(value));
       return null; 
    }

    public object? VisitVar(Var stmt) {
        object? value = null;
        if(stmt.Initializer != null) {
            value = Evaluate(stmt.Initializer);
        }

        environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object? VisitBlock(BlockStmt stmt) {
        ExecuteBlock(stmt.Statements, new Environment(environment));
        return null;
    }

    public object? VisitIf(IfStmt stmt) {
        if(IsTruthy(Evaluate(stmt.Condition))) {
            Execute(stmt.ThenBranch);
        }
        else if(stmt.ElseBranch is not null) {
            Execute(stmt.ElseBranch);
        }
        return null;
    }

    public object? VisitWhile(While stmt) {
        while(IsTruthy(Evaluate(stmt.Condition))){
            Execute(stmt.Body);
        }
        return null;
    }

    public object? VisitFunction(FunctionStmt stmt) {
        var function = new LoxFunction(stmt, environment);
        environment.Define(stmt.Name.Lexeme, function);
        return null;
    }
    
    public object? VisitReturn(Return stmt){
        object? value = null;
        if(stmt.Value is not null) {
            value = Evaluate(stmt.Value);
        }

        throw new ReturnException(value);
    }

    public object? VisitClass(Class stmt) {
        object? superclass = null;
        if(stmt.Superclass is not null) {
            superclass = Evaluate(stmt.Superclass);
            if(superclass is not LoxClass){
                throw new RuntimeErrorException(stmt.Superclass.Name, "Superclass must be a class");
            }
        }
        this.environment.Define(stmt.Name.Lexeme, null);

        if(stmt.Superclass is not null) {
            this.environment = new Environment(this.environment);
            this.environment.Define("super", superclass);
        }

        var methods = new Dictionary<string, LoxFunction>();
        foreach(var method in stmt.Methods) {
            var function = new LoxFunction(method, environment, method.Name.Lexeme.Equals("init"));
            methods.Add(method.Name.Lexeme, function);
        }

        var @class = new LoxClass(stmt.Name.Lexeme, (LoxClass?)superclass, methods);
        if(superclass is not null){
            this.environment = this.environment.Enclosing;
        }
        this.environment.Assign(stmt.Name, @class);
        return null;
    }
#endregion

#region Expression visitors
    #nullable disable
    public object VisitVariable(Variable expr){
        return LookUpVariable(expr.Name, expr);
    }

    public object LookUpVariable(Token name, Expr expr) {
        bool isPresent = locals.TryGetValue(expr, out int distance);
        if(isPresent) {
            return environment.GetAt(distance, name.Lexeme);
        }
        else {
            return Globals.Get(name);
        }
    }

    public object VisitAssignment(Assignment expr) {
        object value = Evaluate(expr.Value);

        bool hasDistance = locals.TryGetValue(expr, out int distance);
        if(hasDistance) {
            environment.AssignAt(distance, expr.Name, value);
        } 
        else {
            Globals.Assign(expr.Name, value);
        }
        return value;
    }

    public object VisitLiteral(Literal expr) {
        return expr.Value;
    }

    public object VisitGrouping(Grouping expr) {
        return Evaluate(expr.Expression);
    }

    #nullable disable
    public object VisitUnary(Unary expr) {
        object right = Evaluate(expr.Right);
        switch (expr.Operator.Type) {
            case MINUS:
                CheckNumberOperand(expr.Operator, right);
                return -(double)right;
            case BANG:
                return !IsTruthy(right);
        }

        // Unreachable
        return null;
    }

    public object VisitBinary(Binary expr) {
        object left = Evaluate(expr.Left);
        object right = Evaluate(expr.Right);
        switch(expr.Operator.Type) {
            case MINUS:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left - (double)right;
            case SLASH:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left / (double)right;
            case STAR:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left * (double)right;
            case PLUS:
                if(left is double && right is double) {
                   return (double)left + (double)right;
                }
                if(left is string && right is string) {
                    return (string)left + (string)right;
                }
                throw new RuntimeErrorException(expr.Operator, "Operands must be two numbers or two strings.");
            case GREATER:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left > (double)right;
            case GREATER_EQUAL:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left >= (double)right;
            case LESS:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left < (double)right;
            case LESS_EQUAL:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left <= (double)right;
            case BANG_EQUAL: 
                return !IsEqual(left, right);
            case EQUAL_EQUAL:
                return IsEqual(left, right);
            
        }

        return null;
    }

    public object VisitLogical(Logical expression) {
        object left = Evaluate(expression.Left);
        if(expression.Operator.Type == OR) {
            if(IsTruthy(left)){
                return left;
            }
        }
        else {
            if(!IsTruthy(left)){
                return left;
            }
        }

        return Evaluate(expression.Right);
    }

    public object VisitCall(Call expr) {
        object callee = Evaluate(expr.Callee);
        List<object> arguments = [];
        foreach(var argument in expr.Args){
            arguments.Add(Evaluate(argument));
        }
        if(callee is not ILoxCallable) {
            throw new RuntimeErrorException(expr.Paren, "Can only call functions and classes.");
        }
        
        ILoxCallable function = (ILoxCallable)callee;
        if(arguments.Count != function.Arity) {
            throw new RuntimeErrorException(expr.Paren, $"Expected ${function.Arity} arguments but got ${arguments.Count}.");
        }
        return function.Call(this, arguments);
    }

    public object VisitGet(Get expr) {
        var @object = Evaluate(expr.Obj);
        if(@object is LoxInstance) {
            return ((LoxInstance)@object).Get(expr.Name);
        }

        throw new RuntimeErrorException(expr.Name, "Only instances have properties.");
    }

    public object VisitSet(Set expr) {
        object obj = Evaluate(expr.Obj);
        if(obj is not LoxInstance) {
            throw new RuntimeErrorException(expr.Name, "Only instances have fields.");
        }
        
        var value = Evaluate(expr.Value);
        (obj as LoxInstance).Set(expr.Name, value);
        return value;
    }

    public object VisitSuper(Super expr) {
        int distance = locals[expr];
        var @superclass = (LoxClass)environment.GetAt(distance, "super");
        LoxInstance @object = (LoxInstance)environment.GetAt(distance - 1, "this");
        LoxFunction method = superclass.FindMethod(expr.Method.Lexeme);
        if (method == null) {
            throw new RuntimeErrorException(expr.Method, "Undefined property '" + expr.Method.Lexeme + "'.");
        }
        return method.Bind(@object);
    }

    public object VisitThis(This expr) {
        return LookUpVariable(expr.Keyword, expr);
    }
#endregion

    private void Execute(Stmt statement) {
        statement.Accept(this);
    }

    internal void Resolve(Expr expr, int depth){
        locals.Add(expr, depth);
    }

    internal void ExecuteBlock(List<Stmt> statements, Environment environment) {
        var previous = this.environment;
        try {
            this.environment = environment;
            foreach (var statement in statements) {
                Execute(statement);
            }
        }
        finally {
            this.environment = previous;
        }
    }

    private object Evaluate(Expr expr){
        return expr.Accept(this);
    }

    private void CheckNumberOperand(Token @operator, object operand){
        if(operand is double) {
            return;
        }
        throw new RuntimeErrorException(@operator, "Operand must be a number");
    }

    private void CheckNumberOperand(Token @operator, object left, object right){
        if(left is double && right is double) {
            return;
        }
        throw new RuntimeErrorException(@operator, "Operands must be numbers.");
    }

    private bool IsTruthy(object @object){
        if(@object is null) {
            return false;
        }
        if(@object is bool) {
            return (bool)@object;
        }
        return true;
    }

    private bool IsEqual(object left, object right) {
        if(left is null && right is null) {
            return true;
        }
        if(left is null) {
            return false;
        }

        return left.Equals(right);
    }

    private string Stringify(object obj) {
    if (obj == null) return "nil";

    if (obj is double) {
      string text = obj.ToString();
      if (text.EndsWith(".0")) {
        text = text.Substring(0, text.Length - 2);
      }
      return text;
    }

    return obj.ToString();
  }
}
