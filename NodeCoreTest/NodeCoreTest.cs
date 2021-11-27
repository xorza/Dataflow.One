using csso.Common;
using csso.NodeCore;
using NUnit.Framework;

namespace NodeCoreTest {
public class Tests {
    private readonly Schema _schema = new();

    [SetUp]
    public void Setup() {
        _schema.Name = "RGB mix";
        _schema.Inputs.Add(
            new SchemaInput {
                Name = "R",
                Type = typeof(int)
            });
        _schema.Inputs.Add(
            new SchemaInput {
                Name = "G",
                Type = typeof(int)
            });
        _schema.Inputs.Add(
            new SchemaInput {
                Name = "B",
                Type = typeof(int)
            });
        _schema.Outputs.Add(
            new SchemaOutput {
                Name = "RGB",
                Type = typeof(int)
            });
    }

    [Test]
    public void Test1() {
        Graph graph = new();
        Node node = new(_schema, graph);

        Assert.AreEqual(node.Graph, graph);
        Assert.AreEqual(node.Inputs.Count, _schema.Inputs.Count);

        node.Inputs.Foreach(input => { Assert.AreSame(input.InputNode, node); });


        Assert.Pass();
    }

    [Test]
    public void Test2() {
        Graph graph = new();
        Node node = new(_schema, graph);

        NoLoopValidator validator = new();
        validator.Go(graph);

        Assert.Pass();
    }

    [Test]
    public void Test3() {
        Graph graph = new();
        Node node = new(_schema, graph);

        NoLoopValidator validator = new();


        //Assert.Throws(typeof(Exception), () => { validator.Go(graph); });

        Assert.Pass();
    }
}
}