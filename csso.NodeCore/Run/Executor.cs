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


        paths.Foreach(UpdateExecutionNode);
    }


    private void UpdateExecutionNode(Node node) {
        var evaluationNode = GetExecutionNode(node);
        if (evaluationNode.UpdatedThisFrame) {
            return;
        }

        foreach (var config in evaluationNode.Node.ConfigValues) {
            if (evaluationNode.ArgValues[config.Config.ArgumentIndex] != config.Value) {
                evaluationNode.Update(true);
                return;
            }
        }

        var hasUpdatedDependencies = evaluationNode.ArgDependencies
            .Any(dependency => {
                return
                    dependency.TargetNode.Behavior == FunctionBehavior.Proactive
                    || GetExecutionNode(dependency.TargetNode).ArgumentsUpdatedThisFrame;
            });

        evaluationNode.Update(hasUpdatedDependencies);
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
                EvaluationNode en = GetExecutionNode(dependency.TargetNode);

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