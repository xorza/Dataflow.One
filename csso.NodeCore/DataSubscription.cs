using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.Common;
using csso.NodeCore.Annotations;

namespace csso.NodeCore;

public enum SubscriptionBehavior {
    Once,
    Always
}

public class DataSubscription : INotifyPropertyChanged {
    protected DataSubscription() { }

    public FunctionInput Input { get; protected set; }
    public Node SubscriberNode { get; protected set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private SubscriptionBehavior _behavior = SubscriptionBehavior.Always;


    public DataSubscription(
        Node subscriberNode,
        FunctionInput input,
        Node targetNode,
        FunctionOutput target) {
        if (input.Type != target.Type && !target.Type.IsSubclassOf(input.Type)) {
            throw new Exception("type mismatch 4fv56g2456g");
        }

        Check.True(subscriberNode.Inputs.Contains(input));
        Check.True(targetNode.Outputs.Contains(target));

        Input = input;
        SubscriberNode = subscriberNode;
        TargetNode = targetNode;
        Target = target;
    }

    internal DataSubscription(Graph graph, SerializedSubscription serialized) {
        Behavior = serialized.Behavior;
        SubscriberNode = graph.GetNode(serialized.InputNodeId);
        Input = SubscriberNode.Inputs
            .Single(input => input.ArgumentIndex == serialized.InputIndex);
        TargetNode = graph.GetNode(serialized.TargetNodeId);
        Target = TargetNode.Outputs
            .Single(output => output.ArgumentIndex == serialized.TargetIndex);
    }

    public Node TargetNode { get; }
    public FunctionOutput Target { get; }

    public SubscriptionBehavior Behavior {
        get => _behavior;
        set {
            if (_behavior == value) return;
            _behavior = value;
            OnPropertyChanged();
        }
    }


    public SerializedSubscription Serialize() {
        SerializedSubscription result = new();

        result.TargetIndex = Target.ArgumentIndex;
        result.TargetNodeId = TargetNode.Id;

        result.InputIndex = Input.ArgumentIndex;
        result.InputNodeId = SubscriberNode.Id;

        result.Behavior = Behavior;

        return result;
    }
}

public struct SerializedSubscription {
    public Int32 TargetIndex { get; set; }
    public Guid TargetNodeId { get; set; }
    public Int32 InputIndex { get; set; }

    public Guid InputNodeId { get; set; }
    public SubscriptionBehavior Behavior { get; set; }
}