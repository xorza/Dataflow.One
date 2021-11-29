using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.NodeCore.Annotations;

namespace csso.NodeCore {
public abstract class Connection : INotifyPropertyChanged {
    protected Connection(Node node, FunctionInput input) {
        Input = input;
        InputNode = node;
    }

    public FunctionInput Input { get; }
    public Node InputNode { get; }


    private FunctionBehavior _behavior = FunctionBehavior.Proactive;

    public virtual FunctionBehavior Behavior {
        get => _behavior;
        set {
            if (_behavior != value) {
                _behavior = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
}