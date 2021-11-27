using System;
using System.Collections.Specialized;
using System.Reflection.Metadata.Ecma335;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using csso.Common;
using csso.NodeCore;
using csso.OpenCL;

namespace csso.WpfNode {
/// <summary>
///     Interaction logic for Graph.xaml
/// </summary>
public partial class Graph : UserControl {
    public static readonly DependencyProperty NodeStyleProperty = DependencyProperty.Register(
        "NodeStyle", typeof(Style), typeof(Graph), new PropertyMetadata(default(Style)));

    private NodeCore.Graph? _graph;

    private GraphView? _graphView;

    private PutView? _pv1;
    private PutView? _pv2;


    public Graph() {
        InitializeComponent();

        LayoutUpdated += LayoutUpdated_Handler;
        Loaded += Loaded_Handler;
    }

    public Style NodeStyle {
        get => (Style) GetValue(NodeStyleProperty);
        set => SetValue(NodeStyleProperty, value);
    }

    public NodeCore.Graph? GraphContext {
        get => _graph;
        set {
            _graph = value;
            SetupDataContext();
        }
    }

    private void SetupDataContext() {
        if (_graph != null) {
            Schema schemaRgbMix = new();
            schemaRgbMix.Name = "RGB mix";
            schemaRgbMix.Inputs.Add(new SchemaInput("R", typeof(Tensor1)));
            schemaRgbMix.Inputs.Add(new SchemaInput("G", typeof(Tensor1)));
            schemaRgbMix.Inputs.Add(new SchemaInput("B", typeof(Tensor1)));
            schemaRgbMix.Outputs.Add(new SchemaOutput("RGB", typeof(Tensor4)));

            Schema schemaBitmap = new();
            schemaBitmap.Name = "Bitmap";
            schemaBitmap.Outputs.Add(new SchemaOutput("R", typeof(Tensor1)));
            schemaBitmap.Outputs.Add(new SchemaOutput("G", typeof(Tensor1)));
            schemaBitmap.Outputs.Add(new SchemaOutput("B", typeof(Tensor1)));
            schemaBitmap.Outputs.Add(new SchemaOutput("RGB", typeof(Tensor4)));

            Schema output = new();
            output.Name = "Output";
            output.Inputs.Add(new SchemaInput("R", typeof(Tensor1)));
            output.Inputs.Add(new SchemaInput("RGB", typeof(Tensor4)));

            NodeCore.Node node0 = new(schemaRgbMix, _graph);
            NodeCore.Node node1 = new(schemaBitmap, _graph);
            NodeCore.Node node2 = new(schemaRgbMix, _graph);
            NodeCore.Node node3 = new(schemaBitmap, _graph);

            OutputNode node4 = new(output, _graph);

        }
        
        
        RefreshDataContext();
    }

    private void RefreshDataContext() {
        DataContext = null;

        if (_graph == null)
            return;

        _graphView = new GraphView(_graph);
        _graphView.Edges.CollectionChanged += Edges_CollectionChanged;
        
        Node0.NodeView = _graphView.Nodes[0];
        Node1.NodeView = _graphView.Nodes[1];
        Node2.NodeView = _graphView.Nodes[3];
        Node3.NodeView = _graphView.Nodes[4];
        DataContext = _graphView;
    }

    private void Edges_CollectionChanged(object? sender,
        NotifyCollectionChangedEventArgs e) {
        RefreshLine(true);
    }

    private void Commit() {
        Debug.Assert.NotNull(_pv1);
        Debug.Assert.NotNull(_pv2);

        if (_pv1!.SchemaPut.PutType == _pv2!.SchemaPut.PutType) {
            _pv1 = null;
            _pv2 = null;
            return;
        }

        if (_pv1!.NodeView == _pv2!.NodeView) {
            _pv1 = null;
            _pv2 = null;
            return;
        }

        if (_pv1 == _pv2) {
            _pv1 = null;
            _pv2 = null;
            return;
        }

        if (_pv1!.SchemaPut.Type != _pv2!.SchemaPut.Type) {
            _pv1 = null;
            _pv2 = null;
            return;
        }

        PutView input = _pv1.SchemaPut.PutType == PutType.In ? _pv1 : _pv2;
        PutView output = _pv1.SchemaPut.PutType == PutType.Out ? _pv1 : _pv2;

        Debug.Assert.True(_pv1 != _pv2);

        OutputBinding binding = new(
            input.NodeView.Node,
            (SchemaInput) input.SchemaPut,
            output.NodeView.Node,
            (SchemaOutput) output.SchemaPut);

        input.NodeView.Node.AddBinding(binding);

        RefreshDataContext();
    }

    private void Loaded_Handler(object? sender, EventArgs e) {
        RefreshLine(true);
    }

    private void LayoutUpdated_Handler(object? sender, EventArgs e) {
        RefreshLine(false);
    }

    private void RefreshLine(bool forceRedraw) {
        if (Node1.NodeView == null || Node2.NodeView == null)
            return;

        var needRedraw = Node0.UpdatePinPositions(Canvas);
        needRedraw |= Node1.UpdatePinPositions(Canvas);
        needRedraw |= Node2.UpdatePinPositions(Canvas);
        needRedraw |= Node3.UpdatePinPositions(Canvas);

        if (!needRedraw && !forceRedraw)
            return;

        RedrawEdges();
    }

    private void RedrawEdges() {
        if (_graphView == null) return;

        while (_graphView!.Edges.Count > EdgesCanvas.Children.Count) {
            Edge line = new();
            line.LeftButtonClick += LeftButtonClickHandler;

            EdgesCanvas.Children.Add(line);
        }

        for (var i = 0; i < _graphView!.Edges.Count; i++) {
            Edge line = (Edge) EdgesCanvas.Children[i];
            line.InputPosition = _graphView!.Edges[i].P1;
            line.OutputPosition = _graphView!.Edges[i].P2;
        }

        while (_graphView!.Edges.Count < EdgesCanvas.Children.Count)
            EdgesCanvas.Children.RemoveAt(EdgesCanvas.Children.Count - 1);
    }


    private void LeftButtonClickHandler(object? sender, MouseButtonEventArgs ea) {
        ;
    }

    private void Node0_OnPinClick(object sender, PinClickEventArgs e) {
        if (_pv1 == null) {
            _pv1 = e.Put;
        }
        else if (_pv2 == null) {
            _pv2 = e.Put;
            Commit();
        }
        else {
            _pv1 = e.Put;
            _pv2 = null;
        }

        RefreshLine(true);
    }
}
}