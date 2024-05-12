namespace sharplox;

public sealed record Call(Expr Callee, Token Paren, List<Expr> Args) : Expr {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitCall(this);
    }
}
