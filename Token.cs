namespace sharplox;

public record Token(TokenType Type, string Lexeme, object? Literal, int Line) {
    public override string ToString() {
        return string.Format("{0} {1} {2}", Type, Lexeme, Literal);
    }
}
