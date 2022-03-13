using csso.Common;

namespace csso.NodeCore;

public enum ArgType {
    In,
    Out,
    Config
}

public abstract class FunctionArg {
    protected FunctionArg(string name, Type type, Int32 argumentIndex) {
        Name = name;
        Type = type;
        ArgumentIndex = argumentIndex;
    }

    public Type Type { get; } = typeof(void);
    public string Name { get; } = "";
    public string FullName => Name + ":" + Type.Name;
    public abstract ArgType ArgType { get; }
    public Int32 ArgumentIndex { get; }
}

public class FunctionInput : FunctionArg {
    public FunctionInput(string name, Type type, Int32 argumentIndex) : base(name, type, argumentIndex) { }

    public override ArgType ArgType => ArgType.In;
}

public class FunctionOutput : FunctionArg {
    public FunctionOutput(string name, Type type, Int32 argumentIndex) : base(name, type, argumentIndex) { }

    public override ArgType ArgType => ArgType.Out;
}

public class FunctionConfig : FunctionArg {
    private Object? _value = default(Type);

    public FunctionConfig(String name, Type type, Int32 argumentIndex) : base(name, type, argumentIndex) { }

    public Object? Value {
        get => _value;
        set => _value = Convert.ChangeType(value, Type);
    }

    public override ArgType ArgType => ArgType.Config;

    public static FunctionConfig Create(String name, Type type, Int32 index) {
        var result =
            typeof(FunctionConfig<>)
                .MakeGenericType(type)
                .GetConstructors()
                .Single()!
                .Invoke(new Object[3] {name, type, index});
        return (FunctionConfig) result;
    }
}

public class FunctionConfig<T> : FunctionConfig {
    public FunctionConfig(string name, Type type, Int32 argumentIndex) : base(name, type, argumentIndex) { }

    public T TypedValue {
        get => (T) Value!;
        set {
            if (value != null)
                Check.True(value.GetType() == Type);

            Value = value;
        }
    }
}