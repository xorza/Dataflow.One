using System;
using csso.NodeCore.Funcs;

namespace csso.NodeCore.Tests;

public class TestGraph {
    public Function AddFunc { get; } = new("Add", F.Add);
    public ConstantFunc<Int32> ReactiveConstFuncBaseBase { get; } = new("Int32 reactive");
    public ConstantFunc<Int32> ProactiveConstFuncBaseBase { get; } = new("Int32 proactive");
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

        ReactiveConstNode = Graph.AddNode(ReactiveConstFuncBaseBase);
        ReactiveConstNode.Name = "ReactiveConstNode";
        ReactiveConstNode.Behavior = FunctionBehavior.Reactive;

        ProactiveConstNode = Graph.AddNode(ProactiveConstFuncBaseBase);
        ProactiveConstNode.Name = "ProactiveConstNode";
        ProactiveConstNode.Behavior = FunctionBehavior.Proactive;

        FrameNoNode = Graph.AddNode(FrameNoFunc);
        AddNode = Graph.AddNode(AddFunc);
        OutputNode = Graph.AddNode(OutputFunc);

        var always = new AlwaysEvent();

        var subscription = new EventSubscription(always, OutputNode);
        Graph.Add(subscription);
    }
}