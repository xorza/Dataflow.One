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

    public FunctionArg SubscriberInput { get; }
    public Node SubscriberNode { get;  }

    public Node SourceNode { get; }
    public FunctionArg SourceOutput { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private SubscriptionBehavior _behavior = SubscriptionBehavior.Always;


    public DataSubscription(
        Node subscriberNode,
        FunctionArg subscriberInput,
        Node sourceNode,
        FunctionArg sourceOutput) {
        if (subscriberInput.Type != sourceOutput.Type && !sourceOutput.Type.IsSubclassOf(subscriberInput.Type)) {
            throw new Exception("type mismatch 4fv56g2456g");
        }

        Check.True(subscriberNode.Inputs.Contains(subscriberInput));
        Check.True(sourceNode.Outputs.Contains(sourceOutput));

        SubscriberInput = subscriberInput;
        SubscriberNode = subscriberNode;
        SourceNode = sourceNode;
        SourceOutput = sourceOutput;
    }

    internal DataSubscription(Graph graph, SerializedSubscription serialized) {
        Behavior = serialized.Behavior;
        SubscriberNode = graph.GetNode(serialized.InputNodeId);
        SubscriberInput = SubscriberNode.Inputs
            .Single(input => input.ArgumentIndex == serialized.InputIndex);
        SourceNode = graph.GetNode(serialized.TargetNodeId);
        SourceOutput = SourceNode.Outputs
            .Single(output => output.ArgumentIndex == serialized.TargetIndex);
    }


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

        result.TargetIndex = SourceOutput.ArgumentIndex;
        result.TargetNodeId = SourceNode.Id;

        result.InputIndex = SubscriberInput.ArgumentIndex;
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