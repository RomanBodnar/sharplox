
namespace sharplox;

public class LoxClass : ILoxCallable
{
    public readonly string Name;
    public readonly LoxClass? Superclass;
    private readonly Dictionary<string, LoxFunction> methods;

    public LoxClass(string name, LoxClass? superclass, Dictionary<string, LoxFunction> methods){
        Name = name;
        Superclass = superclass;
        this.methods = methods;
    }

    public int Arity {
        get 
        {
            var init = FindMethod("init");
            if(init is not null) {
                return init.Arity;
            }
            return 0;
        }
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        LoxInstance instance = new LoxInstance(this);
        LoxFunction initializer = FindMethod("init");
        if(initializer is not null){
            initializer.Bind(instance).Call(interpreter, arguments);
        }
        return instance;
    }

    public LoxFunction FindMethod(string name) {
        if (methods.ContainsKey(name)) {
            return methods[name];
        }

        if(Superclass != null){
            return Superclass.FindMethod(name);
        }

        return null;
    }

    public override string ToString(){
        return Name;
    }
}
