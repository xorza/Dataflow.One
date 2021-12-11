using System;
using System.Windows;
using System.Windows.Controls;

namespace csso.WpfNode;

public partial class Put : UserControl {
    public static readonly DependencyProperty PutViewProperty = DependencyProperty.Register(
        nameof(PutView), typeof(PutView), typeof(Put), new PropertyMetadata(default(PutView?)));

    public PutView? PutView {
        get { return (PutView?) GetValue(PutViewProperty); }
        set { SetValue(PutViewProperty, value); }
    }

    public static readonly DependencyProperty DragCanvasProperty = DependencyProperty.Register(
        nameof(DragCanvas), typeof(Canvas), typeof(Put),
        new PropertyMetadata(default(Canvas), DragCanvas_PropertyChangedCallback));

    private static void DragCanvas_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        Put put = (Put) d;
        put.UpdatePinPoint();
    }

    public Canvas? DragCanvas {
        get { return (Canvas) GetValue(DragCanvasProperty); }
        set { SetValue(DragCanvasProperty, value); }
    }


    public Put() {
        InitializeComponent();

        Loaded += OnLoaded;
        LayoutUpdated += (object? sender, EventArgs e) => { UpdatePinPoint(); };
    }

    private void UpdatePinPoint() {
        if (PutView!.Control == null)
            return;
        
        if(!IsVisible)
            return;
        
        if(DragCanvas==null)
            return;

        var upperLeft = PutView.Control
            .TransformToVisual(DragCanvas)
            .Transform(new Point(0, 0));
        var mid = new Point(
            PutView.Control.RenderSize.Width / 2,
            PutView.Control.RenderSize.Height / 2);

        var newPinPoint = new Point(upperLeft.X + mid.X, upperLeft.Y + mid.Y);
        PutView.PinPoint = newPinPoint;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) { }

    private void PinHighlight_LoadedHandler(object sender, RoutedEventArgs args) {
        var element = (FrameworkElement) sender;
        PutView!.Control = element;
        
        UpdatePinPoint();
    }

    private void PinButton_Click(object sender, RoutedEventArgs e) {
        var pv = (PutView) ((Button) sender).Tag;
        PinClick?.Invoke(this,
            new PinClickEventArgs(pv) {
                RoutedEvent = e.RoutedEvent,
                Source = e.Source,
                Handled = false
            });
    }


    public event PinClickEventHandler? PinClick;
}