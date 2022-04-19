using System.Linq;
using csso.Common;
using csso.NodeCore.Run;
using NUnit.Framework;

namespace csso.NodeCore.Tests;

public class TwoNumbersSumTest {
    [SetUp]
    public void SetUp() { }


    [Test]
    public void Test1() {
        var graph = new TestGraph();
        graph.ConstFunc1.Value = 3;
        graph.ConstFunc2.Value = 1253;

        graph.Graph.Add(
            new DataSubscription(
                graph.OutputNode!,
                graph.OutputNode.Inputs.Single(),
                graph.AddNode,
                graph.AddNode.Outputs.Single())
        );

        graph.Graph.Add(
            new DataSubscription(
                graph.AddNode,
                graph.AddNode.Inputs[0],
                graph.ConstNode1,
                graph.ConstNode1.Outputs.Single())
        );

        var addToConst2Sub = new DataSubscription(
            graph.AddNode,
            graph.AddNode.Inputs[1],
            graph.ConstNode2,
            graph.ConstNode2.Outputs.Single());
        graph.Graph.Add(addToConst2Sub);

        var executor = new Executor(graph.Graph!);
        graph.FrameNoFunc.Executor = executor;


        executor.Run();
        Assert.AreEqual(1256, graph.OutputFunc.Value);

        var addEvaluationNode = executor.GetEvaluationNode(graph.AddNode);
        var frameNoEvaluationNode = executor.GetEvaluationNode(graph.FrameNoNode);
        var constEvaluationNode = executor.GetEvaluationNode(graph.ConstNode1);
        Assert.True(frameNoEvaluationNode.State == EvaluationState.Idle);
        Assert.True(constEvaluationNode.State == EvaluationState.Invoked);
        Assert.True(addEvaluationNode.State >= EvaluationState.Invoked);


        graph.Graph.Remove(addToConst2Sub);
        graph.Graph.Add(
            new DataSubscription(
                graph.AddNode,
                graph.AddNode.Inputs[0],
                graph.FrameNoNode,
                graph.FrameNoNode.Outputs.Single())
        );
        
        executor.Run();
        Assert.AreEqual(1256, graph.OutputFunc.Value);


        Assert.Pass();
    }
}