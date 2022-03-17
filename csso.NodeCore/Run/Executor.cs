using System.Diagnostics;
using csso.Common;
using Debug = csso.Common.Debug;

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
    public Executor(Graph graph) {
        Graph = graph;
        FrameNo = 0;
        Recompile();
    }

    public Int32 FrameNo { get; private set; }

    public List<EvaluationNode> EvaluationNodes { get; private set; } = new();

    public Graph Graph { get; }

    public EvaluationNode GetEvaluationNode(Node node) {
        return EvaluationNodes.Single(_ => _.Node == node);
    }

    public bool TryGetEvaluationNode(Node node, out EvaluationNode? evaluationNode) {
        var result = EvaluationNodes.SingleOrDefault(_ => _.Node == node);

        evaluationNode = result;
        return result != null;
    }

    public void Run() {
        Recompile();

        ProcessEvaluationNodes();

        var invocationList = BuildInvocationList();
        invocationList.Foreach(evaluationNode => evaluationNode.ProcessArguments());
        invocationList.Foreach(evaluationNode => evaluationNode.Invoke(this));

        ++FrameNo;
    }

    private void Recompile() {
        List<EvaluationNode> newEvaluationNodes = new(Graph.Nodes.Count);

        foreach (var node in Graph.Nodes) {
            var existing = EvaluationNodes.SingleOrDefault(_ => _.Node == node);
            newEvaluationNodes.Add(existing ?? new EvaluationNode(node));
        }

        ValidateNodeOrder(Graph, newEvaluationNodes);

        EvaluationNodes = newEvaluationNodes;
        EvaluationNodes.Foreach(_ => _.Reset());
    }

    [Conditional("DEBUG")]
    private static void ValidateNodeOrder(Graph graph, IList<EvaluationNode> evaluationNodes) {
        Debug.Assert.True(evaluationNodes.Count == graph.Nodes.Count);

        for (var i = 0; i < evaluationNodes.Count; i++) Debug.Assert.AreSame(evaluationNodes[i].Node, graph.Nodes[i]);
    }

    private void ProcessEvaluationNodes() {
        Queue<Node> yetToProcessNodes = new();
        Graph.Nodes
            .Where(_ => _.Function.IsProcedure)
            .Foreach(yetToProcessNodes.Enqueue);

        Stack<Node> paths = new();
        while (yetToProcessNodes.TryDequeue(out var node)) {
            node.BindingConnections
                .Select(_ => _.TargetNode)
                .Foreach(yetToProcessNodes.Enqueue);

            paths.Push(node);
        }

        paths.Foreach(UpdateEvaluationNode);
    }

    private void UpdateEvaluationNode(Node node) {
        var evaluationNode = GetEvaluationNode(node);
        if (evaluationNode.State >= EvaluationState.Processed) return;

        foreach (var config in evaluationNode.Node.ConfigValues)
            if (evaluationNode.ArgValues[config.Config.ArgumentIndex] != config.Value) {
                evaluationNode.Process(true);
                return;
            }

        foreach (var binding in evaluationNode.Node.BindingConnections) {
            if (binding.TargetNode.Behavior == FunctionBehavior.Proactive) {
                evaluationNode.Process(true);
                return;
            }

            var targetEvaluationNode = GetEvaluationNode(binding.TargetNode);
            Check.True(targetEvaluationNode.State >= EvaluationState.Processed);

            if (targetEvaluationNode.ArgumentsUpdatedThisFrame) {
                evaluationNode.Process(true);
                return;
            }
        }

        evaluationNode.Process(false);
    }

    private Stack<EvaluationNode> BuildInvocationList() {
        Queue<EvaluationNode> yetToProcessENodes = new();
        EvaluationNodes
            .Where(_ => _.Node.Function.IsProcedure)
            .Foreach(yetToProcessENodes.Enqueue);

        Stack<EvaluationNode> invocationList = new();

        while (yetToProcessENodes.TryDequeue(out var evaluationNode)) {
            invocationList.Push(evaluationNode);

            foreach (var binding in evaluationNode.Node.BindingConnections) {
                var targetEvaluationNode = GetEvaluationNode(binding.TargetNode);

                if (!targetEvaluationNode.HasOutputValues) {
                    yetToProcessENodes.Enqueue(targetEvaluationNode);
                    continue;
                }

                if (binding.Behavior == ConnectionBehavior.Once) continue;

                if (targetEvaluationNode.ArgumentsUpdatedThisFrame
                    || targetEvaluationNode.Behavior == FunctionBehavior.Proactive)
                    yetToProcessENodes.Enqueue(targetEvaluationNode);
            }
        }

        return invocationList;
    }
}