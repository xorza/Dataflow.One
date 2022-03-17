using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace csso.WpfNode;

public class NodeView : INotifyPropertyChanged {
    private bool _isSelected;
    private Point _position;

    public NodeView(GraphVM graphVm, NodeCore.Node node) {
        Node = node;
        GraphVm = graphVm;

        foreach (var input in Node.Function.Inputs) {
            PutView pv = new(input, this);
            Inputs.Add(pv);
        }

        foreach (var output in Node.Function.Outputs) {
            PutView pv = new(output, this);
            Outputs.Add(pv);
        }
    }

    public GraphVM GraphVm { get; }

    public NodeCore.Node Node { get; }

    private double? _executionTime = null;

    public Double? ExecutionTime {
        get => _executionTime;
        set {
            _executionTime = value;
            OnPropertyChanged();
        }
    }

    public List<PutView> Inputs { get; } = new();
    public List<PutView> Outputs { get; } = new();

    public bool IsSelected {
        get => _isSelected;
        set {
            if (_isSelected == value)
                return;

            _isSelected = value;
            GraphVm.SelectedNode = value ? this : null;

            OnPropertyChanged();
        }
    }

    public Point Position {
        get => _position;
        set {
            if (_position != value) {
                _position = value;
                OnPropertyChanged();
            }
        }
    }

    // public string Name {get;}
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    internal SerializedNodeView Serialize() {
        SerializedNodeView result = new();

        result.Position = Position;
        result.Id = Node.Id;

        return result;
    }
}

public struct SerializedNodeView {
    public Point Position { get; set; }
    public Guid Id { get; set; }
}