using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static sharplox.TokenType;

namespace sharplox;
// program        → declaration* EOF
// declaration    → classDecl
//                | funDecl
//                | varDecl
//                | statement ;

// statement      → exprStmt
//                | forStmt
//                | ifStmt
//                | printStmt 
//                | returnStmt
//                | whileStmt
//                | block;

// classDecl      → "class" IDENTIFIER ( "<" IDENTIFIER )?
//                  "{" function* "}" ;
// varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;
// funDecl        → "fun" function ;
// function       → IDENTIFIER "(" parameters? ")" block ;
// parameters     → IDENTIFIER ( "," IDENTIFIER )* ;

// exprStmt       → expression ";" ;
// forStmt        → "for" "(" ( varDecl | exprStmt | ";" )
//                  expression? ";"
//                  expression? ")" statement ;
// ifStmt         → "if" "(" expression ")" statement
//                  ( "else" statement )? ;
// printStmt      → "print" expression ";" ;
// returnStmt     → "return" expression? ";" ;   
// whileStmt      → "while" "(" expression ")" statement ;
// block          → "{" declaration* "}" ;

// expression     → assignment ;
// assignment     → ( call "." )? IDENTIFIER "=" assignment
//                | logic_or ;

// logic_or       → logic_and ( "or" logic_and )* ;
// logic_and      → equality ( "and" equality )* ;

// equality       → comparison ( ( "!=" | "==" ) comparison )* ;
// comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
// term           → factor ( ( "-" | "+" ) factor )* ;
// factor         → unary ( ( "/" | "*" )  unary )* ;        // Putting the recursive production on the left side and unary on the right makes the rule left-associative and unambiguous.
// unary          → ( "!" | "-" ) unary | call ;
// call           → primary ( "(" arguments? ")" | "." IDENTIFIER )* ;
// arguments      → expression ( "," expression )* ;
// primary        → "true" | "false" | "nil" | "this"
//                | NUMBER | STRING | IDENTIFIER | "(" expression ")"
//                | "super" "." IDENTIFIER ;


public class Parser {
    private class ParseErrorException : Exception {}

    private readonly List<Token> tokens;
    private int current = 0;

    public Parser(List<Token> tokens){ 
        this.tokens = tokens;
    }

    public List<Stmt> Parse() {
        var statements = new List<Stmt>();
        while(!IsAtEnd){
            statements.Add(Declaration());
        }
        return statements;
    }

    private Stmt? Declaration(){
        try {
            if(Match(CLASS)) return ClassDeclaration();
            if(Match(FUN)) return FunctionDeclaration("function");
            if(Match(VAR)) {
                return VarDeclaration();
            }
            return Statement();
        } 
        catch(ParseErrorException) {
            Synchronize();
            return null;
        }
    }

    private Stmt ClassDeclaration() {
        var name = Consume(IDENTIFIER, "Expect class name");
        Variable? superclass = null;
        if(Match(LESS)){
            Consume(IDENTIFIER, "Expect superclass name.");
            superclass = new Variable(Previous);
        }
        Consume(LEFT_BRACE, "Expect '{' before class body.");
        var methods = new List<FunctionStmt>();
        while(!Check(RIGHT_BRACE) && !IsAtEnd){
            methods.Add((FunctionStmt)FunctionDeclaration("method"));
        }
        Consume(RIGHT_BRACE, "Expect '}' after class body.");
        return new Class(name, superclass, methods);
    }

    private Stmt FunctionDeclaration(string kind) {
        var name = Consume(IDENTIFIER, $"Expect {kind} name.");
        Consume(LEFT_PAREN,"Expect '(' after " + kind + " name.");
        List<Token> parameters = [];
        if(!Check(RIGHT_PAREN)){
            do {
                if (parameters.Count >= 255) {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
            } while (Match(COMMA));
        }
        Consume(RIGHT_PAREN, "Expect ')' after parameters.");
        Consume(LEFT_BRACE, "Expect '{' before " + kind + " body.");
        var body = Block();
        return new FunctionStmt(name, parameters, body);
    }

    private Stmt VarDeclaration() {
        Token name = Consume(IDENTIFIER, "Expect variable name.");

        Expr? initializer = default;
        if(Match(EQUAL)) {
            initializer = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after varible declaration");
        return new Var(name, initializer);
    }

    private Stmt Statement() {
        if(Match(FOR)) return ForStatement();
        if(Match(IF)) return IfStatement();
        if(Match(WHILE)) return WhileStatement();
        if(Match(PRINT)) return PrintStatement();
        if(Match(RETURN)) return ReturnStatement();
        if(Match(LEFT_BRACE)) return new BlockStmt(Block());
        return ExpressionStatement();
    }
    
    private Stmt ForStatement() {
        Consume(LEFT_PAREN, "Expect '(' after 'for'.");
        Stmt? initializer;
        if(Match(SEMICOLON)) {
            initializer = null;
        }
        else if (Match(VAR)) {
            initializer = VarDeclaration();
        }
        else {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if(!Check(SEMICOLON)){
            condition = Expression();
        }
        Consume(SEMICOLON, "Expect ';' after loop condition.");

        Expr? increment = null;
        if(!Check(RIGHT_PAREN)){
            increment = Expression();
        }
        Consume(RIGHT_PAREN, "Expect ')' after for clauses.");
        Stmt body = Statement();
        if(increment != null) {
            body = new BlockStmt(new List<Stmt> {body, new ExpressionStmt(increment)});
        }
        if(condition == null) {
            condition = new Literal(true);
        }
        body = new While(condition, body);

        if(initializer != null) {
            body = new BlockStmt(new List<Stmt> {initializer, body});
        }

        return body;
    }

    private Stmt IfStatement() {
        Consume(LEFT_PAREN, "Expect '(' after 'if'.");
        Expr condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after if condition");
    
        Stmt thenBranch = Statement();
        Stmt? elseBranch = null;
        if(Match(ELSE)) {
            elseBranch = Statement();
        }

        return new IfStmt(condition, thenBranch, elseBranch);
    }

    public Stmt WhileStatement() {
        Consume(LEFT_PAREN, "Expect '(' after 'while'.");
        Expr condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after condition.");
        Stmt body = Statement();
        return new While(condition, body);
    }

    private Stmt PrintStatement() {
        Expr value = Expression();
        Consume(SEMICOLON, "Expect ';' after value");
        return new Print(value);
    }

    private Stmt ReturnStatement() {
        Token keyword = Previous;
        Expr value = null;
        if(!Check(SEMICOLON)){
            value = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after return value.");
        return new Return(keyword, value);
    }

    private List<Stmt> Block() {
        var statements = new List<Stmt>();
        while(!Check(RIGHT_BRACE) && !IsAtEnd) {
            statements.Add(Declaration());
        }
        Consume(RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Stmt ExpressionStatement() {
        Expr expr = Expression();
        Consume(SEMICOLON, "Expect ';' after expression");
        return new ExpressionStmt(expr);
    }

    private Expr Expression() {
        return Assignment();
    }

    private Expr Assignment() {
        var expr = Or();
        if(Match(EQUAL)){
            Token equals = Previous;
            Expr value = Assignment();

            if(expr is Variable) {
                Token name = (expr as Variable)!.Name;
                return new Assignment(name, value);
            } else if(expr is Get) {
                var get = (Get)expr;
                return new Set(get.Obj, get.Name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or() {
        Expr expr = And();
        while(Match(OR)) {
            Token @operator = Previous;
            Expr right = And();
            expr = new Logical(expr, @operator, right);
        }

        return expr;
    }

    private Expr And() {
        Expr expr = Equality();
        while(Match(AND)) {
            Token @operator = Previous;
            Expr right = Equality();
            expr = new Logical(expr, @operator, right);
        }

        return expr;
    }

    /// <summary>
    /// equality → comparison ( ( "!=" | "==" ) comparison )* ;
    /// </summary>
    /// <returns></returns>
    private Expr Equality() {
        Expr expr = Comparison();

        while(Match(BANG_EQUAL, EQUAL_EQUAL)){
            Token @operator = Previous;
            Expr right = Comparison();
            expr = new Binary(expr, @operator, right);
        }

        return expr;
    }

    /// <summary>
    /// comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
    /// </summary>
    /// <returns>Expression</returns>
    private Expr Comparison() {
        Expr expr = Term();

        while(Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL)) {
            Token @operator = Previous;
            Expr right = Term();
            expr = new Binary(expr, @operator, right);
        }

        return expr;
    }

    /// <summary>
    /// term → factor ( ( "-" | "+" ) factor )* ;
    /// </summary>
    /// <returns></returns>
    private Expr Term() {
        Expr expr = Factor();
        while(Match(MINUS, PLUS)) {
            Token @operator = Previous;
            Expr right = Factor();
            expr = new Binary(expr, @operator, right);
        }

        return expr;
    }

    /// <summary>
    ///  factor → unary ( ( "/" | "*" )  unary )* ;  
    /// </summary>
    /// <returns></returns>
    private Expr Factor() {
        Expr expr = Unary();
        while(Match(STAR, SLASH)) {
            Token @operator = Previous;
            Expr right = Unary();
            expr = new Binary(expr, @operator, right);
        }

        return expr;
    }
    /// <summary>
    /// unary → ( "!" | "-" ) unary | primary ;
    /// </summary>
    /// <returns></returns>
    private Expr Unary() {
        if(Match(BANG, MINUS)) {
            Token @operator = Previous;
            Expr right = Unary();
            return new Unary(@operator, right);
        }

        return Call();
    }

    private Expr Call() {
        var expr = Primary();
        while(true) {
            if(Match(LEFT_PAREN)){
                expr = FinishCall(expr);
            } else if (Match(DOT)) {
                var name = Consume(IDENTIFIER, "Expected property name after '.'.");
                expr = new Get(expr, name);
            }
            else {
                break;
            }
        }
        return expr;
    }

    private Expr FinishCall(Expr callee) {
        var arguments = new List<Expr>();
        if(!Check(RIGHT_PAREN)){
            do {
                if(arguments.Count >= 255){
                    Error(Peek(), "Can't have more than 255 arguments");
                }
                arguments.Add(Expression());
            } while(Match(COMMA));
        }

         Token paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");

         return new Call(callee, paren, arguments);
    }
    /// <summary>
    /// primary → NUMBER | STRING | "true" | "false" | "nil"
    ///         | "(" expression ")" ;
    /// </summary>
    /// <returns></returns>
    private Expr Primary() {
        if(Match(FALSE)) {
            return new Literal(false);
        }
        if(Match(TRUE)) {
            return new Literal(true);
        }
        if(Match(NIL)){
            return new Literal(null);
        }
        if(Match(NUMBER, STRING)) {
            return new Literal(Previous.Literal);
        }

        if(Match(LEFT_PAREN)) {
            Expr expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression.");
            return new Grouping(expr);
        }
        
        if(Match(THIS)) {
            return new This(Previous);
        }

        if(Match(IDENTIFIER)) {
            return new Variable(Previous);
        }

        if(Match(SUPER)){
            var keyword = Previous;
            Consume(DOT, "Expect '.' after 'super'.");
            var method = Consume(IDENTIFIER, "Expect superclass method name.");
            return new Super(keyword, method);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType type, string message) {
        if(Check(type)) {
            return Advance();
        }

        throw Error(Peek(), message);
    }

    private ParseErrorException Error(Token token, string message) {
        Lox.Error(token, message);
        return new ParseErrorException();
    }

    private void Synchronize() {
        Advance();
        while(!IsAtEnd) {
            if(Previous.Type == SEMICOLON) {
                return;
            }

            switch(Peek().Type) {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            Advance();
        }
    }

    /// <summary>
    /// Checks to see if the current token has any of the given types.
    /// If check succeeds, advances current token and returns.
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    private bool Match(params TokenType[] types) {
        foreach(TokenType type in types) {
            if(Check(type)) {
                Advance();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if the current token is of the given type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private bool Check(TokenType type) {
        if(IsAtEnd) {
            return false;
        }

        return Peek().Type == type;
    }

    /// <summary>
    /// Consumes the current token and returns it. 
    /// </summary>
    /// <returns></returns>
    private Token Advance() {
        if(!IsAtEnd) {
            current++;
        }
        return Previous;
    }

    /// <summary>
    /// Checks if we’ve run out of tokens to parse
    /// </summary>
    private bool IsAtEnd => Peek().Type == EOF;

    /// <summary>
    /// Returns the current token we have yet to consume
    /// </summary>
    /// <returns></returns>
    private Token Peek() => tokens[current];

    /// <summary>
    /// Returns the most recently consumed token
    /// </summary>
    private Token Previous => tokens[current - 1];
}
