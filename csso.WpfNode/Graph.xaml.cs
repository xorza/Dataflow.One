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

        while (GraphView!.Edges.Count != EdgesCanvas.Children.Count) {
            if (GraphView!.Edges.Count < EdgesCanvas.Children.Count)
                EdgesCanvas.Children.RemoveAt(EdgesCanvas.Children.Count - 1);
            else {
                Edge line = new();
                line.LeftButtonClick += LeftButtonClickHandler;

                EdgesCanvas.Children.Add(line);
            }
        }

        Debug.Assert.True(EdgesCanvas.Children.Count == GraphView!.Edges.Count);

        for (var i = 0; i < EdgesCanvas.Children.Count; i++) {
            System.Windows.Data.Binding inputPointBinding = new("Input.PinPoint");
            inputPointBinding.Source = GraphView!.Edges[i];

            System.Windows.Data.Binding outputPointBinding = new("Output.PinPoint");
            outputPointBinding.Source = GraphView!.Edges[i];

            Edge edge = (Edge) EdgesCanvas.Children[i];
            edge.SetBinding(Edge.InputPositionDependencyProperty, inputPointBinding);
            edge.SetBinding(Edge.OutputPositionDependencyProperty, outputPointBinding);
        }
    }

    private void LeftButtonClickHandler(object? sender, MouseButtonEventArgs ea) {
        ;
    }

    private void Node_OnPinClick(object sender, PinClickEventArgs e) {
        Debug.Assert.True(GraphView != null);

        GraphView graphView = GraphView!;
        if (graphView.SelectedPutView == null ||
            graphView.SelectedPutView == e.Put) {
            e.Put.IsSelected = !e.Put.IsSelected;
            return;
        }

        PutView p1 = graphView.SelectedPutView;
        PutView p2 = e.Put;

        graphView.SelectedPutView = null;
        
        if (p1!.FunctionArg.PutType == p2!.FunctionArg.PutType) 
            return;
        if (p1!.NodeView == p2!.NodeView) 
            return;
        if (p1 == p2) 
            return;
        if (p1!.FunctionArg.Type != p2!.FunctionArg.Type) 
            return;
        
        PutView input = p1.FunctionArg.PutType == PutType.In ? p1 : p2;
        PutView output = p1.FunctionArg.PutType == PutType.Out ? p1 : p2;
        
        Debug.Assert.True(p1 != p2);
        
        OutputConnection connection = new(
            input.NodeView.Node,
            (FunctionInput) input.FunctionArg,
            output.NodeView.Node,
            (FunctionOutput) output.FunctionArg);
        
        input.NodeView.Node.AddBinding(connection);
        input.NodeView.GraphView.Refresh();
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