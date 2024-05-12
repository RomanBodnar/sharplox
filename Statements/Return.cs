namespace sharplox;

public sealed record Return(Token Keyword, Expr? Value) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitReturn(this);
    }
}
