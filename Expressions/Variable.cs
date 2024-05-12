namespace sharplox;

public sealed record Variable(Token Name) : Expr {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitVariable(this);
    }
}
