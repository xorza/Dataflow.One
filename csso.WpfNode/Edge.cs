using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace csso.WpfNode {
public class EdgeEventArgs : RoutedEventArgs {
    public EdgeEventArgs(Edge edge) {
        Edge = edge;
    }

    public Edge Edge { get; }
}

public class Edge : ClickControl {
    public static readonly DependencyProperty InputPositionDependencyProperty = DependencyProperty.Register(
        "InputPosition", typeof(Point),
        typeof(Edge)
    );

    public static readonly DependencyProperty OutputPositionDependencyProperty = DependencyProperty.Register(
        "OutputPosition", typeof(Point),
        typeof(Edge)
    );

    public Edge() {
        LeftButtonClick += LeftButtonClickHandler;
    }

    public Point InputPosition {
        get => (Point) GetValue(InputPositionDependencyProperty);
        set {
            SetValue(InputPositionDependencyProperty, value);
            InvalidateVisual();
        }
    }

    public Point OutputPosition {
        get => (Point) GetValue(OutputPositionDependencyProperty);
        set {
            SetValue(OutputPositionDependencyProperty, value);
            InvalidateVisual();
        }
    }

    private void LeftButtonClickHandler(object? sender, MouseButtonEventArgs ea) {
        ;
    }

    protected override void OnRender(DrawingContext drawingContext) {
        base.OnRender(drawingContext);

        Pen pen;
        if (IsMouseOver)
            pen = new Pen(Brushes.Coral, 2);
        else
            pen = new Pen(Brushes.SlateGray, 1.5);

        Point[] points = {
            new(InputPosition.X - 5, InputPosition.Y),
            new(InputPosition.X - 50, InputPosition.Y),
            new(OutputPosition.X + 50, OutputPosition.Y),
            new(OutputPosition.X + 5, OutputPosition.Y)
        };

        var pathFigure = new PathFigure();
        pathFigure.StartPoint = points[0];
        pathFigure.Segments.Add(
            new BezierSegment(points[1], points[2], points[3], true));
        pathFigure.IsClosed = false;

        PathGeometry path = new();
        path.Figures.Add(pathFigure);

        drawingContext.DrawGeometry(null, new Pen(Brushes.Transparent, 5), path);
        drawingContext.DrawGeometry(null, pen, path);
    }
}
}