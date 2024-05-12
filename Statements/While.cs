namespace sharplox;

public sealed record While(Expr Condition, Stmt Body) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitWhile(this);
    }
}
