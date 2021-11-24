using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using csso.Common;
using csso.NodeCore;
using csso.WpfNode;
using Graph = csso.NodeCore.Graph;
using Node = csso.NodeCore.Node;

namespace WpfApp1 {
public partial class MainWindow : Window {
    private readonly Graph _graph;
    private GraphView? _graphView;

    private PutView? pv1;
    private PutView? pv2;

    public MainWindow() {
        InitializeComponent();

        _graph = new Graph();

        Schema schemaRgbMix = new();
        schemaRgbMix.Name = "RGB mix";
        schemaRgbMix.Inputs.Add(new SchemaInput("R", typeof(int)));
        schemaRgbMix.Inputs.Add(new SchemaInput("G", typeof(int)));
        schemaRgbMix.Inputs.Add(new SchemaInput("B", typeof(int)));
        schemaRgbMix.Outputs.Add(new SchemaOutput("RGB", typeof(int)));

        Schema schemaBitmap = new();
        schemaBitmap.Name = "Bitmap";
        schemaBitmap.Outputs.Add(new SchemaOutput("R", typeof(int)));
        schemaBitmap.Outputs.Add(new SchemaOutput("G", typeof(int)));
        schemaBitmap.Outputs.Add(new SchemaOutput("B", typeof(int)));
        schemaBitmap.Outputs.Add(new SchemaOutput("RGB", typeof(int)));

        Node node0 = new(schemaRgbMix, _graph);
        Node node1 = new(schemaBitmap, _graph);
        Node node2 = new(schemaRgbMix, _graph);
        Node node3 = new(schemaBitmap, _graph);

        Loaded += (s, ea) => { RefreshLine(true); };
        LayoutUpdated += MainWindow_LayoutUpdated;

        Node0.PinClick += Node_PinClick;
        Node1.PinClick += Node_PinClick;
        Node2.PinClick += Node_PinClick;
        Node3.PinClick += Node_PinClick;

        RefreshGraphView();
    }

    private void MainWindow_LayoutUpdated(object? sender, EventArgs e) {
        RefreshLine(false);
    }

    private void RefreshGraphView() {
        _graphView = new GraphView(_graph);
        _graphView.Edges.CollectionChanged += Edges_CollectionChanged;
        Node0.NodeView = _graphView.Nodes[0];
        Node1.NodeView = _graphView.Nodes[1];
        Node2.NodeView = _graphView.Nodes[2];
        Node3.NodeView = _graphView.Nodes[3];
        DataContext = _graphView;
    }

    private void Edges_CollectionChanged(object? sender,
        NotifyCollectionChangedEventArgs e) {
        RefreshLine(true);
    }

    private void Node_PinClick(object sender, PinClickEventArgs e) {
        if (pv1 == null) {
            pv1 = e.Put;
        }
        else if (pv2 == null) {
            pv2 = e.Put;
            Commit();
        }
        else {
            pv1 = e.Put;
            pv2 = null;
        }

        RefreshLine(true);
    }

    private void Commit() {
        Debug.Assert.NotNull(pv1);
        Debug.Assert.NotNull(pv2);

        if (pv1!.SchemaPut.PutType == pv2!.SchemaPut.PutType) {
            pv1 = null;
            pv2 = null;
            return;
        }

        if (pv1!.NodeView == pv2!.NodeView) {
            pv1 = null;
            pv2 = null;
            return;
        }

        PutView input = pv1.SchemaPut.PutType == PutType.In ? pv1 : pv2;
        PutView output = pv1.SchemaPut.PutType == PutType.Out ? pv1 : pv2;

        Debug.Assert.True(pv1 != pv2);

        OutputBinding binding = new(
            input.NodeView.Node,
            (SchemaInput) input.SchemaPut,
            output.NodeView.Node,
            (SchemaOutput) output.SchemaPut);

        var indexOfBinding = input.NodeView.Node.Schema.Inputs.IndexOf((SchemaInput) input.SchemaPut);
        input.NodeView.Node.Inputs[indexOfBinding] = binding;

        RefreshGraphView();
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
            Line line = new();
            line.StrokeThickness = 2.0;
            line.Stroke = Brushes.Black;

            EdgesCanvas.Children.Add(line);
        }

        for (var i = 0; i < _graphView!.Edges.Count; i++) {
            Line line = (Line) EdgesCanvas.Children[i];
            line.X1 = _graphView!.Edges[i].P1.X;
            line.Y1 = _graphView!.Edges[i].P1.Y;
            line.X2 = _graphView!.Edges[i].P2.X;
            line.Y2 = _graphView!.Edges[i].P2.Y;
        }

        while (_graphView!.Edges.Count < EdgesCanvas.Children.Count)
            EdgesCanvas.Children.RemoveAt(EdgesCanvas.Children.Count - 1);
    }
}
}