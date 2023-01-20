using System;
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
    private SubscriptionBehavior _behavior = SubscriptionBehavior.Always;


    public DataSubscription(NodeArg a, NodeArg b) {
        Check.True(a.ArgDirection != b.ArgDirection);

        var subscriber = a.ArgDirection == ArgDirection.In ? a : b;
        var source = a.ArgDirection == ArgDirection.Out ? a : b;

        if (!new DataCompatibility().IsValueConvertable(subscriber.Type, source.Type)) {
            throw new Exception("type mismatch 4fv56g2456g");
        }

        Subscriber = subscriber;
        Source = source;
    }

    public DataSubscription(NodeArg subscriber, Object value) {
        Check.True(subscriber.ArgDirection == ArgDirection.In);

        Subscriber = subscriber;
        Value = value;
    }

    public NodeArg Subscriber { get; }
    public NodeArg? Source { get; }
    
    // private object? _value = null;
    //
    // public Object? Value {
    //     get => _value;
    //     set {
    //         if (Source != null) {
    //             throw new Exception("cannot set value on subscription with source");
    //         }
    //
    //         if (value != null) {
    //             if (!DataCompatibility.Instance.IsValueConvertable(Subscriber.Type, value.GetType())) {
    //                 throw new Exception("wvesrthbe56u635");
    //             }
    //         }
    //
    //         _value = value;
    //     }
    // }

    public Object? Value { get; set; }

    public SubscriptionBehavior Behavior {
        get => _behavior;
        set {
            if (_behavior == value) {
                return;
            }

            _behavior = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}