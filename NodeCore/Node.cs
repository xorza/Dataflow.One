namespace csso.NodeCore {
public class Node {
    public Node(Schema schema, Graph graph) {
        Schema = schema;
        Graph = graph;

        Graph.Add(this);

        Inputs = new Binding[Schema.Inputs.Count];
        for (var i = 0; i < Schema.Inputs.Count; i++) Inputs[i] = new EmptyBinding(this, Schema.Inputs[i]);

        Graph = graph;
    }

    public Schema Schema { get; }
    public Graph Graph { get; }
    public Binding[] Inputs { get; }
}
}