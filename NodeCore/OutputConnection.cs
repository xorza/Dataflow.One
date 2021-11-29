using System;
using System.Linq;
using csso.Common;

namespace csso.NodeCore {
public class OutputConnection : Connection {
    public OutputConnection(Node inputNode,
        FunctionInput input,
        Node outputNode,
        FunctionOutput output) :
        base(inputNode, input) {
        ;
        if (input.Type != output.Type && !output.Type.IsSubclassOf(input.Type)) 
            throw new Exception("type mismatch");

        OutputNode = outputNode;
        Output = output;

        Check.True(inputNode.Function.Inputs.Contains(input));
        Check.True(outputNode.Function.Outputs.Contains(output));
    }

    public Node OutputNode { get; }
    public FunctionOutput Output { get; }
}
}