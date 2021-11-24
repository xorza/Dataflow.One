using System.Collections.Generic;
using csso.Common;

namespace csso.NodeCore {
public class Graph {
    private readonly List<Node> _nodes = new();

    public Graph() {
        Nodes = _nodes.AsReadOnly();
    }

    public IReadOnlyList<Node> Nodes { get; }
    public List<Binding> Outputs { get; } = new();

    internal void Add(Node node) {
        Debug.Assert.AreSame(node.Graph, this);

        _nodes.Add(node);
    }
}
}