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
            .Foreach(_ => _nodes.Add(new Node(FunctionFactory, _)));

        serialized.OutputConnections
            .Select(_ => new OutputConnection(this, _))
            .Foreach(_ => { _.InputNode.AddConnection(_); });
    }

    public Node GetNode(Guid id) {
        return Nodes.Single(node => node.Id == id);
    }
}

public struct SerializedGraph {
    public SerializedNode[] Nodes { get; set; }
    public SerializedOutputConnection[] OutputConnections { get; set; }
}