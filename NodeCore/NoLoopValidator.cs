using System;
using System.Collections.Generic;

namespace csso.NodeCore {
public class NoLoopValidator {
    public void Go(Graph graph) {
        var nodeCount = graph.Nodes.Count;

        List<Node> path = new();
        foreach (Node outputNode in graph.Outputs)
        foreach (var binding in outputNode.Inputs)
            Go(binding, path);
    }

    private void Go(Binding binding, List<Node> pathBack) {
        var outputBinding = binding as OutputBinding;
        if (outputBinding == null) return;

        Node node = outputBinding.OutputNode;

        if (pathBack.Contains(node))
            throw new Exception("loop detected");

        pathBack.Add(node);

        foreach (Binding b in node.Inputs) {
            Go(b, pathBack);
        }

        pathBack.RemoveAt(pathBack.Count - 1);
    }
}
}