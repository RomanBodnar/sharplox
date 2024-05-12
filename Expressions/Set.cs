namespace sharplox;

public sealed record Set(Expr Obj, Token Name, Expr Value) : Expr {
   public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitSet(this); 
    } 
}
