namespace sharplox;

public sealed record Literal(object? Value) : Expr {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitLiteral(this);
    }
}
