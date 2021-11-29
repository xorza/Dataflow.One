using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using csso.Common;
using csso.NodeCore.Annotations;

namespace csso.NodeCore {
public sealed class OutputConnection : Connection {
    public OutputConnection(
        Node inputNode,
        FunctionInput input,
        Node outputNode,
        FunctionOutput output) :
        base(inputNode, input) {
        if (input.Type != output.Type && !output.Type.IsSubclassOf(input.Type))
            throw new Exception("type mismatch");

        OutputNode = outputNode;
        Output = output;
        Behavior = outputNode.Behavior;

        Check.True(inputNode.Function.Inputs.Contains(input));
        Check.True(outputNode.Function.Outputs.Contains(output));
    }

    public Node OutputNode { get; }
    public FunctionOutput Output { get; }

}
}