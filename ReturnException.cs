namespace sharplox;

public class ReturnException : Exception
{
    public readonly object? Value;

    public ReturnException(object? value) {
        this.Value = value;
    }
}
