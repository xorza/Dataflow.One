using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.Common;
using csso.NodeCore.Annotations;

namespace csso.NodeCore;

public abstract class Node : WithId, INotifyPropertyChanged {
    private readonly List<BindingConnection> _bindingConnection = new();
    private readonly List<ValueConnection> _valueConnection = new();
    private readonly List<Event> _events = new();

    protected Node(Guid id) : base(id) {
        ValueConnections = _valueConnection.AsReadOnly();
        BindingConnections = _bindingConnection.AsReadOnly();
        Events = _events.AsReadOnly();
    }

    public string Name { get; set; }

    public abstract FunctionBehavior Behavior { get; set; }

    public Graph Graph { get; internal set; }
    public IReadOnlyList<ValueConnection> ValueConnections { get; }

    public IReadOnlyList<BindingConnection> BindingConnections { get; }

    public IReadOnlyList<FunctionInput> Inputs { get; protected set; }
    public IReadOnlyList<FunctionOutput> Outputs { get; protected set; }
    public IReadOnlyList<Event> Events { get; protected set; }
    public IReadOnlyList<FunctionArg> Args { get; protected set; }

    public void Add(Event @event) {
        @event.Owner = this;
        _events.Add(@event);
        OnPropertyChanged(nameof(Events));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal void Add(ValueConnection connection) {
        Check.Argument(connection.Node == this, nameof(connection));

        _valueConnection.RemoveAll(_ => _.Input == connection.Input);
        _valueConnection.Add(connection);
    }

    internal void Add(BindingConnection connection) {
        Check.Argument(connection.Node == this, nameof(connection));

        _bindingConnection.RemoveAll(_ => _.Input == connection.Input);
        _bindingConnection.Add(connection);
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
        if (connection is ValueConnection valueConnection) {
            Check.True(_valueConnection.Remove(valueConnection));
        }

        if (connection is BindingConnection bindingConnection) {
            Check.True(_bindingConnection.Remove(bindingConnection));
        }
    }
}

public sealed class FunctionNode : Node {
    public Function Function {
        get => _function;
        private set {
            if (_function == value) {
                return;
            }

            _function = value;

            Behavior = _function.Behavior;
            Name = _function.Name;
            Inputs = _function.Inputs;
            Outputs = _function.Outputs;
            Args = _function.Args;

            _function.Inputs
                .Where(_ => _.HasStaticValue)
                .ForEach(_ => new ValueConnection(this, _, _.StaticValue));
        }
    }

    public FunctionNode(Function function) : base(Guid.NewGuid()) {
        Function = function;
    }

    private FunctionBehavior _behavior = FunctionBehavior.Reactive;
    private Function _function;

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
        FunctionFactory functionFactory,
        SerializedFunctionNode serialized
        ) : base(serialized.Id) {
        Name = serialized.Name;

        if (serialized.FunctionId != null) {
            Function = functionFactory.Get(serialized.FunctionId.Value);
        } else {
            Function = functionFactory.Get(serialized.FunctionName);
        }

        Behavior = serialized.Behavior;

        serialized.ValueConnections
            .ForEach(_ => new ValueConnection(this, _));
    }


    internal SerializedFunctionNode Serialize() {
        SerializedFunctionNode result = new();

        result.Name = Name;
        result.Id = Id;
        result.FunctionName = Function.FullName;
        result.Behavior = Behavior;
        result.FunctionId = Function.Id;

        result.ValueConnections = ValueConnections
            .Select(_ => _.Serialize())
            .ToArray();


        return result;
    }
}

public sealed class GraphNode : Node {
    public GraphNode() : base(Guid.NewGuid()) { }

    internal GraphNode(SerializedGraphNode serialized) : base(serialized.Id) {
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
    public SerializedValueConnection[] ValueConnections { get; set; }
    public FunctionBehavior Behavior { get; set; }
}

public class SerializedGraphNode {
    public string Name { get; set; }
    public Guid Id { get; set; }
    public FunctionBehavior Behavior { get; set; }
    public SerializedGraph SubGraph { get; set; }
}