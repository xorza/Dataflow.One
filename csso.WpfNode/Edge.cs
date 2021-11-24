using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ReactiveUI;

namespace csso.WpfNode {
public class Edge : Control {
    public static readonly DependencyProperty InputPositionDependencyProperty = DependencyProperty.Register(
        "InputPosition", typeof(Point),
        typeof(Edge)
    );

    public Point InputPosition {
        get => (Point) GetValue(InputPositionDependencyProperty);
        set {
            SetValue(InputPositionDependencyProperty, value);
            InvalidateVisual();
        }
    }

    public static readonly DependencyProperty OutputPositionDependencyProperty = DependencyProperty.Register(
        "OutputPosition", typeof(Point),
        typeof(Edge)
    );

    public Point OutputPosition {
        get => (Point) GetValue(OutputPositionDependencyProperty);
        set {
            SetValue(OutputPositionDependencyProperty, value);
            InvalidateVisual();
        }
    }

    public Edge() { }

    protected override void OnRender(DrawingContext drawingContext) {
        base.OnRender(drawingContext);

        Pen p = new Pen(Brushes.Coral, 2);
        drawingContext.DrawLine(p, InputPosition, OutputPosition);
    }
}
}