using System.Linq;
using csso.Common;
using csso.NodeCore.Run;
using NUnit.Framework;

namespace csso.NodeCore.Tests;

public class TwoNumbersSumTest {
    private TestGraph _graph = null!;
    private Executor _executor = null!;

    [SetUp]
    public void Setup() {
        _graph = new TestGraph();
        _graph.ConstFunc1.Value = 3;
        _graph.ConstFunc2.Value = 1253;

        _graph.Graph.Add(
            new DataSubscription(
                _graph.OutputNode.Inputs.Single(),
                _graph.AddNode.Outputs.Single())
        );

        _graph.Graph.Add(
            new DataSubscription(
                _graph.AddNode.Inputs[0],
                _graph.ConstNode1.Outputs.Single())
        );

        _graph.Graph.Add(new DataSubscription(
            _graph.AddNode.Inputs[1],
            _graph.ConstNode2.Outputs.Single())
        );

        _executor = new Executor(_graph.Graph);
        _graph.FrameNoFunc.Executor = _executor;
    }

    [Test] 
    public void simple_run() {
        _executor.Run();
        Assert.AreEqual(1256, _graph.OutputFunc.Value);

        var addEvaluationNode = _executor.GetEvaluationNode(_graph.AddNode);
        var frameNoEvaluationNode = _executor.GetEvaluationNode(_graph.FrameNoNode);
        var const1EvaluationNode = _executor.GetEvaluationNode(_graph.ConstNode1);
        Assert.True(frameNoEvaluationNode.State == EvaluationState.Idle);
        Assert.True(const1EvaluationNode.State == EvaluationState.Invoked);
        Assert.True(addEvaluationNode.State == EvaluationState.Invoked);

        Assert.Pass();
    }
    
    [Test] 
    public void not_invoked_before_run() {
        var addEvaluationNode = _executor.GetEvaluationNode(_graph.AddNode);
        var frameNoEvaluationNode = _executor.GetEvaluationNode(_graph.FrameNoNode);
        var const1EvaluationNode = _executor.GetEvaluationNode(_graph.ConstNode1);
        Assert.True(frameNoEvaluationNode.State == EvaluationState.Idle);
        Assert.True(const1EvaluationNode.State == EvaluationState.Idle);
        Assert.True(addEvaluationNode.State == EvaluationState.Idle);

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
        Assert.AreEqual(3, _graph.OutputFunc.Value);

        _executor.Run();
        Assert.AreEqual(4, _graph.OutputFunc.Value);

        var addEvaluationNode = _executor.GetEvaluationNode(_graph.AddNode);
        var frameNoEvaluationNode = _executor.GetEvaluationNode(_graph.FrameNoNode);
        var const2EvaluationNode = _executor.GetEvaluationNode(_graph.ConstNode2);
        Assert.True(frameNoEvaluationNode.State == EvaluationState.Invoked);
        Assert.True(const2EvaluationNode.State == EvaluationState.Idle);
        Assert.True(addEvaluationNode.State == EvaluationState.Invoked);

        Assert.Pass();
    }

    [Test] 
    public void remove_datasubscription_and_expect_exception() {
        Assert.AreEqual(3, _graph.Graph.DataSubscriptions.Count);
        _graph.Graph.RemoveSubscription(_graph.AddNode.Inputs[1]);
        Assert.AreEqual(2, _graph.Graph.DataSubscriptions.Count);

        Assert.Throws<ArgumentMissingException>(() => _executor.Run());

        Assert.Pass();
    }
}