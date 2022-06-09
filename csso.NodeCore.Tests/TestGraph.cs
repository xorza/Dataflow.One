using System;
using csso.NodeCore.Funcs;

namespace csso.NodeCore.Tests;

public class TestGraph {
    public Function AddFunc { get; } = new("Add", F.Add);
    public ValueFunc<Int32> ReactiveConstFunc { get; } = new();
    public ValueFunc<Int32> ProactiveConstFunc { get; } = new();
    public OutputFunc<Int32> OutputFunc { get; } = new();

    public FrameNoFunc FrameNoFunc { get; } = new();
    public Graph Graph { get; private set; }
    public Node AddNode { get; private set; }
    public Node ReactiveConstNode { get; private set; }
    public Node ProactiveConstNode { get; private set; }
    public Node FrameNoNode { get; private set; }
    public Node OutputNode { get; private set; }

    public TestGraph() {
        ReactiveConstFunc.SetBehavior(FunctionBehavior.Reactive);

        Graph = new Graph();

        ReactiveConstNode = Graph.AddNode(ReactiveConstFunc);
        ReactiveConstNode.Name = "ReactiveConstNode";
        ProactiveConstNode = Graph.AddNode(ProactiveConstFunc);
        ProactiveConstNode.Name = "ProactiveConstNode";
        FrameNoNode = Graph.AddNode(FrameNoFunc);
        AddNode = Graph.AddNode(AddFunc);
        OutputNode = Graph.AddNode(OutputFunc);

        var always = new AlwaysEvent();

        var subscription = new EventSubscription(always, OutputNode);
        Graph.Add(subscription);
    }
}