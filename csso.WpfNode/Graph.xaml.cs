using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using csso.Common;
using csso.NodeCore;

namespace csso.WpfNode;

public partial class Graph : UserControl {
    public static readonly DependencyProperty GraphViewProperty = DependencyProperty.Register(
        nameof(GraphView), typeof(GraphVM), typeof(Graph),
        new PropertyMetadata(default(GraphVM), GraphViewPropertyChangedCallback));

    private readonly List<Node> _nodes = new();

    private Point? _dragStart;

    private Canvas? _nodesCanvas;

    public Graph() {
        InitializeComponent();

        LayoutUpdated += LayoutUpdated_Handler;
        Loaded += Loaded_Handler;
        MouseLeftButtonDown += NodeDeselectButton_Handler;
        MouseRightButtonDown += NodeDeselectButton_Handler;
        MouseWheel += MouseWheel_EventHandler;

        Subscribe();
    }

    private void Subscribe() {
        Point startPoint;
        Vector offset;
        bool isMoving = false;

        void OnMove(object sender, MouseEventArgs ea) {
            if (!isMoving) {
                return;
            }

            var point = ea.GetPosition(this);
            System.Diagnostics.Debug.WriteLine(offset.ToString());

            GraphView!.ViewOffset = point - startPoint + offset;
        }

        void OnLeave(object sender, MouseEventArgs ea) {
            isMoving = false;
        }

        void OnButtonUp(object sender, MouseButtonEventArgs ea) {
            isMoving = false;
        }

        void Down(object sender, MouseButtonEventArgs ea) {
            if (isMoving) {
                isMoving = false;
                return;
            }

            if (ea.ChangedButton != MouseButton.Left) {
                return;
            }

            startPoint = ea.GetPosition(this);
            offset = GraphView!.ViewOffset;
            isMoving = true;
        }

        MouseUp += OnButtonUp;
        MouseLeave += OnLeave;
        MouseMove += OnMove;
        MouseDown += Down;
    }

    public GraphVM? GraphView {
        get => (GraphVM) GetValue(GraphViewProperty);
        set => SetValue(GraphViewProperty, value);
    }

    private void MouseWheel_EventHandler(object sender, MouseWheelEventArgs e) {
        GraphView!.ViewScale *= (float) Math.Pow(1.1, e.Delta / 120.0f);
    }

    private void NodeDeselectButton_Handler(object sender, MouseButtonEventArgs e) {
        if (GraphView != null)
            GraphView.SelectedNode = null;
    }

    private void Loaded_Handler(object? sender, EventArgs e) { }

    private void LayoutUpdated_Handler(object? sender, EventArgs e) { }

    private static void GraphViewPropertyChangedCallback(DependencyObject d,
        DependencyPropertyChangedEventArgs e) {
        var graph = (Graph) d;

        if (e.OldValue is GraphVM oldGraphView)
            ((INotifyCollectionChanged) oldGraphView.Edges).CollectionChanged -= graph.Edges_CollectionChanged;

        if (e.NewValue is GraphVM graphView)
            ((INotifyCollectionChanged) graphView.Edges).CollectionChanged += graph.Edges_CollectionChanged;

        graph.RedrawEdges();
    }

    private void Edges_CollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e) {
        RedrawEdges();
    }

    private void RedrawEdges() {
        if (GraphView == null) return;

        while (GraphView!.Edges.Count != EdgesCanvas.Children.Count)
            if (GraphView!.Edges.Count < EdgesCanvas.Children.Count) {
                EdgesCanvas.Children.RemoveAt(EdgesCanvas.Children.Count - 1);
            } else {
                Edge line = new();
                line.LeftButtonClick += LeftButtonClickHandler;

                EdgesCanvas.Children.Add(line);
            }

        Debug.Assert.True(EdgesCanvas.Children.Count == GraphView!.Edges.Count);

        for (var i = 0; i < EdgesCanvas.Children.Count; i++) {
            Binding inputPointBinding = new("Input.PinPoint");
            inputPointBinding.Source = GraphView!.Edges[i];

            Binding outputPointBinding = new("Output.PinPoint");
            outputPointBinding.Source = GraphView.Edges[i];

            Binding proactivePointBinding = new("IsProactive");
            proactivePointBinding.Source = GraphView.Edges[i];

            Binding contextBinding = new();
            contextBinding.Source = GraphView!.Edges[i];

            var edge = (Edge) EdgesCanvas.Children[i];
            edge.SetBinding(Edge.InputPositionDependencyProperty, inputPointBinding);
            edge.SetBinding(Edge.OutputPositionDependencyProperty, outputPointBinding);
            edge.SetBinding(Edge.IsProactiveProperty, proactivePointBinding);
            edge.SetBinding(DataContextProperty, contextBinding);
        }
    }

    private void LeftButtonClickHandler(object sender, MouseButtonEventArgs ea) { }

    private void Node_OnPinClick(object sender, PinClickEventArgs e) {
        Debug.Assert.True(GraphView != null);

        var graphView = GraphView!;
        if (graphView.SelectedPutView == null ||
            graphView.SelectedPutView == e.Put) {
            e.Put.IsSelected = !e.Put.IsSelected;
            return;
        }

        var p1 = graphView.SelectedPutView;
        var p2 = e.Put;

        graphView.SelectedPutView = null;

        if (p1.NodeArg.ArgType == p2.NodeArg.ArgType)
            return;
        if (p1.NodeView == p2.NodeView)
            return;
        if (p1 == p2)
            return;
        if (p1.NodeArg.Type != p2!.NodeArg.Type &&
            !p1.NodeArg.Type.IsSubclassOf(p2.NodeArg.Type) &&
            !p2.NodeArg.Type.IsSubclassOf(p1.NodeArg.Type))
            return;

        var input = p1.NodeArg.ArgType == ArgType.In ? p1 : p2;
        var output = p1.NodeArg.ArgType == ArgType.Out ? p1 : p2;

        Debug.Assert.True(p1 != p2);

        input.NodeView.Node.Graph.Add(
            new DataSubscription(
                input.NodeArg,
                output.NodeArg)
        );

        input.NodeView.GraphVm.Refresh();
    }

    private void NodesCanvas_OnLoaded(object sender, RoutedEventArgs e) {
        _nodesCanvas = (Canvas) sender;
        Xtentions.ForEach(_nodes, _ => _.DragCanvas = _nodesCanvas);
    }

    private void Node_OnLoaded(object sender, RoutedEventArgs e) {
        var node = (Node) sender;
        node.DragCanvas = _nodesCanvas;
        Check.True(node.NodeView?.GraphVm == GraphView);
        AddNode(node);
    }

    private void Node_Unloaded_Handler(object sender, RoutedEventArgs e) {
        var node = (Node) sender;
        node.DragCanvas = null;
        Check.True(_nodes.Remove(node));
    }

    private void AddNode(Node node) {
        Check.True(node.NodeView != null);

        EnableDrag(node);

        _nodes.Add(node);

        node.PinClick += Node_OnPinClick;
        node.DragCanvas = _nodesCanvas;
    }

    private void EnableDrag(Node node) {
        void Down(object sender, MouseButtonEventArgs args) {
            if (_dragStart == null) args.Handled = true;

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
                    new Point(p2.X - _dragStart.Value.X, p2.Y - _dragStart.Value.Y);

                args.Handled = true;
            }
        }

        node.MouseDown += Down;
        node.MouseMove += Move;
        node.MouseUp += Up;
    }
}