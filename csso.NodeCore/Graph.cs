using csso.Common;

namespace csso.NodeCore; 

public class Graph {
    private readonly List<Node> _nodes = new();

    public Graph() {
        Nodes = _nodes.AsReadOnly();
    }

    public IReadOnlyList<Node> Nodes { get; }

    public void Add(Node node) {
        Debug.Assert.AreSame(node.Graph, this);

        _nodes.Add(node);
    }
}