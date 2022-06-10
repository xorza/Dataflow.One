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
    public NodeArg Subscriber { get; }
    public NodeArg Source { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private SubscriptionBehavior _behavior = SubscriptionBehavior.Always;


    public DataSubscription(NodeArg a, NodeArg b) {
        Check.True(a.ArgDirection != b.ArgDirection);

        NodeArg subscriber = a.ArgDirection == ArgDirection.In ? a : b;
        NodeArg source = a.ArgDirection == ArgDirection.Out ? a : b;

        if (!new DataCompatibility().IsValueConvertable(subscriber.Type, source.Type)) {
            throw new Exception("type mismatch 4fv56g2456g");
        }

        Subscriber = subscriber;
        Source = source;
    }

    public SubscriptionBehavior Behavior {
        get => _behavior;
        set {
            if (_behavior == value) return;
            _behavior = value;
            OnPropertyChanged();
        }
    }
}