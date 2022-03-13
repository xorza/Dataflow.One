using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.NodeCore;
using csso.NodeCore.Annotations;

namespace csso.WpfNode;

public sealed class EdgeView : INotifyPropertyChanged {
    private bool _isProactive;

    public EdgeView(BindingConnection connection, PutView input, PutView output) {
        Input = input;
        Output = output;
        Connection = connection;

        _isProactive = connection.Behavior == ConnectionBehavior.Always;
    }

    public BindingConnection Connection { get; }
    public PutView Input { get; }
    public PutView Output { get; }

    public bool IsProactive {
        get => _isProactive;
        set {
            if (_isProactive == value) return;
            _isProactive = value;
            Connection.Behavior = _isProactive ? ConnectionBehavior.Always : ConnectionBehavior.Once;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}