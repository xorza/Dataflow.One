using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.NodeCore;
using csso.WpfNode.Annotations;

namespace csso.WpfNode {
public class EdgeView {
    public EdgeView(PutView input, PutView output) {
        Input = input;
        Output = output;
    }

    public PutView Input { get; set; }
    public PutView Output { get; set; }
    public Point P1 => Input.PinPoint;
    public Point P2 => Output.PinPoint;
}

public class GraphView : INotifyPropertyChanged {
    public NodeCore.Graph Graph { get; private set; }

    public GraphView(NodeCore.Graph graph) {
        Graph = graph;
        Refresh();
    }

    public ObservableCollection<EdgeView> Edges { get; }
        = new();

    public ObservableCollection<NodeView> Nodes { get; }
        = new();

    private NodeView? _selectedNode = null;

    public NodeView? SelectedNode {
        get => _selectedNode;
        set {
            if (_selectedNode != value) {
                _selectedNode = value;

                foreach (var node in Nodes) {
                    node.IsSelected = (node == value);
                }

                OnPropertyChanged();
            }
        }
    }

    private PutView? _selectedPutView = null;

    public PutView? SelectedPutView {
        get => _selectedPutView;
        set {
            if (_selectedPutView == value) 
                return;

            if (_selectedPutView != null)
                _selectedPutView.IsSelected = false;
            _selectedPutView = value;
            if (_selectedPutView != null)
                _selectedPutView.IsSelected = true;
            OnPropertyChanged();
        }
    }

    private NodeView GetNodeView(NodeCore.Node node) {
        return Nodes.Single(_ => _.Node == node);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Refresh() {
        Nodes.Clear();
        Edges.Clear();
        SelectedNode = null;


        foreach (var node in Graph.Nodes) {
            NodeView nv = new(this, node);
            Nodes.Add(nv);
        }

        foreach (var node in Nodes)
        foreach (var edge in node.Node.Inputs)
            if (edge is OutputBinding binding) {
                var inputNode = GetNodeView(binding.InputNode);
                var outputNode = GetNodeView(binding.OutputNode);

                PutView input = inputNode.Inputs.Single(_ => _.SchemaPut == binding.Input);
                PutView output = outputNode.Outputs.Single(_ => _.SchemaPut == binding.Output);

                Edges.Add(new EdgeView(input, output));
            }
    }
}
}