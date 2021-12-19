using csso.Common;

namespace csso.NodeCore.Run;

public class ExecutionNode {
    private static readonly Object Empty = new NotFound();

    public ExecutionNode(Node node) {
        Node = node;
        ArgCount = node.Function.Args.Count;

        ArgValues = Enumerable.Repeat(Empty, ArgCount).ToArray();
        ArgDependencies = Enumerable.Repeat((Dependency?) null, ArgCount).ToArray();

        Behavior = node.FinalBehavior;
    }

    public Node Node { get; }
    public Object?[] ArgValues { get; }
    public Dependency?[] ArgDependencies { get; }
    public Int32 ArgCount { get; }
    public bool HasOutputValues { get; private set; }
    public FunctionBehavior Behavior { get; private set; } = FunctionBehavior.Proactive;
    public bool ArgumentsUpdatedThisFrame { get; set; }
    public bool ProcessedThisFrame { get; set; }

    public object? GetOutputValue(FunctionOutput outputConnectionOutput) {
        Check.True(HasOutputValues);

        var index = Node.Function.Args.FirstIndexOf(outputConnectionOutput);
        if (index == null)
            throw new Exception("unerty i");
        return ArgValues[index.Value];
    }

    public void Invoke() {
        if (HasOutputValues
            && !ArgumentsUpdatedThisFrame
            && Behavior != FunctionBehavior.Proactive)
            throw new Exception("rb56u etdg");
        if (!ProcessedThisFrame)
            throw new Exception("veldkfgyuivhnwo4875");

        for (var i = 0; i < ArgCount; i++)
            switch (Node.Function.Args[i].ArgType) {
                case ArgType.Config:
                    break;
                case ArgType.Out:
                    ArgValues[i] = Pool(Node.Function.Args[i].Type);
                    break;
                case ArgType.In:
                    if (ArgDependencies[i] != null) {
                        Check.True(ArgDependencies[i]!.Node.HasOutputValues);
                        ArgValues[i] = ArgDependencies[i]!.Node
                            .GetOutputValue(ArgDependencies[i]!.Output);
                    }

                    if (ArgValues[i] == Empty) {
                        // throw new Exception("dfgsdfhsfgh");
                        System.Diagnostics.Debug.WriteLine(
                            "Missing one or more arguments. Skipping node invocation.");
                        return;
                    }

                    break;

                default:
                    Debug.Assert.False();
                    return;
            }

        Node.Function.Invoke(ArgValues.Length == 0 ? null : ArgValues);

        if (!Node.Function.IsProcedure)
            HasOutputValues = true;
    }

    private Object Pool(Type type) {
        return Activator.CreateInstance(type)!;
    }

    private class NotFound { }

    public void Refresh(Executor graph) {
        Check.True(ArgCount == Node.Function.Args.Count);
        Check.True(ArgCount == ArgDependencies.Length);
        Check.True(ArgCount == ArgValues.Length);

        ProcessedThisFrame = false;
        ArgumentsUpdatedThisFrame = false;
        Behavior = Node.FinalBehavior;

        for (var i = 0; i < ArgCount; i++) {
            var arg = Node.Function.Args[i];

            if (arg is FunctionInput input) {
                var connection = Node.Connections.SingleOrDefault(_ => _.Input == input);
                if (connection == null) {
                    System.Diagnostics.Debug.WriteLine("asfdsdfgweg");
                    return;
                }

                if (connection is ValueConnection valueConnection)
                    ArgValues[i] = valueConnection.Value;

                if (connection is OutputConnection outputConnection) {
                    var dependencyNode = graph.GetEvaluated(outputConnection.OutputNode);

                    if (dependencyNode == null)
                        throw new Exception("setsdfsdf");

                    // if (dependencyNode.Behavior == FunctionBehavior.Proactive)
                    //     Behavior = FunctionBehavior.Proactive;

                    ArgDependencies[i] = new Dependency(
                        dependencyNode,
                        outputConnection.Output,
                        outputConnection.Behavior
                    );
                }
            } else if (arg is FunctionConfig config) {
                var value = Node.ConfigValues
                    .Single(_ => _.Config == config);
                ArgValues[i] = value.Value;
            }
        }
    }
}