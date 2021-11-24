using System.Windows;
using csso.NodeCore;

namespace csso.WpfNode {
public class PutView {
    public PutView(SchemaPut schemaPut, NodeView nodeView) {
        SchemaPut = schemaPut;
        NodeView = nodeView;
    }

    public UIElement? Control { get; set; }
    public SchemaPut SchemaPut { get; }
    public Point PinPoint { get; set; }
    public NodeView NodeView { get; }
}
}