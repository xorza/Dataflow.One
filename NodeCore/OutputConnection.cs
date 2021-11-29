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
        OutputNode = outputNode;
        Output = output;

        Check.True(inputNode.Function.Inputs.Contains(input));
        Check.True(outputNode.Function.Outputs.Contains(output));

        if (input.Type != output.Type) throw new Exception("type mismatch");
    }

    public Node OutputNode { get; }
    public FunctionOutput Output { get; }
}
}