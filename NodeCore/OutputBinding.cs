using System;

namespace csso.NodeCore {
public class OutputBinding : Binding {
    public OutputBinding(Node inputNode,
        SchemaInput input,
        Node outputNode,
        SchemaOutput output) :
        base(inputNode, input) {
        OutputNode = outputNode;
        Output = output;

        if (input.Type != output.Type) throw new Exception("type mismatch");
    }

    public Node OutputNode { get; }
    public SchemaOutput Output { get; }
}
}