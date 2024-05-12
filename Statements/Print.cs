namespace sharplox;

public sealed record Print(Expr Expression) : Stmt {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitPrint(this);
    }
}
