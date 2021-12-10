using System.Windows;
using System.Windows.Controls;

namespace csso.WpfNode; 

public partial class NodeEdit : UserControl {
    public static readonly DependencyProperty NodeViewProperty = DependencyProperty.Register(
        "NodeView", typeof(NodeView), typeof(NodeEdit), new PropertyMetadata(default(NodeView)));

    public static readonly DependencyProperty NodeConfigTemplateSelectorProperty = DependencyProperty.Register(
        "NodeConfigTemplateSelector", typeof(NodeConfigTemplateSelector), typeof(NodeEdit),
        new PropertyMetadata(default(NodeConfigTemplateSelector)));

    public NodeEdit() {
        InitializeComponent();
    }

    public NodeView? NodeView {
        get => (NodeView) GetValue(NodeViewProperty);
        set => SetValue(NodeViewProperty, value);
    }

    public NodeConfigTemplateSelector NodeConfigTemplateSelector {
        get => (NodeConfigTemplateSelector) GetValue(NodeConfigTemplateSelectorProperty);
        set => SetValue(NodeConfigTemplateSelectorProperty, value);
    }
}