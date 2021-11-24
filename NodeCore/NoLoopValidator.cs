using System;
using System.Collections.Generic;

namespace csso.NodeCore {
public class NoLoopValidator {
    public void Go(Graph graph) {
        var nodeCount = graph.Nodes.Count;

        List<Node> path = new();
        foreach (Binding binding in graph.Outputs) Go(binding, path);
    }

    private void Go(Binding binding, List<Node> pathBack) {
        var outputBinding = binding as OutputBinding;
        if (outputBinding == null) return;

        Node node = outputBinding.OutputNode;

        if (pathBack.Contains(node)) throw new Exception("loop detected");

        pathBack.Add(node);

        for (var i = 0; i < node.Inputs.Length; i++) Go(node.Inputs[i], pathBack);

        pathBack.RemoveAt(pathBack.Count - 1);
    }
}
}