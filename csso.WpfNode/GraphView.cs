using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using csso.NodeCore;

namespace csso.WpfNode
{
    public class EdgeView
    {
        public PutView Input { get; set; }
        public PutView Output { get; set; }
        public Point P1 => Input.PinPoint;
        public Point P2 => Output.PinPoint;

        public EdgeView(PutView input, PutView output)
        {
            Input = input;
            Output = output;
        }
    }
    public class GraphView
    {
        public ObservableCollection<EdgeView> Edges { get; private set; }
            = new ObservableCollection<EdgeView> { };
        public ObservableCollection<NodeView> Nodes { get; private set; }
            = new ObservableCollection<NodeView> { };


        public GraphView(csso.NodeCore.Graph graph)
        {
            foreach (var node in graph.Nodes)
            {
                NodeView nv = new NodeView(node);
                Nodes.Add(nv);
            }

            foreach (var node in Nodes)
            {
                foreach (var edge in node.Node.Inputs)
                {
                    if (edge is OutputBinding binding)
                    {
                        var inputNode = GetNodeView(binding.InputNode);
                        var outputNode = GetNodeView(binding.OutputNode);

                        PutView input = inputNode.Inputs.Single(_ => _.SchemaPut == binding.Input);
                        PutView output = outputNode.Outputs.Single(_ => _.SchemaPut == binding.Output);

                        Edges.Add(new EdgeView(input, output));
                    }
                }
            }
        }

        private NodeView GetNodeView(csso.NodeCore.Node node)
        {
            return Nodes.Single(_ => _.Node == node);
        }
    }
}
