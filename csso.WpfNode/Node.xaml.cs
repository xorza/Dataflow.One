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
        get { return (Brush) GetValue(HighlightBrushProperty); }
        set { SetValue(HighlightBrushProperty, value); }
    }
    
    private NodeView? _nodeView;

    public Node() {
        InitializeComponent();

        MouseLeftButtonDown += Node_MouseLeftButtonDown;
    }

    public NodeView? NodeView {
        get => _nodeView;
        set {
            if (_nodeView != value) {
                _nodeView = value;
                Panel.DataContext = null;
                Panel.DataContext = _nodeView;
                HeaderLabel.Content = _nodeView?.Name;
                
                OnPropertyChanged();
            }
        }
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

    public bool UpdatePinPositions(Canvas canvas) {
        var updated = false;
        
        _nodeView?.Inputs.ForEach(put => {
            if (put.Control != null) {
                var upperLeft = put.Control
                    .TransformToVisual(canvas)
                    .Transform(new Point(0, 0));
                var mid = new Point(
                    put.Control.RenderSize.Width / 2,
                    put.Control.RenderSize.Height / 2);

                var newPinPoint = new Point(upperLeft.X + mid.X, upperLeft.Y + mid.Y);
                if (put.PinPoint != newPinPoint) {
                    put.PinPoint = newPinPoint;
                    updated = true;
                }
            }
        });

        _nodeView?.Outputs.ForEach(put => {
            if (put.Control != null) {
                var upperLeft = put.Control
                    .TransformToVisual(canvas)
                    .Transform(new Point(0, 0));
                var mid = new Point(
                    put.Control.RenderSize.Width / 2,
                    put.Control.RenderSize.Height / 2);
                //Point mid = new Point();
                var newPinPoint = new Point(upperLeft.X + mid.X, upperLeft.Y + mid.Y);
                if (put.PinPoint != newPinPoint) {
                    put.PinPoint = newPinPoint;
                    updated = true;
                }
            }
        });

        return updated;
    }

 
    private void PinButton_Click(object sender, RoutedEventArgs e) {
        PutView pv = (PutView) ((Button) sender).Tag;
        pv.IsSelected = true;
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
        Check.True(_nodeView != null);

        if (_nodeView!.GraphView.SelectedNode != _nodeView) {
            _nodeView!.GraphView.SelectedNode = _nodeView;
        }
    }

    private void PinHighlight1_OnLoaded(object sender, RoutedEventArgs args) {
        FrameworkElement element = (FrameworkElement) sender;
        PutView pv = (PutView) element.DataContext;
        pv.Control = element;
    }
}
}