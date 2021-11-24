using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace csso.WpfNode {
public class NodeView : INotifyPropertyChanged {
    private Point _position;


    public NodeView(NodeCore.Node node) {
        Node = node;

        foreach (var input in Node.Schema.Inputs) {
            PutView pv = new(input, this);
            Inputs.Add(pv);
        }

        foreach (var output in Node.Schema.Outputs) {
            PutView pv = new(output, this);
            Outputs.Add(pv);
        }
    }

    public NodeCore.Node Node { get; }

    public List<PutView> Inputs { get; } = new();
    public List<PutView> Outputs { get; } = new();

    public Point Position {
        get => _position;
        set {
            if (_position != value) {
                _position = value;
                OnPropertyChanged();
            }
        }
    }

    public string Name => Node.Schema.Name;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
}