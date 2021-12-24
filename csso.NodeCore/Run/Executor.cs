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

    public List<ExecutionNode> EvaluationNodes { get; }

    public ExecutionNode GetExecutionNode(Node node) {
        return EvaluationNodes.Single(_ => _.Node == node);;
    }

    public Graph Graph { get; }

    internal Executor(Graph graph) {
        Graph = graph;
        FrameNo = 0;

        EvaluationNodes =
            Graph.Nodes
                .Select(n => new ExecutionNode(n))
                .ToList();
    }
    
    public void Run() {
        EvaluationNodes.Foreach(_ => _.NextIteration());

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
                .OfType<OutputConnection>()
                .Select(_ => _.OutputNode)
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
                .Any(dependency => GetExecutionNode(dependency.OutputNode).ArgumentsUpdatedThisFrame
                                   || dependency.OutputNode.Behavior == FunctionBehavior.Proactive
                );
    }

    private IReadOnlyList<ExecutionNode> GetInvocationList() {
        Queue<ExecutionNode> yetToProcessENodes = new();
        EvaluationNodes
            .Where(_ => _.Node.Function.IsProcedure)
            .Foreach(yetToProcessENodes.Enqueue);

        List<ExecutionNode> invocationList = new();

        while (yetToProcessENodes.Count > 0) {
            var enode = yetToProcessENodes.Dequeue();
            invocationList.Add(enode);

            foreach (var dependency in enode.ArgDependencies) {
                if (dependency == null) {
                    continue;
                }

                ExecutionNode en = GetExecutionNode(dependency.OutputNode);

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