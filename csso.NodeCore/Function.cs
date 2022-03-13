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

public class Function {
    protected Function() { }

    public Function(String name, Delegate func) {
        Refresh(name, func);
    }

    public Function(String name, Delegate func, FunctionBehavior functionBehavior) : this(name, func) {
        Behavior = functionBehavior;
    }

    public String Namespace { get; private set; }
    public Delegate Delegate { get; private set; }
    public IReadOnlyList<FunctionInput> Inputs { get; private set; }
    public IReadOnlyList<FunctionOutput> Outputs { get; private set; }
    public IReadOnlyList<FunctionArg> Args { get; private set; }
    public IReadOnlyList<FunctionConfig> Config { get; private set; }
    public FunctionBehavior Behavior { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsProcedure => Outputs.Count == 0;
    public string FullName => Namespace + "::" + Name;

    protected void Refresh(String name, Delegate func) {
        Check.Argument(func.Method.ReturnType == typeof(bool), nameof(func));

        List<FunctionArg> args = new();

        Args = args.AsReadOnly();
        Name = name;
        Delegate = func;
        Namespace = func.Method.DeclaringType?.FullName ?? "";

        var descr =
            Attribute.GetCustomAttribute(func.Method, typeof(DescriptionAttribute))
                as DescriptionAttribute;
        Description = descr?.Description ?? "";

        var reactive =
            Attribute.GetCustomAttribute(func.Method, typeof(ReactiveAttribute))
                as ReactiveAttribute;
        Behavior = reactive == null ? FunctionBehavior.Proactive : FunctionBehavior.Reactive;

        var parameters = func.Method.GetParameters();
        for (Int32 i = 0; i < parameters.Length; i++) {
            var parameter = parameters[i];

            var outputAttribute =
                Attribute.GetCustomAttribute(parameter, typeof(OutputAttribute)) as OutputAttribute;
            var configAttribute =
                Attribute.GetCustomAttribute(parameter, typeof(ConfigAttribute)) as ConfigAttribute;

            if (outputAttribute != null && configAttribute != null) throw new Exception("fghoji4r5");

            var argName = parameter.Name!;
            Type argType;
            if (parameter.ParameterType.IsByRef || parameter.ParameterType.IsPointer)
                argType = parameter.ParameterType.GetElementType()!;
            else
                argType = parameter.ParameterType;

            FunctionArg arg;

            if (outputAttribute != null) {
                arg = new FunctionOutput(argName, argType, i);
            } else if (configAttribute != null) {
                var config = FunctionConfig.Create(argName, argType, i);
                if (configAttribute.DefaultValue != null) {
                    Check.True(argType == configAttribute.DefaultValue.GetType());
                    config.Value = configAttribute.DefaultValue;
                }

                arg = config;
            } else {
                arg = new FunctionInput(argName, argType, i);
            }

            args.Add(arg);
        }

        Inputs = args.OfType<FunctionInput>().ToList().AsReadOnly();
        Outputs = args.OfType<FunctionOutput>().ToList().AsReadOnly();
        Config = args.OfType<FunctionConfig>().ToList().AsReadOnly();

        if (IsProcedure)
            Behavior = FunctionBehavior.Proactive;
    }

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
