namespace csso.NodeCore {
public class OutputNode : Node {
    public OutputNode(Schema schema, Graph graph) : base(schema, graph) {
        graph.AddOutput(this);
    }
}
}