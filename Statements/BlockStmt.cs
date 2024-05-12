namespace sharplox;

public sealed record BlockStmt(List<Stmt> Statements) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitBlock(this);
    }
}
