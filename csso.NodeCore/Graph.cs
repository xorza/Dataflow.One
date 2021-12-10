using System.Runtime.InteropServices;
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

    public void Remove(Node node) {
        // Check.True(node.Graph != null);
        Debug.Assert.AreSame(node.Graph, this);

        if (!_nodes.Remove(node))
            throw new Exception("543b6u365");

        _nodes
            .SelectMany(_ => _.Connections)
            .OfType<OutputConnection>()
            .Where(_ => _.OutputNode == node)
            .ToArray()
            .Foreach(_ => _.InputNode.Remove(_));
    }

    public FunctionFactory FunctionFactory { get; set; } = new();

    public SerializedGraph Serialize() {
        SerializedGraph result = new();
        result.Nodes = _nodes
            .Select(node => node.Serialize())
            .ToArray();

        result.OutputConnections = _nodes
            .SelectMany(_ => _.Connections)
            .OfType<OutputConnection>()
            .Select(_ => _.Serialize())
            .ToArray();

        return result;
    }

    public Graph(
        FunctionFactory functionFactory,
        SerializedGraph serialized) : this() {
        FunctionFactory = functionFactory;

        serialized.Nodes
            .Select(_ => new Node(this, FunctionFactory, _))
            .Foreach(_nodes.Add);

        serialized.OutputConnections
            .Select(_ => new OutputConnection(this, _))
            .Foreach(_ => { _.InputNode.Add(_); });
    }

    public Node GetNode(Guid id) {
        return Nodes.Single(node => node.Id == id);
    }
}

public struct SerializedGraph {
    public SerializedNode[] Nodes { get; set; }
    public SerializedOutputConnection[] OutputConnections { get; set; }
}