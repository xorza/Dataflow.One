using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.NodeCore;
using csso.NodeCore.Annotations;

namespace csso.WpfNode; 

public class EdgeView : INotifyPropertyChanged {
    private bool _isProactive;

    public EdgeView(OutputConnection connection, PutView input, PutView output) {
        Input = input;
        Output = output;
        Connection = connection;

        _isProactive = true;
    }

    public Connection Connection { get; }
    public PutView Input { get; }
    public PutView Output { get; }

    public bool IsProactive {
        get => _isProactive;
        set {
            if (_isProactive == value) return;
            _isProactive = value;
            if (Connection is OutputConnection outputConnection)
                outputConnection.Behavior =
                    _isProactive ? ConnectionBehavior.Always : ConnectionBehavior.Once;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}