using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using csso.Common;
using Debug = csso.Common.Debug;

namespace csso.NodeCore.Run;


public class Executor  {
    private readonly List<EvaluationNode> _evaluationNodes = new();

    public Executor(Graph graph) {
        Graph = graph;
        FrameNo = 0;
    }

    public int FrameNo { get; private set; }
    public IReadOnlyList<EvaluationNode> EvaluationNodes => _evaluationNodes.AsReadOnly();

    public Graph Graph { get; }


    public EvaluationNode GetEvaluationNode(Node node) {
        var result = EvaluationNodes.SingleOrDefault(_ => _.Node == node);

        if (result == null) {
            result = new EvaluationNode(node);
            _evaluationNodes.Add(result);
        }

        return result;
    }

    public void Run() {
        Recompile();

        var activatedNodes = ProcessEvents();

        ProcessEvaluationNodes(activatedNodes);

        var invocationList = BuildInvocationList(activatedNodes);
        invocationList.ForEach(evaluationNode => evaluationNode.PrepareArguments());
        invocationList.ForEach(evaluationNode => evaluationNode.Invoke(this));

        ++FrameNo;
    }

    private List<Node> ProcessEvents() {
        var result = Graph.ProcessFiredEvents()
            .SelectMany(_ => Graph.GetSubscribers(_))
            .ToList();

        Graph.Nodes.Where(_ => _.Outputs.Count == 0)
            .ForEach(result.Add);

        return result;
    }

    private void Recompile() {
        List<EvaluationNode> newEvaluationNodes = new(Graph.Nodes.Count);

        foreach (var node in Graph.Nodes) {
            if (node is FunctionNode functionNode) {
                var existing = EvaluationNodes.SingleOrDefault(_ => _.Node == functionNode);
                existing ??= new EvaluationNode(functionNode);
                newEvaluationNodes.Add(existing);
            } else {
                throw new NotImplementedException("wv435ty5yt ");
            }
        }

        ValidateNodeOrder(Graph, newEvaluationNodes);

        _evaluationNodes.Clear();
        _evaluationNodes.AddRange(newEvaluationNodes);
        _evaluationNodes.ForEach(_ => _.Reset());
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
        if (evaluationNode.State == EvaluationState.Processed) {
            return;
        }

        Check.True(evaluationNode.State < EvaluationState.ArgumentsSet);

        foreach (var binding in Graph.GetDataSubscriptions(evaluationNode.Node)) {
            if (binding.Source.Node.Behavior == FunctionBehavior.Proactive) {
                evaluationNode.Process(true);
                return;
            }

            var targetEvaluationNode = GetEvaluationNode(binding.Source.Node);
            Check.True(targetEvaluationNode.State == EvaluationState.Processed);

            if (targetEvaluationNode.ShouldInvokeThisFrame) {
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

                if (binding.Behavior == SubscriptionBehavior.Once) {
                    continue;
                }

                if (targetEvaluationNode.ShouldInvokeThisFrame
                    || targetEvaluationNode.Behavior == FunctionBehavior.Proactive) {
                    yetToProcessENodes.Enqueue(targetEvaluationNode);
                }
            }
        }

        return invocationList;
    }
}