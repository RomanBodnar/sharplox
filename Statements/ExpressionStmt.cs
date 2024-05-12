namespace sharplox;

public sealed record ExpressionStmt(Expr Expression) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitExpressionStmt(this);
    }
}
