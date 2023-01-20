using System;
using System.Collections.Generic;

namespace csso.NodeCore;

public class NoLoopValidator {
    public void Go(Graph graph) {
        List<Node> path = new();

        foreach (var outputNode in graph.Nodes)
        foreach (var binding in graph.GetDataSubscriptions(outputNode)) {
            if (binding.Source != null) {
                Go(binding.Source, path);
            }
        }
    }

    private void Go(NodeArg arg, List<Node> pathBack) {
        var node = arg.Node;

        if (pathBack.Contains(node)) {
            throw new Exception("loop detected");
        }

        pathBack.Add(node);

        foreach (var b in node.Graph.GetDataSubscriptions(node)) {
            if (b.Source != null) {
                Go(b.Source, pathBack);
            }
        }

        pathBack.RemoveAt(pathBack.Count - 1);
    }
}