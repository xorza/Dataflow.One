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
    private Node _constNode;
    private Node _outputNode;
    private Node _frameNoNode;
    private Node _addNode;

    private Function _constFunc = new("Value", Const);
    private Function _outputFunc;
    private Function _addFunc = new("Value", F.Add);

    [SetUp]
    public void Setup() {
        _graph = new Graph();
        _executor = new Executor();

        _outputFunc = new Function("Output", Output);

        _constFunc.Config.Single().DefaultValue = 13;
        _constNode = new(_constFunc, _graph);
        _graph.Add(_constNode);

        _outputNode = new(_outputFunc, _graph);
        _graph.Add(_outputNode);

        _frameNoNode = new(_executor.FrameNoFunction, _graph);
        _graph.Add(_frameNoNode);

        _addNode = new(_addFunc, _graph);
        _graph.Add(_addNode);
    }

    [Test]
    public void Test1() {
        OutputConnection connection = new(
            _outputNode,
            _outputNode.Function.Inputs.Single(),
            _constNode,
            _constNode.Function.Outputs.Single());
        _outputNode.Add(connection);

        _executor.Reset();
        _executor.Run(_graph);
        Assert.AreEqual(13, _outputValue);

        _constNode.ConfigValues.Single().Value = 1253;
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

        // Node const2Node = new(constFunc, graph);
        // graph.Add(const2Node);

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
            _constNode,
            _constNode.Function.Outputs.Single());
        _addNode.Add(connection2);

        OutputConnection connection3 = new(
            _addNode,
            _addNode.Function.Inputs[1],
            _constNode,
            _constNode.Function.Outputs.Single());
        _addNode.Add(connection3);


        _executor.Reset();
        _executor.Run(_graph);
        Assert.AreEqual(26, _outputValue);
        
        _constNode.ConfigValues.Single().Value = 13;
        
        _executor.Run(_graph);
        Assert.AreEqual(26, _outputValue);

        Assert.Pass();
    }

    [Test]
    public void Test4() {
        var outputValue = 0;

        var graph = new Graph();
        var executor = new Executor();

        Function outputFunc = new Function("Output", (Int32 val) => {
            outputValue = val;
            return true;
        });
        Function constFunc = new Function("Value", Const);
        Function addFunc = new Function("Value", F.Add);


        constFunc.Config.Single().DefaultValue = 3;

        Node outputNode = new(outputFunc, graph);
        graph.Add(outputNode);

        Node addNode = new(addFunc, graph);
        graph.Add(addNode);

        Node constNode = new(constFunc, graph);
        graph.Add(constNode);
        constNode.ConfigValues.Single().Value = 2;

        Node frameNoNode = new(executor.FrameNoFunction, graph);
        graph.Add(frameNoNode);


        OutputConnection connection = new(
            outputNode,
            outputNode.Function.Inputs.Single(),
            addNode,
            addNode.Function.Outputs.Single());
        outputNode.Add(connection);


        OutputConnection connection2 = new(
            addNode,
            addNode.Function.Inputs[0],
            constNode,
            constNode.Function.Outputs.Single());
        addNode.Add(connection2);

        OutputConnection connection3 = new(
            addNode,
            addNode.Function.Inputs[1],
            frameNoNode,
            frameNoNode.Function.Outputs.Single());
        addNode.Add(connection3);


        executor.Reset();
        executor.Run(graph);
        Assert.AreEqual(outputValue, 2);
        executor.Run(graph);
        Assert.AreEqual(outputValue, 3);

        Assert.Pass();
    }

    [Test]
    public void Test5() {
        var outputValue = 0;

        var graph = new Graph();
        var executor = new Executor();

        Function outputFunc = new Function("Output", (Int32 val) => {
            outputValue = val;
            return true;
        });
        Function constFunc = new Function("Value", Const);
        Function addFunc = new Function("Add", F.Add);


        constFunc.Config.Single().DefaultValue = 3;

        Node outputNode = new(outputFunc, graph);
        graph.Add(outputNode);

        Node addNode = new(addFunc, graph);
        graph.Add(addNode);

        Node constNode = new(constFunc, graph);
        graph.Add(constNode);
        constNode.ConfigValues.Single().Value = 2;

        Node frameNoNode = new(executor.FrameNoFunction, graph);
        graph.Add(frameNoNode);


        OutputConnection connection = new(
            outputNode,
            outputNode.Function.Inputs.Single(),
            addNode,
            addNode.Function.Outputs.Single());
        connection.Behavior = ConnectionBehavior.Once;
        outputNode.Add(connection);


        OutputConnection connection2 = new(
            addNode,
            addNode.Function.Inputs[0],
            constNode,
            constNode.Function.Outputs.Single());
        addNode.Add(connection2);

        OutputConnection connection3 = new(
            addNode,
            addNode.Function.Inputs[1],
            frameNoNode,
            frameNoNode.Function.Outputs.Single());
        addNode.Add(connection3);


        executor.Reset();
        executor.Run(graph);
        Assert.AreEqual(2, outputValue);
        executor.Run(graph);
        Assert.AreEqual(2, outputValue);

        Assert.Pass();
    }


    [Test]
    public void Test6() {
        var outputValue = 0;

        var graph = new Graph();
        var executor = new Executor();

        Function outputFunc = new Function("Output", (Int32 val) => {
            outputValue = val;
            return true;
        });
        Function constFunc = new Function("Value", Const);
        Function addFunc = new Function("Add", F.Add);


        constFunc.Config.Single().DefaultValue = 3;

        Node outputNode = new(outputFunc, graph);
        graph.Add(outputNode);

        Node addNode = new(addFunc, graph);
        graph.Add(addNode);

        Node constNode = new(constFunc, graph);
        graph.Add(constNode);
        constNode.ConfigValues.Single().Value = 2;

        Node frameNoNode = new(executor.FrameNoFunction, graph);
        graph.Add(frameNoNode);


        OutputConnection connection = new(
            outputNode,
            outputNode.Function.Inputs.Single(),
            addNode,
            addNode.Function.Outputs.Single());
        connection.Behavior = ConnectionBehavior.Always;
        outputNode.Add(connection);


        OutputConnection connection2 = new(
            addNode,
            addNode.Function.Inputs[0],
            constNode,
            constNode.Function.Outputs.Single());
        addNode.Add(connection2);

        OutputConnection connection3 = new(
            addNode,
            addNode.Function.Inputs[1],
            frameNoNode,
            frameNoNode.Function.Outputs.Single());
        addNode.Add(connection3);


        executor.Reset();
        executor.Run(graph);
        Assert.AreEqual(2, outputValue);
        executor.Run(graph);
        Assert.AreEqual(3, outputValue);

        Assert.Pass();
    }
}