using System.Collections.Generic;
using csso.Common;

namespace csso.NodeCore {
public class Graph {
    private readonly List<Node> _nodes = new();
    private readonly List<OutputNode> _outputNodes = new();

    public Graph() {
        Nodes = _nodes.AsReadOnly();
        Outputs = _outputNodes.AsReadOnly();
    }

    public IReadOnlyList<Node> Nodes { get; }
    public IReadOnlyList<OutputNode> Outputs { get; }

    internal void Add(Node node) {
        Debug.Assert.AreSame(node.Graph, this);

        _nodes.Add(node);
    }

    internal void AddOutput(OutputNode node) {
        Debug.Assert.AreSame(node.Graph, this);

        _outputNodes.Add(node);
    }
}
}