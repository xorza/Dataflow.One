using System;
using System.Linq;
using csso.Calculator;
using csso.NodeCore;
using csso.NodeCore.Funcs;
using NUnit.Framework;

namespace csso.Calculator.Tests {
public partial class Tests {
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
        Int32 output1Value = 0;

        csso.NodeCore.Graph graph = new csso.NodeCore.Graph();
        csso.Calculator.Executor executor = new Executor(graph);

        IFunction constFunc = new Function("Value", Const);
        constFunc.Config.Single().DefaultValue = 13;
        IFunction outputFunc = new Function("Output", new Func<Int32, bool>((Int32 val) => {
            output1Value = val;
            return true;
        }));


        csso.NodeCore.Node constNode = new(constFunc, graph);
        graph.Add(constNode);
        csso.NodeCore.Node outputNode = new(outputFunc, graph);
        graph.Add(outputNode);

        OutputConnection connection1 = new(
            outputNode,
            outputNode.Function.Inputs.Single(),
            constNode,
            constNode.Function.Outputs.Single());
        outputNode.AddBinding(connection1);


        executor.Reset();
        executor.Run();
        Assert.AreEqual(output1Value, 13);

        constNode.ConfigValues.Single().Value = 1253;
        executor.Reset();
        executor.Run();

        Assert.AreEqual(output1Value, 1253);


        Assert.Pass();
    }

    [Test]
    public void Test2() {
        csso.NodeCore.Graph graph;
        csso.Calculator.Executor executor;
        csso.NodeCore.Node outputNode;

        Int32 outputValue = 0;

        graph = new csso.NodeCore.Graph();
        executor = new Executor(graph);
        IFunction outputFunc = new Function("Output", (Int32 val) => {
            outputValue = val;
            return true;
        });

        outputNode = new(outputFunc, graph);
        graph.Add(outputNode);

        Node frameNoNode = new(executor.FrameNoFunction, graph);
        graph.Add(frameNoNode);

        OutputConnection connection = new(
            outputNode,
            outputNode.Function.Inputs.Single(),
            frameNoNode,
            frameNoNode.Function.Outputs.Single());
        outputNode.AddBinding(connection);


        executor.Reset();
        executor.Run();
        Assert.AreEqual(outputValue, 0);
        executor.Run();
        Assert.AreEqual(outputValue, 1);

        Assert.Pass();
    }


    [Test]
    public void Test3() {
        Int32 outputValue = 0;

        csso.NodeCore.Graph graph = new csso.NodeCore.Graph();
        csso.Calculator.Executor  executor = new Executor(graph);
        
        IFunction outputFunc = new Function("Output", (Int32 val) => {
            outputValue = val;
            return true;
        });
        IFunction constFunc = new Function("Value", Const);
        IFunction addFunc = new Function("Value", F.Add);
        
        
        constFunc.Config.Single().DefaultValue = 3;

        csso.NodeCore.Node  outputNode = new(outputFunc, graph);
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
        outputNode.AddBinding(connection);
        
        
        OutputConnection connection2 = new(
            addNode,
            addNode.Function.Inputs[0],
            const1Node,
            const1Node.Function.Outputs.Single());
        addNode.AddBinding(connection2);
        
        OutputConnection connection3 = new(
            addNode,
            addNode.Function.Inputs[1],
            const2Node,
            const2Node.Function.Outputs.Single());
        addNode.AddBinding(connection3);


        executor.Reset();
        executor.Run();
        Assert.AreEqual(outputValue, 1256);
        executor.Run();
        Assert.AreEqual(outputValue, 1256);

        Assert.Pass();
    }
    
    [Test]
    public void Test4() {
        Int32 outputValue = 0;

        csso.NodeCore.Graph graph = new csso.NodeCore.Graph();
        csso.Calculator.Executor  executor = new Executor(graph);
        
        IFunction outputFunc = new Function("Output", (Int32 val) => {
            outputValue = val;
            return true;
        });
        IFunction constFunc = new Function("Value", Const);
        IFunction addFunc = new Function("Value", F.Add);
        
        
        constFunc.Config.Single().DefaultValue = 3;

        csso.NodeCore.Node  outputNode = new(outputFunc, graph);
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
        outputNode.AddBinding(connection);
        
        
        OutputConnection connection2 = new(
            addNode,
            addNode.Function.Inputs[0],
            constNode,
            constNode.Function.Outputs.Single());
        addNode.AddBinding(connection2);
        
        OutputConnection connection3 = new(
            addNode,
            addNode.Function.Inputs[1],
            frameNoNode,
            frameNoNode.Function.Outputs.Single());
        addNode.AddBinding(connection3);


        executor.Reset();
        executor.Run();
        Assert.AreEqual(outputValue, 2);
        executor.Run();
        Assert.AreEqual(outputValue, 3);

        Assert.Pass();
    }
}
}