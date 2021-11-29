using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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

        public List<EvaluatedNode> EvaluatedNodes { get; } = new();

        public EvaluatedNode? GetEvaluated(Node node) {
            return EvaluatedNodes.SingleOrDefault(_ => _.Node == node);
        }
    }

    internal class EvaluatedNode {
        public Node Node;
        public Object?[] ArgValues { get; }
        public Int32 ArgCount { get; }

        public EvaluatedNode(Node node) {
            Node = node;
            ArgCount = node.Function.Args.Count;
            ArgValues = new Object?[ArgCount];
        }

        public object? GetOutputValue(FunctionOutput outputConnectionOutput) {
            Int32? index = Node.Function.Args.FirstIndexOf(outputConnectionOutput);
            if (index == null)
                throw new Exception("unerty i");
            else
                return ArgValues[index.Value];
        }
    }

    private class NotFound { }

    private readonly Object _notFound = new NotFound();

    public Graph Graph { get; }

    public Executor(Graph graph) {
        Graph = graph;
    }

    public void Run() {
        var rootNodes = Graph.Nodes
            .Where(_ => _.Function.IsOutput);

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
            EvaluatedNode enode = new(node);

            for (int i = 0; i < enode.ArgCount; i++) {
                Object? argValue = _notFound;
                FunctionArg arg = node.Function.Args[i];

                if (arg is FunctionInput input) {
                    Connection? connection = node.Connections.SingleOrDefault(_ => _.Input == input);
                    if (connection == null) throw new Exception("asfdsdfgweg");

                    if (connection is ValueConnection valueConnection) {
                        argValue = valueConnection.Value;
                    }

                    if (connection is OutputConnection outputConnection) {
                        EvaluatedNode? executedNode = context.GetEvaluated(outputConnection.OutputNode);
                        if (executedNode == null)
                            throw new Exception("tbhjypok dmhiw");

                        argValue = executedNode.GetOutputValue(outputConnection.Output);
                    }
                } else if (arg is FunctionOutput output) {
                    Int32 v = 0;
                    argValue = v;
                } else if (arg is FunctionConfig config)
                    argValue = config.Value;
                else
                    Debug.Assert.False();

                if (argValue == _notFound)
                    throw new Exception("difgdliuytgr");

                enode.ArgValues[i] = argValue;
            }

            if (enode.ArgValues.Length == 0)
                node.Function.Invoke(null);
            else
                node.Function.Invoke(enode.ArgValues);

            Debug.Assert.True(enode.ArgValues.Length == node.Function.Args.Count);

            context.EvaluatedNodes.Add(enode);
        }
    }
}
}