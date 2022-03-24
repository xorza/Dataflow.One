using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.Common;
using csso.NodeCore;
using csso.NodeCore.Run;
using csso.WpfNode.Annotations;
using DynamicData;

namespace csso.WpfNode;

public sealed class GraphVM : INotifyPropertyChanged {
    private readonly ObservableCollection<EdgeView> _edges = new();

    private readonly ObservableCollection<NodeView> _nodes = new();

    private FunctionFactoryView _functionFactory;
    private NodeView? _selectedNode;

    private PutView? _selectedPutView;

    public GraphVM(NodeCore.Graph graph) : this() {
        Graph = graph;
        Refresh();
    }

    public GraphVM() {
        Nodes = new ReadOnlyObservableCollection<NodeView>(_nodes);
        Edges = new ReadOnlyObservableCollection<EdgeView>(_edges);
    }

    public GraphVM(FunctionFactory functionFactory, SerializedGraphView serialized)
        : this(new NodeCore.Graph(functionFactory, serialized.Graph)) {
        ViewOffset = serialized.ViewOffset;
        ViewScale = serialized.ViewScale;

        serialized.NodeViews
            .Foreach(_ => {
                var node = _nodes.Single(n => n.Node.Id == _.Id);
                node.Position = _.Position;
            });
    }

    public NodeCore.Graph Graph { get; }
    public ReadOnlyObservableCollection<EdgeView> Edges { get; }

    private Vector _viewOffset;

    public Vector ViewOffset {
        get => _viewOffset;
        set {
            if (_viewOffset == value) {
                return;
            }

            _viewOffset = value;
            OnPropertyChanged();
        }
    }

    private float _viewScale = 1.0f;

    public float ViewScale {
        get => _viewScale;
        set {
            if (Math.Abs(_viewScale - value) < 1e-3) {
                return;
            }

            _viewScale = value > 0.2f ? value : 1.0f;
            OnPropertyChanged();
        }
    }

    public FunctionFactoryView FunctionFactory {
        get => _functionFactory;
        set {
            if (_functionFactory != value) {
                _functionFactory = value;
                OnPropertyChanged();
            }
        }
    }

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
        foreach (var binding in node.Node.BindingConnections) {
            var inputNode = GetNodeView(binding.Node);
            var outputNode = GetNodeView(binding.TargetNode);

            var input = inputNode.Inputs.Single(_ => _.FunctionArg == binding.Input);
            var output = outputNode.Outputs.Single(_ => _.FunctionArg == binding.Target);

            _edges.Add(new EdgeView(binding, input, output));
        }

        FunctionFactory = new FunctionFactoryView(Graph.FunctionFactory);
    }

    public void RemoveNode(NodeView nodeView) {
        Check.True(nodeView.GraphVm == this);

        Graph.Remove(nodeView.Node);

        Refresh();
    }

    public NodeView CreateNode(Function func) {
        var node = Graph.AddNode(func);
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

        result.ViewScale = ViewScale;
        result.ViewOffset = ViewOffset;

        return result;
    }

    public void OnExecuted(Executor executor) {
        foreach (var nodeView in Nodes)
            if (executor.TryGetEvaluationNode(nodeView.Node, out var en)) {
                nodeView.ExecutionTime = en!.ExecutionTime;

                nodeView.Values.Clear();
                
                
                foreach (var output in nodeView.Inputs) {
                    var index = output.FunctionArg.ArgumentIndex;
                    var value = en.ArgValues?[index];
                    nodeView.Values.Add(
                        ValueView.FromValue(output, value)
                    );
                }

                foreach (var output in nodeView.Outputs) {
                    var index = output.FunctionArg.ArgumentIndex;
                    var value = en.ArgValues?[index];
                    nodeView.Values.Add(
                        ValueView.FromValue(output, value)
                    );
                }
            }
    }
}

public struct SerializedGraphView {
    public SerializedGraph Graph { get; set; }
    public SerializedNodeView[] NodeViews { get; set; }
    public Vector ViewOffset { get; set; }
    public float ViewScale { get; set; }
}