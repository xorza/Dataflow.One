using System;
using csso.NodeCore.Funcs;

namespace csso.NodeCore.Tests;

public class TestGraph {
    public Function AddFunc { get; } = new("Add", F.Add);
    public ConstantFunc<Int32> ReactiveConstFunc { get; } = new();
    public ConstantFunc<Int32> ProactiveConstFunc { get; } = new();
    public OutputFunc<Int32> OutputFunc { get; } = new();

    public FrameNoFunc FrameNoFunc { get; } = new();
    public Graph Graph { get; }
    public Node AddNode { get; }
    public Node ReactiveConstNode { get; }
    public Node ProactiveConstNode { get; }
    public Node FrameNoNode { get; }
    public Node OutputNode { get; }

    public TestGraph() {
        Graph = new Graph();

        ReactiveConstNode = Graph.AddNode(ReactiveConstFunc);
        ReactiveConstNode.Name = "ReactiveConstNode";
        ReactiveConstFunc.SetBehavior(FunctionBehavior.Reactive);

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