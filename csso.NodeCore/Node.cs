using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.Common;
using csso.NodeCore.Annotations;

namespace csso.NodeCore;

public abstract class Node : WithId, INotifyPropertyChanged {
    private readonly List<BindingConnection> _bindingConnections = new();
    private readonly List<ValueConnection> _valueConnections = new();

    protected Node(Graph graph, Guid id) : base(id) {
        ValueConnections = _valueConnections.AsReadOnly();
        BindingConnections = _bindingConnections.AsReadOnly();
        ConfigValues = new List<ConfigValue>();

        Graph = graph;
    }

    public string Name { get; set; }

    public bool IsProcedure { get; protected set; } = false;

    public abstract FunctionBehavior Behavior { get; set; }


    public Graph Graph { get; }
    public IReadOnlyList<ValueConnection> ValueConnections { get; }

    public IReadOnlyList<BindingConnection> BindingConnections { get; }

    //1:1 mapping to Node.Config
    public IReadOnlyList<ConfigValue> ConfigValues { get; protected set; }
    public IReadOnlyList<FunctionConfig> Config { get; protected set; }


    public IReadOnlyList<FunctionInput> Inputs { get; protected set; }
    public IReadOnlyList<FunctionOutput> Outputs { get; protected set; }
    public IReadOnlyList<FunctionArg> Args { get; protected set; }


    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal void Add(ValueConnection connection) {
        Check.Argument(connection.Node == this, nameof(connection));

        _valueConnections.RemoveAll(_ => _.Input == connection.Input);
        _valueConnections.Add(connection);
    }

    internal void Add(BindingConnection connection) {
        Check.Argument(connection.Node == this, nameof(connection));

        _bindingConnections.RemoveAll(_ => _.Input == connection.Input);
        _bindingConnections.Add(connection);
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
        if (connection is ValueConnection valueConnection) Check.True(_valueConnections.Remove(valueConnection));
        if (connection is BindingConnection bindingConnection)
            Check.True(_bindingConnections.Remove(bindingConnection));
    }
}

public sealed class FunctionNode : Node {
    private Function _function;

    public Function Function {
        get => _function;
        set {
            if (value == _function) return;

            _function = value;

            Behavior = value.Behavior;
            Name = value.Name;
            IsProcedure = value.IsProcedure;
            Config = value.Config;
            Inputs = value.Inputs;
            Outputs = value.Outputs;
            Args = value.Args;

            ConfigValues =
                value.Config
                    .Select(_ => new ConfigValue(_))
                    .ToList();
        }
    }


    public FunctionNode(Graph graph, Function function) : base(graph, Guid.NewGuid()) {
        Function = function;
    }

    private FunctionBehavior _behavior = FunctionBehavior.Reactive;

    public override FunctionBehavior Behavior {
        get => _behavior;
        set {
            if (value > Function.Behavior) {
                return;
            }

            if (_behavior != value) {
                _behavior = value;
                OnPropertyChanged();
            }
        }
    }


    internal FunctionNode(
        Graph graph,
        SerializedFunctionNode serialized) : base(graph, serialized.Id) {
        Name = serialized.Name;

        if (serialized.FunctionId != null) {
            Function = graph.FunctionFactory.Get(serialized.FunctionId.Value);
        } else {
            Function = graph.FunctionFactory.Get(serialized.FunctionName);
        }

        Behavior = serialized.Behavior;

        ConfigValues = serialized.ConfigValues
            .Select(serializedValue => new ConfigValue(Function, serializedValue))
            .ToList();

        serialized.ValueConnections
            .Select(_ => new ValueConnection(this, _))
            .Foreach(Add);
    }


    internal SerializedFunctionNode Serialize() {
        SerializedFunctionNode result = new();

        result.Name = Name;
        result.Id = Id;
        result.FunctionName = Function.FullName;
        result.Behavior = Behavior;
        result.FunctionId = Function.Id;

        result.ConfigValues = ConfigValues
            .Select(_ => _.Serialize())
            .ToArray();
        result.ValueConnections = ValueConnections
            .Select(_ => _.Serialize())
            .ToArray();


        return result;
    }
}

public sealed class GraphNode : Node {
    public GraphNode(Graph graph) : base(graph, Guid.NewGuid()) { }

    internal GraphNode(
        Graph graph,
        SerializedGraphNode serialized) : base(graph, serialized.Id) {
        Name = serialized.Name;
        SubGraph = new Graph(Graph.FunctionFactory, serialized.SubGraph);
    }

    public override FunctionBehavior Behavior { get; set; }
    public Graph SubGraph { get; set; }

    internal SerializedGraphNode Serialize() {
        SerializedGraphNode result = new();

        result.Name = Name;
        result.Id = Id;
        result.Behavior = Behavior;
        result.SubGraph = SubGraph.Serialize();
        
        return result;
    }
}

public class SerializedFunctionNode {
    public string Name { get; set; }
    public Guid Id { get; set; }
    public String FunctionName { get; set; }
    public Guid? FunctionId { get; set; }
    public SerializedConfigValue[] ConfigValues { get; set; }
    public SerializedValueConnection[] ValueConnections { get; set; }
    public FunctionBehavior Behavior { get; set; }
}

public class SerializedGraphNode {
    public string Name { get; set; }
    public Guid Id { get; set; }
    public FunctionBehavior Behavior { get; set; }
    public SerializedGraph SubGraph { get; set; }
}