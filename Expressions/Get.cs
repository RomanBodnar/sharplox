namespace sharplox;

public sealed record Get(Expr Obj, Token Name) : Expr {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitGet(this); 
    }
}
