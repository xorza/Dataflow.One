using System.ComponentModel;
using csso.Common;

namespace csso.NodeCore; 

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class OutputAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ConfigAttribute : Attribute {
    public ConfigAttribute() { }

    public ConfigAttribute(Object defaultValue) {
        DefaultValue = defaultValue;
    }

    public Object? DefaultValue { get; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ReactiveAttribute : Attribute { }

public enum FunctionBehavior {
    Reactive,
    Proactive
}

public interface IFunction {
    IReadOnlyList<FunctionInput> Inputs { get; }
    IReadOnlyList<FunctionOutput> Outputs { get; }
    IReadOnlyList<FunctionArg> Args { get; }
    IReadOnlyList<FunctionConfig> Config { get; }
    string Name { get; }
    string Description { get; }
    bool IsProcedure { get; }
    FunctionBehavior Behavior { get; }

    void Invoke(object?[]? args);
}

public class Function : IFunction {
    public Function(String name, Delegate func) {
        Check.Argument(func.Method.ReturnType == typeof(bool), nameof(func));

        List<FunctionArg> args = new();

        Args = args.AsReadOnly();
        Name = name;
        Delegate = func;

        var descr =
            Attribute.GetCustomAttribute(func.Method, typeof(DescriptionAttribute))
                as DescriptionAttribute;
        Description = descr?.Description ?? "";
        var reactive =
            Attribute.GetCustomAttribute(func.Method, typeof(ReactiveAttribute))
                as ReactiveAttribute;
        Behavior = reactive == null ? FunctionBehavior.Proactive : FunctionBehavior.Reactive;

        var parameters = func.Method.GetParameters();
        foreach (var parameter in parameters) {
            var outputAttribute =
                Attribute.GetCustomAttribute(parameter, typeof(OutputAttribute)) as OutputAttribute;
            var configAttribute =
                Attribute.GetCustomAttribute(parameter, typeof(ConfigAttribute)) as ConfigAttribute;

            if (outputAttribute != null && configAttribute != null) throw new Exception("fghoji4r5");

            var argName = parameter.Name!;
            Type argType;
            if (parameter.ParameterType.IsByRef)
                argType = parameter.ParameterType.GetElementType()!;
            else
                argType = parameter.ParameterType;

            FunctionArg arg;

            if (outputAttribute != null) {
                arg = new FunctionOutput(argName, argType);
            } else if (configAttribute != null) {
                var config = FunctionConfig.Create(argName, argType);
                if (configAttribute.DefaultValue != null) {
                    Check.True(argType == configAttribute.DefaultValue.GetType());
                    config.DefaultValue = configAttribute.DefaultValue;
                }

                arg = config;
            } else {
                arg = new FunctionInput(argName, argType);
            }

            args.Add(arg);
        }

        Inputs = args.OfType<FunctionInput>().ToList().AsReadOnly();
        Outputs = args.OfType<FunctionOutput>().ToList().AsReadOnly();
        Config = args.OfType<FunctionConfig>().ToList().AsReadOnly();

        if (IsProcedure)
            Behavior = FunctionBehavior.Proactive;
    }

    public Function(String name, Delegate func, FunctionBehavior functionBehavior) : this(name, func) {
        Behavior = functionBehavior;
    }

    public Delegate Delegate { get; }
    public IReadOnlyList<FunctionInput> Inputs { get; }
    public IReadOnlyList<FunctionOutput> Outputs { get; }
    public IReadOnlyList<FunctionArg> Args { get; }
    public IReadOnlyList<FunctionConfig> Config { get; }
    public FunctionBehavior Behavior { get; }
    public string Name { get; }
    public string Description { get; }
    public bool IsProcedure => Outputs.Count == 0;

    public void Invoke(object?[]? args) {
        CheckArgTypes(args);

        var result = Delegate.DynamicInvoke(args);
        if (result is bool boolResult)
            Check.True(boolResult);
        else
            throw new InvalidOperationException();
    }

    private void CheckArgTypes(object?[]? args) {
        for (var i = 0; i < Args.Count; i++) {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (args.Length != Args.Count)
                throw new ArgumentException(nameof(args));

            if (args[i] == null)
                Check.True(!Args[i].Type.IsValueType);

            Check.True(Args[i].Type.IsInstanceOfType(args[i]!));
        }
    }
}