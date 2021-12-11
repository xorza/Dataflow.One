using System.Windows;
using System.Windows.Controls;
using csso.NodeCore;

namespace csso.WpfNode;

public partial class FunctionFactoryBrowser : UserControl {
    public static readonly DependencyProperty FunctionFactoryProperty = DependencyProperty.Register(
        nameof(FunctionFactory), typeof(FunctionFactoryView), typeof(FunctionFactoryBrowser),
        new PropertyMetadata(default(FunctionFactoryView), FunctionFactoryView_PropertyChangedCallback));

    public FunctionFactoryView? FunctionFactory {
        get { return (FunctionFactoryView) GetValue(FunctionFactoryProperty); }
        set { SetValue(FunctionFactoryProperty, value); }
    }

    public static readonly DependencyProperty SelectedFunctionProperty = DependencyProperty.Register(
        nameof(SelectedFunction), typeof(Function), typeof(FunctionFactoryBrowser),
        new PropertyMetadata(default(Function), SelectedFunction_PropertyChangedCallback));

    public Function? SelectedFunction {
        get { return (Function) GetValue(SelectedFunctionProperty); }
        set { SetValue(SelectedFunctionProperty, value); }
    }


    public static readonly DependencyProperty NodePreviewProperty = DependencyProperty.Register(
        nameof(NodePreview), typeof(NodeView), typeof(FunctionFactoryBrowser),
        new PropertyMetadata(default(NodeView)
            , NodePreview_PropertyChangedCallback));


    public NodeView? NodePreview {
        get { return (NodeView) GetValue(NodePreviewProperty); }
        set { SetValue(NodePreviewProperty, value); }
    }

    public FunctionFactoryBrowser() {
        InitializeComponent();
    }

    private static void FunctionFactoryView_PropertyChangedCallback(DependencyObject d,
        DependencyPropertyChangedEventArgs e) { }

    private static void SelectedFunction_PropertyChangedCallback(DependencyObject d,
        DependencyPropertyChangedEventArgs e) {
        FunctionFactoryBrowser functionFactoryBrowser = (FunctionFactoryBrowser) d;
        if (functionFactoryBrowser.SelectedFunction == null) {
            functionFactoryBrowser.NodePreview = null;
            return;
        }
        

        GraphView graphView = new(new NodeCore.Graph() {
            FunctionFactory = functionFactoryBrowser.FunctionFactory!.FunctionFactory
        });

        NodeView nv = new NodeView(graphView,
            new NodeCore.Node(functionFactoryBrowser.SelectedFunction, graphView.Graph));
        functionFactoryBrowser.NodePreview = nv;
    }

    private static void
        NodePreview_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) { }
}