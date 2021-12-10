using System;
using System.Linq;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using NUnit.Framework;

namespace csso.Calculator.Tests; 

public class Tests {
    [System.ComponentModel.Description("value")]
    [Reactive]
    private static bool Const([Config(12)] Int32 c, [Output] ref Int32 i) {
        i = c;
        return true;
    }


    [SetUp]
    public void Setup() { }

    [Test]
    public void Test1() {
        var output1Value = 0;

        var graph = new Graph();
        var executor = new Executor(graph);

        Function constFunc = new Function("Value", Const);
        constFunc.Config.Single().DefaultValue = 13;
        Function outputFunc = new Function("Output", new Func<Int32, bool>(val => {
            output1Value = val;
            return true;
        }));


        Node constNode = new(constFunc, graph);
        graph.Add(constNode);
        Node outputNode = new(outputFunc, graph);
        graph.Add(outputNode);

        OutputConnection connection1 = new(
            outputNode,
            outputNode.Function.Inputs.Single(),
            constNode,
            constNode.Function.Outputs.Single());
        outputNode.AddConnection(connection1);


        executor.Reset();
        executor.Run();
        Assert.AreEqual(13, output1Value);

        constNode.ConfigValues.Single().Value = 1253;
        executor.Reset();
        executor.Run();

        Assert.AreEqual(output1Value, 1253);


        Assert.Pass();
    }

    [Test]
    public void Test2() {
        Graph graph;
        Executor executor;
        Node outputNode;

        var outputValue = 0;

        graph = new Graph();
        executor = new Executor(graph);
        Function outputFunc = new Function("Output", (Int32 val) => {
            outputValue = val;
            return true;
        });

        outputNode = new Node(outputFunc, graph);
        graph.Add(outputNode);

        Node frameNoNode = new(executor.FrameNoFunction, graph);
        graph.Add(frameNoNode);

        OutputConnection connection = new(
            outputNode,
            outputNode.Function.Inputs.Single(),
            frameNoNode,
            frameNoNode.Function.Outputs.Single());
        outputNode.AddConnection(connection);


        executor.Reset();
        executor.Run();
        Assert.AreEqual(0, outputValue);
        executor.Run();
        Assert.AreEqual(1, outputValue);

        Assert.Pass();
    }


    [Test]
    public void Test3() {
        var outputValue = 0;

        var graph = new Graph();
        var executor = new Executor(graph);

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

        Node const1Node = new(constFunc, graph);
        graph.Add(const1Node);

        Node const2Node = new(constFunc, graph);
        graph.Add(const2Node);

        const2Node.ConfigValues.Single().Value = 1253;

        OutputConnection connection = new(
            outputNode,
            outputNode.Function.Inputs.Single(),
            addNode,
            addNode.Function.Outputs.Single());
        outputNode.AddConnection(connection);


        OutputConnection connection2 = new(
            addNode,
            addNode.Function.Inputs[0],
            const1Node,
            const1Node.Function.Outputs.Single());
        addNode.AddConnection(connection2);

        OutputConnection connection3 = new(
            addNode,
            addNode.Function.Inputs[1],
            const2Node,
            const2Node.Function.Outputs.Single());
        addNode.AddConnection(connection3);


        executor.Reset();
        executor.Run();
        Assert.AreEqual(1256, outputValue);
        executor.Run();
        Assert.AreEqual(1256, outputValue);

        Assert.Pass();
    }

    [Test]
    public void Test4() {
        var outputValue = 0;

        var graph = new Graph();
        var executor = new Executor(graph);

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
        outputNode.AddConnection(connection);


        OutputConnection connection2 = new(
            addNode,
            addNode.Function.Inputs[0],
            constNode,
            constNode.Function.Outputs.Single());
        addNode.AddConnection(connection2);

        OutputConnection connection3 = new(
            addNode,
            addNode.Function.Inputs[1],
            frameNoNode,
            frameNoNode.Function.Outputs.Single());
        addNode.AddConnection(connection3);


        executor.Reset();
        executor.Run();
        Assert.AreEqual(outputValue, 2);
        executor.Run();
        Assert.AreEqual(outputValue, 3);

        Assert.Pass();
    }

    [Test]
    public void Test5() {
        var outputValue = 0;

        var graph = new Graph();
        var executor = new Executor(graph);

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
        outputNode.AddConnection(connection);


        OutputConnection connection2 = new(
            addNode,
            addNode.Function.Inputs[0],
            constNode,
            constNode.Function.Outputs.Single());
        addNode.AddConnection(connection2);

        OutputConnection connection3 = new(
            addNode,
            addNode.Function.Inputs[1],
            frameNoNode,
            frameNoNode.Function.Outputs.Single());
        addNode.AddConnection(connection3);


        executor.Reset();
        executor.Run();
        Assert.AreEqual(2, outputValue);
        executor.Run();
        Assert.AreEqual(2, outputValue);

        Assert.Pass();
    }


    [Test]
    public void Test6() {
        var outputValue = 0;

        var graph = new Graph();
        var executor = new Executor(graph);

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
        outputNode.AddConnection(connection);


        OutputConnection connection2 = new(
            addNode,
            addNode.Function.Inputs[0],
            constNode,
            constNode.Function.Outputs.Single());
        addNode.AddConnection(connection2);

        OutputConnection connection3 = new(
            addNode,
            addNode.Function.Inputs[1],
            frameNoNode,
            frameNoNode.Function.Outputs.Single());
        addNode.AddConnection(connection3);


        executor.Reset();
        executor.Run();
        Assert.AreEqual(2, outputValue);
        executor.Run();
        Assert.AreEqual(3, outputValue);

        Assert.Pass();
    }
}