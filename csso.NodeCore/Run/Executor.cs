using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.Serialization;
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
    public Int32 FrameNo { get; private set; }

    public List<EvaluationNode> EvaluationNodes { get; private set; } = new ();

    public EvaluationNode GetEvaluationNode(Node node) {
        return EvaluationNodes.Single(_ => _.Node == node);
    }

    public Graph Graph { get; }

    public Executor(Graph graph) {
        Graph = graph;
        FrameNo = 0;
        Recompile();
    }

    public void Recompile() {
        List<EvaluationNode> newEvaluationNodes = new(Graph.Nodes.Count);

        for (int i = 0; i < Graph.Nodes.Count; i++) {
            EvaluationNode? existing = EvaluationNodes.SingleOrDefault(_ => _.Node == Graph.Nodes[i]);

            if (existing != null) {
                existing.Reset();
                newEvaluationNodes.Add(existing);
            } else {
                newEvaluationNodes.Add(new EvaluationNode(Graph.Nodes[i]));
            }
        }

        EvaluationNodes = newEvaluationNodes;

        ValidateNodeOrder();
    }  
    
    [Conditional("DEBUG")]
    private void ValidateNodeOrder() {
        Debug.Assert.True(EvaluationNodes.Count == Graph.Nodes.Count);

        for (int i = 0; i < EvaluationNodes.Count; i++) {
            Debug.Assert.AreSame(EvaluationNodes[i].Node, Graph.Nodes[i]);
        }
    }
    
    public void Run() {
        EvaluationNodes.Foreach(_ => _.NextIteration());

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
        EvaluationNodes
            .Where(_ => _.Node.Function.IsProcedure)
            .Foreach(yetToProcessENodes.Enqueue);

        Stack<EvaluationNode> invocationList = new();

        while (yetToProcessENodes.TryDequeue(out var evaluationNode)) {
            invocationList.Push(evaluationNode);

            foreach (var dependency in evaluationNode.ArgDependencies) {
                EvaluationNode targetEvaluationNode = GetEvaluationNode(dependency.TargetNode);

                if (!targetEvaluationNode.HasOutputValues) {
                    yetToProcessENodes.Enqueue(targetEvaluationNode);
                    continue;
                }

                if (dependency.Behavior == ConnectionBehavior.Once) {
                    continue;
                }

                if (targetEvaluationNode.ArgumentsUpdatedThisFrame
                    || targetEvaluationNode.Behavior == FunctionBehavior.Proactive) {
                    yetToProcessENodes.Enqueue(targetEvaluationNode);
                }
            }
        }

        return invocationList;
    }
}