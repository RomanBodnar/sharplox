namespace sharplox;

public sealed record Binary(Expr Left, Token Operator, Expr Right) : Expr
{
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitBinary(this);
    }
}
