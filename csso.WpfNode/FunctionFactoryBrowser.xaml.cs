using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using csso.NodeCore;

namespace csso.WpfNode;

public partial class FunctionFactoryBrowser : UserControl {
    public static readonly DependencyProperty FunctionFactoryProperty = DependencyProperty.Register(
        nameof(FunctionFactory), typeof(FunctionFactoryView), typeof(FunctionFactoryBrowser),
        new PropertyMetadata(default(FunctionFactoryView), FunctionFactoryView_PropertyChangedCallback));

    public static readonly DependencyProperty SelectedFunctionProperty = DependencyProperty.Register(
        nameof(SelectedFunction), typeof(Function), typeof(FunctionFactoryBrowser),
        new PropertyMetadata(default(Function), SelectedFunction_PropertyChangedCallback));


    public static readonly DependencyProperty NodePreviewProperty = DependencyProperty.Register(
        nameof(NodePreview), typeof(NodeView), typeof(FunctionFactoryBrowser),
        new PropertyMetadata(default(NodeView)
            , NodePreview_PropertyChangedCallback));

    public static readonly DependencyProperty ListViewItemStyleProperty = DependencyProperty.Register(
        nameof(ListViewItemStyle), typeof(Style), typeof(FunctionFactoryBrowser), new PropertyMetadata(default(Style)));

    public FunctionFactoryBrowser() {
        InitializeComponent();

        Loaded += (sender, args) => SubscribeToMouseDoubleClicks();
        FunctionsListView.ItemContainerGenerator.ItemsChanged += ItemContainerGeneratorOnItemsChanged;
        FunctionsListView.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorOnStatusChanged;
    }

    public FunctionFactoryView? FunctionFactory {
        get => (FunctionFactoryView) GetValue(FunctionFactoryProperty);
        set => SetValue(FunctionFactoryProperty, value);
    }

    public Function? SelectedFunction {
        get => (Function) GetValue(SelectedFunctionProperty);
        set => SetValue(SelectedFunctionProperty, value);
    }

    public Style? ListViewItemStyle {
        get => (Style) GetValue(ListViewItemStyleProperty);
        set => SetValue(ListViewItemStyleProperty, value);
    }

    public NodeView? NodePreview {
        get => (NodeView) GetValue(NodePreviewProperty);
        set => SetValue(NodePreviewProperty, value);
    }

    public event EventHandler<Function>? FunctionChosen;

    private void ItemContainerGeneratorOnStatusChanged(object? sender, EventArgs e) {
        SubscribeToMouseDoubleClicks();
    }

    private void ItemContainerGeneratorOnItemsChanged(object sender, ItemsChangedEventArgs e) {
        SubscribeToMouseDoubleClicks();
    }

    private static void FunctionFactoryView_PropertyChangedCallback(DependencyObject d,
        DependencyPropertyChangedEventArgs e) {
        var functionFactoryBrowser = (FunctionFactoryBrowser) d;

        if (e.OldValue is FunctionFactoryView ffViewOld)
            ((INotifyCollectionChanged) ffViewOld.Functions).CollectionChanged -=
                functionFactoryBrowser.FunctionFactory_Functions_OnCollectionChanged;
        if (e.NewValue is FunctionFactoryView ffViewNew)
            ((INotifyCollectionChanged) ffViewNew.Functions).CollectionChanged +=
                functionFactoryBrowser.FunctionFactory_Functions_OnCollectionChanged;

        functionFactoryBrowser.SelectedFunction = null;
        functionFactoryBrowser.NodePreview = null;
    }

    private void FunctionFactory_Functions_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) { }

    private void SubscribeToMouseDoubleClicks() {
        for (var i = 0; i < FunctionsListView.Items.Count; i++)
            if (FunctionsListView.ItemContainerGenerator.ContainerFromIndex(i)
                is ListViewItem listViewItem) {
                listViewItem.MouseDoubleClick -= ListViewItemOnMouseDoubleClick;
                listViewItem.MouseDoubleClick += ListViewItemOnMouseDoubleClick;
            }
    }

    private void ListViewItemOnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
        var lvi = (ListViewItem) sender;
        var func = (Function) lvi.Content;
        FunctionChosen?.Invoke(this, func);
    }

    private static void SelectedFunction_PropertyChangedCallback(DependencyObject d,
        DependencyPropertyChangedEventArgs e) {
        var functionFactoryBrowser = (FunctionFactoryBrowser) d;
        if (functionFactoryBrowser.SelectedFunction == null) {
            functionFactoryBrowser.NodePreview = null;
            return;
        }

        GraphVM graphVm = new(new NodeCore.Graph {
            FunctionFactory = functionFactoryBrowser.FunctionFactory!.FunctionFactory
        });

        var node
            = graphVm.Graph.AddNode(functionFactoryBrowser.SelectedFunction);

        var nv = new NodeView(graphVm, node);
        functionFactoryBrowser.NodePreview = nv;
    }

    private static void
        NodePreview_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

    private void UIElement_OnPreviewMouse_Skip(object sender, MouseEventArgs e) {
        e.Handled = true;
    }
}