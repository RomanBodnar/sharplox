namespace sharplox;

public sealed record Assignment(Token Name, Expr Value) : Expr {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitAssignment(this);
    }
}
