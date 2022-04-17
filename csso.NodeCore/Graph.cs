using csso.Common;

namespace csso.NodeCore;

public sealed class Graph {
    private readonly List<Node> _nodes = new();

    public Graph() {
        Nodes = _nodes.AsReadOnly();
    }

    public Graph(
        FunctionFactory functionFactory,
        SerializedGraph serialized) : this() {
        FunctionFactory = functionFactory;

        serialized.FunctionNodes
            .Select(_ => new FunctionNode(this, _))
            .ForEach(_nodes.Add);

        serialized.GraphNodes?
            .Select(_ => new GraphNode(this, _))
            .ForEach(_nodes.Add);

        serialized.OutputConnections
            .Select(_ => new BindingConnection(this, _))
            .ForEach(_ => { _.Node.Add(_); });
    }

    public IReadOnlyList<Node> Nodes { get; }

    public FunctionFactory FunctionFactory { get; set; } = new();

    private void Add(Node node) {
        Check.True(node.Graph == this);
        _nodes.Add(node);
    }

    public Node AddNode(Function function) {
        Node node = new FunctionNode(this, function);
        Add(node);
        return node;
    }

    public void Remove(Node node) {
        Check.True(node.Graph == this);

        if (!_nodes.Remove(node)) {
            throw new Exception("5h4gub677ge657");
        }

        _nodes
            .SelectMany(_ => _.BindingConnections)
            .Where(_ => _.TargetNode == node)
            .ToArray()
            .ForEach(_ => _.Node.Remove(_));
    }

    public SerializedGraph Serialize() {
        SerializedGraph result = new();
        result.FunctionNodes = new List<SerializedFunctionNode>();
        result.GraphNodes = new List<SerializedGraphNode>();

        foreach (var node in _nodes) {
            if (node is FunctionNode functionNode) {
                result.FunctionNodes.Add(functionNode.Serialize());
            } else if (node is GraphNode graphNode) {
                result.GraphNodes.Add(graphNode.Serialize());
            } else {
                throw new NotImplementedException("ervthhb35e65");
            }
        }


        result.OutputConnections = _nodes
            .SelectMany(_ => _.BindingConnections)
            .Select(_ => _.Serialize())
            .ToArray();

        return result;
    }

    public Node GetNode(Guid id) {
        return Nodes.Single(node => node.Id == id);
    }
}

public struct SerializedGraph {
    public List<SerializedFunctionNode> FunctionNodes { get; set; }
    public List<SerializedGraphNode> GraphNodes { get; set; }
    public SerializedOutputConnection[] OutputConnections { get; set; }
}