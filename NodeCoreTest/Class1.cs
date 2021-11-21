using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csso.NodeCore;
using NUnit.Framework;

namespace NodeCoreTest
{
    public class Tests2
    {
        private readonly Schema _schemaRgbMix = new Schema();
        private readonly Schema _schemaRgbSplit = new Schema();
        private readonly Schema _schemaBitmap = new Schema();
        private readonly Graph _graph = new Graph();

        [SetUp]
        public void Setup()
        {
            _schemaRgbMix.Name = "RGB mix";
            _schemaRgbMix.Inputs.Add(new SchemaInput("R", typeof(Int32)));
            _schemaRgbMix.Inputs.Add(new SchemaInput("G", typeof(Int32)));
            _schemaRgbMix.Inputs.Add(new SchemaInput("B", typeof(Int32)));
            _schemaRgbMix.Outputs.Add(new SchemaOutput("RGB", typeof(Int32)));

            _schemaRgbSplit.Name = "RGB split";
            _schemaRgbSplit.Inputs.Add(new SchemaInput("RGB", typeof(Int32)));
            _schemaRgbSplit.Outputs.Add(new SchemaOutput("R", typeof(Int32)));
            _schemaRgbSplit.Outputs.Add(new SchemaOutput("G", typeof(Int32)));
            _schemaRgbSplit.Outputs.Add(new SchemaOutput("B", typeof(Int32)));

            _schemaBitmap.Name = "Bitmap";
            _schemaBitmap.Outputs.Add(new SchemaOutput("R", typeof(Int32)));
            _schemaBitmap.Outputs.Add(new SchemaOutput("G", typeof(Int32)));
            _schemaBitmap.Outputs.Add(new SchemaOutput("B", typeof(Int32)));
            _schemaBitmap.Outputs.Add(new SchemaOutput("RGB", typeof(Int32)));

            Node bitmap = new Node(_schemaBitmap, _graph);
            Node rgbSplit = new Node(_schemaRgbSplit, _graph);
            Node rgbMix = new Node(_schemaRgbMix, _graph);



        }

        [Test]
        public void Test1()
        {

        }
    }
}
