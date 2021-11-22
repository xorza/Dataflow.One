using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csso.WpfNode
{
    public class EdgeView
    {
        public Point P1 { get; set; }
        public Point P2 { get; set; }
    }
    public class GraphView
    {
        public List<EdgeView> Edges { get; private set; } = new List<EdgeView> { };
        public List<NodeView> Nodes { get; private set; } = new List<NodeView> { };


        public GraphView(csso.NodeCore.Graph graph)
        {
            foreach (var node in graph.Nodes)
            {
                Nodes.Add(new NodeView(node));

                //foreach (var edge in node.)
                //{

                //}
            }
        }
    }
}
