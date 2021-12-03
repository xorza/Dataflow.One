using System;
using System.Linq;
using System.Security.AccessControl;
using csso.Common;

namespace csso.NodeCore {
public enum ArgType {
    In,
    Out,
    Config
}

public abstract class FunctionArg {
    protected FunctionArg(string name, Type type) {
        Name = name;
        Type = type;
    }

    public Type Type { get; } = typeof(void);
    public string Name { get; } = "";
    public string FullName => Name + ":" + Type.Name;
    public abstract ArgType ArgType { get; }
}

public class FunctionInput : FunctionArg {
    public FunctionInput(string name, Type type) : base(name, type) { }

    public override ArgType ArgType => ArgType.In;
}

public class FunctionOutput : FunctionArg {
    public FunctionOutput(string name, Type type) : base(name, type) { }

    public override ArgType ArgType => ArgType.Out;
}

public class FunctionConfig : FunctionArg {
    protected FunctionConfig(String name, Type type) : base(name, type) { }

    private Object? _defaultValue = default(Type);
    
    public Object? DefaultValue { 
        get => _defaultValue;
        set {
            if (value != null)
                Check.True(value.GetType() == Type);
    
            _defaultValue = value;
        }
    }

    public override ArgType ArgType => ArgType.Config;

    public static FunctionConfig Create(String name, Type type) {
        Object result =
            typeof(FunctionConfig<>)
                .MakeGenericType(type)
                .GetConstructors()
                .Single()!
                .Invoke(new Object[2] {name, type});
        return  (FunctionConfig)result;
    }
}

public class FunctionConfig<T> : FunctionConfig {
    public T TypedValue {
        get => (T) DefaultValue!;
        set {
            if (value != null)
                Check.True(value.GetType() == Type);

            DefaultValue = value;
        }
    }

    public FunctionConfig(string name, Type type) : base(name, type) { }
}
}