using csso.Common;
using csso.NodeCore;

namespace csso.Calculator; 

internal static class Xtensions {
    public static Int32? FirstIndexOf<T>(this IEnumerable<T> enumerable, T element) {
        var i = 0;
        foreach (var item in enumerable) {
            if (item == null && element == null)
                return i;
            if (element?.Equals(item) ?? item?.Equals(element) ?? false)
                return i;
            ++i;
        }

        return null;
    }
}

public class Executor {
    private ExecutionContext _context = new();

    private Int32 _frameNo;


    public Executor(Graph graph) {
        Graph = graph;

        FrameNoFunction = new Function(
            "Frame number",
            ([Output] ref Int32 frameNumber) => {
                frameNumber = _frameNo;
                return true;
            }
        );
        DeltaTimeFunction = new Function(
            "Frame number",
            ([Output] ref Double deltaTime) => {
                deltaTime = 0.1555f;
                return true;
            }
        );
    }

    public Graph Graph { get; }


    public Function FrameNoFunction { get; }

    public Function DeltaTimeFunction { get; }


    public void Reset() {
        _context = new ExecutionContext();
        _frameNo = 0;
    }

    public void Run() {
        _context.EvaluationNodes.Foreach(_ => {
            _.ProcessedThisFrame = false;
            _.ArgumentsUpdatedThisFrame = false;
        });

        var pathsFromProcedures = GetPathsToProcedures();
        pathsFromProcedures.Foreach(UpdateEvaluationNode);

        var invokationList = GetInvokationList();
        invokationList.Foreach(_ => _.Invoke());

        ++_frameNo;
    }

    private IReadOnlyList<Node> GetPathsToProcedures() {
        List<Node> pathsFromProcedures = new();

        Queue<Node> yetToProcessNodes = new();
        Graph.Nodes
            .Where(_ => _.Function.IsProcedure)
            .Foreach(yetToProcessNodes.Enqueue);

        while (yetToProcessNodes.TryDequeue(out var node)) {
            node.Connections
                .OfType<OutputConnection>()
                .Foreach(_ => yetToProcessNodes.Enqueue(_.OutputNode));

            pathsFromProcedures.Add(node);
        }

        pathsFromProcedures.Reverse();
        return pathsFromProcedures.AsReadOnly();
    }

    private void UpdateEvaluationNode(Node node) {
        var evaluationNode = _context.GetEvaluated(node);
        if (evaluationNode.ProcessedThisFrame)
            return;

        evaluationNode.ProcessedThisFrame = true;

        Debug.Assert.True(evaluationNode.ArgValues.Length == node.Function.Args.Count);

        evaluationNode.ArgumentsUpdatedThisFrame =
            evaluationNode.ArgDependencies
                .SkipNulls()
                .Any(dependency => !dependency!.Node.ArgumentsUpdatedThisFrame);
    }

    private IReadOnlyList<EvaluationNode> GetInvokationList() {
        Queue<EvaluationNode> yetToProcessENodes = new();
        _context.EvaluationNodes
            .Where(_ => _.Node.Function.IsProcedure)
            .Foreach(yetToProcessENodes.Enqueue);

        List<EvaluationNode> invokationList = new();

        while (yetToProcessENodes.Count > 0) {
            var enode = yetToProcessENodes.Dequeue();
            invokationList.Add(enode);

            for (var i = 0; i < enode.ArgCount; i++) {
                var dependency = enode.ArgDependencies[i];
                if (dependency == null)
                    continue;

                if (!dependency.Node.HasOutputValues) {
                    yetToProcessENodes.Enqueue(dependency.Node);
                    continue;
                }

                if (dependency.Behavior == ConnectionBehavior.Once)
                    continue;

                if (dependency.Node.ArgumentsUpdatedThisFrame ||
                    dependency.Node.Behavior == FunctionBehavior.Proactive) {
                    yetToProcessENodes.Enqueue(dependency.Node);
                }
            }
        }

        invokationList.Reverse();
        return invokationList;
    }

    internal class ExecutionContext {
        public List<EvaluationNode> EvaluationNodes { get; } = new();

        public EvaluationNode GetEvaluated(Node node) {
            var result = EvaluationNodes.SingleOrDefault(_ => _.Node == node);

            if (result == null) {
                result = new EvaluationNode(this, node);
                EvaluationNodes.Add(result);
            }

            return result;
        }
    }

    internal class Dependency {
        public Dependency(EvaluationNode evaluationNode, FunctionOutput output, ConnectionBehavior behavior) {
            Node = evaluationNode;
            Output = output;
            Behavior = behavior;
        }

        public EvaluationNode Node { get; }
        public FunctionOutput Output { get; }

        public ConnectionBehavior Behavior { get; }
    }

    internal class EvaluationNode {
        private static readonly Object Empty = new NotFound();

        public EvaluationNode(ExecutionContext context, Node node) {
            Node = node;
            ArgCount = node.Function.Args.Count;

            ArgValues = Enumerable.Repeat(Empty, ArgCount).ToArray();
            ArgDependencies = Enumerable.Repeat((Dependency?) null, ArgCount).ToArray();

            Behavior = node.FinalBehavior;

            for (var i = 0; i < ArgCount; i++) {
                var arg = node.Function.Args[i];

                if (arg is FunctionInput input) {
                    var connection = node.Connections.SingleOrDefault(_ => _.Input == input);
                    if (connection == null) {
                        System.Diagnostics.Debug.WriteLine("asfdsdfgweg");
                        return;
                    }

                    if (connection is ValueConnection valueConnection)
                        ArgValues[i] = valueConnection.Value;

                    if (connection is OutputConnection outputConnection) {
                        var dependencyNode = context.GetEvaluated(outputConnection.OutputNode);

                        if (dependencyNode == null)
                            throw new Exception("setsdfsdf");

                        if (dependencyNode.Behavior == FunctionBehavior.Proactive)
                            Behavior = FunctionBehavior.Proactive;

                        ArgDependencies[i] = new Dependency(
                            dependencyNode,
                            outputConnection.Output,
                            outputConnection.Behavior
                        );
                    }
                } else if (arg is FunctionConfig config) {
                    var value = node.ConfigValues
                        .Single(_ => _.Config == config);
                    ArgValues[i] = value.Value;
                }
            }
        }

        public Node Node { get; }

        public Object?[] ArgValues { get; }
        public Dependency?[] ArgDependencies { get; }

        public Int32 ArgCount { get; }
        public bool HasOutputValues { get; private set; }
        public FunctionBehavior Behavior { get; } = FunctionBehavior.Proactive;

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

                        if (ArgValues[i] == Empty) throw new Exception("dfgsdfhsfgh");

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
    }
}