namespace sharplox;

public sealed record Grouping(Expr Expression) : Expr
{
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitGrouping(this);
    }
}
