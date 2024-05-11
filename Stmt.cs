namespace sharplox;

public abstract record Stmt
{
    public interface IVisitor<T> {
        T VisitExpressionStmt(ExpressionStmt stmt);
        T VisitPrint(Print stmt);
        T VisitVar(Var stmt);
        T VisitBlock(BlockStmt stmt);
        T VisitIf(IfStmt stmt);
        T VisitWhile(While stmt);
        T VisitFunction(FunctionStmt stmt);
        T VisitReturn(Return stmt);
        T VisitClass(Class stmt);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);
}

public sealed record ExpressionStmt(Expr Expression) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitExpressionStmt(this);
    }
}

public sealed record Print(Expr Expression) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitPrint(this);
    }
}

public sealed record Var(Token Name, Expr? Initializer) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitVar(this);
    }
}

public sealed record BlockStmt(List<Stmt> Statements) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitBlock(this);
    }
}

public sealed record IfStmt(Expr Condition, Stmt ThenBranch, Stmt? ElseBranch) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitIf(this);
    }
}

public sealed record While(Expr Condition, Stmt Body) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitWhile(this);
    }
}
public sealed record FunctionStmt(Token Name, List<Token> Params, List<Stmt> Body) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor){
        return visitor.VisitFunction(this);
    }
}

public sealed record Return(Token Keyword, Expr? Value) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitReturn(this);
    }
}

public sealed record Class(Token Name, Variable? Superclass, List<FunctionStmt> Methods) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitClass(this);
    }
}