using dfo.NodeCore.Funcs;

namespace dfo.NodeCore.Tests;

public class TestGraph {
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

    public Function AddFunc { get; } = new("Add", F.Add);
    public ConstantFunc<int> ReactiveConstFuncBaseBase { get; } = new("Int32 reactive");
    public ConstantFunc<int> ProactiveConstFuncBaseBase { get; } = new("Int32 proactive");
    public OutputFunc<int> OutputFunc { get; } = new();

    public FrameNoFunc FrameNoFunc { get; } = new();
    public Graph Graph { get; }
    public Node AddNode { get; }
    public Node ReactiveConstNode { get; }
    public Node ProactiveConstNode { get; }
    public Node FrameNoNode { get; }
    public Node OutputNode { get; }
}