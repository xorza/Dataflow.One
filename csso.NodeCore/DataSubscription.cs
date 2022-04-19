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

    public NodeArg Subscriber { get; }
    public NodeArg Source { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private SubscriptionBehavior _behavior = SubscriptionBehavior.Always;


    public DataSubscription(NodeArg a, NodeArg b) {
        Check.True(a.ArgType != b.ArgType);

        NodeArg subscriber = a.ArgType == ArgType.In ? a : b;
        NodeArg source = a.ArgType == ArgType.Out ? a : b;

        if (subscriber.Type != source.Type && !source.Type.IsSubclassOf(subscriber.Type)) {
            throw new Exception("type mismatch 4fv56g2456g");
        }


        Subscriber = subscriber;
        Source = source;
    }
    

    internal DataSubscription(Graph graph, SerializedSubscription serialized) {
        Behavior = serialized.Behavior;
        // SubscriberNode = graph.GetNode(serialized.InputNodeId);
        // SubscriberInput = SubscriberNode.Inputs
        //     .Single(input => input.FunctionArg.ArgumentIndex == serialized.InputIndex);
        // SourceNode = graph.GetNode(serialized.TargetNodeId);
        // SourceOutput = SourceNode.Outputs
        //     .Single(output => output.FunctionArg.ArgumentIndex == serialized.TargetIndex);
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

        result.TargetIndex = Source.FunctionArg.ArgumentIndex;
        result.TargetNodeId = Source.Node.Id;

        result.InputIndex = Subscriber.FunctionArg.ArgumentIndex;
        result.InputNodeId = Subscriber.Node.Id;

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