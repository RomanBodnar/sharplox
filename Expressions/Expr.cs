namespace sharplox;

public abstract record Expr
{ 
    public interface IVisitor<T>
    {
        T VisitBinary(Binary expression);
        T VisitGrouping(Grouping expression);
        T VisitLiteral(Literal expression);
        T VisitUnary(Unary expression);
        T VisitVariable(Variable expression);
        T VisitAssignment(Assignment expression);
        T VisitLogical(Logical expression);
        T VisitCall(Call expression);
        T VisitGet(Get expression);
        T VisitSet(Set expression);
        T VisitThis(This expression); 
        T VisitSuper(Super expression);
    } 
    public abstract T Accept<T>(IVisitor<T> visitor); 
}
