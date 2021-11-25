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

        Pen pen = new(Brushes.Gray, 2);

        var pathFigure = new PathFigure();

        Point[] points = {
            InputPosition,
            new Point(InputPosition.X - 50, InputPosition.Y),
            new Point(OutputPosition.X + 50, OutputPosition.Y),
            OutputPosition,
        };

        pathFigure.StartPoint = points[0];

        pathFigure.Segments.Add(
            new BezierSegment(points[1], points[2], points[3], true));
        pathFigure.IsClosed = false;

        PathGeometry path = new();
        path.Figures.Add(pathFigure);

        drawingContext.DrawGeometry(null, pen, path);
    }
}
}