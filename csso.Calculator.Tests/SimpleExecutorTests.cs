using System;
using System.Linq;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using NUnit.Framework;

namespace csso.Calculator.Tests;

public class Tests {
    [System.ComponentModel.Description("value")]
    [Reactive]
    private static bool Const([Config(12)] Int32 c, [Output] out Int32 i) {
        i = c;
        return true;
    }

    private Int32 _outputValue = -1;

    [Reactive]
    private bool Output(Int32 value) {
        _outputValue = value;
        return true;
    }

    private Graph _graph;
    private Executor _executor;
    private Node _constNode1;
    private Node _constNode2;
    private Node _outputNode;
    private Node _frameNoNode;
    private Node _addNode;

    private readonly Function _constFunc = new("Value", Const);
    private Function _outputFunc;
    private readonly Function _addFunc = new("Value", F.Add);

    [SetUp]
    public void Setup() {
        _graph = new Graph();
        _executor = new Executor();

        _outputFunc = new Function("Output", Output);

        _constFunc.Config.Single().DefaultValue = 13;
        _constNode1 = new(_constFunc, _graph);
        _graph.Add(_constNode1);

        _outputNode = new(_outputFunc, _graph);
        _graph.Add(_outputNode);

        _frameNoNode = new(_executor.FrameNoFunction, _graph);
        _graph.Add(_frameNoNode);

        _addNode = new(_addFunc, _graph);
        _graph.Add(_addNode);
        
        _constNode2 = new(_constFunc, _graph);
        _graph.Add(_constNode2);
    }

    [Test]
    public void Test1() {
        OutputConnection connection = new(
            _outputNode,
            _outputNode.Function.Inputs.Single(),
            _constNode1,
            _constNode1.Function.Outputs.Single());
        _outputNode.Add(connection);

        _executor.Reset();
        _executor.Run(_graph);
        Assert.AreEqual(13, _outputValue);

        _constNode1.ConfigValues.Single().Value = 1253;
        _executor.Reset();
        _executor.Run(_graph);

        Assert.AreEqual(_outputValue, 1253);

        Assert.Pass();
    }

    [Test]
    public void Test2() {
        OutputConnection connection = new(
            _outputNode,
            _outputNode.Function.Inputs.Single(),
            _frameNoNode,
            _frameNoNode.Function.Outputs.Single());
        _outputNode.Add(connection);

        _executor.Reset();
        _executor.Run(_graph);
        Assert.AreEqual(0, _outputValue);
        _executor.Run(_graph);
        Assert.AreEqual(1, _outputValue);

        Assert.Pass();
    }


    [Test]
    public void Test3() {
        _constFunc.Config.Single().DefaultValue = 3;
        
        // const2Node.ConfigValues.Single().Value = 1253;

        OutputConnection connection = new(
            _outputNode,
            _outputNode.Function.Inputs.Single(),
            _addNode,
            _addNode.Function.Outputs.Single());
        _outputNode.Add(connection);


        OutputConnection connection2 = new(
            _addNode,
            _addNode.Function.Inputs[0],
            _constNode1,
            _constNode1.Function.Outputs.Single());
        _addNode.Add(connection2);

        OutputConnection connection3 = new(
            _addNode,
            _addNode.Function.Inputs[1],
            _constNode1,
            _constNode1.Function.Outputs.Single());
        _addNode.Add(connection3);


        _executor.Reset();
        _executor.Run(_graph);
        Assert.AreEqual(26, _outputValue);
        
        _constNode1.ConfigValues.Single().Value = 13;
        
        _executor.Run(_graph);
        Assert.AreEqual(26, _outputValue);

        Assert.Pass();
    }

    [Test]
    public void Test4() {
        _constNode1.ConfigValues.Single().Value = 3;

        OutputConnection connection = new(
            _outputNode,
            _outputNode.Function.Inputs.Single(),
            _addNode,
            _addNode.Function.Outputs.Single());
        _outputNode.Add(connection);

        OutputConnection connection2 = new(
            _addNode,
            _addNode.Function.Inputs[0],
            _constNode1,
            _constNode1.Function.Outputs.Single());
        _addNode.Add(connection2);

        OutputConnection connection3 = new(
            _addNode,
            _addNode.Function.Inputs[1],
            _frameNoNode,
            _frameNoNode.Function.Outputs.Single());
        _addNode.Add(connection3);

        _executor.Reset();
        _executor.Run(_graph);
        Assert.AreEqual( 3, _outputValue);
        _executor.Run(_graph);
        Assert.AreEqual( 4, _outputValue);

        Assert.Pass();
    }

    [Test]
    public void Test5() {
        _constNode1.ConfigValues.Single().Value = 3;

        OutputConnection connection = new(
            _outputNode,
            _outputNode.Function.Inputs.Single(),
            _addNode,
            _addNode.Function.Outputs.Single());
        connection.Behavior = ConnectionBehavior.Once;
        _outputNode.Add(connection);

        OutputConnection connection2 = new(
            _addNode,
            _addNode.Function.Inputs[0],
            _constNode1,
            _constNode1.Function.Outputs.Single());
        _addNode.Add(connection2);

        OutputConnection connection3 = new(
            _addNode,
            _addNode.Function.Inputs[1],
            _frameNoNode,
            _frameNoNode.Function.Outputs.Single());
        _addNode.Add(connection3);

        _executor.Reset();
        _executor.Run(_graph);
        Assert.AreEqual(3, _outputValue);
        _executor.Run(_graph);
        Assert.AreEqual(3, _outputValue);

        Assert.Pass();
    }

    [Test]
    public void Test6() {

    }
}