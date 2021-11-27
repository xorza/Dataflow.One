using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using csso.Common;
using csso.NodeCore;
using csso.OpenCL;

namespace csso.WpfNode {
public partial class Graph : UserControl {
    public static readonly DependencyProperty NodeStyleProperty = DependencyProperty.Register(
        "NodeStyle", typeof(Style), typeof(Graph), new PropertyMetadata(default(Style)));

    private GraphView? _graphView;

    public static readonly DependencyProperty GraphViewProperty = DependencyProperty.Register(
        "GraphView", typeof(GraphView), typeof(Graph), new PropertyMetadata(default(GraphView)));

    public GraphView? GraphView {
        get { return (GraphView) GetValue(GraphViewProperty); }
        set {
            SetValue(GraphViewProperty, value);
            SetDataContext(value);
        }
    }

    private PutView? _pv1;
    private PutView? _pv2;

    public Graph() {
        InitializeComponent();

        LayoutUpdated += LayoutUpdated_Handler;
        Loaded += Loaded_Handler;
        MouseLeftButtonDown += NodeDeselectButton_Handler;
        MouseRightButtonDown += NodeDeselectButton_Handler;

        AddNode(Node0);
        AddNode(Node1);
        AddNode(Node2);
        AddNode(Node3);
    }


    private void NodeDeselectButton_Handler(object sender, MouseButtonEventArgs e) {
        if (_graphView != null)
            _graphView.SelectedNode = null;
    }

    public Style NodeStyle {
        get => (Style) GetValue(NodeStyleProperty);
        set => SetValue(NodeStyleProperty, value);
    }

    private void SetDataContext(GraphView? graphView) {
        DataContext = null;

        _graphView = graphView;

        if (_graphView != null) {
            _graphView.Edges.CollectionChanged += Edges_CollectionChanged;
            _graphView.PropertyChanged += PropertyChanged_Handler;

            Node0.NodeView = _graphView.Nodes[0];
            Node1.NodeView = _graphView.Nodes[1];
            Node2.NodeView = _graphView.Nodes[3];
            Node3.NodeView = _graphView.Nodes[4];
            DataContext = _graphView;
        }
    }

    private void Edges_CollectionChanged(object? sender,
        NotifyCollectionChangedEventArgs e) {
        RefreshLine(true);
    }

    private void PropertyChanged_Handler(object? sender, PropertyChangedEventArgs e) { }

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

        SetDataContext(_graphView);
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

        RefreshLine(true);
    }

    Point? _dragStart = null;
    private void AddNode(Node node) {
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
                var p2 = args.GetPosition(Canvas);
                Canvas.SetLeft(element, p2.X - _dragStart.Value.X);
                Canvas.SetTop(element, p2.Y - _dragStart.Value.Y);

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
    }
}
}