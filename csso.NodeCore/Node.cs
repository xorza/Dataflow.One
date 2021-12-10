using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.Common;
using csso.NodeCore.Annotations;

namespace csso.NodeCore; 

public class ConfigValue {
    private Object? _value;

    public ConfigValue(FunctionConfig config) : this(config, config.DefaultValue) { }

    public ConfigValue(FunctionConfig config, Object? value) {
        Config = config;
        Value = value;
    }

    public FunctionConfig Config { get; }

    public Type Type => Config.Type;

    public Object? Value {
        get => _value;
        set {
            if (value != null)
                Check.True(value.GetType() == Type);

            _value = value;
        }
    }
}

public class Node : INotifyPropertyChanged {
    private readonly List<ConfigValue> _configValues = new();
    private readonly List<Connection> _connections = new();

    private FunctionBehavior _behavior = FunctionBehavior.Proactive;

    public Node(IFunction function, Graph graph) {
        Function = function;
        Behavior = function.Behavior;
        Graph = graph;

        Connections = _connections.AsReadOnly();
        ConfigValues = _configValues.AsReadOnly();

        foreach (var funcConfig in function.Config) {
            ConfigValue value = new(funcConfig);
            _configValues.Add(value);
        }
    }

    public string Name => Function.Name;

    public IFunction Function { get; }

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

    public void AddBinding(Connection connection) {
        _connections.RemoveAll(_ => _.Input == connection.Input);

        Check.True(connection.InputNode == this);

        _connections.Add(connection);
    }
}