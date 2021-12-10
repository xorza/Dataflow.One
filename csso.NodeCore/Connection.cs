using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.NodeCore.Annotations;

namespace csso.NodeCore;

public abstract class Connection : INotifyPropertyChanged {
    protected Connection(Node node, FunctionInput input) {
        Input = input;
        InputNode = node;
    }

    protected Connection() { }

    public FunctionInput Input { get; protected set; }
    public Node InputNode { get; protected set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}