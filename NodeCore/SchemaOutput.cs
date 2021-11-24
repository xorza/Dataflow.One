using System;

namespace csso.NodeCore {
public class SchemaOutput : SchemaPut {
    public SchemaOutput() { }
    public SchemaOutput(string name, Type type) : base(name, type) { }

    public override PutType PutType => PutType.Out;
}
}