using System.ComponentModel;
using System.Data.Common;
using static sharplox.TokenType;
using static System.Char;
using discardFunc = System.Func<sharplox.TokenType?>;
namespace sharplox;

public class Scanner {
    private readonly string source;
    private readonly List<Token> tokens = [];

    /// <summary>
    /// the first character in the lexeme being scanned
    /// </summary>
    private int start = 0; 
    
    /// <summary>
    /// Points at character currntly being considered
    /// </summary>
    private int current = 0;
    private int line = 1;

    private readonly Dictionary<string, TokenType> keywords = new () {
        ["and"] = AND,
        ["class"] = CLASS,
        ["else"] = ELSE,
        ["false"] = FALSE,
        ["for"] = FOR,
        ["fun"] = FUN,
        ["if"] = IF,
        ["nil"] = NIL,
        ["or"] = OR,
        ["print"] = PRINT,
        ["return"] = RETURN,
        ["super"] = SUPER,
        ["this"] = THIS,
        ["true"] = TRUE,
        ["var"] = VAR,
        ["while"] = WHILE
    };

    public Scanner(string source) {
        this.source = source;
    }

    public List<Token> ScanTokens() {
        while (!IsAtEnd){
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(EOF, "", null, line));
        return tokens;
    }

    private void ScanToken() {
        char c = Advance();
       
        TokenType? token = c switch {
            '(' => LEFT_PAREN,
            ')' => RIGHT_PAREN,
            '{' => LEFT_BRACE,
            '}' => RIGHT_BRACE,
            ',' => COMMA,
            '.' => DOT,
            '-' => MINUS,
            '+' => PLUS,
            ';' => SEMICOLON,
            '*' => STAR,
            '!' when MatchNext('=') => BANG_EQUAL,
            '!' => BANG,
            '=' when MatchNext('=') => EQUAL_EQUAL,
            '=' => EQUAL,
            '<' when MatchNext('=') => LESS_EQUAL,
            '<' => LESS,
            '>' when MatchNext('=') => GREATER_EQUAL,
            '>' => GREATER, 
            // would be much easier with classic switch
            // but I want to experiment with pattern matching
            '/' when MatchNext('/') => HandleComment(),
            '/' => SLASH,
            //   case ' ':
            //   case '\r':
            //   case '\t':
            //     // Ignore whitespace.
            //     break;
            var whitespace when new char[] {' ', '\r', '\t'}.Contains(whitespace) => DISCARD,
            '\n' => new discardFunc (() => { line++; return DISCARD; })(),
            '"' => String(), // returns DISCARD because we are adding a token inside the function
            var digit when IsAsciiDigit(digit) => Number(),
            var alpha when IsAlpha(alpha) => Identifier(),
            _ => (TokenType?)null
        };

        if(token == null) {
            Lox.Error(line, "Unexpected character");
        } 
        else if(token != DISCARD){
            AddToken(token.Value);
        }

    }

    private char Advance() {
        return source[current++];
    }

    private void AddToken(TokenType type) {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, object? literal) {
        string text = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }

    private bool MatchNext(char expected) {
        // in Advance we already moved forward, so `current` already 
        // points at one character ahead of the one being considered in the switch
        if(IsAtEnd) {
            return false;
        }

        if(source[current] != expected) {
            return false;
        }

        current++;
        return true;
    }

    private char Peek(){
        if(IsAtEnd) {
            return '\0';
        }
        return source[current];
    }

    private char PeekNext() {
        if(current + 1 >= source.Length) {
            return '\0';
        }
        return source[current+1];
    }

    private TokenType HandleComment() {
        while(Peek() != '\n' && !IsAtEnd) {
            Advance();
        }
        return DISCARD;
    }

    private TokenType String() {
        while (Peek() != '"' && !IsAtEnd) {
            if(Peek() == '\n'){
                line++;
            }
            Advance();
        }
        if(IsAtEnd) {
            Lox.Error(line, "Unterminated string.");
        }

        // The closing "
        Advance();

        string value = source.Substring(start + 1 /* + 1 is for opening "*/, current - start - 2 /* - 1 is for closing "*/);
        AddToken(STRING, value);
        return DISCARD;
    }

    private TokenType Number() {
        while(IsAsciiDigit(Peek())) {
            Advance();
        }

        if(Peek() == '.' && IsAsciiDigit(PeekNext())) {
            Advance();
            while(IsAsciiDigit(Peek())){
                Advance();
            }
        }

        string value = source.Substring(start, current - start);
        AddToken(NUMBER, Double.Parse(value));
        return DISCARD;
    }

    public TokenType Identifier() {
        while(IsAlphaNumeric(Peek())) {
            Advance();
        }
        string text = source.Substring(start, current - start);
        if(keywords.ContainsKey(text)) {
            AddToken(keywords[text]);
        } 
        else {
            AddToken(IDENTIFIER);
        }
        return DISCARD;
    }

    private bool IsAtEnd => current >= source.Length;

    private bool IsAlpha(char c) {
        return IsBetween(c, 'a', 'z')
            || IsBetween(c, 'A', 'Z')
            || c == '_';
    }

    private bool IsAlphaNumeric(char c) {
        return IsAlpha(c) || IsAsciiDigit(c);
    }
}
