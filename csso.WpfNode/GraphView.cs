using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using csso.NodeCore;
using csso.WpfNode.Annotations;

namespace csso.WpfNode; 

public class GraphView : INotifyPropertyChanged {
    private NodeView? _selectedNode;

    private PutView? _selectedPutView;

    public GraphView(NodeCore.Graph graph) {
        Graph = graph;
        Refresh();
    }

    public NodeCore.Graph Graph { get; }

    public ObservableCollection<EdgeView> Edges { get; }
        = new();

    public ObservableCollection<NodeView> Nodes { get; }
        = new();

    public NodeView? SelectedNode {
        get => _selectedNode;
        set {
            if (_selectedNode == value)
                return;

            if (_selectedNode != null)
                _selectedNode.IsSelected = false;
            _selectedNode = value;
            if (_selectedNode != null)
                _selectedNode.IsSelected = true;

            OnPropertyChanged();
        }
    }

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

    public event PropertyChangedEventHandler? PropertyChanged;

    private NodeView GetNodeView(NodeCore.Node node) {
        return Nodes.Single(_ => _.Node == node);
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Refresh() {
        foreach (var node in Graph.Nodes) {
            if (Nodes.Any(_ => _.Node == node))
                continue;

            NodeView nv = new(this, node);
            Nodes.Add(nv);
        }

        if (_selectedNode != null && !Nodes.Contains(_selectedNode))
            SelectedNode = null;

        List<EdgeView> newEdgeViews = new();
        foreach (var node in Nodes)
        foreach (var edge in node.Node.Connections)
            if (edge is OutputConnection binding) {
                var inputNode = GetNodeView(binding.InputNode);
                var outputNode = GetNodeView(binding.OutputNode);

                var input = inputNode.Inputs.Single(_ => _.FunctionArg == binding.Input);
                var output = outputNode.Outputs.Single(_ => _.FunctionArg == binding.Output);

                newEdgeViews.Add(new EdgeView(binding, input, output));
            }

        for (var i = 0; i < newEdgeViews.Count; i++)
            if (Edges.Count > i)
                Edges[i] = newEdgeViews[i];
            else
                Edges.Add(newEdgeViews[i]);

        while (Edges.Count > newEdgeViews.Count)
            Edges.RemoveAt(Edges.Count - 1);
    }
}