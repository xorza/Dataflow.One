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

    public Object?[] GetArgValues() {
        return ArgumentProvider.GetArguments(this);
    }

    public bool HasOutputValues { get; private set; }
    public FunctionBehavior Behavior { get; }
    public bool ArgumentsUpdatedThisFrame { get; private set; }
    public EvaluationState State { get; private set; } = EvaluationState.Idle;
    public double ExecutionTime { get; private set; } = double.NaN;

    private object? GetOutputValue(FunctionArg output) {
        Check.True(HasOutputValues);

        return GetArgValues()[output.ArgumentIndex];
    }

    public void Reset() {
        State = EvaluationState.Idle;
    }

    public void Process(bool hasUpdatedArguments) {
        Check.True(State == EvaluationState.Idle);

        ArgumentsUpdatedThisFrame = hasUpdatedArguments;
        State = EvaluationState.Processed;
    }

    public class Arguments {
        public Object?[] ArgValues { get; set; }
    }

    public void PrepareArguments() {
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
                GetArgValues()[nodeArg.FunctionArg.ArgumentIndex] = null;
            } else {
                Check.Fail();
            }

        State = EvaluationState.ArgumentsSet;
    }

    public void Invoke(Executor executor) {
        if (State == EvaluationState.Invoked) return;

        Check.True(State == EvaluationState.ArgumentsSet);

        {
            Stopwatch sw = new();
            sw.Start();

            _dependencyValues.ForEach(_ => {
                var targetEvaluationNode = executor.GetEvaluationNode(_.TargetNode);
                Check.True(targetEvaluationNode.State >= EvaluationState.Processed);
                GetArgValues()[_.Index] = targetEvaluationNode.GetOutputValue(_.Target);
            });

            ValidateArguments();

            ((FunctionNode) Node).Function.Invoke(GetArgValues().Length == 0 ? null : GetArgValues());

            sw.Stop();
            ExecutionTime = sw.ElapsedMilliseconds * 1.0;
        }

        HasOutputValues = true;

        State = EvaluationState.Invoked;
    }

    private void ValidateArguments() {
        for (var i = 0; i < GetArgValues().Length; i++) {
            if (GetArgValues()[i] == Empty.One) {
                throw new ArgumentMissingException(Node, Node.Args[i].FunctionArg);
            }
        }
    }

    private struct DependencyValue {
        public int Index { get; set; }
        public Node TargetNode { get; set; }
        public FunctionArg Target { get; set; }
    }
}