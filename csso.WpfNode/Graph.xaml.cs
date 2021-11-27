using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using csso.Common;
using csso.NodeCore;
using csso.OpenCL;

namespace csso.WpfNode {
public partial class Graph : UserControl {
    public static readonly DependencyProperty NodeStyleProperty = DependencyProperty.Register(
        "NodeStyle", typeof(Style), typeof(Graph), new PropertyMetadata(default(Style)));

    public static readonly DependencyProperty GraphViewProperty = DependencyProperty.Register(
        "GraphView", typeof(GraphView), typeof(Graph),
        new PropertyMetadata(default(GraphView), GraphViewPropertyChangedCallback));

    public GraphView? GraphView {
        get => (GraphView) GetValue(GraphViewProperty);
        set => SetValue(GraphViewProperty, value);
    }

    private PutView? _pv1;
    private PutView? _pv2;

    private readonly List<Node> _nodes = new();

    private Canvas? _nodesCanvas = null;

    public Graph() {
        InitializeComponent();

        LayoutUpdated += LayoutUpdated_Handler;
        Loaded += Loaded_Handler;
        MouseLeftButtonDown += NodeDeselectButton_Handler;
        MouseRightButtonDown += NodeDeselectButton_Handler;
    }

    private void NodeDeselectButton_Handler(object sender, MouseButtonEventArgs e) {
        if (GraphView != null)
            GraphView.SelectedNode = null;
    }

    public Style NodeStyle {
        get => (Style) GetValue(NodeStyleProperty);
        set => SetValue(NodeStyleProperty, value);
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
        input.NodeView.GraphView.Refresh();
    }

    private void Loaded_Handler(object? sender, EventArgs e) { }

    private void LayoutUpdated_Handler(object? sender, EventArgs e) { }

    private static void GraphViewPropertyChangedCallback(DependencyObject d,
        DependencyPropertyChangedEventArgs e) {
        WpfNode.Graph graph = (Graph) d;

        if (e.OldValue is GraphView oldGraphView)
            oldGraphView.Edges.CollectionChanged -= graph.Edges_CollectionChanged;

        if (e.NewValue is GraphView graphView)
            graphView.Edges.CollectionChanged += graph.Edges_CollectionChanged;
    }

    private void Edges_CollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e) {
        RedrawEdges();
    }

    private void RedrawEdges() {
        if (GraphView == null) return;

        while (GraphView!.Edges.Count > EdgesCanvas.Children.Count) {
            Edge line = new();
            line.LeftButtonClick += LeftButtonClickHandler;

            EdgesCanvas.Children.Add(line);
        }

        Debug.Assert.True(EdgesCanvas.Children.Count == GraphView!.Edges.Count);

        for (var i = 0; i < EdgesCanvas.Children.Count; i++) {

            System.Windows.Data.Binding inputPointBinding = new("Input.PinPoint");
            inputPointBinding.Source = GraphView!.Edges[i];
            inputPointBinding.Mode = BindingMode.Default;
            
            System.Windows.Data.Binding outputPointBinding = new("Output.PinPoint");
            outputPointBinding.Source = GraphView!.Edges[i];
            inputPointBinding.Mode = BindingMode.Default;
            
            // edge.InputPosition = GraphView!.Edges[i].Input.PinPoint;
            //  edge.OutputPosition = GraphView!.Edges[i].Output.PinPoint;
            
            Edge edge = (Edge) EdgesCanvas.Children[i];
            edge.SetBinding(Edge.InputPositionDependencyProperty, inputPointBinding);
            edge.SetBinding(Edge.OutputPositionDependencyProperty, outputPointBinding);
        }

        while (GraphView!.Edges.Count < EdgesCanvas.Children.Count)
            EdgesCanvas.Children.RemoveAt(EdgesCanvas.Children.Count - 1);
    }

    private void LeftButtonClickHandler(object? sender, MouseButtonEventArgs ea) {
        ;
    }

    private void Node_OnPinClick(object sender, PinClickEventArgs e) {
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
    }


    private void NodesCanvas_OnLoaded(object sender, RoutedEventArgs e) {
        _nodesCanvas = (Canvas) sender;
        _nodes.Foreach(_ => _.DragCanvas = _nodesCanvas);
    }

    private void Node_OnLoaded(object sender, RoutedEventArgs e) {
        Node node = (Node) sender;
        AddNode(node);
    }


    Point? _dragStart = null;

    private void AddNode(Node node) {
        Check.True(node.NodeView != null);

        void Down(object sender, MouseButtonEventArgs args) {
            if (_dragStart == null) {
                args.Handled = true;
            }

            var element = (UIElement) sender;
            _dragStart = args.GetPosition(element);
        }

        void Up(object sender, MouseButtonEventArgs args) {
            var element = (UIElement) sender;
            _dragStart = null;
            element.ReleaseMouseCapture();
        }

        void Move(object sender, MouseEventArgs args) {
            if (_dragStart != null && args.LeftButton == MouseButtonState.Pressed) {
                var element = (UIElement) sender;
                element.CaptureMouse();
                var p2 = args.GetPosition(_nodesCanvas);
                node.NodeView!.Position =
                    new(p2.X - _dragStart.Value.X, p2.Y - _dragStart.Value.Y);

                args.Handled = true;
            }
        }

        void EnableDrag(UIElement element) {
            element.MouseDown += Down;
            element.MouseMove += Move;
            element.MouseUp += Up;
        }

        EnableDrag(node);
        node.PinClick += Node_OnPinClick;
        _nodes.Add(node);
        node.DragCanvas = _nodesCanvas;
        node.Style = NodeStyle;
    }
}
}