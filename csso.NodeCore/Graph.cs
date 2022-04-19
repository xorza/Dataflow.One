using csso.Common;

namespace csso.NodeCore;

public sealed class Graph {
    private readonly List<Node> _nodes = new();
    private readonly Queue<Event> _eventsToProcess = new();

    private readonly List<EventSubscription> _eventSubscriptions = new();
    public IReadOnlyList<EventSubscription> EventSubscriptions => _eventSubscriptions.AsReadOnly();

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
        _eventSubscriptions.Add(eventEventSubscription);
    }

    public void Add(DataSubscription dataSubscription) {
        Check.True(dataSubscription.Subscriber.Node.Graph == this);
        Check.True(dataSubscription.Source.Node.Graph == this);

        _dataSubscriptions.RemoveAll(_ => _.Subscriber == dataSubscription.Subscriber);
        _dataSubscriptions.Add(dataSubscription);
    }

    public void Remove(DataSubscription dataSubscription) {
        Check.True(dataSubscription.Subscriber.Node.Graph == this);
        Check.True(dataSubscription.Source.Node.Graph == this);
        _dataSubscriptions.Remove(dataSubscription);
    }

    public void RemoveSubscription(NodeArg subscriber) {
        Check.True(subscriber.ArgType == ArgType.In);
        _dataSubscriptions.RemoveAll(_ => _.Subscriber == subscriber);
    }

    public void Fire(Event @event) {
        _eventsToProcess.Enqueue(@event);
    }

    public List<DataSubscription> GetDataSubscriptions(Node node) {
        return
            _dataSubscriptions
                .Where(_ => _.Subscriber.Node == node)
                .ToList();
    }

    public DataSubscription? GetDataSubscription(NodeArg subscriber) {
        return
            _dataSubscriptions
                .SingleOrDefault(_ => _.Subscriber == subscriber);
    }

    public List<Event> GetFiredEvents() {
        var list = new List<Event>();

        EventSubscriptions
            .Select(_ => _.Event)
            .OfType<AlwaysEvent>()
            .ForEach(list.Add);

        list.AddRange(_eventsToProcess.ToList());

        return list;
    }

    public List<Node> GetSubscribers(Event @event) {
        return _eventSubscriptions
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
            .RemoveAll(_ => _.Subscriber.Node == node);
        _dataSubscriptions
            .RemoveAll(_ => _.Source.Node == node);
        _eventSubscriptions
            .RemoveAll(_ => _.Node == node);
        _eventSubscriptions
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
}

public struct SerializedGraph {
    public List<SerializedFunctionNode> FunctionNodes { get; set; }
    public List<SerializedGraphNode> GraphNodes { get; set; }
    public SerializedSubscription[] Subscriptions { get; set; }
}