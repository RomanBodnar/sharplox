namespace sharplox;

public sealed record Var(Token Name, Expr? Initializer) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitVar(this);
    }
}
