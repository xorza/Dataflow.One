using System.Windows;
using System.Windows.Controls;

namespace csso.WpfNode {
public partial class NodeEdit : UserControl {
    public NodeEdit() {
        InitializeComponent();
    }

    public static readonly DependencyProperty NodeViewProperty = DependencyProperty.Register(
        "NodeView", typeof(NodeView), typeof(NodeEdit), new PropertyMetadata(default(NodeView)));

    public NodeView? NodeView {
        get { return (NodeView) GetValue(NodeViewProperty); }
        set { SetValue(NodeViewProperty, value); }
    }
}
}