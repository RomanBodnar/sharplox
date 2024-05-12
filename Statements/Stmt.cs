namespace sharplox;

public abstract record Stmt
{
    public interface IVisitor<T> {
        T VisitExpressionStmt(ExpressionStmt stmt);
        T VisitPrint(Print stmt);
        T VisitVar(Var stmt);
        T VisitBlock(BlockStmt stmt);
        T VisitIf(IfStmt stmt);
        T VisitWhile(While stmt);
        T VisitFunction(FunctionStmt stmt);
        T VisitReturn(Return stmt);
        T VisitClass(Class stmt);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);
}
