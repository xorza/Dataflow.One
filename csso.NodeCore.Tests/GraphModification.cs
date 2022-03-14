using System;
using System.Linq;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using NUnit.Framework;

namespace csso.NodeCore.Tests;

public class GraphModification {
    private Graph? _graph;
    private Node? _constNode1;
    private Node? _outputNode;
    private Node? _frameNoNode;
    private Node? _addNode;

    private readonly ConfigValueFunc<Int32> _configConstFunc1 = new();
    private readonly OutputFunc<Int32> _outputFunc = new();
    private readonly FrameNoFunc _frameNoFunc = new();

    private readonly Function _addFunc = new("Add", F.Add);


    [SetUp]
    public void Setup() {
        _graph = new Graph();

        _configConstFunc1.Config.Single().Value = 1253;

        _constNode1 = _graph.AddNode(_configConstFunc1);
        _outputNode = _graph.AddNode(_outputFunc);
        _frameNoNode = _graph.AddNode(_frameNoFunc);
        _addNode = _graph.AddNode(_addFunc);

        _outputNode!.AddConnection(
            _outputNode!.Function.Inputs.Single(),
            _addNode!,
            _addNode!.Function.Outputs.Single());

        _addNode!.AddConnection(
            _addNode!.Function.Inputs[0],
            _frameNoNode!,
            _frameNoNode!.Function.Outputs.Single());

        // _addNode!.AddConnection(
        //     _addNode!.Function.Inputs[1],
        //     _frameNoNode!,
        //     _frameNoNode!.Function.Outputs.Single());
    }

    [Test]
    public void Test1() {
        var executor = new Executor(_graph!);
        _frameNoFunc.Executor = executor;


        Assert.Throws<ArgumentMissingException>(() => executor.Run());
        Assert.Throws<ArgumentMissingException>(() => executor.Run());

        _addNode!.AddConnection(
            _addNode!.Function.Inputs[1],
            _constNode1!,
            _constNode1!.Function.Outputs.Single());

        executor.Run();
        Assert.AreEqual(1253, _outputFunc.Value);

        executor.Run();
        Assert.AreEqual(1254, _outputFunc.Value);
        
        Assert.Pass();
    }
}