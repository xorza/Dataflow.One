using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.Common;
using csso.NodeCore.Annotations;

namespace csso.NodeCore;

public class Node : WithId, INotifyPropertyChanged {
    private readonly List<ConfigValue> _configValues = new();
    private readonly List<Connection> _connections = new();

    private FunctionBehavior _behavior = FunctionBehavior.Proactive;

    private Node() : this(Guid.NewGuid()) { }

    private Node(Guid id) : base(id) {
        Connections = _connections.AsReadOnly();
        ConfigValues = _configValues.AsReadOnly();
    }

    public Node(Function function, Graph graph) : this() {
        Function = function;
        Behavior = function.Behavior;
        Name = function.Name;
        Graph = graph;

        foreach (var funcConfig in function.Config) {
            ConfigValue value = new(funcConfig);
            _configValues.Add(value);
        }
    }

    public string Name { get; set; }

    public Function Function { get; }

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
            if (_behavior == FunctionBehavior.Proactive) {
                return Function.Behavior;
            }

            Check.True(_behavior == FunctionBehavior.Reactive);
            return FunctionBehavior.Reactive;
        }
    }

    public Graph Graph { get; }
    public IReadOnlyList<Connection> Connections { get; }
    public IReadOnlyList<ConfigValue> ConfigValues { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void AddConnection(Connection connection) {
        _connections.RemoveAll(_ => _.Input == connection.Input);

        Check.True(connection.InputNode == this);

        _connections.Add(connection);
    }

    internal SerializedNode Serialize() {
        SerializedNode result = new();

        result.Name = Name;
        result.Id = Id;
        result.ConfigValues = _configValues
            .Select(_ => _.Serialize())
            .ToArray();
        result.ValueConnections = _connections
            .OfType<ValueConnection>()
            .Select(_ => _.Serialize())
            .ToArray();

        result.FunctionId = Function.Id;

        return result;
    }

    internal Node(
        FunctionFactory functionFactory,
        SerializedNode serialized) : this(serialized.Id) {
        Name = serialized.Name;
        Function = functionFactory.Get(serialized.FunctionId);

        serialized.ConfigValues
            .Select(serializedValue => new ConfigValue(Function, serializedValue))
            .Foreach(_configValues.Add);

        serialized.ValueConnections
            .Select(_ => new ValueConnection(this, _))
            .Foreach(AddConnection);
    }
}

public class SerializedNode {
    public string Name { get; set; }
    public Guid Id { get; set; }
    public Guid FunctionId { get; set; }
    public SerializedConfigValue[] ConfigValues { get; set; }
    public SerializedValueConnection[] ValueConnections { get; set; }
}