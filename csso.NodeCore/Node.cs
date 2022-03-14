using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.Common;
using csso.NodeCore.Annotations;

namespace csso.NodeCore;

public sealed class Node : WithId, INotifyPropertyChanged {
    private readonly List<ConfigValue> _configValues = new();
    private readonly List<Connection> _connections = new();

    private FunctionBehavior _behavior = FunctionBehavior.Proactive;

    private Node() : this(Guid.NewGuid()) { }

    private Node(Guid id) : base(id) {
        Connections = _connections.AsReadOnly();
        ConfigValues = _configValues.AsReadOnly();
    }

    public Node(Graph graph, Function function) : this() {
        Graph = graph;
        Function = function;
        Behavior = function.Behavior;
        Name = function.Name;

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
    //1:1 mapping to Function.Config
    public IReadOnlyList<ConfigValue> ConfigValues { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal void Add(Connection connection) {
        Check.Argument(connection.Node == this, nameof(connection));

        _connections.RemoveAll(_ => _.Input == connection.Input);
        _connections.Add(connection);
    }

    public BindingConnection AddConnection(FunctionInput selfInput, Node outputNode, FunctionOutput nodeOutput) {
        BindingConnection connection = new(
            this,
            selfInput,
            outputNode,
            nodeOutput);

        Add(connection);

        return connection;
    }

    public void Remove(Connection connection) {
        Check.Argument(connection.Node == this, nameof(connection));
        if (!_connections.Remove(connection))
            throw new Exception("dfgdvryui6");
    }

    internal SerializedNode Serialize() {
        SerializedNode result = new();

        result.Name = Name;
        result.Id = Id;
        result.FunctionName = Function.FullName;
        result.Behavior = Behavior;
        result.FunctionId = Function.Id;

        result.ConfigValues = _configValues
            .Select(_ => _.Serialize())
            .ToArray();
        result.ValueConnections = _connections
            .OfType<ValueConnection>()
            .Select(_ => _.Serialize())
            .ToArray();


        return result;
    }

    internal Node(
        Graph graph,
        SerializedNode serialized) : this(serialized.Id) {
        Graph = graph;
        Name = serialized.Name;

        if (serialized.FunctionId != null) {
            Function = graph.FunctionFactory.Get(serialized.FunctionId.Value);
        } else {
            Function = graph.FunctionFactory.Get(serialized.FunctionName);
        }

        Behavior = serialized.Behavior;

        serialized.ConfigValues
            .Select(serializedValue => new ConfigValue(Function, serializedValue))
            .Foreach(_configValues.Add);

        serialized.ValueConnections
            .Select(_ => new ValueConnection(this, _))
            .Foreach(Add);
    }
}

public class SerializedNode {
    public string Name { get; set; }
    public Guid Id { get; set; }
    public String FunctionName { get; set; }
    public Guid? FunctionId { get; set; }
    public SerializedConfigValue[] ConfigValues { get; set; }
    public SerializedValueConnection[] ValueConnections { get; set; }
    public FunctionBehavior Behavior { get; set; }
}