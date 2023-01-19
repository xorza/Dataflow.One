using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace csso.Nodeshop.UI;

public class PinClickEventArgs : RoutedEventArgs {
    public PinClickEventArgs(PutView put) {
        Put = put;
    }

    public PutView Put { get; }
}

public delegate void PinClickEventHandler(object sender, PinClickEventArgs e);

public partial class Node : UserControl, INotifyPropertyChanged {
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(CornerRadius), typeof(Node), new PropertyMetadata(default(CornerRadius)));

    public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(
        nameof(HeaderBackground), typeof(Brush), typeof(Node), new PropertyMetadata(default(Brush)));

    public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.Register(
        nameof(HighlightBrush), typeof(Brush), typeof(Node), new PropertyMetadata(default(Brush)));

    public static readonly DependencyProperty NodeViewProperty = DependencyProperty.Register(
        nameof(NodeView), typeof(NodeView), typeof(Node),
        new PropertyMetadata(default(NodeView), NodeView_PropertyChangedCallback));

    public static readonly DependencyProperty DragCanvasProperty = DependencyProperty.Register(
        nameof(DragCanvas), typeof(Canvas), typeof(Node),
        new PropertyMetadata(default(Canvas), DragCanvas_PropertyChangedCallback));

    public static readonly DependencyProperty DeletionEnabledProperty = DependencyProperty.Register(
        nameof(DeletionEnabled), typeof(bool), typeof(Node),
        new PropertyMetadata(default(bool)));

    public Node() {
        InitializeComponent();

        MouseLeftButtonDown += Node_MouseLeftButtonDown;
        LayoutUpdated += LayoutUpdated_EventHandler;
        Loaded += OnLoaded;

        RefreshExecutionTime();
    }

    public bool DeletionEnabled {
        get => (bool) GetValue(DeletionEnabledProperty);
        set => SetValue(DeletionEnabledProperty, value);
    }

    public Brush HighlightBrush {
        get => (Brush) GetValue(HighlightBrushProperty);
        set => SetValue(HighlightBrushProperty, value);
    }

    public NodeView? NodeView {
        get => (NodeView) GetValue(NodeViewProperty);
        set => SetValue(NodeViewProperty, value);
    }

    public Canvas? DragCanvas {
        get => (Canvas) GetValue(DragCanvasProperty);
        set => SetValue(DragCanvasProperty, value);
    }

    public CornerRadius CornerRadius {
        get => (CornerRadius) GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Brush HeaderBackground {
        get => (Brush) GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static void DragCanvas_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var graph = (Node) d;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        // if (NodeView != null) {
        //     NodeView.PropertyChanged += NodeView_PropertyChanged;
        // }

        RefreshExecutionTime();
    }

    public event PinClickEventHandler? PinClick;

    private void LayoutUpdated_EventHandler(object? sender, EventArgs e) { }

    private static void NodeView_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var node = (Node) d;
        if (e.OldValue is NodeView nv1) {
            nv1.PropertyChanged -= node.NodeView_PropertyChanged;
        }

        node.EditableValueControl.Visibility = Visibility.Collapsed;
        if (e.NewValue is NodeView nv2) {
            nv2.PropertyChanged += node.NodeView_PropertyChanged;
            node.EditableValueControl.Visibility =
                nv2.EditableValue != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void NodeView_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        RefreshExecutionTime();
    }

    private void RefreshExecutionTime() {
        if (NodeView != null) {
            ExecutionTimePanel.Visibility =
                NodeView.ExecutionTime.HasValue ? Visibility.Visible : Visibility.Hidden;
        } else {
            ExecutionTimePanel.Visibility = Visibility.Hidden;
        }
    }

    private void PinButton_Click(object sender, RoutedEventArgs e) {
        var pv = ((Put) sender).PutView!;
        PinClick?.Invoke(this,
            new PinClickEventArgs(pv) {
                RoutedEvent = e.RoutedEvent,
                Source = e.Source,
                Handled = true
            });
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs args) {
        NodeView!.GraphView.SelectedNode = NodeView;
    }

    private void Close_Button_OnClick(object sender, RoutedEventArgs e) {
        NodeView!.GraphView.RemoveNode(NodeView);
    }

    private void PinButton_OnLoaded(object sender, RoutedEventArgs e) {
        var put = (Put) sender;
        put.PinClick += PinButton_Click;
    }
}