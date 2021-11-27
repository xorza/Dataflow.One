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

    private NodeView? _nodeView;

    public Node() {
        InitializeComponent();

        MouseLeftButtonDown += Node_MouseLeftButtonDown;
        ;
    }


    public NodeView? NodeView {
        get => _nodeView;
        set {
            if (_nodeView != value) {
                _nodeView = value;
                Panel.DataContext = _nodeView;
                HeaderLabel.Content = _nodeView?.Name;
                Refresh();
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

    private void Refresh() {
        InputsStackPanel.Children.Clear();
        OutputsStackPanel.Children.Clear();
        InvalidateVisual();

        if (_nodeView == null) return;

        foreach (var pv in _nodeView.Inputs) {
            StackPanel stackPanel = new();
            stackPanel.Orientation = Orientation.Horizontal;
            Button button = new();
            pv.Control = button;
            button.Click += PinButton_Click;
            button.Tag = pv;
            stackPanel.Children.Add(button);
            stackPanel.Children.Add(new Label {Content = pv.SchemaPut.Name});
            InputsStackPanel.Children.Add(stackPanel);
        }

        foreach (var pv in _nodeView.Outputs) {
            StackPanel stackPanel = new();
            stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Children.Add(new Label {Content = pv.SchemaPut.Name});
            Button button = new();
            pv.Control = button;
            button.Click += PinButton_Click;
            button.Tag = pv;
            stackPanel.Children.Add(button);
            OutputsStackPanel.Children.Add(stackPanel);
        }
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

    private void Node_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        Check.True(_nodeView != null);

        _nodeView!.GraphView.SelectedNode = _nodeView;
    }
}
}