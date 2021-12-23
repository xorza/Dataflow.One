using System.Diagnostics;
using csso.Common;
using Debug = csso.Common.Debug;

namespace csso.NodeCore.Run;

public class ExecutionNode {
    private static readonly Object Empty = new NotFound();

    public ExecutionNode(Node node) {
        Node = node;
        ArgCount = node.Function.Args.Count;

        ArgValues = Enumerable.Repeat(Empty, ArgCount).ToArray();
        ArgDependencies = Enumerable.Repeat((OutputConnection?) null, ArgCount).ToArray();

        Behavior = node.FinalBehavior;
    }

    public Node Node { get; }
    public Object?[] ArgValues { get; }
    public OutputConnection?[] ArgDependencies { get; }
    public Int32 ArgCount { get; }
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

        for (var i = 0; i < ArgCount; i++) {
            switch (Node.Function.Args[i].ArgType) {
                case ArgType.Config:
                    ArgValues[i] = Node.ConfigValues[i].Value;
                    break;
                case ArgType.Out:
                    ArgValues[i] = Pool(Node.Function.Args[i].Type);
                    break;
                case ArgType.In:
                    if (ArgDependencies[i] != null) {
                        ExecutionNode en = executor.GetExecutionNode(ArgDependencies[i]!.OutputNode);

                        Check.True(en.HasOutputValues);
                        ArgValues[i] = en.GetOutputValue(ArgDependencies[i]!.Output);
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
        }

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

                if (connection is ValueConnection valueConnection) {
                    ArgValues[i] = valueConnection.Value;
                }

                ArgDependencies[i] = connection as OutputConnection;
            } 
        }
    }
}