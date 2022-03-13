using System.Diagnostics;
using csso.Common;
using Debug = csso.Common.Debug;

namespace csso.NodeCore.Run;

public class EvaluationNode {
    private static readonly Object Empty = new NotFound();

    public EvaluationNode(Node node) {
        Node = node;
        Behavior = node.FinalBehavior;

        NextIteration();

        ArgDependencies.Clear();

        foreach (var input in Node.Function.Inputs) {
            var connection = Node.Connections.SingleOrDefault(_ => _.Input == input);

            if (connection is BindingConnection bindingConnection) {
                ArgDependencies.Add(bindingConnection);
            }
        }
    }

    public Node Node { get; }
    public Object?[] ArgValues { get; private set; }

    public List<BindingConnection> ArgDependencies { get; } = new();
    public bool HasOutputValues { get; private set; }
    public FunctionBehavior Behavior { get; private set; }
    public bool ArgumentsUpdatedThisFrame { get; private set; }
    public bool ProcessedThisFrame { get; private set; }
    public bool InvokedThisFrame { get; private set; }

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

        if (InvokedThisFrame) {
            return;
        }

        InvokedThisFrame = true;

        Stopwatch sw = new();
        sw.Start();

        ArgValues = Enumerable.Repeat(Empty, Node.Function.Args.Count).ToArray();
        

        ProcessArguments(executor);

        Debug.Assert.True(ArgValues.None(_ => _ == Empty));
        

        Node.Function.Invoke(ArgValues.Length == 0 ? null : ArgValues);

        if (!Node.Function.IsProcedure) {
            HasOutputValues = true;
        }

        sw.Stop();
        ExecutionTime = sw.ElapsedMilliseconds * 1.0;
    }

    private void ProcessArguments(Executor executor) {
        Node.ConfigValues.Foreach(config => ArgValues[config.Config.ArgumentIndex] = config.Value);
        Node.Function.Outputs.Foreach(output => ArgValues[output.ArgumentIndex] = null);

        foreach (var connection in Node.Connections) {
            if (connection is ValueConnection valueConnection) {
                ArgValues[connection.Input.ArgumentIndex] = valueConnection.Value;
            }

            if (connection is BindingConnection bindingConnection) {
                Debug.Assert.AreSame(bindingConnection.Input, Node.Function.Args[bindingConnection.Input.ArgumentIndex]);

                EvaluationNode en = executor.GetExecutionNode(bindingConnection.TargetNode);
                Check.True(en.HasOutputValues);

                ArgValues[bindingConnection.Input.ArgumentIndex] = en.GetOutputValue(bindingConnection.Target);
            }
        }
    }

    private class NotFound { }

    public void NextIteration() {
        ProcessedThisFrame = false;
        ArgumentsUpdatedThisFrame = false;
        Behavior = Node.FinalBehavior;
        InvokedThisFrame = false;
    }

    public void Update(bool hasUpdatedArguments) {
        ProcessedThisFrame = true;
        ArgumentsUpdatedThisFrame = hasUpdatedArguments;
    }
}