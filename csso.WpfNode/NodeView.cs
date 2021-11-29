using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.NodeCore;

namespace csso.WpfNode {
public class NodeView : INotifyPropertyChanged {
    private Point _position;

    public GraphView GraphView { get; }

    public NodeView(GraphView graphView, NodeCore.Node node) {
        Node = node;
        GraphView = graphView;

        foreach (var input in Node.Function.Inputs) {
            PutView pv = new(input, this);
            Inputs.Add(pv);
        }

        foreach (var output in Node.Function.Outputs) {
            PutView pv = new(output, this);
            Outputs.Add(pv);
        }
    }

    public NodeCore.Node Node { get; }

    public List<PutView> Inputs { get; } = new();
    public List<PutView> Outputs { get; } = new();

    private bool _isSelected = false;

    public bool IsSelected {
        get => _isSelected;
        set {
            if (_isSelected == value)
                return;
            
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

    // public string Name {get;}
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
}