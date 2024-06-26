﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using csso.Common;
using csso.NodeCore;
using csso.NodeCore.Annotations;
using csso.NodeCore.Run;

namespace csso.Nodeshop.UI;

public sealed class GraphView : INotifyPropertyChanged {
    private readonly ObservableCollection<EdgeView> _edges = new();
    private readonly ObservableCollection<NodeView> _nodes = new();

    private NodeView? _selectedNode;
    private PutView? _selectedPutView;

    private Vector _viewOffset;

    private float _viewScale = 1.0f;


    public GraphView(NodeCore.Graph graph) {
        Nodes = new ReadOnlyObservableCollection<NodeView>(_nodes);
        Edges = new ReadOnlyObservableCollection<EdgeView>(_edges);


        Graph = graph;
        Sync();
    }

    public NodeCore.Graph Graph { get; }
    public ReadOnlyObservableCollection<EdgeView> Edges { get; }
    public ReadOnlyObservableCollection<NodeView> Nodes { get; }

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

    public NodeView? SelectedNode {
        get => _selectedNode;
        set {
            if (_selectedNode == value) {
                return;
            }

            if (_selectedNode != null) {
                _selectedNode.IsSelected = false;
            }

            _selectedNode = value;
            if (_selectedNode != null) {
                _selectedNode.IsSelected = true;
            }

            OnPropertyChanged();
        }
    }

    public PutView? SelectedPutView {
        get => _selectedPutView;
        set {
            if (_selectedPutView == value) {
                return;
            }

            if (_selectedPutView != null) {
                _selectedPutView.IsSelected = false;
            }

            _selectedPutView = value;
            if (_selectedPutView != null) {
                _selectedPutView.IsSelected = true;
            }

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

    public void Sync() {
        _nodes
            .Where(_ => !Graph.Nodes.Contains(_.Node))
            .ForEach(_ => _nodes.Remove(_));

        Graph.Nodes
            .Where(n => _nodes.All(nv => nv.Node != n))
            .Select(_ => new NodeView(this, _))
            .ForEach(_nodes.Add);

        _nodes.ForEach(_ => _.Sync());

        if (_selectedNode != null && !Nodes.Contains(_selectedNode)) {
            SelectedNode = null;
        }

        _edges.Clear();
        foreach (var binding in Graph.DataSubscriptions) {
            if (binding.Source == null) {
                continue;
            }

            var inputNode = GetNodeView(binding.Subscriber.Node);
            var outputNode = GetNodeView(binding.Source.Node);

            var input = inputNode.Inputs.Single(_ => _.NodeArg == binding.Subscriber);
            var output = outputNode.Outputs.Single(_ => _.NodeArg == binding.Source);

            _edges.Add(new EdgeView(binding, input, output));
        }
    }

    [Obsolete("should be done to base graph then sync")]
    public void RemoveNode(NodeView nodeView) {
        Check.True(nodeView.GraphView == this);

        Graph.Remove(nodeView.Node);

        Sync();
    }

    [Obsolete("should be done to base graph then sync")]
    public NodeView CreateNode(Function func) {
        var node = Graph.AddNode(func);
        NodeView result = new(this, node);
        _nodes.Add(result);

        result.Position = new Point(100, 100);
        result.IsSelected = true;
        return result;
    }

    public void OnFinishRun(Executor executor) {
        foreach (var nodeView in Nodes) {
            nodeView.ExecutionTime = null;
            nodeView.InputValues.Clear();
            nodeView.OutputValues.Clear();

            var en = executor.EvaluationNodes
                .SingleOrDefault(_ => _.Node == nodeView.Node);

            if (en != null) {
                foreach (var output in nodeView.Inputs) {
                    var index = output.NodeArg.FunctionArg.ArgumentIndex;
                    var value = en.ArgValues[index];
                    nodeView.InputValues.Add(
                        ValueView.FromValue(output, value)
                    );
                }

                if (en.State >= EvaluationState.Invoked) {
                    nodeView.ExecutionTime = en.ExecutionTime;

                    foreach (var output in nodeView.Outputs) {
                        var index = output.NodeArg.FunctionArg.ArgumentIndex;
                        var value = en.ArgValues[index];
                        nodeView.OutputValues.Add(
                            ValueView.FromValue(output, value)
                        );
                    }
                }
            }
        }
    }
}