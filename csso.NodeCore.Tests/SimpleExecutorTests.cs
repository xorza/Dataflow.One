using System;
using System.Linq;
using csso.NodeCore.Funcs;
using csso.NodeCore.Run;
using NUnit.Framework;

namespace csso.NodeCore.Tests;

public class Tests {
    private TestGraph _graph = null!;

    [SetUp]
    public void Setup() {
        _graph = new TestGraph();
        _graph.ReactiveConstFuncBaseBase.TypedValue = 3;
        _graph.ProactiveConstFuncBaseBase.TypedValue = 1253;
    }

    [Test]
    public void Test1() {
        _graph.Graph.Add(
            new DataSubscription(
                _graph.OutputNode.Inputs.Single(),
                _graph.ProactiveConstNode.Outputs.Single())
        );

        var executor = new Executor(_graph.Graph);
        _graph.FrameNoFunc.Executor = executor;

        executor.Run();

        var proactiveConstNode = executor.GetEvaluationNode(_graph.ProactiveConstNode);

        Assert.AreEqual(1253, _graph.OutputFunc.Value);
        Assert.True(proactiveConstNode.State >= EvaluationState.Invoked);

        _graph.ProactiveConstFuncBaseBase.TypedValue = 33;
        executor = new Executor(_graph.Graph);
        _graph.FrameNoFunc.Executor = executor;
        executor.Run();
        Assert.AreEqual(33, _graph.OutputFunc.Value);

        _graph.ProactiveConstFuncBaseBase.TypedValue = 4;
        executor.Run();
        Assert.AreEqual(4, _graph.OutputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test2() {
        _graph.Graph!.Add(
            new DataSubscription(
                _graph.OutputNode.Inputs.Single(),
                _graph.FrameNoNode.Outputs.Single())
        );

        var executor = new Executor(_graph.Graph!);
        _graph.FrameNoFunc.Executor = executor;

        executor.Run();
        Assert.AreEqual(0, _graph.OutputFunc.Value);

        executor.Run();
        Assert.AreEqual(1, _graph.OutputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test3() {
        _graph.Graph!.Add(
            new DataSubscription(
                _graph.OutputNode.Inputs.Single(),
                _graph.AddNode.Outputs.Single())
        );

        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[0],
                _graph.ReactiveConstNode.Outputs.Single())
        );

        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[1],
                _graph.ProactiveConstNode.Outputs.Single())
        );

        var executor = new Executor(_graph.Graph!);
        _graph.FrameNoFunc.Executor = executor;

        executor.Run();
        Assert.AreEqual(1256, _graph.OutputFunc.Value);

        executor.Run();
        Assert.AreEqual(1256, _graph.OutputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test4() {
        _graph.Graph.Add(
            new DataSubscription(
                _graph.OutputNode.Inputs.Single(),
                _graph.AddNode.Outputs.Single())
        );

        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[0],
                _graph.ReactiveConstNode.Outputs.Single())
        );

        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[1],
                _graph.FrameNoNode!.Outputs.Single())
        );

        var executor = new Executor(_graph.Graph);
        _graph.FrameNoFunc.Executor = executor;

        executor.Run();

        Assert.AreEqual(3, _graph.OutputFunc.Value);

        var addEvaluationNode = executor.GetEvaluationNode(_graph.AddNode);
        var frameNoEvaluationNode = executor.GetEvaluationNode(_graph.FrameNoNode);
        var reactiveConstNode = executor.GetEvaluationNode(_graph.ReactiveConstNode);
        Assert.True(frameNoEvaluationNode.State == EvaluationState.Invoked);
        Assert.True(reactiveConstNode.State == EvaluationState.Invoked);
        Assert.True(addEvaluationNode.State == EvaluationState.Invoked);

        executor.Run();

        Assert.AreEqual(4, _graph.OutputFunc.Value);
        Assert.AreEqual(EvaluationState.Invoked,   frameNoEvaluationNode.State);
        Assert.AreEqual(EvaluationState.Processed, reactiveConstNode.State);
        Assert.AreEqual(EvaluationState.Invoked,   addEvaluationNode.State);

        Assert.Pass();
    }

    [Test]
    public void Test5() {
        var connection = new DataSubscription(
            _graph.OutputNode.Inputs.Single(),
            _graph.AddNode.Outputs.Single());
        _graph.Graph.Add(connection);
        connection.Behavior = SubscriptionBehavior.Once;

        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[0],
                _graph.ReactiveConstNode.Outputs.Single()));

        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[1],
                _graph.FrameNoNode.Outputs.Single()));

        var executor = new Executor(_graph.Graph);
        _graph.FrameNoFunc.Executor = executor;

        executor.Run();

        Assert.AreEqual(3, _graph.OutputFunc.Value);

        var addEvaluationNode = executor.GetEvaluationNode(_graph.AddNode!);
        var outputEvaluationNode = executor.GetEvaluationNode(_graph.OutputNode);
        Assert.True(addEvaluationNode.State >= EvaluationState.Invoked);
        Assert.True(outputEvaluationNode.State >= EvaluationState.Invoked);

        executor.Run();

        Assert.AreEqual(3, _graph.OutputFunc.Value);

        Assert.False(addEvaluationNode.State >= EvaluationState.Invoked);
        Assert.True(outputEvaluationNode.State >= EvaluationState.Invoked);

        Assert.Pass();
    }

    [Test]
    public void Test6() {
        _graph.Graph.Add(
            new DataSubscription(
                _graph.OutputNode.Inputs.Single(),
                _graph.ProactiveConstNode.Outputs.Single()));

        var executor = new Executor(_graph.Graph);
        _graph.FrameNoFunc.Executor = executor;

        _graph.ProactiveConstFuncBaseBase.TypedValue = 133;

        executor.Run();
        Assert.AreEqual(133, _graph.OutputFunc.Value);

        _graph.ProactiveConstFuncBaseBase.TypedValue = 132;
        executor.Run();
        Assert.AreEqual(132, _graph.OutputFunc.Value);

        Assert.Pass();
    }

    [Test]
    public void Test7() {
        _graph.Graph.Add(
            new DataSubscription(
                _graph.OutputNode.Inputs.Single(),
                _graph.AddNode.Outputs.Single()));

        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[0],
                _graph.FrameNoNode!.Outputs.Single()));

        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[1],
                _graph.FrameNoNode.Outputs.Single()));

        var executor = new Executor(_graph.Graph);
        _graph.FrameNoFunc.Executor = executor;

        executor.Run();

        Assert.AreEqual(0, _graph.OutputFunc.Value);

        var addEvaluationNode = executor.GetEvaluationNode(_graph.AddNode!);
        var outputEvaluationNode = executor.GetEvaluationNode(_graph.OutputNode);
        Assert.True(addEvaluationNode.State >= EvaluationState.Invoked);
        Assert.True(outputEvaluationNode.State >= EvaluationState.Invoked);

        executor.Run();

        Assert.AreEqual(2, _graph.OutputFunc.Value);

        Assert.True(addEvaluationNode.State == EvaluationState.Invoked);
        Assert.True(outputEvaluationNode.State == EvaluationState.Invoked);

        Assert.Pass();
    }
}