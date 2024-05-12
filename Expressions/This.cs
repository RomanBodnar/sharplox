namespace sharplox;

public sealed record This(Token Keyword) : Expr
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitThis(this);
    }
}
