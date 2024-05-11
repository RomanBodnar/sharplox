namespace sharplox;

public class LoxInstance
{
    private LoxClass @class;
    private readonly Dictionary<string, object> fields = [];

    public LoxInstance(LoxClass @class){
        this.@class = @class;
    }

    public object Get(Token name) {
        if(fields.ContainsKey(name.Lexeme)) {
            return fields[name.Lexeme];
        }

        var method = @class.FindMethod(name.Lexeme);
        if(method is not null) {
            return method.Bind(this);
        }

        throw new RuntimeErrorException(name, $"Undefined property {name.Lexeme}.");
    }

    public void Set(Token name, object value){
        fields.Add(name.Lexeme, value);
    }

    public override string ToString() {
        return @class.Name + " instance";
    }
}
