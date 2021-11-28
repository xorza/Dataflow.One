using csso.Common;
using csso.NodeCore;
using NUnit.Framework;

namespace NodeCoreTest {
public class Tests {
    // private readonly Function _function = new();

    [SetUp]
    public void Setup() {
        // _function.Name = "RGB mix";
        // _function.Inputs.Add(
        //     new FunctionInput {
        //         Name = "R",
        //         Type = typeof(int)
        //     });
        // _function.Inputs.Add(
        //     new FunctionInput {
        //         Name = "G",
        //         Type = typeof(int)
        //     });
        // _function.Inputs.Add(
        //     new FunctionInput {
        //         Name = "B",
        //         Type = typeof(int)
        //     });
        // _function.Outputs.Add(
        //     new FunctionOutput {
        //         Name = "RGB",
        //         Type = typeof(int)
        //     });
    }

    [Test]
    public void Test1() {
        // Graph graph = new();
        // Node node = new(_function, graph);
        //
        // Assert.AreEqual(node.Graph, graph);
        // Assert.AreEqual(node.Connections.Count, _function.Inputs.Count);
        //
        // node.Connections.Foreach(input => { Assert.AreSame(input.InputNode, node); });
        //
        //
        // Assert.Pass();
    }

    [Test]
    public void Test2() {
        // Graph graph = new();
        // Node node = new(_function, graph);
        //
        // NoLoopValidator validator = new();
        // validator.Go(graph);
        //
        // Assert.Pass();
    }

    [Test]
    public void Test3() {
        // Graph graph = new();
        // Node node = new(_function, graph);
        //
        // NoLoopValidator validator = new();
        //
        //
        // //Assert.Throws(typeof(Exception), () => { validator.Go(graph); });
        //
        // Assert.Pass();
    }
}
}