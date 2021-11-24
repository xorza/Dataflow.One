using System;

namespace csso.NodeCore {
public class SchemaInput : SchemaPut {
    public SchemaInput() { }
    public SchemaInput(string name, Type type) : base(name, type) { }

    public override PutType PutType => PutType.In;
}
}