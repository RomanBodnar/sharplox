namespace sharplox;

public sealed record Class(Token Name, Variable? Superclass, List<FunctionStmt> Methods) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitClass(this);
    }
}