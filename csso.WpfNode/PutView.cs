using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using csso.NodeCore;
using csso.WpfNode.Annotations;

namespace csso.WpfNode {
public sealed class PutView : INotifyPropertyChanged {
    public PutView(FunctionArg functionArg, NodeView nodeView) {
        FunctionArg = functionArg;
        NodeView = nodeView;
    }

    public UIElement? Control { get; set; }
    public PutType PutType => FunctionArg.PutType;
    public bool IsInput => PutType == PutType.In;
    public bool IsOutput => PutType == PutType.Out;

    public FunctionArg FunctionArg { get; }

    private Point _pinPoint;

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

    private bool _isSelected = false;

    public bool IsSelected {
        get => _isSelected;
        set {
            if (_isSelected == value)
                return;
            
            _isSelected = value;
            NodeView.GraphView.SelectedPutView = value ? this : null;
            
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
}