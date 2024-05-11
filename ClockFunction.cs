namespace sharplox;

public class ClockFunction : ILoxCallable
{
    public int Arity => 0;

    public object Call(Interpreter interpreter, List<object> args) {
        return (double)DateTime.Now.Millisecond;
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}
