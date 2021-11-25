using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using csso.Common;
using csso.ImageProcessing;
using csso.NodeCore;
using csso.WpfNode;
using OpenTK.Compute.OpenCL;
using Graph = csso.NodeCore.Graph;
using Node = csso.NodeCore.Node;

namespace WpfApp1 {
public partial class MainWindow : Window {
    private readonly Graph _graph;
    private GraphView? _graphView;

    private csso.ImageProcessing.Context? _clContext;

    private PutView? _pv1;
    private PutView? _pv2;

    public MainWindow() {
        InitializeComponent();

        _graph = new Graph();

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

        Node node0 = new(schemaRgbMix, _graph);
        Node node1 = new(schemaBitmap, _graph);
        Node node2 = new(schemaRgbMix, _graph);
        Node node3 = new(schemaBitmap, _graph);

        OutputNode node4 = new(output, _graph);


        LayoutUpdated += MainWindow_LayoutUpdated;
        Loaded += MainWindow_Loaded;

        Node0.PinClick += Node_PinClick;
        Node1.PinClick += Node_PinClick;
        Node2.PinClick += Node_PinClick;
        Node3.PinClick += Node_PinClick;

        RefreshGraphView();
    }

    private void MainWindow_Loaded(object? sender, EventArgs e) {
        RefreshLine(true);

        _clContext = Context.Create();
    }

    private void MainWindow_LayoutUpdated(object? sender, EventArgs e) {
        RefreshLine(false);
    }

    private void RefreshGraphView() {
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

    private void Node_PinClick(object sender, PinClickEventArgs e) {
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
            csso.WpfNode.Edge line = new Edge();


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

    private void DetectCycles_Button_OnClick(object sender, RoutedEventArgs e) {
        NoLoopValidator validator = new();
        validator.Go(_graph);
    }

    private void OpenCLTest1_Button_OnClick(object sender, RoutedEventArgs e) {
        if (_clContext != null) {
            String result = _clContext.Test1();
            MessageBox.Show(this, result, "OpenCL test results");
        }
    }
    private void OpenCLTest2_Button_OnClick(object sender, RoutedEventArgs e) {
        if (_clContext == null) {
            return;
        }


        String code = @"
                __kernel void add(__global float* A, __global float* B,__global float* result, const float C)
                {
                    int i = get_global_id(0);
                    result[i] = (A[i] + B[i]) + C;
					result[i] = (A[i] + B[i]);
                }";
        Program p = new Program(_clContext!, code);
    }
}
}