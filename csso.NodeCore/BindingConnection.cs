using csso.Common;

namespace csso.NodeCore;

public enum ConnectionBehavior {
    Once,
    Always
}

public sealed class BindingConnection : Connection {
    private ConnectionBehavior _behavior = ConnectionBehavior.Always;

    public BindingConnection(
        Node inputNode,
        FunctionInput input,
        Node targetNode,
        FunctionOutput target) :
        base(inputNode, input) {
        if (input.Type != target.Type && !target.Type.IsSubclassOf(input.Type))
            throw new Exception("type mismatch");
        
        Check.True(inputNode.Function.Inputs.Contains(input));
        Check.True(targetNode.Function.Outputs.Contains(target));

        TargetNode = targetNode;
        Target = target;
    }
    
    internal BindingConnection(Graph graph, SerializedOutputConnection serialized) {
        Behavior = serialized.Behavior;
        Node = graph.GetNode(serialized.InputNodeId);
        Input = Node.Function.Inputs
            .Single(input => input.Index == serialized.InputIndex);
        TargetNode = graph.GetNode(serialized.TargetNodeId);
        Target = TargetNode.Function.Outputs
            .Single(output => output.Index == serialized.TargetIndex);
    }
    
    public Node TargetNode { get; }
    public FunctionOutput Target { get; }

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

        result.TargetIndex = Target.Index;
        result.TargetNodeId = TargetNode.Id;

        result.InputIndex = Input.Index;
        result.InputNodeId = Node.Id;

        result.Behavior = Behavior;

        return result;
    }


}

public struct SerializedOutputConnection {
    public Int32 TargetIndex { get; set; }
    public Guid TargetNodeId { get; set; }
    public Int32 InputIndex { get; set; }
    public Guid InputNodeId { get; set; }
    public ConnectionBehavior Behavior { get; set; }
}