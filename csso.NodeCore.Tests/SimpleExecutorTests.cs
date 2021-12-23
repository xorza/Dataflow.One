using System;
using System.Diagnostics;
using System.Linq;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using NUnit.Framework;

namespace csso.NodeCore.Tests;

public class Tests {
    private Graph? _graph;
    private Node? _constNode1;
    private Node? _constNode2;
    private Node? _outputNode;
    private Node? _frameNoNode;
    private Node? _addNode;

    private readonly ValueFunc<Int32> _constFunc1 = new();
    private readonly ConfigValueFunc<Int32> _configConstFunc2 = new();
    private readonly OutputFunc<Int32> _outputFunc = new();
    private readonly FrameNoFunc _frameNoFunc = new();

    private readonly Function _addFunc = new("Value", F.Add);


    [SetUp]
    public void Setup() {
        _graph = new Graph();

        _constFunc1.Value = 3;
        _configConstFunc2.Config.Single().Value = 1253;

        _constNode1 = _graph.AddNode(_constFunc1);
        _constNode2 = _graph.AddNode(_configConstFunc2);
        _outputNode = _graph.AddNode(_outputFunc);
        _frameNoNode = _graph.AddNode(_frameNoFunc);
        _addNode = _graph.AddNode(_addFunc);
    }

    [Test]
    public void Test1() {
        _outputNode!.AddConnection(
            _outputNode!.Function.Inputs.Single(),
            _constNode1!,
            _constNode1!.Function.Outputs.Single());
        
        var executor = _graph!.Compile();
        _frameNoFunc.Executor = executor;
        
        executor.Run();
        Assert.AreEqual(3, _outputFunc.Value);

        _constFunc1.Value = 33;
        executor.Run();

        Assert.AreEqual(33, _outputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test2() {
        _outputNode!.AddConnection(
            _outputNode!.Function.Inputs.Single(),
            _frameNoNode!,
            _frameNoNode!.Function.Outputs.Single());

        var executor = _graph!.Compile();
        _frameNoFunc.Executor = executor;
        
        executor.Run();
        Assert.AreEqual(0, _outputFunc.Value);
        
        executor.Run();
        Assert.AreEqual(1, _outputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test3() {
        _outputNode!.AddConnection(
            _outputNode!.Function.Inputs.Single(),
            _addNode!,
            _addNode!.Function.Outputs.Single());

        _addNode.AddConnection(
            _addNode.Function.Inputs[0],
            _constNode1!,
            _constNode1!.Function.Outputs.Single());

        _addNode.AddConnection(
            _addNode.Function.Inputs[1],
            _constNode2!,
            _constNode2!.Function.Outputs.Single());
        
        var executor = _graph!.Compile();
        _frameNoFunc.Executor = executor;

        executor.Run();
        Assert.AreEqual(1256, _outputFunc.Value);

        executor.Run();
        Assert.AreEqual(1256, _outputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test4() {
        _outputNode!.AddConnection(
            _outputNode!.Function.Inputs.Single(),
            _addNode!,
            _addNode!.Function.Outputs.Single());

        _addNode.AddConnection(
            _addNode.Function.Inputs[0],
            _constNode1!,
            _constNode1!.Function.Outputs.Single());

        _addNode.AddConnection(
            _addNode.Function.Inputs[1],
            _frameNoNode!,
            _frameNoNode!.Function.Outputs.Single());
        
        var executor = _graph!.Compile();
        _frameNoFunc.Executor = executor;

        executor.Run();
        Assert.AreEqual(3, _outputFunc.Value);
        executor.Run();
        Assert.AreEqual(4, _outputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test5() {
        var connection = _outputNode!.AddConnection(
            _outputNode!.Function.Inputs.Single(),
            _addNode!,
            _addNode!.Function.Outputs.Single());
        connection.Behavior = ConnectionBehavior.Once;

        _addNode.AddConnection(
            _addNode.Function.Inputs[0],
            _constNode1!,
            _constNode1!.Function.Outputs.Single());

        _addNode.AddConnection(
            _addNode.Function.Inputs[1],
            _frameNoNode!,
            _frameNoNode!.Function.Outputs.Single());
        
        var executor = _graph!.Compile();
        _frameNoFunc.Executor = executor;

        executor.Run();
        Assert.AreEqual(3, _outputFunc.Value);
        executor.Run();
        Assert.AreEqual(3, _outputFunc.Value);

        Assert.Pass();
    }
    
    [Test]
    public void Test6() {
        _outputNode!.AddConnection(
            _outputNode!.Function.Inputs.Single(),
            _constNode2!,
            _constNode2!.Function.Outputs.Single());

        var executor = _graph!.Compile();
        _frameNoFunc.Executor = executor;

        _constNode2.ConfigValues.Single().Value = 133;
        
        executor.Run();
        Assert.AreEqual(133, _outputFunc.Value);
        
        
        _constNode2.ConfigValues.Single().Value = 132;
        executor.Run();
        Assert.AreEqual(132, _outputFunc.Value);

        Assert.Pass();
    }
}