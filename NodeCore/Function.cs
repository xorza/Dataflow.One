using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace csso.NodeCore {

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class OutputAttribute : Attribute {
    public OutputAttribute() { }
}

public interface IFunction {
    IReadOnlyList<FunctionInput> Inputs { get; }
    IReadOnlyList<FunctionOutput> Outputs { get; }
    string Name { get; }
    string Description { get; }
    bool IsOutput { get; }
}

public class Function : IFunction {
    public IReadOnlyList<FunctionInput> Inputs { get; }
    public IReadOnlyList<FunctionOutput> Outputs { get; }
    public string Name { get; }
    public string Description { get; }
    public bool IsOutput { get; }

    public Function(String name, MethodInfo method) {
        List<FunctionInput> inputs = new();
        List<FunctionOutput> outputs = new();

        Inputs = inputs.AsReadOnly();
        Outputs = outputs.AsReadOnly();
        Name = name;

        DescriptionAttribute? descr =
            Attribute.GetCustomAttribute(method, typeof(DescriptionAttribute))
                as DescriptionAttribute;
        Description = descr?.Description ?? "";

        IsOutput = (Attribute.GetCustomAttribute(method, typeof(OutputAttribute)) is OutputAttribute);

        ParameterInfo[] parameters = method.GetParameters();
        foreach (var parameter in parameters) {
            FunctionArg arg;

            if (parameter.IsOut) {
                outputs.Add(new FunctionOutput());
                arg = outputs.Last();
            }
            else {
                inputs.Add(new FunctionInput());
                arg = inputs.Last();
            }

            arg.Name = parameter.Name!;
            if (parameter.ParameterType.IsByRef) {
                arg.Type = parameter.ParameterType.GetElementType()!;
            }
            else {
                arg.Type = parameter.ParameterType;
            }
        }
    }

    public Function(String name, Delegate func) : this(name, func.Method) { }
    public Function(Delegate func) : this(func.Method.Name, func.Method) { }
}
}