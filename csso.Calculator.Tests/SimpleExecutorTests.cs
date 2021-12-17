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

        _constNode1 = new(_constFunc1);
        _graph.Add(_constNode1);

        _outputNode = new(_outputFunc);
        _graph.Add(_outputNode);

        _frameNoNode = new(_executor.FrameNoFunction);
        _graph.Add(_frameNoNode);

        _addNode = new(_addFunc);
        _graph.Add(_addNode);

        _constNode2 = new(_constFunc2);
        _graph.Add(_constNode2);
    }

    [Test]
    public void Test1() {
        OutputConnection connection = new(
            _outputNode!,
            _outputNode!.Function.Inputs.Single(),
            _constNode1!,
            _constNode1!.Function.Outputs.Single());
        _outputNode.Add(connection);

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
        OutputConnection connection = new(
            _outputNode!,
            _outputNode!.Function.Inputs.Single(),
            _frameNoNode!,
            _frameNoNode!.Function.Outputs.Single());
        _outputNode.Add(connection);

        _executor!.Reset();
        _executor.Run(_graph!);
        Assert.AreEqual(0, _outputFunc.Value);
        _executor.Run(_graph!);
        Assert.AreEqual(1, _outputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test3() {
        OutputConnection connection = new(
            _outputNode!,
            _outputNode!.Function.Inputs.Single(),
            _addNode!,
            _addNode!.Function.Outputs.Single());
        _outputNode.Add(connection);

        OutputConnection connection2 = new(
            _addNode,
            _addNode.Function.Inputs[0],
            _constNode1!,
            _constNode1!.Function.Outputs.Single());
        _addNode.Add(connection2);

        OutputConnection connection3 = new(
            _addNode,
            _addNode.Function.Inputs[1],
            _constNode2!,
            _constNode2!.Function.Outputs.Single());
        _addNode.Add(connection3);

        _executor!.Reset();
        _executor.Run(_graph!);
        Assert.AreEqual(1256, _outputFunc.Value);

        _executor.Run(_graph!);
        Assert.AreEqual(1256, _outputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test4() {
        OutputConnection connection = new(
            _outputNode!,
            _outputNode!.Function.Inputs.Single(),
            _addNode!,
            _addNode!.Function.Outputs.Single());
        _outputNode.Add(connection);

        OutputConnection connection2 = new(
            _addNode,
            _addNode.Function.Inputs[0],
            _constNode1!,
            _constNode1!.Function.Outputs.Single());
        _addNode.Add(connection2);

        OutputConnection connection3 = new(
            _addNode,
            _addNode.Function.Inputs[1],
            _frameNoNode!,
            _frameNoNode!.Function.Outputs.Single());
        _addNode.Add(connection3);

        _executor!.Reset();
        _executor.Run(_graph!);
        Assert.AreEqual(3, _outputFunc.Value);
        _executor.Run(_graph!);
        Assert.AreEqual(4, _outputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test5() {
        OutputConnection connection = new(
            _outputNode!,
            _outputNode!.Function.Inputs.Single(),
            _addNode!,
            _addNode!.Function.Outputs.Single());
        connection.Behavior = ConnectionBehavior.Once;
        _outputNode.Add(connection);

        OutputConnection connection2 = new(
            _addNode,
            _addNode.Function.Inputs[0],
            _constNode1!,
            _constNode1!.Function.Outputs.Single());
        _addNode.Add(connection2);

        OutputConnection connection3 = new(
            _addNode,
            _addNode.Function.Inputs[1],
            _frameNoNode!,
            _frameNoNode!.Function.Outputs.Single());
        _addNode.Add(connection3);

        _executor!.Reset();
        _executor.Run(_graph!);
        Assert.AreEqual(3, _outputFunc.Value);
        _executor.Run(_graph!);
        Assert.AreEqual(3, _outputFunc.Value);

        Assert.Pass();
    }
}
