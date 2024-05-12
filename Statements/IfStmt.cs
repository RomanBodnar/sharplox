namespace sharplox;

public sealed record IfStmt(Expr Condition, Stmt ThenBranch, Stmt? ElseBranch) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitIf(this);
    }
}
