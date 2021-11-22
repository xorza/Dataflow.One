using csso.NodeCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace csso.WpfNode
{
    public class PutView
    {
        public UIElement? Control { get; set; }
        public SchemaPut SchemaPut { get; private set; }
        public Point PinPoint { get; set; }
        public NodeView NodeView { get; private set; }

        public PutView(SchemaPut schemaPut, NodeView nodeView)
        {
            SchemaPut = schemaPut;
            NodeView = nodeView;
        }
    }
}