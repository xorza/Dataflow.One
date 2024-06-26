﻿using System.Linq;
using csso.NodeCore.Run;
using NUnit.Framework;

namespace csso.NodeCore.Tests;

public class TwoNumbersSumTest {
    private Executor _executor = null!;
    private TestGraph _graph = null!;

    [SetUp]
    public void Setup() {
        _graph = new TestGraph();
        _graph.ReactiveConstFuncBaseBase.TypedValue = 3;
        _graph.ProactiveConstFuncBaseBase.TypedValue = 1253;

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

        _graph.Graph.Add(new DataSubscription(
            _graph.AddNode.Inputs[1],
            _graph.ProactiveConstNode.Outputs.Single())
        );

        _executor = new Executor(_graph.Graph);
        _graph.FrameNoFunc.Executor = _executor;
    }

    [Test]
    public void first_run() {
        _executor.Run();
        Assert.That(1256 == _graph.OutputFunc.Value);

        Assert.That(_executor.GetEvaluationNode(_graph.ProactiveConstNode).State == EvaluationState.Invoked);
        Assert.That(_executor.GetEvaluationNode(_graph.ReactiveConstNode).State == EvaluationState.Invoked);
        Assert.That(_executor.GetEvaluationNode(_graph.AddNode).State == EvaluationState.Invoked);
        Assert.That(_executor.GetEvaluationNode(_graph.FrameNoNode).State == EvaluationState.Idle);

        Assert.Pass();
    }

    [Test]
    public void prewarmed_subscription_change() {
        _executor.Run();

        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[1],
                _graph.ReactiveConstNode.Outputs.Single())
        );

        _executor.Run();
        Assert.That(6 == _graph.OutputFunc.Value);

        Assert.That(EvaluationState.Idle == _executor.GetEvaluationNode(_graph.ProactiveConstNode).State);
        Assert.That(EvaluationState.Processed == _executor.GetEvaluationNode(_graph.ReactiveConstNode).State);
        Assert.That(EvaluationState.Invoked == _executor.GetEvaluationNode(_graph.AddNode).State);
        Assert.That(EvaluationState.Idle == _executor.GetEvaluationNode(_graph.FrameNoNode).State);

        Assert.Pass();
    }

    [Test]
    public void run_subscription_change_second_run() {
        _executor.Run();

        fully_reactive_graph_second_run();
    }

    [Test]
    public void fully_reactive_graph_first_run() {
        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[1],
                _graph.ReactiveConstNode.Outputs.Single())
        );

        _executor.Run();
        Assert.That(6 == _graph.OutputFunc.Value);

        Assert.That(EvaluationState.Idle == _executor.GetEvaluationNode(_graph.ProactiveConstNode).State);
        Assert.That(EvaluationState.Invoked == _executor.GetEvaluationNode(_graph.ReactiveConstNode).State);
        Assert.That(EvaluationState.Invoked == _executor.GetEvaluationNode(_graph.AddNode).State);
        Assert.That(EvaluationState.Idle == _executor.GetEvaluationNode(_graph.FrameNoNode).State);

        Assert.Pass();
    }

    [Test]
    public void fully_reactive_graph_second_run() {
        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[1],
                _graph.ReactiveConstNode.Outputs.Single())
        );


        _executor.Run();

        _executor.Run();
        Assert.That(6 == _graph.OutputFunc.Value);

        Assert.That(EvaluationState.Idle == _executor.GetEvaluationNode(_graph.ProactiveConstNode).State);
        Assert.That(EvaluationState.Processed == _executor.GetEvaluationNode(_graph.ReactiveConstNode).State);
        Assert.That(EvaluationState.Processed == _executor.GetEvaluationNode(_graph.AddNode).State);
        Assert.That(EvaluationState.Idle == _executor.GetEvaluationNode(_graph.FrameNoNode).State);

        Assert.Pass();
    }

    [Test]
    public void second_and_third_run() {
        _executor.Run();

        for (var i = 0; i < 2; i++) {
            _executor.Run();
            Assert.That(1256 == _graph.OutputFunc.Value);


            Assert.That(_executor.GetEvaluationNode(_graph.ProactiveConstNode).State == EvaluationState.Invoked);
            Assert.That(_executor.GetEvaluationNode(_graph.ReactiveConstNode).State == EvaluationState.Processed);
            Assert.That(_executor.GetEvaluationNode(_graph.AddNode).State == EvaluationState.Invoked);
            Assert.That(_executor.GetEvaluationNode(_graph.FrameNoNode).State == EvaluationState.Idle);
        }

        Assert.Pass();
    }

    [Test]
    public void not_invoked_before_run() {
        Assert.That(_executor.GetEvaluationNode(_graph.ProactiveConstNode).State == EvaluationState.Idle);
        Assert.That(_executor.GetEvaluationNode(_graph.ReactiveConstNode).State == EvaluationState.Idle);
        Assert.That(_executor.GetEvaluationNode(_graph.AddNode).State == EvaluationState.Idle);
        Assert.That(_executor.GetEvaluationNode(_graph.FrameNoNode).State == EvaluationState.Idle);

        Assert.Pass();
    }

    [Test]
    public void subscribe_to_a_different_node() {
        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[1],
                _graph.FrameNoNode.Outputs.Single())
        );

        _executor.Run();
        Assert.That(3 == _graph.OutputFunc.Value);

        _executor.Run();
        Assert.That(4 == _graph.OutputFunc.Value);

        var addEvaluationNode = _executor.GetEvaluationNode(_graph.AddNode);
        var frameNoEvaluationNode = _executor.GetEvaluationNode(_graph.FrameNoNode);
        var const2EvaluationNode = _executor.GetEvaluationNode(_graph.ProactiveConstNode);
        Assert.That(frameNoEvaluationNode.State == EvaluationState.Invoked);
        Assert.That(const2EvaluationNode.State == EvaluationState.Idle);
        Assert.That(addEvaluationNode.State == EvaluationState.Invoked);

        Assert.Pass();
    }

    [Test]
    public void remove_datasubscription_and_expect_exception() {
        Assert.That(3 == _graph.Graph.DataSubscriptions.Count);
        _graph.Graph.RemoveSubscription(_graph.AddNode.Inputs[1]);
        Assert.That(2 == _graph.Graph.DataSubscriptions.Count);

        Assert.Throws<ArgumentMissingException>(() => _executor.Run());

        Assert.Pass();
    }
}