namespace sharplox;

public class Resolver : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
    private enum FunctionType {
        NONE,
        FUNCTION,
        METHOD,
        INITIALIZER
    }

    private enum ClassType {
        None,
        Class,
        Subclass
    }

    private readonly Interpreter interpreter;
    private readonly Stack<Dictionary<string, bool>> scopes = new();
    private FunctionType currentFunction = FunctionType.NONE;
    private ClassType currentClass = ClassType.None;

    public Resolver(Interpreter interpreter){
        this.interpreter = interpreter;       
    }    

    public object? VisitBlock(BlockStmt stmt){
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public object? VisitClass(Class stmt) {
        ClassType enclosingClass = currentClass;
        currentClass = ClassType.Class;
        Declare(stmt.Name);
        Define(stmt.Name);

        if(stmt.Superclass != null
        && stmt.Name.Lexeme == stmt.Superclass.Name.Lexeme){
            Lox.Error(stmt.Superclass.Name, "A class can't inherit from itself.");
        }

        if(stmt.Superclass != null){
            currentClass = ClassType.Subclass;
            Resolve(stmt.Superclass);
        }

        if(stmt.Superclass is not null) {
            BeginScope();
            scopes.Peek().Add("super", true);
        }

        BeginScope();
        scopes.Peek().Add("this", true);

        foreach(var method in stmt.Methods){
            var declaration = FunctionType.METHOD;
            if (method.Name.Lexeme.Equals("init")) {
                declaration = FunctionType.INITIALIZER;
            }
            ResolveFunction(method, declaration);
        }
        EndScope();
        if(stmt.Superclass is not null){
            EndScope();
        }
        currentClass = enclosingClass;
        return null;
    }

    public object? VisitExpressionStmt(ExpressionStmt stmt){
        Resolve(stmt.Expression);
        return null;
    }
    
    public object? VisitFunction(FunctionStmt stmt){
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    public object? VisitIf(IfStmt stmt){
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if(stmt.ElseBranch is not null){
            Resolve(stmt.ElseBranch);
        }
        return null;
    }

    public object? VisitPrint(Print stmt) {
        Resolve(stmt.Expression);
        return null;
    }

    public object? VisitReturn(Return stmt) {
        if (currentFunction == FunctionType.NONE) {
            Lox.Error(stmt.Keyword, "Can't return from top-level code.");
        }
        
        if(stmt.Value is not null) {
            if (currentFunction == FunctionType.INITIALIZER) {
                Lox.Error(stmt.Keyword,
                    "Can't return a value from an initializer.");
            }
            Resolve(stmt.Value);
        }

        return null;
    }

    public object? VisitVar(Var stmt) {
        Declare(stmt.Name);
        if(stmt.Initializer is not null) {
            Resolve(stmt.Initializer);
        }
        Define(stmt.Name);
        return null;
    }

    public object? VisitWhile(While stmt) {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return null;
    }

    public object? VisitAssignment(Assignment expr){
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitBinary(Binary expr) {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitCall(Call expr){
        Resolve(expr.Callee);
        foreach(var arg in expr.Args) {
            Resolve(arg);
        }
        return null;
    }

    public object? VisitGet(Get expr) {
        Resolve(expr.Obj);
        return null;
    }

    public object? VisitSet(Set expr) {
        Resolve(expr.Obj);
        Resolve(expr.Value);
        return null;
    }

    public object? VisitSuper(Super expr) {
        if(currentClass == ClassType.None){
            Lox.Error(expr.Keyword, "Can't use 'super' outside of a class");
        } else if(currentClass != ClassType.Subclass) {
            Lox.Error(expr.Keyword, "Can't use 'super' in a class with no superclass.");
        }
        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object? VisitThis(This expr) {
        if(currentClass == ClassType.None) {
            Lox.Error(expr.Keyword, "Can't use 'this' outside of a class");
            return null;
        }
        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object? VisitGrouping(Grouping expr) {
        Resolve(expr.Expression);
        return null;
    }

    public object? VisitLiteral(Literal expr) {
        return null;
    }

    public object? VisitLogical(Logical expr) {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitUnary(Unary expr) {
        Resolve(expr.Right);
        return null;
    }

    public object? VisitVariable(Variable expr) {
        if(scopes.Any() 
        && scopes.Peek().TryGetValue(expr.Name.Lexeme, out var val) && val == false ) {
            Lox.Error(expr.Name,  "Can't read local variable in its own initializer.");
        }
        ResolveLocal(expr, expr.Name);

        return null;
    }

    private void BeginScope(){
        scopes.Push(new Dictionary<string,bool>());
    }

    private void EndScope() {
        scopes.Pop();
    }

    private void Declare(Token name) {
        if(!scopes.Any()) return;

        var scope = scopes.Peek();
        if(scope.ContainsKey(name.Lexeme)){
            Lox.Error(name, "Already a variable with this name in this scope.");
        }
        scope[name.Lexeme] = false;
    }

    private void Define(Token name) {
        if(!scopes.Any()) return;
        scopes.Peek()[name.Lexeme] = true;
    }

    private void ResolveLocal(Expr expr, Token name) {
        //This looks, for good reason, a lot like the code in Environment 
        //for evaluating a variable. 
        //We start at the innermost scope and work outwards, 
        //looking in each map for a matching name.
        // If we find the variable, we resolve it, 
        //passing in the number of scopes between the 
        //current innermost scope and the scope where the variable was found. 
        //So, if the variable was found in the current scope, 
        //we pass in 0. If it’s in the immediately enclosing scope, 1.
        // You get the idea.


        for(int i = 0; i < scopes.Count; i++) {
            if(scopes.ElementAt(i).ContainsKey(name.Lexeme)) {
                  interpreter.Resolve(expr, i);
                return;
            }
        }
    }

    public void Resolve(List<Stmt> statements) {
        foreach (Stmt statement in statements){
            Resolve(statement);
        }
    }

    private void ResolveFunction(FunctionStmt function, FunctionType functionType) {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = functionType;

        BeginScope();
        foreach(var param in function.Params){
            Declare(param);
            Define(param);
        }

        Resolve(function.Body);
        EndScope();
        currentFunction = enclosingFunction;
    }

    private void Resolve(Stmt stmt){
        stmt.Accept(this);
    }

    private void Resolve(Expr expr) {
        expr.Accept(this);
    }
}
