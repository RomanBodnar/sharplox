namespace sharplox;

public sealed record Unary(Token Operator, Expr Right) : Expr {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitUnary(this);
    }
}
