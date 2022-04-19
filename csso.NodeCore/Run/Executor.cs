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

        var activatedNodes = ProcessEvents();

        ProcessEvaluationNodes(activatedNodes);

        var invocationList = BuildInvocationList(activatedNodes);
        invocationList.ForEach(evaluationNode => evaluationNode.ProcessArguments());
        invocationList.ForEach(evaluationNode => evaluationNode.Invoke(this));

        ++FrameNo;
    }

    private List<Node> ProcessEvents() {
        List<Node> result = new();

        Graph.GetFiredEvents()
            .SelectMany(_ => Graph.GetSubscribers(_))
            .ForEach(result.Add);

        return result;
    }

    private void Recompile() {
        List<EvaluationNode> newEvaluationNodes = new(Graph.Nodes.Count);

        foreach (var node in Graph.Nodes) {
            if (node is FunctionNode functionNode) {
                var existing = EvaluationNodes.SingleOrDefault(_ => _.Node == functionNode);
                newEvaluationNodes.Add(existing ?? new EvaluationNode(functionNode));
            } else {
                throw new NotImplementedException("wv435ty5yt ");
            }
        }

        ValidateNodeOrder(Graph, newEvaluationNodes);

        EvaluationNodes = newEvaluationNodes;
        Xtentions.ForEach(EvaluationNodes, _ => _.Reset());
    }

    [Conditional("DEBUG")]
    private static void ValidateNodeOrder(Graph graph, IList<EvaluationNode> evaluationNodes) {
        Debug.Assert.True(evaluationNodes.Count == graph.Nodes.Count);

        for (var i = 0; i < evaluationNodes.Count; i++) {
            Debug.Assert.AreSame(evaluationNodes[i].Node, graph.Nodes[i]);
        }
    }

    private void ProcessEvaluationNodes(List<Node> activatedNodes) {
        Queue<Node> yetToProcessNodes = new();
        activatedNodes
            .ForEach(yetToProcessNodes.Enqueue);

        Stack<Node> paths = new();
        while (yetToProcessNodes.TryDequeue(out var node)) {
            Graph.GetDataSubscriptions(node)
                .Select(_ => _.Source.Node)
                .ForEach(yetToProcessNodes.Enqueue);

            paths.Push(node);
        }

        paths.ForEach(UpdateEvaluationNode);
    }

    private void UpdateEvaluationNode(Node node) {
        var evaluationNode = GetEvaluationNode(node);
        if (evaluationNode.State >= EvaluationState.Processed) {
            return;
        }

        foreach (var binding in Graph.GetDataSubscriptions(evaluationNode.Node)) {
            if (binding.Source.Node.Behavior == FunctionBehavior.Proactive) {
                evaluationNode.Process(true);
                return;
            }

            var targetEvaluationNode = GetEvaluationNode(binding.Source.Node);
            Check.True(targetEvaluationNode.State >= EvaluationState.Processed);

            if (targetEvaluationNode.ArgumentsUpdatedThisFrame) {
                evaluationNode.Process(true);
                return;
            }
        }

        evaluationNode.Process(false);
    }

    private Stack<EvaluationNode> BuildInvocationList(List<Node> activatedNodes) {
        Queue<EvaluationNode> yetToProcessENodes = new();
        EvaluationNodes
            .Where(_ => activatedNodes.Contains(_.Node))
            .ForEach(yetToProcessENodes.Enqueue);

        Stack<EvaluationNode> invocationList = new();

        while (yetToProcessENodes.TryDequeue(out var evaluationNode)) {
            invocationList.Push(evaluationNode);

            foreach (var binding in Graph.GetDataSubscriptions(evaluationNode.Node)) {
                var targetEvaluationNode = GetEvaluationNode(binding.Source.Node);

                if (!targetEvaluationNode.HasOutputValues) {
                    yetToProcessENodes.Enqueue(targetEvaluationNode);
                    continue;
                }

                if (binding.Behavior == SubscriptionBehavior.Once) continue;

                if (targetEvaluationNode.ArgumentsUpdatedThisFrame
                    || targetEvaluationNode.Behavior == FunctionBehavior.Proactive)
                    yetToProcessENodes.Enqueue(targetEvaluationNode);
            }
        }

        return invocationList;
    }
}