﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using csso.Common;
using csso.NodeCore.Annotations;

namespace csso.NodeCore;

public abstract class Node : INotifyPropertyChanged {
    private readonly List<Event> _events = new();
    
    public Guid Id { get; protected set; }

    protected Node(Guid id) {
        Id = id;
    }

    public string Name { get; set; }

    public abstract FunctionBehavior Behavior { get; set; }

    public Graph Graph { get; internal set; }

    public IReadOnlyList<FunctionArg> Inputs { get; protected set; }
    public IReadOnlyList<FunctionArg> Outputs { get; protected set; }
    public IReadOnlyList<Event> Events => _events.AsReadOnly();
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
    }


    internal SerializedFunctionNode Serialize() {
        SerializedFunctionNode result = new();

        result.Name = Name;
        result.Id = Id;
        result.FunctionName = Function.FullName;
        result.Behavior = Behavior;
        result.FunctionId = Function.Id;

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
    public FunctionBehavior Behavior { get; set; }
}

public class SerializedGraphNode {
    public string Name { get; set; }
    public Guid Id { get; set; }
    public FunctionBehavior Behavior { get; set; }
    public SerializedGraph SubGraph { get; set; }
}