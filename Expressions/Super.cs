namespace sharplox;

public sealed record Super(Token Keyword, Token Method) : Expr {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitSuper(this);
    }
}