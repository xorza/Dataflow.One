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

    [Test] // sum of 1253 and 3 
    public void Test1() {
        _executor.Run();
        Assert.AreEqual(1256, _graph.OutputFunc.Value);
        
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
    
    [Test] // subscribe to frameno instead of 1253, then sum it with 3
    public void Test3() {
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

    [Test] //unsubscribe and expect exception
    public void Test4() {
        Assert.AreEqual(3, _graph.Graph.DataSubscriptions.Count);
        _graph.Graph.RemoveSubscription(_graph.AddNode.Inputs[1]);
        Assert.AreEqual(2, _graph.Graph.DataSubscriptions.Count);

        Assert.Throws<ArgumentMissingException>(() => _executor.Run());

        Assert.Pass();
    }
}