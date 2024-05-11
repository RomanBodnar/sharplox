namespace sharplox;

public class RuntimeErrorException : Exception
{
    public readonly Token token;

    public RuntimeErrorException(Token token, string message) 
        : base(message) {
        this.token = token;
    } 
}
