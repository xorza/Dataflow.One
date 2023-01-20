using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using csso.Common;

namespace csso.NodeCore.Run;

public enum EvaluationState {
    Idle,
    Processed,
    ArgumentsSet,
    Invoked
}

public class EvaluationNode {
    private static readonly object Empty = new();
    private readonly List<DependencyValue> _dependencyValues = new();
    private List<DataSubscription> _dataSubscriptions = new();

    public EvaluationNode(Node node) {
        Node = node;
        Behavior = node.Behavior;
        HasOutputValues = false;

        ArgValues = Enumerable.Repeat(Empty, Node.Args.Count).ToArray();

        Reset();
    }

    public Node Node { get; }

    public bool HasOutputValues { get; private set; }
    public FunctionBehavior Behavior { get; }
    public bool ShouldInvokeThisFrame { get; private set; }
    public EvaluationState State { get; private set; } = EvaluationState.Idle;
    public double ExecutionTime { get; private set; } = double.NaN;

    public object?[] ArgValues { get; }

    private object? GetOutputValue(FunctionArg output) {
        Check.True(HasOutputValues);

        return ArgValues[output.ArgumentIndex];
    }

    public void Reset() {
        State = EvaluationState.Idle;
        ShouldInvokeThisFrame = false;
    }
    
    public void ForceInvoke() {
        ShouldInvokeThisFrame = true;
    }

    public void Process(bool shouldInvokeThisFrame) {
        Check.True(State == EvaluationState.Idle);

        ShouldInvokeThisFrame |= shouldInvokeThisFrame;

        var newDataSubscriptions = Node.Graph.GetDataSubscriptions(Node);

        if (!newDataSubscriptions.SequenceEqual(_dataSubscriptions)) {
            ShouldInvokeThisFrame = true;
            _dataSubscriptions = newDataSubscriptions;
        }

        State = EvaluationState.Processed;
    }

    public void PrepareArguments() {
        if (State == EvaluationState.ArgumentsSet) {
            return;
        }

        Check.True(State == EvaluationState.Processed);

        _dependencyValues.Clear();

        foreach (var nodeArg in Node.Args) {
            if (nodeArg.ArgDirection == ArgDirection.In) {
                var dataSubscription = Node.Graph.GetDataSubscription(nodeArg);

                if (dataSubscription != null) {
                    Check.True(
                        dataSubscription.Subscriber == Node.Args[dataSubscription.Subscriber.FunctionArg.ArgumentIndex]
                    );

                    if (dataSubscription.Value != null) {
                        ArgValues[nodeArg.FunctionArg.ArgumentIndex] = dataSubscription.Value;
                    } else {
                        _dependencyValues.Add(
                            new DependencyValue {
                                TargetNode = dataSubscription.Source!.Node,
                                Index = dataSubscription.Subscriber.FunctionArg.ArgumentIndex,
                                Target = dataSubscription.Source.FunctionArg
                            });
                    }
                }
            } else if (nodeArg.ArgDirection == ArgDirection.Out) {
                ArgValues[nodeArg.FunctionArg.ArgumentIndex] = null;
            } else {
                Check.Fail();
            }
        }

        State = EvaluationState.ArgumentsSet;
    }

    public void Invoke(Executor executor) {
        if (State == EvaluationState.Invoked) {
            return;
        }

        Check.True(State == EvaluationState.ArgumentsSet);

        {
            Stopwatch sw = new();
            sw.Start();

            _dependencyValues.ForEach(_ => {
                var targetEvaluationNode = executor.GetEvaluationNode(_.TargetNode);
                Check.True(targetEvaluationNode.State >= EvaluationState.Processed);
                ArgValues[_.Index] = targetEvaluationNode.GetOutputValue(_.Target);
            });

            ValidateArguments();

            ((FunctionNode) Node).Function.Invoke(ArgValues.Length == 0 ? null : ArgValues);

            sw.Stop();
            ExecutionTime = sw.ElapsedMilliseconds * 1.0;
        }

        HasOutputValues = true;

        State = EvaluationState.Invoked;
    }

    private void ValidateArguments() {
        for (var i = 0; i < ArgValues.Length; i++) {
            if (ArgValues[i] == Empty) {
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