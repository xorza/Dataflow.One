using System.Diagnostics;
using csso.Common;
using Debug = csso.Common.Debug;

namespace csso.NodeCore.Run;

public class EvaluationNode {
    private static readonly Object Empty = new NotFound();

    public EvaluationNode(Node node) {
        Node = node;
        Behavior = node.FinalBehavior;
        
        ArgValues = Enumerable.Repeat(Empty, Node.Function.Args.Count).ToArray();
        ArgDependencies = new List<BindingConnection>();
        
        foreach (var input in Node.Function.Inputs) {
            var connection = Node.Connections.SingleOrDefault(_ => _.Input == input);

            if (connection is BindingConnection bindingConnection) {
                ArgDependencies.Add(bindingConnection);
            }
        }

        NextIteration();


    }

    public Node Node { get; }
    public Object?[] ArgValues { get; private set; }

    public List<BindingConnection> ArgDependencies { get; }
    public bool HasOutputValues { get; private set; }
    public FunctionBehavior Behavior { get;  }
    public bool ArgumentsUpdatedThisFrame { get; private set; }
    public bool UpdatedThisFrame { get; private set; }
    public bool InvokedThisFrame { get; private set; }

    public double ExecutionTime { get; private set; } = Double.NaN;

    public object? GetOutputValue(FunctionOutput output) {
        Check.True(HasOutputValues);

        var index = Node.Function.Args.FirstIndexOf(output);
        if (index == null) {
            throw new Exception("unerty i");
        }

        return ArgValues[index.Value];
    }

    public void Invoke(Executor executor) {
        if (HasOutputValues
            && !ArgumentsUpdatedThisFrame
            && Behavior != FunctionBehavior.Proactive) {
            throw new Exception("rb56u etdg");
        }

        if (!UpdatedThisFrame) {
            throw new Exception("veldkfgyuivhnwo4875");
        }

        if (InvokedThisFrame) {
            return;
        }

        InvokedThisFrame = true;

        Stopwatch sw = new();
        sw.Start();

        ProcessArguments(executor);
        Node.Function.Invoke(ArgValues.Length == 0 ? null : ArgValues);

        sw.Stop();
        ExecutionTime = sw.ElapsedMilliseconds * 1.0;

        if (!Node.Function.IsProcedure) {
            HasOutputValues = true;
        }
    }

    private void ProcessArguments(Executor executor) {
        ArgValues.Populate(Empty);
        Debug.Assert.True(ArgValues.All(_ => _ == Empty));

        Node.ConfigValues.Foreach(config => ArgValues[config.Config.ArgumentIndex] = config.Value);
        Node.Function.Outputs.Foreach(output => ArgValues[output.ArgumentIndex] = null);

        foreach (var connection in Node.Connections) {
            if (connection is ValueConnection valueConnection) {
                ArgValues[connection.Input.ArgumentIndex] = valueConnection.Value;
            }

            if (connection is BindingConnection bindingConnection) {
                Debug.Assert.AreSame(bindingConnection.Input,
                    Node.Function.Args[bindingConnection.Input.ArgumentIndex]);

                EvaluationNode en = executor.GetEvaluationNode(bindingConnection.TargetNode);
                Check.True(en.HasOutputValues);

                ArgValues[bindingConnection.Input.ArgumentIndex] = en.GetOutputValue(bindingConnection.Target);
            }
        }

        Debug.Assert.True(ArgValues.None(_ => _ == Empty));
    }

    private class NotFound { }

    public void NextIteration() {
        UpdatedThisFrame = false;
        ArgumentsUpdatedThisFrame = false;
        InvokedThisFrame = false;
    }

    public void Update(bool hasUpdatedArguments) {
        UpdatedThisFrame = true;
        ArgumentsUpdatedThisFrame = hasUpdatedArguments;
    }
}