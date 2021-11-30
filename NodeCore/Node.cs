using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using csso.Common;
using csso.NodeCore.Annotations;

namespace csso.NodeCore {
public class Node : INotifyPropertyChanged {
    private readonly List<Connection> _connections = new();


    public Node(IFunction function, Graph graph) {
        Function = function;
        Graph = graph;

        Connections = _connections.AsReadOnly();
    }

    public string Name => Function.Name;

    public IFunction Function { get; private set; }

    private FunctionBehavior _behavior = FunctionBehavior.Proactive;

    public FunctionBehavior Behavior {
        get => _behavior;
        set {
            if (_behavior != value) {
                Check.Argument(value != FunctionBehavior.Proactive, nameof(value));
                _behavior = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FinalBehavior));
            }
        }
    }

    public FunctionBehavior FinalBehavior {
        get {
            if (_behavior == FunctionBehavior.Proactive)
                return Function.Behavior;
            else {
                Check.True(_behavior == FunctionBehavior.Reactive);
                return FunctionBehavior.Reactive;
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Graph Graph { get; }
    public IReadOnlyList<Connection> Connections { get; }

    public void AddBinding(Connection connection) {
        _connections.RemoveAll(_ => _.Input == connection.Input);

        Check.True(connection.InputNode == this);

        _connections.Add(connection);
    }
}
}