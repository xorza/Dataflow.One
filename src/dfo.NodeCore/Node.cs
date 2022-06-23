using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using dfo.NodeCore.Annotations;

namespace dfo.NodeCore;

public abstract class Node : INotifyPropertyChanged {
    protected Node(Guid id) {
        Id = id;
    }

    public Guid Id { get; }
    public string Name { get; set; }
    public abstract FunctionBehavior Behavior { get; set; }
    public Graph Graph { get; internal set; }

    public IReadOnlyList<NodeArg> Inputs => Args.Where(_ => _.ArgDirection == ArgDirection.In).ToList();
    public IReadOnlyList<NodeArg> Outputs => Args.Where(_ => _.ArgDirection == ArgDirection.Out).ToList();
    public IReadOnlyList<NodeArg> Args { get; protected set; } = new List<NodeArg>();


    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class FunctionNode : Node {
    private FunctionBehavior _behavior = FunctionBehavior.Reactive;
    private Function _function;

    public FunctionNode(Function function) : base(Guid.NewGuid()) {
        Function = function;
    }
    
    public Function Function {
        get => _function;
        private set {
            if (_function == value) return;

            _function = value;

            Behavior = _function.Behavior;
            Name = _function.Name;
            Args = _function.Args
                .Select(_ => new NodeArg {
                    Node = this,
                    FunctionArg = _
                })
                .ToList();

            OnPropertyChanged(nameof(Args));
            OnPropertyChanged(nameof(Name));
        }
    }

    public override FunctionBehavior Behavior {
        get => _behavior;
        set {
            if (value > Function.Behavior) return;

            if (_behavior != value) {
                _behavior = value;
                OnPropertyChanged();
            }
        }
    }
}

public sealed class GraphNode : Node {
    public GraphNode() : base(Guid.NewGuid()) { }

    public override FunctionBehavior Behavior { get; set; }
    public Graph SubGraph { get; set; }
}
