using System;
using System.Collections.Generic;

namespace csso.NodeCore {
public class NoLoopValidator {
    public void Go(Graph graph) {
        var nodeCount = graph.Nodes.Count;

        List<Node> path = new();
        foreach (Node outputNode in graph.Nodes)
        foreach (var binding in outputNode.Connections)
            Go(binding, path);
    }

    private void Go(Connection connection, List<Node> pathBack) {
        var outputBinding = connection as OutputConnection;
        if (outputBinding == null) return;

        Node node = outputBinding.OutputNode;

        if (pathBack.Contains(node))
            throw new Exception("loop detected");

        pathBack.Add(node);

        foreach (Connection b in node.Connections) Go(b, pathBack);

        pathBack.RemoveAt(pathBack.Count - 1);
    }
}
}