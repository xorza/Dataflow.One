using System;
using System.Linq;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using NUnit.Framework;

namespace csso.NodeCore.Tests;

public class Tests {
    private readonly Function _addFunc = new("Add", F.Add);

    private readonly ValueFunc<Int32> _constFunc1 = new();
    private readonly ValueFunc<Int32> _constFunc2 = new();
    private readonly FrameNoFunc _frameNoFunc = new();
    private readonly OutputFunc<Int32> _outputFunc = new();
    private Node? _addNode;
    private Node? _constNode1;
    private Node? _constNode2;
    private Node? _frameNoNode;
    private Graph? _graph;
    private Node? _outputNode;


    [SetUp]
    public void Setup() {
        _graph = new Graph();

        _constFunc1.Value = 3;
        _constFunc2.Value = 1253;

        _constNode1 = _graph.AddNode(_constFunc1);
        _constNode2 = _graph.AddNode(_constFunc2);

        _frameNoNode = _graph.AddNode(_frameNoFunc);
        _addNode = _graph.AddNode(_addFunc);

        _outputNode = _graph.AddNode(_outputFunc);

        var once = new Event("Once");
        _outputNode.Add(once);

        var subscription = new EventSubscription(once, _outputNode);
        _graph.Add(subscription);

        _graph.Fire(once);
    }

    [Test]
    public void Test1() {
        _graph!.Add(
            new DataSubscription(
                _outputNode!,
                _outputNode!.Inputs.Single(),
                _constNode1!,
                _constNode1!.Outputs.Single())
        );

        var executor = new Executor(_graph!);
        _frameNoFunc.Executor = executor;

        executor.Run();

        var constEvaluationNode = executor.GetEvaluationNode(_constNode1!);

        Assert.AreEqual(3, _outputFunc.Value);
        Assert.True(constEvaluationNode.State >= EvaluationState.Invoked);

        _constFunc1.Value = 33;
        executor = new Executor(_graph!);
        _frameNoFunc.Executor = executor;
        executor.Run();
        Assert.AreEqual(33, _outputFunc.Value);

        _constFunc1.Value = 4;
        executor.Run();
        Assert.AreEqual(4, _outputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test2() {
        _graph!.Add(
            new DataSubscription(
                _outputNode!,
                _outputNode!.Inputs.Single(),
                _frameNoNode!,
                _frameNoNode!.Outputs.Single())
        );

        var executor = new Executor(_graph!);
        _frameNoFunc.Executor = executor;

        executor.Run();
        Assert.AreEqual(0, _outputFunc.Value);

        executor.Run();
        Assert.AreEqual(1, _outputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test3() {
        _graph!.Add(
            new DataSubscription(
                _outputNode!,
                _outputNode!.Inputs.Single(),
                _addNode!,
                _addNode!.Outputs.Single())
        );

        _graph.Add(
            new DataSubscription(
                _addNode,
                _addNode.Inputs[0],
                _constNode1!,
                _constNode1!.Outputs.Single())
        );

        _graph.Add(
            new DataSubscription(
                _addNode,
                _addNode.Inputs[1],
                _constNode2!,
                _constNode2!.Outputs.Single())
        );

        var executor = new Executor(_graph!);
        _frameNoFunc.Executor = executor;

        executor.Run();
        Assert.AreEqual(1256, _outputFunc.Value);

        executor.Run();
        Assert.AreEqual(1256, _outputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test4() {
        _graph!.Add(
            new DataSubscription(
                _outputNode!,
                _outputNode!.Inputs.Single(),
                _addNode!,
                _addNode!.Outputs.Single())
        );

        _graph.Add(
            new DataSubscription(
                _addNode,
                _addNode.Inputs[0],
                _constNode1!,
                _constNode1!.Outputs.Single())
        );

        _graph.Add(
            new DataSubscription(
                _addNode,
                _addNode.Inputs[1],
                _frameNoNode!,
                _frameNoNode!.Outputs.Single())
        );

        var executor = new Executor(_graph!);
        _frameNoFunc.Executor = executor;

        executor.Run();

        Assert.AreEqual(3, _outputFunc.Value);

        var addEvaluationNode = executor.GetEvaluationNode(_addNode);
        var frameNoEvaluationNode = executor.GetEvaluationNode(_frameNoNode);
        var constEvaluationNode = executor.GetEvaluationNode(_constNode1);
        Assert.True(frameNoEvaluationNode.State >= EvaluationState.Invoked);
        Assert.True(constEvaluationNode.State >= EvaluationState.Invoked);
        Assert.True(addEvaluationNode.State >= EvaluationState.Invoked);

        executor.Run();

        Assert.AreEqual(4, _outputFunc.Value);
        Assert.True(frameNoEvaluationNode.State >= EvaluationState.Invoked);
        Assert.True(constEvaluationNode.State >= EvaluationState.Invoked);
        Assert.True(addEvaluationNode.State >= EvaluationState.Invoked);

        Assert.Pass();
    }

    [Test]
    public void Test5() {
        var connection = new DataSubscription(
            _outputNode!,
            _outputNode!.Inputs.Single(),
            _addNode!,
            _addNode!.Outputs.Single());
        _graph!.Add(connection);
        connection.Behavior = SubscriptionBehavior.Once;

        _graph.Add(
            new DataSubscription(
                _addNode,
                _addNode.Inputs[0],
                _constNode1!,
                _constNode1!.Outputs.Single()));

        _graph.Add(
            new DataSubscription(
                _addNode,
                _addNode.Inputs[1],
                _frameNoNode!,
                _frameNoNode!.Outputs.Single()));

        var executor = new Executor(_graph!);
        _frameNoFunc.Executor = executor;

        executor.Run();

        Assert.AreEqual(3, _outputFunc.Value);

        var addEvaluationNode = executor.GetEvaluationNode(_addNode!);
        var outputEvaluationNode = executor.GetEvaluationNode(_outputNode!);
        Assert.True(addEvaluationNode.State >= EvaluationState.Invoked);
        Assert.True(outputEvaluationNode.State >= EvaluationState.Invoked);

        executor.Run();

        Assert.AreEqual(3, _outputFunc.Value);

        Assert.False(addEvaluationNode.State >= EvaluationState.Invoked);
        Assert.True(outputEvaluationNode.State >= EvaluationState.Invoked);

        Assert.Pass();
    }

    [Test]
    public void Test6() {
        _graph!.Add(
            new DataSubscription(
        _outputNode!,
            _outputNode!.Inputs.Single(),
            _constNode2!,
            _constNode2!.Outputs.Single()));

        var executor = new Executor(_graph!);
        _frameNoFunc.Executor = executor;

        _constFunc2.Value = 133;

        executor.Run();
        Assert.AreEqual(133, _outputFunc.Value);

        _constFunc2.Value = 132;
        executor.Run();
        Assert.AreEqual(132, _outputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test7() {
        _graph!.Add(
            new DataSubscription(
        _outputNode!,
            _outputNode!.Inputs.Single(),
            _addNode!,
            _addNode!.Outputs.Single()));
        
        _graph.Add(
            new DataSubscription(
        _addNode,
            _addNode.Inputs[0],
            _frameNoNode!,
            _frameNoNode!.Outputs.Single()));
        
        _graph.Add(
            new DataSubscription(
        _addNode,
            _addNode.Inputs[1],
            _frameNoNode!,
            _frameNoNode!.Outputs.Single()));

        var executor = new Executor(_graph!);
        _frameNoFunc.Executor = executor;

        executor.Run();

        Assert.AreEqual(0, _outputFunc.Value);

        var addEvaluationNode = executor.GetEvaluationNode(_addNode!);
        var outputEvaluationNode = executor.GetEvaluationNode(_outputNode!);
        Assert.True(addEvaluationNode.State >= EvaluationState.Invoked);
        Assert.True(outputEvaluationNode.State >= EvaluationState.Invoked);

        executor.Run();

        Assert.AreEqual(2, _outputFunc.Value);

        Assert.True(addEvaluationNode.State == EvaluationState.Invoked);
        Assert.True(outputEvaluationNode.State == EvaluationState.Invoked);

        Assert.Pass();
    }
}