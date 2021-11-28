namespace csso.NodeCore {
public abstract class Connection {
    protected Connection(Node node, FunctionInput input) {
        Input = input;
        InputNode = node;
    }

    public FunctionInput Input { get; }
    public Node InputNode { get; }
}
}