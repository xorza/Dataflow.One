using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.NodeCore;
using csso.NodeCore.Annotations;

namespace csso.WpfNode;

public sealed class EdgeView : INotifyPropertyChanged {
    private bool _isProactive;

    public EdgeView(DataSubscription dataSubscription, PutView input, PutView output) {
        Input = input;
        Output = output;
        DataSubscription = dataSubscription;

        _isProactive = dataSubscription.Behavior == SubscriptionBehavior.Always;
    }

    public DataSubscription DataSubscription { get; }
    public PutView Input { get; }
    public PutView Output { get; }

    public bool IsProactive {
        get => _isProactive;
        set {
            if (_isProactive == value) return;
            _isProactive = value;
            DataSubscription.Behavior = _isProactive ? SubscriptionBehavior.Always : SubscriptionBehavior.Once;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}