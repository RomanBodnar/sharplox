namespace sharplox;

public sealed record FunctionStmt(Token Name, List<Token> Params, List<Stmt> Body) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor){
        return visitor.VisitFunction(this);
    }
}
