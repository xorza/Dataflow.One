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
        public List<EvaluationNode> EvaluationNodes { get; } = new();

        public EvaluationNode? GetEvaluated(Node node) {
            return EvaluationNodes.SingleOrDefault(_ => _.Node == node);
        }
    }

    internal struct Dependency {
        public EvaluationNode Node { get; }
        public FunctionOutput Output { get; }

        public FunctionBehavior Behavior { get; }

        public Dependency(EvaluationNode evaluationNode, FunctionOutput output, FunctionBehavior behavior) {
            Node = evaluationNode;
            Output = output;
            Behavior = behavior;
        }
    }

    internal class EvaluationNode {
        public Node Node { get; }
        public Object?[] ArgValues { get; }

        public Dependency?[] ArgDependencies { get; }

        public Int32 ArgCount { get; }
        public bool Invoked { get; set; } = false;
        public bool UpdatedThisFrame { get; set; } = false;
        public bool ProcessedThisFrame { get; set; } = false;

        private class NotFound { }

        private static readonly Object Empty = new NotFound();

        public EvaluationNode(ExecutionContext context, Node node) {
            Node = node;
            ArgCount = node.Function.Args.Count;

            ArgValues = Enumerable.Repeat(Empty, ArgCount).ToArray();
            ArgDependencies = Enumerable.Repeat((Dependency?) null, ArgCount).ToArray();

            Invoked = false;
            UpdatedThisFrame = true;
            ProcessedThisFrame = true;

            for (int i = 0; i < ArgCount; i++) {
                FunctionArg arg = node.Function.Args[i];

                if (arg is FunctionInput input) {
                    Connection? connection = node.Connections.SingleOrDefault(_ => _.Input == input);
                    if (connection == null) {
                        System.Diagnostics.Debug.WriteLine("asfdsdfgweg");
                        return;
                    }

                    if (connection is ValueConnection valueConnection)
                        ArgValues[i] = valueConnection.Value;

                    if (connection is OutputConnection outputConnection) {
                        EvaluationNode? dependencyNode = context.GetEvaluated(outputConnection.OutputNode);

                        if (dependencyNode == null) {
                            return;
                        }

                        if (dependencyNode.Invoked) {
                            ArgValues[i] = dependencyNode.GetOutputValue(outputConnection.Output);
                        }

                        ArgDependencies[i] = new Dependency(dependencyNode, outputConnection.Output,
                            outputConnection.FinalBehavior);
                    }
                } else if (arg is FunctionConfig config)
                    ArgValues[i] = config.Value;
            }
        }

        public object? GetOutputValue(FunctionOutput outputConnectionOutput) {
            Check.True(Invoked);

            Int32? index = Node.Function.Args.FirstIndexOf(outputConnectionOutput);
            if (index == null)
                throw new Exception("unerty i");
            else
                return ArgValues[index.Value];
        }

        public void Invoke() {
            if (Invoked) return;

            for (int i = 0; i < ArgCount; i++) {
                switch (Node.Function.Args[i].ArgType) {
                    case ArgType.Config:
                        break;
                    case ArgType.Out:
                        ArgValues[i] = Pool(Node.Function.Args[i].Type);
                        break;
                    case ArgType.In:
                        if (ArgValues[i] == Empty || ArgDependencies[i] != null) {
                            Check.True(ArgDependencies[i]!.Value.Node.Invoked);

                            ArgValues[i] = ArgDependencies[i]!.Value.Node
                                .GetOutputValue(ArgDependencies[i]!.Value.Output);
                        }

                        break;

                    default:
                        Debug.Assert.False();
                        return;
                }
            }

            Check.True(ArgValues.All(_ => _ != Empty));

            Node.Function.Invoke(ArgValues.Length == 0 ? null : ArgValues);

            Invoked = true;
        }

        private Object Pool(Type type) {
            return Activator.CreateInstance(type)!;
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

    private ExecutionContext context = new ExecutionContext();


    public void Reset() {
        context = new ExecutionContext();
    }

    public void Run() {
        Queue<Node> yetToProcessNodes = new();
        List<Node> pathsFromProcedures = new();

        context.EvaluationNodes.Foreach(_ => _.ProcessedThisFrame = false);

        var procedures = Graph.Nodes
            .Where(_ => _.Function.IsProcedure
                        && _.FinalBehavior == FunctionBehavior.Proactive)
            .ToArray();

        procedures.Foreach(yetToProcessNodes.Enqueue);

        while (yetToProcessNodes.Count > 0) {
            Node node = yetToProcessNodes.Dequeue();

            foreach (var c in node.Connections)
                if (c is OutputConnection connection)
                    yetToProcessNodes.Enqueue(connection.OutputNode);

            pathsFromProcedures.Add(node);
        }

        pathsFromProcedures.Reverse();

        foreach (var node in pathsFromProcedures) {
            if (context.GetEvaluated(node)?.ProcessedThisFrame ?? false)
                continue;

            EvaluationNode? evaluationNode = context.GetEvaluated(node);
            if (evaluationNode != null) {
                evaluationNode.UpdatedThisFrame = false;
                if (evaluationNode.Node.Function.IsProcedure) {
                    evaluationNode.Invoked = false;
                }
            } else {
                evaluationNode = new(context, node);
                context.EvaluationNodes.Add(evaluationNode);
            }

            Debug.Assert.True(evaluationNode.ArgValues.Length == node.Function.Args.Count);

            evaluationNode.ProcessedThisFrame = true;
        }

        Queue<EvaluationNode> yetToProcessENodes = new();
        context.EvaluationNodes
            .Where(_ => _.Node.Function.IsProcedure)
            .Foreach(yetToProcessENodes.Enqueue);


        context.EvaluationNodes.Clear();

        while (yetToProcessENodes.Count > 0) {
            EvaluationNode enode = yetToProcessENodes.Dequeue();
            context.EvaluationNodes.Add(enode);

            for (int i = 0; i < enode.ArgCount; i++) {
                Dependency? dependency = enode.ArgDependencies[i];
                if (dependency == null)
                    continue;

                if (dependency.Value.Behavior == FunctionBehavior.Reactive
                    && dependency.Value.Node.Invoked) {
                    dependency.Value.Node.ArgValues[i] = dependency.Value.Node.GetOutputValue(dependency.Value.Output);
                } else {
                    yetToProcessENodes.Enqueue(dependency.Value.Node);
                    dependency.Value.Node.Invoked = false;
                }
            }
        }

        context.EvaluationNodes.Reverse();
        context.EvaluationNodes.Foreach(_ => _.Invoke());

        ++_frameNo;
    }
}
}