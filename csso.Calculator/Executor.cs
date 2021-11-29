using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using csso.Common;
using csso.NodeCore;

namespace csso.Calculator {
internal static class Xtensions {
    public static Int32? FirstIndexOf<T>(this IEnumerable<T> enumerable, T element) {
        Int32 i = 0;
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
    internal class ExecutionContext {
        public Queue<Node> YetToProcess { get; } = new();
        public List<Node> Order { get; } = new();

        public List<EvaluationNode> EvaluatedNodes { get; } = new();

        public EvaluationNode? GetEvaluated(Node node) {
            return EvaluatedNodes.SingleOrDefault(_ => _.Node == node);
        }
    }

    internal class EvaluationNode {
        public Node Node { get; }
        public Object?[] ArgValues { get; }
        public Func<Object?>?[] ArgFunctions { get; private set; }

        public Int32 ArgCount { get; }
        public bool Updated { get; set; } = false;
        public bool Invoked { get; set; } = false;

        private class NotFound { }

        private static readonly Object Empty = new NotFound();

        public EvaluationNode(Node node) {
            Node = node;
            ArgCount = node.Function.Args.Count;

            ArgValues = Enumerable.Repeat(Empty, ArgCount).ToArray();
            ArgFunctions = Enumerable.Repeat<Func<Object?>?>(null, ArgCount).ToArray();
        }

        public object? GetOutputValue(FunctionOutput outputConnectionOutput) {
            if (!Invoked)
                Invoke();

            Int32? index = Node.Function.Args.FirstIndexOf(outputConnectionOutput);
            if (index == null)
                throw new Exception("unerty i");
            else
                return ArgValues[index.Value];
        }

        public void Invoke() {
            if (Invoked) return;

            for (int i = 0; i < ArgCount; i++)
                if (ArgFunctions[i] != null)
                    ArgValues[i] = ArgFunctions[i]!.Invoke();

            for (int i = 0; i < ArgCount; i++)
                if (Node.Function.Args[i].ArgType == ArgType.Out)
                    ArgValues[i] = Activator.CreateInstance(Node.Function.Args[i].Type);

            Check.True(ArgValues.All(_ => _ != Empty));

            Node.Function.Invoke(ArgValues.Length == 0 ? null : ArgValues);

            Invoked = true;
            Updated = true;
        }
    }

    private Int32 _frameNo = 0;

    public Graph Graph { get; }

    public Executor(Graph graph) {
        Graph = graph;

        FrameNoFunction = new(
            "Frame number",
            ([Output] ref Int32 frameNumber) => {
                frameNumber = _frameNo;
                return true;
            }
        );
        DeltaTimeFunction = new(
            "Frame number",
            ([Output] ref Double deltaTime) => {
                deltaTime = 0.1555f;
                return true;
            }
        );
    }


    public Function FrameNoFunction { get; }

    public Function DeltaTimeFunction { get; }

    private ExecutionContext? _previousContext;


    public void Reset() {
        _previousContext = null;
    }

    public void Run() {
        var rootNodes = Graph.Nodes
            .Where(_ => _.Function.IsProcedure);

        ExecutionContext context = new();
        rootNodes.Foreach(context.YetToProcess.Enqueue);

        while (context.YetToProcess.Count > 0) {
            Node node = context.YetToProcess.Dequeue();

            foreach (var c in node.Connections)
                if (c is OutputConnection connection)
                    context.YetToProcess.Enqueue(connection.OutputNode);

            context.Order.Add(node);
        }

        context.Order.Reverse();

        foreach (var node in context.Order) {
            if (context.GetEvaluated(node) != null) continue;

            EvaluationNode? evaluationNode = null;

            if (node.Behavior == FunctionBehavior.Reactive) {
                bool anyProactiveConnection = node.Connections.Any(_ => {
                    return _.Behavior == FunctionBehavior.Proactive;
                });

                if (!anyProactiveConnection)
                    evaluationNode = _previousContext?.GetEvaluated(node);
            }

            if (evaluationNode == null) {
                evaluationNode = new(node);
                evaluationNode.Updated = true;

                for (int i = 0; i < evaluationNode.ArgCount; i++) {
                    FunctionArg arg = node.Function.Args[i];

                    if (arg is FunctionInput input) {
                        Connection? connection = node.Connections.SingleOrDefault(_ => _.Input == input);
                        if (connection == null) {
                            System.Diagnostics.Debug.WriteLine("asfdsdfgweg");
                            return;
                        }

                        if (connection is ValueConnection valueConnection)
                            evaluationNode.ArgValues[i] = valueConnection.Value;

                        if (connection is OutputConnection outputConnection) {
                            EvaluationNode? evaluatedNode = null;
                            if (outputConnection.Behavior == FunctionBehavior.Reactive)
                                evaluatedNode = _previousContext?.GetEvaluated(outputConnection.OutputNode);

                            evaluatedNode ??= context.GetEvaluated(outputConnection.OutputNode);

                            if (evaluatedNode != null)
                                evaluationNode.ArgFunctions[i] = new Func<object?>(() => {
                                    // TODO flatten recursion
                                    return evaluatedNode.GetOutputValue(outputConnection.Output);
                                });
                        }
                    } else if (arg is FunctionConfig config)
                        evaluationNode.ArgValues[i] = config.Value;
                }

                // enode.Invoke();
            }

            Debug.Assert.True(evaluationNode.ArgValues.Length == node.Function.Args.Count);

            context.EvaluatedNodes.Add(evaluationNode);
        }

        context.EvaluatedNodes
            .Where(_ => _.Node.Function.IsProcedure)
            .Foreach(_ => _.Invoke());

        ++_frameNo;
        _previousContext = context;
    }
}
}