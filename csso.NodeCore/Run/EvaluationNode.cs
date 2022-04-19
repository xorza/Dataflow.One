using System.ComponentModel;
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
    private readonly List<DependencyValue> _dependencyValues = new();

    private IArgumentProvider ArgumentProvider { get; }

    public EvaluationNode(IArgumentProvider argumentProvider, Node node) {
        Node = node;
        Behavior = node.Behavior;
        HasOutputValues = false;
        ArgumentProvider = argumentProvider;

        Reset();
    }

    public Node Node { get; }

    public Object?[] ArgValues() {
        return ArgumentProvider.GetArguments(this);
    }

    public bool HasOutputValues { get; private set; }
    public FunctionBehavior Behavior { get; }
    public bool ArgumentsUpdatedThisFrame { get; private set; }
    public EvaluationState State { get; private set; } = EvaluationState.Idle;
    public double ExecutionTime { get; private set; } = double.NaN;

    public object? GetOutputValue(FunctionArg output) {
        Check.True(HasOutputValues);

        return ArgValues()[output.ArgumentIndex];
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
        if (State == EvaluationState.ArgumentsSet) {
            return;
        }

        Check.True(State == EvaluationState.Processed);

        _dependencyValues.Clear();

        foreach (var nodeArg in Node.Args)
            if (nodeArg.ArgType == ArgType.In) {
                var dataSubscription = Node.Graph.GetDataSubscription(nodeArg);

                if (dataSubscription != null) {
                    Check.True(
                        dataSubscription.Subscriber == Node.Args[dataSubscription.Subscriber.FunctionArg.ArgumentIndex]
                    );

                    _dependencyValues.Add(
                        new DependencyValue {
                            TargetNode = dataSubscription.Source.Node,
                            Index = dataSubscription.Subscriber.FunctionArg.ArgumentIndex,
                            Target = dataSubscription.Source.FunctionArg
                        });
                }
            } else if (nodeArg.ArgType == ArgType.Out) {
                ArgValues()[nodeArg.FunctionArg.ArgumentIndex] = null;
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
                ArgValues()[_.Index] = targetEvaluationNode.GetOutputValue(_.Target);
            });

            ValidateArguments();

            ((FunctionNode) Node).Function.Invoke(ArgValues().Length == 0 ? null : ArgValues());

            sw.Stop();
            ExecutionTime = sw.ElapsedMilliseconds * 1.0;
        }

        HasOutputValues = true;

        State = EvaluationState.Invoked;
    }

    private void ValidateArguments() {
        for (var i = 0; i < ArgValues().Length; i++) {
            if (ArgValues()[i] == Empty.One) {
                throw new ArgumentMissingException(Node, Node.Args[i].FunctionArg);
            }
        }
    }

    private class NotFound { }

    private struct DependencyValue {
        public int Index { get; set; }
        public Node TargetNode { get; set; }
        public FunctionArg Target { get; set; }
    }
}