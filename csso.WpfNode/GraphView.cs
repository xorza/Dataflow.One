using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.Common;
using csso.NodeCore;
using csso.WpfNode.Annotations;
using DynamicData;
using OpenTK.Compute.OpenCL;

namespace csso.WpfNode;

public sealed class GraphView : INotifyPropertyChanged {
    private NodeView? _selectedNode;

    private PutView? _selectedPutView;

    public GraphView(NodeCore.Graph graph) : this() {
        Graph = graph;
        Refresh();
    }

    public GraphView() {
        Nodes = new ReadOnlyObservableCollection<NodeView>(_nodes);
        Edges = new ReadOnlyObservableCollection<EdgeView>(_edges);
    }

    public NodeCore.Graph Graph { get; }

    private readonly ObservableCollection<EdgeView> _edges = new();
    public ReadOnlyObservableCollection<EdgeView> Edges { get; }


    private FunctionFactoryView _functionFactory;
    public FunctionFactoryView FunctionFactory {
        get => _functionFactory;
        set {
            if (_functionFactory != value) {
                _functionFactory = value;
                OnPropertyChanged();
            }  
        } 
    }


    private readonly ObservableCollection<NodeView> _nodes = new();
    public ReadOnlyObservableCollection<NodeView> Nodes { get; }


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
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Refresh() {
        var nodesToRemove = _nodes
            .Where(_ => !Graph.Nodes.Contains(_.Node))
            .ToArray();
        _nodes.RemoveMany(nodesToRemove);

        Graph.Nodes
            .Where(n => _nodes.All(nv => nv.Node != n))
            .Select(_ => new NodeView(this, _))
            .Foreach(_nodes.Add);

        if (_selectedNode != null && !Nodes.Contains(_selectedNode))
            SelectedNode = null;


        _edges.Clear();
        foreach (var node in Nodes)
        foreach (var edge in node.Node.Connections)
            if (edge is OutputConnection binding) {
                var inputNode = GetNodeView(binding.InputNode);
                var outputNode = GetNodeView(binding.OutputNode);

                var input = inputNode.Inputs.Single(_ => _.FunctionArg == binding.Input);
                var output = outputNode.Outputs.Single(_ => _.FunctionArg == binding.Output);

                _edges.Add(new EdgeView(binding, input, output));
            }

        FunctionFactory = new FunctionFactoryView(Graph.FunctionFactory);
    }

    public void RemoveNode(NodeView nodeView) {
        Check.True(nodeView.GraphView == this);

        Graph.Remove(nodeView.Node);

        Refresh();
    }

    public NodeView CreateNode(Function func) {
        NodeCore.Node node = new(func, Graph);
        Graph.Add(node);
        NodeView result = new(this, node);
        _nodes.Add(result);

        result.Position = new Point(100, 100);
        result.IsSelected = true;
        return result;
    }

    public SerializedGraphView Serialize() {
        SerializedGraphView result = new();

        result.Graph = Graph.Serialize();

        result.NodeViews = _nodes
            .Select(_ => _.Serialize())
            .ToArray();

        return result;
    }

    public GraphView(FunctionFactory functionFactory, SerializedGraphView serialized)
        : this(new NodeCore.Graph(functionFactory, serialized.Graph)) {
        serialized.NodeViews
            .Foreach(_ => {
                var node = _nodes.Single(n => n.Node.Id == _.Id);
                node.Position = _.Position;
            });
    }
}

public struct SerializedGraphView {
    public SerializedGraph Graph { get; set; }
    public SerializedNodeView[] NodeViews { get; set; }
}