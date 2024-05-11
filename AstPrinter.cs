using System.Text;

namespace sharplox;

public class AstPrinter : Expr.IVisitor<string> {
    public string VisitBinary(Binary expression)
    {
        return Parenthesize
        (
            expression.Operator.Lexeme, 
            expression.Left, 
            expression.Right
        );
    }

    public string VisitGrouping(Grouping expression)
    {
        return Parenthesize
        (
            "group",
            expression.Expression
        );
    }

    public string VisitLiteral(Literal expression)
    {
        if(expression.Value is null) {
            return "nil";
        }
        return expression.Value.ToString()!;
    }

    public string VisitUnary(Unary expression)
    {
        return Parenthesize
        (
            expression.Operator.Lexeme,
            expression.Right
        );
    }

    public string Print(Expr expr) {
        return expr.Accept(this);
    }

    private string Parenthesize(string name, params Expr[] exprs) {
        var builder = new StringBuilder();
        builder.Append("(").Append(name);
        foreach(var expr in exprs) {
            builder.Append(" ").Append(expr.Accept(this));
        }
        builder.Append(")");

        return builder.ToString();
    }

    public string VisitVariable(Variable expression)
    {
        throw new NotImplementedException();
    }

    public string VisitAssignment(Assignment expression)
    {
        throw new NotImplementedException();
    }

    public string VisitLogical(Logical expression)
    {
        throw new NotImplementedException();
    }

    public string VisitCall(Call expression)
    {
        throw new NotImplementedException();
    }

    public string VisitFunction(FunctionStmt expression)
    {
        throw new NotImplementedException();
    }

    public string VisitGet(Get expression)
    {
        throw new NotImplementedException();
    }

    public string VisitSet(Set expression)
    {
        throw new NotImplementedException();
    }

    public string VisitThis(This expression)
    {
        throw new NotImplementedException();
    }

    public string VisitSuper(Super expression)
    {
        throw new NotImplementedException();
    }
}
