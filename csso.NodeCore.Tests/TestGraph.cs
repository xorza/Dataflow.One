using System;
using csso.NodeCore.Funcs;

namespace csso.NodeCore.Tests;

public class TestGraph {
    public Function AddFunc { get; } = new("Add", F.Add);
    public ValueFunc<Int32> ConstFunc1 { get; } = new();
    public ValueFunc<Int32> ConstFunc2 { get; } = new();
    public OutputFunc<Int32> OutputFunc { get; } = new();

    public FrameNoFunc FrameNoFunc { get; } = new();
    public Graph Graph { get; private set; }
    public Node AddNode { get; private set; }
    public Node ConstNode1 { get; private set; }
    public Node ConstNode2 { get; private set; }
    public Node FrameNoNode { get; private set; }
    public Node OutputNode { get; private set; }

    public TestGraph() {
        Graph = new Graph();

 

        ConstNode1 = Graph.AddNode(ConstFunc1);
        ConstNode2 = Graph.AddNode(ConstFunc2);
        FrameNoNode = Graph.AddNode(FrameNoFunc);
        AddNode = Graph.AddNode(AddFunc);
        OutputNode = Graph.AddNode(OutputFunc);

        var always = new AlwaysEvent();
        OutputNode.Add(always);

        var subscription = new EventSubscription(always, OutputNode);
        Graph.Add(subscription);
    }
}