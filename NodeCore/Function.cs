using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace csso.NodeCore {
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
sealed class ArgumentDescriptionAttribute : Attribute {
    public string Text { get; }

    public ArgumentDescriptionAttribute(String text) {
        Text = text;
    }
}

public interface IFunction {
    IReadOnlyList<FunctionInput> Inputs { get; }
    IReadOnlyList<FunctionOutput> Outputs { get; }
    string Name { get; }
    string Description { get; }
}

public class Function : IFunction {
    public IReadOnlyList<FunctionInput> Inputs { get; }
    public IReadOnlyList<FunctionOutput> Outputs { get; }
    public string Name { get; }
    public string Description { get; }

    public Function(String name, Delegate func) {
        List<FunctionInput> inputs = new();
        List<FunctionOutput> outputs = new();

        Inputs = inputs.AsReadOnly();
        Outputs = outputs.AsReadOnly();
        Name = name;

        ArgumentDescriptionAttribute? descr =
            Attribute.GetCustomAttribute(func.Method, typeof(ArgumentDescriptionAttribute))
                as ArgumentDescriptionAttribute;

        Description = descr?.Text ?? "";

        ParameterInfo[] parameters = func.Method.GetParameters();
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
}
}