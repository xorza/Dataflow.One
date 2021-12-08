using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
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

        public EvaluationNode GetEvaluated(Node node) {
            EvaluationNode? result = EvaluationNodes.SingleOrDefault(_ => _.Node == node);

            if (result == null) {
                result = new(this, node);
                EvaluationNodes.Add(result);
            }

            return result;
        }
    }

    internal class Dependency {
        public EvaluationNode Node { get; }
        public FunctionOutput Output { get; }

        public ConnectionBehavior Behavior { get; }

        public Dependency(EvaluationNode evaluationNode, FunctionOutput output, ConnectionBehavior behavior) {
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
        public bool HasOutputValues { get; private set; } = false;
        public FunctionBehavior Behavior { get; } = FunctionBehavior.Proactive;

        public bool ArgumentsUpdatedThisFrame { get; set; } = false;
        public bool ProcessedThisFrame { get; set; } = false;


        private class NotFound { }

        private static readonly Object Empty = new NotFound();

        public EvaluationNode(ExecutionContext context, Node node) {
            Node = node;
            ArgCount = node.Function.Args.Count;

            ArgValues = Enumerable.Repeat(Empty, ArgCount).ToArray();
            ArgDependencies = Enumerable.Repeat((Dependency?) null, ArgCount).ToArray();

            Behavior = node.FinalBehavior;

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

                        if (dependencyNode == null)
                            throw new Exception("setsdfsdf");

                        if (dependencyNode.Behavior == FunctionBehavior.Proactive)
                            Behavior = FunctionBehavior.Proactive;

                        ArgDependencies[i] = new Dependency(
                            dependencyNode,
                            outputConnection.Output,
                            outputConnection.Behavior
                        );
                    }
                } else if (arg is FunctionConfig config) {
                    ConfigValue value = node.ConfigValues
                        .Single(_ => _.Config == config);
                    ArgValues[i] = value.Value;
                }
            }
        }

        public object? GetOutputValue(FunctionOutput outputConnectionOutput) {
            Check.True(HasOutputValues);

            Int32? index = Node.Function.Args.FirstIndexOf(outputConnectionOutput);
            if (index == null)
                throw new Exception("unerty i");
            else
                return ArgValues[index.Value];
        }

        public void Invoke() {
            if (HasOutputValues
                && !ArgumentsUpdatedThisFrame
                && Behavior != FunctionBehavior.Proactive)
                throw new Exception("rb56u etdg");
            if (!ProcessedThisFrame)
                throw new Exception("veldkfgyuivhnwo4875");

            for (int i = 0; i < ArgCount; i++) {
                switch (Node.Function.Args[i].ArgType) {
                    case ArgType.Config:
                        break;
                    case ArgType.Out:
                        ArgValues[i] = Pool(Node.Function.Args[i].Type);
                        break;
                    case ArgType.In:
                        if (ArgDependencies[i] != null) {
                            Check.True(ArgDependencies[i]!.Node.HasOutputValues);
                            ArgValues[i] = ArgDependencies[i]!.Node
                                .GetOutputValue(ArgDependencies[i]!.Output);
                        }

                        if (ArgValues[i] == Empty) {
                            throw new Exception("dfgsdfhsfgh");
                        }

                        break;

                    default:
                        Debug.Assert.False();
                        return;
                }
            }

            Node.Function.Invoke(ArgValues.Length == 0 ? null : ArgValues);

            if (!Node.Function.IsProcedure)
                HasOutputValues = true;
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

    private ExecutionContext _context = new ExecutionContext();


    public void Reset() {
        _context = new ExecutionContext();
        _frameNo = 0;
    }

    public void Run() {
        _context.EvaluationNodes.Foreach(_ => {
            _.ProcessedThisFrame = false;
            _.ArgumentsUpdatedThisFrame = false;
        });

        var pathsFromProcedures = GetPathsToProcedures();
        pathsFromProcedures.Foreach(UpdateEvaluationNode);

        var invokationList = GetInvokationList();
        invokationList.Foreach(_ => _.Invoke());

        ++_frameNo;
    }

    private IReadOnlyList<Node> GetPathsToProcedures() {
        List<Node> pathsFromProcedures = new();

        Queue<Node> yetToProcessNodes = new();
        Graph.Nodes
            .Where(_ => _.Function.IsProcedure)
            .Foreach(yetToProcessNodes.Enqueue);

        while (yetToProcessNodes.TryDequeue(out Node? node)) {
            node.Connections
                .OfType<OutputConnection>()
                .Foreach(_ => yetToProcessNodes.Enqueue(_.OutputNode));

            pathsFromProcedures.Add(node);
        }

        pathsFromProcedures.Reverse();
        return pathsFromProcedures.AsReadOnly();
    }

    private void UpdateEvaluationNode(Node node) {
        EvaluationNode evaluationNode = _context.GetEvaluated(node);
        if (evaluationNode.ProcessedThisFrame)
            return;

        evaluationNode.ProcessedThisFrame = true;

        Debug.Assert.True(evaluationNode.ArgValues.Length == node.Function.Args.Count);

        evaluationNode.ArgumentsUpdatedThisFrame =
            evaluationNode.ArgDependencies
                .SkipNulls()
                .Any(dependency => !dependency!.Node.ArgumentsUpdatedThisFrame);
    }

    private IReadOnlyList<EvaluationNode> GetInvokationList() {
        Queue<EvaluationNode> yetToProcessENodes = new();
        _context.EvaluationNodes
            .Where(_ => _.Node.Function.IsProcedure)
            .Foreach(yetToProcessENodes.Enqueue);

        List<EvaluationNode> invokationList = new();

        while (yetToProcessENodes.Count > 0) {
            EvaluationNode enode = yetToProcessENodes.Dequeue();
            invokationList.Add(enode);

            for (int i = 0; i < enode.ArgCount; i++) {
                Dependency? dependency = enode.ArgDependencies[i];
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
                    continue;
                }
            }
        }

        invokationList.Reverse();
        return invokationList;
    }
}
}