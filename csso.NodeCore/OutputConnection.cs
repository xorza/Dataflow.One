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
}