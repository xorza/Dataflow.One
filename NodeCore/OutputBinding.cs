using System;
using csso.Common;

namespace csso.NodeCore {
public class OutputBinding : Binding {
    public OutputBinding(Node inputNode,
        SchemaInput input,
        Node outputNode,
        SchemaOutput output) :
        base(inputNode, input) {
        OutputNode = outputNode;
        Output = output;
        
        Check.True(inputNode.Schema.Inputs.Contains(input));
        Check.True(outputNode.Schema.Outputs.Contains(output));

        if (input.Type != output.Type) throw new Exception("type mismatch");
    }

    public Node OutputNode { get; }
    public SchemaOutput Output { get; }
}
}