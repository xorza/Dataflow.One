using csso.NodeCore;
using NUnit.Framework;
using System;
using csso.Common;

namespace NodeCoreTest
{
    public class Tests
    {
        private readonly Schema _schema = new Schema();

        [SetUp]
        public void Setup()
        {
            _schema.Name = "RGB mix";
            _schema.Inputs.Add(
                new SchemaInput()
                {
                    Name = "R",
                    Type = typeof(Int32)
                });
            _schema.Inputs.Add(
                new SchemaInput()
                {
                    Name = "G",
                    Type = typeof(Int32)
                });
            _schema.Inputs.Add(
                new SchemaInput()
                {
                    Name = "B",
                    Type = typeof(Int32)
                });
            _schema.Outputs.Add(
               new SchemaOutput()
               {
                   Name = "RGB",
                   Type = typeof(Int32)
               });
        }

        [Test]
        public void Test1()
        {
            Graph graph = new Graph();
            Node node = new Node(_schema, graph);

            Assert.AreEqual(node.Graph, graph);
            Assert.AreEqual(node.Inputs.Count, _schema.Inputs.Count);

            node.Inputs.ForEach(input =>
            {
                Assert.AreSame(input.Node, node);
            });


            Assert.Pass();
        }

        [Test]
        public void Test2()
        {
            Graph graph = new Graph();
            Node node = new Node(_schema, graph);

            NoLoopValidator validator = new NoLoopValidator();
            validator.Go(graph);

            Assert.Pass();
        }

        [Test]
        public void Test3()
        {
            Graph graph = new Graph();
            Node node = new Node(_schema, graph);

            NoLoopValidator validator = new NoLoopValidator();


            //Assert.Throws(typeof(Exception), () => { validator.Go(graph); });

            Assert.Pass();
        }
    }
}