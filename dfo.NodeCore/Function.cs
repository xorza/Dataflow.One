using System;
using System.Collections.Generic;
using System.ComponentModel;
using dfo.Common;

namespace dfo.NodeCore;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class OutputAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ReactiveAttribute : Attribute { }

public enum FunctionBehavior {
    Reactive,
    Proactive
}

public class Function {
    protected Function() { }

    public Function(string name, Delegate func) {
        Name = name;
        SetFunction(func);
    }

    public string Namespace { get; private set; }
    public string Name { get; protected set; }
    public Guid? Id { get; private set; }
    public Delegate Delegate { get; private set; }
    public IReadOnlyList<FunctionArg> Args { get; private set; }
    public FunctionBehavior Behavior { get; set; }
    public string Description { get; private set; }
    public string FullName => Namespace + "::" + Name;

    protected void SetFunction(Delegate func) {
        Check.Argument(func.Method.ReturnType == typeof(bool), nameof(func));

        List<FunctionArg> args = new();

        Args = args.AsReadOnly();
        Delegate = func;
        Namespace = func.Method.DeclaringType?.FullName ?? "";

        var descr =
            Attribute.GetCustomAttribute(func.Method, typeof(DescriptionAttribute))
                as DescriptionAttribute;
        Description = descr?.Description ?? "";

        var reactiveAttribute =
            Attribute.GetCustomAttribute(func.Method, typeof(ReactiveAttribute))
                as ReactiveAttribute;
        Behavior = reactiveAttribute == null ? FunctionBehavior.Proactive : FunctionBehavior.Reactive;

        var idAttribute =
            Attribute.GetCustomAttribute(func.Method, typeof(FunctionIdAttribute))
                as FunctionIdAttribute;
        Id = idAttribute?.Id;

        var parameters = func.Method.GetParameters();
        for (var i = 0; i < parameters.Length; i++) {
            var parameter = parameters[i];


            var argName = parameter.Name!;
            Type argType;
            if (parameter.ParameterType.IsByRef || parameter.ParameterType.IsPointer)
                argType = parameter.ParameterType.GetElementType()!;
            else
                argType = parameter.ParameterType;

            FunctionArg arg;

            if (Attribute.GetCustomAttribute(parameter, typeof(OutputAttribute)) is OutputAttribute outputAttribute)
                arg = new FunctionArg(argName, ArgDirection.Out, argType, i);
            else
                arg = new FunctionArg(argName, ArgDirection.In, argType, i);

            arg.Function = this;

            args.Add(arg);
        }
    }

    public void Invoke(object?[]? args) {
        CheckArgTypes(args);

        var result = Delegate.DynamicInvoke(args);
        if (result is bool boolResult)
            Check.True(boolResult);
        else
            throw new InvalidOperationException("349j-70v5t g");
    }

    private void CheckArgTypes(object?[]? args) {
        for (var i = 0; i < Args.Count; i++) {
            if (args == null) throw new ArgumentNullException(nameof(args));

            if (args.Length != Args.Count) throw new ArgumentException(nameof(args));

            if (args[i] != null) Check.True(Args[i].Type.IsInstanceOfType(args[i]!));
        }
    }
}

public abstract class StatefulFunction : Function {
    public virtual Function CreateInstance() {
        return (Function) Activator.CreateInstance(GetType())!;
    }
}