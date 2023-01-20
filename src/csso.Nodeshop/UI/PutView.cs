using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.Common;
using csso.NodeCore;
using csso.NodeCore.Annotations;

namespace csso.Nodeshop.UI;

public sealed class PutView : INotifyPropertyChanged {
    private UIElement? _control;
    private bool _isSelected;

    private Point _pinPoint;


    public PutView(NodeView nodeView, NodeArg nodeArg) {
        NodeArg = nodeArg;
        NodeView = nodeView;
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

    public EditableValueView? InputValueView { get; private set;}

    public NodeArg NodeArg { get; }

    public Point PinPoint {
        get => _pinPoint;
        set {
            if (_pinPoint == value) {
                return;
            }

            _pinPoint = value;
            OnPropertyChanged();
        }
    }

    public NodeView NodeView { get; }

    public bool IsSelected {
        get => _isSelected;
        set {
            if (_isSelected == value) {
                return;
            }

            _isSelected = value;
            NodeView.GraphView.SelectedPutView = value ? this : null;

            OnPropertyChanged();
        }
    }

    public DataSubscription? GetDataSubscription() {
        Check.True(IsInput);

        return NodeArg.Node.Graph.GetDataSubscription(NodeArg);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Sync() {
        if (IsInput) {
            InputValueView = EditableValueView.Create(this);
            OnPropertyChanged(nameof(InputValueView));
        }
    }
}