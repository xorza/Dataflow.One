using System;
using System.Linq;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using NUnit.Framework;

namespace csso.NodeCore.Tests;

public class GraphModification {
    private readonly Function _addFunc = new("Add", F.Add);

    private readonly FrameNoFunc _frameNoFunc = new();
    private readonly OutputFunc<Int32> _outputFunc = new();
    private readonly ConstantFunc<Int32> _constFunc1 = new("Integer");

    private Node? _addNode;
    private Node? _constNode1;
    private Node? _frameNoNode;
    private Graph? _graph;
    private Node? _outputNode;

    [SetUp]
    public void Setup() {
        _graph = new Graph();

        _constFunc1.TypedValue = 1253;

        _constNode1 = _graph.AddNode(_constFunc1);
        _outputNode = _graph.AddNode(_outputFunc);
        _frameNoNode = _graph.AddNode(_frameNoFunc);
        _addNode = _graph.AddNode(_addFunc);

        _graph.Add(
            new DataSubscription(
                _outputNode.Inputs.Single(),
                _addNode.Outputs.Single())
        );

        _graph.Add(
            new DataSubscription(
                _addNode.Inputs[0],
                _frameNoNode.Outputs.Single())
        );

        var alwaysEvent = new AlwaysEvent();

        var subscription = new EventSubscription(alwaysEvent, _outputNode);
        _graph.Add(subscription);
    }

    [Test]
    public void Test1() {
        var executor = new Executor(_graph!);
        _frameNoFunc.Executor = executor;


        Assert.Throws<ArgumentMissingException>(() => executor.Run());
        Assert.Throws<ArgumentMissingException>(() => executor.Run());

        _graph!.Add(
            new DataSubscription(
                _addNode!.Inputs[1],
                _constNode1!.Outputs.Single())
        );

        executor.Run();
        Assert.AreEqual(1253, _outputFunc.Value);

        executor.Run();
        Assert.AreEqual(1254, _outputFunc.Value);

        Assert.Pass();
    }
}