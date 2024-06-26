﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.NodeCore;
using csso.NodeCore.Funcs;

namespace csso.Nodeshop.UI;

public class NodeView : INotifyPropertyChanged {
    private double? _executionTime;
    private bool _isSelected;
    private Point _position;

    public NodeView(GraphView graphView, NodeCore.Node node) {
        Node = node;
        GraphView = graphView;

        Sync();
    }

    public GraphView GraphView { get; }

    public NodeCore.Node Node { get; }

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

    public EditableValueView? EditableValue { get; private set; }

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

    public void Sync() {
        foreach (var input in Node.Inputs) {
            PutView pv = new(this, input);
            Inputs.Add(pv);
        }

        foreach (var output in Node.Outputs) {
            PutView pv = new(this, output);
            Outputs.Add(pv);
        }

        if (Node is FunctionNode { Function: ConstantFunc constFunc }) {
            EditableValue = EditableValueView.Create(constFunc);
        }

        Inputs.ForEach(_ => _.Sync());
    }
}