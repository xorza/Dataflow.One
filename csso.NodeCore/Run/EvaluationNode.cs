using System.Diagnostics;
using csso.Common;
using Debug = csso.Common.Debug;

namespace csso.NodeCore.Run;

public class EvaluationNode {
    private static readonly Object Empty = new NotFound();

    public EvaluationNode(Node node) {
        Node = node;
        ArgValues = Enumerable.Repeat(Empty, Node.Function.Args.Count).ToArray();
        Behavior = node.FinalBehavior;
    }

    public Node Node { get; }
    public Object?[] ArgValues { get; }

    public List<BindingConnection> ArgDependencies { get; } = new();
    public bool HasOutputValues { get; private set; }
    public FunctionBehavior Behavior { get; private set; }
    public bool ArgumentsUpdatedThisFrame { get; set; }
    public bool ProcessedThisFrame { get; set; }

    public double ExecutionTime { get; private set; } = Double.NaN;

    public object? GetOutputValue(FunctionOutput outputConnectionOutput) {
        Check.True(HasOutputValues);

        var index = Node.Function.Args.FirstIndexOf(outputConnectionOutput);
        if (index == null)
            throw new Exception("unerty i");
        return ArgValues[index.Value];
    }

    public void Invoke(Executor executor) {
        if (HasOutputValues
            && !ArgumentsUpdatedThisFrame
            && Behavior != FunctionBehavior.Proactive) {
            throw new Exception("rb56u etdg");
        }

        if (!ProcessedThisFrame) {
            throw new Exception("veldkfgyuivhnwo4875");
        }

        Stopwatch sw = new();
        sw.Start();

        Node.ConfigValues.Foreach(config => ArgValues[config.Config.Index] = config.Value);
        Node.Function.Outputs.Foreach(output => ArgValues[output.Index] = Pool(output.Type));

        foreach (var input in Node.Function.Inputs) {
            var dependency = ArgDependencies.Single(_ => _.Input == input);
            Debug.Assert.AreSame(dependency.Input, Node.Function.Args[input.Index]);

            EvaluationNode en = executor.GetExecutionNode(dependency.TargetNode);
            Check.True(en.HasOutputValues);

            ArgValues[input.Index] = en.GetOutputValue(dependency.Target);
        }


        Debug.Assert.True(ArgValues.None(_ => _ == Empty));

        Node.Function.Invoke(ArgValues.Length == 0 ? null : ArgValues);

        if (!Node.Function.IsProcedure) {
            HasOutputValues = true;
        }

        sw.Stop();
        ExecutionTime = sw.ElapsedMilliseconds / 1000.0;
    }

    private Object Pool(Type type) {
        return Activator.CreateInstance(type)!;
    }

    private class NotFound { }

    public void NextIteration() {
        ProcessedThisFrame = false;
        ArgumentsUpdatedThisFrame = false;
        Behavior = Node.FinalBehavior;
        ArgDependencies.Clear();

        foreach (var input in Node.Function.Inputs) {
            var connection = Node.Connections.SingleOrDefault(_ => _.Input == input);
            if (connection == null) {
                System.Diagnostics.Debug.WriteLine("asfdsdfgweg");
                return;
            }

            if (connection is ValueConnection valueConnection) {
                ArgValues[input.Index] = valueConnection.Value;
            }

            if (connection is BindingConnection bindingConnection) {
                ArgDependencies.Add(bindingConnection);
            }
        }
    }
}