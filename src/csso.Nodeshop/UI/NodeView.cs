using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.Common;
using csso.NodeCore;
using csso.NodeCore.Funcs;

namespace csso.Nodeshop.UI;

public class NodeView : INotifyPropertyChanged {
    private double? _executionTime;
    private bool _isSelected;
    private Point _position;

    public NodeView(GraphView graphView, csso.NodeCore.Node node) {
        Node = node;
        GraphView = graphView;

        foreach (var input in Node.Inputs) {
            PutView pv = new(input, this);
            Inputs.Add(pv);
        }

        foreach (var output in Node.Outputs) {
            PutView pv = new(output, this);
            Outputs.Add(pv);
        }

        if (Node is FunctionNode functionNode) {
            if (functionNode.Function is ConstantFunc constFunc) {
                EditableValue = EditableValueView.Create(constFunc);
            }
        }
    }

    public GraphView GraphView { get; }

    public csso.NodeCore.Node Node { get; }

    public double? ExecutionTime {
        get => _executionTime;
        set {
            _executionTime = value;
            OnPropertyChanged();
        }
    }

    public List<PutView> Inputs { get; } = new();
    public List<PutView> Outputs { get; } = new();
    public ObservableCollection<ValueView> InputValues { get; } = new();
    public ObservableCollection<ValueView> OutputValues { get; } = new();

    public EditableValueView? EditableValue { get; }

    public bool IsSelected {
        get => _isSelected;
        set {
            if (_isSelected == value) {
                return;
            }

            _isSelected = value;
            GraphView.SelectedNode = value ? this : null;

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}