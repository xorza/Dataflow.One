using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace dfo.NodeRunner.UI;

public class DragBehavior {
    public static readonly DependencyProperty IsDragProperty =
        DependencyProperty.RegisterAttached(
            "Drag",
            typeof(bool),
            typeof(DragBehavior),
            new PropertyMetadata(false, OnChanged));

    public readonly TranslateTransform Transform = new();
    private Point _elementStartPosition2;
    private Point _mouseStartPosition2;

    public static bool GetDrag(DependencyObject obj) {
        return (bool) obj.GetValue(IsDragProperty);
    }

    public static void SetDrag(DependencyObject obj, bool value) {
        obj.SetValue(IsDragProperty, value);
    }

    private static void OnChanged(object sender, DependencyPropertyChangedEventArgs e) {
        var element = (UIElement) sender;
        var isDrag = (bool) e.NewValue;

        DragBehavior dragBehavior = new();
        element.RenderTransform = dragBehavior.Transform;

        if (isDrag) {
            element.MouseLeftButtonDown += dragBehavior.ElementOnMouseLeftButtonDown;
            element.MouseLeftButtonUp += dragBehavior.ElementOnMouseLeftButtonUp;
            element.MouseMove += dragBehavior.ElementOnMouseMove;
        } else {
            element.MouseLeftButtonDown -= dragBehavior.ElementOnMouseLeftButtonDown;
            element.MouseLeftButtonUp -= dragBehavior.ElementOnMouseLeftButtonUp;
            element.MouseMove -= dragBehavior.ElementOnMouseMove;
        }
    }

    private void ElementOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
        var element = (UIElement) sender;
        _mouseStartPosition2 = mouseButtonEventArgs.GetPosition(element);
        element.CaptureMouse();
    }

    private void ElementOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs) {
        var element = (UIElement) sender;
        ((UIElement) sender).ReleaseMouseCapture();
        _elementStartPosition2.X = Transform.X;
        _elementStartPosition2.Y = Transform.Y;
    }

    private void ElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs) {
        var element = (UIElement) sender;
        var mousePos = mouseEventArgs.GetPosition(element);
        var diff = mousePos - _mouseStartPosition2;
        if (!((UIElement) sender).IsMouseCaptured) {
            return;
        }

        Transform.X = _elementStartPosition2.X + diff.X;
        Transform.Y = _elementStartPosition2.Y + diff.Y;
    }
}