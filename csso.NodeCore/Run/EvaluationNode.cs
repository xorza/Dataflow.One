using System.Diagnostics;
using csso.Common;

namespace csso.NodeCore.Run;

public enum EvaluationState {
    Idle,
    Processed,
    ArgumentsSet,
    Invoked
}

public class EvaluationNode {
    private static readonly Object Empty = new NotFound();

    private readonly List<DependencyValue> _dependencyValues = new();

    public EvaluationNode(Node node) {
        Node = node;
        Behavior = node.FinalBehavior;
        ArgValues = Enumerable.Repeat(Empty, Node.Function.Args.Count).ToArray();
        HasOutputValues = false;
        Reset();
    }

    public Node Node { get; }
    public Object?[] ArgValues { get; }
    public bool HasOutputValues { get; private set; }
    public FunctionBehavior Behavior { get; }
    public bool ArgumentsUpdatedThisFrame { get; private set; }
    public EvaluationState State { get; private set; } = EvaluationState.Idle;
    public double ExecutionTime { get; private set; } = double.NaN;

    public object? GetOutputValue(FunctionOutput output) {
        Check.True(HasOutputValues);

        var index = Node.Function.Args.FirstIndexOf(output);
        Check.False(index == null);

        return ArgValues[index!.Value];
    }

    public void Reset() {
        State = EvaluationState.Idle;
    }

    public void Process(bool hasUpdatedArguments) {
        Check.True(State == EvaluationState.Idle);

        ArgumentsUpdatedThisFrame = hasUpdatedArguments;
        State = EvaluationState.Processed;
    }

    public void ProcessArguments() {
        if (State == EvaluationState.ArgumentsSet) return;

        Check.True(State == EvaluationState.Processed);

        ArgValues.Populate(Empty);
        _dependencyValues.Clear();

        Node.ConfigValues.Foreach(config => ArgValues[config.Config.ArgumentIndex] = config.Value);

        foreach (var functionArg in Node.Function.Args)
            if (functionArg is FunctionInput inputArg) {
                var valueConnection = Node.ValueConnections.SingleOrDefault(_ => _.Input == inputArg);
                var bindingConnection = Node.BindingConnections.SingleOrDefault(_ => _.Input == inputArg);

                if (valueConnection != null) {
                    Check.True(bindingConnection == null);
                    ArgValues[valueConnection.Input.ArgumentIndex] = valueConnection.Value;
                }

                if (bindingConnection != null) {
                    Check.True(valueConnection == null);
                    Check.True(
                        bindingConnection.Input == Node.Function.Args[bindingConnection.Input.ArgumentIndex]
                    );

                    _dependencyValues.Add(new DependencyValue {
                        TargetNode = bindingConnection.TargetNode,
                        Index = bindingConnection.Input.ArgumentIndex,
                        Target = bindingConnection.Target
                    });
                }
            } else if (functionArg is FunctionOutput) {
                ArgValues[functionArg.ArgumentIndex] = null;
            } else if (functionArg is FunctionConfig configArg) {
                var configValue = Node.ConfigValues.Single(_ => _.Config == configArg);
                ArgValues[functionArg.ArgumentIndex] = configValue.Value;
            } else {
                Check.Fail();
            }

        State = EvaluationState.ArgumentsSet;
    }

    public void Invoke(Executor executor) {
        if (State == EvaluationState.Invoked) return;

        Check.True(State == EvaluationState.ArgumentsSet);
        Check.False(HasOutputValues
                    && !ArgumentsUpdatedThisFrame
                    && Behavior != FunctionBehavior.Proactive);

        {
            Stopwatch sw = new();
            sw.Start();

            _dependencyValues.ForEach(_ => {
                var targetEvaluationNode = executor.GetEvaluationNode(_.TargetNode);
                Check.True(targetEvaluationNode.State >= EvaluationState.Processed);
                ArgValues[_.Index] = targetEvaluationNode.GetOutputValue(_.Target);
            });

            ValidateArguments();

            Node.Function.Invoke(ArgValues.Length == 0 ? null : ArgValues);

            sw.Stop();
            ExecutionTime = sw.ElapsedMilliseconds * 1.0;
        }

        if (!Node.Function.IsProcedure) HasOutputValues = true;

        State = EvaluationState.Invoked;
    }

    private void ValidateArguments() {
        for (var i = 0; i < ArgValues.Length; i++)
            if (ArgValues[i] == Empty)
                throw new ArgumentMissingException(Node, (FunctionInput) Node.Function.Args[i]);
    }

    private class NotFound { }

    private struct DependencyValue {
        public int Index { get; set; }
        public Node TargetNode { get; set; }
        public FunctionOutput Target { get; set; }
    }
}