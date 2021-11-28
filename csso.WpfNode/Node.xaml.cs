using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using csso.Common;
using OpenTK.Windowing.Common;

namespace csso.WpfNode {
public class PinClickEventArgs : RoutedEventArgs {
    public PinClickEventArgs(PutView put) {
        Put = put;
    }

    public PutView Put { get; }
}

public delegate void PinClickEventHandler(object sender, PinClickEventArgs e);


public partial class Node : UserControl, INotifyPropertyChanged {
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        "CornerRadius", typeof(CornerRadius), typeof(Node), new PropertyMetadata(default(CornerRadius)));

    public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(
        "HeaderBackground", typeof(Brush), typeof(Node), new PropertyMetadata(default(Brush)));

    public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.Register(
        "HighlightBrush", typeof(Brush), typeof(Node), new PropertyMetadata(default(Brush)));

    public Brush HighlightBrush {
        get => (Brush) GetValue(HighlightBrushProperty);
        set => SetValue(HighlightBrushProperty, value);
    }

    public static readonly DependencyProperty NodeViewProperty = DependencyProperty.Register(
        "NodeView", typeof(NodeView), typeof(Node), new PropertyMetadata(default(NodeView)));

    public NodeView? NodeView {
        get => (NodeView) GetValue(NodeViewProperty);
        set => SetValue(NodeViewProperty, value);
    }

    public static readonly DependencyProperty DragCanvasProperty = DependencyProperty.Register(
        "DragCanvas", typeof(Canvas), typeof(Node), new PropertyMetadata(default(Canvas)));

    public Canvas? DragCanvas {
        get => (Canvas) GetValue(DragCanvasProperty);
        set => SetValue(DragCanvasProperty, value);
    }

    public Node() {
        InitializeComponent();

        MouseLeftButtonDown += Node_MouseLeftButtonDown;
        LayoutUpdated += EventHandler;
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

    public event PinClickEventHandler? PinClick;

    private void EventHandler(object? sender, EventArgs e) {
        if (DragCanvas != null) {
            UpdatePinPositions(DragCanvas!);
        }
    }

    private void UpdatePinPositions(Canvas canvas) {
        Check.True(NodeView != null);

        NodeView!.Inputs.ForEach(put => {
            if (put.Control != null) {
                var upperLeft = put.Control
                    .TransformToVisual(canvas)
                    .Transform(new Point(0, 0));
                var mid = new Point(
                    put.Control.RenderSize.Width / 2,
                    put.Control.RenderSize.Height / 2);

                var newPinPoint = new Point(upperLeft.X + mid.X, upperLeft.Y + mid.Y);
                put.PinPoint = newPinPoint;
            }
        });

        NodeView!.Outputs.ForEach(put => {
            if (put.Control != null) {
                var upperLeft = put.Control
                    .TransformToVisual(canvas)
                    .Transform(new Point(0, 0));
                var mid = new Point(
                    put.Control.RenderSize.Width / 2,
                    put.Control.RenderSize.Height / 2);

                var newPinPoint = new Point(upperLeft.X + mid.X, upperLeft.Y + mid.Y);
                put.PinPoint = newPinPoint;
            }
        });
    }


    private void PinButton_Click(object sender, RoutedEventArgs e) {
        PutView pv = (PutView) ((Button) sender).Tag;
        PinClick?.Invoke(sender,
            new PinClickEventArgs(pv) {
                RoutedEvent = e.RoutedEvent,
                Source = e.Source,
                Handled = true
            });
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void Node_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs args) {
        Check.True(NodeView != null);

        if (NodeView!.GraphView.SelectedNode != NodeView) {
            NodeView!.GraphView.SelectedNode = NodeView;
        }
    }

    private void PinHighlight_LoadedHandler(object sender, RoutedEventArgs args) {
        FrameworkElement element = (FrameworkElement) sender;
        PutView pv = (PutView) element.Tag;
        pv.Control = element;
    }
}
}