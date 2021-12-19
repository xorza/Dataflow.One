using System;
using System.Diagnostics;
using System.Linq;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using NUnit.Framework;

namespace csso.Calculator.Tests;

public class Tests {
    private Graph? _graph;
    private Executor? _executor;
    private Node? _constNode1;
    private Node? _constNode2;
    private Node? _outputNode;
    private Node? _frameNoNode;
    private Node? _addNode;

    private readonly ValueFunc<Int32> _constFunc1 = new();
    private readonly ValueFunc<Int32> _constFunc2 = new();
    private readonly OutputFunc<Int32> _outputFunc = new();

    private readonly Function _addFunc = new("Value", F.Add);


    [SetUp]
    public void Setup() {
        _graph = new Graph();
        _executor = new Executor();

        _constFunc1.Value = 3;
        _constFunc2.Value = 1253;

        _constNode1 = _graph.AddNode(_constFunc1);
        _outputNode = _graph.AddNode(_outputFunc);
        _frameNoNode = _graph.AddNode(_executor.FrameNoFunction);
        _addNode = _graph.AddNode(_addFunc);
        _constNode2 = _graph.AddNode(_constFunc2);
    }

    [Test]
    public void Test1() {
        _outputNode!.AddConnection(
            _outputNode!.Function.Inputs.Single(),
            _constNode1!,
            _constNode1!.Function.Outputs.Single());
        _executor!.Reset();
        _executor.Run(_graph!);
        Assert.AreEqual(3, _outputFunc.Value);

        _constFunc1.Value = 33;
        _executor.Reset();
        _executor.Run(_graph!);

        Assert.AreEqual(_outputFunc.Value, 33);

        Assert.Pass();
    }

    [Test]
    public void Test2() {
        _outputNode!.AddConnection(
            _outputNode!.Function.Inputs.Single(),
            _frameNoNode!,
            _frameNoNode!.Function.Outputs.Single());

        _executor!.Reset();
        _executor.Run(_graph!);
        Assert.AreEqual(0, _outputFunc.Value);
        _executor.Run(_graph!);
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

        _executor!.Reset();
        _executor.Run(_graph!);
        Assert.AreEqual(1256, _outputFunc.Value);

        _executor.Run(_graph!);
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

        _executor!.Reset();
        _executor.Run(_graph!);
        Assert.AreEqual(3, _outputFunc.Value);
        _executor.Run(_graph!);
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

        _executor!.Reset();
        _executor.Run(_graph!);
        Assert.AreEqual(3, _outputFunc.Value);
        _executor.Run(_graph!);
        Assert.AreEqual(3, _outputFunc.Value);

        Assert.Pass();
    }
}