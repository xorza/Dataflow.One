﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using csso.Common;

namespace csso.WpfNode;

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
        nameof(DeletionEnabled), typeof(bool), typeof(Node), new PropertyMetadata(default(bool)));

    public bool DeletionEnabled {
        get => (bool) GetValue(DeletionEnabledProperty);
        set => SetValue(DeletionEnabledProperty, value);
    }

    private static void DragCanvas_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        Node graph = (Node) d;
    }

    public Node() {
        InitializeComponent();

        MouseLeftButtonDown += Node_MouseLeftButtonDown;
        LayoutUpdated += LayoutUpdated_EventHandler;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) { }

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

    public event PinClickEventHandler? PinClick;

    private void LayoutUpdated_EventHandler(object? sender, EventArgs e) { }

    private static void NodeView_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        Node node = (Node) d;
        if (e.OldValue is NodeView nv1)
            nv1.PropertyChanged -= node.OnPropertyChanged;
        if (e.OldValue is NodeView nv2)
            nv2.PropertyChanged += node.OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (double.IsFinite(NodeView!.ExecutionTime)) {
            ExecutionTimeTextBlock.Visibility = Visibility.Hidden;
        } else {
            ExecutionTimeTextBlock.Visibility = Visibility.Visible;
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
        Check.True(NodeView != null);

        if (NodeView!.GraphView.SelectedNode != NodeView) NodeView!.GraphView.SelectedNode = NodeView;
    }


    private void Close_Button_OnClick(object sender, RoutedEventArgs e) {
        NodeView!.GraphView.RemoveNode(NodeView);
    }

    private void PinButton_OnLoaded(object sender, RoutedEventArgs e) {
        Put put = (Put) sender;
        put.PinClick += PinButton_Click;
    }
}