using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.NodeCore;
using csso.NodeCore.Funcs;

namespace csso.WpfNode;

public class NodeView : INotifyPropertyChanged {
    private bool _isSelected;
    private Point _position;

    public NodeView(GraphVM graphVm, NodeCore.Node node) {
        Node = node;
        GraphVm = graphVm;

        foreach (var input in Node.Inputs) {
            PutView pv = new(input, this);
            Inputs.Add(pv);
        }

        foreach (var output in Node.Outputs) {
            PutView pv = new(output, this);
            Outputs.Add(pv);
        }

        if (Node is FunctionNode functionNode) {
            if (functionNode.Function is ValueFunc valueFunc) {
                EditableValueView editableValueView = 
                    (EditableValueView)
                    typeof(EditableValueView<>)
                    .MakeGenericType(valueFunc.Type)
                    .GetConstructors()
                    .First()
                    .Invoke(new object[] { valueFunc });
                
                 // new EditableValueView(valueFunc, valueFunc.ValueProperty);
                EditableValues.Add(editableValueView);
            }
        }
    }

    public GraphVM GraphVm { get; }

    public NodeCore.Node Node { get; }


    private double? _executionTime;

    public Double? ExecutionTime {
        get => _executionTime;
        set {
            _executionTime = value;
            OnPropertyChanged();
        }
    }

    public List<PutView> Inputs { get; } = new();
    public List<PutView> Outputs { get; } = new();
    public ObservableCollection<ValueView> Values { get; } = new();

    public ObservableCollection<EditableValueView> EditableValues { get; } = new();

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}