using csso.Common;

namespace csso.NodeCore;

public sealed class Graph {
    private readonly List<Node> _nodes = new();
    private readonly Queue<Event> _eventsToProcess = new();

    private readonly List<EventSubscription> _eventConnections = new();

    public IReadOnlyList<EventSubscription> Subscriptions => _eventConnections.AsReadOnly();

    private readonly List<DataSubscription> _dataSubscriptions = new();

    public IReadOnlyList<DataSubscription> DataSubscriptions => _dataSubscriptions.AsReadOnly();


    public Graph() {
        Nodes = _nodes.AsReadOnly();
    }

    public Graph(
        FunctionFactory functionFactory,
        SerializedGraph serialized) : this() {
        FunctionFactory = functionFactory;

        serialized.FunctionNodes
            .Select(_ => new FunctionNode(functionFactory, _))
            .ForEach(_nodes.Add);

        serialized.GraphNodes?
            .Select(_ => new GraphNode(_))
            .ForEach(_nodes.Add);
    }

    public IReadOnlyList<Node> Nodes { get; }

    public FunctionFactory FunctionFactory { get; set; } = new();

    private void Add(Node node) {
        node.Graph = this;
        _nodes.Add(node);
    }

    public void Add(EventSubscription eventEventSubscription) {
        Check.True(eventEventSubscription.Node.Graph == this);
        Check.True(eventEventSubscription.Event.Owner.Graph == this);
        _eventConnections.Add(eventEventSubscription);
    }
    
    public void Add(DataSubscription dataSubscription) {
        Check.True(dataSubscription.SubscriberNode.Graph == this);
        Check.True(dataSubscription.TargetNode.Graph == this);
        _dataSubscriptions.Add(dataSubscription);
    }

    public void Remove(DataSubscription dataSubscription) {
        Check.True(dataSubscription.SubscriberNode.Graph == this);
        Check.True(dataSubscription.TargetNode.Graph == this);
        _dataSubscriptions.Remove(dataSubscription);
    }

    public void Fire(Event @event) {
        _eventsToProcess.Enqueue(@event);
    }

    public List<DataSubscription> GetDataSubscriptions(Node node) {
        return
            _dataSubscriptions
                .Where(_ => _.SubscriberNode == node)
                .ToList();
    }

    public DataSubscription? GetDataSubscription(Node node, FunctionInput input) {
        Debug.Assert.True(() => node.Inputs.Contains(input));

        return
            _dataSubscriptions
                .SingleOrDefault(_ => _.SubscriberNode == node && _.Input == input);
    }

    public List<Event> GetFiredEvents() {
        var list = new List<Event>();

        Subscriptions
            .Select(_ => _.Event)
            .OfType<AlwaysEvent>()
            .ForEach(list.Add);

        list.AddRange(_eventsToProcess.ToList());

        return list;
    }

    public List<Node> GetSubscribers(Event @event) {
        return _eventConnections
            .Where(_ => _.Event == @event)
            .Select(_ => _.Node)
            .ToList();
    }

    public Node AddNode(Function function) {
        Node node = new FunctionNode(function);
        Add(node);
        return node;
    }


    public void Remove(Node node) {
        Check.True(node.Graph == this);

        if (!_nodes.Remove(node)) {
            throw new Exception("5h4gub677ge657");
        }

        _dataSubscriptions
            .RemoveAll(_ => _.SubscriberNode == node);
        _dataSubscriptions
            .RemoveAll(_ => _.TargetNode == node);
        _eventConnections
            .RemoveAll(_ => _.Node == node);
        _eventConnections
            .RemoveAll(_ => _.Event.Owner == node);
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


        // result.OutputConnections = _dataSubscriptions
        //     .Select(_ => _.Serialize())
        //     .ToList();

        return result;
    }

    public Node GetNode(Guid id) {
        return Nodes.Single(node => node.Id == id);
    }
}

public struct SerializedGraph {
    public List<SerializedFunctionNode> FunctionNodes { get; set; }
    public List<SerializedGraphNode> GraphNodes { get; set; }
    public SerializedSubscription[] Subscriptions { get; set; }
}