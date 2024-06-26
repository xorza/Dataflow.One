﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace csso.Nodeshop.UI;

public class EdgeEventArgs : RoutedEventArgs {
    public EdgeEventArgs(Edge edge) {
        Edge = edge;
    }

    public Edge Edge { get; }
}

public class Edge : ClickControl {
    public static readonly DependencyProperty InputPositionDependencyProperty = DependencyProperty.Register(
        nameof(InputPosition),
        typeof(Point),
        typeof(Edge),
        new PropertyMetadata(Position_PropertyChangedCallback)
    );


    public static readonly DependencyProperty OutputPositionDependencyProperty = DependencyProperty.Register(
        nameof(OutputPosition),
        typeof(Point),
        typeof(Edge),
        new PropertyMetadata(Position_PropertyChangedCallback)
    );

    public static readonly DependencyProperty IsProactiveProperty = DependencyProperty.Register(
        nameof(IsProactive), typeof(bool), typeof(Edge), new PropertyMetadata(default(bool)));

    public Edge() {
        LeftButtonClick += LeftButtonClickHandler;
        MouseDoubleClick += MouseButtonEventHandler;


        Resources.Source = new Uri("/Styles/Main.xaml", UriKind.Relative);
    }

    public bool IsProactive {
        get => (bool)GetValue(IsProactiveProperty);
        set => SetValue(IsProactiveProperty, value);
    }

    public Point InputPosition {
        get => (Point)GetValue(InputPositionDependencyProperty);
        set => SetValue(InputPositionDependencyProperty, value);
    }

    public Point OutputPosition {
        get => (Point)GetValue(OutputPositionDependencyProperty);
        set => SetValue(OutputPositionDependencyProperty, value);
    }

    private static void Position_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        ((UIElement)d).InvalidateVisual();
    }

    private void LeftButtonClickHandler(object? sender, MouseButtonEventArgs ea) {
        ;
    }

    private void MouseButtonEventHandler(object sender, MouseButtonEventArgs e) {
        var view = (EdgeView)((FrameworkElement)sender).DataContext;
        view.IsProactive = !view.IsProactive;
    }

    protected override void OnRender(DrawingContext drawingContext) {
        base.OnRender(drawingContext);

        Pen pen;
        if (IsMouseOver) {
            pen = new Pen(Brushes.Coral, 2);
        } else if (IsProactive) {
            pen = new Pen(Resources["HighlightBackgroundBrush"] as Brush, 1.5);
        } else {
            pen = new Pen(Brushes.SlateGray, 1.5);
        }

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