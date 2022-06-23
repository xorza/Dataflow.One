using System;
using System.Collections.Generic;
using System.Linq;
using dfo.Common;

namespace dfo.NodeCore;

public sealed class Graph {
    private readonly List<DataSubscription> _dataSubscriptions = new();
    private readonly Queue<Event> _eventsToProcess = new();

    private readonly List<EventSubscription> _eventSubscriptions = new();
    private readonly List<Node> _nodes = new();

    public Graph() {
        Nodes = _nodes.AsReadOnly();
    }

    public IReadOnlyList<EventSubscription> EventSubscriptions => _eventSubscriptions.AsReadOnly();
    public IReadOnlyList<DataSubscription> DataSubscriptions => _dataSubscriptions.AsReadOnly();

    public IReadOnlyList<Node> Nodes { get; }

    public FunctionFactory FunctionFactory { get; } = new();

    private void Add(Node node) {
        node.Graph = this;
        _nodes.Add(node);
    }

    public void Add(EventSubscription eventEventSubscription) {
        Check.True(eventEventSubscription.Node.Graph == this);
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
        Check.True(subscriber.ArgDirection == ArgDirection.In);
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

    public List<Event> ProcessFiredEvents() {
        var list = new List<Event>();

        EventSubscriptions
            .Select(_ => _.Event)
            .OfType<AlwaysEvent>()
            .ForEach(list.Add);

        list.AddRange(_eventsToProcess.ToList());

        _eventsToProcess.Clear();

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

        if (!_nodes.Remove(node)) throw new Exception("5h4gub677ge657");

        _dataSubscriptions
            .RemoveAll(_ => _.Subscriber.Node == node);
        _dataSubscriptions
            .RemoveAll(_ => _.Source.Node == node);
        _eventSubscriptions
            .RemoveAll(_ => _.Node == node);
    }
}