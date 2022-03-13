using System.Collections.Immutable;
using System.Runtime.Serialization;
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
    public ExecutionGraph ExecutionGraph { get; }

    public EvaluationNode GetEvaluationNode(Node node) {
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

        MarkActive();

        var invocationList = BuildInvocationList();
        invocationList.Foreach(evaluationNode => evaluationNode.Invoke(this));

        ++FrameNo;
    }

    private void MarkActive() {
        Queue<Node> yetToProcessNodes = new();
        Graph.Nodes
            .Where(_ => _.Function.IsProcedure)
            .Foreach(yetToProcessNodes.Enqueue);

        Stack<Node> paths = new();
        while (yetToProcessNodes.TryDequeue(out var node)) {
            node.Connections
                .OfType<BindingConnection>()
                .Select(_ => _.TargetNode)
                .Foreach(yetToProcessNodes.Enqueue);

            paths.Push(node);
        }


        paths.Foreach(UpdateEvaluationNode);
    }


    private void UpdateEvaluationNode(Node node) {
        var evaluationNode = GetEvaluationNode(node);
        if (evaluationNode.UpdatedThisFrame) {
            return;
        }

        foreach (var config in evaluationNode.Node.ConfigValues) {
            if (evaluationNode.ArgValues[config.Config.ArgumentIndex] != config.Value) {
                evaluationNode.Update(true);
                return;
            }
        }

        foreach (var dependency in evaluationNode.ArgDependencies) {
            if (dependency.TargetNode.Behavior == FunctionBehavior.Proactive) {
                evaluationNode.Update(true);
                return;
            }

            var targetEvaluationNode = GetEvaluationNode(dependency.TargetNode);
            Debug.Assert.True(targetEvaluationNode.UpdatedThisFrame);
            
            if (targetEvaluationNode.ArgumentsUpdatedThisFrame) {
                evaluationNode.Update(true);
                return;
            }
        }

        evaluationNode.Update(false);
    }

    private Stack<EvaluationNode> BuildInvocationList() {
        Queue<EvaluationNode> yetToProcessENodes = new();
        ExecutionGraph.EvaluationNodes
            .Where(_ => _.Node.Function.IsProcedure)
            .Foreach(yetToProcessENodes.Enqueue);

        Stack<EvaluationNode> invocationList = new();

        while (yetToProcessENodes.TryDequeue(out var enode)) {
            invocationList.Push(enode);

            foreach (var dependency in enode.ArgDependencies) {
                EvaluationNode en = GetEvaluationNode(dependency.TargetNode);

                if (!en.HasOutputValues) {
                    yetToProcessENodes.Enqueue(en);
                    continue;
                }

                if (dependency.Behavior == ConnectionBehavior.Once) {
                    continue;
                }

                if (en.ArgumentsUpdatedThisFrame
                    || en.Behavior == FunctionBehavior.Proactive) {
                    yetToProcessENodes.Enqueue(en);
                }
            }
        }

        return invocationList;
    }
}