using csso.Common;

namespace csso.NodeCore;

public enum ConnectionBehavior {
    Once,
    Always
}

public sealed class OutputConnection : Connection {
    private ConnectionBehavior _behavior = ConnectionBehavior.Always;

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

        Check.True(inputNode.Function.Inputs.Contains(input));
        Check.True(outputNode.Function.Outputs.Contains(output));
    }

    public Node OutputNode { get; }
    public FunctionOutput Output { get; }

    public ConnectionBehavior Behavior {
        get => _behavior;
        set {
            if (_behavior == value) return;
            _behavior = value;
            OnPropertyChanged();
        }
    }

    public SerializedOutputConnection Serialize() {
        SerializedOutputConnection result = new();

        result.OutputIndex = Output.Index;
        result.OutputNodeId = OutputNode.Id;

        result.InputIndex = Input.Index;
        result.InputNodeId = InputNode.Id;

        return result;
    }

    internal OutputConnection(Graph graph, SerializedOutputConnection serialized) {
        InputNode = graph.GetNode(serialized.InputNodeId);
        Input = InputNode.Function.Inputs
            .Single(input => input.Index == serialized.InputIndex);
        OutputNode = graph.GetNode(serialized.OutputNodeId);
        Output = OutputNode.Function.Outputs
            .Single(output => output.Index == serialized.OutputIndex);
    }
}

public struct SerializedOutputConnection {
    public UInt32 OutputIndex { get; set; }
    public Guid OutputNodeId { get; set; }
    public UInt32 InputIndex { get; set; }
    public Guid InputNodeId { get; set; }
}