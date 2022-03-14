using System.Diagnostics;
using csso.Common;
using Debug = csso.Common.Debug;

namespace csso.NodeCore.Run;

public enum EvaluationState {
    Exists,
    Processed,
    ArgumentsSet,
    Invoked
}

public class EvaluationNode {
    private class NotFound { }

    private static readonly Object Empty = new NotFound();

    public Node Node { get; }
    public Object?[] ArgValues { get; }
    public List<BindingConnection> ArgDependencies { get; } = new();
    public bool HasOutputValues { get; private set; }
    public FunctionBehavior Behavior { get; }
    public bool ArgumentsUpdatedThisFrame { get; private set; }
    public EvaluationState State { get; private set; } = EvaluationState.Exists;
    public double ExecutionTime { get; private set; } = Double.NaN;

    private struct DependencyValue {
        public int Index { get; set; }
        public Node TargetNode { get; set; }
        public FunctionOutput Target { get; set; }
    }

    private readonly List<DependencyValue> _dependencyValues = new();

    public EvaluationNode(Node node) {
        Node = node;
        Behavior = node.FinalBehavior;

        ArgValues = Enumerable.Repeat(Empty, Node.Function.Args.Count).ToArray();

        Reset(true);
    }

    public object? GetOutputValue(FunctionOutput output) {
        Check.True(HasOutputValues);

        var index = Node.Function.Args.FirstIndexOf(output);
        Check.False(index == null);

        return ArgValues[index!.Value];
    }

    public void ProcessArguments() {
        Check.False(State < EvaluationState.Processed);

        ArgValues.Populate(Empty);
        _dependencyValues.Clear();

        Node.ConfigValues.Foreach(config => ArgValues[config.Config.ArgumentIndex] = config.Value);

        foreach (var functionArg in Node.Function.Args) {
            if (functionArg is FunctionInput inputArg) {
                var connection = Node.Connections.SingleOrDefault(_ => _.Input == inputArg);
                if (connection == null) {
                    throw new InputMissingException(Node, inputArg);
                }

                if (connection is ValueConnection valueConnection) {
                    ArgValues[connection.Input.ArgumentIndex] = valueConnection.Value;
                } else if (connection is BindingConnection bindingConnection) {
                    Debug.Assert.AreSame(bindingConnection.Input,
                        Node.Function.Args[bindingConnection.Input.ArgumentIndex]);

                    _dependencyValues.Add(new DependencyValue() {
                        TargetNode = bindingConnection.TargetNode,
                        Index = bindingConnection.Input.ArgumentIndex,
                        Target = bindingConnection.Target
                    });
                } else {
                    Check.Fail();
                }
            } else if (functionArg is FunctionOutput outputArg) {
                ArgValues[functionArg.ArgumentIndex] = null;
            } else if (functionArg is FunctionConfig configArg) {
                var configValue = Node.ConfigValues.Single(_ => _.Config == configArg);
                ArgValues[functionArg.ArgumentIndex] = configValue.Value;
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

        Check.True(State >= EvaluationState.ArgumentsSet);
        Check.False(HasOutputValues
                    && !ArgumentsUpdatedThisFrame
                    && Behavior != FunctionBehavior.Proactive);

        Stopwatch sw = new();
        sw.Start();

        _dependencyValues.ForEach(_ => {
            EvaluationNode targetEvaluationNode = executor.GetEvaluationNode(_.TargetNode);
            Check.True(targetEvaluationNode.State >= EvaluationState.Processed);
            ArgValues[_.Index] = targetEvaluationNode.GetOutputValue(_.Target);
        });

        Check.False(ArgValues.Contains(Empty));

        Node.Function.Invoke(ArgValues.Length == 0 ? null : ArgValues);

        sw.Stop();
        ExecutionTime = sw.ElapsedMilliseconds * 1.0;

        if (!Node.Function.IsProcedure) {
            HasOutputValues = true;
        }

        State = EvaluationState.Invoked;
    }

    public void Process(bool hasUpdatedArguments) {
        State = EvaluationState.Processed;
        ArgumentsUpdatedThisFrame = hasUpdatedArguments;
    }

    public void Reset(bool resetOutputValues = false) {
        State = EvaluationState.Exists;
        if (resetOutputValues) {
            HasOutputValues = false;

            ArgDependencies.Clear();
            foreach (var input in Node.Function.Inputs) {
                var connection = Node.Connections.SingleOrDefault(_ => _.Input == input);

                if (connection is BindingConnection bindingConnection) {
                    ArgDependencies.Add(bindingConnection);
                }
            }
        }
    }
}