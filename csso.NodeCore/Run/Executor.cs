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
    public Int32 FrameNo { get; private set; } = 0;

    public List<ExecutionNode> EvaluationNodes { get; set; } = new();

    public ExecutionNode? GetEvaluated(Node node) {
        var result = EvaluationNodes
            .SingleOrDefault(_ => _.Node == node);

        return result;
    }

    public Graph Graph { get; }

    internal Executor(Graph graph) {
        Graph = graph;
    }


    public void Reset() {
        FrameNo = 0;

        EvaluationNodes.Clear();

        var activeEvaluationNodes = Graph.Nodes
            .Select(n => {
                ExecutionNode en = GetEvaluated(n)
                                   ?? new(n);
                return en;
            })
            .SkipNulls()
            .ToList();

        EvaluationNodes = activeEvaluationNodes;
    }

    public void Run() {
        if (FrameNo == 0)
            Reset();

        EvaluationNodes.Foreach(_ => _.Refresh(this));

        var pathsFromProcedures = GetPathsToProcedures(Graph);
        pathsFromProcedures.Foreach(UpdateEvaluationNode);

        var invokationList =
            GetInvocationList()
                .Distinct();
        invokationList.Foreach(_ => _.Invoke());

        ++FrameNo;
    }

    private IReadOnlyList<Node> GetPathsToProcedures(Graph graph) {
        List<Node> pathsFromProcedures = new();

        Queue<Node> yetToProcessNodes = new();
        graph.Nodes
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
        var evaluationNode = GetEvaluated(node)!;
        if (evaluationNode.ProcessedThisFrame)
            return;

        evaluationNode.ProcessedThisFrame = true;

        Debug.Assert.True(evaluationNode.ArgValues.Length == node.Function.Args.Count);

        evaluationNode.ArgumentsUpdatedThisFrame =
            evaluationNode.ArgDependencies
                .SkipNulls()
                .Any(dependency => dependency!.Node.ArgumentsUpdatedThisFrame
                                   || dependency!.Node.Behavior == FunctionBehavior.Proactive);
    }

    private IReadOnlyList<ExecutionNode> GetInvocationList() {
        Queue<ExecutionNode> yetToProcessENodes = new();
        EvaluationNodes
            .Where(_ => _.Node.Function.IsProcedure)
            .Foreach(yetToProcessENodes.Enqueue);

        List<ExecutionNode> invokationList = new();

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
}