using System.Collections.Immutable;
using csso.Common;

namespace csso.NodeCore.Run;

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
    public Int32 FrameNo { get; private set; }
    public ExecutionGraph ExecutionGraph { get;  }

    public EvaluationNode GetExecutionNode(Node node) {
        return ExecutionGraph.EvaluationNodes.Single(_ => _.Node == node);
    }

    public Graph Graph { get; }

    internal Executor(Graph graph) {
        Graph = graph;
        FrameNo = 0;
        ExecutionGraph = new ExecutionGraph(graph);
    }

    public void Run() {
        ExecutionGraph.Sync();

        ExecutionGraph.EvaluationNodes.Foreach(_ => _.NextIteration());

        GetPathsToProcedures()
            .Foreach(UpdateExecutionNode);

        GetInvocationList()
            .Distinct()
            .Foreach(_ => _.Invoke(this));

        ++FrameNo;
    }


    private IReadOnlyList<Node> GetPathsToProcedures() {
        Queue<Node> yetToProcessNodes = new();
        Graph.Nodes
            .Where(_ => _.Function.IsProcedure)
            .Foreach(yetToProcessNodes.Enqueue);

        List<Node> pathsFromProcedures = new();
        while (yetToProcessNodes.TryDequeue(out var node)) {
            node.Connections
                .OfType<BindingConnection>()
                .Select(_ => _.TargetNode)
                .Foreach(yetToProcessNodes.Enqueue);

            pathsFromProcedures.Add(node);
        }

        pathsFromProcedures.Reverse();
        return pathsFromProcedures.AsReadOnly();
    }

    private void UpdateExecutionNode(Node node) {
        var evaluationNode = GetExecutionNode(node);
        if (evaluationNode.ProcessedThisFrame) {
            return;
        }

        evaluationNode.ProcessedThisFrame = true;

        Debug.Assert.True(evaluationNode.ArgValues.Length == node.Function.Args.Count);

        evaluationNode.ArgumentsUpdatedThisFrame =
            evaluationNode.ArgDependencies
                .SkipNulls()
                .Any(dependency => GetExecutionNode(dependency.TargetNode).ArgumentsUpdatedThisFrame
                                   || dependency.TargetNode.Behavior == FunctionBehavior.Proactive
                );
    }

    private IReadOnlyList<EvaluationNode> GetInvocationList() {
        Queue<EvaluationNode> yetToProcessENodes = new();
        ExecutionGraph.EvaluationNodes
            .Where(_ => _.Node.Function.IsProcedure)
            .Foreach(yetToProcessENodes.Enqueue);

        List<EvaluationNode> invocationList = new();

        while (yetToProcessENodes.Count > 0) {
            var enode = yetToProcessENodes.Dequeue();
            invocationList.Add(enode);

            foreach (var dependency in enode.ArgDependencies) {
                EvaluationNode en = GetExecutionNode(dependency.TargetNode);

                if (!en.HasOutputValues) {
                    yetToProcessENodes.Enqueue(en);
                    continue;
                }

                //exactly now and not earlier
                if (dependency.Behavior == ConnectionBehavior.Once) {
                    continue;
                }

                if (en.ArgumentsUpdatedThisFrame ||
                    en.Behavior == FunctionBehavior.Proactive) {
                    yetToProcessENodes.Enqueue(en);
                }
            }
        }

        invocationList.Reverse();
        return invocationList;
    }
}