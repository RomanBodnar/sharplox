namespace sharplox;

public sealed record Logical(Expr Left, Token Operator, Expr Right) : Expr {
    public override T Accept<T> (IVisitor<T> visitor) {
        return visitor.VisitLogical(this);
    }
}
