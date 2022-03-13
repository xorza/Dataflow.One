﻿using System.Runtime.InteropServices;
using csso.Common;
using csso.NodeCore.Run;

namespace csso.NodeCore;

public sealed class Graph {
    private readonly List<Node> _nodes = new();

    public Graph() {
        Nodes = _nodes.AsReadOnly();
    }

    public IReadOnlyList<Node> Nodes { get; }

    private void Add(Node node) {
        Check.True(node.Graph == this);
        _nodes.Add(node);
    }

    public Node AddNode(Function function) {
        Node node = new (this, function);
        Add(node);
        return node;
    }

    public void Remove(Node node) {
        Check.True(node.Graph == this);

        if (!_nodes.Remove(node))
            throw new Exception("543b6u365");

        _nodes
            .SelectMany(_ => _.Connections)
            .OfType<BindingConnection>()
            .Where(_ => _.TargetNode == node)
            .ToArray()
            .Foreach(_ => _.Node.Remove(_));
    }

    public Executor Compile() {
        return new Executor(this);
    }

    public FunctionFactory FunctionFactory { get; set; } = new();

    public SerializedGraph Serialize() {
        SerializedGraph result = new();
        result.Nodes = _nodes
            .Select(node => node.Serialize())
            .ToArray();

        result.OutputConnections = _nodes
            .SelectMany(_ => _.Connections)
            .OfType<BindingConnection>()
            .Select(_ => _.Serialize())
            .ToArray();

        return result;
    }

    public Graph(
        FunctionFactory functionFactory,
        SerializedGraph serialized) : this() {
        FunctionFactory = functionFactory;

        serialized.Nodes
            .Select(_ => new Node(this, _))
            .Foreach(_nodes.Add);

        serialized.OutputConnections
            .Select(_ => new BindingConnection(this, _))
            .Foreach(_ => { _.Node.Add(_); });
    }

    public Node GetNode(Guid id) {
        return Nodes.Single(node => node.Id == id);
    }
}

public struct SerializedGraph {
    public SerializedNode[] Nodes { get; set; }
    public SerializedOutputConnection[] OutputConnections { get; set; }
}