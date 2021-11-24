namespace csso.NodeCore {
public class Binding {
    public Binding(Node node, SchemaInput input) {
        Input = input;
        InputNode = node;
    }

    public SchemaInput Input { get; }
    public Node InputNode { get; }
}
}