using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.NodeCore;
using csso.WpfNode.Annotations;

namespace csso.WpfNode;

public sealed class PutView : INotifyPropertyChanged {
    private UIElement? _control;
    private bool _isSelected;

    private Point _pinPoint;


    public PutView(NodeArg nodeArg, NodeView nodeView) {
        NodeArg = nodeArg;
        NodeView = nodeView;
        
        ((INotifyCollectionChanged)nodeView.GraphVm.Edges).CollectionChanged += Edges_CollectionChanged;
    }

    public UIElement? Control {
        get => _control;
        set {
            if (value != _control) {
                _control = value;
                OnPropertyChanged();
            }
        }
    }

    public ArgDirection ArgDirection => NodeArg.ArgDirection;
    public bool IsInput => ArgDirection == ArgDirection.In;
    public bool IsOutput => ArgDirection == ArgDirection.Out;

    public NodeArg NodeArg { get; }

    public Point PinPoint {
        get => _pinPoint;
        set {
            if (_pinPoint == value)
                return;

            _pinPoint = value;
            OnPropertyChanged();
        }
    }

    public NodeView NodeView { get; }

    public bool IsSelected {
        get => _isSelected;
        set {
            if (_isSelected == value)
                return;

            _isSelected = value;
            NodeView.GraphVm.SelectedPutView = value ? this : null;

            OnPropertyChanged();
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void Edges_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        // NodeView.GraphVm.Graph.ValueSubscriptions
    }
}