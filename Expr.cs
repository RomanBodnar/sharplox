using System.Linq.Expressions;

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

public sealed record Binary(Expr Left, Token Operator, Expr Right) : Expr
{
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitBinary(this);
    }
}

public sealed record Grouping(Expr Expression) : Expr
{
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitGrouping(this);
    }
}
public sealed record Literal(object? Value) : Expr {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitLiteral(this);
    }
}

public sealed record Logical(Expr Left, Token Operator, Expr Right) : Expr {
    public override T Accept<T> (IVisitor<T> visitor) {
        return visitor.VisitLogical(this);
    }
}

public sealed record Unary(Token Operator, Expr Right) : Expr {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitUnary(this);
    }
}

public sealed record Variable(Token Name) : Expr {
    public override T Accept<T>(IVisitor<T> visitor) {
        return visitor.VisitVariable(this);
    }
}

public sealed record Assignment(Token Name, Expr Value) : Expr {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitAssignment(this);
    }
}

public sealed record Call(Expr Callee, Token Paren, List<Expr> Args) : Expr {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitCall(this);
    }
}

public sealed record Get(Expr Obj, Token Name) : Expr {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitGet(this); 
    }
}

public sealed record Set(Expr Obj, Token Name, Expr Value) : Expr {
   public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitSet(this); 
    } 
}

public sealed record This(Token Keyword) : Expr
{
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitThis(this);
    }
}

public sealed record Super(Token Keyword, Token Method) : Expr {
    public override T Accept<T>(IVisitor<T> visitor)
    {
        return visitor.VisitSuper(this);
    }
}