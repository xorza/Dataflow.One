using csso.NodeCore;
using NUnit.Framework;

namespace NodeCoreTest {
public class Tests2 {
    private readonly Graph _graph = new();
    private readonly Schema _schemaBitmap = new();
    private readonly Schema _schemaRgbMix = new();
    private readonly Schema _schemaRgbSplit = new();

    [SetUp]
    public void Setup() {
        _schemaRgbMix.Name = "RGB mix";
        _schemaRgbMix.Inputs.Add(new SchemaInput("R", typeof(int)));
        _schemaRgbMix.Inputs.Add(new SchemaInput("G", typeof(int)));
        _schemaRgbMix.Inputs.Add(new SchemaInput("B", typeof(int)));
        _schemaRgbMix.Outputs.Add(new SchemaOutput("RGB", typeof(int)));

        _schemaRgbSplit.Name = "RGB split";
        _schemaRgbSplit.Inputs.Add(new SchemaInput("RGB", typeof(int)));
        _schemaRgbSplit.Outputs.Add(new SchemaOutput("R", typeof(int)));
        _schemaRgbSplit.Outputs.Add(new SchemaOutput("G", typeof(int)));
        _schemaRgbSplit.Outputs.Add(new SchemaOutput("B", typeof(int)));

        _schemaBitmap.Name = "Bitmap";
        _schemaBitmap.Outputs.Add(new SchemaOutput("R", typeof(int)));
        _schemaBitmap.Outputs.Add(new SchemaOutput("G", typeof(int)));
        _schemaBitmap.Outputs.Add(new SchemaOutput("B", typeof(int)));
        _schemaBitmap.Outputs.Add(new SchemaOutput("RGB", typeof(int)));

        Node bitmap = new(_schemaBitmap, _graph);
        Node rgbSplit = new(_schemaRgbSplit, _graph);
        Node rgbMix = new(_schemaRgbMix, _graph);
    }

    [Test]
    public void Test1() { }
}
}