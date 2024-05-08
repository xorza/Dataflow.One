using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace csso.Nodeshop.UI;

public partial class NodeEdit : UserControl {
    public static readonly DependencyProperty NodeViewProperty = DependencyProperty.Register
    (
        "NodeView", typeof(NodeView),
        typeof(NodeEdit),
        new PropertyMetadata(default(NodeView), PropertyChangedCallback)
    );


    public NodeEdit() {
        InitializeComponent();

        Refresh();
    }

    public NodeView? NodeView {
        get => (NodeView)GetValue(NodeViewProperty);
        set => SetValue(NodeViewProperty, value);
    }

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        ((NodeEdit)d).Refresh();
    }

    private void Refresh() {
        MainPanel.Visibility =
            NodeView == null
                ? Visibility.Collapsed
                : Visibility.Visible;

        if (NodeView == null) {
            return;
        }


        var hasValues = NodeView.InputValues.Any() || NodeView.OutputValues.Any();

        DebugValuesExpander.Visibility = hasValues ? Visibility.Visible : Visibility.Collapsed;
    }
}