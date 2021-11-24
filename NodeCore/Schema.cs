using System.Collections.Generic;

namespace csso.NodeCore {
public class Schema {
    public List<SchemaInput> Inputs { get; } = new();
    public List<SchemaOutput> Outputs { get; } = new();
    public string Name { get; set; } = "";
}
}