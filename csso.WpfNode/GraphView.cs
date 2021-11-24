using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using csso.NodeCore;

namespace csso.WpfNode {
public class EdgeView {
    public EdgeView(PutView input, PutView output) {
        Input = input;
        Output = output;
    }

    public PutView Input { get; set; }
    public PutView Output { get; set; }
    public Point P1 => Input.PinPoint;
    public Point P2 => Output.PinPoint;
}

public class GraphView {
    public GraphView(NodeCore.Graph graph) {
        foreach (var node in graph.Nodes) {
            NodeView nv = new(node);
            Nodes.Add(nv);
        }

        foreach (var node in Nodes)
        foreach (var edge in node.Node.Inputs)
            if (edge is OutputBinding binding) {
                var inputNode = GetNodeView(binding.InputNode);
                var outputNode = GetNodeView(binding.OutputNode);

                PutView input = inputNode.Inputs.Single(_ => _.SchemaPut == binding.Input);
                PutView output = outputNode.Outputs.Single(_ => _.SchemaPut == binding.Output);

                Edges.Add(new EdgeView(input, output));
            }
    }

    public ObservableCollection<EdgeView> Edges { get; }
        = new();

    public ObservableCollection<NodeView> Nodes { get; }
        = new();

    private NodeView GetNodeView(NodeCore.Node node) {
        return Nodes.Single(_ => _.Node == node);
    }
}
}